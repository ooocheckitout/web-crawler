using Xunit;

namespace crawler.tests;

public class ParserTests2
{
    [Fact]
    public void ParseKeyValues()
    {
        var expectedResults = new[]
        {
            new Dictionary<string, object>
            {
                {"Title", "A"},
                {"RelativeUrl", "B"},
            }
        };
    }

    [Fact]
    public void ParseArrays()
    {
        var expectedResults = new[]
        {
            new Dictionary<string, object>
            {
                {
                    "Urls", new List<string>
                    {
                        "1",
                        "2",
                        "3"
                    }
                }
            }
        };
    }

    [Fact]
    public void ParseObjects()
    {
        var expectedResults = new[]
        {
            new Dictionary<string, object>
            {
                {"Title", "A"},
                {"RelativeUrl", "A"},
                {"Abilities", new[] {"1", "2", "3"}}
            },
            new Dictionary<string, object>
            {
                {"Title", "B"},
                {"RelativeUrl", "B"},
                {"Abilities", new[] {"4", "5", "6"}}
            },
            new Dictionary<string, object>
            {
                {"Title", "C"},
                {"RelativeUrl", "C"},
                {"Abilities", new[] {"7", "8", "9"}}
            }
        };
    }
}
