namespace Cloning.Tests;

using System;
using System.Collections.Generic;
using System.Text;

public class Parcel
{
    public Parcel()
    {
    }

    public Parcel(string name)
    {
        Name = name;
    }

    public Guid Id { get; set; }

    public string Name { get; }
    public int Weight { get; set; }

    public List<Parcel> Children { get; } = [];
}
