# Channels Benchmarks

Note: Channels code moved to [`mkmrk.Channel`](https://github.com/erichiller/mkmrk.Channels)

<!-- TOC -->
* [Channels Benchmarks](#channels-benchmarks)
  * [Channels Testing & Development Benchmarks History](#channels-testing--development-benchmarks-history)
    * [`ChannelMux` backed by `ConcurrentQueue`](#channelmux-backed-by-concurrentqueue)
    * [`ChannelMux` backed by `SingleProducerSingleConsumerQueue`](#channelmux-backed-by-singleproducersingleconsumerqueue)
    * [lock replaced with `SpinLock`](#lock-replaced-with-spinlock)
    * [lock in `TryWrite` replaced with `Monitor.TryEnter`](#lock-in-trywrite-replaced-with-monitortryenter)
    * [lock in `TryWrite` replaced with `Monitor.TryEnter` + revised `Exception` code](#lock-in-trywrite-replaced-with-monitortryenter--revised-exception-code)
    * [lock in `TryWrite` replaced with `Monitor.TryEnter` + revised `Exception` code + volatile bool pre-check for `waitingReader`](#lock-in-trywrite-replaced-with-monitortryenter--revised-exception-code--volatile-bool-pre-check-for-waitingreader)
    * [w/ `if ( Interlocked.Increment( ref _parent._readableItems ) > 1 )` pre-check](#w-if--interlockedincrement-ref-parentreadableitems---1--pre-check)
    * [using singleton `AsyncOperation`](#using-singleton-asyncoperation)
    * [`TryWrite` with added initial `|| _parent._completeException is { }` check](#trywrite-with-added-initial--parentcompleteexception-is---check)
    * [using boolean exception check](#using-boolean-exception-check)
    * [`CancellationToken` use comparison ( for `WaitToReadAsync( CancellationToken )` + no `!cancellationToken.CanBeCanceled`](#cancellationtoken-use-comparison--for-waittoreadasync-cancellationtoken---no-cancellationtokencanbecanceled)
    * [`CancellationToken` use comparison ( for `WaitToReadAsync( CancellationToken )` + YES !`cancellationToken.CanBeCanceled`](#cancellationtoken-use-comparison--for-waittoreadasync-cancellationtoken---yes-cancellationtokencanbecanceled-)
    * [`completeException {}` check removed](#completeexception--check-removed)
    * [`ValueTask`.FromX](#valuetaskfromx)
    * [Single copy](#single-copy)
    * [Revert single copy](#revert-single-copy)
    * [Revert `ValueTask`.FromX](#revert-valuetaskfromx)
    * [With `[MethodImpl(MethodImplOptions.AggressiveInlining)]`](#with-methodimplmethodimploptionsaggressiveinlining)
    * [With Manual Value Task reset token.](#with-manual-value-task-reset-token)
    * [Reverted](#reverted)
    * [Final ( 2023-05-30 )](#final--2023-05-30-)
<!-- TOC -->

## Channels Testing & Development Benchmarks History


| Method     | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |    Gen1 | Allocated [B] |
|------------|----------:|-----------:|------------:|---------:|--------:|--------------:|
| ChannelMux |  39.35 ms |   0.754 ms |    0.869 ms | 846.1538 | 76.9231 |     4187471 B |


### `ChannelMux` backed by `ConcurrentQueue`

| Method                       | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |     Gen1 | Allocated [B] |
|------------------------------|----------:|-----------:|------------:|---------:|---------:|--------------:|
| ChannelMux_LoopTryRead       |  37.76 ms |   0.696 ms |    0.617 ms | 857.1429 | 214.2857 |     4201950 B |
| ChannelMux_AsyncWaitLoopOnly |  39.57 ms |   0.525 ms |    0.466 ms | 846.1538 |  76.9231 |     4132924 B |



### `ChannelMux` backed by `SingleProducerSingleConsumerQueue`

| Method                 | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------|----------:|-----------:|------------:|---------:|---------:|---------:|--------------:|
| BroadcastChannelOnly   |  12.11 ms |   0.208 ms |    0.264 ms | 921.8750 | 531.2500 | 203.1250 |     5008816 B |
| ChannelMux_LoopTryRead |  34.33 ms |   0.681 ms |    1.245 ms | 866.6667 | 133.3333 |  66.6667 |     4456274 B |
| ChannelMux             |  36.24 ms |   0.419 ms |    0.392 ms | 785.7143 |  71.4286 |        - |     4085486 B |



### lock replaced with `SpinLock`

| Method                 | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |    Gen1 | Allocated [B] |
|------------------------|----------:|-----------:|------------:|---------:|--------:|--------------:|
| ChannelMux_LoopTryRead |  47.16 ms |   0.926 ms |    1.716 ms | 818.1818 | 90.9091 |     4192649 B |



### lock in `TryWrite` replaced with `Monitor.TryEnter`

| Method                       | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------------|----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| ChannelMux_LoopTryRead       |  23.70 ms |   0.470 ms |    0.674 ms | 1187.5000 | 406.2500 | 312.5000 |     6701274 B |
| ChannelMux_AsyncWaitLoopOnly |  24.56 ms |   0.313 ms |    0.293 ms | 1687.5000 | 406.2500 | 187.5000 |     8332670 B |

| Method                       | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------------|----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| ChannelMux_LoopTryRead       |  23.37 ms |   0.218 ms |    0.204 ms | 1375.0000 | 468.7500 | 343.7500 |     6643177 B |
| ChannelMux_AsyncWaitLoopOnly |  24.97 ms |   0.401 ms |    0.376 ms | 1062.5000 |  62.5000 |        - |     5223778 B |



### lock in `TryWrite` replaced with `Monitor.TryEnter` + revised `Exception` code

| Method                       | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------------|----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| ChannelMux_LoopTryRead       |  23.55 ms |   0.464 ms |    0.476 ms | 1125.0000 | 406.2500 | 343.7500 |     6601251 B |
| ChannelMux_AsyncWaitLoopOnly |  24.77 ms |   0.247 ms |    0.219 ms | 1187.5000 | 156.2500 |        - |     5841614 B |



### lock in `TryWrite` replaced with `Monitor.TryEnter` + revised `Exception` code + volatile bool pre-check for `waitingReader`

| Method                       | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------------|----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| ChannelMux_LoopTryRead       |  15.96 ms |   0.308 ms |    0.342 ms | 1125.0000 | 937.5000 | 906.2500 |     8112448 B |
| ChannelMux_AsyncWaitLoopOnly |  26.04 ms |   0.535 ms |    1.569 ms | 1593.7500 | 500.0000 | 375.0000 |     8718857 B |


| Method                       | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------------|----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| ChannelMux_LoopTryRead       |  17.52 ms |   0.348 ms |    0.572 ms | 1343.7500 | 812.5000 | 812.5000 |     8508873 B |
| ChannelMux_AsyncWaitLoopOnly |  27.44 ms |   0.542 ms |    1.339 ms | 1718.7500 | 625.0000 | 343.7500 |     8855990 B |



### w/ `if ( Interlocked.Increment( ref _parent._readableItems ) > 1 )` pre-check

| Method                       | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------------|----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| ChannelMux_LoopTryRead       |  22.63 ms |   0.530 ms |    1.562 ms | 1843.7500 | 468.7500 | 343.7500 |     9014399 B |
| ChannelMux_AsyncWaitLoopOnly |  26.92 ms |   0.533 ms |    1.555 ms | 1250.0000 | 687.5000 | 437.5000 |     6799739 B |


### using singleton `AsyncOperation`

| Method                       | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------------|----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| ChannelMux_LoopTryRead       |  13.96 ms |   0.231 ms |    0.216 ms | 1015.6250 | 968.7500 | 968.7500 |     8268526 B |
| ChannelMux_AsyncWaitLoopOnly |  20.85 ms |   0.413 ms |    1.081 ms | 1125.0000 | 781.2500 | 625.0000 |     7056407 B |

| Method                       | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------------|----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| ChannelMux_LoopTryRead_2     |  11.27 ms |   0.222 ms |    0.272 ms | 1109.3750 | 781.2500 | 500.0000 |     6044332 B |
| ChannelMux_LoopTryRead       |  13.95 ms |   0.178 ms |    0.166 ms | 1046.8750 | 968.7500 | 968.7500 |     8227056 B |
| ChannelMux_AsyncWaitLoopOnly |  21.47 ms |   0.426 ms |    1.243 ms | 1343.7500 | 812.5000 | 437.5000 |     7189697 B |


| Method                                                                        | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|-------------------------------------------------------------------------------|--------------|----------:|-----------:|------------:|----------:|----------:|----------:|--------------:|
| ChannelMux_LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes           | 100000       |  42.20 ms |   0.842 ms |    1.899 ms | 3750.0000 | 2083.3333 |  583.3333 |    22639463 B |
| ChannelMux_LoopTryRead2_4Producer_4Tasks_4ReferenceTypes                      | 100000       |  42.23 ms |   0.821 ms |    1.966 ms | 4000.0000 | 2500.0000 |  750.0000 |    22910485 B |
| ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes            | 100000       |  99.11 ms |   1.141 ms |    0.891 ms | 2500.0000 |         - |         - |    12011148 B |
| ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync | 100000       | 108.61 ms |   0.073 ms |    0.061 ms | 5600.0000 |         - |         - |    27211277 B |
| ChannelMux_LoopTryRead2_8Producer_8Tasks                                      | 100000       |  88.84 ms |   1.749 ms |    3.765 ms | 6500.0000 | 4166.6667 | 1333.3333 |    39397537 B |


### `TryWrite` with added initial `|| _parent._completeException is { }` check

| Method                                                                        | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|-------------------------------------------------------------------------------|--------------|----------:|-----------:|------------:|----------:|----------:|----------:|--------------:|
| ChannelMux_LoopTryRead2_2Producer                                             | 100000       |  13.65 ms |   0.271 ms |    0.333 ms |  968.7500 |  406.2500 |  171.8750 |     5175566 B |
| ChannelMux_LoopTryRead                                                        | 100000       |  15.51 ms |   0.206 ms |    0.193 ms | 1046.8750 |  968.7500 |  968.7500 |     8202302 B |
| ChannelMux_AsyncWaitLoopOnly_2Producer                                        | 100000       |  20.43 ms |   0.454 ms |    1.324 ms | 1406.2500 |  843.7500 |  437.5000 |     7483242 B |
| ChannelMux_LoopTryRead2_3Producer                                             | 100000       |  33.25 ms |   0.663 ms |    1.869 ms | 3187.5000 | 1000.0000 |  312.5000 |    15981770 B |
| ChannelMux_LoopTryRead2_4Producer_4Tasks_4ReferenceTypes                      | 100000       |  42.79 ms |   0.850 ms |    2.102 ms | 4166.6667 | 2083.3333 |  583.3333 |    22975547 B |
| ChannelMux_LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes           | 100000       |  45.07 ms |   0.854 ms |    0.913 ms | 3307.6923 | 2153.8462 |  538.4615 |    20049183 B |
| ChannelMux_LoopTryRead2_8Producer_8Tasks                                      | 100000       |  94.29 ms |   1.852 ms |    3.698 ms | 6666.6667 | 4500.0000 | 1333.3333 |    40123284 B |
| ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes            | 100000       |  98.03 ms |   0.894 ms |    0.836 ms | 2500.0000 |         - |         - |    12011204 B |
| ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync | 100000       | 107.50 ms |   0.299 ms |    0.265 ms | 5600.0000 |         - |         - |    27211333 B |


### using boolean exception check

| Method                                                                        | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|-------------------------------------------------------------------------------|--------------|----------:|-----------:|------------:|----------:|----------:|----------:|--------------:|
| ChannelMux_LoopTryRead2_2Producer                                             | 100000       |  11.27 ms |   0.224 ms |    0.349 ms | 1062.5000 |  765.6250 |  484.3750 |     6092240 B |
| ChannelMux_LoopTryRead                                                        | 100000       |  14.79 ms |   0.286 ms |    0.281 ms | 1046.8750 |  968.7500 |  968.7500 |     8191449 B |
| ChannelMux_AsyncWaitLoopOnly_2Producer                                        | 100000       |  20.00 ms |   0.398 ms |    1.150 ms | 1500.0000 |  687.5000 |  312.5000 |     7733617 B |
| ChannelMux_LoopTryRead2_3Producer                                             | 100000       |  31.05 ms |   0.631 ms |    1.861 ms | 3062.5000 | 1062.5000 |  312.5000 |    16802876 B |
| ChannelMux_LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes           | 100000       |  42.36 ms |   0.843 ms |    1.903 ms | 3250.0000 | 2333.3333 |  750.0000 |    20347538 B |
| ChannelMux_LoopTryRead2_4Producer_4Tasks_4ReferenceTypes                      | 100000       |  43.00 ms |   0.855 ms |    1.841 ms | 3846.1538 | 2230.7692 |  769.2308 |    21816030 B |
| ChannelMux_LoopTryRead2_8Producer_8Tasks                                      | 100000       |  86.39 ms |   1.705 ms |    3.633 ms | 6500.0000 | 4666.6667 | 1666.6667 |    39577919 B |
| ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes            | 100000       |  98.87 ms |   0.874 ms |    0.818 ms | 2500.0000 |         - |         - |    12011204 B |
| ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync | 100000       | 107.61 ms |   0.271 ms |    0.240 ms | 5600.0000 |         - |         - |    27211333 B |

| Method                                                                        | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] | Median [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|-------------------------------------------------------------------------------|--------------|----------:|-----------:|------------:|------------:|----------:|----------:|----------:|--------------:|
| ChannelMux_LoopTryRead2_2Producer                                             | 100000       |  11.99 ms |   0.367 ms |    1.066 ms |    11.60 ms | 1156.2500 |  937.5000 |  531.2500 |     6237784 B |
| ChannelMux_LoopTryRead                                                        | 100000       |  14.74 ms |   0.276 ms |    0.245 ms |    14.72 ms | 1187.5000 |  921.8750 |  890.6250 |     8227336 B |
| ChannelMux_AsyncWaitLoopOnly_2Producer                                        | 100000       |  19.96 ms |   0.397 ms |    1.046 ms |    19.88 ms | 1156.2500 |  718.7500 |  468.7500 |     6973142 B |
| ChannelMux_LoopTryRead2_3Producer                                             | 100000       |  30.15 ms |   0.600 ms |    1.672 ms |    30.19 ms | 2968.7500 | 1281.2500 |  406.2500 |    16182473 B |
| ChannelMux_LoopTryRead2_4Producer_4Tasks_4ReferenceTypes                      | 100000       |  42.82 ms |   0.850 ms |    2.480 ms |    43.04 ms | 3545.4545 | 2363.6364 |  636.3636 |    21435335 B |
| ChannelMux_LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes           | 100000       |  42.89 ms |   0.828 ms |    1.534 ms |    43.04 ms | 3538.4615 | 2307.6923 |  692.3077 |    20610846 B |
| ChannelMux_LoopTryRead2_8Producer_8Tasks                                      | 100000       |  88.07 ms |   1.761 ms |    3.598 ms |    88.23 ms | 7000.0000 | 4000.0000 | 1333.3333 |    41136219 B |
| ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes            | 100000       |  99.02 ms |   0.904 ms |    0.846 ms |    99.17 ms | 2500.0000 |         - |         - |    12011204 B |
| ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync | 100000       | 107.24 ms |   0.309 ms |    0.258 ms |   107.26 ms | 5600.0000 |         - |         - |    27211333 B |


###u sing boolean completion / closed check in `TryRead` and `TryComplete`

| Method                                                             | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|--------------------------------------------------------------------|--------------|----------:|-----------:|------------:|----------:|----------:|----------:|--------------:|
| BroadcastChannelOnly                                               | 100000       |  12.47 ms |   0.242 ms |    0.227 ms |  890.6250 |  421.8750 |  203.1250 |     4964095 B |
| LoopTryRead2_2Producer                                             | 100000       |  12.67 ms |   0.249 ms |    0.380 ms | 1218.7500 |  921.8750 |  562.5000 |     6582722 B |
| LoopTryRead_2Producer                                              | 100000       |  16.19 ms |   0.320 ms |    0.427 ms | 1093.7500 |  937.5000 |  937.5000 |     8158376 B |
| AsyncWaitLoopOnly_2Producer                                        | 100000       |  21.47 ms |   0.428 ms |    1.034 ms | 1500.0000 |  906.2500 |  625.0000 |     8434774 B |
| LoopTryRead2_3Producer                                             | 100000       |  35.00 ms |   0.693 ms |    1.521 ms | 2437.5000 | 1062.5000 |  500.0000 |    13529446 B |
| LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes           | 100000       |  47.06 ms |   0.939 ms |    1.786 ms | 3909.0909 | 2090.9091 |  636.3636 |    22122820 B |
| LoopTryRead2_4Producer_4Tasks_4ReferenceTypes                      | 100000       |  48.13 ms |   0.958 ms |    1.777 ms | 4363.6364 | 2181.8182 |  727.2727 |    23332613 B |
| LoopTryRead2_8Producer_8Tasks                                      | 100000       |  96.80 ms |   1.931 ms |    4.030 ms | 6600.0000 | 3600.0000 | 1400.0000 |    41053501 B |
| LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes            | 100000       | 102.98 ms |   0.541 ms |    0.423 ms | 2400.0000 |         - |         - |    12011277 B |
| LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync | 100000       | 114.41 ms |   1.706 ms |    1.596 ms | 5600.0000 |         - |         - |    27211365 B |

| Method                                                             | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|--------------------------------------------------------------------|--------------|----------:|-----------:|------------:|----------:|----------:|----------:|--------------:|
| BroadcastChannelOnly                                               | 100000       |  12.35 ms |   0.209 ms |    0.196 ms |  843.7500 |  406.2500 |  125.0000 |     4693254 B |
| LoopTryRead2_2Producer                                             | 100000       |  12.78 ms |   0.239 ms |    0.256 ms | 1265.6250 | 1078.1250 |  578.1250 |     6665754 B |
| LoopTryRead_2Producer                                              | 100000       |  16.32 ms |   0.307 ms |    0.287 ms | 1343.7500 |  812.5000 |  812.5000 |     8346454 B |
| AsyncWaitLoopOnly_2Producer                                        | 100000       |  21.22 ms |   0.419 ms |    1.019 ms | 1437.5000 |  656.2500 |  406.2500 |     7549334 B |
| LoopTryRead2_3Producer                                             | 100000       |  35.76 ms |   0.701 ms |    1.171 ms | 2375.0000 |  937.5000 |  437.5000 |    11790162 B |
| LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes           | 100000       |  47.81 ms |   0.922 ms |    1.293 ms | 3545.4545 | 2363.6364 | 1000.0000 |    20018297 B |
| LoopTryRead2_4Producer_4Tasks_4ReferenceTypes                      | 100000       |  48.43 ms |   0.962 ms |    1.250 ms | 3545.4545 | 2454.5455 |  636.3636 |    20069187 B |
| LoopTryRead2_8Producer_8Tasks                                      | 100000       |  94.87 ms |   1.866 ms |    4.213 ms | 6333.3333 | 4500.0000 | 1333.3333 |    39677557 B |
| LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes            | 100000       | 100.78 ms |   0.243 ms |    0.215 ms | 2400.0000 |         - |         - |    12011277 B |
| LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync | 100000       | 112.59 ms |   0.756 ms |    0.707 ms | 5600.0000 |         - |         - |    27211365 B |




### `CancellationToken` use comparison ( for `WaitToReadAsync( CancellationToken )` + no `!cancellationToken.CanBeCanceled`
check in WaitToReadAsync

| Method                 | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] | Median [ms] |      Gen0 |      Gen1 |     Gen2 | Allocated [B] |
|------------------------|--------------|-----------------------|----------:|-----------:|------------:|------------:|----------:|----------:|---------:|--------------:|
| LoopTryRead2_2Producer | 100000       | False                 |  13.43 ms |   0.523 ms |    1.535 ms |    12.57 ms | 1265.6250 | 1031.2500 | 562.5000 |     6664571 B |
| LoopTryRead2_2Producer | 100000       | True                  |  14.10 ms |   0.566 ms |    1.660 ms |    13.51 ms | 1062.5000 |  625.0000 | 453.1250 |     6066909 B |


### `CancellationToken` use comparison ( for `WaitToReadAsync( CancellationToken )` + YES !`cancellationToken.CanBeCanceled` 
check in WaitToReadAsync

| Method                 | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] | Median [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------|--------------|-----------------------|----------:|-----------:|------------:|------------:|----------:|---------:|---------:|--------------:|
| LoopTryRead2_2Producer | 100000       | True                  |  12.77 ms |   0.254 ms |    0.458 ms |    12.67 ms | 1140.6250 | 859.3750 | 468.7500 |     6407654 B |
| LoopTryRead2_2Producer | 100000       | False                 |  12.93 ms |   0.283 ms |    0.797 ms |    12.63 ms | 1203.1250 | 968.7500 | 546.8750 |     6421922 B |


### `completeException {}` check removed

|                 Method | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------- |------------- |---------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| LoopTryRead2_2Producer |       100000 |                 False |  12.24 ms |   0.177 ms |    0.157 ms | 1156.2500 | 937.5000 | 562.5000 |     6436744 B |
| LoopTryRead2_2Producer |       100000 |                  True |  12.97 ms |   0.258 ms |    0.458 ms | 1140.6250 | 578.1250 | 437.5000 |     6396460 B |

### `ValueTask`.FromX

| Method                 | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------|--------------|-----------------------|----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| LoopTryRead2_2Producer | 100000       | False                 |  12.41 ms |   0.220 ms |    0.184 ms | 1109.3750 | 875.0000 | 562.5000 |     6271371 B |
| LoopTryRead2_2Producer | 100000       | True                  |  13.18 ms |   0.262 ms |    0.714 ms | 1093.7500 | 812.5000 | 437.5000 |     6667105 B |

### Single copy

| Method                 | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------|--------------|-----------------------|----------:|-----------:|------------:|---------:|---------:|---------:|--------------:|
| LoopTryRead2_2Producer | 100000       | False                 |  14.84 ms |   0.293 ms |    0.643 ms | 703.1250 | 515.6250 | 468.7500 |     3798187 B |
| LoopTryRead2_2Producer | 100000       | True                  |  15.89 ms |   0.315 ms |    0.824 ms | 906.2500 | 343.7500 | 312.5000 |     5223618 B |

### Revert single copy

| Method                 | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------|--------------|-----------------------|----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| LoopTryRead2_2Producer | 100000       | False                 |  12.50 ms |   0.244 ms |    0.476 ms | 1171.8750 | 937.5000 | 546.8750 |     6493993 B |
| LoopTryRead2_2Producer | 100000       | True                  |  12.93 ms |   0.245 ms |    0.449 ms | 1171.8750 | 875.0000 | 500.0000 |     6415734 B |

### Revert `ValueTask`.FromX

| Method                 | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------|--------------|-----------------------|----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| LoopTryRead2_2Producer | 100000       | False                 |  12.35 ms |   0.219 ms |    0.171 ms | 1265.6250 | 890.6250 | 531.2500 |     7051939 B |
| LoopTryRead2_2Producer | 100000       | True                  |  12.96 ms |   0.258 ms |    0.491 ms | 1062.5000 | 796.8750 | 515.6250 |     6275981 B |

### With `[MethodImpl(MethodImplOptions.AggressiveInlining)]`

| Method                 | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|------------------------|--------------|-----------------------|----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
| LoopTryRead2_2Producer | 100000       | False                 |  12.31 ms |   0.190 ms |    0.169 ms | 1171.8750 | 843.7500 | 546.8750 |     6321049 B |
| LoopTryRead2_2Producer | 100000       | True                  |  12.73 ms |   0.252 ms |    0.520 ms | 1125.0000 | 921.8750 | 500.0000 |     6163336 B |

### With Manual Value Task reset token.

| Method                 | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |     Gen1 | Allocated [B] |
|------------------------|--------------|-----------------------|----------:|-----------:|------------:|---------:|---------:|--------------:|
| LoopTryRead2_2Producer | 100000       | False                 |  23.76 ms |   0.395 ms |    0.369 ms | 875.0000 | 156.2500 |     4205528 B |
| LoopTryRead2_2Producer | 100000       | True                  |  23.77 ms |   0.291 ms |    0.272 ms | 875.0000 |  31.2500 |     4131396 B |



### Reverted

| Method                 | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |     Gen2 | Allocated [B] |
|------------------------|--------------|-----------------------|----------:|-----------:|------------:|----------:|----------:|---------:|--------------:|
| LoopTryRead2_2Producer | 100000       | False                 |  12.28 ms |   0.198 ms |    0.220 ms | 1265.6250 | 1000.0000 | 546.8750 |     6518853 B |
| LoopTryRead2_2Producer | 100000       | True                  |  12.94 ms |   0.255 ms |    0.602 ms | 1125.0000 |  843.7500 | 546.8750 |     6250709 B |

### Final ( 2023-05-30 )

| Method                                                             | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|--------------------------------------------------------------------|--------------|-----------------------|----------:|-----------:|------------:|----------:|----------:|----------:|--------------:|
| LoopTryRead2_2Producer                                             | 100000       | False                 |  11.91 ms |   0.205 ms |    0.220 ms | 1156.2500 |  921.8750 |  578.1250 |     6429463 B |
| BroadcastChannelOnly                                               | 100000       | True                  |  12.36 ms |   0.235 ms |    0.231 ms |  937.5000 |  578.1250 |  250.0000 |     5147916 B |
| BroadcastChannelOnly                                               | 100000       | False                 |  12.42 ms |   0.177 ms |    0.165 ms |  906.2500 |  500.0000 |  203.1250 |     4929678 B |
| LoopTryRead2_2Producer                                             | 100000       | True                  |  12.60 ms |   0.202 ms |    0.189 ms | 1218.7500 |  890.6250 |  546.8750 |     6665272 B |
| LoopTryRead_2Producer                                              | 100000       | False                 |  15.61 ms |   0.303 ms |    0.283 ms |  968.7500 |  937.5000 |  937.5000 |     8083897 B |
| LoopTryRead_2Producer                                              | 100000       | True                  |  16.17 ms |   0.312 ms |    0.457 ms | 1093.7500 |  937.5000 |  906.2500 |     8083861 B |
| AsyncWaitLoopOnly_2Producer                                        | 100000       | True                  |  17.53 ms |   0.345 ms |    0.526 ms | 1156.2500 |  843.7500 |  531.2500 |     6600646 B |
| AsyncWaitLoopOnly_2Producer                                        | 100000       | False                 |  17.76 ms |   0.340 ms |    0.454 ms | 1093.7500 |  875.0000 |  562.5000 |     6496561 B |
| LoopTryRead2_3Producer                                             | 100000       | False                 |  34.85 ms |   0.696 ms |    1.341 ms | 2285.7143 | 1500.0000 |  714.2857 |    11963434 B |
| LoopTryRead2_3Producer                                             | 100000       | True                  |  35.47 ms |   0.707 ms |    1.079 ms | 2642.8571 | 1428.5714 |  642.8571 |    14057475 B |
| LoopTryRead2_4Producer_4Tasks_4ReferenceTypes                      | 100000       | False                 |  45.84 ms |   0.912 ms |    2.185 ms | 4000.0000 | 2090.9091 |  545.4545 |    22905860 B |
| LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes           | 100000       | True                  |  46.39 ms |   0.911 ms |    1.547 ms | 3090.9091 | 2181.8182 |  636.3636 |    19109684 B |
| LoopTryRead2_4Producer_4Tasks_4ReferenceTypes                      | 100000       | True                  |  46.55 ms |   0.889 ms |    1.024 ms | 3909.0909 | 2454.5455 |  818.1818 |    22194829 B |
| LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes           | 100000       | False                 |  46.69 ms |   0.916 ms |    1.222 ms | 3181.8182 | 2363.6364 |  727.2727 |    19731084 B |
| LoopTryRead2_8Producer_8Tasks                                      | 100000       | True                  |  95.54 ms |   1.893 ms |    4.154 ms | 6333.3333 | 4500.0000 | 1500.0000 |    39573260 B |
| LoopTryRead2_8Producer_8Tasks                                      | 100000       | False                 |  97.05 ms |   1.935 ms |    3.539 ms | 6833.3333 | 4333.3333 | 1333.3333 |    41053573 B |
| LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes            | 100000       | True                  | 101.79 ms |   0.559 ms |    0.523 ms | 2400.0000 |         - |         - |    12011293 B |
| LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes            | 100000       | False                 | 104.08 ms |   0.539 ms |    0.478 ms | 2400.0000 |         - |         - |    12013760 B |
| LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync | 100000       | True                  | 111.60 ms |   0.537 ms |    0.502 ms | 5600.0000 |         - |         - |    27211381 B |
| LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync | 100000       | False                 | 112.70 ms |   0.885 ms |    0.828 ms | 5600.0000 |         - |         - |    27211381 B |



