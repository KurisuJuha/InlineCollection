# InlineCollection

English | [日本語](./README_JA.md)

## Overview

InlineCollection is a library that provides zero-allocation stack-based collections for Unity.

Since Unity uses a relatively old C# version (C# 10), it cannot utilize features like `InlineArray` or collection expressions that are available in C# 12 and later.

While `stackalloc` can be used in Unity, it can only create Spans of unmanaged types and cannot allocate arrays of managed types on the stack.

This library adds:

-   An `InlineCollection` feature equivalent to the `InlineArray` available in C# 12 and later
-   A convenient functionality to convert `ValueTuple` to `Span`

## Installation

## Usage

### Tuple To Span

You can convert a `ValueTuple` to a `Span`:

```csharp
var span = (1, 2, 3).AsSpan();

foreach (var i in span)
{
    Debug.Log(i);
}
```

Note that all elements in the tuple must be of the same type.

### InlineCollection

You can create array-like structures by adding the `InlineCollection` attribute to a `partial struct`:

```csharp
[InlineCollection(length: 3)]
public partial struct SampleInlineCollection<T> {}
```

Usage example:

```csharp
using InlineCollection;

var sample = new SampleInlineCollection<string>("a", "b", "C");

sample[0] = "a";

foreach (var s in sample)
{
    Debug.Log(s);
}
```

You can also convert it to a `Span`:

```csharp
Span<string> span = sample.AsSpan();
```

Additionally, you can specify a specific element type by adding type information as an argument to the `InlineCollection` attribute:

```csharp
[InlineCollection(elementType: typeof(int), length: 3)]
public partial struct SampleInlineCollection {}
```

Example:

```csharp
var sample = new SampleInlineCollection(0, 1, 2);
```
