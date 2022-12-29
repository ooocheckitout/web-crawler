public class Schema
{
    public QueryField IdentifierField { get; init; }
    public IReadOnlyList<QueryField> PropertyFields { get; init; } = new List<QueryField>();
    public IReadOnlyList<StaticField> StaticFields { get; init; } = new List<StaticField>();
}
