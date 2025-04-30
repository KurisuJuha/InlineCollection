using System;

namespace InlineCollection.Sample;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var array = new TestInlineCollection<string>("a", "b", "c", "e");
        array[3] = "d";
        foreach (var value in array) Console.WriteLine(value);

        foreach (var foo in (1, 2, 3, 4, 5, 6).AsSpan()) Console.WriteLine(foo);

        foreach (var text in ("hoge", "foo", "bar").AsSpan()) Console.WriteLine(text);
    }
}

[InlineCollection(4)]
public partial struct TestInlineCollection<T>
{
}
