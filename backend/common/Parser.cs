using HtmlAgilityPack;

public class Parser
{
    public IEnumerable<Dictionary<string, object>> Parse(string htmlContent, Schema schema)
    {
        var document = new HtmlDocument();
        document.LoadHtml(htmlContent);

        var objects = new List<Dictionary<string, object>>();

        var identifiers = GetFieldValue(schema.IdentifierField, document);
        foreach (string identifier in identifiers)
        {
            objects.Add(new Dictionary<string, object>
            {
                {schema.IdentifierField.Name, identifier}
            });
        }

        for (var index = 0; index < objects.Count; index++)
        {
            var obj = objects[index];
            foreach (var propertyField in schema.PropertyFields)
            {
                var values = GetFieldValue(propertyField, document, new ParserVariables {Index = index + 1});

                if (values.Count == 1)
                {
                    obj.Add(propertyField.Name, values.First());
                    continue;
                }

                obj.Add(propertyField.Name, values);
            }

            foreach (var staticField in schema.StaticFields)
            {
                obj.Add(staticField.Name, staticField.Value);
            }
        }

        return objects;
    }

    private ICollection<string> GetFieldValue(QueryField field, HtmlDocument document, ParserVariables? specials = default)
    {
        string calculatedXpath = field.XPath.Replace("_index_", specials?.Index.ToString());

        var results = document.DocumentNode.SelectNodes(calculatedXpath);
        if (results is null)
            throw new InvalidOperationException($"No elements found for {field.Name} field!");

        return results.Select(x => GetNodeValue(x, field)).ToList();
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
