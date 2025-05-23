﻿using System;

namespace InlineCollection.Sample;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Hello, World!");
        var array = new TestInlineCollection<string>("A", "b", "c", "D");
        array[3] = "d";
        array.AsSpan()[0] = "a";

        (1, 2, 3).AsSpan();

        foreach (var value in array) Console.WriteLine(value);

        array = ("e", "f", "g", "h");

        foreach (var value in array) Console.WriteLine(value);

        foreach (var foo in (1, 2, 3, 4, 5, 6).AsSpan()) Console.WriteLine(foo);

        foreach (var text in ("hoge", "foo", "bar").AsSpan()) Console.WriteLine(text);


        (1, 2).AsReadOnlySpan();
    }
}

[InlineCollection(4)]
public partial struct TestInlineCollection<T>
{
}

[InlineCollection(typeof(string), 4)]
public partial struct TestStringInlineCollection
{
}
