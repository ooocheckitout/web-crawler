using System.Text;

namespace common.Silver;

public class Data : List<Property>
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
