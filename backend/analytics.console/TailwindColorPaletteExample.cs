using Microsoft.Spark.Sql;
using static Microsoft.Spark.Sql.Functions;

namespace analytics.console;

class TailwindColorPaletteExample : IExample
{
    public void Show(string collectionRoot, SparkSession sparkSession)
    {
        var bronze = sparkSession
            .Read()
            .Option("multiline", true)
            .Json($"{collectionRoot}/tailwind-color-palette/bronze");

        var silver = bronze
            .WithColumn("expColor", Explode(Col("Color")))
            .WithColumn("index", MonotonicallyIncreasingId())
            .WithColumn("from", (Col("index") * 10 + 1))
            .WithColumn("to", ((Col("index") + 1) * 10))
            .WithColumn("colorShades", Slice(Col("Shade"), Col("from"), Col("to")))
            .WithColumn("colorHexes", Slice(Col("Hex"), Col("from"), Col("to")))
            .WithColumn("zipped", ArraysZip(Col("colorShades"), Col("colorHexes")))
            .WithColumn("exploded", Explode(Col("zipped")))
            .Select(Col("expColor").Alias("Color"), Col("exploded.colorShades").Alias("Shade"), Col("exploded.colorHexes").Alias("Hex"));

        silver.Filter("Color in ('Slate', 'Rose')").Show();
    }
}
