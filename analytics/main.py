from pyspark.sql import SparkSession
spark = SparkSession.builder.getOrCreate()


players_df = (spark
                .read
                .option("multiline","true")
                .json("collections/statistics/data"))
players_df.show()


from pyspark.sql.functions import arrays_zip, col
players_df.select(arrays_zip(col("Heroes"), col("HeroIndexes").alias("zipped"))).show()