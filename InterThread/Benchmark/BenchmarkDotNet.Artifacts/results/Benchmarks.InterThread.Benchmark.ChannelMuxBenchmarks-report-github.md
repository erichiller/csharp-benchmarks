``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                 Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| ChannelMux_LoopTryRead |  15.19 ms |   0.152 ms |    0.142 ms | 1093.7500 | 406.2500 | 312.5000 |     5334845 B |
