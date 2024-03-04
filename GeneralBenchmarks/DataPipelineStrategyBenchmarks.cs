using System;
using System.Threading;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.General;

/*
 * | Method                                     | Mean [us] | Error [us] | StdDev [us] | Gen0    | Allocated [B] |
   |------------------------------------------- |----------:|-----------:|------------:|--------:|--------------:|
   | Wait_DirectCall                            |  138.6 us |    0.21 us |     0.20 us |       - |             - |
   | Wait_WorkReturningValueTask                |  139.7 us |    0.07 us |     0.05 us |       - |             - |
   | Wait_EventCall                             |  141.3 us |    0.39 us |     0.36 us |  2.4414 |       12064 B |
   | Wait_WorkReturningTask                     |  149.0 us |    0.50 us |     0.42 us |  1.7090 |        8064 B |
   | Wait_SendAndForgetTask                     |  235.8 us |    4.69 us |    13.00 us | 16.1133 |       76000 B |
   | Wait_WorkReturningValueTaskWithThreadYield |  470.5 us |    1.31 us |     1.09 us |       - |             - |
   | Wait_WorkReturningTaskWithThreadYield      |  472.9 us |    0.34 us |     0.26 us |  1.4648 |        8064 B |


   | Method                                     | Mean [ms] | Error [ms] | StdDev [ms] | Ratio | RatioSD | Gen0     | Allocated [B] | Alloc Ratio |
   |------------------------------------------- |----------:|-----------:|------------:|------:|--------:|---------:|--------------:|------------:|
   | Wait_DirectCall                            |  1.392 ms |  0.0094 ms |   0.0088 ms |  1.00 |    0.00 |        - |           1 B |        1.00 |
   | Wait_WorkReturningValueTask                |  1.409 ms |  0.0005 ms |   0.0005 ms |  1.01 |    0.01 |        - |           1 B |        1.00 |
   | Wait_EventCall                             |  1.425 ms |  0.0015 ms |   0.0012 ms |  1.02 |    0.01 |  25.3906 |      120065 B |  120,065.00 |
   | Wait_WorkReturningTask                     |  1.499 ms |  0.0015 ms |   0.0014 ms |  1.08 |    0.01 |  15.6250 |       80065 B |   80,065.00 |
   | Wait_SendAndForgetTask                     |  2.383 ms |  0.0559 ms |   0.1520 ms |  1.76 |    0.12 | 160.1563 |      760000 B |  760,000.00 |
   | Wait_WorkReturningValueTaskWithThreadYield |  4.675 ms |  0.0105 ms |   0.0098 ms |  3.36 |    0.02 |        - |           6 B |        6.00 |
   | Wait_WorkReturningTaskWithThreadYield      |  4.797 ms |  0.0080 ms |   0.0071 ms |  3.44 |    0.02 |  15.6250 |       80070 B |   80,070.00 |


   _iterations = 1;
   | Method                                     | WorkDelayMilliseconds | Mean [ms] | Error [ms] | StdDev [ms] | Ratio | Allocated [B] | Alloc Ratio |
   |------------------------------------------- |---------------------- |----------:|-----------:|------------:|------:|--------------:|------------:|
   | Wait_SendAndForgetTask                     | 25                    |  25.44 ms |   0.105 ms |    0.098 ms |  0.20 |         792 B |        4.30 |
   | Wait_WorkReturningTask                     | 25                    |  27.97 ms |   0.015 ms |    0.013 ms |  0.22 |        1752 B |        9.52 |
   | Wait_WorkReturningTaskWithThreadYield      | 25                    |  27.97 ms |   0.021 ms |    0.018 ms |  0.22 |        1752 B |        9.52 |
   | Wait_EventCall                             | 25                    | 125.59 ms |   0.036 ms |    0.032 ms |  1.00 |         368 B |        2.00 |
   | Wait_DirectCall                            | 25                    | 125.62 ms |   0.050 ms |    0.047 ms |  1.00 |         184 B |        1.00 |
   | Wait_WorkReturningValueTask                | 25                    | 139.80 ms |   0.352 ms |    0.330 ms |  1.11 |        1840 B |       10.00 |
   | Wait_WorkReturningValueTaskWithThreadYield | 25                    | 139.81 ms |   0.266 ms |    0.249 ms |  1.11 |        1840 B |       10.00 |

   _iterations = 5;

   | Method                                     | WorkDelayMilliseconds | Mean [ms] | Error [ms] | StdDev [ms] | Ratio | Allocated [B] | Alloc Ratio |
   |------------------------------------------- |---------------------- |----------:|-----------:|------------:|------:|--------------:|------------:|
   | Wait_SendAndForgetTask                     | 10                    |  51.82 ms |   0.283 ms |    0.265 ms |  0.20 |        3902 B |       10.60 |
   | Wait_WorkReturningTaskWithThreadYield      | 10                    |  59.86 ms |   0.134 ms |    0.125 ms |  0.24 |        7754 B |       21.07 |
   | Wait_WorkReturningTask                     | 10                    |  59.92 ms |   0.153 ms |    0.143 ms |  0.24 |        7754 B |       21.07 |
   | Wait_DirectCall                            | 10                    | 253.09 ms |   0.183 ms |    0.171 ms |  1.00 |         368 B |        1.00 |
   | Wait_EventCall                             | 10                    | 253.29 ms |   0.103 ms |    0.086 ms |  1.00 |        1032 B |        2.80 |
   | Wait_WorkReturningValueTask                | 10                    | 299.69 ms |   0.386 ms |    0.343 ms |  1.18 |        7696 B |       20.91 |
   | Wait_WorkReturningValueTaskWithThreadYield | 10                    | 299.90 ms |   0.630 ms |    0.590 ms |  1.18 |        7696 B |       20.91 |
   |                                            |                       |           |            |             |       |               |             |
   | Wait_WorkReturningTask                     | 20                    | 101.81 ms |   0.201 ms |    0.188 ms |  0.20 |        7845 B |       10.66 |
   | Wait_WorkReturningTaskWithThreadYield      | 20                    | 101.84 ms |   0.168 ms |    0.157 ms |  0.20 |        7845 B |       10.66 |
   | Wait_SendAndForgetTask                     | 20                    | 101.86 ms |   0.246 ms |    0.230 ms |  0.20 |        4005 B |        5.44 |
   | Wait_EventCall                             | 20                    | 502.25 ms |   0.150 ms |    0.125 ms |  1.00 |        1400 B |        1.90 |
   | Wait_DirectCall                            | 20                    | 502.29 ms |   0.208 ms |    0.195 ms |  1.00 |         736 B |        1.00 |
   | Wait_WorkReturningValueTask                | 20                    | 507.30 ms |   2.042 ms |    1.910 ms |  1.01 |        8208 B |       11.15 |
   | Wait_WorkReturningValueTaskWithThreadYield | 20                    | 507.31 ms |   3.001 ms |    2.807 ms |  1.01 |        8208 B |       11.15 |
   
   ****

   | Method                                            | WorkDelayMilliseconds | Mean [ms] | Error [ms] | StdDev [ms] | Ratio | Allocated [B] | Alloc Ratio |
   |-------------------------------------------------- |---------------------- |----------:|-----------:|------------:|------:|--------------:|------------:|
   | Wait_SendAndForgetTask_Static                     | 10                    |  51.75 ms |   0.240 ms |    0.213 ms |  0.20 |        2206 B |        5.99 |
   | Wait_SendAndForgetTask                            | 10                    |  51.83 ms |   0.279 ms |    0.261 ms |  0.20 |        3998 B |       10.86 |
   | Wait_WorkReturningTaskWithThreadYield             | 10                    |  59.88 ms |   0.101 ms |    0.089 ms |  0.24 |        7754 B |       21.07 |
   | Wait_WorkReturningTaskWithThreadYield_Static      | 10                    |  59.96 ms |   0.180 ms |    0.160 ms |  0.24 |        7546 B |       20.51 |
   | Wait_WorkReturningTask                            | 10                    |  59.97 ms |   0.184 ms |    0.172 ms |  0.24 |        7754 B |       21.07 |
   | Wait_EventCall                                    | 10                    | 253.18 ms |   0.218 ms |    0.193 ms |  1.00 |        1032 B |        2.80 |
   | Wait_DirectCall                                   | 10                    | 253.21 ms |   0.139 ms |    0.130 ms |  1.00 |         368 B |        1.00 |
   | Wait_WorkReturningValueTaskWithThreadYield        | 10                    | 299.46 ms |   0.426 ms |    0.356 ms |  1.18 |        7696 B |       20.91 |
   | Wait_WorkReturningValueTask                       | 10                    | 299.67 ms |   0.704 ms |    0.658 ms |  1.18 |        7696 B |       20.91 |
   | Wait_WorkReturningValueTaskWithThreadYield_Static | 10                    | 299.78 ms |   0.756 ms |    0.670 ms |  1.18 |        7488 B |       20.35 |
   |                                                   |                       |           |            |             |       |               |             |
   | Wait_WorkReturningTaskWithThreadYield_Static      | 20                    | 101.57 ms |   0.370 ms |    0.346 ms |  0.20 |        7637 B |       10.38 |
   | Wait_WorkReturningTaskWithThreadYield             | 20                    | 101.60 ms |   0.580 ms |    0.543 ms |  0.20 |        7845 B |       10.66 |
   | Wait_SendAndForgetTask_Static                     | 20                    | 101.89 ms |   0.330 ms |    0.309 ms |  0.20 |        2405 B |        3.27 |
   | Wait_SendAndForgetTask                            | 20                    | 101.98 ms |   0.242 ms |    0.227 ms |  0.20 |        4005 B |        5.44 |
   | Wait_WorkReturningTask                            | 20                    | 103.28 ms |   1.553 ms |    1.377 ms |  0.21 |        7845 B |       10.66 |
   | Wait_DirectCall                                   | 20                    | 503.23 ms |   0.129 ms |    0.120 ms |  1.00 |         736 B |        1.00 |
   | Wait_EventCall                                    | 20                    | 503.33 ms |   0.125 ms |    0.117 ms |  1.00 |        1400 B |        1.90 |
   | Wait_WorkReturningValueTask                       | 20                    | 506.52 ms |   2.923 ms |    2.734 ms |  1.01 |        8208 B |       11.15 |
   | Wait_WorkReturningValueTaskWithThreadYield_Static | 20                    | 506.94 ms |   2.481 ms |    2.321 ms |  1.01 |        8000 B |       10.87 |
   | Wait_WorkReturningValueTaskWithThreadYield        | 20                    | 507.47 ms |   2.053 ms |    1.920 ms |  1.01 |        8208 B |       11.15 |
   

 */

