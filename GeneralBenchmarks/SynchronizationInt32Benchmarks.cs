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
   | LockTasks             | 1         | 163.92 ms |  0.112 ms |  0.094 ms | 163.89 ms |  1.00 |
   | CodeOnly              | 1         |  12.43 ms |  0.012 ms |  0.011 ms |           |  0.08 | 
   | CodeOnlyInTask        | 1         |  12.56 ms |  0.070 ms |  0.055 ms |           |  0.08 |
   | SemaphoreSlimTasks    | 1         | 430.73 ms |  2.492 ms |  2.331 ms | 428.77 ms |  2.63 |
   | InterlockedTasks      | 1         |  75.87 ms |  0.038 ms |  0.035 ms |  75.87 ms |  0.46 |
   | LockOnThreads         | 1         | 169.71 ms |  1.259 ms |  1.116 ms | 169.10 ms |  1.04 |
   | LockOnThreads2        | 1         | 169.43 ms |  0.414 ms |  0.367 ms | 169.42 ms |  1.03 |
   | InterlockedOnThreads2 | 1         |  79.04 ms |  0.324 ms |  0.253 ms |  79.05 ms |  0.48 |
   |                       |           |           |           |           |           |       |
   | LockTasks             | 2         | 219.50 ms |  1.440 ms |  1.276 ms | 219.40 ms |  1.00 |
   | SemaphoreSlimTasks    | 2         | 728.02 ms |  2.979 ms |  2.787 ms | 727.40 ms |  3.32 |
   | InterlockedTasks      | 2         | 332.83 ms |  8.456 ms | 24.932 ms | 331.40 ms |  1.53 |
   | LockOnThreads         | 2         | 239.37 ms |  4.593 ms |  6.287 ms | 238.18 ms |  1.10 |
   | LockOnThreads2        | 2         | 231.58 ms |  2.252 ms |  2.107 ms | 230.76 ms |  1.05 |
   | InterlockedOnThreads2 | 2         | 446.02 ms |  8.792 ms | 16.513 ms | 447.77 ms |  2.01 |
   |                       |           |           |           |           |           |       |
   | LockTasks             | 3         | 265.38 ms |  4.966 ms |  4.645 ms | 266.35 ms |  1.00 |
   | SemaphoreSlimTasks    | 3         | 727.21 ms | 14.222 ms | 16.378 ms | 731.87 ms |  2.73 |
   | InterlockedTasks      | 3         | 357.56 ms |  7.094 ms | 15.270 ms | 357.65 ms |  1.38 |
   | LockOnThreads         | 3         | 299.30 ms |  5.714 ms |  5.345 ms | 301.00 ms |  1.13 |
   | LockOnThreads2        | 3         | 287.51 ms |  2.669 ms |  2.497 ms | 287.04 ms |  1.08 |
   | InterlockedOnThreads2 | 3         | 413.51 ms |  8.222 ms | 22.088 ms | 423.84 ms |  1.56 |
   |                       |           |           |           |           |           |       |
   | LockTasks             | 4         | 261.94 ms |  4.206 ms |  3.935 ms | 263.38 ms |  1.00 |
   | SemaphoreSlimTasks    | 4         | 705.56 ms | 12.366 ms | 11.567 ms | 708.89 ms |  2.69 |
   | InterlockedTasks      | 4         | 341.10 ms |  6.764 ms | 10.330 ms | 340.59 ms |  1.29 |
   | LockOnThreads         | 4         | 275.98 ms |  5.264 ms |  6.267 ms | 277.97 ms |  1.05 |
   | LockOnThreads2        | 4         | 280.38 ms |  4.524 ms |  4.232 ms | 282.32 ms |  1.07 |
   | InterlockedOnThreads2 | 4         | 379.72 ms |  6.508 ms |  5.434 ms | 380.11 ms |  1.45 |
   |                       |           |           |           |           |           |       |
   | LockTasks             | 5         | 255.22 ms |  5.092 ms |  7.776 ms | 257.80 ms |  1.00 |
   | SemaphoreSlimTasks    | 5         | 700.30 ms | 13.903 ms | 26.452 ms | 703.69 ms |  2.76 |
   | InterlockedTasks      | 5         | 301.47 ms |  3.319 ms |  2.772 ms | 301.20 ms |  1.19 |
   | LockOnThreads         | 5         | 254.26 ms |  5.075 ms | 11.762 ms | 250.34 ms |  1.03 |
   | LockOnThreads2        | 5         | 275.37 ms |  5.268 ms |  6.469 ms | 276.47 ms |  1.08 |
   | InterlockedOnThreads2 | 5         | 368.94 ms |  2.442 ms |  2.285 ms | 368.90 ms |  1.45 |

 */

