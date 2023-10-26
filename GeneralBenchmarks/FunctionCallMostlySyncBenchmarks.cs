using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;

using Benchmarks.Common;

namespace Benchmarks.General;

/*
 *
 * _numCalls = 20_000_000;
 * _workFrequency = 100;
 *
   | Method                                 | Mean [ms] | Error [ms] | StdDev [ms] | Ratio | RatioSD | Gen0       | Allocated [B] | Alloc Ratio |
   |--------------------------------------- |----------:|-----------:|------------:|------:|--------:|-----------:|--------------:|------------:|
   | SimpleReturn                           |  20.42 ms |   0.408 ms |    0.692 ms |  1.00 |    0.00 |          - |        3336 B |        1.00 |
   |                                        |           |            |             |       |         |            |               |             |
   | TaskPassthroughCacheCompleted          |  68.78 ms |   0.070 ms |    0.062 ms |     ? |       ? | 10000.0000 |    48000936 B |           ? |
   | TaskPassthroughCacheCompletedNotStatic |  69.20 ms |   0.630 ms |    0.558 ms |     ? |       ? | 10000.0000 |    48000936 B |           ? |
   | TaskPassthrough                        |  76.57 ms |   0.183 ms |    0.153 ms |     ? |       ? | 10000.0000 |    48000936 B |           ? |
   |                                        |           |            |             |       |         |            |               |             |
   | ValueTaskOuterReturn                   |  39.53 ms |   0.578 ms |    0.512 ms |     ? |       ? | 10000.0000 |    48000936 B |           ? |
   | ValueTaskOutParamReturn                |  60.88 ms |   0.111 ms |    0.099 ms |     ? |       ? | 10000.0000 |    48000936 B |           ? |
   | ValueTaskReturn                        | 488.51 ms |   0.655 ms |    0.581 ms |     ? |       ? | 10000.0000 |    48000936 B |           ? |
 *
 *
 * _numCalls = 40_000_000;
 *
 * | Method                        | WorkFrequency | Mean [ms] | Error [ms] | StdDev [ms] | Gen0       | Allocated [B] |
   |------------------------------ |-------------- |----------:|-----------:|------------:|-----------:|--------------:|
   | TaskPassthrough               | 5000          |  147.7 ms |    0.05 ms |     0.04 ms |          - |     1920936 B |
   | TaskPassthroughCacheCompleted | 5000          |  147.8 ms |    0.20 ms |     0.16 ms |          - |     1920936 B |
   | TaskPassthroughCacheCompleted | 1000          |  149.6 ms |    0.08 ms |     0.06 ms |  2000.0000 |     9600936 B |
   | TaskPassthrough               | 1000          |  150.7 ms |    1.42 ms |     1.33 ms |  2000.0000 |     9600936 B |
   | TaskPassthroughCacheCompleted | 100           |  164.4 ms |    1.75 ms |     1.63 ms | 20000.0000 |    96000936 B |
   | TaskPassthrough               | 100           |  166.5 ms |    3.10 ms |     3.05 ms | 20000.0000 |    96000936 B |
   |                               |               |           |            |             |            |               |
   | ValueTaskOuterReturn          | 5000          |  110.1 ms |    0.17 ms |     0.15 ms |          - |     1920936 B |
   | ValueTaskOuterReturn          | 1000          |  111.3 ms |    0.06 ms |     0.05 ms |  2000.0000 |     9600936 B |
   | ValueTaskOuterReturn          | 100           |  126.1 ms |    0.88 ms |     0.74 ms | 20000.0000 |    96000936 B |
   | ValueTaskOutParamReturn       | 5000          |  137.3 ms |    0.05 ms |     0.04 ms |          - |     1920936 B |
   | ValueTaskOutParamReturn       | 1000          |  139.0 ms |    0.14 ms |     0.11 ms |  2000.0000 |     9600936 B |
   | ValueTaskOutParamReturn       | 100           |  153.4 ms |    0.19 ms |     0.17 ms | 20000.0000 |    96000936 B |

 */

[ Config( typeof(BenchmarkConfig) ) ]
[ GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory ) ]
public class FunctionCallMostlySyncBenchmarks {
    private const int _numCalls = 10_000_000;

    //
    // [ Params( 100, 1_000, 5_000 ) ]
    // [ Params( 100, 1_000 ) ]
    [ Params( 1_000 ) ]
    [ SuppressMessage( "ReSharper", "UnassignedGetOnlyAutoProperty" ) ]
    // lower number = more async work
    public int WorkFrequency { get; set; }
    // private const int _workFrequency = 100;
    // private const int _workFrequency = 1_000;

