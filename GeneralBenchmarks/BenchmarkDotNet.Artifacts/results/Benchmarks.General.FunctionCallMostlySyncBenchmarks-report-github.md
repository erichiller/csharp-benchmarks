```

BenchmarkDotNet v0.13.9+228a464e8be6c580ad9408e98f18813f6407fb5a, Ubuntu 22.04.3 LTS (Jammy Jellyfish)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK 7.0.402
  [Host]     : .NET 7.0.12 (7.0.1223.47720), X64 RyuJIT AVX2
  Job-GKPLOR : .NET 7.0.12 (7.0.1223.47720), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                     | WorkFrequency | SyncWorkLevel | Mean [ms] | Error [ms] | StdDev [ms] | Allocated [B] |
|--------------------------- |-------------- |-------------- |----------:|-----------:|------------:|--------------:|
| SyncTriggerTask            | 1000          | 5             |  184.8 ms |    0.31 ms |     0.24 ms |     2721208 B |
| TaskOuter                  | 1000          | 5             |  190.7 ms |    0.17 ms |     0.15 ms |     2720560 B |
| TaskOuter                  | 1000          | 20            |  540.5 ms |    2.94 ms |     2.75 ms |     2721472 B |
| SyncTriggerTask            | 1000          | 20            |  557.1 ms |    2.20 ms |     2.05 ms |     2721448 B |
|                            |               |               |           |            |             |               |
| AsyncTaskWorkInCalled      | 1000          | 5             |  295.3 ms |    0.22 ms |     0.19 ms |     2401208 B |
| AsyncTaskWorkInCalled      | 1000          | 20            |  623.3 ms |    0.59 ms |     0.53 ms |     2401448 B |
|                            |               |               |           |            |             |               |
| AsyncValueTaskWorkInCalled | 1000          | 5             |  422.7 ms |    2.72 ms |     2.55 ms |     2401208 B |
| AsyncValueTaskWorkInCalled | 1000          | 20            |  796.1 ms |    2.76 ms |     2.58 ms |     2401448 B |
