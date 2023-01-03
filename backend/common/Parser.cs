using HtmlAgilityPack;

namespace common;

public class Parser
{
    public IDictionary<string, object> Parse(string htmlContent, Schema schema)
    {
        var document = new HtmlDocument();
        document.LoadHtml(htmlContent);

        var dataObject = new Dictionary<string, object>();
        foreach (var field in schema)
        {
            if (field.IsStatic)
            {
                dataObject.Add(field.Name, field.StaticValue);
                continue;
            }

            var cleanedXPath = $"{field.XPath} | {field.XPath.Replace("tbody", "")}";
            var results = document.DocumentNode.SelectNodes(cleanedXPath);
            if (results is null)
                throw new InvalidOperationException($"No elements found for {field.Name} field!");

            var values = results.Select(x => GetNodeValue(x, field));
            dataObject.Add(field.Name, results.Count == 1 ? values.Single() : values);
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
