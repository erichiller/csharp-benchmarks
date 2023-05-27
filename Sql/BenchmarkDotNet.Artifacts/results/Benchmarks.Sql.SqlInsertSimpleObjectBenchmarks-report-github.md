``` ini

BenchmarkDotNet=v0.13.1, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.101
  [Host]     : .NET 6.0.12 (6.0.1222.56807), X64 RyuJIT
  DefaultJob : .NET 6.0.12 (6.0.1222.56807), X64 RyuJIT


```
|                                                        Method | ObjectsPerSave | SaveIterations | Mean [s] | Error [s] | StdDev [s] | Median [s] | Allocated [B] |
|-------------------------------------------------------------- |--------------- |--------------- |---------:|----------:|-----------:|-----------:|--------------:|
|                              NpgsqlInsert_Batched_Boxed_Value |             10 |            100 |  1.309 s |  0.2634 s |   0.7766 s |   0.7080 s |   1,275,304 B |
|                 NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value |             10 |            100 |  2.167 s |  0.0538 s |   0.1542 s |   2.1763 s |   1,280,728 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value_LessDefinedVars |             10 |            100 |  2.177 s |  0.0435 s |   0.0702 s |   2.1643 s |   1,280,544 B |
|                               NpgsqlInsert_Batched_TypedValue |             10 |            100 |  2.179 s |  0.0430 s |   0.0869 s |   2.1674 s |   1,264,672 B |
|           NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue |             10 |            100 |  2.182 s |  0.0433 s |   0.0759 s |   2.1817 s |   1,280,728 B |
