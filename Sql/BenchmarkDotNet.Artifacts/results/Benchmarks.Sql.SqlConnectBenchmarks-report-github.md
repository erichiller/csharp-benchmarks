``` ini

BenchmarkDotNet=v0.13.1, OS=ubuntu 21.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.202
  [Host]     : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT
  DefaultJob : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT


```
|                           Method | Iterations | Mean [ms] | Error [ms] | StdDev [ms] | Allocated [B] |
|--------------------------------- |----------- |----------:|-----------:|------------:|--------------:|
| NpgsqlConnectionPerIterationTime |       1000 |  56.21 ms |   1.089 ms |    2.098 ms |   1,012,584 B |
|       NpgsqlConnectionSingleTime |       1000 |  73.14 ms |   1.397 ms |    1.663 ms |   1,347,200 B |
