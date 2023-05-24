``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                                 Method | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|--------------------------------------- |------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|      ChannelMux_LoopTryRead2_2Producer |       100000 |  10.83 ms |   0.187 ms |    0.175 ms | 1140.6250 | 890.6250 | 531.2500 |     6372327 B |
| ChannelMux_AsyncWaitLoopOnly_2Producer |       100000 |  21.48 ms |   0.427 ms |    1.232 ms | 1437.5000 | 812.5000 | 468.7500 |     7642989 B |