// [ Config( typeof(BenchmarkConfig) ) ]
// [ GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory ) ]
[ SuppressMessage( "Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Cleanup is performed in the GlobalCleanupAttribute marked method" ) ]
[ HideColumns( "RatioSD" ) ]
public class SynchronizationInt32Benchmarks {
    [ Params(
        1
        // 2, 3,
        // 4, 5 
    )]
    // [ SuppressMessage( "ReSharper", "UnassignedGetOnlyAutoProperty" ) ]
    // lower number = more async work
    public int TaskCount { get; set; }

    // [ Params( 1_000_000 ) ]
    [ SuppressMessage( "ReSharper", "MemberCanBePrivate.Global" ) ]
    public int RunLimit { get; set; } = 10_000_000;

    // private readonly TaskCreationOptions _taskCreationOptions = TaskCreationOptions.LongRunning;

    // private volatile int   _counterVolatileInt32;
    private int   _counterInt32;
    private int[] _taskRunCounts = Array.Empty<int>();
    private int[] _threadIds     = Array.Empty<int>();

    private readonly object        _lockObject    = new ();
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim( 1, 1 );

    [ IterationSetup ]
    public void Setup( ) {
        _counterInt32 = 0;
        // _counterVolatileInt32 = 0;
        _taskRunCounts = new int[ TaskCount ];
        _threadIds     = new int[ TaskCount ];
        Array.Fill( _taskRunCounts, 0 );
        // nothing for now
    }

    // [ IterationCleanup ]
    [ GlobalCleanup ]
    public void Cleanup( ) {
        _semaphoreSlim.Dispose();
    }


    private int doWorkAsTasks( Func<int> function ) {
        List<Task<int>> tasks = new ();
        for ( int i = 0 ; i < TaskCount ; i++ ) {
            tasks.Add( new Task<int>( function ) );
            // tasks.Add( Task.Factory.StartNew( function: function, creationOptions: _taskCreationOptions ) );
        }
        for ( int i = 0 ; i < TaskCount ; i++ ) {
            tasks[ i ].Start();
        }
        Task.WhenAll( tasks ).Wait();
        // int sumAllReturned = tasks.Sum( t => t.Result );
        // int sumAllArray    = _taskRunCounts.Sum();
        // System.Console.WriteLine( $"{nameof(sumAllReturned)}={sumAllReturned} ; {nameof(sumAllArray)}={sumAllArray} ; {nameof(_totalRuns)}={_totalRuns}" );

        int sumTaskResults = 0;
        for ( int i = 0 ; i < tasks.Count ; i++ ) {
            var taskResult = tasks[ i ].Result;
            if ( taskResult == 0 ) {
                throw new Exception( $"taskResult[{i}]={taskResult} is 0" );
            }
            sumTaskResults += taskResult;
        }
        if ( sumTaskResults < RunLimit ) {
            throw new Exception( $"sumTaskResults={sumTaskResults} < RunLimit={RunLimit}" );
        }

        if ( _counterInt32 < RunLimit ) {
            throw new Exception( $"_totalRuns {_counterInt32} < RunLimit={RunLimit}" );
        }

        if ( _counterInt32 != sumTaskResults ) {
            throw new Exception( $"_counterInt32 {_counterInt32} != sumTaskResults={sumTaskResults}" );
        }

        return _counterInt32;
    }


