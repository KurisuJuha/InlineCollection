using System.Collections.Generic;

namespace InlineCollection.Generator;

public static class Common
{
    public static string Indent(this string self, int count = 1)
    {
        var indent = new string(' ', count * 4);

        return $"{indent}{self.Replace("\n", $"\n{indent}")}";
    }

    public static string Join(this IEnumerable<string> self, string separator = "")
    {
        return string.Join(separator, self);
    }
}