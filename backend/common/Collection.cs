public class Collection
{
    public string Name { get; init; }
    public IEnumerable<string> Urls { get; init; } = Array.Empty<string>();
    public IEnumerable<Schema> Schemas { get; init; } = Array.Empty<Schema>();
}
