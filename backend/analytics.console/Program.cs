using System.Text;
using Microsoft.Spark.Sql;
using static Microsoft.Spark.Sql.Functions;

Console.OutputEncoding = Encoding.UTF8;

var spark = SparkSession
    .Builder()
    .Config("spark.sql.session.timeZone", "UTC")
    .GetOrCreate();

const string collectionsRoot = @"D:\code\web-crawler\collections";

var heroesDf = spark
    .Read()
    .Option("multiline", true)
    .Json($"{collectionsRoot}/overwatch-heroes/data/Heroes/*.json")
    .WithColumn("DetailsUrl", ConcatWs("", Col("Host"), Col("RelativeDetailsUrl")))
    .Select("Title", "DetailsUrl", "ImageUrl");

var detailsDf = spark
    .Read()
    .Option("multiline", true)
    .Json($"{collectionsRoot}/overwatch-details/data/HeroDetails/*.json");

var heroesWithDetailsDf = heroesDf
    .Join(detailsDf, detailsDf["Title"] == heroesDf["Title"])
    .Select(heroesDf["Title"], Col("Role"), Col("Abilities"), Col("DetailsUrl"), Col("ImageUrl"));

heroesWithDetailsDf.Show();

var petrolPricesDf = spark
    .Read()
    .Option("multiline", true)
    .Json($"{collectionsRoot}/minfin-petrol-prices/data/Prices/*.json")
    .WithColumn("PriceAsDouble",  RegexpReplace(Col("Price"), ",", ".").Cast("double"));

petrolPricesDf.Where("Title == 'Бензин А-95 премиум'").Show(100);

petrolPricesDf
    .GroupBy("Title").Agg(
        Round(Min("PriceAsDouble"), 2).Alias("min"),
        Round(Max("PriceAsDouble"), 2).Alias("max"),
        Round(Mean("PriceAsDouble"), 2).Alias("mean"),
        Round(Sum("PriceAsDouble"), 2).Alias("sum"))
    .Sort(Col("mean"))
    .Show(100);

