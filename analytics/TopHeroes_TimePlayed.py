from pyspark.sql import SparkSession
spark = SparkSession.builder.getOrCreate()
spark.sparkContext.setLogLevel("WARN")
spark.conf.set("spark.sql.session.timeZone", "UTC")

# bronse
time_played_df = (
    spark
    .read
    .option("multiline","true")
    .json("collections/statistics/data/TopHeroes_TimePlayed")
)
time_played_df.show()

# silver
from pyspark.sql.functions import col, split, array_union, array, lit, when, size

time_played_as_columns_df = (
    time_played_df
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
time_played_as_columns_df.show()

# gold

from pyspark.sql.functions import to_timestamp, sum, dayofmonth, hour, minute, second
total_played_time_df = (
    time_played_as_columns_df
    
    .groupBy("HeroName").agg(sum("total_seconds").alias("played_seconds"))
    .orderBy(col("played_seconds"), ascending=False)

    .withColumn("played_timestamp", to_timestamp(col("played_seconds")))

    .withColumn("played_days", dayofmonth(col("played_timestamp")) - lit(1))
    .withColumn("played_hours", hour(col("played_timestamp")))
    .withColumn("played_minutes", minute(col("played_timestamp")))
    .withColumn("played_seconds", second(col("played_timestamp")))
)

total_played_time_df.show()