    [ Params( 5, 20 ) ]
    [ SuppressMessage( "ReSharper", "UnassignedGetOnlyAutoProperty" ) ]
    // Higher number = more sync work
    public int SyncWorkLevel { get; set; }


    [ IterationSetup ]
    public void Setup( ) { }

    // [ BenchmarkCategory( "Sync" ) ]
    // [ Benchmark( Baseline = true ) ]
    // public int SimpleReturn( ) {
    //     int returnedDtoCount = 0;
    //
    //     static LargeDto? doWork( int data ) {
    //         if ( data % _workFrequency == 0 ) {
    //             return new LargeDto();
    //         }
    //         return null;
    //     }
    //
    //     for ( int i = 0 ; i < _numCalls ; i++ ) {
    //         if ( doWork( i ) is { } ) {
    //             returnedDtoCount++;
    //         }
    //     }
    //
    //     return returnedDtoCount;
    // }


    [ BenchmarkCategory( "Task" ) ]
    [ Benchmark ]
    public async Task<int> AsyncTaskWorkInCalled( ) {
        int returnedDtoCount = 0;
        var worker           = new Worker();

        for ( int i = 0 ; i < _numCalls ; i++ ) {
            if ( await worker.DoWorkAsTask( i, WorkFrequency, SyncWorkLevel ) ) {
                returnedDtoCount++;
            }
        }

        return returnedDtoCount;
    }


    [ BenchmarkCategory( "ValueTask" ) ]
    [ Benchmark ]
    public async Task<int> AsyncValueTaskWorkInCalled( ) {
        int returnedDtoCount = 0;
        var worker           = new Worker();

        for ( int i = 0 ; i < _numCalls ; i++ ) {
            if ( await worker.DoWorkAsValueTask( i, WorkFrequency, SyncWorkLevel ) ) {
                returnedDtoCount++;
            }
        }

        return returnedDtoCount;
    }


    [ BenchmarkCategory( "Sync" ) ]
    [ Benchmark ]
    [SuppressMessage("ReSharper", "UnusedVariable")]
    public async Task<int> TaskOuter( ) {
        int returnedDtoCount = 0;
        var worker           = new Worker();
        var api              = new SomeApi();

        for ( int i = 0 ; i < _numCalls ; i++ ) {
            worker.Results += worker.SyncOnly( i, WorkFrequency, SyncWorkLevel ) switch {
                                  { Exception: { } exception }        => 0,
                                  { Payload  : LargeDto { } payload } => await api.CallApi( payload ),
                                  _ => 0
                              };
            returnedDtoCount++;
        }
        return returnedDtoCount;
    }

