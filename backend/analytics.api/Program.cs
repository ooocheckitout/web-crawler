using System.Text.Json.Serialization;
using common;
using Microsoft.Spark.Sql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var spark = SparkSession
    .Builder()
    .Config("spark.sql.session.timeZone", "UTC")
    .GetOrCreate();
builder.Services.AddSingleton(spark);

const string collectionsRoot = @"D:\code\web-crawler\collections";
var collectionLocator = new CollectionLocator(collectionsRoot, new Hasher());
builder.Services.AddSingleton(collectionLocator);
builder.Services.AddSingleton<FileReader>();

builder.Services
    .AddControllers()
    .AddJsonOptions(opts =>
    {
        var enumConverter = new JsonStringEnumConverter();
        opts.JsonSerializerOptions.Converters.Add(enumConverter);
    });

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
