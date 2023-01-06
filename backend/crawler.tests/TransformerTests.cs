using common.Silver;
using FluentAssertions;

namespace crawler.tests;

using Xunit;

public class TransformerTests
{
    readonly Transformer _sut;

    public TransformerTests()
    {
        _sut = new Transformer();
    }

    [Fact]
    public void Do()
    {
        var data = new Data
        {
            { "Hero", new[] { "MERCY", "MOIRA", "ZENYATTA" } },
            { "Statistic", new[] { "04:22:54", "03:48:39", "02:17:36" } },
            { "Category", new[] { "Time Played" } },
            { "Username", new[] { "ezkatka" } }
        };
        var schema = new TransformerSchema
        {
            new()
            {
                PartitionBy = "Category",
                Properties = new[]
                {
                    "Hero",
                    "Statistic"
                },
                Enrichments = new[]
                {
                    new Enrichment
                    {
                        From = "Username",
                        AtIndex = 0
                    }
                },
                Constants = new[]
                {
                    new Constant
                    {
                        Name = "Host",
                        Value = "https://overwatch.blizzard.com"
                    }
                }
            }
        };
        var results = _sut.Transform(data, schema);
        results.Should().NotBeEmpty();
    }
}
