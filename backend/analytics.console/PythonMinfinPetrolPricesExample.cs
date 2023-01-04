using Microsoft.Spark.Sql;
using static Microsoft.Spark.Sql.Functions;

namespace analytics.console;

class TransformColumn
{
    public string Name { get; init; }
    public string Type { get; init; }
    public IDictionary<string, string> Replacements { get; init; } = new Dictionary<string, string>();
}

class TransformSchema : Dictionary<string, IEnumerable<TransformColumn>>
{
}

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
            .Json($"{collectionRoot}/minfin-petrol-prices/data");

        var columns = schema.Select(x => Col(x.Name));
        var explodeColumns = schema.Select(x =>
            x.Replacements
                .Aggregate(Col($"explode.{x.Name}"), (acc, replacement) => RegexpReplace(acc, replacement.Key, replacement.Value))
                .Cast(x.Type)
                .Alias(x.Name));

        var silver = bronze
            .WithColumn("explode", GroupColumns(columns.ToArray()))
            .Select(explodeColumns.ToArray());

        silver.Show();
    }

    static Column GroupColumns(params Column[] columns)
    {
        return Explode(ArraysZip(columns));
    }
}

class PythonMinfinPetrolPricesExample : IExample
{
    public void Show(string collectionRoot, SparkSession sparkSession)
    {
        var bronze = sparkSession
            .Read()
            .Option("multiline", true)
            .Json($"{collectionRoot}/minfin-petrol-prices/data");

        bronze.Show();

        var silver = bronze
            .WithColumn("zipped", ArraysZip(
                    Col("Operator"),
                    Col("A95Plus"),
                    Col("A95"),
                    Col("A92"),
                    Col("Disel"),
                    Col("Gasoline")
                )
            )
            .WithColumn("exploded", Explode(Col("zipped")))
            .WithColumn("Operator", Col("exploded.Operator"))
            .WithColumn("A95Plus", RegexpReplace(Col("exploded.A95Plus"), ",", ".").Cast("int"))
            .WithColumn("A95", RegexpReplace(Col("exploded.A95"), ",", ".").Cast("int"))
            .WithColumn("A92", RegexpReplace(Col("exploded.A92"), ",", ".").Cast("int"))
            .WithColumn("Disel", RegexpReplace(Col("exploded.Disel"), ",", ".").Cast("int"))
            .WithColumn("Gasoline", RegexpReplace(Col("exploded.Gasoline"), ",", ".").Cast("int"))
            .Select("Operator", "A95Plus", "A95", "A92", "Disel", "Gasoline", "Region");

        silver.Filter("Operator == 'Shell'").Show();

        var gold = silver
            .Where("A95Plus is not null")
            .GroupBy("Operator").Agg(
                Round(Min("A95Plus"), 2).Alias("A95Plus_Min"),
                Round(Max("A95Plus"), 2).Alias("A95Plus_Max"),
                Round(Mean("A95Plus"), 2).Alias("A95Plus_Mean")
            )
            .Sort(Col("A95Plus_Mean"));

        gold.Show();
    }
}
