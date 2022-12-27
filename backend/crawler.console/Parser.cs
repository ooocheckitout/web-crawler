using HtmlAgilityPack;

class Parser
{
    public IEnumerable<IDictionary<string, string>> ParseMultipleObject(string content, IEnumerable<Field> fields)
    {
        var document = new HtmlDocument();
        document.LoadHtml(content);

        // collect values for every field
        var fieldValues = fields.ToDictionary<Field?, Field, IList<string>>(
            field => field,
            field => GetFieldValues(document, field).ToList());

        // ensure number of values is equal between fields
        var numberOfElements = fieldValues.Values.First().Count;
        if (fieldValues.Where(x => !x.Key.IsMetadata).Any(x => x.Value.Count != numberOfElements))
            throw new Exception("Invalid number of parsed values!");

        // construct object
        for (var i = 0; i < numberOfElements; i++)
        {
            var dict = new Dictionary<string, string>();
            foreach (var field in fieldValues.Keys)
            {
                if (field.IsMetadata)
                {
                    var staticValue = fieldValues[field].Single();

                    dict.Add(field.Name, staticValue);
                    continue;
                }

                var value = fieldValues[field][i];
                dict.Add(field.Name, value);
            }

            yield return dict;
        }
    }

    public IDictionary<string, object> ParseSingleObject(string content, IEnumerable<Field> fields)
    {
        var document = new HtmlDocument();
        document.LoadHtml(content);

        var dict = new Dictionary<string, object>();
        foreach (var field in fields)
        {
            var values = GetFieldValues(document, field);

            if (!field.IsArray)
                dict.Add(field.Name, values.Single());
            else
                dict.Add(field.Name, values);
        }

        return dict;
    }

    IEnumerable<string> GetFieldValues(HtmlDocument document, Field field)
    {
        if (field.IsMetadata && field.IsStatic)
            return new[] { field.Value! };

        var results = document.DocumentNode.SelectNodes(field.XPath);
        if (results is null)
            return Array.Empty<string>();

        var values = results.Select(x => GetNodeValue(x, field));
        if (field.IsArray)
            return values;

        return values;
    }

    string GetNodeValue(HtmlNode node, Field field)
    {
        if (field.Attribute is not null)
        {
            return node.Attributes[field.Attribute].Value;
        }

        if (field.IsStatic && field.Value is not null)
        {
            return field.Value;
        }

        return node.InnerText;
    }
}