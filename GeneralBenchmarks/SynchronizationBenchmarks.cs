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
   | Method        | TaskCount | RunLimit | Mean [ms] | Error [ms] | StdDev [ms] | Ratio | RatioSD | Allocated [B] | Alloc Ratio |
   |-------------- |---------- |--------- |----------:|-----------:|------------:|------:|--------:|--------------:|------------:|
   | Lock          | 1         | 1000000  |  17.18 ms |   0.132 ms |    0.110 ms |  1.00 |    0.00 |        1728 B |        1.00 |
   | SemaphoreSlim | 1         | 1000000  |  43.68 ms |   0.061 ms |    0.051 ms |  2.54 |    0.02 |        1728 B |        1.00 |
   |               |           |          |           |            |             |       |         |               |             |
   | Lock          | 2         | 1000000  |  23.17 ms |   0.452 ms |    1.003 ms |  1.00 |    0.00 |        2184 B |        1.00 |
   | SemaphoreSlim | 2         | 1000000  |  76.08 ms |   1.518 ms |    3.067 ms |  3.29 |    0.21 |        2184 B |        1.00 |
   |               |           |          |           |            |             |       |         |               |             |
   | Lock          | 3         | 1000000  |  26.96 ms |   0.652 ms |    1.912 ms |  1.00 |    0.00 |        2648 B |        1.00 |
   | SemaphoreSlim | 3         | 1000000  |  63.65 ms |   1.262 ms |    3.642 ms |  2.38 |    0.23 |        2648 B |        1.00 |
   |               |           |          |           |            |             |       |         |               |             |
   | Lock          | 4         | 1000000  |  24.48 ms |   0.597 ms |    1.750 ms |  1.00 |    0.00 |        3104 B |        1.00 |
   | SemaphoreSlim | 4         | 1000000  |  54.43 ms |   1.086 ms |    2.451 ms |  2.26 |    0.19 |        3104 B |        1.00 |
   |               |           |          |           |            |             |       |         |               |             |
   | Lock          | 5         | 1000000  |  22.23 ms |   0.489 ms |    1.427 ms |  1.00 |    0.00 |        3656 B |        1.00 |
   | SemaphoreSlim | 5         | 1000000  |  51.61 ms |   1.013 ms |    1.975 ms |  2.34 |    0.16 |        3656 B |        1.00 |
   
 */

// [ Config( typeof(BenchmarkConfig) ) ]
// [ GroupBenchmarksBy( BenchmarkLogicalGroupRule.ByCategory ) ]
[ SuppressMessage( "Design", "CA1001:Types that own disposable fields should be disposable", Justification = "Cleanup is performed in the GlobalCleanupAttribute marked method" ) ]
[HideColumns("RatioSD")]
public class SynchronizationBenchmarks {
    [ Params( 1, 2, 3, 4, 5 ) ]
    // [ SuppressMessage( "ReSharper", "UnassignedGetOnlyAutoProperty" ) ]
    // lower number = more async work
    public int TaskCount { get; set; }

    // [ Params( 1_000_000 ) ]
    public int RunLimit { get; set; } = 1_000_000;

    private readonly TaskCreationOptions _taskCreationOptions = TaskCreationOptions.LongRunning;

    private int   _totalRuns     = 0;
    private int[] _taskRunCounts = System.Array.Empty<int>();

    private readonly object        _lockObject    = new ();
    private readonly SemaphoreSlim _semaphoreSlim = new SemaphoreSlim( 1, 1 );

    [ IterationSetup ]
    public void Setup( ) {
        _totalRuns     = 0;
        _taskRunCounts = new int[ TaskCount ];
        Array.Fill( _taskRunCounts, 0 );
    }

    // [ IterationCleanup ]
    [ GlobalCleanup ]
    public void Cleanup( ) {
        _semaphoreSlim.Dispose();
    }


    private int lockUpdateLoop( int taskId ) {
        int localRunCount = 0;
        while ( _totalRuns < RunLimit ) {
            lock ( _lockObject ) {
                _totalRuns++;
                _taskRunCounts[ taskId ]++;
            }
            localRunCount++;
        }
        return localRunCount;
    }

    private int semaphoreSlimUpdateLoop( int taskId ) {
        int localRunCount = 0;
        while ( _totalRuns < RunLimit ) {
            _semaphoreSlim.Wait();
            try {
                _totalRuns++;
                _taskRunCounts[ taskId ]++;
            } finally {
                _semaphoreSlim.Release();
            }
            localRunCount++;
        }
        return localRunCount;
    }

    [ Benchmark( Baseline = true ) ]
    public int Lock( ) {
        List<Task<int>> tasks = new ();
        for ( int i = 0 ; i < TaskCount ; i++ ) {
            tasks.Add( Task.Factory.StartNew( taskId => lockUpdateLoop( ( int )taskId! ), state: i, _taskCreationOptions ) );
        }
        Task.WhenAll( tasks ).Wait();
        // int sumAllReturned = tasks.Sum( t => t.Result );
        // int sumAllArray    = _taskRunCounts.Sum();
        // System.Console.WriteLine( $"{nameof(sumAllReturned)}={sumAllReturned} ; {nameof(sumAllArray)}={sumAllArray} ; {nameof(_totalRuns)}={_totalRuns}" );
        
        
        int sumTaskRunCounts = 0;
        for ( int i = 0 ; i < this._taskRunCounts.Length ; i++ ) {
            sumTaskRunCounts += _taskRunCounts[ i ];
        }
        if ( sumTaskRunCounts < RunLimit ) {
            throw new Exception("sumTaskRunCounts < RunLimit");
        }

        int sumTaskResults = 0;
        for ( int i = 0 ; i < tasks.Count ; i++ ) {
            sumTaskResults += tasks[ i ].Result;
        }
        if ( sumTaskResults < RunLimit ) {
            throw new Exception("sumTaskResults < RunLimit");
        }
        
        if ( _totalRuns < RunLimit ) {
            throw new Exception("_totalRuns < RunLimit");
        }
        
        return _totalRuns;
    }

    [ Benchmark ]
    public int SemaphoreSlim( ) {
        List<Task<int>> tasks = new ();
        for ( int i = 0 ; i < TaskCount ; i++ ) {
            tasks.Add( Task.Factory.StartNew( taskId => semaphoreSlimUpdateLoop( ( int )taskId! ), state: i, _taskCreationOptions ) );
        }
        Task.WhenAll( tasks ).Wait();
        // int sumAllReturned = tasks.Sum( t => t.Result );
        // int sumAllArray    = _taskRunCounts.Sum();
        // System.Console.WriteLine( $"{nameof(sumAllReturned)}={sumAllReturned} ; {nameof(sumAllArray)}={sumAllArray} ; {nameof(_totalRuns)}={_totalRuns}" );
        
        int sumTaskRunCounts = 0;
        for ( int i = 0 ; i < this._taskRunCounts.Length ; i++ ) {
            sumTaskRunCounts += _taskRunCounts[ i ];
        }
        if ( sumTaskRunCounts < RunLimit ) {
            throw new Exception("sumTaskRunCounts < RunLimit");
        }

        int sumTaskResults = 0;
        for ( int i = 0 ; i < tasks.Count ; i++ ) {
            sumTaskResults += tasks[ i ].Result;
        }
        if ( sumTaskResults < RunLimit ) {
            throw new Exception("sumTaskResults < RunLimit");
        }
        
        if ( _totalRuns < RunLimit ) {
            throw new Exception("_totalRuns < RunLimit");
        }
        return _totalRuns;
    }
}