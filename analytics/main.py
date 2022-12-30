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
    .json(f"{collectionsRoot}/overwatch-heroes/data")
    .withColumn("DetailsUrl", concat_ws("", col("Host"), col("RelativeDetailsUrl")))
    .select("Title", "DetailsUrl", "ImageUrl")
)
heroes_df.show()

details_df = (
    spark
    .read
    .option("multiline","true")
    .json(f"{collectionsRoot}/overwatch-details/data")
)
details_df.show()

players_df = (
    spark
    .read
    .option("multiline","true")
    .json(f"{collectionsRoot}/overwatch-statistics/data/player")
)
players_df.show()

# silver
heroes_with_details_df = (
    heroes_df
    .join(details_df, details_df["Title"] == heroes_df["Title"], "left")
    .select(heroes_df["Title"], "Role", "Abilities", "DetailsUrl", "ImageUrl")
)
heroes_with_details_df.show()