    private int doWorkOnThreads( Action<UpdateLoopArgs> function ) {
        TaskCompletionSource tcs     = new TaskCompletionSource(); //  creationOptions: TaskCreationOptions.RunContinuationsAsynchronously
        Thread[]             threads = new Thread[ this.TaskCount ];
        for ( int i = 0 ; i < TaskCount ; i++ ) {
            // ThreadPool.QueueUserWorkItem( function );
            int threadId = i;
            threads[ i ] = new Thread( ( ) => function( new (threadId, tcs) ) );
            // threads[ i ] = new Thread(  );
        }
        for ( int i = 0 ; i < threads.Length ; i++ ) {
            threads[ i ].Start();
        }

        tcs.Task.Wait();

        SpinWait waiter = new SpinWait();
        while ( true ) {
            bool isComplete = true;
            for ( int i = 0 ; i < threads.Length ; i++ ) {
                // isComplete &= threads[ i ].IsAlive;
                isComplete &= threads[ i ].ThreadState == ThreadState.Stopped;
                // Console.WriteLine($"ThreadState[{i}] = {threads[i].ThreadState}");
            }
            if ( isComplete ) {
                break;
            }
            // Thread.SpinWait( 100 );
            waiter.SpinOnce();
        }

        int sumTaskResults = 0;
        for ( int i = 0 ; i < threads.Length ; i++ ) {
            sumTaskResults += this._taskRunCounts[ i ];
        }
        if ( sumTaskResults < RunLimit ) {
            throw new Exception( $"sumTaskResults={sumTaskResults} < RunLimit={RunLimit}" );
        }

        if ( _counterInt32 < RunLimit ) {
            throw new Exception( $"_totalRuns {_counterInt32} < RunLimit={RunLimit}" );
        }

        if ( _counterInt32 != sumTaskResults ) {
            throw new Exception( $"_counterInt32 {_counterInt32} != sumTaskResults={sumTaskResults}" );
        }

        return _counterInt32;
    }

    private int doWorkOnThreads2( Action<int> function ) {
        Thread[] threads = new Thread[ this.TaskCount ];
        for ( int i = 0 ; i < TaskCount ; i++ ) {
            // ThreadPool.QueueUserWorkItem( function );
            int threadId = i;
            threads[ i ] = new Thread( ( ) => function( threadId ) );
            // threads[ i ] = new Thread(  );
        }
        for ( int i = 0 ; i < threads.Length ; i++ ) {
            threads[ i ].Start();
        }

        SpinWait waiter = new SpinWait();
        while ( true ) {
            bool isComplete = true;
            for ( int i = 0 ; i < threads.Length ; i++ ) {
                // isComplete &= threads[ i ].IsAlive;
                isComplete &= threads[ i ].ThreadState == ThreadState.Stopped;
                // Console.WriteLine($"ThreadState[{i}] = {threads[i].ThreadState}");
            }
            if ( isComplete ) {
                break;
            }
            // Thread.SpinWait( 100 );
            waiter.SpinOnce();
        }

        int sumTaskResults = 0;
        for ( int i = 0 ; i < threads.Length ; i++ ) {
            sumTaskResults += this._taskRunCounts[ i ];
        }
        if ( sumTaskResults < RunLimit ) {
            throw new Exception( $"sumTaskResults={sumTaskResults} < RunLimit={RunLimit}" );
        }

        if ( _counterInt32 < RunLimit ) {
            throw new Exception( $"_totalRuns {_counterInt32} < RunLimit={RunLimit}" );
        }

        if ( _counterInt32 != sumTaskResults ) {
            throw new Exception( $"_counterInt32 {_counterInt32} != sumTaskResults={sumTaskResults}" );
        }

        return _counterInt32;
    }

    [ Benchmark( Baseline = true ) ]
    public int LockTasks( ) {
        return doWorkAsTasks( lockUpdateLoop );

        int lockUpdateLoop( ) {
            int localRunCount = 0;
            while ( _counterInt32 < RunLimit ) {
                lock ( _lockObject ) {
                    _counterInt32++;
                }
                localRunCount++;
            }
            // Console.WriteLine($"localRunCount={localRunCount}; _counterInt32={_counterInt32}; RunLimit={RunLimit}");
            return localRunCount;
        }
    }

