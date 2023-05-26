using System;
using System.Threading;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using BroadcastChannel;

using BroadcastChannelMux;

namespace Benchmarks.InterThread.Benchmark;

public class UnexpectedCountsException : Exception {
    public UnexpectedCountsException( string message ) : base( message ) {
        Console.WriteLine( "EXCEPTION!!!" );
    }
}

/*
 * 
|     Method | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |    Gen1 | Allocated [B] |
|----------- |----------:|-----------:|------------:|---------:|--------:|--------------:|
| ChannelMux |  39.35 ms |   0.754 ms |    0.869 ms | 846.1538 | 76.9231 |     4187471 B |


 ChannelMux backed by ConcurrentQueue
======================================
|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |     Gen1 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|---------:|---------:|--------------:|
|       ChannelMux_LoopTryRead |  37.76 ms |   0.696 ms |    0.617 ms | 857.1429 | 214.2857 |     4201950 B |
| ChannelMux_AsyncWaitLoopOnly |  39.57 ms |   0.525 ms |    0.466 ms | 846.1538 |  76.9231 |     4132924 B |



 ChannelMux backed by SingleProducerSingleConsumerQueue
========================================================
|                 Method | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------- |----------:|-----------:|------------:|---------:|---------:|---------:|--------------:|
|   BroadcastChannelOnly |  12.11 ms |   0.208 ms |    0.264 ms | 921.8750 | 531.2500 | 203.1250 |     5008816 B |
| ChannelMux_LoopTryRead |  34.33 ms |   0.681 ms |    1.245 ms | 866.6667 | 133.3333 |  66.6667 |     4456274 B |
|             ChannelMux |  36.24 ms |   0.419 ms |    0.392 ms | 785.7143 |  71.4286 |        - |     4085486 B |



lock replaced with SpinLock
======================================
|                 Method | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |    Gen1 | Allocated [B] |
|----------------------- |----------:|-----------:|------------:|---------:|--------:|--------------:|
| ChannelMux_LoopTryRead |  47.16 ms |   0.926 ms |    1.716 ms | 818.1818 | 90.9091 |     4192649 B |



lock in TryWrite replaced with Monitor.TryEnter
================================================
|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|       ChannelMux_LoopTryRead |  23.70 ms |   0.470 ms |    0.674 ms | 1187.5000 | 406.2500 | 312.5000 |     6701274 B |
| ChannelMux_AsyncWaitLoopOnly |  24.56 ms |   0.313 ms |    0.293 ms | 1687.5000 | 406.2500 | 187.5000 |     8332670 B |

|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|       ChannelMux_LoopTryRead |  23.37 ms |   0.218 ms |    0.204 ms | 1375.0000 | 468.7500 | 343.7500 |     6643177 B |
| ChannelMux_AsyncWaitLoopOnly |  24.97 ms |   0.401 ms |    0.376 ms | 1062.5000 |  62.5000 |        - |     5223778 B |



lock in TryWrite replaced with Monitor.TryEnter + revised Exception code
==========================================================================
|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|       ChannelMux_LoopTryRead |  23.55 ms |   0.464 ms |    0.476 ms | 1125.0000 | 406.2500 | 343.7500 |     6601251 B |
| ChannelMux_AsyncWaitLoopOnly |  24.77 ms |   0.247 ms |    0.219 ms | 1187.5000 | 156.2500 |        - |     5841614 B |



lock in TryWrite replaced with Monitor.TryEnter + revised Exception code + volatile bool pre-check for waitingReader
====================================================================================================================
|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|       ChannelMux_LoopTryRead |  15.96 ms |   0.308 ms |    0.342 ms | 1125.0000 | 937.5000 | 906.2500 |     8112448 B |
| ChannelMux_AsyncWaitLoopOnly |  26.04 ms |   0.535 ms |    1.569 ms | 1593.7500 | 500.0000 | 375.0000 |     8718857 B |


|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|       ChannelMux_LoopTryRead |  17.52 ms |   0.348 ms |    0.572 ms | 1343.7500 | 812.5000 | 812.5000 |     8508873 B |
| ChannelMux_AsyncWaitLoopOnly |  27.44 ms |   0.542 ms |    1.339 ms | 1718.7500 | 625.0000 | 343.7500 |     8855990 B |



w/ if ( Interlocked.Increment( ref _parent._readableItems ) > 1 ) ;; pre-check
==============================================================================
|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|       ChannelMux_LoopTryRead |  22.63 ms |   0.530 ms |    1.562 ms | 1843.7500 | 468.7500 | 343.7500 |     9014399 B |
| ChannelMux_AsyncWaitLoopOnly |  26.92 ms |   0.533 ms |    1.555 ms | 1250.0000 | 687.5000 | 437.5000 |     6799739 B |


using singleton AsyncOperation
==================
|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|       ChannelMux_LoopTryRead |  13.96 ms |   0.231 ms |    0.216 ms | 1015.6250 | 968.7500 | 968.7500 |     8268526 B |
| ChannelMux_AsyncWaitLoopOnly |  20.85 ms |   0.413 ms |    1.081 ms | 1125.0000 | 781.2500 | 625.0000 |     7056407 B |

|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|     ChannelMux_LoopTryRead_2 |  11.27 ms |   0.222 ms |    0.272 ms | 1109.3750 | 781.2500 | 500.0000 |     6044332 B |
|       ChannelMux_LoopTryRead |  13.95 ms |   0.178 ms |    0.166 ms | 1046.8750 | 968.7500 | 968.7500 |     8227056 B |
| ChannelMux_AsyncWaitLoopOnly |  21.47 ms |   0.426 ms |    1.243 ms | 1343.7500 | 812.5000 | 437.5000 |     7189697 B |


|                                                                        Method | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|------------------------------------------------------------------------------ |------------- |----------:|-----------:|------------:|----------:|----------:|----------:|--------------:|
|           ChannelMux_LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes |       100000 |  42.20 ms |   0.842 ms |    1.899 ms | 3750.0000 | 2083.3333 | 583.3333  |    22639463 B |
|                      ChannelMux_LoopTryRead2_4Producer_4Tasks_4ReferenceTypes |       100000 |  42.23 ms |   0.821 ms |    1.966 ms | 4000.0000 | 2500.0000 | 750.0000  |    22910485 B |
|            ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes |       100000 |  99.11 ms |   1.141 ms |    0.891 ms | 2500.0000 |         - |        -  |    12011148 B |
| ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync |       100000 | 108.61 ms |   0.073 ms |    0.061 ms | 5600.0000 |         - |        -  |    27211277 B |
|                                      ChannelMux_LoopTryRead2_8Producer_8Tasks |       100000 |  88.84 ms |   1.749 ms |    3.765 ms | 6500.0000 | 4166.6667 | 1333.3333 |    39397537 B |


`TryWrite` with added initial `|| _parent._completeException is { }` check
============================================================================
|                                                                        Method | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|------------------------------------------------------------------------------ |------------- |----------:|-----------:|------------:|----------:|----------:|----------:|--------------:|
|                                             ChannelMux_LoopTryRead2_2Producer |       100000 |  13.65 ms |   0.271 ms |    0.333 ms |  968.7500 |  406.2500 |  171.8750 |     5175566 B |
|                                                        ChannelMux_LoopTryRead |       100000 |  15.51 ms |   0.206 ms |    0.193 ms | 1046.8750 |  968.7500 |  968.7500 |     8202302 B |
|                                        ChannelMux_AsyncWaitLoopOnly_2Producer |       100000 |  20.43 ms |   0.454 ms |    1.324 ms | 1406.2500 |  843.7500 |  437.5000 |     7483242 B |
|                                             ChannelMux_LoopTryRead2_3Producer |       100000 |  33.25 ms |   0.663 ms |    1.869 ms | 3187.5000 | 1000.0000 |  312.5000 |    15981770 B |
|                      ChannelMux_LoopTryRead2_4Producer_4Tasks_4ReferenceTypes |       100000 |  42.79 ms |   0.850 ms |    2.102 ms | 4166.6667 | 2083.3333 |  583.3333 |    22975547 B |
|           ChannelMux_LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes |       100000 |  45.07 ms |   0.854 ms |    0.913 ms | 3307.6923 | 2153.8462 |  538.4615 |    20049183 B |
|                                      ChannelMux_LoopTryRead2_8Producer_8Tasks |       100000 |  94.29 ms |   1.852 ms |    3.698 ms | 6666.6667 | 4500.0000 | 1333.3333 |    40123284 B |
|            ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes |       100000 |  98.03 ms |   0.894 ms |    0.836 ms | 2500.0000 |         - |         - |    12011204 B |
| ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync |       100000 | 107.50 ms |   0.299 ms |    0.265 ms | 5600.0000 |         - |         - |    27211333 B |


using boolean exception check
===============================
|                                                                        Method | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|------------------------------------------------------------------------------ |------------- |----------:|-----------:|------------:|----------:|----------:|----------:|--------------:|
|                                             ChannelMux_LoopTryRead2_2Producer |       100000 |  11.27 ms |   0.224 ms |    0.349 ms | 1062.5000 |  765.6250 |  484.3750 |     6092240 B |
|                                                        ChannelMux_LoopTryRead |       100000 |  14.79 ms |   0.286 ms |    0.281 ms | 1046.8750 |  968.7500 |  968.7500 |     8191449 B |
|                                        ChannelMux_AsyncWaitLoopOnly_2Producer |       100000 |  20.00 ms |   0.398 ms |    1.150 ms | 1500.0000 |  687.5000 |  312.5000 |     7733617 B |
|                                             ChannelMux_LoopTryRead2_3Producer |       100000 |  31.05 ms |   0.631 ms |    1.861 ms | 3062.5000 | 1062.5000 |  312.5000 |    16802876 B |
|           ChannelMux_LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes |       100000 |  42.36 ms |   0.843 ms |    1.903 ms | 3250.0000 | 2333.3333 |  750.0000 |    20347538 B |
|                      ChannelMux_LoopTryRead2_4Producer_4Tasks_4ReferenceTypes |       100000 |  43.00 ms |   0.855 ms |    1.841 ms | 3846.1538 | 2230.7692 |  769.2308 |    21816030 B |
|                                      ChannelMux_LoopTryRead2_8Producer_8Tasks |       100000 |  86.39 ms |   1.705 ms |    3.633 ms | 6500.0000 | 4666.6667 | 1666.6667 |    39577919 B |
|            ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes |       100000 |  98.87 ms |   0.874 ms |    0.818 ms | 2500.0000 |         - |         - |    12011204 B |
| ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync |       100000 | 107.61 ms |   0.271 ms |    0.240 ms | 5600.0000 |         - |         - |    27211333 B |

|                                                                        Method | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] | Median [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|------------------------------------------------------------------------------ |------------- |----------:|-----------:|------------:|------------:|----------:|----------:|----------:|--------------:|
|                                             ChannelMux_LoopTryRead2_2Producer |       100000 |  11.99 ms |   0.367 ms |    1.066 ms |    11.60 ms | 1156.2500 |  937.5000 |  531.2500 |     6237784 B |
|                                                        ChannelMux_LoopTryRead |       100000 |  14.74 ms |   0.276 ms |    0.245 ms |    14.72 ms | 1187.5000 |  921.8750 |  890.6250 |     8227336 B |
|                                        ChannelMux_AsyncWaitLoopOnly_2Producer |       100000 |  19.96 ms |   0.397 ms |    1.046 ms |    19.88 ms | 1156.2500 |  718.7500 |  468.7500 |     6973142 B |
|                                             ChannelMux_LoopTryRead2_3Producer |       100000 |  30.15 ms |   0.600 ms |    1.672 ms |    30.19 ms | 2968.7500 | 1281.2500 |  406.2500 |    16182473 B |
|                      ChannelMux_LoopTryRead2_4Producer_4Tasks_4ReferenceTypes |       100000 |  42.82 ms |   0.850 ms |    2.480 ms |    43.04 ms | 3545.4545 | 2363.6364 |  636.3636 |    21435335 B |
|           ChannelMux_LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes |       100000 |  42.89 ms |   0.828 ms |    1.534 ms |    43.04 ms | 3538.4615 | 2307.6923 |  692.3077 |    20610846 B |
|                                      ChannelMux_LoopTryRead2_8Producer_8Tasks |       100000 |  88.07 ms |   1.761 ms |    3.598 ms |    88.23 ms | 7000.0000 | 4000.0000 | 1333.3333 |    41136219 B |
|            ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes |       100000 |  99.02 ms |   0.904 ms |    0.846 ms |    99.17 ms | 2500.0000 |         - |         - |    12011204 B |
| ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync |       100000 | 107.24 ms |   0.309 ms |    0.258 ms |   107.26 ms | 5600.0000 |         - |         - |    27211333 B |


using boolean completion / closed check in TryRead and TryComplete
=====================================================================

|                                                             Method | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|------------------------------------------------------------------- |------------- |----------:|-----------:|------------:|----------:|----------:|----------:|--------------:|
|                                               BroadcastChannelOnly |       100000 |  12.47 ms |   0.242 ms |    0.227 ms |  890.6250 |  421.8750 |  203.1250 |     4964095 B |
|                                             LoopTryRead2_2Producer |       100000 |  12.67 ms |   0.249 ms |    0.380 ms | 1218.7500 |  921.8750 |  562.5000 |     6582722 B |
|                                              LoopTryRead_2Producer |       100000 |  16.19 ms |   0.320 ms |    0.427 ms | 1093.7500 |  937.5000 |  937.5000 |     8158376 B |
|                                        AsyncWaitLoopOnly_2Producer |       100000 |  21.47 ms |   0.428 ms |    1.034 ms | 1500.0000 |  906.2500 |  625.0000 |     8434774 B |
|                                             LoopTryRead2_3Producer |       100000 |  35.00 ms |   0.693 ms |    1.521 ms | 2437.5000 | 1062.5000 |  500.0000 |    13529446 B |
|           LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes |       100000 |  47.06 ms |   0.939 ms |    1.786 ms | 3909.0909 | 2090.9091 |  636.3636 |    22122820 B |
|                      LoopTryRead2_4Producer_4Tasks_4ReferenceTypes |       100000 |  48.13 ms |   0.958 ms |    1.777 ms | 4363.6364 | 2181.8182 |  727.2727 |    23332613 B |
|                                      LoopTryRead2_8Producer_8Tasks |       100000 |  96.80 ms |   1.931 ms |    4.030 ms | 6600.0000 | 3600.0000 | 1400.0000 |    41053501 B |
|            LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes |       100000 | 102.98 ms |   0.541 ms |    0.423 ms | 2400.0000 |         - |         - |    12011277 B |
| LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync |       100000 | 114.41 ms |   1.706 ms |    1.596 ms | 5600.0000 |         - |         - |    27211365 B |

|                                                             Method | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |      Gen1 |      Gen2 | Allocated [B] |
|------------------------------------------------------------------- |------------- |----------:|-----------:|------------:|----------:|----------:|----------:|--------------:|
|                                               BroadcastChannelOnly |       100000 |  12.35 ms |   0.209 ms |    0.196 ms |  843.7500 |  406.2500 |  125.0000 |     4693254 B |
|                                             LoopTryRead2_2Producer |       100000 |  12.78 ms |   0.239 ms |    0.256 ms | 1265.6250 | 1078.1250 |  578.1250 |     6665754 B |
|                                              LoopTryRead_2Producer |       100000 |  16.32 ms |   0.307 ms |    0.287 ms | 1343.7500 |  812.5000 |  812.5000 |     8346454 B |
|                                        AsyncWaitLoopOnly_2Producer |       100000 |  21.22 ms |   0.419 ms |    1.019 ms | 1437.5000 |  656.2500 |  406.2500 |     7549334 B |
|                                             LoopTryRead2_3Producer |       100000 |  35.76 ms |   0.701 ms |    1.171 ms | 2375.0000 |  937.5000 |  437.5000 |    11790162 B |
|           LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes |       100000 |  47.81 ms |   0.922 ms |    1.293 ms | 3545.4545 | 2363.6364 | 1000.0000 |    20018297 B |
|                      LoopTryRead2_4Producer_4Tasks_4ReferenceTypes |       100000 |  48.43 ms |   0.962 ms |    1.250 ms | 3545.4545 | 2454.5455 |  636.3636 |    20069187 B |
|                                      LoopTryRead2_8Producer_8Tasks |       100000 |  94.87 ms |   1.866 ms |    4.213 ms | 6333.3333 | 4500.0000 | 1333.3333 |    39677557 B |
|            LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes |       100000 | 100.78 ms |   0.243 ms |    0.215 ms | 2400.0000 |         - |         - |    12011277 B |
| LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync |       100000 | 112.59 ms |   0.756 ms |    0.707 ms | 5600.0000 |         - |         - |    27211365 B |




CancellationToken use comparison ( for WaitToReadAsync( CancellationToken ) + no !cancellationToken.CanBeCanceled check in WaitToReadAsync
===========================================================================================================================================
|                 Method | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] | Median [ms] |      Gen0 |      Gen1 |     Gen2 | Allocated [B] |
|----------------------- |------------- |---------------------- |----------:|-----------:|------------:|------------:|----------:|----------:|---------:|--------------:|
| LoopTryRead2_2Producer |       100000 |                 False |  13.43 ms |   0.523 ms |    1.535 ms |    12.57 ms | 1265.6250 | 1031.2500 | 562.5000 |     6664571 B |
| LoopTryRead2_2Producer |       100000 |                  True |  14.10 ms |   0.566 ms |    1.660 ms |    13.51 ms | 1062.5000 |  625.0000 | 453.1250 |     6066909 B |


CancellationToken use comparison ( for WaitToReadAsync( CancellationToken ) + YES !cancellationToken.CanBeCanceled check in WaitToReadAsync
============================================================================================================================================
|                 Method | MessageCount | WithCancellationToken | Mean [ms] | Error [ms] | StdDev [ms] | Median [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------- |------------- |---------------------- |----------:|-----------:|------------:|------------:|----------:|---------:|---------:|--------------:|
| LoopTryRead2_2Producer |       100000 |                  True |  12.77 ms |   0.254 ms |    0.458 ms |    12.67 ms | 1140.6250 | 859.3750 | 468.7500 |     6407654 B |
| LoopTryRead2_2Producer |       100000 |                 False |  12.93 ms |   0.283 ms |    0.797 ms |    12.63 ms | 1203.1250 | 968.7500 | 546.8750 |     6421922 B |



 */

