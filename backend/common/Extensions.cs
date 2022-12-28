using System.Collections;

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
}