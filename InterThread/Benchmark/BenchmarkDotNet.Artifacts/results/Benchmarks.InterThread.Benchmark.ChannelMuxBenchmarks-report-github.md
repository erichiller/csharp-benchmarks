``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                                   Method | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|----------------------------------------- |------------- |----------:|-----------:|------------:|----------:|----------:|----------:|--------------:|
| ChannelMux_LoopTryRead2_8Producer_8Tasks |       100000 |  88.84 ms |   1.749 ms |    3.765 ms | 6500.0000 | 4166.6667 | 1333.3333 |    39397537 B |
