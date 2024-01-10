using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

// ReSharper disable ForCanBeConvertedToForeach
namespace Benchmarks.General;

/*
 *
   | Method                | TaskCount | Mean      | Error     | StdDev    | Median    | Ratio | 
   |---------------------- |---------- |----------:|----------:|----------:|----------:|------:|-
   | LockInt32SetTasks     | 1         | 211.30 ms |  0.441 ms |  0.391 ms | 211.17 ms |  1.00 | 
   | LocalInt32Set         | 1         |  61.43 ms |  0.217 ms |  0.193 ms |  61.36 ms |  0.29 | 
   | SemaphoreSlimInt32Set | 1         | 473.81 ms |  0.198 ms |  0.185 ms | 473.77 ms |  2.24 | 
   | InterlockedInt32Set   | 1         |  88.61 ms |  0.591 ms |  0.553 ms |  88.23 ms |  0.42 | 
   | VolatileInt32Set      | 1         |  61.37 ms |  0.079 ms |  0.070 ms |  61.36 ms |  0.29 | 
   |                       |           |           |           |           |           |       | 
   | LockInt32SetTasks     | 2         | 318.28 ms |  5.877 ms |  5.497 ms | 317.83 ms |  1.00 | 
   | LocalInt32Set         | 2         | 274.43 ms | 10.742 ms | 31.674 ms | 290.11 ms |  0.86 | 
   | SemaphoreSlimInt32Set | 2         | 822.57 ms | 16.331 ms | 27.285 ms | 828.34 ms |  2.60 | 
   | InterlockedInt32Set   | 2         | 406.61 ms |  9.282 ms | 27.221 ms | 410.80 ms |  1.30 | 
   | VolatileInt32Set      | 2         | 286.92 ms | 10.245 ms | 30.207 ms | 292.14 ms |  0.90 | 
   |                       |           |           |           |           |           |       | 
   | LockInt32SetTasks     | 3         | 354.84 ms |  6.870 ms |  9.172 ms | 355.10 ms |  1.00 | 
   | LocalInt32Set         | 3         | 281.04 ms |  4.836 ms |  4.039 ms | 282.12 ms |  0.79 | 
   | SemaphoreSlimInt32Set | 3         | 752.53 ms | 14.920 ms | 37.432 ms | 763.40 ms |  2.15 | 
   | InterlockedInt32Set   | 3         | 374.44 ms |  7.713 ms | 22.741 ms | 383.30 ms |  1.05 | 
   | VolatileInt32Set      | 3         | 321.70 ms |  6.297 ms |  6.185 ms | 323.50 ms |  0.90 | 
   |                       |           |           |           |           |           |       | 
   | LockInt32SetTasks     | 4         | 360.02 ms |  7.025 ms |  9.378 ms | 362.86 ms |  1.00 | 
   | LocalInt32Set         | 4         | 284.44 ms |  7.275 ms | 21.450 ms | 293.12 ms |  0.75 | 
   | SemaphoreSlimInt32Set | 4         | 590.15 ms | 11.456 ms | 19.454 ms | 589.23 ms |  1.65 | 
   | InterlockedInt32Set   | 4         | 396.22 ms | 14.791 ms | 43.611 ms | 402.93 ms |  1.07 | 
   | VolatileInt32Set      | 4         | 321.97 ms |  6.402 ms |  8.763 ms | 324.66 ms |  0.89 | 
   |                       |           |           |           |           |           |       | 
   | LockInt32SetTasks     | 5         | 282.41 ms |  9.599 ms | 28.303 ms | 272.44 ms |  1.00 | 
   | LocalInt32Set         | 5         | 267.60 ms |  5.244 ms |  7.179 ms | 268.72 ms |  0.99 | 
   | SemaphoreSlimInt32Set | 5         | 558.09 ms | 10.976 ms | 16.428 ms | 557.32 ms |  2.06 | 
   | InterlockedInt32Set   | 5         | 470.41 ms |  9.385 ms | 16.189 ms | 476.20 ms |  1.70 | 
   | VolatileInt32Set      | 5         | 310.49 ms |  6.407 ms | 18.891 ms | 316.40 ms |  1.11 | 
   
 */

// [ Config( typeof(BenchmarkConfig) ) ]
// [ GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory ) ]
[ SuppressMessage( "Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Cleanup is performed in the GlobalCleanupAttribute marked method" ) ]
[ HideColumns( "RatioSD" ) ]
public class SynchronizationSetBenchmarks {
    [ Params( 1, 2, 3, 4, 5 ) ]
    public int TaskCount { get; set; }

