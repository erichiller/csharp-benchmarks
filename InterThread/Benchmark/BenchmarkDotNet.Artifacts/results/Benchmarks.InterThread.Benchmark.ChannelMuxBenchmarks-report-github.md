``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|       ChannelMux_LoopTryRead |  23.55 ms |   0.464 ms |    0.476 ms | 1125.0000 | 406.2500 | 343.7500 |     6601251 B |
| ChannelMux_AsyncWaitLoopOnly |  24.77 ms |   0.247 ms |    0.219 ms | 1187.5000 | 156.2500 |        - |     5841614 B |
