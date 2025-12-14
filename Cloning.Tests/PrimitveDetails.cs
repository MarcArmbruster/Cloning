namespace Cloning.Tests;

public enum Supported
{
    Unknown,
    Yes,
    No
}

internal class PrimitveDetails
{
    internal bool BoolValue { get; init; } = true;
    internal byte ByteValue { get; set; }
    internal sbyte SByteValue { get; set; }
    internal short ShortValue { get; set; }
    internal ushort UShortValue { get; set; }
    internal char CharValue { get; set; }
    internal decimal DecimalValue { get; set; }
    internal double DoubleValue { get; set; }
    internal float FloatValue { get; set; }
    internal int IntValue { get; set; }
    internal uint UIntValue { get; set; }
    internal long LongValue { get; set; }
    internal ulong ULongValue { get; set; }
    internal string StringValue { get; set; } = string.Empty;
    internal DateTime DateTimeValue { get; set; }
    internal DateTimeOffset DateTimeOffsetValue { get; set; }
    internal Guid GuidValue { get; set; }
    internal TimeSpan TimeSpanValue { get; set; }
    internal Supported Supported { get; set; }
}
