class Field
{
    public string Name { get; set; }
    public string? XPath { get; set; }
    public string? Attribute { get; set; }
    public string? Value { get; set; }
    public bool IsMetadata { get; set; }
    public bool IsStatic { get; set; }
    public bool IsArray { get; set; }
}