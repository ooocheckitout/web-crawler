using HtmlAgilityPack;

class Parser
{
    public IEnumerable<IDictionary<string, object>> Parse(string htmlContent, Schema schema)
    {
        var document = new HtmlDocument();
        document.LoadHtml(htmlContent);

        var objects = new List<IDictionary<string, object>>();
        foreach (var field in schema.Fields)
        {
            var xpathResults = document.DocumentNode.SelectNodes(field.XPath);
            if (xpathResults is null)
                throw new InvalidOperationException($"No elements found for {field.Name} field!");

            var values = GetDocumentValues(document, field).ToList();

            for (var i = 0; i < values.Count; i++)
            {
                if (objects.ElementAtOrDefault(i) is null)
                    objects.Add(new Dictionary<string, object>());

                objects[i].Add(field.Name, values[i]);
            }
        }

        foreach (var obj in objects)
        foreach (var field in schema.MetadataFields)
        {
            var values = GetDocumentValues(document, field).ToList();
            if (values.Count == 1)
            {
                obj.Add(field.Name, values.First());
                continue;
            }

            obj.Add(field.Name, values);
        }

        foreach (var obj in objects)
        foreach (var field in schema.StaticFields)
            obj.Add(field.Name, field.Value);

        return objects;
    }

    IEnumerable<string> GetDocumentValues(HtmlDocument document, QueryField field)
    {
        Console.WriteLine($"Retrieving value for {field.Name} field");
        
        var xpathResults = document.DocumentNode.SelectNodes(field.XPath);
        if (xpathResults is null)
            throw new InvalidOperationException($"No elements found for {field.Name} field!");

        return xpathResults.Select(x => GetNodeValue(x, field)).ToList();
    }

    string GetNodeValue(HtmlNode node, QueryField field)
    {
        if (field.Attribute is null)
            return node.InnerText;

        return node.Attributes[field.Attribute].Value;
    }
}