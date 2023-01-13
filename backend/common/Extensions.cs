using System.Collections;
using System.Reflection;
using System.Text;

namespace common;

public static class Extensions
{
    public static string? Dump<T>(this T obj)
    {
        if (obj is null) return null;

        if (obj is IEnumerable asEnumerable and not string)
        {
            var sb = new StringBuilder();
            foreach (object? item in asEnumerable)
            {
                sb.Append(Dump(item));
            }
            return sb.ToString();
        }

        var objString = obj.ToString();
        var typeString = obj.GetType().ToString();

        return objString == typeString ? obj.PrintPublicProperties() : objString;
    }

    public static string PrintPublicProperties(this object obj)
    {
        var sb = new StringBuilder();
        var properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
        foreach (var property in properties)
        {
            object? value = property.GetValue(obj);
            if (value is IEnumerable asEnumerable and not string)
            {
                value = $"[Count: {asEnumerable.Cast<object>().Count()}]";
            }


            sb.Append($"{property.Name}: {value} {Environment.NewLine}");
        }

        return sb.ToString();
    }

    public static IEnumerable<(T Item, int Index)> WithIndex<T>(this IEnumerable<T> enumerable)
    {
        var index = 0;
        return enumerable.Select(x => (x, index++));
    }
}