``` ini

BenchmarkDotNet=v0.13.1, OS=ubuntu 21.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.201
  [Host]     : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT
  DefaultJob : .NET 6.0.3 (6.0.322.12309), X64 RyuJIT


```
|                     Method | ObjectsPerSave | SaveIterations | InitialTableObjects | Mean [ms] | Error [ms] | StdDev [ms] |     Gen 0 | Allocated [B] |
|--------------------------- |--------------- |--------------- |-------------------- |----------:|-----------:|------------:|----------:|--------------:|
| NpgsqlCopyToPartitionTable |            100 |           1000 |           100000000 |  812.7 ms |   16.17 ms |    30.76 ms | 3000.0000 |  17,562,144 B |
