namespace Cloning.Tests;

public class NoDefCtor(int count, string text)
{
    public int Count { get; } = count;
    public string Text { get; } = text;
    public override string ToString() => $"Count: {Count}, Text: {Text}";
}
