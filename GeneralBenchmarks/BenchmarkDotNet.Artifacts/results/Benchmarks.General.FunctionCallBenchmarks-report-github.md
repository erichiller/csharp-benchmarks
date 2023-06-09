``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.302
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  Job-VKUVUP : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
|                                                                             Method | Mean [ms] | Error [ms] | StdDev [ms] | Ratio | RatioSD |        Gen0 | Allocated [B] | Alloc Ratio |
|----------------------------------------------------------------------------------- |----------:|-----------:|------------:|------:|--------:|------------:|--------------:|------------:|
|                                               Direct_Sync_NoParams_ReferenceReturn |  130.8 ms |    0.89 ms |     0.84 ms |  1.00 |    0.00 | 108000.0000 |   512000936 B |        1.00 |
|                                         Intermediate_Sync_NoParams_ReferenceReturn |  133.6 ms |    1.51 ms |     1.41 ms |  1.02 |    0.01 | 108000.0000 |   512000936 B |        1.00 |
|                                Nullable_Intermediate_Sync_NoParams_ReferenceReturn |  139.6 ms |    2.38 ms |     2.23 ms |  1.07 |    0.02 | 108000.0000 |   512000960 B |        1.00 |
|                        Nullable_NotNull_Intermediate_Sync_NoParams_ReferenceReturn |  139.6 ms |    1.36 ms |     1.27 ms |  1.07 |    0.01 | 108000.0000 |   512000936 B |        1.00 |
|                                    Intermediate_Sync_WithEndParams_ReferenceReturn |  259.4 ms |    3.37 ms |     3.15 ms |  1.98 |    0.03 | 217000.0000 |  1024000936 B |        2.00 |
|                                             Direct_Sync_WithParams_ReferenceReturn |  262.6 ms |    2.01 ms |     1.88 ms |  2.01 |    0.02 | 217000.0000 |  1024000936 B |        2.00 |
|                           Nullable_Intermediate_Sync_WithEndParams_ReferenceReturn |  272.0 ms |    2.62 ms |     2.45 ms |  2.08 |    0.02 | 217000.0000 |  1024000960 B |        2.00 |
|                   Nullable_NotNull_Intermediate_Sync_WithEndParams_ReferenceReturn |  279.7 ms |    2.12 ms |     1.88 ms |  2.14 |    0.02 | 217000.0000 |  1024000936 B |        2.00 |
|                                                                                    |           |            |             |       |         |             |               |             |
|                  Nullable_NotNull_Intermediate_Async_Task_NoParams_ReferenceReturn |  262.1 ms |    1.03 ms |     0.91 ms |  0.99 |    0.02 | 261000.0000 |  1232001008 B |        1.00 |
|                                   Intermediate_Async_Task_NoParams_ReferenceReturn |  268.3 ms |    5.25 ms |     9.46 ms |  1.00 |    0.05 | 261000.0000 |  1232001008 B |        1.00 |
|                                         Direct_Async_Task_NoParams_ReferenceReturn |  269.5 ms |    5.31 ms |     7.61 ms |  1.00 |    0.00 | 261000.0000 |  1232001008 B |        1.00 |
|                          Nullable_Intermediate_Async_Task_NoParams_ReferenceReturn |  275.3 ms |    4.96 ms |     4.64 ms |  1.03 |    0.04 | 261000.0000 |  1232001032 B |        1.00 |
|                                       Direct_Async_Task_WithParams_ReferenceReturn |  510.1 ms |   10.19 ms |    10.47 ms |  1.91 |    0.06 | 523000.0000 |  2464001008 B |        2.00 |
|                              Intermediate_Async_Task_WithEndParams_ReferenceReturn |  510.5 ms |    8.88 ms |    10.91 ms |  1.90 |    0.08 | 523000.0000 |  2464001008 B |        2.00 |
|                     Nullable_Intermediate_Async_Task_WithEndParams_ReferenceReturn |  525.4 ms |   10.07 ms |    13.10 ms |  1.95 |    0.09 | 523000.0000 |  2464001032 B |        2.00 |
|             Nullable_NotNull_Intermediate_Async_Task_WithEndParams_ReferenceReturn |  533.5 ms |   10.65 ms |     9.44 ms |  2.02 |    0.06 | 523000.0000 |  2464001008 B |        2.00 |
|                                                                                    |           |            |             |       |         |             |               |             |
|                                    Direct_Async_ValueTask_NoParams_ReferenceReturn |  206.0 ms |    1.73 ms |     1.35 ms |  1.00 |    0.00 | 108000.0000 |   512000936 B |        1.00 |
|             Nullable_NotNull_Intermediate_Async_ValueTask_NoParams_ReferenceReturn |  207.0 ms |    0.81 ms |     0.72 ms |  1.00 |    0.01 | 108000.0000 |   512000936 B |        1.00 |
|      Nullable_NotNull_Intermediate_Inline_Async_ValueTask_NoParams_ReferenceReturn |  211.5 ms |    3.64 ms |     3.41 ms |  1.03 |    0.02 | 108000.0000 |   512000936 B |        1.00 |
|                              Intermediate_Async_ValueTask_NoParams_ReferenceReturn |  212.3 ms |    2.87 ms |     2.69 ms |  1.03 |    0.01 | 108000.0000 |   512000936 B |        1.00 |
|                     Nullable_Intermediate_Async_ValueTask_NoParams_ReferenceReturn |  212.9 ms |    3.54 ms |     3.14 ms |  1.04 |    0.02 | 108000.0000 |   512000960 B |        1.00 |
|                         Intermediate_Async_ValueTask_WithEndParams_ReferenceReturn |  372.1 ms |    4.47 ms |     4.18 ms |  1.81 |    0.02 | 217000.0000 |  1024000936 B |        2.00 |
|                                  Direct_Async_ValueTask_WithParams_ReferenceReturn |  373.2 ms |    5.30 ms |     4.69 ms |  1.81 |    0.03 | 217000.0000 |  1024000936 B |        2.00 |
|                Nullable_Intermediate_Async_ValueTask_WithEndParams_ReferenceReturn |  376.4 ms |    6.50 ms |     6.08 ms |  1.83 |    0.02 | 217000.0000 |  1024000960 B |        2.00 |
|        Nullable_NotNull_Intermediate_Async_ValueTask_WithEndParams_ReferenceReturn |  377.7 ms |    6.86 ms |     6.42 ms |  1.83 |    0.04 | 217000.0000 |  1024000936 B |        2.00 |
| Nullable_NotNull_Intermediate_Inline_Async_ValueTask_WithEndParams_ReferenceReturn |  431.3 ms |    8.57 ms |     8.02 ms |  2.10 |    0.05 | 217000.0000 |  1024000936 B |        2.00 |
