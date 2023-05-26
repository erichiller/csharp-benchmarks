using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Benchmarks.InterThread.BroadcastQueueTests;

using BroadcastChannel;

using BroadcastChannelMux;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace Benchmarks.InterThread.ChannelMuxTests;

file readonly record struct DataTypeA(
    int  Sequence,
    long WrittenTicks
);

file readonly record struct DataTypeB(
    int  Sequence,
    long WrittenTicks
);

file class SomeException : Exception { }

public class ChannelMuxTests : TestBase<ChannelMuxTests> {
    public ChannelMuxTests( ITestOutputHelper testOutputHelper ) : base( testOutputHelper ) { }


    private static void producerTaskSimple<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in int totalMessages, System.Func<int, T> objectFactory ) {
        int i = 0;
        while ( i++ < totalMessages ) {
            writer.TryWrite( objectFactory( i ) );
        }
        writer.Complete();
    }

    [ Fact ]
    public async Task ChannelMuxBasicTest( ) {
        int                              msgCountChannel1 = 100;
        int                              msgCountChannel2 = 50;
        BroadcastChannel<DataTypeA>      channel1         = new ();
        BroadcastChannel<DataTypeB>      channel2         = new ();
        ChannelMux<DataTypeA, DataTypeB> mux              = new (channel1.Writer, channel2.Writer);
        CancellationToken                ct               = CancellationToken.None;
        Stopwatch                        stopwatch        = Stopwatch.StartNew();
        Task                             producer1        = Task.Run( ( ) => producerTaskSimple( channel1.Writer, msgCountChannel1, i => new DataTypeA( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), ct );
        Task                             producer2        = Task.Run( ( ) => producerTaskSimple( channel2.Writer, msgCountChannel2, i => new DataTypeB( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), ct );
        int                              receivedCountA   = 0;
        int                              receivedCountB   = 0;

        // ReSharper disable UnusedVariable        
        DataTypeA a;
        DataTypeB b;
        while ( await mux.WaitToReadAsync( ct ) ) {
            if ( mux.TryRead( out a ) ) {
                receivedCountA++;
            }
            if ( mux.TryRead( out b ) ) {
                receivedCountB++;
            }
        }
        // ReSharper restore UnusedVariable
        await producer1;
        await producer2;
        receivedCountA.Should().Be( msgCountChannel1 );
        receivedCountB.Should().Be( msgCountChannel2 );
        if ( receivedCountA != msgCountChannel1 || receivedCountB != msgCountChannel2 ) {
            throw new System.Exception( $"Not all messages were read. {nameof(receivedCountA)}: {receivedCountA} ; {nameof(receivedCountB)}: {receivedCountB}" );
        }
    }

    [ InlineData( true ) ]
    [ InlineData( false ) ]
    [ Theory ]
    public async Task MuxShouldWaitForBothChannelsToComplete( bool withCancellableCancellationToken ) {
        static void producerTaskWithDelay<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in int totalMessages, TimeSpan sleepTime, System.Func<int, T> objectFactory ) {
            Thread.Sleep( sleepTime );
            for ( int i = 0 ; i < totalMessages ; i++ ) {
                writer.TryWrite( objectFactory( i ) );
            }
            writer.Complete();
        }

        const int                        msgCountChannel1    = 20_000;
        const int                        msgCountChannel2    = 50;
        int                              receivedCountA      = 0, receivedCountB = 0;
        int                              waitToReadLoopCount = 0;
        Stopwatch                        stopwatch           = Stopwatch.StartNew();
        BroadcastChannel<DataTypeA>      channel1            = new ();
        BroadcastChannel<DataTypeB>      channel2            = new ();
        ChannelMux<DataTypeA, DataTypeB> mux                 = new (channel1.Writer, channel2.Writer) { StopWatch = stopwatch };
        Task                             producer1           = Task.Run( ( ) => producerTaskWithDelay( channel1.Writer, msgCountChannel1, TimeSpan.FromTicks( 10_000 ), i => new DataTypeA( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), CancellationToken.None );
        Task                             producer2           = Task.Run( ( ) => producerTaskSimple( channel2.Writer, msgCountChannel2, i => new DataTypeB( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), CancellationToken.None );

        using CancellationTokenSource cts = new CancellationTokenSource();
        CancellationToken             ct  = withCancellableCancellationToken ? cts.Token : CancellationToken.None;
        while ( await mux.WaitToReadAsync( ct ) ) {
            waitToReadLoopCount++;
            if ( mux.TryRead( out DataTypeA _ ) ) {
                receivedCountA++;
            }
            if ( mux.TryRead( out DataTypeB _ ) ) {
                receivedCountB++;
            }
        }
        await producer1;
        await producer2;
        _logger.LogDebug( $"{nameof(waitToReadLoopCount)}: {{WaitToReadLoopCount}}\n\t" +
                          $"receivedCountA: {{receivedCountA}}\n\t"                     +
                          $"receivedCountB: {{receivedCountB}}", waitToReadLoopCount, receivedCountA, receivedCountB );
        receivedCountA.Should().Be( msgCountChannel1 );
        receivedCountB.Should().Be( msgCountChannel2 );
    }

    [ InlineData( true ) ]
    [ InlineData( false ) ]
    [ Theory ]
    public async Task ChannelComplete_WithException_ShouldThrow_UponAwait(  bool withCancellableCancellationToken ) {
        static void producerTaskCompleteWithErrorAfter<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in int completeWithExceptionAfterCount, System.Func<int, T> objectFactory ) {
            TimeSpan sleepTime = TimeSpan.FromTicks( 100_000 );
            Thread.Sleep( sleepTime );
            for ( int i = 0 ; i < completeWithExceptionAfterCount ; i++ ) {
                writer.TryWrite( objectFactory( i ) );
            }
            Thread.Sleep( 3 );
            writer.Complete( new SomeException() );
        }

        const int                     completeWithExceptionAfterCount = 25_000;
        const int                     msgCountChannel1                = 50_000;
        int                           receivedCountA                  = 0, receivedCountB = 0;
        int                           waitToReadLoopCount             = 0;
        Stopwatch                     stopwatch                       = Stopwatch.StartNew();
        BroadcastChannel<DataTypeA>   channel1                        = new ();
        BroadcastChannel<DataTypeB>   channel2                        = new ();
        using CancellationTokenSource cts                             = new CancellationTokenSource();
        CancellationToken             ct                              = withCancellableCancellationToken ? cts.Token : CancellationToken.None;
        ChannelMux<DataTypeA, DataTypeB> mux = new (channel1.Writer, channel2.Writer) {
            StopWatch   = stopwatch,
            OnException = ( exception => exception )
        };
        Task producer1 = Task.Run( ( ) => producerTaskSimple( channel1.Writer, msgCountChannel1, i => new DataTypeA( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), CancellationToken.None );
        Task producer2 = Task.Run( ( ) => producerTaskCompleteWithErrorAfter( channel2.Writer, completeWithExceptionAfterCount, i => new DataTypeB( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), CancellationToken.None );

        Func<Task> readerTask = async ( ) => {
            while ( await mux.WaitToReadAsync( ct ) ) {
                waitToReadLoopCount++;
                if ( mux.TryRead( out DataTypeA _ ) ) {
                    receivedCountA++;
                }
                if ( mux.TryRead( out DataTypeB _ ) ) {
                    receivedCountB++;
                }
            }
        };
        await readerTask.Should().ThrowAsync<SomeException>();
        await producer1;
        await producer2;
        _logger.LogDebug( $"{nameof(waitToReadLoopCount)}: {{WaitToReadLoopCount}}\n\t" +
                          $"receivedCountA: {{receivedCountA}}\n\t"                     +
                          $"receivedCountB: {{receivedCountB}}", waitToReadLoopCount, receivedCountA, receivedCountB );
        receivedCountA.Should().NotBe( 0 );
        receivedCountB.Should().NotBe( 0 );
        waitToReadLoopCount.Should().NotBe( 0 );
    }


    public async Task OnException_Test( ) {
        // URGENT:!!!!
        const int                        msgCountChannel1 = 50, msgCountChannel2 = 100;
        int                              receivedCountA   = 0,  receivedCountB   = 0;
        BroadcastChannel<DataTypeA>      channel1         = new ();
        BroadcastChannel<DataTypeB>      channel2         = new ();
        ChannelMux<DataTypeA, DataTypeB> mux              = new (channel1.Writer, channel2.Writer) { OnException = exception => exception };
        // URGENT:!!!!
    }


    /* **************************************************
     * CancellationToken Tests
     * **************************************************/

    #region CancellationToken Tests

    /// <summary>
    /// If WaitToReadAsync cancellation token is cancelled when started, it should immediately exit.
    /// </summary>
    [ Fact ]
    public async Task CancellationToken_PreWaitToReadAsync_Test( ) {
        const int                        msgCountChannel1 = 50, msgCountChannel2 = 100;
        int                              receivedCountA   = 0,  receivedCountB   = 0;
        BroadcastChannel<DataTypeA>      channel1         = new ();
        BroadcastChannel<DataTypeB>      channel2         = new ();
        ChannelMux<DataTypeA, DataTypeB> mux              = new (channel1.Writer, channel2.Writer);
        CancellationTokenSource          cts              = new CancellationTokenSource();
        CancellationToken                ct               = cts.Token;
        int                              cancelAfterCount = 25;
        int                              loopsSinceReadB  = 0;
        Stopwatch                        stopwatch        = Stopwatch.StartNew();
        Task                             producer1        = Task.Run( ( ) => producerTaskSimple( channel1.Writer, msgCountChannel1, i => new DataTypeA( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), ct );
        Task                             producer2        = Task.Run( ( ) => producerTaskSimple( channel2.Writer, msgCountChannel2, i => new DataTypeB( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), ct );
        Func<Task> readerTask = async ( ) => {
            while ( await mux.WaitToReadAsync( ct ) ) {
                loopsSinceReadB++;
                if ( mux.TryRead( out DataTypeA _ ) ) {
                    receivedCountA++;
                }
                if ( mux.TryRead( out DataTypeB _ ) ) {
                    if ( receivedCountB == cancelAfterCount ) {
                        cts.Cancel();
                    } else {
                        receivedCountB++;
                    }
                    loopsSinceReadB = 0;
                }
            }
        };
        await readerTask.Should().ThrowAsync<OperationCanceledException>();
        await producer1;
        await producer2;
        receivedCountA.Should().NotBe( 0 );
        loopsSinceReadB.Should().Be( 0 );
        receivedCountB.Should().Be( cancelAfterCount );
    }

    /// <summary>
    /// If WaitToReadAsync cancellation token is cancelled when started, it should immediately exit.
    /// </summary>
    [ Fact ]
    public async Task CancellationToken_DuringWaitToReadAsync_Test( ) {
        BroadcastChannel<DataTypeA>      channel1 = new ();
        BroadcastChannel<DataTypeB>      channel2 = new ();
        ChannelMux<DataTypeA, DataTypeB> mux      = new (channel1.Writer, channel2.Writer);
        CancellationTokenSource          cts      = new CancellationTokenSource();
        CancellationToken                ct       = cts.Token;
        int                              loops    = 0;
        Func<Task> readerTask = async ( ) => {
            cts.CancelAfter( 5 );
            while ( await mux.WaitToReadAsync( ct ) ) {
                loops++;
            }
        };
        await readerTask.Should().ThrowAsync<OperationCanceledException>();
        loops.Should().Be( 0 );
    }

    [ Fact ]
    public async Task CancellationToken_PreWaitToReadAsync_NewAfterException_Test( ) {
        const int                        msgCountChannel1                 = 50_000, msgCountChannel2 = 1_000;
        const int                        cancelEveryCount                 = 25;
        const int                        throwExceptionCount              = 4;
        int                              operationCancelledExceptionsSeen = 0;
        int                              receivedCountA                   = 0, receivedCountB = 0;
        BroadcastChannel<DataTypeA>      channel1                         = new ();
        BroadcastChannel<DataTypeB>      channel2                         = new ();
        ChannelMux<DataTypeA, DataTypeB> mux                              = new (channel1.Writer, channel2.Writer);
        Stopwatch                        stopwatch                        = Stopwatch.StartNew();
        Task                             producer1                        = Task.Run( ( ) => producerTaskSimple( channel1.Writer, msgCountChannel1, i => new DataTypeA( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), CancellationToken.None );
        Task                             producer2                        = Task.Run( ( ) => producerTaskSimple( channel2.Writer, msgCountChannel2, i => new DataTypeB( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), CancellationToken.None );

        Func<Task> readerTask = async ( ) => {
            for ( int exceptionLoop = 0 ; exceptionLoop < throwExceptionCount ; exceptionLoop++ ) {
                try {
                    CancellationTokenSource cts = new CancellationTokenSource();
                    while ( await mux.WaitToReadAsync( cts.Token ) ) {
                        if ( mux.TryRead( out DataTypeA _ ) ) {
                            receivedCountA++;
                        }
                        if ( mux.TryRead( out DataTypeB _ ) ) {
                            receivedCountB++;
                            if ( receivedCountB % cancelEveryCount == 0 ) {
                                cts.Cancel();
                            }
                        }
                    }
                } catch ( OperationCanceledException ) {
                    _logger.LogDebug( "OperationCanceledException @exceptionLoop={ExceptionLoop} ; receivedCountB={ReceivedCountB}", exceptionLoop, receivedCountB );
                    operationCancelledExceptionsSeen++;
                    if ( exceptionLoop == throwExceptionCount - 1 ) {
                        throw;
                    }
                }
            }
        };
        await readerTask.Should().ThrowAsync<OperationCanceledException>();
        await producer1;
        await producer2;
        receivedCountA.Should().NotBe( 0 );
        receivedCountA.Should().BeLessThan( msgCountChannel1 );
        receivedCountB.Should().Be( cancelEveryCount * throwExceptionCount );
        operationCancelledExceptionsSeen.Should().Be( throwExceptionCount );
    }

    [ Fact ]
    public async Task CancellationToken_DuringWaitToReadAsync_NewAfterException_Test( ) {
        BroadcastChannel<DataTypeA>      channel1                         = new ();
        BroadcastChannel<DataTypeB>      channel2                         = new ();
        ChannelMux<DataTypeA, DataTypeB> mux                              = new (channel1.Writer, channel2.Writer);
        const int                        throwExceptionCount              = 4;
        int                              operationCancelledExceptionsSeen = 0;
        CancellationTokenSource          cts                              = new CancellationTokenSource();
        CancellationToken                ct                               = cts.Token;
        int                              loops                            = 0;
        Func<Task> readerTask = async ( ) => {
            for ( int exceptionLoop = 0 ; exceptionLoop < throwExceptionCount ; exceptionLoop++ ) {
                try {
                    cts.CancelAfter( 5 );
                    while ( await mux.WaitToReadAsync( ct ) ) {
                        loops++;
                    }
                } catch ( OperationCanceledException ) {
                    _logger.LogDebug( "OperationCanceledException @exceptionLoop={ExceptionLoop}", exceptionLoop );
                    operationCancelledExceptionsSeen++;
                    if ( exceptionLoop == throwExceptionCount - 1 ) {
                        throw;
                    }
                }
            }
        };
        await readerTask.Should().ThrowAsync<OperationCanceledException>();
        loops.Should().Be( 0 );
        operationCancelledExceptionsSeen.Should().Be( throwExceptionCount );
    }

    #endregion
}