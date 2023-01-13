namespace common.Silver;

public class Transformer
{
    public IEnumerable<Property> Transform(IEnumerable<Property> data, TransformerSchema schema)
    {
        var properties = data.Select(x => new Property { Name = x.Name, Values = x.Values }).ToList();

        foreach (var group in schema)
        {
            // computes
            foreach (var compute in group.Computes)
            {
                if (compute.Type == "constant")
                {
                    properties.Add(new Property
                    {
                        Name = compute.Alias,
                        Values = compute.ConstantValues.Cast<object>().ToList()
                    });
                }

                if (compute.Type == "concatenate")
                {
                    var computeProperties = compute.Properties.Select(x =>
                    {
                        var property = properties.Single(y => y.Name == x);
                        return new TransformProperty { Name = property.Name, Values = property.Values, Alias = compute.Alias };
                    }).ToList();

                    int maxLength = computeProperties.Max(x => x.Values.Count);

                    var filledProperties = new List<Property>();
                    foreach (var property in computeProperties)
                    {
                        if (property.Values.Count == 1)
                        {
                            filledProperties.Add(new Property { Name = property.Name, Values = Enumerable.Repeat(property.Values[0], maxLength).ToList() });
                            continue;
                        }

                        filledProperties.Add(property);
                    }

                    var concatenations = new List<string>();
                    for (var concatIndex = 0; concatIndex < maxLength; concatIndex++)
                    {
                        var values = new List<object>();
                        foreach (var property in filledProperties)
                        {
                            if (property.Values.Count == 0) continue;

                            values.Add(property.Values[concatIndex]);
                        }

                        concatenations.Add(string.Join(compute.Separator, values));
                    }

                    properties.Add(new Property
                    {
                        Name = compute.Alias,
                        Values = concatenations.Cast<object>().ToList()
                    });
                }
            }


            // groupings
            var objects = new List<IDictionary<string, object>>();
            var groupProperties = group.Properties.Select(x =>
            {
                var property = properties.Single(y => y.Name == x.Ref);
                return new TransformProperty { Name = property.Name, Values = property.Values, Alias = x.Alias };
            }).ToList();

            if (!groupProperties.Any()) continue;

            int numberOfElements = groupProperties.Max(x => x.Values.Count);

            for (var index = 0; index < numberOfElements; index++)
            {
                var obj = new Dictionary<string, object>();
                foreach (var property in groupProperties)
                {
                    if (property.Values.Count == 0)
                        continue;

                    obj[property.Alias!] = property.Values[index];
                }

                objects.Add(obj);
            }

            // partitions
            var partitionProperties = group.Partitions.Select(x =>
            {
                var property = properties.Single(y => y.Name == x.Ref);
                return new TransformProperty { Name = property.Name, Values = property.Values, Alias = x.Alias };
            });
            foreach (var property in partitionProperties)
            {
                int numberOfObjects = objects.Count;
                int numberOfPartitions = property.Values.Count;
                int numberOfElementsInPartition = numberOfObjects / numberOfPartitions;

                for (var index = 0; index < numberOfPartitions; index++)
                {
                    int from = index * numberOfElementsInPartition;
                    int to = from + numberOfElementsInPartition;

                    foreach (var obj in objects.Take(new Range(from, to)))
                    {
                        obj[property.Alias!] = property.Values[index];
                    }
                }
            }

            // mappigns
            foreach (var mapping in group.Mappings)
            {
                var property = properties.Single(y => y.Name == mapping.Ref);
                var enrichmentProperty = new TransformProperty { Name = property.Name, Values = property.Values, Alias = mapping.Alias };

                foreach (var obj in objects)
                {
                    if (mapping.AtIndex is null)
                    {
                        obj[enrichmentProperty.Alias] = enrichmentProperty.Values;
                    }
                    else
                    {
                        obj[enrichmentProperty.Alias] = enrichmentProperty.Values[mapping.AtIndex.Value];
                    }
                }
            }

            // yield
            yield return new Property { Name = group.Name, Values = objects.Cast<object>().ToList() };
        }
    }
}
