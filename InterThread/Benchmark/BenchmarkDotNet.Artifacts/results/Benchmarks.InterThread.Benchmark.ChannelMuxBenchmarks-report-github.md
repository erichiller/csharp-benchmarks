``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                 Method | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |     Gen2 | Allocated [B] |
|----------------------- |------------- |---------------------- |----------:|-----------:|------------:|----------:|----------:|---------:|--------------:|
| LoopTryRead2_2Producer |       100000 |                 False |  12.28 ms |   0.198 ms |    0.220 ms | 1265.6250 | 1000.0000 | 546.8750 |     6518853 B |
| LoopTryRead2_2Producer |       100000 |                  True |  12.94 ms |   0.255 ms |    0.602 ms | 1125.0000 |  843.7500 | 546.8750 |     6250709 B |
