``` ini

BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
Intel Core i7-8700K CPU 3.70GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.101
  [Host]     : .NET Core 5.0.1 (CoreCLR 5.0.120.57516, CoreFX 5.0.120.57516), X64 RyuJIT
  DefaultJob : .NET Core 5.0.1 (CoreCLR 5.0.120.57516, CoreFX 5.0.120.57516), X64 RyuJIT


```
|    Method |        Mean |     Error |    StdDev |      Median |  Gen 0 | Gen 1 | Gen 2 | Allocated |
|---------- |------------:|----------:|----------:|------------:|-------:|------:|------:|----------:|
| GenRand25 |   0.0004 ns | 0.0005 ns | 0.0005 ns |   0.0003 ns |      - |     - |     - |         - |
|    GenKey | 220.3643 ns | 0.1859 ns | 0.1452 ns | 220.3512 ns | 0.0114 |     - |     - |      72 B |
