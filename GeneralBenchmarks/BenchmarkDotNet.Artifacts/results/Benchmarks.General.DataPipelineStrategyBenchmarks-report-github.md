```

BenchmarkDotNet v0.13.12, Ubuntu 22.04.4 LTS (Jammy Jellyfish)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK 8.0.200
  [Host]     : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2


```
| Method                                            | WorkDelayMilliseconds | Mean [ms] | Error [ms] | StdDev [ms] | Ratio | Allocated [B] | Alloc Ratio |
|-------------------------------------------------- |---------------------- |----------:|-----------:|------------:|------:|--------------:|------------:|
| Wait_SendAndForgetTask_Static                     | 10                    |  51.75 ms |   0.240 ms |    0.213 ms |  0.20 |        2206 B |        5.99 |
| Wait_SendAndForgetTask                            | 10                    |  51.83 ms |   0.279 ms |    0.261 ms |  0.20 |        3998 B |       10.86 |
| Wait_WorkReturningTaskWithThreadYield             | 10                    |  59.88 ms |   0.101 ms |    0.089 ms |  0.24 |        7754 B |       21.07 |
| Wait_WorkReturningTaskWithThreadYield_Static      | 10                    |  59.96 ms |   0.180 ms |    0.160 ms |  0.24 |        7546 B |       20.51 |
| Wait_WorkReturningTask                            | 10                    |  59.97 ms |   0.184 ms |    0.172 ms |  0.24 |        7754 B |       21.07 |
| Wait_EventCall                                    | 10                    | 253.18 ms |   0.218 ms |    0.193 ms |  1.00 |        1032 B |        2.80 |
| Wait_DirectCall                                   | 10                    | 253.21 ms |   0.139 ms |    0.130 ms |  1.00 |         368 B |        1.00 |
| Wait_WorkReturningValueTaskWithThreadYield        | 10                    | 299.46 ms |   0.426 ms |    0.356 ms |  1.18 |        7696 B |       20.91 |
| Wait_WorkReturningValueTask                       | 10                    | 299.67 ms |   0.704 ms |    0.658 ms |  1.18 |        7696 B |       20.91 |
| Wait_WorkReturningValueTaskWithThreadYield_Static | 10                    | 299.78 ms |   0.756 ms |    0.670 ms |  1.18 |        7488 B |       20.35 |
|                                                   |                       |           |            |             |       |               |             |
| Wait_WorkReturningTaskWithThreadYield_Static      | 20                    | 101.57 ms |   0.370 ms |    0.346 ms |  0.20 |        7637 B |       10.38 |
| Wait_WorkReturningTaskWithThreadYield             | 20                    | 101.60 ms |   0.580 ms |    0.543 ms |  0.20 |        7845 B |       10.66 |
| Wait_SendAndForgetTask_Static                     | 20                    | 101.89 ms |   0.330 ms |    0.309 ms |  0.20 |        2405 B |        3.27 |
| Wait_SendAndForgetTask                            | 20                    | 101.98 ms |   0.242 ms |    0.227 ms |  0.20 |        4005 B |        5.44 |
| Wait_WorkReturningTask                            | 20                    | 103.28 ms |   1.553 ms |    1.377 ms |  0.21 |        7845 B |       10.66 |
| Wait_DirectCall                                   | 20                    | 503.23 ms |   0.129 ms |    0.120 ms |  1.00 |         736 B |        1.00 |
| Wait_EventCall                                    | 20                    | 503.33 ms |   0.125 ms |    0.117 ms |  1.00 |        1400 B |        1.90 |
| Wait_WorkReturningValueTask                       | 20                    | 506.52 ms |   2.923 ms |    2.734 ms |  1.01 |        8208 B |       11.15 |
| Wait_WorkReturningValueTaskWithThreadYield_Static | 20                    | 506.94 ms |   2.481 ms |    2.321 ms |  1.01 |        8000 B |       10.87 |
| Wait_WorkReturningValueTaskWithThreadYield        | 20                    | 507.47 ms |   2.053 ms |    1.920 ms |  1.01 |        8208 B |       11.15 |
