using System.Linq;
using Microsoft.CodeAnalysis;

namespace InlineCollection.Generator;

[Generator]
public class MainGenerator : IIncrementalGenerator
{
    private const string Namespace = "InlineCollection";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(c =>
        {
            c.AddSource(
                "Collection.InlineCollectionAttributes.g.cs",
                $$$"""
                   namespace {{{Namespace}}}
                   {
                       [global::System.AttributeUsage(global::System.AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
                       public class InlineCollectionAttribute : global::System.Attribute
                       {
                           public global::System.Type ElementType { get; }
                           public uint Length { get; }
                   
                           public InlineCollectionAttribute(uint length)
                           {
                               Length = length;
                           }
                           
                           public InlineCollectionAttribute(global::System.Type elementType, uint length)
                           {
                               ElementType = elementType;
                               Length = length;
                           }
                       }
                   }
                   """);
            c.AddSource(
                "Collection.TupleToSpan.g.cs",
                $$$"""
                   namespace {{{Namespace}}}
                   {
                       public static class TupleToSpan
                       {
                   {{{Enumerable.Range(2, 100).Select(GetTupleToSpanCode).Join("\n")}}}
                   {{{Enumerable.Range(2, 100).Select(GetTupleToReadOnlySpanCode).Join("\n")}}}
                       }
                   }
                   """
            );
        });

        var provider = context.SyntaxProvider.ForAttributeWithMetadataName(
            "InlineCollection.InlineCollectionAttribute",
            static (node, ct) => true,
            static (context, ct) => context
        );

