namespace common.Silver;

public class Property
{
    public string Name { get; init; }
    public IList<string> Values { get; init; }
}

public class TransformProperty : Property
{
    public string? Alias { get; init; }
}
