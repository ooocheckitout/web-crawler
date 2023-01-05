using Microsoft.Spark.Sql;

namespace analytics.console;

class TestExample : IExample
{
    public void Show(string collectionRoot, SparkSession sparkSession)
    {
        var silver = sparkSession
            .Read()
            .Option("multiline", true)
            .Json($"{collectionRoot}/local-data-editor/silver");

        silver.Show();
    }
}
