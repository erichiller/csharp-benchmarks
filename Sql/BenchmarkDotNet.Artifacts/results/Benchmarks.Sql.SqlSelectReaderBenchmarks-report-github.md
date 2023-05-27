``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                                                       Method | Iterations | Mean [ns] | Error [ns] |
|------------------------------------------------------------- |----------- |----------:|-----------:|
| &#39;Use type specific NpgsqlDataReader methods, eg. GetInt32()&#39; |        200 |        NA |         NA |

Benchmarks with issues:
  SqlSelectReaderBenchmarks.'Use type specific NpgsqlDataReader methods, eg. GetInt32()': DefaultJob [Iterations=200]
