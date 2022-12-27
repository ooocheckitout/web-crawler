using System.Collections;

static class Extensions
{
    public static void Dump(this object obj)
    {
        if (obj is IEnumerable asEnumerable and not string)
        {
            foreach (var item in asEnumerable)
            {
                Dump(item);
            }
        }
        else
        {
            Console.WriteLine(obj);
        }
    }
}