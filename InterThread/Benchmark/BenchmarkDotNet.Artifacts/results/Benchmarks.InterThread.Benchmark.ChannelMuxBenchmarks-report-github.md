``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                 Method | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] | Median [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------- |------------- |---------------------- |----------:|-----------:|------------:|------------:|----------:|---------:|---------:|--------------:|
| LoopTryRead2_2Producer |       100000 |                  True |  12.77 ms |   0.254 ms |    0.458 ms |    12.67 ms | 1140.6250 | 859.3750 | 468.7500 |     6407654 B |
| LoopTryRead2_2Producer |       100000 |                 False |  12.93 ms |   0.283 ms |    0.797 ms |    12.63 ms | 1203.1250 | 968.7500 | 546.8750 |     6421922 B |
