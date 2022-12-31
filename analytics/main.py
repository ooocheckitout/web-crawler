from pyspark.sql import SparkSession
spark = SparkSession.builder.enableHiveSupport().getOrCreate()
spark.sparkContext.setLogLevel("WARN")
spark.conf.set("spark.sql.session.timeZone", "UTC")

collectionsRoot = "D:\code\web-crawler\collections"

from pyspark.sql.functions import col, lit, concat_ws

# bronse
heroes_df = (
    spark
    .read
    .option("multiline","true")
    .json(f"{collectionsRoot}/overwatch-heroes/data/Heroes/*.json")
    .withColumn("DetailsUrl", concat_ws("", col("Host"), col("RelativeDetailsUrl")))
    .select("Title", "DetailsUrl", "ImageUrl")
)
heroes_df.show()

details_df = (
    spark
    .read
    .option("multiline","true")
    .json(f"{collectionsRoot}/overwatch-details/data/HeroDetails/*.json")
)
details_df.show()

quickplay_time_played_df = (
    spark
    .read
    .option("multiline","true")
    .json(f"{collectionsRoot}/overwatch-statistics/data/QuickPlay_TopHeroes_TimePlayed/*.json")
    .withColumn("GameMode", lit("Quick"))
)
quickplay_time_played_df.show()

competitive_time_played_df = (
    spark
    .read
    .option("multiline","true")
    .json(f"{collectionsRoot}/overwatch-statistics/data/Competitive_TopHeroes_TimePlayed/*.json")
    .withColumn("GameMode", lit("Competitive"))
)
competitive_time_played_df.show()

# silver
heroes_with_details_df = (
    heroes_df
    .join(details_df, details_df["Title"] == heroes_df["Title"], "left")
    .select(heroes_df["Title"], "Role", "Abilities", "DetailsUrl", "ImageUrl")
)
heroes_with_details_df.show()

from pyspark.sql.functions import col, split, array_union, array, lit, when, size
time_played_df = (
    quickplay_time_played_df.union(competitive_time_played_df)

    .withColumn("splits", split(col("HumanTime"), ":"))
    .withColumn("hours_minutes_seconds", 
        when(size("splits") == 2, array_union(array(lit("00")), col("splits")))
        .otherwise(col("splits"))
    )

    .withColumn("hours", col("hours_minutes_seconds").getItem(0).cast("int"))
    .withColumn("minutes", col("hours_minutes_seconds").getItem(1).cast("int"))
    .withColumn("seconds", col("hours_minutes_seconds").getItem(2).cast("int"))
    .withColumn("total_seconds", col("hours") * 3600 + col("minutes") * 60 + col("seconds"))
)
time_played_df.show()

# gold

(
    time_played_df
    .groupBy("Username").count()
    # .filter("HeroName == 'Winston' and Username == 'Adam'")
    .show()
)

from pyspark.sql.functions import to_timestamp, sum, floor
(
    time_played_df
    
    .groupBy("Username").agg(sum("total_seconds").alias("sum_played_seconds"))
    .orderBy(col("sum_played_seconds"), ascending=False)

    .withColumn("days_seconds", floor(col("sum_played_seconds") / 86400) * 86400)
    .withColumn("hours_seconds", floor((col("sum_played_seconds") - col("days_seconds")) / 3600) * 3600)

    .withColumn("played_days", (col("days_seconds") / 86400).cast("int"))
    .withColumn("played_hours", (col("hours_seconds") / 3600).cast("int"))
    .show()
)

