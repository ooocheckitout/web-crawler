from pyspark.sql import SparkSession
spark = SparkSession.builder.getOrCreate()


players_df = (spark
                .read
                .option("multiline","true")
                .json("collections/statistics/data/TopHeroes_TimePlayed"))
players_df.show()


players_df.groupBy("HeroName").count().show()



