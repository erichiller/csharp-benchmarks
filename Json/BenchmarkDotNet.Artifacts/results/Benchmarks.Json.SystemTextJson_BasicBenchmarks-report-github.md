``` ini

BenchmarkDotNet=v0.13.1, OS=ubuntu 21.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.200
  [Host]     : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT
  DefaultJob : .NET 6.0.2 (6.0.222.6406), X64 RyuJIT


```
|                                                             Method | Iterations | Mean [μs] | Error [μs] | StdDev [μs] |    Gen 0 |   Gen 1 | Allocated [B] |
|------------------------------------------------------------------- |----------- |----------:|-----------:|------------:|---------:|--------:|--------------:|
|              SystemTextJson_Deserialize_Scalars_NodaTime_SourceGen |       1000 |  697.8 μs |    2.13 μs |     1.99 μs |  41.9922 | 13.6719 |     200,025 B |
| SystemTextJson_Deserialize_Scalars_NodaTimeWithAttribute_SourceGen |       1000 |  709.5 μs |    5.68 μs |     5.31 μs |  41.9922 | 13.6719 |     200,025 B |
|           SystemTextJson_Deserialize_Scalars_NodaTimeWithAttribute |       1000 |  709.6 μs |    6.71 μs |     6.28 μs |  41.9922 | 13.6719 |     200,033 B |
|                        SystemTextJson_Deserialize_Scalars_NodaTime |       1000 |  725.9 μs |    3.26 μs |     2.72 μs |  41.9922 | 13.6719 |     200,033 B |
|                SystemTextJson_Deserialize_Scalars_NodaTime__Record |       1000 |  974.5 μs |    5.48 μs |     5.12 μs | 156.2500 | 50.7813 |     744,026 B |
