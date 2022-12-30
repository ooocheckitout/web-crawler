from pyspark.sql import SparkSession
spark = SparkSession.builder.enableHiveSupport().getOrCreate()
spark.sparkContext.setLogLevel("WARN")
spark.conf.set("spark.sql.session.timeZone", "UTC")

collectionsRoot = "D:\code\web-crawler\collections"

from pyspark.sql.functions import col, concat_ws

# bronse
heroes_df = (
    spark
    .read
    .option("multiline","true")
    .json(f"{collectionsRoot}/overwatch-heroes/data/Heroes")
    .withColumn("DetailsUrl", concat_ws("", col("Host"), col("RelativeDetailsUrl")))
    .select("Title", "DetailsUrl", "ImageUrl")
)
heroes_df.show()

details_df = (
    spark
    .read
    .option("multiline","true")
    .json(f"{collectionsRoot}/overwatch-details/data/HeroDetails")
)
details_df.show()

quickplay_time_played_df = (
    spark
    .read
    .option("multiline","true")
    .json(f"{collectionsRoot}/overwatch-statistics/data/QuickPlay_TopHeroes_TimePlayed")
)
quickplay_time_played_df.show()

competitive_time_played_df = (
    spark
    .read
    .option("multiline","true")
    .json(f"{collectionsRoot}/overwatch-statistics/data/Competitive_TopHeroes_TimePlayed")
)
competitive_time_played_df.show()

# silver
heroes_with_details_df = (
    heroes_df
    .join(details_df, details_df["Title"] == heroes_df["Title"], "left")
    .select(heroes_df["Title"], "Role", "Abilities", "DetailsUrl", "ImageUrl")
)
heroes_with_details_df.show()