[ Config( typeof(BenchmarkConfig) ) ]
public class DataPipelineStrategyBenchmarks : IDisposable {
    // private const int _iterations = 1_000;
    private const int _iterations = 5;
    private const int _innerLoop  = 5;

    // [Params(500,5000,)]
    // public int WorkLoops { get; set; }


    // [Params(0, 25, 50)]
    // [Params(25, 50)]
    [ Params( 10, 20 ) ] public int WorkDelayMilliseconds { get => _workDelayMilliseconds; set => _workDelayMilliseconds = value; } // emulate I/O work
    private static              int _workDelayMilliseconds = 0;

    /*
     *
     */

    [ GlobalSetup ]
    public void Setup( ) {
        _greaterThanOneCount = 0;
    }

    private static readonly CountdownEvent _countdownEvent = new CountdownEvent( _innerLoop );

    private static int _greaterThanOneCount = 0;

    private void work( long number ) {
        Thread.Sleep( WorkDelayMilliseconds );
        for ( int i = 0 ; i < 500 ; i++ ) {
            number += number + i;
        }
        if ( number > 1 ) {
            _greaterThanOneCount++;
        }
    }

    private event EventHandler<NumberEventArgs>? _numberEvent;

    private class NumberEventArgs : EventArgs {
        public long Number { get; init; }
    }

