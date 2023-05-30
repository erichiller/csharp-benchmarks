``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.302
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                 Method | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] | Median [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------- |------------- |---------------------- |----------:|-----------:|------------:|------------:|----------:|---------:|---------:|--------------:|
| LoopTryRead2_2Producer |       100000 |                 False |  13.54 ms |   0.570 ms |    1.654 ms |    12.63 ms | 1125.0000 | 859.3750 | 546.8750 |     6394771 B |
| LoopTryRead2_2Producer |       100000 |                  True |  13.85 ms |   0.569 ms |    1.658 ms |    13.22 ms | 1093.7500 | 859.3750 | 437.5000 |     6332514 B |
