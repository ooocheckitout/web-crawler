namespace common.Silver;

public class Property
{
    public string Name { get; init; }
    public List<object> Values { get; init; } = new();
}

public class TransformProperty : Property
{
    public string? Alias { get; init; }
}
