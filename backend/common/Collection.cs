using common.Bronze;
using common.Silver;

namespace common;

public class Collection
{
    public string Name { get; init; }
    public IEnumerable<string> Urls { get; init; } = Array.Empty<string>();
    public ParserSchema ParserSchema { get; init; }
    public TransformerSchema TransformerSchema { get; init; }
}