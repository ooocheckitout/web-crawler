namespace common.Silver;

public class PropertyReference
{
    public string Ref { get; set; }
    public string Alias { get; set; }
}

public class PropertyEnrichment : PropertyReference
{
    public int? AtIndex { get; set; }
}

public class ComputedProperty
{
    public string Type { get; set; }
    public IEnumerable<string> Properties { get; init; } = new List<string>();
    public string Separator { get; set; }
    public string Alias { get; set; }
    public IEnumerable<string> ConstantValues { get; init; } = new List<string>();
}

public class Group
{
    public string Name { get; set; }
    public IEnumerable<ComputedProperty> Computes { get; init; } = new List<ComputedProperty>();
    public IEnumerable<PropertyReference> Properties { get; init; } = new List<PropertyReference>();
    public IEnumerable<PropertyReference> Partitions { get; init; } = new List<PropertyReference>();
    public IEnumerable<PropertyEnrichment> Mappings { get; init; } = new List<PropertyEnrichment>();
}
