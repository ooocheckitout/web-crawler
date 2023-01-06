using HtmlAgilityPack;

namespace common.Bronze;

public class Parser
{
    public IDictionary<string, object> Parse(string htmlContent, ParserSchema schema)
    {
        var document = new HtmlDocument();
        document.LoadHtml(htmlContent);

        var dataObject = new Dictionary<string, object>();
        foreach (var field in schema)
        {
            var cleanedXPath = $"{field.XPath} | {field.XPath.Replace("tbody", "")}";
            var results = document.DocumentNode.SelectNodes(cleanedXPath);
            if (results is null)
                throw new InvalidOperationException($"No elements found for {field.Name} field!");

            dataObject.Add(field.Name, results.Select(x => GetNodeValue(x, field)));
        }

        return dataObject;
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
