using FluentAssertions;
using Xunit;

class HeroDetails
{
    public string Title { get; set; }
    public IEnumerable<string> Abilities { get; set; }
}

class Hero
{
    public string Title { get; set; }
    public string RelativeDetailsUrl { get; set; }
    public string ImageUrl { get; set; }
    public string Host { get; set; }
}

public class IntegrationTests
{
    [Fact]
    public async Task Details()
    {
        var fileReader = new FileReader();

        var content = await fileReader.ReadTextFileAsync("cases/details/content.html");
        var schema = await fileReader.ReadJsonFileAsync<Schema>("cases/details/schema.json");
        var results = await fileReader.ReadJsonFileAsync<IEnumerable<HeroDetails>>("cases/details/results.json");
        var dataAsDictionary = results.Select(x => new Dictionary<string, object>
        {
            { nameof(x.Title), x.Title },
            { nameof(x.Abilities), x.Abilities }
        });

        var parser = new Parser();
        parser.Parse(content, schema).Should().BeEquivalentTo(dataAsDictionary);
    }
    [Fact]
    public async Task Heroes()
    {
        var fileReader = new FileReader();

        var content = await fileReader.ReadTextFileAsync("cases/heroes/content.html");
        var schema = await fileReader.ReadJsonFileAsync<Schema>("cases/heroes/schema.json");
        var results = await fileReader.ReadJsonFileAsync<IEnumerable<Hero>>("cases/heroes/results.json");
        var dataAsDictionary = results.Select(x => new Dictionary<string, object>
        {
            { nameof(x.Title), x.Title },
            { nameof(x.RelativeDetailsUrl), x.RelativeDetailsUrl },
            { nameof(x.ImageUrl), x.ImageUrl },
            { nameof(x.Host), x.Host },
        });

        var parser = new Parser();
        parser.Parse(content, schema).Should().BeEquivalentTo(dataAsDictionary);
    }
}