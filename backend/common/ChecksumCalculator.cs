using common.Bronze;
using common.Silver;

namespace common;

public class ChecksumCalculator
{
    readonly Hasher _hasher;

    public ChecksumCalculator(Hasher hasher)
    {
        _hasher = hasher;
    }

    public string GetChecksum(string str)
    {
        return _hasher.GetSha256HashAsHex(str);
    }

    public string GetChecksum(Property property)
    {
        return _hasher.GetSha256HashAsHex(property.PrintPublicProperties());
    }

    public string GetChecksum(ParserSchema schema)
    {
        return _hasher.GetSha256HashAsHex(
            schema.Select(x => x.PrintPublicProperties()).ToArray()
        );
    }

    public string GetChecksum(TransformerSchema schema)
    {
        return _hasher.GetSha256HashAsHex(
            schema.Select(x => x.PrintPublicProperties()).ToArray()
        );
    }

    public string GetParserChecksum(ParserSchema schema, string htmlContent)
    {
        string schemaChecksum = GetChecksum(schema);
        string htmlContentChecksum = GetChecksum(htmlContent);

        return _hasher.GetSha256HashAsHex(schemaChecksum, htmlContentChecksum);
    }

    public string GetTransformerChecksum(TransformerSchema schema, IEnumerable<Property> properties)
    {
        string schemaChecksum = GetChecksum(schema);
        string propertiesChecksum = _hasher.GetSha256HashAsHex(
            properties.Select(x => x.PrintPublicProperties()).ToArray()
        );

        return _hasher.GetSha256HashAsHex(schemaChecksum, propertiesChecksum);
    }
}
