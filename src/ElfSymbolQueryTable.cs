using System.Runtime.CompilerServices;
using ELFSharp.ELF;
using ELFSharp.ELF.Sections;

namespace Hosihikari.FastElfQuery;

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
            switch (name)
            {
                case var _ when name.StartsWith("GCC_except_table"):
                case var _ when name.StartsWith("__cxx_global_var_init"):
                case var _ when name.StartsWith("___stack_chk_fail"):
                case var _ when name.StartsWith("_GLOBAL__sub_I_unity_bucket_"):
                case "_ZStL8__ioinit":
                case "__cxx_global_array_dtor":
                case "init":
                case "final":
                case "update":
                case "file_ctrl":
                case "__tls_guard":
                case "__tls_init":
                case "_ZL32TextProcessingEventOriginEnumMapB5cxx11":
                    continue;
            }
            //cast ulong to int to optimize memory usage
            Add(name, (int)offset);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Add(string name, int offset)
    {
        //convert to hashcode to avoid string comparison
        //for better performance
#if UnknownSymbol
        if (!
#endif
            _table.TryAdd(name.GetHashCode(), offset)
#if UnknownSymbol
        )
        {
            Console.WriteLine("duplicated symbol {0}, offset {1}", name, offset);
        }
#else
        ;
#endif
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
