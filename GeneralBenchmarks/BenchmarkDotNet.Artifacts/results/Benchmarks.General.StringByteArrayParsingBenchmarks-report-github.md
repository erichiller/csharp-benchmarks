``` ini

BenchmarkDotNet=v0.13.4, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2


```
|           Method | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 | Allocated [B] |
|----------------- |----------:|-----------:|------------:|---------:|--------------:|
| ParseManualAllV3 |  1.670 ms |  0.0065 ms |   0.0060 ms | 236.3281 |     1120002 B |
|   ParseManualAll |  4.139 ms |  0.0345 ms |   0.0306 ms | 234.3750 |     1120007 B |