    private void work( object? sender, NumberEventArgs numberEvent ) {
        Thread.Sleep( WorkDelayMilliseconds );
        long number = numberEvent.Number;
        for ( int i = 0 ; i < 500 ; i++ ) {
            number += number + i;
        }
        if ( number > 1 ) {
            _greaterThanOneCount++;
        }
    }

    private void workAsConstructedTask( object? obj ) {
        Thread.Sleep( WorkDelayMilliseconds );
        if ( obj is not long number ) {
            throw new ArgumentException( null, nameof(obj) );
        }
        for ( int i = 0 ; i < 500 ; i++ ) {
            number += number + i;
        }
        if ( number > 1 ) {
            _greaterThanOneCount++;
        }
        _countdownEvent.Signal();
    }

    private static void workAsConstructedTaskStatic( object? obj ) {
        Thread.Sleep( _workDelayMilliseconds );
        if ( obj is not long number ) {
            throw new ArgumentException( null, nameof(obj) );
        }
        for ( int i = 0 ; i < 500 ; i++ ) {
            number += number + i;
        }
        if ( number > 1 ) {
            _greaterThanOneCount++;
        }
        _countdownEvent.Signal();
    }

    private async Task workReturningTask( long number ) {
        await Task.Delay( WorkDelayMilliseconds );
        for ( int i = 0 ; i < 500 ; i++ ) {
            number += number + i;
        }
        if ( number > 1 ) {
            _greaterThanOneCount++;
        }
    }

