namespace common.Silver;

public class Group
{
    public IEnumerable<string> Properties { get; init; }  = new List<string>();
    public string? PartitionBy { get; init; }
    public IEnumerable<Enrichment> Enrichments { get; init; } = new List<Enrichment>();
    public IEnumerable<Constant> Constants { get; init; }  = new List<Constant>();
}

