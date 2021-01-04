using System;
using System.Diagnostics;
using System.Runtime.Intrinsics.Arm;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using CustomBot;
using InsideKeys;

namespace InsideKeyTests
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<Bench>();

            //BenchmarkMultiThreaded();
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
    
    [MemoryDiagnoser]
    public class Bench
    {
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";

        private Random Rand;

        private InsideKeyEngine KE;

        private TimeSpan TS;
        
        [GlobalSetup]
        public void GlobalSetup()
        {
            Rand = new Random(1258);

            KE = new InsideKeyEngine();

            TS = TimeSpan.FromHours(12);
        }

        [Benchmark]
        public void GenKey()
        {
            var Code = KE.GenCode(TS);
        }
    }
}