    private async Task workReturningTaskWithThreadYield( long number ) {
        Thread.Yield();
        await Task.Delay( WorkDelayMilliseconds );
        for ( int i = 0 ; i < 500 ; i++ ) {
            number += number + i;
        }
        if ( number > 1 ) {
            _greaterThanOneCount++;
        }
    }

    private static async Task workReturningTaskWithThreadYieldStatic( long number ) {
        Thread.Yield();
        await Task.Delay( _workDelayMilliseconds );
        for ( int i = 0 ; i < 500 ; i++ ) {
            number += number + i;
        }
        if ( number > 1 ) {
            _greaterThanOneCount++;
        }
    }

    private async ValueTask workReturningValueTask( long number ) {
        await Task.Delay( WorkDelayMilliseconds );
        for ( int i = 0 ; i < 500 ; i++ ) {
            number += number + i;
        }
        if ( number > 1 ) {
            _greaterThanOneCount++;
        }
    }

    private async ValueTask workReturningValueTaskWithThreadYield( long number ) {
        Thread.Yield();
        await Task.Delay( WorkDelayMilliseconds );
        for ( int i = 0 ; i < 500 ; i++ ) {
            number += number + i;
        }
        if ( number > 1 ) {
            _greaterThanOneCount++;
        }
    }

    private static async ValueTask workReturningValueTaskWithThreadYieldStatic( long number ) {
        Thread.Yield();
        await Task.Delay( _workDelayMilliseconds );
        for ( int i = 0 ; i < 500 ; i++ ) {
            number += number + i;
        }
        if ( number > 1 ) {
            _greaterThanOneCount++;
        }
    }

    /*
     *
     */

