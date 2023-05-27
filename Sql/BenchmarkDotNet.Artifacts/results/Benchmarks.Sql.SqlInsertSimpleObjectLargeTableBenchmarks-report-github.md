``` ini

BenchmarkDotNet=v0.13.1, OS=ubuntu 21.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.201
  [Host]     : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT
  DefaultJob : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT


```
|                     Method | ObjectsPerSave | SaveIterations | InitialTableObjects | Mean [ms] | Error [ms] | StdDev [ms] | Allocated [B] |
|--------------------------- |--------------- |--------------- |-------------------- |----------:|-----------:|------------:|--------------:|
|                 NpgsqlCopy |              2 |            100 |            10000000 |  64.01 ms |   2.330 ms |    6.573 ms |     189,549 B |
| NpgsqlCopyToPartitionTable |              2 |            100 |            10000000 |  65.01 ms |   2.122 ms |    6.020 ms |     192,381 B |
|                 NpgsqlCopy |             10 |            100 |            10000000 |  69.17 ms |   2.008 ms |    5.728 ms |     317,695 B |
| NpgsqlCopyToPartitionTable |             10 |            100 |            10000000 |  69.67 ms |   2.323 ms |    6.475 ms |     321,424 B |
