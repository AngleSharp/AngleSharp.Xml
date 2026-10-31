namespace AngleSharp.Xml.Benchmarks
{
    using BenchmarkDotNet.Running;
    using System;

    static class Program
    {
        static void Main(String[] args)
        {
            if (args.Length == 1 && args[0].Equals("--check", StringComparison.OrdinalIgnoreCase))
            {
                Check();
                return;
            }

            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
        }

        /// <summary>
        /// Runs every benchmark's setup and body once per parameter combination, without
        /// measuring, so a build that cannot handle a shape (or a document that stopped being
        /// valid) fails in seconds instead of in the middle of a long run.
        /// </summary>
        private static void Check()
        {
            Console.WriteLine(Documents.LibraryMarker);

            foreach (var shape in new[] { "catalog", "catalog-500", "catalog-2000", "deep-10", "deep-100", "deep-1000", "deep-2000" })
            {
                foreach (ParseValidationBenchmark.DoctypeKind doctype in Enum.GetValues(typeof(ParseValidationBenchmark.DoctypeKind)))
                {
                    var benchmark = new ParseValidationBenchmark { Shape = shape, Doctype = doctype };
                    benchmark.GlobalSetup();

                    try
                    {
                        Console.WriteLine($"ParseValidation {shape} {doctype}: valid={benchmark.Parse()}");
                    }
                    finally
                    {
                        benchmark.GlobalCleanup();
                    }
                }
            }

            foreach (var shape in new[] { "leaf", "item", "catalog", "deep-10", "deep-100", "deep-1000", "deep-4000" })
            {
                var benchmark = new TextContentBenchmark { Shape = shape };
                benchmark.GlobalSetup();
                Console.WriteLine($"TextContent {shape}: length={benchmark.TextContent()}");
            }
        }
    }
}
