``` ini

BenchmarkDotNet=v0.13.1, OS=ubuntu 21.10
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.300
  [Host]     : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT
  Job-DCWPAL : .NET 6.0.5 (6.0.522.21309), X64 RyuJIT

InvocationCount=1  UnrollFactor=1  

```
|                                                                Method | MessageCount |  Mean [ms] | Error [ms] | StdDev [ms] |      Gen 0 |     Gen 1 | Allocated [B] |
|---------------------------------------------------------------------- |------------- |-----------:|-----------:|------------:|-----------:|----------:|--------------:|
|    BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_WriteEnumerable |      2000000 |   192.6 ms |    3.70 ms |     9.43 ms | 21000.0000 | 1000.0000 | 105,054,976 B |
|                    BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber |      2000000 |   202.0 ms |   12.26 ms |    35.95 ms | 13000.0000 |         - |  64,070,072 B |
|   BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_WriteEnumerable |      2000000 |   618.5 ms |   12.07 ms |    31.80 ms | 22000.0000 |         - | 104,072,280 B |
|                   BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers |      2000000 |   653.1 ms |   21.02 ms |    61.98 ms | 13000.0000 | 1000.0000 |  64,270,608 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_WriteEnumerable |      2000000 | 1,047.9 ms |   20.57 ms |    28.84 ms | 22000.0000 | 1000.0000 | 104,437,552 B |
|                 BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers |      2000000 | 1,243.2 ms |   24.71 ms |    59.69 ms | 13000.0000 | 1000.0000 |  64,223,144 B |
