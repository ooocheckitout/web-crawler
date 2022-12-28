using FluentAssertions;
using Xunit;

namespace crawler.tests;

public class ParserTests
{
    readonly Parser _sut;

    public ParserTests()
    {
        _sut = new Parser();
    }

    [Fact]
    public void Parse_ShouldHandleTextValue()
    {
        const string htmlContent = @"
<html>
    <body>
        <h1>Excepteur</h1>
    </body>
</html>
";

        var schema = new Schema
        {
            Fields = new[]
            {
                new QueryField
                {
                    Name = "Title",
                    XPath = "html/body/h1"
                }
            }
        };

        _sut.Parse(htmlContent, schema).Should().BeEquivalentTo(new[]
        {
            new Dictionary<string, object>
            {
                { "Title", "Excepteur" }
            }
        });
    }

    [Fact]
    public void Parse_ShouldHandleAttributeValue()
    {
        const string htmlContent = @"
<html>
    <body>
        <img src=""https:\\images.com\Excepteur.png"" />
    </body>
</html>
";

        var schema = new Schema
        {
            Fields = new[]
            {
                new QueryField
                {
                    Name = "Image",
                    XPath = "html/body/img",
                    Attribute = "src"
                }
            }
        };

        _sut.Parse(htmlContent, schema).Should().BeEquivalentTo(new[]
        {
            new Dictionary<string, object>
            {
                { "Image", @"https:\\images.com\Excepteur.png" }
            }
        });
    }

    [Fact]
    public void Parse_ShouldHandleMultipleFields()
    {
        const string htmlContent = @"
<html>
    <body>
        <a href=""https:\\heroes.com\Consequatur"">
            <h1>Consequatur</h1>
            <p>Et harum quidem rerum facilis est et expedita distinctio.</p>
        </a>
    </body>
</html>
";

        var schema = new Schema
        {
            Fields = new[]
            {
                new QueryField
                {
                    Name = "Title",
                    XPath = "html/body/a/h1",
                },
                new QueryField
                {
                    Name = "Description",
                    XPath = "html/body/a/p",
                },
                new QueryField
                {
                    Name = "Link",
                    XPath = "html/body/a",
                    Attribute = "href"
                }
            }
        };

        _sut.Parse(htmlContent, schema).Should().BeEquivalentTo(new[]
        {
            new Dictionary<string, object>
            {
                { "Title", "Consequatur" },
                { "Description", "Et harum quidem rerum facilis est et expedita distinctio." },
                { "Link", @"https:\\heroes.com\Consequatur" }
            }
        });
    }

    [Fact]
    public void Parse_ShouldHandleMultipleResults()
    {
        const string htmlContent = @"
<html>
    <body>
        <h1>Voluptatibus</h1>
        <h1>Repudiandae</h1>
        <h1>Delectus</h1>
    </body>
</html>
";

        var schema = new Schema
        {
            Fields = new[]
            {
                new QueryField
                {
                    Name = "Title",
                    XPath = "html/body/h1",
                }
            }
        };

        _sut.Parse(htmlContent, schema).Should().BeEquivalentTo(new[]
        {
            new Dictionary<string, object>
            {
                { "Title", "Voluptatibus" },
            },
            new Dictionary<string, object>
            {
                { "Title", "Repudiandae" },
            },
            new Dictionary<string, object>
            {
                { "Title", "Delectus" },
            }
        });
    }

    [Fact]
    public void Parse_ShouldHandleMixedResults()
    {
        const string htmlContent = @"
<html>
    <body>
        <h1>Voluptatibus</h1>
        <p>Nam libero tempore, cum soluta nobis est eligendi.</p>
        <p>Aut reiciendis voluptatibus maiores alias consequatur.</p>
        <p>Temporibus autem quibusdam et aut officiis debitis.</p>
    </body>
</html>
";

        var schema = new Schema
        {
            Fields = new[]
            {
                new QueryField
                {
                    Name = "Title",
                    XPath = "html/body/h1",
                }
            },
            MetadataFields = new []
            {
                new QueryField
                {
                    Name = "Descriptions",
                    XPath = "html/body/p",
                }
            }
        };

        _sut.Parse(htmlContent, schema).Should().BeEquivalentTo(new[]
        {
            new Dictionary<string, object>
            {
                { "Title", "Voluptatibus" },
                {
                    "Descriptions", new[]
                    {
                        "Nam libero tempore, cum soluta nobis est eligendi.",
                        "Aut reiciendis voluptatibus maiores alias consequatur.",
                        "Temporibus autem quibusdam et aut officiis debitis."
                    }
                },
            }
        });
    }

    [Fact]
    public void Parse_ShouldHandleStaticFields()
    {
        const string htmlContent = @"
<html>
    <head>
        <meta content=""en_US"">
    </head>
    <body>
        <h1>Voluptatibus</h1>
        <h1>Temporibus</h1>
        <h1>Debitis</h1>
    </body>
</html>
";

        var schema = new Schema
        {
            Fields = new[]
            {
                new QueryField
                {
                    Name = "Title",
                    XPath = "html/body/h1"
                }
            },
            MetadataFields = new[]
            {
                new QueryField
                {
                    Name = "Locale",
                    XPath = "/html/head/meta",
                    Attribute = "content"
                }
            },
            StaticFields = new[]
            {
                new StaticField
                {
                    Name = "Host",
                    Value = @"https:\\heroes.com"
                },
            }
        };

        _sut.Parse(htmlContent, schema).Should().BeEquivalentTo(new[]
        {
            new Dictionary<string, object>
            {
                { "Title", "Voluptatibus" },
                { "Host", @"https:\\heroes.com" },
                { "Locale", "en_US" },
            },
            new Dictionary<string, object>
            {
                { "Title", "Temporibus" },
                { "Host", @"https:\\heroes.com" },
                { "Locale", "en_US" },
            },
            new Dictionary<string, object>
            {
                { "Title", "Debitis" },
                { "Host", @"https:\\heroes.com" },
                { "Locale", "en_US" },
            }
        });
    }
}