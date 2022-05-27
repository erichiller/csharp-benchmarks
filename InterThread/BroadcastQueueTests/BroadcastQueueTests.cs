using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Benchmarks.InterThread.BroadcastQueue;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Xunit;
using Xunit.Abstractions;

namespace Benchmarks.InterThread.BroadcastQueueTests;

// // fixture
// public class SetupFixture {
//     private IHost   host;
//     private ILogger logger;
//
//     public static IHostBuilder CreateHostBuilder_BroadcastQueue( string[] args, LogLevel? logLevel ) =>
//         Host.CreateDefaultBuilder( args )
//             .ConfigureServices( ( hostContext, services ) => {
//                 services.AddSingleton<BroadcastQueue<ChannelMessage, ChannelResponse>>();
//                 if ( logLevel is { } ll ) {
//                     services.AddLogging( logBuilder => logBuilder.AddConsole().SetMinimumLevel( ll ) );
//                 }
//             } );
//
//     private BroadcastQueue<ChannelMessage, ChannelResponse> _broadcastQueue;
//
//     public async Task CreateHost<T>( LogLevel? logLevel ) {
//         host = CreateHostBuilder_BroadcastQueue( Array.Empty<string>(), logLevel )
//             .Build();
//         await host.StartAsync();
//
//         logger = host.Services.GetService<ILogger<T>>() ?? throw new Exception();
//         // cts             = new CancellationTokenSource();
//         _broadcastQueue = host.Services.GetService<BroadcastQueue<ChannelMessage, ChannelResponse>>() ?? throw new Exception();
//
//         // Console.WriteLine( "Host is running" );
//     }
// }

public record ChannelMessage( int Id, string Property1 = "some text" );

public record ChannelResponse(
    int               ReadId,
    string            ReaderType,
    System.Exception? Exception = null
) : IBroadcastQueueResponse;

public class BroadcastQueueTests : TestBase<BroadcastQueueTests> {
    public BroadcastQueueTests( ITestOutputHelper testOutputHelper ) : base( testOutputHelper ) { }

    static async Task<int> writerTask( BroadcastQueueWriter<ChannelMessage, ChannelResponse> bqWriter, int messageCount, CancellationToken ct, ( int min, int max )? delayMs = null, ILogger? logger = null ) {
        int i      = 0;
        var random = new Random();
        while ( await bqWriter.WaitToWriteAsync( ct ) ) {
            while ( bqWriter.TryWrite( new ChannelMessage( i ) ) ) {
                if ( i >= messageCount ) {
                    logger?.LogDebug( "[BroadcastQueueWriter] wrote messageCount: {MessageCount}", i );
                    return i;
                }

                if ( delayMs is var (min, max) ) {
                    await Task.Delay( random.Next( min, max ), ct );
                }

                i++;
            }
        }

        return -1;
    }

    static async Task<int> readerTask( BroadcastQueueReader<ChannelMessage, ChannelResponse> bqReader, int messageCount, string taskName, CancellationToken ct, ILogger? logger = null ) {
        int lastMessage = -1;
        logger?.LogDebug( $"[BroadcastQueueReader] start" );
        while ( await bqReader.WaitToReadAsync( ct ) ) {
            logger?.LogDebug( $"[BroadcastQueueReader] start receiving" );
            while ( bqReader.TryRead( out ChannelMessage? result ) ) {
                logger?.LogDebug( "[BroadcastQueueReader] received messageCount: {MessageId}", result.Id );
                result.Id.Should().Be( lastMessage + 1, "[BroadcastQueueReader] ERROR at message ID" );
                if ( result.Id >= messageCount ) {
                    await bqReader.WriteResponseAsync( new ChannelResponse( result.Id, taskName ), ct );
                    return result.Id;
                }

                lastMessage++;
            }
        }

        await bqReader.WriteResponseAsync( new ChannelResponse( -1, taskName, new Exception( "Incomplete sequence" ) ), ct );
        return -1;
    }

    static async Task<(int readerCount, List<int> uniqueThreadIds, List<long> intervals)> addReaderTask( BroadcastQueue<ChannelMessage, ChannelResponse> broadcastQueue, int expectedMessageCount, CancellationToken ct, ILogger? logger = null ) {
        logger ??= NullLogger.Instance;
        int readerCount = 0;
        List<int>  threadIds    = new List<int>();
        List<long> intervals = new ();
        long       lastTime     = DateTime.UtcNow.Ticks;
        while ( !ct.IsCancellationRequested ) {
            long now = DateTime.UtcNow.Ticks;
            intervals.Add( now - lastTime );
            lastTime = now;
            logger!.LogDebug( "[{MethodName}][{ReaderCount}] looping", nameof(addReaderTask), readerCount );
            var reader = broadcastQueue.GetReader();
            logger?.LogDebug( "[{MethodName}][{ReaderCount}] Waiting to read", nameof(addReaderTask), readerCount );
            await reader.WaitToReadAsync( ct ); // read at least one message
            logger?.LogDebug( "[{MethodName}] Waiting to read...found", nameof(addReaderTask), readerCount );
            while ( reader.TryRead( out ChannelMessage? message ) ) {
                if ( !threadIds.Contains( Thread.CurrentThread.ManagedThreadId ) ) {
                    threadIds.Add( Thread.CurrentThread.ManagedThreadId );
                }
                logger?.LogDebug( "[{MethodName}][{ReaderCount}] New reader read: {Message}", nameof(addReaderTask), readerCount, message );
                if ( message.Id == expectedMessageCount ) {
                    return ( readerCount, threadIds, intervals );
                }
            }

            broadcastQueue.RemoveReader( reader );
            readerCount++;
        }

        return ( readerCount, threadIds, intervals );
    }

