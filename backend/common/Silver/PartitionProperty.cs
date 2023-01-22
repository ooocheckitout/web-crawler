namespace common.Silver;

public class PartitionProperty
{
    public string Name { get; set; }
    public object Value { get; set; }
    public IEnumerable<IDictionary<string, object>> Objects { get; set; }
}
