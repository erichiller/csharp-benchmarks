``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.202
  [Host]     : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2


```
|            Method | Mean [μs] | Error [μs] | StdDev [μs] | Ratio | Allocated [B] | Alloc Ratio |
|------------------ |----------:|-----------:|------------:|------:|--------------:|------------:|
|   InParams_Return |  151.2 μs |    0.27 μs |     0.21 μs |  0.92 |             - |          NA |
|      SimpleReturn |  165.0 μs |    1.01 μs |     0.90 μs |  1.00 |             - |          NA |
| InParams_OutParam |  169.4 μs |    0.70 μs |     0.65 μs |  1.03 |             - |          NA |