    [ Benchmark( Baseline = true /* , OperationsPerInvoke =  */ ) ]
    [ BenchmarkCategory( "WaitForCompletion" ) ]
    public int Wait_DirectCall( ) {
        int i;
        for ( i = 0 ; i < _iterations ; i++ ) {
            for ( int loop = 0 ; loop < _innerLoop ; loop++ ) {
                work( Random.Shared.Next( -10_000, 10_000 ) );
            }
        }
        return _greaterThanOneCount;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "WaitForCompletion" ) ]
    public int Wait_EventCall( ) {
        _numberEvent += work;
        for ( var i = 0 ; i < _iterations ; i++ ) {
            for ( int loop = 0 ; loop < _innerLoop ; loop++ ) {
                this._numberEvent.Invoke( this, new NumberEventArgs() { Number = Random.Shared.Next( -10_000, 10_000 ) } );
            }
        }
        _numberEvent = null;
        return _greaterThanOneCount;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "WaitForCompletion", "AsyncOptimum" ) ]
    public int Wait_SendAndForgetTask( ) {
        for ( var i = 0 ; i < _iterations ; i++ ) {
            _countdownEvent.Reset();
            for ( int loop = 0 ; loop < _innerLoop ; loop++ ) {
                new Task( workAsConstructedTask, ( long )Random.Shared.Next( -10_000, 10_000 ) ).Start();
            }
            _countdownEvent.Wait();
        }

        return _greaterThanOneCount;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "WaitForCompletion", "AsyncOptimum" ) ]
    public int Wait_SendAndForgetTask_Static( ) {
        for ( var i = 0 ; i < _iterations ; i++ ) {
            _countdownEvent.Reset();
            for ( int loop = 0 ; loop < _innerLoop ; loop++ ) {
                new Task( workAsConstructedTaskStatic, ( long )Random.Shared.Next( -10_000, 10_000 ) ).Start();
            }
            _countdownEvent.Wait();
        }

        return _greaterThanOneCount;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "WaitForCompletion", "AsyncOptimum" ) ]
    public async Task<int> Wait_WorkReturningTask( ) {
        Task[] tasks = new Task[ _innerLoop ];
        for ( var i = 0 ; i < _iterations ; i++ ) {
            for ( int loop = 0 ; loop < _innerLoop ; loop++ ) {
                tasks[ loop ] = workReturningTask( Random.Shared.Next( -10_000, 10_000 ) );
            }
            await Task.WhenAll( tasks );
        }
        return _greaterThanOneCount;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "WaitForCompletion", "AsyncOptimum" ) ]
    public async Task<int> Wait_WorkReturningTaskWithThreadYield( ) {
        Task[] tasks = new Task[ _innerLoop ];
        for ( var i = 0 ; i < _iterations ; i++ ) {
            for ( int loop = 0 ; loop < _innerLoop ; loop++ ) {
                tasks[ loop ] = workReturningTaskWithThreadYield( Random.Shared.Next( -10_000, 10_000 ) );
            }
            await Task.WhenAll( tasks );
        }
        return _greaterThanOneCount;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "WaitForCompletion", "AsyncOptimum" ) ]
    public async Task<int> Wait_WorkReturningTaskWithThreadYield_Static( ) {
        Task[] tasks = new Task[ _innerLoop ];
        for ( var i = 0 ; i < _iterations ; i++ ) {
            for ( int loop = 0 ; loop < _innerLoop ; loop++ ) {
                tasks[ loop ] = workReturningTaskWithThreadYieldStatic( Random.Shared.Next( -10_000, 10_000 ) );
            }
            await Task.WhenAll( tasks );
        }
        return _greaterThanOneCount;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "WaitForCompletion" ) ]
    public async Task<int> Wait_WorkReturningValueTask( ) {
        for ( var i = 0 ; i < _iterations ; i++ ) {
            for ( int loop = 0 ; loop < _innerLoop ; loop++ ) {
                await workReturningValueTask( Random.Shared.Next( -10_000, 10_000 ) );
            }
        }
        return _greaterThanOneCount;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "WaitForCompletion" ) ]
    public async Task<int> Wait_WorkReturningValueTaskWithThreadYield( ) {
        for ( var i = 0 ; i < _iterations ; i++ ) {
            for ( int loop = 0 ; loop < _innerLoop ; loop++ ) {
                await workReturningValueTaskWithThreadYield( Random.Shared.Next( -10_000, 10_000 ) );
            }
        }
        return _greaterThanOneCount;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "WaitForCompletion" ) ]
    public async Task<int> Wait_WorkReturningValueTaskWithThreadYield_Static( ) {
        for ( var i = 0 ; i < _iterations ; i++ ) {
            for ( int loop = 0 ; loop < _innerLoop ; loop++ ) {
                await workReturningValueTaskWithThreadYieldStatic( Random.Shared.Next( -10_000, 10_000 ) );
            }
        }
        return _greaterThanOneCount;
    }


    /*
     *
     */

    public void Dispose( ) {
        _countdownEvent.Dispose();
    }
}