namespace Cloning.Tests;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public class Parcel
{
    public Parcel() : this(string.Empty)
    {
    }

    public Parcel(string name)
    {
        this.Name = name;
    }

    public BoringCustomType Boring { get; set; } = new();
    internal PrimitveDetails PrimitveDetails { get; set; } = new();

    public byte[]FakeImageData { get; set; } = Array.Empty<byte>();

    public NoDefCtor? NoDefCtorProp { get; set; }

    internal HashSet<int> HashSetProp { get; private set; } = [];
    internal Stack<string> StackProp { get; private set; } = new();
    internal ConcurrentStack<string> ConcStackProp { get; private set; } = new();
    public LinkedList<decimal> LinkedListProp { get; } = new();
    public Tuple<int, string, decimal, object?> TupleProp { get; set; } = new(0, string.Empty, 0.0m, null);
    public ValueTuple<int, string, decimal, object?> ValueTupleProp { get; set; } = new(0, string.Empty, 0.0m, null);

    public Guid Id { get; set; }

    public string Name { get; }
    public int Weight { get; set; }
    internal decimal Value { get; set; }

    public List<Parcel> Children { get; } = [];
    public Dictionary<string, object> DictProp { get; } = [];

    public ConcurrentBag<string> ConcBagProp { get; } = [];

    public ConcurrentDictionary<int, string> ConcDictProp { get; } = new();
}
