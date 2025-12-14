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

    internal PrimitveDetails PrimitveDetails { get; set; } = new();

    public NoDefCtor? NoDefCtorProp { get; set; }

    public Guid Id { get; set; }

    public string Name { get; }
    public int Weight { get; set; }
    internal decimal Value { get; set; }

    public List<Parcel> Children { get; } = [];
    public Dictionary<string, object> Metadata { get; } = [];

    public ConcurrentBag<string> Notes { get; } = new();
}