    [ BenchmarkCategory( "Sync" ) ]
    [ Benchmark ]
    public async Task<int> SyncTriggerTask( ) {
        int returnedDtoCount = 0;
        var worker           = new Worker();

        for ( int i = 0 ; i < _numCalls ; i++ ) {
            if ( worker.SyncOnly( i, WorkFrequency, SyncWorkLevel ) is { } data ) {
                returnedDtoCount++;
                await worker.InputObjectDoTask( data );
            }
        }

        return returnedDtoCount;
    }


/*
 *
 */

// [ BenchmarkCategory( "Task", "AsyncWorkInCalled" ) ]
// [ Benchmark() ]
// public async ValueTask<int> TaskReturn( ) {
//     static async Task doWork( int data, int workFrequency ) {
//         if ( data % workFrequency == 0 ) {
//             await callApi( new LargeDto() );
//         }
//     }
//
//     static Task<int> callApi( LargeDto dto ) {
//         return Task.FromResult( dto.Prop10 + 1 );
//     }
//
//     int i;
//     for ( i = 0 ; i < _numCalls ; i++ ) {
//         await doWork( i, WorkFrequency );
//     }
//
//     return i;
// }
//
// [ BenchmarkCategory( "Task", "AsyncWorkInCalled" ) ]
// [ Benchmark() ]
// public async ValueTask<int> TaskReturnTwoCalls( ) {
//     static async Task doWork( int data, int workFrequency ) {
//         if ( data % workFrequency == 0 ) {
//             await callApi( new LargeDto() );
//         }
//     }
//
//     static Task<int> callApi( LargeDto dto ) {
//         return Task.FromResult( dto.Prop10 + 1 );
//     }
//
//     int i;
//     for ( i = 0 ; i < _numCalls ; i++ ) {
//         var t1 = doWork( i, WorkFrequency );
//         var t2 = doWork( i, WorkFrequency );
//         await t1;
//         await t2;
//     }
//
//     return i;
// }

// [ BenchmarkCategory( "Task", "AsyncWorkInCalled" ) ]
// [ Benchmark() ]
// public async ValueTask<int> TaskReturnTwoCallsWhenAll( ) {
//     static async Task doWork( int data, int workFrequency ) {
//         if ( data % workFrequency == 0 ) {
//             await callApi( new LargeDto() );
//         }
//     }
//
//     static Task<int> callApi( LargeDto dto ) {
//         return Task.FromResult( dto.Prop10 + 1 );
//     }
//
//     int i;
//     for ( i = 0 ; i < _numCalls ; i++ ) {
//         var t1 = doWork( i, WorkFrequency );
//         var t2 = doWork( i, WorkFrequency );
//         await Task.WhenAll( t1, t2 );
//     }
//
//     return i;
// }
//
// [ BenchmarkCategory( "ValueTask", "AsyncWorkInCalled"  ) ]
// [ Benchmark() ]
// public async ValueTask<int> ValueTaskReturn( ) {
//     static async ValueTask doWork( int data, int workFrequency ) {
//         if ( data % workFrequency == 0 ) {
//             await callApi( new LargeDto() );
//         }
//     }
//
//     static Task<int> callApi( LargeDto dto ) {
//         return Task.FromResult( dto.Prop10 + 1 );
//     }
//
//     int i;
//     for ( i = 0 ; i < _numCalls ; i++ ) {
//         await doWork( i, WorkFrequency );
//     }
//
//     return i;
// }

// [ BenchmarkCategory( "ValueTask", "AsyncWorkInCalled"  ) ]
// [ Benchmark() ]
// public async ValueTask<int> ValueTaskReturnTwoCalls( ) {
//     static async ValueTask doWork( int data, int workFrequency ) {
//         if ( data % workFrequency == 0 ) {
//             await callApi( new LargeDto() );
//         }
//     }
//
//     static Task<int> callApi( LargeDto dto ) {
//         return Task.FromResult( dto.Prop10 + 1 );
//     }
//
//     int i;
//     for ( i = 0 ; i < _numCalls ; i++ ) {
//         var t1 = doWork( i, WorkFrequency );
//         var t2 = doWork( i, WorkFrequency );
//         await t1;
//         await t2;
//     }
//
//     return i;
// }

// [ BenchmarkCategory( "ValueTask" ) ]
// [ Benchmark ]
// public async ValueTask<int> ValueTaskOutParamReturn( ) {
//     int returnedDtoCount = 0;
//
//     static bool doWork( int data, int workFrequency, [ NotNullWhen( true ) ] out LargeDto? dto ) {
//         if ( data % workFrequency == 0 ) {
//             dto = new LargeDto();
//             return true;
//         }
//         dto = null;
//         return false;
//     }
//
//     static Task<int> callApi( LargeDto dto ) {
//         return Task.FromResult( dto.Prop10 + 1 );
//     }
//
//     for ( int i = 0 ; i < _numCalls ; i++ ) {
//         if ( doWork( i, WorkFrequency, out var dto ) ) {
//             returnedDtoCount += await callApi( dto );
//         }
//     }
//
//     return returnedDtoCount;
// }

// [ BenchmarkCategory( "ValueTask" ) ]
// [ Benchmark ]
// public async Task<int> TaskOuter( ) {
//     int returnedDtoCount = 0;
//
//     static LargeDto? doWork( int data, int workFrequency ) {
//         if ( data % workFrequency == 0 ) {
//             return new LargeDto();
//         }
//         return null;
//     }
//
//     static Task<int> callApi( LargeDto dto ) {
//         return Task.FromResult( dto.Prop10 + 1 );
//     }
//
//     for ( int i = 0 ; i < _numCalls ; i++ ) {
//         if ( doWork( i, WorkFrequency ) is { } dto ) {
//             returnedDtoCount += await callApi( dto );
//         }
//     }
//
//     return returnedDtoCount;
// }

//
// [ BenchmarkCategory( "Task" ) ]
// [ Benchmark ]
// public async ValueTask<int> TaskPassthrough( ) {
//     static Task doWork( int data, int workFrequency ) {
//         if ( data % workFrequency == 0 ) {
//             return callApi( new LargeDto() );
//         }
//         return Task.CompletedTask;
//     }
//
//     static Task<int> callApi( LargeDto dto ) {
//         return Task.FromResult( dto.Prop10 + 1 );
//     }
//
//     int i;
//     for ( i = 0 ; i < _numCalls ; i++ ) {
//         await doWork( i, WorkFrequency );
//     }
//
//     return i;
// }

// [ BenchmarkCategory( "Task" ) ]
// [ Benchmark ]
// public async ValueTask<int> TaskPassthroughCacheCompleted( ) {
//     static Task doWork( int data, int workFrequency ) {
//         if ( data % workFrequency == 0 ) {
//             return callApi( new LargeDto() );
//         }
//         return _completedTask;
//     }
//
//     static Task<int> callApi( LargeDto dto ) {
//         return Task.FromResult( dto.Prop10 + 1 );
//     }
//
//     int i;
//     for ( i = 0 ; i < _numCalls ; i++ ) {
//         await doWork( i, WorkFrequency );
//     }
//
//     return i;
// }

// [ BenchmarkCategory( "Task" ) ]
// [ Benchmark ]
// public async ValueTask<int> TaskPassthroughCacheCompletedNotStatic( ) {
//     Task doWork( int data ) {
//         if ( data % _workFrequency == 0 ) {
//             return callApi( new LargeDto() );
//         }
//         return _completedTask;
//     }
//
//     Task<int> callApi( LargeDto dto ) {
//         return Task.FromResult( dto.Prop10 + 1 );
//     }
//
//     int i;
//     for ( i = 0 ; i < _numCalls ; i++ ) {
//         await doWork( i );
//     }
//
//     return i;
// }

// [ BenchmarkCategory( "Task" ) ]
// [ Benchmark ]
// public async ValueTask<int> TaskPassthroughCacheCompletedStaticPassIn( ) {
//     static Task doWork( int data, int workFrequency ) {
//         if ( data % workFrequency == 0 ) {
//             return callApi( new LargeDto() );
//         }
//         return _completedTask;
//     }
//
//     static Task<int> callApi( LargeDto dto ) {
//         return Task.FromResult( dto.Prop10 + 1 );
//     }
//
//     int i;
//     for ( i = 0 ; i < _numCalls ; i++ ) {
//         await doWork( i, _workFrequency );
//     }
//
//     return i;
// }

// private static readonly Task _completedTask = Task.CompletedTask;
}

