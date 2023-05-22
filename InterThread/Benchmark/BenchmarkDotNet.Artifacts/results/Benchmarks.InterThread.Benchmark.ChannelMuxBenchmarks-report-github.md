``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|     ChannelMux_LoopTryRead_2 |  11.27 ms |   0.222 ms |    0.272 ms | 1109.3750 | 781.2500 | 500.0000 |     6044332 B |
|       ChannelMux_LoopTryRead |  13.95 ms |   0.178 ms |    0.166 ms | 1046.8750 | 968.7500 | 968.7500 |     8227056 B |
| ChannelMux_AsyncWaitLoopOnly |  21.47 ms |   0.426 ms |    1.243 ms | 1343.7500 | 812.5000 | 437.5000 |     7189697 B |
