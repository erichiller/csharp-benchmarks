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

    [ Fact ]
    public async Task ChannelComplete_WithException_ShouldThrow_UponAwait( ) { // KILL !!!!
        static void producerTaskCompleteWithErrorAfter<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in int completeAfterCount, System.Func<int, T> objectFactory ) {
            TimeSpan sleepTime = TimeSpan.FromTicks( 10_000 );
            for ( int i = 0 ; i < completeAfterCount ; i++ ) {
                writer.TryWrite( objectFactory( i ) );
                Thread.Sleep( sleepTime );
            }
            writer.Complete( new SomeException() );
        }

        static void producerTaskWithDelay<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in int totalMessages, System.Func<int, T> objectFactory ) {
            TimeSpan sleepTime = TimeSpan.FromTicks( 10_000 );
            for ( int i = 0 ; i < totalMessages ; i++ ) {
                writer.TryWrite( objectFactory( i ) );
                // Thread.Sleep( sleepTime );
            }
            writer.Complete();
        }

        const int                        completeAfterCount  = 25;
        const int                        msgCountChannel1    = 50_000;
        int                              receivedCountA      = 0, receivedCountB = 0;
        int                              waitToReadLoopCount = 0;
        Stopwatch                        stopwatch           = Stopwatch.StartNew();
        BroadcastChannel<DataTypeA>      channel1            = new ();
        BroadcastChannel<DataTypeB>      channel2            = new ();
        ChannelMux<DataTypeA, DataTypeB> mux                 = new (channel1.Writer, channel2.Writer) { StopWatch = stopwatch };
        Task                             producer1           = Task.Run( ( ) => producerTaskWithDelay( channel1.Writer, msgCountChannel1, i => new DataTypeA( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), CancellationToken.None );
        Task                             producer2           = Task.Run( ( ) => producerTaskCompleteWithErrorAfter( channel2.Writer, completeAfterCount, i => new DataTypeB( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), CancellationToken.None );

        Func<Task> readerTask = async ( ) => {
            while ( await mux.WaitToReadAsync( CancellationToken.None ) ) {
                waitToReadLoopCount++;
                // URGENT: also check where an actual CancellationToken is provided
                if ( mux.TryRead( out DataTypeA _ ) ) {
                    receivedCountA++;
                }
                if ( mux.TryRead( out DataTypeB _ ) ) {
                    // URGENT: also check that TryRead returns false
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
        receivedCountA.Should().BeLessThan( msgCountChannel1 );
        receivedCountB.Should().Be( completeAfterCount );
    }


    public async Task OnException_Test( ) { // URGENT:!!!!
        const int                        msgCountChannel1 = 50, msgCountChannel2 = 100;
        int                              receivedCountA   = 0,  receivedCountB   = 0;
        BroadcastChannel<DataTypeA>      channel1         = new ();
        BroadcastChannel<DataTypeB>      channel2         = new ();
        ChannelMux<DataTypeA, DataTypeB> mux              = new (channel1.Writer, channel2.Writer) { OnException = exception => exception is SomeException };
        // URGENT:!!!!

    }

    [ Fact ]
    public async Task CancellationToken_Test( ) {
        const int                        msgCountChannel1 = 50, msgCountChannel2 = 100;
        int                              receivedCountA   = 0,  receivedCountB   = 0;
        BroadcastChannel<DataTypeA>      channel1         = new ();
        BroadcastChannel<DataTypeB>      channel2         = new ();
        ChannelMux<DataTypeA, DataTypeB> mux              = new (channel1.Writer, channel2.Writer);
        CancellationTokenSource          cts              = new CancellationTokenSource();
        CancellationToken                ct               = cts.Token;
        int                              cancelAfterCount = 25;
        Stopwatch                        stopwatch        = Stopwatch.StartNew();
        Task                             producer1        = Task.Run( ( ) => producerTaskSimple( channel1.Writer, msgCountChannel1, i => new DataTypeA( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), ct );
        Task                             producer2        = Task.Run( ( ) => producerTaskSimple( channel2.Writer, msgCountChannel2, i => new DataTypeB( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), ct );
        Func<Task> readerTask = async ( ) => {
            while ( await mux.WaitToReadAsync( ct ) ) {
                if ( mux.TryRead( out DataTypeA _ ) ) {
                    receivedCountA++;
                }
                if ( mux.TryRead( out DataTypeB _ ) ) {
                    if ( receivedCountB == cancelAfterCount ) {
                        cts.Cancel();
                    }
                    receivedCountB++;
                }
            }
        };
        await readerTask.Should().ThrowAsync<OperationCanceledException>();
        await producer1;
        await producer2;
        receivedCountA.Should().NotBe( 0 );
        receivedCountB.Should().Be( cancelAfterCount + 1 );
    }

    [ Fact ]
    public async Task CancellationToken_NewAfterException_Test( ) {
        const int                        msgCountChannel1    = 50_000, msgCountChannel2 = 1_000;
        int                              cancelEveryCount    = 25;
        int                              throwExceptionCount = 4;
        int                              receivedCountA      = 0, receivedCountB = 0;
        BroadcastChannel<DataTypeA>      channel1            = new ();
        BroadcastChannel<DataTypeB>      channel2            = new ();
        ChannelMux<DataTypeA, DataTypeB> mux                 = new (channel1.Writer, channel2.Writer);
        Stopwatch                        stopwatch           = Stopwatch.StartNew();
        Task                             producer1           = Task.Run( ( ) => producerTaskSimple( channel1.Writer, msgCountChannel1, i => new DataTypeA( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), CancellationToken.None );
        Task                             producer2           = Task.Run( ( ) => producerTaskSimple( channel2.Writer, msgCountChannel2, i => new DataTypeB( Sequence: i, WrittenTicks: stopwatch.ElapsedTicks ) ), CancellationToken.None );

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
    }

    //
    // [ Fact ]
    // public void AddingAndRemovingReadersShouldNeverError( ) {
    //     // int subscriberCount = 3;
    //     // // int messageCount    = 10_000;
    //     // int          messageCount       = 1_000;
    //     // var          broadcastQueue     = new BroadcastQueue<ChannelMessage, ChannelResponse>();
    //     // var          cts                = new CancellationTokenSource();
    //     // // ( int, int ) writeIntervalRange = ( 1, 200 ); 
    //     // ( int, int ) writeIntervalRange = ( 1, 100 ); 
    //     // cts.CancelAfter( 300_000 );
    //     //
    //     // List<Task<int>> readerTasks = new List<Task<int>>();
    //     // for ( int i = 0 ; i < subscriberCount ; i++ ) {
    //     //     readerTasks.Add( readerTask( broadcastQueue.GetReader(), messageCount, $"readerTask{i}", cts.Token ) );
    //     // }
    //     //
    //     // var addReaderTaskRunner = addReaderTask( broadcastQueue, messageCount, cts.Token, _logger );
    //     //
    //     // List<Task> tasks = new List<Task>(
    //     //     readerTasks
    //     // ) { addReaderTaskRunner, writerTask( broadcastQueue.Writer, messageCount, cts.Token, writeIntervalRange, _logger ), };
    //     //
    //     // try {
    //     //     Task.WaitAll(
    //     //         tasks.ToArray()
    //     //     );
    //     // } catch ( System.AggregateException ex ) {
    //     //     bool taskCanceledException = false;
    //     //     foreach ( var inner in ex.InnerExceptions ) {
    //     //         if ( inner.GetType() == typeof(System.Threading.Tasks.TaskCanceledException) ) {
    //     //             _logger.LogDebug( "Task was cancelled" );
    //     //             taskCanceledException = true;
    //     //         }
    //     //     }
    //     //
    //     //     if ( !taskCanceledException ) {
    //     //         throw;
    //     //     }
    //     // }
    //     //
    //     // foreach ( var task in readerTasks ) {
    //     //     task.Result.Should().Be( messageCount );
    //     //     _logger.LogDebug( "Task had result {ResultMessageId}", task.Result );
    //     // }
    //     //
    //     // _logger.LogDebug( "BroadcastQueue ended with {ReaderCount} readers", broadcastQueue.Writer.ReaderCount );
    //     // _logger.LogDebug(
    //     //     "AddReaderTask created {ReaderCount} on {UniqueTaskIdCount} threads with an average interval between messages of {AverageInterval} ms",
    //     //     addReaderTaskRunner.Result.readerCount,
    //     //     addReaderTaskRunner.Result.uniqueThreadIds.Count,
    //     //     Math.Round(addReaderTaskRunner.Result.intervals.Average()/System.TimeSpan.TicksPerMillisecond,2) );
    //     // broadcastQueue.Writer.ReaderCount.Should().Be( 4 );
    // }
    //
    // [ Theory ]
    // [ InlineData( 1 ) ]
    // [ InlineData( 2 ) ]
    // [ InlineData( 3 ) ]
    // public void SubscribersShouldReceiveAllMessagesInOrder( int subscriberCount ) {
    //     // int messageCount   = 10_000;
    //     // var broadcastQueue = new BroadcastQueue<ChannelMessage, ChannelResponse>();
    //     // var cts            = new CancellationTokenSource();
    //     //
    //     // List<Task<int>> readerTasks = new List<Task<int>>();
    //     // for ( int i = 0 ; i < subscriberCount ; i++ ) {
    //     //     readerTasks.Add( readerTask( broadcastQueue.GetReader(), messageCount, $"readerTask{i}", cts.Token ) );
    //     // }
    //     //
    //     // List<Task> tasks = new List<Task>( readerTasks ) { writerTask( broadcastQueue.Writer, messageCount, cts.Token ) };
    //     //
    //     //
    //     // Task.WaitAll(
    //     //     tasks.ToArray()
    //     // );
    //     // foreach ( var task in readerTasks ) {
    //     //     task.Result.Should().Be( messageCount );
    //     //     _logger.LogDebug( "Task had result {ResultMessageId}", task.Result );
    //     // }
    // }
}