    [ Benchmark ]
    public int CodeOnly( ) {
        int localRunCount = 0;
        while ( _counterInt32 < RunLimit ) {
            _counterInt32++;
            localRunCount++;
        }
        // Console.WriteLine($"localRunCount={localRunCount}; _counterInt32={_counterInt32}; RunLimit={RunLimit}");
        return localRunCount;
    }

    [ Benchmark ]
    public int CodeOnlyInTask( ) {
        return doWorkAsTasks( loop );

        int loop( ) {
            int localRunCount = 0;
            while ( _counterInt32 < RunLimit ) {
                _counterInt32++;
                localRunCount++;
            }
            // Console.WriteLine($"localRunCount={localRunCount}; _counterInt32={_counterInt32}; RunLimit={RunLimit}");
            return localRunCount;
        }
    }

    [ Benchmark ]
    public int SemaphoreSlimTasks( ) {
        return doWorkAsTasks( semaphoreSlimSyncWaitUpdateLoop );

        int semaphoreSlimSyncWaitUpdateLoop( ) {
            int localRunCount = 0;
            while ( _counterInt32 < RunLimit ) {
                _semaphoreSlim.Wait();
                try {
                    _counterInt32++;
                } finally {
                    _semaphoreSlim.Release();
                }
                localRunCount++;
            }
            return localRunCount;
        }
    }

    [ Benchmark ]
    public int InterlockedTasks( ) {
        return doWorkAsTasks( updateLoop );

        int updateLoop( ) {
            int localRunCount = 0;
            while ( _counterInt32 < RunLimit ) {
                Interlocked.Increment( ref _counterInt32 );
                localRunCount++;
            }
            return localRunCount;
        }
    }

    [ Benchmark ]
    public int LockOnThreads( ) {
        return doWorkOnThreads( lockUpdateLoop );

        void lockUpdateLoop( UpdateLoopArgs args ) {
            var (threadId, tcs) = args;
            while ( true ) {
                lock ( _lockObject ) {
                    _counterInt32++;
                    this._taskRunCounts[ threadId ]++;
                    if ( _counterInt32 >= RunLimit ) {
                        // Console.WriteLine("Setting Result");
                        if ( !tcs.Task.IsCompleted ) {
                            tcs.SetResult();
                        }
                        return;
                    }
                }
                // Console.WriteLine($"localRunCount={localRunCount}; _counterInt32={_counterInt32}; RunLimit={RunLimit}");
            }
        }
    }

    [ Benchmark ]
    public int LockOnThreads2( ) {
        return doWorkOnThreads2( lockUpdateLoop );

        void lockUpdateLoop( int threadId ) {
            while ( true ) {
                lock ( _lockObject ) {
                    _counterInt32++;
                    this._taskRunCounts[ threadId ]++;
                    if ( _counterInt32 >= RunLimit ) {
                        return;
                    }
                }
                // Console.WriteLine($"localRunCount={localRunCount}; _counterInt32={_counterInt32}; RunLimit={RunLimit}");
            }
        }
    }

    [ Benchmark ]
    public int InterlockedOnThreads2( ) {
        return doWorkOnThreads2( lockUpdateLoop );

        void lockUpdateLoop( int threadId ) {
            while ( true ) {
                Interlocked.Increment( ref _counterInt32 );
                this._taskRunCounts[ threadId ]++;
                if ( _counterInt32 >= RunLimit ) {
                    return;
                }
                // Console.WriteLine($"localRunCount={localRunCount}; _counterInt32={_counterInt32}; RunLimit={RunLimit}");
            }
        }
    }
    //
    // [ Benchmark ]
    // public int VolatileTasks( ) {
    //     return doWorkAsTasks( semaphoreSlimSyncWaitUpdateLoop );
    //
    //     int semaphoreSlimSyncWaitUpdateLoop( ) {
    //         int localRunCount = 0;
    //         while ( _counterInt32 < RunLimit ) {
    //             _semaphoreSlim.Wait();
    //             try {
    //                 _counterInt32++;
    //             } finally {
    //                 _semaphoreSlim.Release();
    //             }
    //             localRunCount++;
    //         }
    //         return localRunCount;
    //     }
    // }

    private record UpdateLoopArgs( int ThreadId, TaskCompletionSource TaskCompletionSource );
}