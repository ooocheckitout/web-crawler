using Microsoft.Spark.Sql;
using static Microsoft.Spark.Sql.Functions;

namespace analytics.console;

class PythonMinfinPetrolPricesExample : IExample
{
    public void Show(string collectionRoot, SparkSession sparkSession)
    {
        var bronze = sparkSession
            .Read()
            .Option("multiline", true)
            .Json($"{collectionRoot}/minfin-petrol-prices/bronze");

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
