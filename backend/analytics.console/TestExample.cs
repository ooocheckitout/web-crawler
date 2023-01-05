using analytics.console;
using Microsoft.Spark.Sql;
using static Microsoft.Spark.Sql.Functions;

class TestExample : IExample
{
    public void Show(string collectionRoot, SparkSession sparkSession)
    {
        var bronze = sparkSession
            .Read()
            .Option("multiline", true)
            .Json($"{collectionRoot}/test/data");

        bronze.Show();

        var silver = bronze
                .WithColumn("schema", SchemaOfJson(bronze.Select("values").Collect().First().Values.First().ToString()))
            // .WithColumn("explode", Explode(FromJson(Col("values"), SchemaOfJson(Col("values")))))
            ;
        silver.Show();
    }
}
