namespace Cloning;

internal sealed class SubResult
{
    public SubResult(bool isCloned, object? clone)
    {
        this.IsCloned = isCloned;
        this.Clone = clone;
    }

    internal bool IsCloned { get; set; }

    internal object? Clone { get; set; }
}
