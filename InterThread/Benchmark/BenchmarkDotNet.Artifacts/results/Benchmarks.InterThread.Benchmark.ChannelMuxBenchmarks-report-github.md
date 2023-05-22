``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|       ChannelMux_LoopTryRead |  23.88 ms |   0.464 ms |    0.434 ms | 1343.7500 | 500.0000 | 343.7500 |     6496966 B |
| ChannelMux_AsyncWaitLoopOnly |  24.66 ms |   0.410 ms |    0.320 ms | 1281.2500 | 281.2500 | 125.0000 |     6694434 B |