    [ Fact ]
    public void AddingAndRemovingReadersShouldNeverError( ) {
        int subscriberCount = 3;
        // int messageCount    = 10_000;
        int          messageCount       = 1_000;
        var          broadcastQueue     = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        var          cts                = new CancellationTokenSource();
        // ( int, int ) writeIntervalRange = ( 1, 200 ); 
        ( int, int ) writeIntervalRange = ( 1, 100 ); 
        cts.CancelAfter( 300_000 );

        List<Task<int>> readerTasks = new List<Task<int>>();
        for ( int i = 0 ; i < subscriberCount ; i++ ) {
            readerTasks.Add( readerTask( broadcastQueue.GetReader(), messageCount, $"readerTask{i}", cts.Token ) );
        }

        var addReaderTaskRunner = addReaderTask( broadcastQueue, messageCount, cts.Token, _logger );

        List<Task> tasks = new List<Task>(
            readerTasks
        ) { addReaderTaskRunner, writerTask( broadcastQueue.Writer, messageCount, cts.Token, writeIntervalRange, _logger ), };

        try {
            Task.WaitAll(
                tasks.ToArray()
            );
        } catch ( System.AggregateException ex ) {
            bool taskCanceledException = false;
            foreach ( var inner in ex.InnerExceptions ) {
                if ( inner.GetType() == typeof(System.Threading.Tasks.TaskCanceledException) ) {
                    _logger.LogDebug( "Task was cancelled" );
                    taskCanceledException = true;
                }
            }

            if ( !taskCanceledException ) {
                throw;
            }
        }

        foreach ( var task in readerTasks ) {
            task.Result.Should().Be( messageCount );
            _logger.LogDebug( "Task had result {ResultMessageId}", task.Result );
        }

        _logger.LogDebug( "BroadcastQueue ended with {ReaderCount} readers", broadcastQueue.Writer.ReaderCount );
        _logger.LogDebug(
            "AddReaderTask created {ReaderCount} on {UniqueTaskIdCount} threads with an average interval between messages of {AverageInterval} ms",
            addReaderTaskRunner.Result.readerCount,
            addReaderTaskRunner.Result.uniqueThreadIds.Count,
            Math.Round(addReaderTaskRunner.Result.intervals.Average()/System.TimeSpan.TicksPerMillisecond,2) );
        broadcastQueue.Writer.ReaderCount.Should().Be( 4 );
    }


    [ Fact ]
    public void PublisherShouldWriteWithoutReaders( ) {
        _writeLine( $"In {nameof(PublisherShouldWriteWithoutReaders)}" );
        int messageCount   = 10_000;
        var broadcastQueue = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        var cts            = new CancellationTokenSource();

        var writerTaskNoReaders = writerTask( broadcastQueue.Writer, messageCount, cts.Token ); // must create / start last so that it doesn't write into nothing.

        Task.WaitAll(
            writerTaskNoReaders
        );
        writerTaskNoReaders.Result.Should().Be( messageCount );
    }

    [ Theory ]
    [ InlineData( 1 ) ]
    [ InlineData( 2 ) ]
    [ InlineData( 3 ) ]
    public void SubscribersShouldReceiveAllMessagesInOrder( int subscriberCount ) {
        int messageCount   = 10_000;
        var broadcastQueue = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        var cts            = new CancellationTokenSource();

        List<Task<int>> readerTasks = new List<Task<int>>();
        for ( int i = 0 ; i < subscriberCount ; i++ ) {
            readerTasks.Add( readerTask( broadcastQueue.GetReader(), messageCount, $"readerTask{i}", cts.Token ) );
        }

        List<Task> tasks = new List<Task>( readerTasks ) { writerTask( broadcastQueue.Writer, messageCount, cts.Token ) };


        Task.WaitAll(
            tasks.ToArray()
        );
        foreach ( var task in readerTasks ) {
            task.Result.Should().Be( messageCount );
            _logger.LogDebug( "Task had result {ResultMessageId}", task.Result );
        }
    }
}