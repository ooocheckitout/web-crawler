using HtmlAgilityPack;

class Parser
{
    public IEnumerable<IDictionary<string, string>> ParseMultipleObject(string content, IEnumerable<Field> fields)
    {
        var document = new HtmlDocument();
        document.LoadHtml(content);
        
        // collect values for every field
        var fieldValues = new Dictionary<string, IList<string>>(); 
        foreach (var field in fields)
        {
            var results = document.DocumentNode.SelectNodes(field.XPath);
            if (results is null || !results.Any())
            {
                Console.WriteLine($"No results for field {field.Name}");
                continue;
            }

            var values = results.Select(x => GetNodeValue(x, field)).ToList();
            fieldValues.Add(field.Name, values);
        }

        // ensure number of values is equal between fields
        var numberOfElements = fieldValues.Values.First().Count;
        if (fieldValues.Values.Any(x => x.Count != numberOfElements))
            throw new Exception("Invalid number of parsed values!");

        // construct object
        for (var i = 0; i < numberOfElements; i++)
        {
            var dict = new Dictionary<string, string>();
            foreach (var field in fieldValues.Keys)
            {
                var value = fieldValues[field][i];
                dict.Add(field, value);
            }

            yield return dict;
        }
    }

    public IDictionary<string, string> ParseSingleObject(string content, IEnumerable<Field> fields)
    {
        var document = new HtmlDocument();
        document.LoadHtml(content);
        
        var dict = new Dictionary<string, string>();
        foreach (var field in fields)
        {
            var results = document.DocumentNode.SelectNodes(field.XPath);
            if (results is null || !results.Any())
            {
                Console.WriteLine($"No results for field {field.Name}");
                continue;
            }

            var node = results.SingleOrDefault();
            if (node is null)
                throw new Exception("Only one node is expected!");
            
            dict.Add(field.Name, GetNodeValue(node, field));
        }

        return dict;
    }

    string GetNodeValue(HtmlNode node, Field field)
    {
        return field.Attribute is not null
            ? node.Attributes[field.Attribute].Value
            : node.InnerText;
    }
}