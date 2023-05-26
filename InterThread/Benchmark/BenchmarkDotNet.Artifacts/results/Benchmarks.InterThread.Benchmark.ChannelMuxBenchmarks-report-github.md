``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                 Method | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------- |------------- |---------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| LoopTryRead2_2Producer |       100000 |                 False |  12.52 ms |   0.243 ms |    0.203 ms | 1140.6250 | 890.6250 | 562.5000 |     6294296 B |
| LoopTryRead2_2Producer |       100000 |                  True |  13.15 ms |   0.252 ms |    0.569 ms | 1234.3750 | 781.2500 | 484.3750 |     6588368 B |
