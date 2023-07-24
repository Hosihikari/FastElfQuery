namespace Hosihikari.FastElfQuery;

public class ElfSymbolNotFoundException : Exception
{
    public string SymbolName { get; }

    internal ElfSymbolNotFoundException(string name)
        : base($"Symbol {name} not found")
    {
        SymbolName = name;
    }
}
