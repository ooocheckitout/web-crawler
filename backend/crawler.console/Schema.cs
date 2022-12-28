public class Schema
{
    public IReadOnlyList<QueryField> Fields { get; init; } = new List<QueryField>();
    public IReadOnlyList<QueryField> MetadataFields { get; init; } = new List<QueryField>();
    public IReadOnlyList<StaticField> StaticFields { get; init; } = new List<StaticField>();
}