        context.RegisterSourceOutput(provider, Emit);
    }

    private static void Emit(SourceProductionContext context, GeneratorAttributeSyntaxContext syntaxContext)
    {
        var targetTypeNamespace = syntaxContext.TargetSymbol.ContainingNamespace;
        var typeSymbol = (INamedTypeSymbol)syntaxContext.TargetSymbol;
        var targetType = typeSymbol.Name;
        var attribute = syntaxContext.Attributes.First(attributeData =>
            attributeData.AttributeClass?.ToDisplayString() == "InlineCollection.InlineCollectionAttribute"
        );
        int length;
        ITypeSymbol type;

        var isSpecialized = attribute.ConstructorArguments.Length == 2;

        if (isSpecialized)
        {
            type = (ITypeSymbol)attribute.ConstructorArguments[0].Value!;
            length = (int)(uint)(attribute.ConstructorArguments[1].Value ?? 0);
        }
        else
        {
            type = typeSymbol.TypeArguments[0];
            length = (int)(uint)(attribute.ConstructorArguments[0].Value ?? 0);
        }

        var typeText = type.ToDisplayString();

        var typeArgumentsCode = typeSymbol.TypeArguments.Length == 0
            ? ""
            : $"<{typeSymbol.TypeArguments.Select(t => t.ToDisplayString()).Join(", ")}>";

        var selfTypeName = $"{targetType}{typeArgumentsCode}";

        var fileName =
            $"Collection{(targetTypeNamespace.IsGlobalNamespace ? "" : $".{targetTypeNamespace}")}.{targetType}.g.cs";

        var typeCode = $$$"""
                          partial struct {{{selfTypeName}}}
                          {
                          {{{GetPropertiesCode(length, typeText)}}}

                          {{{GetConstructorCode(targetType, typeText, length)}}}

                          {{{GetIndexerCode(typeText)}}}

                          {{{GetAsSpanCode(typeText, length)}}}

                          {{{GetToArrayCode(typeText)}}}

                          {{{GetAsReadOnlySpanCode(typeText, length)}}}

                          {{{GetEnumerableCode(typeText)}}}

                          {{{GetAsEnumerableCode(typeText, length)}}}

                          {{{GetTupleImplicitConversionCode(selfTypeName, typeText, length)}}}
                          }
                          """;

        context.AddSource(
            fileName,
            targetTypeNamespace.IsGlobalNamespace
                ? typeCode
                : $$$"""
                     namespace {{{targetTypeNamespace}}}
                     {
                     {{{typeCode.Indent()}}}
                     }
                     """
        );
    }

    private static string GetPropertiesCode(int length, string type)
    {
        return string.Join(
            "\n",
            Enumerable
                .Range(0, length)
                .Select(i => $"        public {type} Item{i};")
        );
    }

    private static string GetAsSpanCode(string type, int length)
    {
        return $$$"""
                  [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                  public global::System.Span<{{{type}}}> AsSpan() 
                      => global::System.Runtime.InteropServices.MemoryMarshal.CreateSpan(ref Item0, {{{length}}});
                  """.Indent(2);
    }

    private static string GetToArrayCode(string type)
    {
        return $$$"""
                  public {{{type}}}[] ToArray() => AsSpan().ToArray();
                  """.Indent(2);
    }

    private static string GetAsReadOnlySpanCode(string type, int length)
    {
        return $$$"""
                  [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                  public global::System.ReadOnlySpan<{{{type}}}> AsReadOnlySpan() 
                      => global::System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref Item0, {{{length}}});
                  """.Indent(2);
    }

    private static string GetEnumerableCode(string type)
    {
        return $$$"""
                  public global::System.Span<{{{type}}}>.Enumerator GetEnumerator()
                      => this.AsSpan().GetEnumerator();
                  """.Indent(2);
    }

    private static string GetAsEnumerableCode(string type, int length)
    {
        return $$$"""
                  public global::System.Collections.Generic.IEnumerable<{{{type}}}> AsEnumerable()
                  {
                  {{{
                      Enumerable.Range(0, length).Select(i => $"yield return Item{i};").Join("\n").Indent()
                  }}}
                  }
                  """.Indent(2);
    }

    private static string GetIndexerCode(string type)
    {
        return $$$"""
                  public {{{type}}} this[int index]
                  {
                      [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                      get => AsSpan()[index];
                      [global::System.Runtime.CompilerServices.MethodImpl(global::System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                      set => AsSpan()[index] = value;
                  }
                  """.Indent(2);
    }

    private static string GetConstructorCode(string type, string elementType, int length)
    {
        return $$$"""
                  public {{{type}}}({{{Enumerable.Range(0, length).Select(i => $"{elementType} item{i}").Join(", ")}}})
                  {
                  {{{Enumerable.Range(0, length).Select(i => $"Item{i} = item{i};").Join("\n").Indent()}}}
                  }
                  """.Indent(2);
    }

    private static string GetTupleImplicitConversionCode(string selfType, string elementType, int length)
    {
        return $$$"""
                  public static implicit operator {{{selfType}}}(({{{Enumerable.Range(0, length).Select(i => elementType).Join(", ")}}}) tuple)
                  {
                      return new {{{selfType}}}({{{Enumerable.Range(0, length).Select(i => $"tuple.Item{i + 1}").Join(", ")}}});
                  }
                  """.Indent(2);
    }

    private static string GetTupleToSpanCode(int count)
    {
        return $$$"""
                  public static global::System.Span<T> AsSpan<T>(this ({{{Enumerable.Repeat("T", count).Join(", ")}}}) tuple)
                      => global::System.Runtime.InteropServices.MemoryMarshal.CreateSpan(ref tuple.Item1, {{{count}}});
                  """.Indent(2);
    }

    private static string GetTupleToReadOnlySpanCode(int count)
    {
        return $$$"""
                  public static global::System.ReadOnlySpan<T> AsReadOnlySpan<T>(this ({{{Enumerable.Repeat("T", count).Join(", ")}}}) tuple)
                      => global::System.Runtime.InteropServices.MemoryMarshal.CreateReadOnlySpan(ref tuple.Item1, {{{count}}});
                  """.Indent(2);
    }
}
