### Analytics

# Run
```powershell

spark-submit `
--class org.apache.spark.deploy.dotnet.DotnetRunner `
--master local `
microsoft-spark-3-2_2.12-2.1.1.jar `
debug

```
```powershell

spark-submit `
--class org.apache.spark.deploy.dotnet.DotnetRunner `
--master local `
microsoft-spark-3-2_2.12-2.1.1.jar `
dotnet analytics.console.dll

```
