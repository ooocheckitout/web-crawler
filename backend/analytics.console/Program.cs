var fileReader = new FileReader();
var fileWriter = new FileWriter();
var webDownloader = new WebDownloader(new HttpClient(), fileWriter);
var hasher = new Hasher();
var collectionManager = new CollectionManager(fileReader, webDownloader, hasher, fileWriter);

await foreach (var playersArray in collectionManager.GetJsonDataAsync(Constants.Collections.Players))
{
    foreach (var player in playersArray.EnumerateArray())
    {
       var totalMillisecondsPlayed = player
            .GetProperty("TimePlayed_Hours")
            .EnumerateArray()
            .Select(x => x.GetString())
            .Select(x =>
            {
                if (TimeSpan.TryParseExact(x, @"hh\:mm\:ss", null, out var asTimeSpan))
                    return asTimeSpan;

                return TimeSpan.ParseExact(x, @"mm\:ss", null);
            })
            .Sum(x => x.TotalMilliseconds);

       var name = player.GetProperty("Title").GetString();
       var timePlayed = TimeSpan.FromMilliseconds(totalMillisecondsPlayed);
       Console.WriteLine($"{name} played {timePlayed} in total");
    }
}

