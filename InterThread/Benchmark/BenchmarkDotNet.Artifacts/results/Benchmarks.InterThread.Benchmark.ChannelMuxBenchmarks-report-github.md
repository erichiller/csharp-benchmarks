``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.302
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                                                             Method | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|------------------------------------------------------------------- |------------- |---------------------- |----------:|-----------:|------------:|----------:|----------:|----------:|--------------:|
|                                             LoopTryRead2_2Producer |       100000 |                 False |  11.91 ms |   0.205 ms |    0.220 ms | 1156.2500 |  921.8750 |  578.1250 |     6429463 B |
|                                               BroadcastChannelOnly |       100000 |                  True |  12.36 ms |   0.235 ms |    0.231 ms |  937.5000 |  578.1250 |  250.0000 |     5147916 B |
|                                               BroadcastChannelOnly |       100000 |                 False |  12.42 ms |   0.177 ms |    0.165 ms |  906.2500 |  500.0000 |  203.1250 |     4929678 B |
|                                             LoopTryRead2_2Producer |       100000 |                  True |  12.60 ms |   0.202 ms |    0.189 ms | 1218.7500 |  890.6250 |  546.8750 |     6665272 B |
|                                              LoopTryRead_2Producer |       100000 |                 False |  15.61 ms |   0.303 ms |    0.283 ms |  968.7500 |  937.5000 |  937.5000 |     8083897 B |
|                                              LoopTryRead_2Producer |       100000 |                  True |  16.17 ms |   0.312 ms |    0.457 ms | 1093.7500 |  937.5000 |  906.2500 |     8083861 B |
|                                        AsyncWaitLoopOnly_2Producer |       100000 |                  True |  17.53 ms |   0.345 ms |    0.526 ms | 1156.2500 |  843.7500 |  531.2500 |     6600646 B |
|                                        AsyncWaitLoopOnly_2Producer |       100000 |                 False |  17.76 ms |   0.340 ms |    0.454 ms | 1093.7500 |  875.0000 |  562.5000 |     6496561 B |
|                                             LoopTryRead2_3Producer |       100000 |                 False |  34.85 ms |   0.696 ms |    1.341 ms | 2285.7143 | 1500.0000 |  714.2857 |    11963434 B |
|                                             LoopTryRead2_3Producer |       100000 |                  True |  35.47 ms |   0.707 ms |    1.079 ms | 2642.8571 | 1428.5714 |  642.8571 |    14057475 B |
|                      LoopTryRead2_4Producer_4Tasks_4ReferenceTypes |       100000 |                 False |  45.84 ms |   0.912 ms |    2.185 ms | 4000.0000 | 2090.9091 |  545.4545 |    22905860 B |
|           LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes |       100000 |                  True |  46.39 ms |   0.911 ms |    1.547 ms | 3090.9091 | 2181.8182 |  636.3636 |    19109684 B |
|                      LoopTryRead2_4Producer_4Tasks_4ReferenceTypes |       100000 |                  True |  46.55 ms |   0.889 ms |    1.024 ms | 3909.0909 | 2454.5455 |  818.1818 |    22194829 B |
|           LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes |       100000 |                 False |  46.69 ms |   0.916 ms |    1.222 ms | 3181.8182 | 2363.6364 |  727.2727 |    19731084 B |
|                                      LoopTryRead2_8Producer_8Tasks |       100000 |                  True |  95.54 ms |   1.893 ms |    4.154 ms | 6333.3333 | 4500.0000 | 1500.0000 |    39573260 B |
|                                      LoopTryRead2_8Producer_8Tasks |       100000 |                 False |  97.05 ms |   1.935 ms |    3.539 ms | 6833.3333 | 4333.3333 | 1333.3333 |    41053573 B |
|            LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes |       100000 |                  True | 101.79 ms |   0.559 ms |    0.523 ms | 2400.0000 |         - |         - |    12011293 B |
|            LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes |       100000 |                 False | 104.08 ms |   0.539 ms |    0.478 ms | 2400.0000 |         - |         - |    12013760 B |
| LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync |       100000 |                  True | 111.60 ms |   0.537 ms |    0.502 ms | 5600.0000 |         - |         - |    27211381 B |
| LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync |       100000 |                 False | 112.70 ms |   0.885 ms |    0.828 ms | 5600.0000 |         - |         - |    27211381 B |
