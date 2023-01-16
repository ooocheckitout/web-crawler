namespace common.Silver;

public class Group
{
    public string Name { get; set; }
    public IEnumerable<ComputedProperty> Computes { get; init; } = new List<ComputedProperty>();
    public IEnumerable<PropertyReference> Properties { get; init; } = new List<PropertyReference>();
    public IEnumerable<PropertyReference> Partitions { get; init; } = new List<PropertyReference>();
    public IEnumerable<PropertyEnrichment> Mappings { get; init; } = new List<PropertyEnrichment>();
}