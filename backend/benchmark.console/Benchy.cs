using BenchmarkDotNet.Attributes;
using MoreLinq.Extensions;

namespace benchmark.console
{
    [ShortRunJob]
    public class Benchy
    {
        List<int> _items;

        [Params(100, 1000)]
        public int NumberOfItems { get; set; }

        [Params(100)]
        public int DelayMs { get; set; }

        [GlobalSetup]
        public void Setup() => _items = Enumerable.Range(0, NumberOfItems).ToList();

        [Benchmark]
        public void Foreach()
        {
            foreach (int _ in _items)
            {
                Thread.Sleep(DelayMs);
            }
        }

        [Benchmark]
        public void Parallel()
        {
            foreach (var batch in _items.Batch(10))
            {
                foreach (int _ in batch.AsParallel())
                {
                    Thread.Sleep(DelayMs);
                }
            }
        }
    }
}