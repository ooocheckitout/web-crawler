using common.Silver;
using HtmlAgilityPack;

namespace common.Bronze;

public class Parser
{
    public IEnumerable<Property> Parse(string htmlContent, ParserSchema schema)
    {
        var document = new HtmlDocument();
        document.LoadHtml(htmlContent);

        var properties = new List<Property>();
        foreach (var field in schema)
        {
            var cleanedXPath = $"{field.XPath} | {field.XPath.Replace("tbody", "")}";
            var results = document.DocumentNode.SelectNodes(cleanedXPath);
            // if (results is null)
            //     throw new InvalidOperationException($"No elements found for {field.Name} field!");

            if (results is null)
            {
                properties.Add(new Property
                {
                    Name = field.Name, Values = new List<object>()
                });
            }
            else
            {
                properties.Add(new Property
                {
                    Name = field.Name, Values = results.Select(x => GetNodeValue(x, field)).Cast<object>().ToList()
                });
            }
        }

        return properties;
    }

    string GetNodeValue(HtmlNode node, QueryField field)
    {
        if (field.Attribute is null)
            return node.InnerText;

        if (!node.Attributes.Contains(field.Attribute))
            throw new Exception($"Field {field.Name} attribute {field.Attribute} does not exist on the element!");

        return node.Attributes[field.Attribute].Value;
    }
}
