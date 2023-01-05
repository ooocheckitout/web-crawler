using Microsoft.Spark.Sql;

namespace analytics.console;

class SqlMinfinPetrolPricesExample : IExample
{
    public void Show(string collectionRoot, SparkSession sparkSession)
    {
        var path = @$"{collectionRoot}/minfin-petrol-prices/bronze";

        var sql = @$"
DROP VIEW IF EXISTS bronze;

CREATE TEMPORARY VIEW bronze
USING org.apache.spark.sql.json
OPTIONS (
    path ""{path}"",
    multiline true
);

DROP VIEW IF EXISTS silver;

CREATE TEMPORARY VIEW silver AS (
    SELECT
        explode.Operator,
        cast(replace(explode.A95Plus, ',', '.') as int) AS A95Plus,
        cast(replace(explode.A95, ',', '.') as int) AS A95,
        cast(replace(explode.A92, ',', '.') as int) AS A92,
        cast(replace(explode.Disel, ',', '.') as int) AS Disel,
        cast(replace(explode.Gasoline, ',', '.') as int) AS Gasoline
    FROM (
        SELECT explode(arrays_zip(Operator, A95Plus, A95, A92, Disel, Gasoline)) AS explode FROM bronze
    )
);

SELECT
    Operator,
    round(min(A95Plus), 2) as A95Plus_Min,
    round(max(A95Plus), 2) as A95Plus_Max,
    round(mean(A95Plus), 2) as A95Plus_Mean
FROM silver
WHERE A95Plus is not null
GROUP BY Operator
ORDER BY A95Plus_Mean;
";

        string[] queries = sql
            .Replace(Environment.NewLine, " ")
            .Split(";", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (string query in queries)
        {
            var df = sparkSession.Sql(query);

            if (!df.IsEmpty())
                df.Show();
        }
    }
}
