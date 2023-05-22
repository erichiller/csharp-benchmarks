``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                                                                     Method | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|--------------------------------------------------------------------------- |----------:|-----------:|------------:|---------:|---------:|---------:|--------------:|
|                                          SingleProducerSingleConsumerQueue |  4.411 ms |  0.0766 ms |   0.0716 ms | 687.5000 |  78.1250 |        - |     3233036 B |
| Channel_SyncWrite_AsyncReadWait_SyncReadLoop_SingleProducer_SingleConsumer |  5.015 ms |  0.0922 ms |   0.0863 ms | 570.3125 | 546.8750 | 281.2500 |     4254436 B |
|                                                       ConcurrentQueueBased |  6.370 ms |  0.1245 ms |   0.1164 ms | 687.5000 | 125.0000 |        - |     3265626 B |
|                               Channel_SyncWrite_AsyncReadWait_SyncReadLoop |  8.046 ms |  0.0634 ms |   0.0562 ms | 890.6250 | 890.6250 | 500.0000 |     5301806 B |
|                                                         QueueWithLockBased |  9.952 ms |  0.1986 ms |   0.4442 ms | 640.6250 |  93.7500 |  46.8750 |     3409784 B |
|                                   Channel_SyncWrite_AsyncReadWait_SyncRead |  9.975 ms |  0.1399 ms |   0.1374 ms | 890.6250 | 890.6250 | 500.0000 |     5301947 B |
|                                                Channel_SyncWrite_AsyncRead | 10.662 ms |  0.0177 ms |   0.0165 ms | 890.6250 | 890.6250 | 500.0000 |     5301843 B |
|                                               Channel_AsyncWrite_AsyncRead | 11.634 ms |  0.0523 ms |   0.0489 ms | 890.6250 | 890.6250 | 500.0000 |     5301950 B |
|                                                 Channel_SyncWrite_SyncRead | 14.067 ms |  0.2458 ms |   0.2299 ms | 687.5000 |  78.1250 |        - |     3237339 B |
