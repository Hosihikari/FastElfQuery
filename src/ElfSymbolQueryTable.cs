using ELFSharp.ELF;
using ELFSharp.ELF.Sections;
using System.Runtime.CompilerServices;

namespace Hosihikari.FastElfQuery;

public sealed class ElfSymbolQueryTable
{
    #region ---Private Fields---

    private readonly SortedDictionary<int, nint> _table = new();

    #endregion

    #region ---Constructors---

    public ElfSymbolQueryTable(string path)
    {
        LoadFromElf(path);
    }

    #endregion

    #region ---Private Methods---

    private static IEnumerable<(string name, nint offset)> EnumerableSymbolsFromElf(IELF elf)
    {
        ISymbolTable? symbolTable = elf.GetSection(".symtab") as ISymbolTable;
        IEnumerable<SymbolEntry<nint>> symbolEntries = symbolTable!.Entries.Cast<SymbolEntry<nint>>();
        foreach (SymbolEntry<nint> symbolEntry in symbolEntries)
        {
            string name = symbolEntry.Name;
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            yield return (name, symbolEntry.Value);
        }
    }

    private void LoadFromElf(string path)
    {
        _table.Clear();
        using IELF elf = ELFReader.Load(path);
        foreach ((string name, nint offset) in EnumerableSymbolsFromElf(elf))
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

            //cast nint to int to optimize memory usage
            Add(name, offset);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Add(string name, nint offset)
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

    #region ---Public Methods---

    public nint Query(string symbolName)
    {
        if (_table.TryGetValue(symbolName.GetHashCode(), out nint offset))
        {
            return offset;
        }

        throw new ElfSymbolNotFoundException(symbolName);
    }

    public bool TryQuery(string symbolName, out nint offset)
    {
        return _table.TryGetValue(symbolName.GetHashCode(), out offset);
    }

    #endregion
}