# Analytics

### Prepare

```powershell
setx SPARK_HOME D:\spark-3.2.1-bin-hadoop2.7
setx HADOOP_HOME D:\hadoop-2.7.1
```

### Run

```cmd
cd D:\code\web-crawler\backend\analytics.console\bin\Debug\net6.0
```

```powershell

spark-submit `
--class org.apache.spark.deploy.dotnet.DotnetRunner `
--master local `
microsoft-spark-3-2_2.12-2.1.1.jar `
dotnet analytics.console.dll

```

### Debug

```powershell

spark-submit `
--class org.apache.spark.deploy.dotnet.DotnetRunner `
--master local `
microsoft-spark-3-2_2.12-2.1.1.jar `
debug

```
