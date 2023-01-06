namespace common.Silver;

public class Property
{
    public string Name { get; init; }
    public IList<object> Values { get; init; }
    public string? Alias { get; set; }
}
