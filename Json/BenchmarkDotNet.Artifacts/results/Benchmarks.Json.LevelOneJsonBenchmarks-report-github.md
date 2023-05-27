``` ini

BenchmarkDotNet=v0.13.1, OS=ubuntu 21.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.200
  [Host]     : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT
  DefaultJob : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT


```
|                                              Method | Iterations | WithSourceGenerationContext | Mean [ms] | Error [ms] | StdDev [ms] |   Gen 0 | Allocated [B] |
|---------------------------------------------------- |----------- |---------------------------- |----------:|-----------:|------------:|--------:|--------------:|
| ResponseContentSlim_SourceGenNoOptions_NoRename_Key |       1000 |                       False |  1.213 ms |  0.0045 ms |   0.0042 ms | 11.7188 |      64,002 B |
| ResponseContentSlim_SourceGenNoOptions_NoRename_Key |       1000 |                        True |  1.237 ms |  0.0114 ms |   0.0106 ms | 11.7188 |      64,018 B |
