namespace TreatyGraph.Core.Model;

/// <summary>Looks up <see cref="Country"/> records by COW country code.</summary>
public sealed class CountryDirectory
{
    private readonly Dictionary<int, Country> _byCode;

    public CountryDirectory(IEnumerable<Country> countries)
    {
        _byCode = countries.ToDictionary(c => c.Ccode);
    }

    public IReadOnlyCollection<Country> All => _byCode.Values;

    public bool Contains(int ccode) => _byCode.ContainsKey(ccode);

    public string Name(int ccode) =>
        _byCode.TryGetValue(ccode, out var c) ? c.Name : $"Unknown (COW {ccode})";
}
