public class QueryField
{
    public string Name { get; set; }
    public string XPath { get; set; }
    public string? Attribute { get; set; }
    public bool IsStatic { get; set; }
    public object StaticValue { get; set; }
}
