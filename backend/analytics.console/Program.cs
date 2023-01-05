using System.Reflection;
using System.Text;
using analytics.console;
using Microsoft.Spark.Sql;

Console.OutputEncoding = Encoding.UTF8;

var spark = SparkSession
    .Builder()
    .Config("spark.sql.session.timeZone", "UTC")
    .GetOrCreate();

const string collectionsRoot = @"D:/code/web-crawler/collections";

// var quickExample = new TestExample();
// quickExample.Show(collectionsRoot, spark);

var exampleTypes = Assembly
    .GetExecutingAssembly()
    .GetTypes()
    .Where(x => x.GetInterface(nameof(IExample), false) is not null);

var instances = exampleTypes.Select(x => (IExample)Activator.CreateInstance(x)!);
foreach (var example in instances)
{
    example.Show(collectionsRoot, spark);
}
