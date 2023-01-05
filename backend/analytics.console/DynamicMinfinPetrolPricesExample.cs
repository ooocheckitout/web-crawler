using Microsoft.Spark.Sql;

namespace analytics.console;

class DynamicMinfinPetrolPricesExample : IExample
{
    public void Show(string collectionRoot, SparkSession sparkSession)
    {
        var schema = new[]
        {
            new TransformColumn { Name = "Operator", Type = "string" },
            new TransformColumn { Name = "A95Plus", Type = "int", Replacements = new Dictionary<string, string> { { ",", "." } } },
            new TransformColumn { Name = "A95", Type = "int", Replacements = new Dictionary<string, string> { { ",", "." } } },
            new TransformColumn { Name = "A92", Type = "int", Replacements = new Dictionary<string, string> { { ",", "." } } },
            new TransformColumn { Name = "Disel", Type = "int", Replacements = new Dictionary<string, string> { { ",", "." } } },
            new TransformColumn { Name = "Gasoline", Type = "int", Replacements = new Dictionary<string, string> { { ",", "." } } },
        };

        var bronze = sparkSession
            .Read()
            .Option("multiline", true)
            .Json($"{collectionRoot}/minfin-petrol-prices/bronze");

        var columns = schema.Select(x => Functions.Col(x.Name));
        var explodeColumns = schema.Select(x =>
            x.Replacements
                .Aggregate(Functions.Col($"explode.{x.Name}"), (acc, replacement) => Functions.RegexpReplace(acc, replacement.Key, replacement.Value))
                .Cast(x.Type)
                .Alias(x.Name));

        var silver = bronze
            .WithColumn("explode", GroupColumns(columns.ToArray()))
            .Select(explodeColumns.ToArray());

        silver.Show();
    }

    static Column GroupColumns(params Column[] columns)
    {
        return Functions.Explode(Functions.ArraysZip(columns));
    }
}
