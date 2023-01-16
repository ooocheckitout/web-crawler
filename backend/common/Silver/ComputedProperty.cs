namespace common.Silver;

public class ComputedProperty
{
    public string Type { get; set; }
    public IEnumerable<string> Properties { get; init; } = new List<string>();
    public string Separator { get; set; }
    public string Alias { get; set; }
    public IEnumerable<string> ConstantValues { get; init; } = new List<string>();
}
