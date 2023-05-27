``` ini

BenchmarkDotNet=v0.13.4, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.202
  [Host]     : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2


```
|                                                                    Method | ObjectsPerSave | SaveIterations | Mean [ns] | Error [ns] |
|-------------------------------------------------------------------------- |--------------- |--------------- |----------:|-----------:|
| NpgSqlInsert_SingularCommand_TypedValue_NpgsqlDbType_NpgsqlValue_Prepared |              2 |             10 |        NA |         NA |

Benchmarks with issues:
  SqlInsertComplexObjectBenchmarks.NpgSqlInsert_SingularCommand_TypedValue_NpgsqlDbType_NpgsqlValue_Prepared: DefaultJob [ObjectsPerSave=2, SaveIterations=10]
