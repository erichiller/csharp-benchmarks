using System.Diagnostics;

using Benchmarks.LanguageTests;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace LanguageTests;

public class AsyncTests : TestBase<AsyncTests> {
    /// <inheritdoc />
    public AsyncTests( ITestOutputHelper? output, ILogger? logger = null ) : base( output, logger ) { }

    private static async Task doSomeWork( ) => await Task.Delay( 1000 );

    /* Results
        Inline x3 took: "00:00:03.0001159"
        Await After Task start x3 took: "00:00:01.0149079"
        WaitAll with inline tasks x3 took: "00:00:01.0027456"
        WaitAll After Task start x3 took: "00:00:00.9986269"
        WhenAll with inline tasks x3 took: "00:00:00.9976685"
     */
    // see: https://alt-dev.com.au/why-you-may-be-doing-async-wrong-in-c
    [ Fact ]
    public async Task InlineAsyncShouldBeSlowerThanSplit( ) {
        Stopwatch stopwatch = new Stopwatch();

        stopwatch.Restart();
        await doSomeWork();
        await doSomeWork();
        await doSomeWork();
        this._logger.LogInformation( "Inline x3 took: {Stopwatch}", stopwatch.Elapsed );

        stopwatch.Restart();
        var t1 = doSomeWork();
        var t2 = doSomeWork();
        var t3 = doSomeWork();
        await t1;
        await t2;
        await t3;
        this._logger.LogInformation( "Await After Task start x3 took: {Stopwatch}", stopwatch.Elapsed );


        stopwatch.Restart();
        Task.WaitAll( doSomeWork(), doSomeWork(), doSomeWork() );
        this._logger.LogInformation( "WaitAll with inline tasks x3 took: {Stopwatch}", stopwatch.Elapsed );

        stopwatch.Restart();
        var t1B = doSomeWork();
        var t2B = doSomeWork();
        var t3B = doSomeWork();
        Task.WaitAll( t1B, t2B, t3B );
        this._logger.LogInformation( "WaitAll After Task start x3 took: {Stopwatch}", stopwatch.Elapsed );

        stopwatch.Restart();
        await Task.WhenAll( doSomeWork(), doSomeWork(), doSomeWork() );
        this._logger.LogInformation( "WhenAll with inline tasks x3 took: {Stopwatch}", stopwatch.Elapsed );
    }
}

public class AsyncEnumeratorTests : TestBase<AsyncEnumeratorTests> {
    /// <inheritdoc />
    public AsyncEnumeratorTests( ITestOutputHelper? output, ILogger? logger = null ) : base( output, logger ) { }

    [ Fact ]
    public async Task PlainAsyncEnumerators( ) {
        CancellationTokenSource cts       = new CancellationTokenSource( 10_000 );
        var                     e100      = getAsyncEnumerableWithDelay( 10, 100 ).GetAsyncEnumerator( cts.Token );
        var                     last100   = -1;
        var                     e500      = getAsyncEnumerableWithDelay( 5, 500 ).GetAsyncEnumerator( cts.Token );
        var                     last500   = -1;
        var                     e1000     = getAsyncEnumerableWithDelay( 5, 1_000 ).GetAsyncEnumerator( cts.Token );
        var                     last1000  = -1;
        var                     stopwatch = Stopwatch.StartNew();
        while ( !cts.IsCancellationRequested ) {
            if ( await e100.MoveNextAsync() ) {
                last100 = e100.Current;
                if ( last100 == 10 ) {
                    break;
                }
            }
            if ( await e500.MoveNextAsync() ) {
                last500 = e500.Current;
                if ( last500 == 5 ) {
                    break;
                }
            }
            if ( await e1000.MoveNextAsync() ) {
                last1000 = e1000.Current;
                if ( last1000 == 5 ) {
                    break;
                }
            }
        }
        stopwatch.Stop();
        _logger.LogInformation( "Completed in {Time}ms. Last received e100={E100Last}, e500={E500Last}, e1000={E10000Last}", stopwatch.ElapsedMilliseconds, last100, last500, last1000 );
    }

