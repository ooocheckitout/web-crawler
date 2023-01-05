using System.Text;
using analytics.console;
using Microsoft.Spark.Sql;

Console.OutputEncoding = Encoding.UTF8;

var spark = SparkSession
    .Builder()
    .Config("spark.sql.session.timeZone", "UTC")
    .GetOrCreate();

const string collectionsRoot = @"D:/code/web-crawler/collections";

var example = new TestExample();
example.Show(collectionsRoot, spark);

// var example = new TailwindColorPaletteExample();
// example.Show(collectionsRoot, spark);

// var exampleTypes = Assembly
//     .GetExecutingAssembly()
//     .GetTypes()
//     .Where(x => x.GetInterface(nameof(IExample), false) is not null);
//
// var instances = exampleTypes.Select(x => (IExample)Activator.CreateInstance(x)!);
// foreach (var example in instances)
// {
//     example.Show(collectionsRoot, spark);
// }


// var heroesDf = spark
//     .Read()
//     .Option("multiline", true)
//     .Json($"{collectionsRoot}/overwatch-heroes/data/Heroes/*.json")
//     .WithColumn("DetailsUrl", ConcatWs("", Col("Host"), Col("RelativeDetailsUrl")))
//     .Select("Title", "DetailsUrl", "ImageUrl");
//
// var detailsDf = spark
//     .Read()
//     .Option("multiline", true)
//     .Json($"{collectionsRoot}/overwatch-details/data/HeroDetails/*.json");
//
// var heroesWithDetailsDf = heroesDf
//     .Join(detailsDf, detailsDf["Title"] == heroesDf["Title"])
//     .Select(heroesDf["Title"], Col("Role"), Col("Abilities"), Col("DetailsUrl"), Col("ImageUrl"));
//
// heroesWithDetailsDf.Show();
