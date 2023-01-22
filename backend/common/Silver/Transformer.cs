using Microsoft.Extensions.Logging;

namespace common.Silver;

public class Transformer
{
    readonly ILogger<Transformer> _logger;

    readonly Dictionary<string, Func<ComputedProperty, IReadOnlyDictionary<string, Property>, Property>> _computeHandlers = new()
    {
        { "constant", CreateConstantProperty },
        { "concatenate", CreateConcatenatedProperty },
    };

    public Transformer(ILogger<Transformer> logger)
    {
        _logger = logger;
    }

    public IEnumerable<Property> Transform(IReadOnlyCollection<Property> data, TransformerSchema schema)
    {
        var propertiesMap = data.ToDictionary(x => x.Name);

        foreach (var group in schema)
        {
            // computes
            foreach (var compute in group.Computes)
            {
                if (!_computeHandlers.ContainsKey(compute.Type))
                {
                    _logger.LogWarning("Compute type {computeType} is not supported!", compute.Type);
                    continue;
                }

                var property = _computeHandlers[compute.Type](compute, propertiesMap);
                propertiesMap.Add(property.Name, property);
            }

            // groupings
            var groupProperties = group.Properties.Select(x => CreatePropertyByRef(propertiesMap, x)).ToList();
            var objects = CreateGroupBy(groupProperties).ToList();

            // partitions
            var partitionProperties = group.Partitions.Select(x => CreatePropertyByRef(propertiesMap, x)).ToList();
            var partitions = partitionProperties.SelectMany(x => PartitionOver(objects, x));
            foreach (var partition in partitions)
            {
                foreach (var partitionObject in partition.Objects)
                    partitionObject[partition.Name] = partition.Value;
            }

            // mappings
            foreach (var mapping in group.Mappings)
            {
                var property = propertiesMap[mapping.Ref];
                foreach (var obj in objects)
                    obj[mapping.Alias] = mapping.AtIndex.HasValue ? property.Values[mapping.AtIndex.Value] : property.Values;
            }

            yield return new Property { Name = group.Name, Values = objects.Cast<object>().ToList() };
        }
    }

    Property CreatePropertyByRef(IDictionary<string, Property> propertyMap, PropertyReference reference)
    {
        if (propertyMap.ContainsKey(reference.Ref))
            return new Property { Name = reference.Alias, Values = propertyMap[reference.Ref].Values };

        _logger.LogInformation("Property is not found by reference {reference}. Continue with empty property...", reference.Ref);
        return new Property { Name = reference.Alias };
    }

    static Property CreateConcatenatedProperty(ComputedProperty compute, IReadOnlyDictionary<string, Property> propertiesMap)
    {
        int maxLength = propertiesMap.Values.Max(x => x.Values.Count);
        var propertyValues = compute.Properties
            .Select(x => propertiesMap[x])
            .Select(x => x.Values.Count == 1 ? Enumerable.Repeat(x.Values[0], maxLength).ToList() : x.Values)
            .ToList();

        var concatenations = new List<object>();
        for (var concatIndex = 0; concatIndex < maxLength; concatIndex++)
        {
            var results = new List<object>();
            foreach (var values in propertyValues)
            {
                if (values.Count != maxLength)
                    throw new NotSupportedException(values.Count.ToString());

                results.Add(values[concatIndex]);
            }

            concatenations.Add(string.Join(compute.Separator, results));
        }

        return new Property
        {
            Name = compute.Alias,
            Values = concatenations.ToList()
        };
    }

    static Property CreateConstantProperty(ComputedProperty compute, IReadOnlyDictionary<string, Property> propertiesMap)
    {
        return new Property
        {
            Name = compute.Alias,
            Values = compute.ConstantValues.Cast<object>().ToList()
        };
    }

    static IEnumerable<PartitionProperty> PartitionOver(List<IDictionary<string, object>> objects, Property property)
    {
        int numberOfPartitions = property.Values.Count;
        int numberOfElementsInPartition = objects.Count / numberOfPartitions;

        var partitions = new List<PartitionProperty>();
        for (var partitionIndex = 0; partitionIndex < numberOfPartitions; partitionIndex++)
        {
            int from = partitionIndex * numberOfElementsInPartition;
            int to = from + numberOfElementsInPartition;

            partitions.Add(new PartitionProperty { Name = property.Name, Value = property.Values[partitionIndex], Objects = objects.GetRange(from, to) });
        }

        return partitions;
    }

    static IEnumerable<IDictionary<string, object>> CreateGroupBy(ICollection<Property> properties)
    {
        int maxLength = properties.Max(x => x.Values.Count);

        var objects = new List<IDictionary<string, object>>();
        for (var objectIndex = 0; objectIndex < maxLength; objectIndex++)
            objects.Add(properties.ToDictionary(property => property.Name, property => property.Values[objectIndex]));

        return objects;
    }
}
