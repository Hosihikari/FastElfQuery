namespace Hosihikari.FastElfQuery;

public sealed class ElfSymbolNotFoundException : Exception
{
    internal ElfSymbolNotFoundException(string name)
        : base($"Symbol {name} not found")
    {
        SymbolName = name;
    }

    public string SymbolName { get; }
}