// [ Config( typeof(BenchmarkConfig) ) ]
public class ChannelMuxBenchmarks {
    [ Params( 100_000 ) ]
    // [ Params( 100 ) ]
    // ReSharper disable once UnassignedField.Global
    public int MessageCount;

    [ Params( true, false ) ]
    // [ Params( 100 ) ]
    // ReSharper disable once UnassignedField.Global
    public bool WithCancellationToken;

    // [ GlobalSetup ]
    // public void Setup( ) {
    //     System.Diagnostics.Debugger.Launch();
    // }
    // [ GlobalSetup ]
    // public void Setup( ) {
    //     while ( !System.Diagnostics.Debugger.IsAttached )
    //         Thread.Sleep( TimeSpan.FromMilliseconds( 100 ) );
    // }

    private static void producerTask<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in int totalMessages, System.Func<int, T> objectFactory ) {
        int i = 0;
        while ( i++ < totalMessages ) {
            writer.TryWrite( objectFactory( i ) );
        }
        writer.Complete();
    }

    [ Benchmark ]
    public async Task AsyncWaitLoopOnly_2Producer( ) {
        BroadcastChannel<StructA, IBroadcastChannelResponse> channel1 = new ();
        BroadcastChannel<ClassA>                             channel2 = new ();
        ChannelMux<StructA, ClassA>                          mux      = new (channel1.Writer, channel2.Writer); // { EnableLogging = false };
        using CancellationTokenSource?                       cts      = new CancellationTokenSource();
        CancellationToken                                    ct       = CancellationToken.None;
        if ( WithCancellationToken ) {
            ct = cts.Token;
            if ( !ct.CanBeCanceled ) {
                throw new Exception( "CancellationToken needs to be able to be cancelled to properly test." );
            }
        }
        Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, MessageCount, i => new StructA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, MessageCount, i => new ClassA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        int receivedCountStructA = 0;
        int receivedCountClassA  = 0;

        // ReSharper disable UnusedVariable        
        ClassA? classA;
        StructA structA;
        while ( await mux.WaitToReadAsync( ct ) ) {
            if ( mux.TryRead( out classA ) ) {
                receivedCountClassA++;
            }
            if ( mux.TryRead( out structA ) ) {
                receivedCountStructA++;
            }
        }
        // ReSharper restore UnusedVariable
        await producer1;
        await producer2;
        if ( receivedCountClassA != MessageCount || receivedCountStructA != MessageCount ) {
            throw new UnexpectedCountsException( $"Not all messages were read. {nameof(receivedCountClassA)}: {receivedCountClassA} ; {nameof(receivedCountStructA)}: {receivedCountStructA}" );
        }
    }

    [ Benchmark ]
    public async Task LoopTryRead_2Producer( ) {
        BroadcastChannel<StructA?, IBroadcastChannelResponse> channel1 = new ();
        BroadcastChannel<ClassA>                              channel2 = new ();
        ChannelMux<StructA?, ClassA>                          mux      = new (channel1.Writer, channel2.Writer);
        using CancellationTokenSource?                        cts      = new CancellationTokenSource();
        CancellationToken                                     ct       = CancellationToken.None;
        if ( WithCancellationToken ) {
            ct = cts.Token;
            if ( !ct.CanBeCanceled ) {
                throw new Exception( "CancellationToken needs to be able to be cancelled to properly test." );
            }
        }
        Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, MessageCount, i => new StructA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, MessageCount, i => new ClassA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        int      receivedCountStructA = 0;
        int      receivedCountClassA  = 0;
        StructA? structA              = null;
        while ( await mux.WaitToReadAsync( ct ) ) {
            while ( mux.TryRead( out ClassA? classA ) || mux.TryRead( out structA ) ) {
                if ( classA is { } ) {
                    receivedCountClassA++;
                }
                if ( structA is { } ) {
                    receivedCountStructA++;
                    structA = null;
                }
            }
        }
        await producer1;
        await producer2;
        if ( receivedCountClassA != MessageCount || receivedCountStructA != MessageCount ) {
            throw new UnexpectedCountsException( $"Not all messages were read. {nameof(receivedCountClassA)}: {receivedCountClassA} ; {nameof(receivedCountStructA)}: {receivedCountStructA}" );
        }
    }

    [ Benchmark ]
    public async Task LoopTryRead2_2Producer( ) {
        BroadcastChannel<StructA?, IBroadcastChannelResponse> channel1 = new ();
        BroadcastChannel<ClassA>                              channel2 = new ();
        ChannelMux<StructA?, ClassA>                          mux      = new (channel1.Writer, channel2.Writer);
        using CancellationTokenSource?                        cts      = new CancellationTokenSource();
        CancellationToken                                     ct       = CancellationToken.None; // URGENT: PUT ON OTHERS
        if ( WithCancellationToken ) {
            ct = cts.Token;
            if ( !ct.CanBeCanceled ) {
                throw new Exception( "CancellationToken needs to be able to be cancelled to properly test." );
            }
        }
        Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, MessageCount, i => new StructA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, MessageCount, i => new ClassA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        int receivedCountStructA = 0;
        int receivedCountClassA  = 0;
        while ( await mux.WaitToReadAsync( ct ) ) {
            while ( ( mux.TryRead( out ClassA? classA ), mux.TryRead( out StructA? structA ) ) != ( false, false ) ) {
                if ( classA is { } ) {
                    receivedCountClassA++;
                }
                if ( structA is { } ) {
                    receivedCountStructA++;
                }
            }
        }
        await producer1;
        await producer2;
        if ( mux._tryWrite_enter != 0 ) {
            throw new Exception( "Statistics collection should not be enabled during benchmarks" );
        }
        if ( receivedCountClassA != MessageCount || receivedCountStructA != MessageCount ) {
            throw new UnexpectedCountsException( $"Not all messages were read. {nameof(receivedCountClassA)}: {receivedCountClassA} ; {nameof(receivedCountStructA)}: {receivedCountStructA}" );
        }
    }

    [ Benchmark ]
    public async Task LoopTryRead2_3Producer( ) {
        BroadcastChannel<StructA?, IBroadcastChannelResponse> channel1 = new ();
        BroadcastChannel<ClassA>                              channel2 = new ();
        BroadcastChannel<ClassB>                              channel3 = new ();
        ChannelMux<StructA?, ClassA, ClassB>                  mux      = new (channel1.Writer, channel2.Writer, channel3.Writer);
        CancellationToken                                     ct       = CancellationToken.None;
        Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, MessageCount, i => new StructA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, MessageCount, i => new ClassA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer3 = Task.Run( ( ) => producerTask( channel3.Writer, MessageCount, i => new ClassB {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        int receivedCountStructA = 0;
        int receivedCountClassA  = 0;
        int receivedCountClassB  = 0;
        while ( await mux.WaitToReadAsync( ct ) ) {
            while ( (
                       mux.TryRead( out StructA? structA ),
                       mux.TryRead( out ClassA? classA ),
                       mux.TryRead( out ClassB? classB ) ) != ( false, false, false ) ) {
                if ( structA is { } ) {
                    receivedCountStructA++;
                }
                if ( classA is { } ) {
                    receivedCountClassA++;
                }
                if ( classB is { } ) {
                    receivedCountClassB++;
                }
            }
        }
        await producer1;
        await producer2;
        await producer3;
        if ( receivedCountClassA != MessageCount || receivedCountStructA != MessageCount || receivedCountClassB != MessageCount ) {
            throw new UnexpectedCountsException( $"Not all messages were read. Expected {MessageCount}\n\t"    +
                                                 $"{nameof(receivedCountStructA)}: {receivedCountStructA}\n\t" +
                                                 $"{nameof(receivedCountClassA)}: {receivedCountClassA}\n\t"   +
                                                 $"{nameof(receivedCountClassB)}: {receivedCountClassB}\n\t"
            );
        }
    }

    [ Benchmark ]
    public async Task LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes( ) {
        BroadcastChannel<StructA?, IBroadcastChannelResponse> channel1 = new ();
        BroadcastChannel<ClassA>                              channel2 = new ();
        BroadcastChannel<ClassB>                              channel3 = new ();
        BroadcastChannel<ClassC>                              channel4 = new ();
        ChannelMux<StructA?, ClassA, ClassB, ClassC>          mux      = new (channel1.Writer, channel2.Writer, channel3.Writer, channel4.Writer);
        CancellationToken                                     ct       = CancellationToken.None;
        Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, MessageCount, i => new StructA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, MessageCount, i => new ClassA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer3 = Task.Run( ( ) => producerTask( channel3.Writer, MessageCount, i => new ClassB {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer4 = Task.Run( ( ) => producerTask( channel4.Writer, MessageCount, i => new ClassC {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        int receivedCountStructA = 0;
        int receivedCountClassA  = 0;
        int receivedCountClassB  = 0;
        int receivedCountClassC  = 0;
        while ( await mux.WaitToReadAsync( ct ) ) {
            while ( (
                       mux.TryRead( out StructA? structA ),
                       mux.TryRead( out ClassA? classA ),
                       mux.TryRead( out ClassB? classB ),
                       mux.TryRead( out ClassC? classC ) ) != ( false, false, false, false ) ) {
                if ( structA is { } ) {
                    receivedCountStructA++;
                }
                if ( classA is { } ) {
                    receivedCountClassA++;
                }
                if ( classB is { } ) {
                    receivedCountClassB++;
                }
                if ( classC is { } ) {
                    receivedCountClassC++;
                }
            }
        }
        await producer1;
        await producer2;
        await producer3;
        await producer4;
        if ( receivedCountClassA != MessageCount || receivedCountStructA != MessageCount || receivedCountClassB != MessageCount || receivedCountClassC != MessageCount ) {
            throw new UnexpectedCountsException( $"Not all messages were read. Expected {MessageCount}\n\t"    +
                                                 $"{nameof(receivedCountStructA)}: {receivedCountStructA}\n\t" +
                                                 $"{nameof(receivedCountClassA)}: {receivedCountClassA}\n\t"   +
                                                 $"{nameof(receivedCountClassB)}: {receivedCountClassB}\n\t"   +
                                                 $"{nameof(receivedCountClassC)}: {receivedCountClassC}\n\t"
            );
        }
    }

    [ Benchmark ]
    public async Task LoopTryRead2_4Producer_4Tasks_4ReferenceTypes( ) {
        BroadcastChannel<ClassA>                   channel1 = new ();
        BroadcastChannel<ClassB>                   channel2 = new ();
        BroadcastChannel<ClassC>                   channel3 = new ();
        BroadcastChannel<ClassD>                   channel4 = new ();
        ChannelMux<ClassA, ClassB, ClassC, ClassD> mux      = new (channel1.Writer, channel2.Writer, channel3.Writer, channel4.Writer);
        CancellationToken                          ct       = CancellationToken.None;
        Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, MessageCount, i => new ClassA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, MessageCount, i => new ClassB {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer3 = Task.Run( ( ) => producerTask( channel3.Writer, MessageCount, i => new ClassC {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer4 = Task.Run( ( ) => producerTask( channel4.Writer, MessageCount, i => new ClassD {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        int receivedCountClassA = 0;
        int receivedCountClassB = 0;
        int receivedCountClassC = 0;
        int receivedCountClassD = 0;
        while ( await mux.WaitToReadAsync( ct ) ) {
            while ( (
                       mux.TryRead( out ClassA? classA ),
                       mux.TryRead( out ClassB? classB ),
                       mux.TryRead( out ClassC? classC ),
                       mux.TryRead( out ClassD? classD ) ) != ( false, false, false, false ) ) {
                if ( classA is { } ) {
                    receivedCountClassA++;
                }
                if ( classB is { } ) {
                    receivedCountClassB++;
                }
                if ( classC is { } ) {
                    receivedCountClassC++;
                }
                if ( classD is { } ) {
                    receivedCountClassD++;
                }
            }
        }
        await producer1;
        await producer2;
        await producer3;
        await producer4;
        if ( receivedCountClassA != MessageCount || receivedCountClassB != MessageCount || receivedCountClassC != MessageCount || receivedCountClassD != MessageCount ) {
            throw new UnexpectedCountsException( $"Not all messages were read. Expected {MessageCount}\n\t"  +
                                                 $"{nameof(receivedCountClassA)}: {receivedCountClassA}\n\t" +
                                                 $"{nameof(receivedCountClassB)}: {receivedCountClassB}\n\t" +
                                                 $"{nameof(receivedCountClassC)}: {receivedCountClassC}\n\t" +
                                                 $"{nameof(receivedCountClassD)}: {receivedCountClassD}\n\t"
            );
        }
    }

    [ Benchmark ]
    public async Task LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes( ) {
        BroadcastChannel<StructA?, IBroadcastChannelResponse> channel1 = new ();
        BroadcastChannel<ClassA>                              channel2 = new ();
        BroadcastChannel<ClassB>                              channel3 = new ();
        BroadcastChannel<ClassC>                              channel4 = new ();
        ChannelMux<StructA?, ClassA, ClassB, ClassC>          mux      = new (channel1.Writer, channel2.Writer, channel3.Writer, channel4.Writer);
        CancellationToken                                     ct       = CancellationToken.None;
        Task                                                  producer = Task.Run( producerTaskMultiChannel, ct );

        void producerTaskMultiChannel( ) {
            int i = 0;
            while ( i++ < MessageCount ) {
                channel1.Writer.TryWrite( new StructA {
                                              Id   = i,
                                              Name = @"some_text"
                                          } );
                channel2.Writer.TryWrite( new ClassA {
                                              Id   = i,
                                              Name = @"some_text"
                                          } );
                channel3.Writer.TryWrite( new ClassB {
                                              Id   = i,
                                              Name = @"some_text"
                                          } );
                channel4.Writer.TryWrite( new ClassC {
                                              Id   = i,
                                              Name = @"some_text"
                                          } );
            }
            channel1.Writer.Complete();
            channel2.Writer.Complete();
            channel3.Writer.Complete();
            channel4.Writer.Complete();
        }

        int receivedCountStructA = 0;
        int receivedCountClassA  = 0;
        int receivedCountClassB  = 0;
        int receivedCountClassC  = 0;
        while ( await mux.WaitToReadAsync( ct ) ) {
            while ( (
                       mux.TryRead( out StructA? structA ),
                       mux.TryRead( out ClassA? classA ),
                       mux.TryRead( out ClassB? classB ),
                       mux.TryRead( out ClassC? classC ) ) != ( false, false, false, false ) ) {
                if ( structA is { } ) {
                    receivedCountStructA++;
                }
                if ( classA is { } ) {
                    receivedCountClassA++;
                }
                if ( classB is { } ) {
                    receivedCountClassB++;
                }
                if ( classC is { } ) {
                    receivedCountClassC++;
                }
            }
        }
        await producer;
        if ( receivedCountClassA != MessageCount || receivedCountStructA != MessageCount || receivedCountClassB != MessageCount || receivedCountClassC != MessageCount ) {
            throw new UnexpectedCountsException( $"Not all messages were read. Expected {MessageCount}\n\t"    +
                                                 $"{nameof(receivedCountStructA)}: {receivedCountStructA}\n\t" +
                                                 $"{nameof(receivedCountClassA)}: {receivedCountClassA}\n\t"   +
                                                 $"{nameof(receivedCountClassB)}: {receivedCountClassB}\n\t"   +
                                                 $"{nameof(receivedCountClassC)}: {receivedCountClassC}\n\t"
            );
        }
    }

    [ Benchmark ]
    public async Task LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes_WriteAsync( ) {
        BroadcastChannel<StructA?, IBroadcastChannelResponse> channel1 = new ();
        BroadcastChannel<ClassA>                              channel2 = new ();
        BroadcastChannel<ClassB>                              channel3 = new ();
        BroadcastChannel<ClassC>                              channel4 = new ();
        ChannelMux<StructA?, ClassA, ClassB, ClassC>          mux      = new (channel1.Writer, channel2.Writer, channel3.Writer, channel4.Writer);
        CancellationToken                                     ct       = CancellationToken.None;
        Task                                                  producer = Task.Run( producerTaskMultiChannel, ct );

        async Task producerTaskMultiChannel( ) {
            int i = 0;
            while ( i++ < MessageCount ) {
                await channel1.Writer.WriteAsync( new StructA {
                                                      Id   = i,
                                                      Name = @"some_text"
                                                  }, ct ).ConfigureAwait( false );
                await channel2.Writer.WriteAsync( new ClassA {
                                                      Id   = i,
                                                      Name = @"some_text"
                                                  }, ct ).ConfigureAwait( false );
                await channel3.Writer.WriteAsync( new ClassB {
                                                      Id   = i,
                                                      Name = @"some_text"
                                                  }, ct ).ConfigureAwait( false );
                await channel4.Writer.WriteAsync( new ClassC {
                                                      Id   = i,
                                                      Name = @"some_text"
                                                  }, ct ).ConfigureAwait( false );
            }
            channel1.Writer.Complete();
            channel2.Writer.Complete();
            channel3.Writer.Complete();
            channel4.Writer.Complete();
        }

        int receivedCountStructA = 0;
        int receivedCountClassA  = 0;
        int receivedCountClassB  = 0;
        int receivedCountClassC  = 0;
        while ( await mux.WaitToReadAsync( ct ) ) {
            while ( (
                       mux.TryRead( out StructA? structA ),
                       mux.TryRead( out ClassA? classA ),
                       mux.TryRead( out ClassB? classB ),
                       mux.TryRead( out ClassC? classC ) ) != ( false, false, false, false ) ) {
                if ( structA is { } ) {
                    receivedCountStructA++;
                }
                if ( classA is { } ) {
                    receivedCountClassA++;
                }
                if ( classB is { } ) {
                    receivedCountClassB++;
                }
                if ( classC is { } ) {
                    receivedCountClassC++;
                }
            }
        }
        await producer;
        if ( receivedCountClassA != MessageCount || receivedCountStructA != MessageCount || receivedCountClassB != MessageCount || receivedCountClassC != MessageCount ) {
            throw new UnexpectedCountsException( $"Not all messages were read. Expected {MessageCount}\n\t"    +
                                                 $"{nameof(receivedCountStructA)}: {receivedCountStructA}\n\t" +
                                                 $"{nameof(receivedCountClassA)}: {receivedCountClassA}\n\t"   +
                                                 $"{nameof(receivedCountClassB)}: {receivedCountClassB}\n\t"   +
                                                 $"{nameof(receivedCountClassC)}: {receivedCountClassC}\n\t"
            );
        }
    }


    [ Benchmark ]
    public async Task LoopTryRead2_8Producer_8Tasks( ) {
        BroadcastChannel<ClassA> channel1 = new ();
        BroadcastChannel<ClassB> channel2 = new ();
        BroadcastChannel<ClassC> channel3 = new ();
        BroadcastChannel<ClassD> channel4 = new ();
        BroadcastChannel<ClassE> channel5 = new ();
        BroadcastChannel<ClassF> channel6 = new ();
        BroadcastChannel<ClassG> channel7 = new ();
        BroadcastChannel<ClassH> channel8 = new ();
        ChannelMux<ClassA, ClassB, ClassC, ClassD, ClassE, ClassF, ClassG, ClassH> mux = new (
            channel1.Writer, channel2.Writer, channel3.Writer, channel4.Writer,
            channel5.Writer, channel6.Writer, channel7.Writer, channel8.Writer);
        CancellationToken ct = CancellationToken.None;
        Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, MessageCount, i => new ClassA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, MessageCount, i => new ClassB {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer3 = Task.Run( ( ) => producerTask( channel3.Writer, MessageCount, i => new ClassC {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer4 = Task.Run( ( ) => producerTask( channel4.Writer, MessageCount, i => new ClassD {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer5 = Task.Run( ( ) => producerTask( channel5.Writer, MessageCount, i => new ClassE {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer6 = Task.Run( ( ) => producerTask( channel6.Writer, MessageCount, i => new ClassF {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer7 = Task.Run( ( ) => producerTask( channel7.Writer, MessageCount, i => new ClassG {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer8 = Task.Run( ( ) => producerTask( channel8.Writer, MessageCount, i => new ClassH {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        int receivedCountClassA = 0;
        int receivedCountClassB = 0;
        int receivedCountClassC = 0;
        int receivedCountClassD = 0;
        int receivedCountClassE = 0;
        int receivedCountClassF = 0;
        int receivedCountClassG = 0;
        int receivedCountClassH = 0;
        while ( await mux.WaitToReadAsync( ct ) ) {
            while ( (
                       mux.TryRead( out ClassA? classA ),
                       mux.TryRead( out ClassB? classB ),
                       mux.TryRead( out ClassC? classC ),
                       mux.TryRead( out ClassD? classD ),
                       mux.TryRead( out ClassE? classE ),
                       mux.TryRead( out ClassF? classF ),
                       mux.TryRead( out ClassG? classG ),
                       mux.TryRead( out ClassH? classH ) ) != ( false, false, false, false, false, false, false, false ) ) {
                if ( classA is { } ) {
                    receivedCountClassA++;
                }
                if ( classB is { } ) {
                    receivedCountClassB++;
                }
                if ( classC is { } ) {
                    receivedCountClassC++;
                }
                if ( classD is { } ) {
                    receivedCountClassD++;
                }
                if ( classE is { } ) {
                    receivedCountClassE++;
                }
                if ( classF is { } ) {
                    receivedCountClassF++;
                }
                if ( classG is { } ) {
                    receivedCountClassG++;
                }
                if ( classH is { } ) {
                    receivedCountClassH++;
                }
            }
        }
        await producer1;
        await producer2;
        await producer3;
        await producer4;
        await producer5;
        await producer6;
        await producer7;
        await producer8;
        if ( receivedCountClassA    != MessageCount || receivedCountClassB != MessageCount || receivedCountClassC != MessageCount || receivedCountClassD != MessageCount
             || receivedCountClassE != MessageCount || receivedCountClassF != MessageCount || receivedCountClassG != MessageCount || receivedCountClassH != MessageCount ) {
            throw new UnexpectedCountsException( $"Not all messages were read. Expected {MessageCount}\n\t"  +
                                                 $"{nameof(receivedCountClassA)}: {receivedCountClassA}\n\t" +
                                                 $"{nameof(receivedCountClassB)}: {receivedCountClassB}\n\t" +
                                                 $"{nameof(receivedCountClassC)}: {receivedCountClassC}\n\t" +
                                                 $"{nameof(receivedCountClassD)}: {receivedCountClassD}\n\t" +
                                                 $"{nameof(receivedCountClassE)}: {receivedCountClassE}\n\t" +
                                                 $"{nameof(receivedCountClassF)}: {receivedCountClassF}\n\t" +
                                                 $"{nameof(receivedCountClassG)}: {receivedCountClassG}\n\t" +
                                                 $"{nameof(receivedCountClassH)}: {receivedCountClassH}\n\t"
            );
        }
    }


    /*
     * 
     */

    [ Benchmark ]
    public async Task BroadcastChannelOnly( ) {
        BroadcastChannel<StructA?, IBroadcastChannelResponse> channel1       = new ();
        BroadcastChannel<ClassA>                              channel2       = new ();
        var                                                   channelReader1 = channel1.GetReader(); // these must be setup BEFORE the producer begins
        var                                                   channelReader2 = channel2.GetReader();
        CancellationToken                                     ct             = CancellationToken.None;
        Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, MessageCount, i => new StructA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, MessageCount, i => new ClassA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        int receivedCountStructA = 0;
        int receivedCountClassA  = 0;
        // ReSharper disable UnusedVariable
        Task reader1 = Task.Run( async ( ) => {
            while ( await channelReader1.WaitToReadAsync( ct ) ) {
                if ( channelReader1.TryRead( out StructA? structA ) ) {
                    receivedCountStructA++;
                }
            }
        } );
        Task reader2 = Task.Run( async ( ) => {
            while ( await channelReader2.WaitToReadAsync( ct ) ) {
                if ( channelReader2.TryRead( out ClassA? classA ) ) {
                    receivedCountClassA++;
                }
            }
        } );
        // ReSharper restore UnusedVariable
        await producer1;
        await producer2;
        await reader1;
        await reader2;
        if ( receivedCountClassA != MessageCount || receivedCountStructA != MessageCount ) {
            throw new UnexpectedCountsException( $"Not all messages were read. {nameof(receivedCountClassA)}: {receivedCountClassA} ; {nameof(receivedCountStructA)}: {receivedCountStructA}" );
        }
    }
}

public struct StructA {
    public          int    Id;
    public          string Name;
    public          string Description;
    public override string ToString( ) => $"{nameof(StructA)} {{ Id = {Id}, Name = {Name}, Description = {Description} }}";
}

public class ClassA {
    public          int    Id;
    public          string Name        = String.Empty;
    public          string Description = String.Empty;
    public override string ToString( ) => $"{nameof(ClassA)} {{ Id = {Id}, Name = {Name}, Description = {Description} }}";
}

public class ClassB {
    public          int    Id;
    public          string Name        = String.Empty;
    public          string Description = String.Empty;
    public override string ToString( ) => $"{nameof(ClassB)} {{ Id = {Id}, Name = {Name}, Description = {Description} }}";
}

public class ClassC {
    public          int    Id;
    public          string Name        = String.Empty;
    public          string Description = String.Empty;
    public override string ToString( ) => $"{nameof(ClassC)} {{ Id = {Id}, Name = {Name}, Description = {Description} }}";
}

public class ClassD {
    public          int    Id;
    public          string Name        = String.Empty;
    public          string Description = String.Empty;
    public override string ToString( ) => $"{nameof(ClassD)} {{ Id = {Id}, Name = {Name}, Description = {Description} }}";
}

public class ClassE {
    public          int    Id;
    public          string Name        = String.Empty;
    public          string Description = String.Empty;
    public override string ToString( ) => $"{nameof(ClassE)} {{ Id = {Id}, Name = {Name}, Description = {Description} }}";
}

public class ClassF {
    public          int    Id;
    public          string Name        = String.Empty;
    public          string Description = String.Empty;
    public override string ToString( ) => $"{nameof(ClassF)} {{ Id = {Id}, Name = {Name}, Description = {Description} }}";
}

public class ClassG {
    public          int    Id;
    public          string Name        = String.Empty;
    public          string Description = String.Empty;
    public override string ToString( ) => $"{nameof(ClassG)} {{ Id = {Id}, Name = {Name}, Description = {Description} }}";
}

public class ClassH {
    public          int    Id;
    public          string Name        = String.Empty;
    public          string Description = String.Empty;
    public override string ToString( ) => $"{nameof(ClassH)} {{ Id = {Id}, Name = {Name}, Description = {Description} }}";
}