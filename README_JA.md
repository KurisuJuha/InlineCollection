# InlineCollection

## 概要

InlineCollection は Unity 向けにゼロアロケーションのスタック内コレクションを提供するライブラリです。

Unity の C#バージョンは 10 と低く、C#12 以降で使える`InlineArray`やコレクション式を利用することができません。

また、Unity でも stackalloc は使用できますが unmanaged な型の Span を作ることしかできず、managed な型の配列をスタック上に確保することはできません。

このライブラリでは、以下の機能を提供します

-   C#12 以降で使える`InlineArray`相当の`InlineCollection`の実装
-   より気軽に使える`ValueTuple`から`Span`への変換機能

## インストール

## 使い方

### Tuple To Span

`ValueTuple`を`Span`に変換することが可能です。

```csharp
var span = (1, 2, 3).AsSpan();

foreach (var i in span)
{
    Debug.Log(i);
}
```

この際、タプル内のすべての要素の型が共通である必要があります。

### InlineCollection

`partial struct`に`InlineCollection`アトリビュートを追加することで配列のように扱うことができます。

```csharp
[InlineCollection(length: 3)]
public partial struct SampleInlineCollection<T> {}
```

```csharp
using InlineCollection;

var sample = new SampleInlineCollection<string>("a", "b", "C");

sample[0] = "a";

foreach (var s in sample)
{
    Debug.Log(s);
}
```

`Span`へ変換することも可能です

```csharp
Span<string> span = sample.AsSpan();
```

また、以下のように`InlineCollection`アトリビュートの引数に型情報を追加することで特定の型のみを要素とすることも可能です。

```csharp
[InlineCollection(elementType: typeof(int), length: 3)]
public partial struct SampleInlineCollection {}
```

```csharp
var sample = new SampleInlineCollection(0, 1, 2);
```
