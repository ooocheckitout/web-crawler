using System.Text;

namespace common.Bronze;

public class ParserSchema: List<QueryField>
{
    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var item in this)
        {
            sb.Append(item.Print());
        }

        return sb.ToString();
    }
}
