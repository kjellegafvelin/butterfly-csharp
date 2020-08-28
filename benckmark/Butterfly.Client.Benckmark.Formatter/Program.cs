using System;

namespace Butterfly.Client.Benckmark.Formatter
{
    static class Program
    {
        private static void Main(string[] args)
        {
            BenchmarkDotNet.Running.BenchmarkRunner.Run<SerializationBenchmarks>();
        }
    }
}