[ SuppressMessage( "ReSharper", "UnusedMember.Local" ) ]
[ SuppressMessage( "ReSharper", "UnusedAutoPropertyAccessor.Local" ) ]
file class LargeDto {
    public string?  Prop0  { get; init; }
    public string?  Prop1  { get; init; }
    public string?  Prop2  { get; init; }
    public string?  Prop3  { get; init; }
    public string?  Prop4  { get; init; }
    public decimal? Prop5  { get; init; }
    public decimal? Prop6  { get; init; }
    public decimal? Prop7  { get; init; }
    public decimal? Prop8  { get; init; }
    public decimal? Prop9  { get; init; }
    public int      Prop10 { get; init; }
    public int      Prop11 { get; init; }
    public int      Prop12 { get; init; }
    public int      Prop13 { get; init; }
    public int      Prop14 { get; init; }
    public double   Prop15 { get; init; }
    public double   Prop16 { get; init; }
    public double   Prop17 { get; init; }
    public double   Prop18 { get; init; }
    public double   Prop19 { get; init; }
}

file class Worker {
    public  int  Results         { get; set; }
    private long _numWorkResults { get; set; }

    private readonly SomeApi _api = new SomeApi();

    private readonly List<int> _nums = new ();

    private int someSyncWork( int count ) {
        for ( int i = 0 ; i < count ; i++ ) {
            _nums.Add( i );
        }
        int counter = 0;
        // ReSharper disable once ForCanBeConvertedToForeach
        for ( int i = 0 ; i < _nums.Count ; i++ ) {
            counter += _nums[ i ];
        }
        _nums.Clear();
        return counter;
    }

    public async Task<bool> DoWorkAsTask( int data, int workFrequency, int syncWorkLevel ) {
        _numWorkResults += someSyncWork( syncWorkLevel );
        if ( data % workFrequency == 0 ) {
            Results += await _api.CallApi( new LargeDto() );
            return true;
        }
        return false;
    }

    public async ValueTask<bool> DoWorkAsValueTask( int data, int workFrequency, int syncWorkLevel ) {
        _numWorkResults += someSyncWork( syncWorkLevel );
        if ( data % workFrequency == 0 ) {
            Results += await _api.CallApi( new LargeDto() );
            return true;
        }
        return false;
    }

    public async Task InputObjectDoTask( object data ) {
        if ( data is LargeDto dto ) {
            Results += await _api.CallApi( dto );
        }
    }

    public DescriptionOfWorkToDo? SyncOnly( int data, int workFrequency, int syncWorkLevel ) {
        _numWorkResults += someSyncWork( syncWorkLevel );
        if ( data % workFrequency == 0 ) {
            return new DescriptionOfWorkToDo { Payload = new LargeDto() };
        }
        return null;
    }
}

file class DescriptionOfWorkToDo {
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    public Exception? Exception { get; init; }
    public object?    Payload   { get; init; }
}

file class SomeApi {
    public Task<int> CallApi( LargeDto dto ) {
        return Task.FromResult( dto.Prop10 + 1 );
    }
}