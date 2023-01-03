using Microsoft.Spark.Sql;

namespace analytics.console;

interface IExample
{
    void Show(string collectionRoot, SparkSession sparkSession);
}
