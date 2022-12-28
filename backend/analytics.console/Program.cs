using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Text.Json.Serialization;

var fileReader = new FileReader();

var playerFiles = Directory.EnumerateFiles("../../../../crawler.console/json/players");
var playerStatisticTasks = playerFiles
    .Select(x => fileReader.FromJsonFileAsync<IEnumerable<PlayerHeroStatistics>>(x));
var playerStatisticArray = await Task.WhenAll(playerStatisticTasks);
var playerStatistics = playerStatisticArray.SelectMany(x => x).ToList();

var totalTimePlayed = TimeSpan.FromMilliseconds(playerStatistics.Sum(x => x.TimePlayedHours.TotalMilliseconds)).Dump();

class PlayerHeroStatistics
{
    public string Name { get; set; }
    public string AvatarUrl { get; set; }
    public string TimePlayedHeroNames { get; set; }

    [JsonConverter(typeof(CustomTimeSpanConverter))]
    public TimeSpan TimePlayedHours { get; set; }

    public override string ToString()
    {
        var properties = GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        var keyValueStrings = properties.Select(x => $"{x.Name}: {x.GetValue(this)}");
        return string.Join(", ", keyValueStrings);
    }
}
