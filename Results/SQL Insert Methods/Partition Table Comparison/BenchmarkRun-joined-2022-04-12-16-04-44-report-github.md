``` ini

BenchmarkDotNet=v0.13.1, OS=ubuntu 21.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.202
  [Host]     : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT
  DefaultJob : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT


```
|                                               Type |                     Method | ObjectsPerSave | SaveIterations | InitialTableObjects | Mean [s] | Error [s] | StdDev [s] |     Gen 0 | Allocated [B] |
|--------------------------------------------------- |--------------------------- |--------------- |--------------- |-------------------- |---------:|----------:|-----------:|----------:|--------------:|
| SqlInsertSimpleObjectLargePartitionTableBenchmarks | NpgsqlCopyToPartitionTable |            100 |           1000 |           100000000 |  1.405 s |  0.0931 s |   0.2657 s | 3000.0000 |  17,566,928 B |
|          SqlInsertSimpleObjectLargeTableBenchmarks |                 NpgsqlCopy |            100 |           1000 |           100000000 |  1.815 s |  0.1203 s |   0.3294 s | 3000.0000 |  17,537,872 B |
