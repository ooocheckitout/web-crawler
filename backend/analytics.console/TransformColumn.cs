namespace analytics.console;

class TransformColumn
{
    public string Name { get; init; }
    public string Type { get; init; }
    public IDictionary<string, string> Replacements { get; init; } = new Dictionary<string, string>();
}