    [ Fact ]
    public async Task CheckMultipleAsyncEnumeratorsUsingTasks( ) {
        CancellationTokenSource cts      = new CancellationTokenSource( 8_000 );
        var                     e100     = getAsyncEnumerableWithDelay( 10, 100 ).GetAsyncEnumerator( cts.Token );
        var                     last100  = -1;
        var                     e500     = getAsyncEnumerableWithDelay( 5, 500 ).GetAsyncEnumerator( cts.Token );
        var                     last500  = -1;
        var                     e1000    = getAsyncEnumerableWithDelay( 5, 1_000 ).GetAsyncEnumerator( cts.Token );
        var                     last1000 = -1;


        printThreadInfo( this._logger );

        Task task100 = Task.Run( async ( ) => {
            while ( !cts.IsCancellationRequested ) {
                if ( await e100.MoveNextAsync() ) {
                    last100 = e100.Current;
                    printThreadInfo( this._logger );
                    if ( last100 == 10 ) {
                        break;
                    }
                }
            }
        } );
        Task task500 = Task.Run( async ( ) => {
            while ( !cts.IsCancellationRequested ) {
                if ( await e500.MoveNextAsync() ) {
                    last500 = e500.Current;
                    printThreadInfo( this._logger );
                    if ( last500 == 5 ) {
                        break;
                    }
                }
            }
        } );
        Task task1000 = Task.Run( async ( ) => {
            while ( !cts.IsCancellationRequested ) {
                if ( await e1000.MoveNextAsync() ) {
                    last1000 = e1000.Current;
                    printThreadInfo( this._logger );
                    if ( last1000 == 5 ) {
                        break;
                    }
                }
            }
        } );


        var stopwatch = Stopwatch.StartNew();
        printThreadInfo( this._logger );
        await Task.WhenAll( task100, task500, task1000 );

        printThreadInfo( this._logger );
        stopwatch.Stop();
        _logger.LogInformation( "Completed in {Time}ms. Last received e100={E100Last}, e500={E500Last}, e1000={E10000Last}", stopwatch.ElapsedMilliseconds, last100, last500, last1000 );

        
        static void printThreadInfo( ILogger logger ) {
            ThreadPool.GetAvailableThreads( out int workerThreads, out int completionPortThreads );
            logger.LogInformation(
                $$"""
                  {ThreadID}
                  {{nameof(ThreadPool.ThreadCount)}}: {ThreadCount}
                  {{nameof(ThreadPool.CompletedWorkItemCount)}}: {CompletedWorkItemCount}
                  {{nameof(ThreadPool.PendingWorkItemCount)}}: {PendingWorkItems}
                  {{nameof(ThreadPool.GetAvailableThreads)}}: {WorkerThreads}, {CompletionPortThreads}
                  """,
                Thread.CurrentThread.ManagedThreadId,
                ThreadPool.ThreadCount,
                ThreadPool.CompletedWorkItemCount,
                ThreadPool.PendingWorkItemCount,
                workerThreads, completionPortThreads);
        }
    }

    /*
     * NOTE: the below works in normal run, locks up in Debug (Rider)
     */
    // [ Fact ]
    // public async Task CheckMultipleAsyncEnumerators( ) {
    //     CancellationTokenSource cts       = new CancellationTokenSource( 6_000 );
    //     var                     e100      = getAsyncEnumerableWithDelay( 10, 100 ).GetAsyncEnumerator( cts.Token );
    //     ValueTask<bool>?        move100   = e100.MoveNextAsync();
    //     var                     last100   = -1;
    //     var                     e500      = getAsyncEnumerableWithDelay( 5, 500 ).GetAsyncEnumerator( cts.Token );
    //     ValueTask<bool>?        move500   = e500.MoveNextAsync();
    //     var                     last500   = -1;
    //     var                     e1000     = getAsyncEnumerableWithDelay( 5, 1_000 ).GetAsyncEnumerator( cts.Token );
    //     ValueTask<bool>?        move1000  = e1000.MoveNextAsync();
    //     var                     last1000  = -1;
    //     var                     stopwatch = Stopwatch.StartNew();
    //     while ( !cts.IsCancellationRequested ) {
    //         if ( move100 is { IsCompleted: true } vt100 ) {
    //             if ( !await vt100 ) {
    //                 move100 = null;
    //             } else {
    //                 last100 = e100.Current;
    //                 // if ( last100 < 10 ) {
    //                 //     break;
    //                 // }
    //                 move100 = e100.MoveNextAsync();
    //             }
    //         }
    //         if ( move500 is { IsCompleted: true } vt500 ) {
    //             if ( !await vt500 ) {
    //                 move500 = null;
    //             } else {
    //                 last500 = e500.Current;
    //                 // if ( last500 == 5 ) {
    //                 //     break;
    //                 // }
    //                 move500 = e500.MoveNextAsync();
    //             }
    //         }
    //         if ( move1000 is { IsCompleted: true } vt1000 ) {
    //             if ( !await vt1000 ) {
    //                 move1000 = null;
    //             } else {
    //                 last1000 = e1000.Current;
    //                 // if ( last1000 == 5 ) {
    //                 //     break;
    //                 // }
    //                 move1000 = e1000.MoveNextAsync();
    //             }
    //         }
    //         if ( ( move100, move500, move1000 ) is (null, null, null) ) {
    //             break;
    //         }
    //     }
    //     stopwatch.Stop();
    //     _logger.LogInformation( "Completed in {Time}ms. Last received e100={E100Last}, e500={E500Last}, e1000={E10000Last}", stopwatch.ElapsedMilliseconds, last100, last500, last1000 );
    // }

    private static async IAsyncEnumerable<int> getAsyncEnumerableWithDelay( int maxNumber, int msDelay ) {
        for ( var i = 1 ; i <= maxNumber ; i++ ) {
            await Task.Delay( msDelay );
            yield return i;
        }
    }
}