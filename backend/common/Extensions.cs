using System.Collections;
using System.Reflection;
using System.Text;

public static class Extensions
{
    public static T Dump<T>(this T obj)
    {
        if (obj is IEnumerable asEnumerable and not string)
            foreach (var item in asEnumerable)
                Dump(item);
        else
            Console.WriteLine(obj);

        return obj;
    }

    public static string Print(this object obj)
    {
        var sb = new StringBuilder();
        var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var property in properties)
        {
            sb.Append($"{property.Name}: {property.GetValue(obj)} {Environment.NewLine}");
        }

        return sb.ToString();
    }
}
