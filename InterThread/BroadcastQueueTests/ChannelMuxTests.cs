using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Benchmarks.InterThread.BroadcastQueueTests;

using BroadcastChannel;

using BroadcastChannelMux;

using FluentAssertions;

using Microsoft.Extensions.Logging;

using Xunit;
using Xunit.Abstractions;

namespace Benchmarks.InterThread.ChannelMuxTests;

[ SuppressMessage( "ReSharper", "NotAccessedPositionalProperty.Local" ) ]
file readonly record struct DataTypeA(
    int  Sequence,
    long WrittenTicks
);

[ SuppressMessage( "ReSharper", "NotAccessedPositionalProperty.Local" ) ]
file readonly record struct DataTypeB(
    int  Sequence,
    long WrittenTicks
);

[ SuppressMessage( "ReSharper", "NotAccessedPositionalProperty.Local" ) ]
file readonly record struct DataTypeC(
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

    [ InlineData( true ) ]
    [ InlineData( false ) ]
    [ Theory ]
    public async Task ChannelMuxBasicTest( bool withCancellableCancellationToken ) {
        int                              msgCountChannel1 = 100;
        int                              msgCountChannel2 = 50;
        BroadcastChannel<DataTypeA>      channel1         = new ();
        BroadcastChannel<DataTypeB>      channel2         = new ();
        ChannelMux<DataTypeA, DataTypeB> mux              = new (channel1.Writer, channel2.Writer);
        using CancellationTokenSource    cts              = new CancellationTokenSource();
        CancellationToken                ct               = withCancellableCancellationToken ? cts.Token : CancellationToken.None;
        Stopwatch                        stopwatch        = Stopwatch.StartNew();
        Task                             producer1        = Task.Run( ( ) => producerTaskSimple( channel1.Writer, msgCountChannel1, i => new DataTypeA( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), ct );
        Task                             producer2        = Task.Run( ( ) => producerTaskSimple( channel2.Writer, msgCountChannel2, i => new DataTypeB( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), ct );
        int                              receivedCountA   = 0;
        int                              receivedCountB   = 0;

        while ( await mux.WaitToReadAsync( ct ) ) {
            if ( mux.TryRead( out DataTypeA _ ) ) {
                receivedCountA++;
            }
            if ( mux.TryRead( out DataTypeB _ ) ) {
                receivedCountB++;
            }
        }
        await producer1;
        await producer2;
        receivedCountA.Should().Be( msgCountChannel1 );
        receivedCountB.Should().Be( msgCountChannel2 );
        mux.Completion.IsCompleted.Should().BeTrue();
        mux.Completion.Exception.Should().BeNull();
        mux.Completion.IsCompletedSuccessfully.Should().BeTrue();
        if ( receivedCountA != msgCountChannel1 || receivedCountB != msgCountChannel2 ) {
            throw new System.Exception( $"Not all messages were read. {nameof(receivedCountA)}: {receivedCountA} ; {nameof(receivedCountB)}: {receivedCountB}" );
        }
    }

    private readonly record struct MessageWithSendTicks( long Ticks );

    [ InlineData( true ) ]
    [ InlineData( false ) ]
    [ Theory ]
    public async Task ChannelMuxLatencyTest( bool withCancellableCancellationToken ) {
        const int                                   msgCountChannel1 = 50;
        const int                                   sleepMs          = 4;
        BroadcastChannel<MessageWithSendTicks>      channel1         = new ();
        BroadcastChannel<DataTypeB>                 channel2         = new ();
        ChannelMux<MessageWithSendTicks, DataTypeB> mux              = new (channel1.Writer, channel2.Writer);
        using CancellationTokenSource               cts              = new CancellationTokenSource();
        CancellationToken                           ct               = withCancellableCancellationToken ? cts.Token : CancellationToken.None;
        Stopwatch                                   stopwatch        = Stopwatch.StartNew();
        Task producer1 = Task.Run( ( ) => {
            int i      = 0;
            var writer = channel1.Writer;
            while ( i++ < msgCountChannel1 ) {
                writer.TryWrite( new MessageWithSendTicks( stopwatch.ElapsedTicks ) );
                Thread.Sleep( sleepMs );
            }
            writer.Complete();
        }, ct );
        Task   producer2        = Task.Run( ( ) => channel2.Writer.Complete(), ct );
        int    receivedCountA   = 0;
        long[] messageLatencies = new long[ msgCountChannel1 ];

        while ( await mux.WaitToReadAsync( ct ) ) {
            if ( mux.TryRead( out MessageWithSendTicks a ) ) {
                if ( receivedCountA > 2 ) {
                    // The first one, especially on slower systems can be quite a bit slower.
                    ( stopwatch.ElapsedTicks - a.Ticks ).Should().BeLessThan( ( sleepMs - 1 ) * Stopwatch.Frequency / 1_000 ); // ticks as ms
                }
                messageLatencies[ receivedCountA ] = ( stopwatch.ElapsedTicks - a.Ticks );
                receivedCountA++;
            }
        }
        await producer1;
        await producer2;
        receivedCountA.Should().Be( msgCountChannel1 );
        mux.Completion.IsCompleted.Should().BeTrue();
        mux.Completion.Exception.Should().BeNull();
        mux.Completion.IsCompletedSuccessfully.Should().BeTrue();
        if ( receivedCountA != msgCountChannel1 ) {
            throw new System.Exception( $"Not all messages were read. {nameof(receivedCountA)}: {receivedCountA}" );
        }
        _logger.LogInformation( "Latencies: {Latencies}", String.Join( ", ", messageLatencies ) );
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
        mux.Completion.IsCompleted.Should().BeTrue();
        mux.Completion.Exception.Should().BeNull();
        mux.Completion.IsCompletedSuccessfully.Should().BeTrue();
    }

    [ Fact ]
    public async Task AlternateChannelWriterMethods( ) {
        static async Task producerTaskUsingAsync<T>( BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, int totalMessages, System.Func<int, T> objectFactory ) {
            for ( int i = 0 ; i < totalMessages ; i++ ) {
                await writer.WriteAsync( objectFactory( i ) );
            }
            writer.Complete();
        }

        int                              msgCountChannel1 = 100;
        int                              msgCountChannel2 = 50;
        BroadcastChannel<DataTypeA>      channel1         = new ();
        BroadcastChannel<DataTypeB>      channel2         = new ();
        ChannelMux<DataTypeA, DataTypeB> mux              = new (channel1.Writer, channel2.Writer);
        CancellationToken                ct               = CancellationToken.None;
        Stopwatch                        stopwatch        = Stopwatch.StartNew();
        Task                             producer1        = producerTaskUsingAsync( channel1.Writer, msgCountChannel1, i => new DataTypeA( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) );
        Task                             producer2        = producerTaskUsingAsync( channel2.Writer, msgCountChannel2, i => new DataTypeB( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) );
        int                              receivedCountA   = 0;
        int                              receivedCountB   = 0;

        while ( await mux.WaitToReadAsync( ct ) ) {
            if ( mux.TryRead( out DataTypeA _ ) ) {
                receivedCountA++;
            }
            if ( mux.TryRead( out DataTypeB _ ) ) {
                receivedCountB++;
            }
        }
        await producer1;
        await producer2;
        receivedCountA.Should().Be( msgCountChannel1 );
        receivedCountB.Should().Be( msgCountChannel2 );
        if ( receivedCountA != msgCountChannel1 || receivedCountB != msgCountChannel2 ) {
            throw new System.Exception( $"Not all messages were read. {nameof(receivedCountA)}: {receivedCountA} ; {nameof(receivedCountB)}: {receivedCountB}" );
        }
        mux.Completion.IsCompleted.Should().BeTrue();
        mux.Completion.Exception.Should().BeNull();
        mux.Completion.IsCompletedSuccessfully.Should().BeTrue();
    }


    [ Fact ]
    public async Task ReplaceChannel_WhenMuxIsCompleted( ) {
        int                         msgCountChannel1    = 100;
        int                         msgCountChannel2    = 50;
        int                         msgCountChannel3    = 75;
        BroadcastChannel<DataTypeA> channel1            = new ();
        BroadcastChannel<DataTypeB> channel2            = new ();
        BroadcastChannel<DataTypeA> channelReplacement1 = new ();
        using ( ChannelMux<DataTypeA, DataTypeB> mux = new (channel1.Writer, channel2.Writer) ) {
            CancellationToken ct             = CancellationToken.None;
            Stopwatch         stopwatch      = Stopwatch.StartNew();
            Task              producer1      = Task.Run( ( ) => producerTaskSimple( channel1.Writer, msgCountChannel1, i => new DataTypeA( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), ct );
            Task              producer2      = Task.Run( ( ) => producerTaskSimple( channel2.Writer, msgCountChannel2, i => new DataTypeB( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), ct );
            int               receivedCountA = 0;
            int               receivedCountB = 0;
            int               receivedCountC = 0;

            TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeA>>>( channel1.Writer, "_outputWriters" ).Should().HaveCount( 1 );
            TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeB>>>( channel2.Writer, "_outputWriters" ).Should().HaveCount( 1 );
            TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeA>>>( channelReplacement1.Writer, "_outputWriters" ).Should().HaveCount( 0 );
            TestUtils.getPrivateField<ChannelMux,int>( mux, "_closedChannels" ).Should().Be( 0 );

            while ( await mux.WaitToReadAsync( ct ) ) {
                if ( mux.TryRead( out DataTypeA _ ) ) {
                    receivedCountA++;
                }
                if ( mux.TryRead( out DataTypeB _ ) ) {
                    receivedCountB++;
                }
            }
            await producer1;
            await producer2;
            receivedCountA.Should().Be( msgCountChannel1 );
            receivedCountB.Should().Be( msgCountChannel2 );
            mux.Completion.IsCompleted.Should().BeTrue();
            mux.Completion.Exception.Should().BeNull();
            mux.Completion.IsCompletedSuccessfully.Should().BeTrue();
            TestUtils.getPrivateField<ChannelMux,int>( mux, "_closedChannels" ).Should().Be( 2 );

            // Replace
            mux.ReplaceChannel( channelReplacement1.Writer );
            mux.Completion.IsCompleted.Should().BeFalse();
            TestUtils.getPrivateField<ChannelMux,int>( mux, "_closedChannels" ).Should().Be( 1 );
            TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeA>>>( channel1.Writer, "_outputWriters" ).Should().HaveCount( 0 );
            TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeB>>>( channel2.Writer, "_outputWriters" ).Should().HaveCount( 1 );
            TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeA>>>( channelReplacement1.Writer, "_outputWriters" ).Should().HaveCount( 1 );

            Task producer3 = Task.Run( ( ) => producerTaskSimple( channelReplacement1.Writer, msgCountChannel3, i => new DataTypeA( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), ct );
            while ( await mux.WaitToReadAsync( ct ) ) {
                if ( mux.TryRead( out DataTypeA _ ) ) {
                    receivedCountC++;
                }
                if ( mux.TryRead( out DataTypeB _ ) ) {
                    receivedCountB++;
                }
            }
            await producer3;
            receivedCountC.Should().Be( msgCountChannel3 );
            mux.Completion.IsCompleted.Should().BeTrue();
            mux.Completion.Exception.Should().BeNull();
            mux.Completion.IsCompletedSuccessfully.Should().BeTrue();
        }
        TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeA>>>( channel1.Writer, "_outputWriters" ).Should().HaveCount( 0 );
        TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeB>>>( channel2.Writer, "_outputWriters" ).Should().HaveCount( 0 );
        TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeA>>>( channelReplacement1.Writer, "_outputWriters" ).Should().HaveCount( 0 );
    }


    [ Fact ]
    public async Task ReplaceChannel_WhenMuxIsOpen( ) {
        // URGENT: EXACT SAME , BUT HAVE THE CHANNEL BEING REPLACED STILL BE OPEN.
    }


    /* **************************************************
     * Exception Tests
     * **************************************************/

    #region Exception Tests

    [ InlineData( true ) ]
    [ InlineData( false ) ]
    [ Theory ]
    public async Task ChannelComplete_WithException_ShouldThrow_UponAwait( bool withCancellableCancellationToken ) {
        static void producerTaskCompleteWithErrorAfter<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in int completeWithExceptionAfterCount, System.Func<int, T> objectFactory ) {
            TimeSpan sleepTime = TimeSpan.FromTicks( 10_000 );
            for ( int i = 0 ; i < completeWithExceptionAfterCount ; i++ ) {
                writer.TryWrite( objectFactory( i ) );
            }
            Thread.Sleep( sleepTime );
            writer.Complete( new SomeException() );
        }

        const int                     completeWithExceptionAfterCount = 500;
        const int                     msgCountChannel1                = 50_000;
        int                           receivedCountA                  = 0, receivedCountB = 0;
        int                           waitToReadLoopCount             = 0;
        Stopwatch                     stopwatch                       = Stopwatch.StartNew();
        BroadcastChannel<DataTypeA>   channel1                        = new ();
        BroadcastChannel<DataTypeB>   channel2                        = new ();
        using CancellationTokenSource cts                             = new CancellationTokenSource();
        CancellationToken             ct                              = withCancellableCancellationToken ? cts.Token : CancellationToken.None;
        int                           onChannelCompleteCounter        = 0;
        ChannelMux<DataTypeA, DataTypeB> mux = new (channel1.Writer, channel2.Writer) {
            StopWatch = stopwatch,
            OnChannelComplete = ( ( dType, exception ) => {
                onChannelCompleteCounter++;
                _logger.LogDebug( $"OnException: {{Exception}} for channel of type {{DType}}\n\t" +
                                  $"{nameof(waitToReadLoopCount)}: {{WaitToReadLoopCount}}\n\t"   +
                                  $"receivedCountA: {{receivedCountA}}\n\t"                       +
                                  $"receivedCountB: {{receivedCountB}}", exception, dType.Name, waitToReadLoopCount, receivedCountA, receivedCountB );
                if ( exception is { } ) {
                    dType.Should().Be( typeof(DataTypeB) );
                } else {
                    dType.Should().Be( typeof(DataTypeA) );
                }
                return exception;
            } )
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
        await readerTask.Should().ThrowAsync<SomeException>( because: $"A second call to {nameof(mux.WaitToReadAsync)} should immediately throw." );
        Func<Task> asyncWriterShouldThrow = async ( ) => {
            await channel1.Writer.WriteAsync( new DataTypeA {
                                                  Sequence     = 10,
                                                  WrittenTicks = -1
                                              } );
        };
        await asyncWriterShouldThrow.Should().ThrowAsync<ChannelClosedException>()
                                    .WithInnerException( typeof(SomeException) );
        _logger.LogDebug( $"{nameof(waitToReadLoopCount)}: {{WaitToReadLoopCount}}\n\t" +
                          $"receivedCountA: {{receivedCountA}}\n\t"                     +
                          $"receivedCountB: {{receivedCountB}}", waitToReadLoopCount, receivedCountA, receivedCountB );
        // read remaining messages ( if any )
        while ( ( mux.TryRead( out DataTypeA _ ), mux.TryRead( out DataTypeB _ ) ) != ( false, false ) ) { }

        onChannelCompleteCounter.Should().Be( 2 );
        mux.Completion.IsCompleted.Should().BeTrue();
        var aggregateException = mux.Completion.Exception.Should().BeOfType<AggregateException>().Subject;
        aggregateException.InnerException.Should().BeOfType<SomeException>();
        mux.Completion.IsCompletedSuccessfully.Should().BeFalse();
    }

    #endregion


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
        mux.Completion.IsCompleted.Should().BeFalse(); // not completed because not all messages are read
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
        mux.Completion.IsCompleted.Should().BeFalse(); // not completed because not all messages are read
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
        mux.Completion.IsCompleted.Should().BeFalse(); // not completed because not all messages are read
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
        mux.Completion.IsCompleted.Should().BeFalse(); // not completed because not all messages are read
    }

    #endregion


    /* **************************************************
     * Disposal Tests
     * **************************************************/

    #region Disposal tests

    [ Fact ]
    public async Task DisposalOfMuxShouldRemoveFromBroadcastChannel( ) {
        int                         msgCountChannel1 = 100;
        int                         msgCountChannel2 = 50;
        BroadcastChannel<DataTypeA> channel1         = new ();
        BroadcastChannel<DataTypeB> channel2         = new ();
        using ( ChannelMux<DataTypeA, DataTypeB> mux = new (channel1.Writer, channel2.Writer) ) {
            CancellationToken ct             = CancellationToken.None;
            Stopwatch         stopwatch      = Stopwatch.StartNew();
            Task              producer1      = Task.Run( ( ) => producerTaskSimple( channel1.Writer, msgCountChannel1, i => new DataTypeA( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), ct );
            Task              producer2      = Task.Run( ( ) => producerTaskSimple( channel2.Writer, msgCountChannel2, i => new DataTypeB( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), ct );
            int               receivedCountA = 0;
            int               receivedCountB = 0;

            TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeA>>>( channel1.Writer, "_outputWriters" ).Should().HaveCount( 1 );

            while ( await mux.WaitToReadAsync( ct ) ) {
                if ( mux.TryRead( out DataTypeA _ ) ) {
                    receivedCountA++;
                }
                if ( mux.TryRead( out DataTypeB _ ) ) {
                    receivedCountB++;
                }
            }
            await producer1;
            await producer2;
            receivedCountA.Should().Be( msgCountChannel1 );
            receivedCountB.Should().Be( msgCountChannel2 );
            mux.Completion.IsCompleted.Should().BeTrue();
            mux.Completion.Exception.Should().BeNull();
            mux.Completion.IsCompletedSuccessfully.Should().BeTrue();
            if ( receivedCountA != msgCountChannel1 || receivedCountB != msgCountChannel2 ) {
                throw new System.Exception( $"Not all messages were read. {nameof(receivedCountA)}: {receivedCountA} ; {nameof(receivedCountB)}: {receivedCountB}" );
            }
            mux.Dispose(); // try an explicit Dispose() which will cause exiting the using block to make a second Dispose() call. make sure it doesn't error
            TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeA>>>( channel1.Writer, "_outputWriters" ).Should().HaveCount( 0 );
        }
        TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeA>>>( channel1.Writer, "_outputWriters" ).Should().HaveCount( 0 );
    }

    [ Fact ]
    public async Task ExceptionShouldRemoveFromBroadcastChannel( ) {
        int                         msgCountChannel1 = 100;
        int                         msgCountChannel2 = 50;
        BroadcastChannel<DataTypeA> channel1         = new ();
        BroadcastChannel<DataTypeB> channel2         = new ();
        try {
            using ( ChannelMux<DataTypeA, DataTypeB> mux = new (channel1.Writer, channel2.Writer) ) {
                CancellationToken ct        = CancellationToken.None;
                Stopwatch         stopwatch = Stopwatch.StartNew();
                Task producer1 = Task.Run( ( ) => {
                    try {
                        throw new SomeException();
                    } catch ( Exception e ) {
                        channel1.Writer.Complete( e );
                        throw;
                    }
                }, ct );
                Task producer2      = Task.Run( ( ) => producerTaskSimple( channel2.Writer, msgCountChannel2, i => new DataTypeB( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), ct );
                int  receivedCountA = 0;
                int  receivedCountB = 0;

                TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeA>>>( channel1.Writer, "_outputWriters" ).Should().HaveCount( 1 );

                while ( await mux.WaitToReadAsync( ct ) ) {
                    if ( mux.TryRead( out DataTypeA _ ) ) {
                        receivedCountA++;
                    }
                    if ( mux.TryRead( out DataTypeB _ ) ) {
                        receivedCountB++;
                    }
                }
                await producer1;
                await producer2;
                receivedCountA.Should().Be( msgCountChannel1 );
                receivedCountB.Should().Be( msgCountChannel2 );
                mux.Completion.IsCompleted.Should().BeTrue();
                mux.Completion.Exception.Should().BeNull();
                mux.Completion.IsCompletedSuccessfully.Should().BeTrue();
                if ( receivedCountA != msgCountChannel1 || receivedCountB != msgCountChannel2 ) {
                    throw new System.Exception( $"Not all messages were read. {nameof(receivedCountA)}: {receivedCountA} ; {nameof(receivedCountB)}: {receivedCountB}" );
                }
                TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeA>>>( channel1.Writer, "_outputWriters" ).Should().HaveCount( 0 );
            }
        } catch ( Exception ) {
            // nothing
        } finally {
            TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeA>>>( channel1.Writer, "_outputWriters" ).Should().HaveCount( 0 );
            _logger.LogDebug( "count of writers: {X}", TestUtils.getPrivateField<ImmutableArray<ChannelWriter<DataTypeA>>>( channel1.Writer, "_outputWriters" ).Length );
        }
    }

    #endregion


    #region Inheritance Testing

    private record BaseClass( int PropertyBase );

    private record SubClassA( int PropertyBase, int PropertySubA ) : BaseClass( PropertyBase );


    private record SubClassB( int PropertyBase, int PropertySubB ) : BaseClass( PropertyBase );


    internal static async Task TypeInheritanceTestingOneSubOfOther( ) {
        BroadcastChannel<BaseClass>            channel1         = new ();
        BroadcastChannel<SubClassA>            channel2         = new ();
        using ChannelMux<BaseClass, SubClassA> mux              = new (channel1.Writer, channel2.Writer);
        int                                    msgCountChannel1 = 100;
        int                                    msgCountChannel2 = 50;
        CancellationToken                      ct               = CancellationToken.None;
        Task                                   producer1        = Task.Run( ( ) => producerTaskSimple( channel1.Writer, msgCountChannel1, static x => new BaseClass( PropertyBase: -x - 1 ) ), ct );
        Task                                   producer2        = Task.Run( ( ) => producerTaskSimple( channel2.Writer, msgCountChannel2, static x => new SubClassA( PropertyBase: x  + 1, PropertySubA: x + 2 ) ), ct );
        int                                    receivedCountA   = 0;
        int                                    receivedCountB   = 0;

        while ( await mux.WaitToReadAsync( ct ) ) {
            while ( ( mux.TryRead( out BaseClass? baseClass ), mux.TryRead( out SubClassA? subClassA ) ) != ( false, false ) ) {
                if ( baseClass is { } bc ) {
                    bc.PropertyBase.Should().BeLessThan( 0 );
                    receivedCountA++;
                }
                if ( subClassA is { } subA ) {
                    subA.PropertyBase.Should().BeGreaterThan( 0 );
                    subA.PropertySubA.Should().Be( subA.PropertyBase + 1 );
                    receivedCountB++;
                }
            }
        }
        await producer1;
        await producer2;
        receivedCountA.Should().Be( msgCountChannel1 );
        receivedCountB.Should().Be( msgCountChannel2 );
        mux.Completion.IsCompleted.Should().BeTrue();
        mux.Completion.Exception.Should().BeNull();
        mux.Completion.IsCompletedSuccessfully.Should().BeTrue();
        if ( receivedCountA != msgCountChannel1 || receivedCountB != msgCountChannel2 ) {
            throw new System.Exception( $"Not all messages were read. {nameof(receivedCountA)}: {receivedCountA} ; {nameof(receivedCountB)}: {receivedCountB}" );
        }
    }

    internal static async Task TypeInheritanceTestingBothSubOfSame( ) {
        BroadcastChannel<SubClassB>            channel1         = new ();
        BroadcastChannel<SubClassA>            channel2         = new ();
        using ChannelMux<SubClassB, SubClassA> mux              = new (channel1.Writer, channel2.Writer);
        int                                    msgCountChannel1 = 100;
        int                                    msgCountChannel2 = 50;
        CancellationToken                      ct               = CancellationToken.None;
        Task                                   producer1        = Task.Run( ( ) => producerTaskSimple( channel1.Writer, msgCountChannel1, static x => new SubClassB( PropertyBase: -x - 1, PropertySubB: -x - 2 ) ), ct );
        Task                                   producer2        = Task.Run( ( ) => producerTaskSimple( channel2.Writer, msgCountChannel2, static x => new SubClassA( PropertyBase: x  + 1, PropertySubA: x  + 2 ) ), ct );
        int                                    receivedCountA   = 0;
        int                                    receivedCountB   = 0;

        while ( await mux.WaitToReadAsync( ct ) ) {
            while ( ( mux.TryRead( out SubClassB? baseClass ), mux.TryRead( out SubClassA? subClassA ) ) != ( false, false ) ) {
                if ( baseClass is { } subB ) {
                    subB.PropertyBase.Should().BeLessThan( 0 );
                    subB.PropertySubB.Should().Be( subB.PropertyBase - 1 );
                    receivedCountA++;
                }
                if ( subClassA is { } subA ) {
                    subA.PropertyBase.Should().BeGreaterThan( 0 );
                    subA.PropertySubA.Should().Be( subA.PropertyBase + 1 );
                    receivedCountB++;
                }
            }
        }
        await producer1;
        await producer2;
        receivedCountA.Should().Be( msgCountChannel1 );
        receivedCountB.Should().Be( msgCountChannel2 );
        mux.Completion.IsCompleted.Should().BeTrue();
        mux.Completion.Exception.Should().BeNull();
        mux.Completion.IsCompletedSuccessfully.Should().BeTrue();
        if ( receivedCountA != msgCountChannel1 || receivedCountB != msgCountChannel2 ) {
            throw new System.Exception( $"Not all messages were read. {nameof(receivedCountA)}: {receivedCountA} ; {nameof(receivedCountB)}: {receivedCountB}" );
        }
    }

    #endregion
}

internal static class TestUtils {
    //
    // internal static TReturn invokePrivateMethod<TInstance, TReturn>( object?[] constructorArgs, string methodName, object?[] methodArgs ) {
    //     Type type     = typeof(TInstance);
    //     var  instance = Activator.CreateInstance( type, constructorArgs );
    //     if ( instance is null ) {
    //         throw new InstanceCreationException<TInstance>();
    //     }
    //     return invokePrivateMethod<TReturn>( instance, methodName, methodArgs );
    // }

    internal static TReturn invokePrivateMethod<TReturn>( object instance, string methodName, params object?[] methodArgs ) {
        MethodInfo method = instance.GetType()
                                    .GetMethods( BindingFlags.NonPublic | BindingFlags.Instance )
                                    // .GetMethods( BindingFlags.NonPublic | BindingFlags.Static )
                                    .First( x => x.Name == methodName && x.IsPrivate );
        return ( TReturn )method.Invoke( instance, methodArgs )!;
    }

    internal static TReturn getPrivateProperty<TReturn>( object instance, string methodName ) {
        PropertyInfo property = instance.GetType()
                                        .GetProperty( methodName, BindingFlags.NonPublic | BindingFlags.Instance ) ?? throw new Exception();
        return ( TReturn )property.GetValue( instance )!;
    }

    internal static TReturn getPrivateField<TReturn>( object instance, string fieldName ) {
        FieldInfo field = instance.GetType().GetField( fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default ) ?? throw new Exception($"Could not get field '{fieldName}'");
        return ( TReturn )field.GetValue( instance )!;
    }
    internal static TReturn getPrivateField<TFieldOwner,TReturn>( object instance, string fieldName ) {
        FieldInfo field = typeof(TFieldOwner).GetField( fieldName, BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default ) ?? throw new Exception($"Could not get field '{fieldName}'");
        return ( TReturn )field.GetValue( instance )!;
    }


    internal static TReturn invokePrivateStaticMethod<TReturn>( Type type, string methodName, params object[] inputParams ) {
        MethodInfo method = type
                            .GetMethods( BindingFlags.NonPublic | BindingFlags.Static )
                            .First( x => x.Name == methodName && x.IsPrivate );
        return ( TReturn )method.Invoke( null, inputParams )!;
    }
}