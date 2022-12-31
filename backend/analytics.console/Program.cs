using Apache.Arrow;
using Microsoft.Spark.Sql;
using static Microsoft.Spark.Sql.Functions;

var spark = SparkSession
    .Builder()
    .Config("spark.sql.session.timeZone", "UTC")
    .GetOrCreate();

var dataFrameReader = new DataFrameReader(spark);

const string collectionsRoot = @"D:\code\web-crawler\collections";

var heroes_df = spark
    .Read()
    .Option("multiline", true)
    .Json($"{collectionsRoot}/overwatch-heroes/data/Heroes/*.json")
    .WithColumn("DetailsUrl", Functions.ConcatWs("", Functions.Col("Host"), Functions.Col("RelativeDetailsUrl")))
    .Select("Title", "DetailsUrl", "ImageUrl");

var details_df = spark
    .Read()
    .Option("multiline", true)
    .Json($"{collectionsRoot}/overwatch-details/data/HeroDetails/*.json");

var heroes_with_details_df = heroes_df
    .Join(details_df, details_df["Title"] == heroes_df["Title"])
    .Select(heroes_df["Title"], Col("Role"), Col("Abilities"), Col("DetailsUrl"), Col("ImageUrl"));

heroes_with_details_df.Show();
