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

heroUrls = [row[0] for row in heroes_df.select("DetailsUrl").collect() ]

import os
import json
fileLocation = f"{collectionsRoot}/overwatch-heroes/analytics/urls.json"
os.makedirs(os.path.dirname(fileLocation), exist_ok=True)
with open(fileLocation, "w") as writer:
    json.dump(heroUrls, writer, ensure_ascii=False, indent=4)
