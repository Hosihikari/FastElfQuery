using System.Runtime.CompilerServices;

namespace FastElfQuery;

using ELFSharp.ELF;
using ELFSharp.ELF.Sections;

public class ElfSymbolQueryTable
{
    #region ---Constructors---
    public ElfSymbolQueryTable(string path)
    {
        LoadFromElf(path);
    }
    #endregion
    #region ---Private Methods---

    private static IEnumerable<(string name, ulong offset)> EnumerableSymbolsFromElf(IELF elf)
    {
        var symbolTable = (ISymbolTable)elf.GetSection(".symtab");
        var symbolEntries = symbolTable.Entries.Cast<SymbolEntry<ulong>>();
        foreach (var symbolEntry in symbolEntries)
        {
            var name = symbolEntry.Name;
            if (!string.IsNullOrWhiteSpace(name))
            {
                yield return (name, symbolEntry.Value);
            }
        }
    }

    private void LoadFromElf(string path)
    {
        _table.Clear();
        using var elf = ELFReader.Load(path);
        foreach (var (name, offset) in EnumerableSymbolsFromElf(elf))
        {
            //cast ulong to int to optimize memory usage
            Add(name, (int)offset);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Add(string name, int offset)
    {
        //convert to hashcode to avoid string comparison
        //for better performance
        _table.Add(name.GetHashCode(), offset);
    }
    #endregion
    #region ---Private Fields---
    private readonly SortedDictionary<int, int> _table = new();
    #endregion
    #region ---Public Methods---
    public int Query(string symbolName)
    {
        if (_table.TryGetValue(symbolName.GetHashCode(), out var offset))
        {
            return offset;
        }
        throw new ElfSymbolNotFoundException(symbolName);
    }

    public bool TryQuery(string symbolName, out int offset)
    {
        return _table.TryGetValue(symbolName.GetHashCode(), out offset);
    }
    #endregion
}
