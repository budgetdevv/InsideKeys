using System;
using System.Diagnostics;
using System.Threading.Tasks;
using CustomBot;
using InsideKeys;

namespace InsideKeyTests
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkMultiThreaded();
        }

        public static void FunctionalityTest()
        {
            var KE = new InsideKeyEngine();
            
            var x = KE.GenCode(TimeSpan.FromHours(42));

            Console.WriteLine(x);
            
            var Expiry = KE.Decode(x);

            Console.WriteLine(InsideHelper.TimeSpanToWords(Expiry - DateTime.UtcNow));
        }
        
        public static void BenchmarkMultiThreaded()
        {
            var KE = new InsideKeyEngine();

            var CachedTimeSpan = TimeSpan.FromHours(42);

            var SW = new Stopwatch();
            
            SW.Restart();
            
            Parallel.For(0, Environment.ProcessorCount, _ =>
            {
                for (int I = 1; I <= 5_000_000; I++)
                {
                    var x = KE.GenCode(CachedTimeSpan);
                }
            });
            
            SW.Stop();

            Console.WriteLine(SW.ElapsedMilliseconds);
        }
    }
}