namespace common.Silver;

public class Transformer
{
    public IEnumerable<IDictionary<string, object>> Transform(Data data, TransformerSchema schema)
    {
        var properties = data.Select(item => new Property
        {
            Name = item.Key,
            Values = item.Value.ToList()
        }).ToList();

        foreach (var group in schema)
        {
            int numberOfElements = group.Properties
                .SelectMany(p => properties.Where(y => y.Name == p))
                .Max(x => x.Values.Count);

            var objects = new List<IDictionary<string, object>>();
            for (var objectIndex = 0; objectIndex < numberOfElements; objectIndex++)
            {
                var obj = new Dictionary<string, object>();
                foreach (var property in group.Properties.SelectMany(p => properties.Where(y => y.Name == p)))
                {
                    obj[property.Alias ?? property.Name] = property.Values[objectIndex];
                }

                objects.Add(obj);
            }

            if (group.PartitionBy is not null)
            {
                var property = properties.Single(y => y.Name == group.PartitionBy);
                int numberOfObjects = objects.Count;
                int numberOfPartitions = property.Values.Count;
                int numberOfElementsInPartition = numberOfObjects / numberOfPartitions;

                for (var partitionIndex = 0; partitionIndex < numberOfPartitions; partitionIndex++)
                {
                    int from = partitionIndex * numberOfElementsInPartition;
                    int to = from + numberOfElementsInPartition;

                    foreach (var obj in objects.Take(new Range(from, to)))
                    {
                        obj[property.Name] = property.Values[partitionIndex];
                    }
                }
            }

            foreach (var map in group.Enrichments)
            {
                var property = properties.Single(y => y.Name == map.From);
                foreach (var obj in objects)
                {
                    obj[property.Name] = property.Values[map.AtIndex];
                }
            }

            foreach (var constant in group.Constants)
            {
                foreach (var obj in objects)
                {
                    obj[constant.Name] = constant.Value;
                }
            }

            foreach (var obj in objects)
            {
                yield return obj;
            }
        }
    }
}