    [ SuppressMessage( "ReSharper", "MemberCanBePrivate.Global" ) ]
    public int RunLimit { get; set; } = 10_000_000;

    private volatile int   _int32Volatile;
    private          int   _counterInt32;
    private          int   _int32;
    private          int[] _taskRunCounts = Array.Empty<int>();

    private readonly object        _lockObject    = new ();
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim( 1, 1 );

    [ IterationSetup ]
    public void Setup( ) {
        _counterInt32  = 0;
        _int32Volatile = 0;
        _int32         = 0;
        _taskRunCounts = new int[ TaskCount ];
        Array.Fill( _taskRunCounts, 0 );
    }

    // [ IterationCleanup ]
    [ GlobalCleanup ]
    public void Cleanup( ) {
        _semaphoreSlim.Dispose();
    }

    private int doWork( Func<int> function ) {
        List<Task<int>> tasks = new ();
        for ( int i = 0 ; i < TaskCount ; i++ ) {
            tasks.Add( new Task<int>( function
                           // , creationOptions: TaskCreationOptions.LongRunning
                       ) );
        }
        for ( int i = 0 ; i < TaskCount ; i++ ) {
            tasks[ i ].Start();
        }
        Task.WhenAll( tasks ).Wait();

        int sumTaskResults = 0;
        for ( int i = 0 ; i < tasks.Count ; i++ ) {
            if ( !tasks[ i ].IsCompletedSuccessfully ) {
                throw new Exception( $"Task failed: {tasks[ i ].Exception}" );
            }
            var taskResult = tasks[ i ].Result;
            if ( taskResult == 0 ) {
                throw new Exception( $"taskResult[{i}]={taskResult} is 0" );
            }
            sumTaskResults += taskResult;
        }
        if ( sumTaskResults < RunLimit ) {
            throw new Exception( $"sumTaskResults={sumTaskResults} < RunLimit={RunLimit}" );
        }

        var counter = this._counterInt32;

        if ( counter < RunLimit ) {
            throw new Exception( $"totalRuns: {counter} < RunLimit={RunLimit}" );
        }

        if ( counter != sumTaskResults ) {
            throw new Exception( $"counter {counter} != sumTaskResults={sumTaskResults}" );
        }

        return _counterInt32;
    }


    [ Benchmark( Baseline = true ) ]
    public int LockInt32SetTasks( ) {
        return doWork( lockUpdateLoop );

        int lockUpdateLoop( ) {
            int localRunCount = 0;
            do {
                lock ( _lockObject ) {
                    this._int32 = localRunCount;
                }
                localRunCount++;
            } while ( Interlocked.Increment( ref _counterInt32 ) - 1 < RunLimit );
            return localRunCount;
        }
    }

    [ Benchmark ]
    public int LocalInt32Set( ) {
        return doWork( loop );

        int loop( ) {
            int localRunCount = 0;
            int value;
            do {
                localRunCount++;
                value = localRunCount;
            } while ( Interlocked.Increment( ref _counterInt32 ) - 1 < RunLimit );
            return value;
        }
    }

    [ Benchmark ]
    public int SemaphoreSlimInt32Set( ) {
        return doWork( semaphoreSlimSyncWaitUpdateLoop );

        int semaphoreSlimSyncWaitUpdateLoop( ) {
            int localRunCount = 0;
            do {
                _semaphoreSlim.Wait();
                try {
                    this._int32 = localRunCount;
                } finally {
                    _semaphoreSlim.Release();
                }
                localRunCount++;
            } while ( Interlocked.Increment( ref _counterInt32 ) - 1 < RunLimit );
            return localRunCount;
        }
    }

    [ Benchmark ]
    public int InterlockedInt32Set( ) {
        return doWork( updateLoop );

        int updateLoop( ) {
            int localRunCount = 0;
            do {
                Interlocked.Exchange( ref _int32, localRunCount );
                localRunCount++;
            } while ( Interlocked.Increment( ref _counterInt32 ) - 1 < RunLimit );
            return localRunCount;
        }
    }

    [ Benchmark ]
    public int VolatileInt32Set( ) {
        return doWork( loop );

        int loop( ) {
            int localRunCount = 0;
            do {
                _int32Volatile = localRunCount;
                localRunCount++;
            } while ( Interlocked.Increment( ref _counterInt32 ) - 1 < RunLimit );
            return localRunCount;
        }
    }
}