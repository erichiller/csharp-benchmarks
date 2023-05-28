``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                 Method | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------- |------------- |---------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| LoopTryRead2_2Producer |       100000 |                 False |  12.44 ms |   0.246 ms |    0.328 ms | 1140.6250 | 875.0000 | 578.1250 |     6330847 B |
| LoopTryRead2_2Producer |       100000 |                  True |  13.04 ms |   0.259 ms |    0.700 ms | 1218.7500 | 843.7500 | 453.1250 |     6482290 B |
