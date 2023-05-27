``` ini

BenchmarkDotNet=v0.13.1, OS=ubuntu 21.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.200
  [Host]     : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT
  DefaultJob : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT


```
|                          Method | Iterations | Mean [ms] | Error [ms] | StdDev [ms] |   Gen 0 |  Gen 1 | Allocated [B] |
|-------------------------------- |----------- |----------:|-----------:|------------:|--------:|-------:|--------------:|
|   ResponseContentSlim_SourceGen |       1000 |  1.180 ms |  0.0031 ms |   0.0029 ms | 13.6719 | 3.9063 |      72,026 B |
| ResponseContentSlim_NoSourceGen |       1000 |  1.210 ms |  0.0085 ms |   0.0080 ms | 13.6719 | 3.9063 |      72,026 B |
