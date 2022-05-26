using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Benchmarks.InterThread.BroadcastQueue;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

    public BroadcastQueueTests( ITestOutputHelper testOutputHelper ) : base( testOutputHelper ){ }

    static async Task<int> writerTask( BroadcastQueueWriter<ChannelMessage, ChannelResponse> bqWriter, int messageCount, CancellationToken ct, ILogger? logger = null ) {
        int i = 0;
        while ( await bqWriter.WaitToWriteAsync( ct ) ) {
            while ( bqWriter.TryWrite( new ChannelMessage( i ) ) ) {
                if ( i >= messageCount ) {
                    logger?.LogDebug( "[BroadcastQueueWriter] wrote messageCount: {MessageCount}", i );
                    return i;
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
                // if ( result.Id != lastMessage + 1 ) {
                    // logger?.LogDebug( "[BroadcastQueueReader] ERROR at message ID: {MessageId}", result.Id );
                    // await bqReader.WriteResponseAsync( new ChannelResponse( result.Id, taskName, new Exception( "Unexpected sequence" ) ), ct );
                    // throw new Exception(); // TODO: Assert.False?
                // }

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

    static async Task<int> addReaderTask( BroadcastQueue<ChannelMessage, ChannelResponse> broadcastQueue, CancellationToken ct, ILogger? logger = null ) {
        int readersAdded = 0;
        while ( !ct.IsCancellationRequested ) {
            var random = new Random();
            var reader = broadcastQueue.GetReader();
            reader.TryRead( out ChannelMessage? message );
            logger?.LogDebug( "New reader read: {Message}", message );
            await Task.Delay( random.Next( 1, 2000 ), ct );
            broadcastQueue.RemoveReader( reader );
            await Task.Delay( random.Next( 1, 2000 ), ct );
            readersAdded++;
        }

        return readersAdded;
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

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void SubscribersShouldReceiveAllMessagesInOrder( int subscriberCount ) {
        int messageCount   = 10_000;
        var broadcastQueue = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        var cts            = new CancellationTokenSource();

        List<Task<int>> readerTasks = new List<Task<int>>();
        for ( int i = 0 ; i < subscriberCount ; i++ ) {
            readerTasks.Add( readerTask( broadcastQueue.GetReader(), messageCount, $"readerTask{i}", cts.Token ) );
        }

        List<Task> tasks = new List<Task>(readerTasks) {
            writerTask( broadcastQueue.Writer, messageCount, cts.Token )
        };


        Task.WaitAll(
            tasks.ToArray()
        );
        foreach ( var task in readerTasks ) {
            task.Result.Should().Be( messageCount );
            _logger?.LogDebug( "Task had result {ResultMessageId}", task.Result );
        }
    }

    [Fact]
    public void AddingAndRemovingReadersShouldNeverError( ) { // URGENT: deadlock test
        int subscriberCount = 3;
        int messageCount    = 10_000;
        var broadcastQueue  = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        var cts             = new CancellationTokenSource();
        cts.CancelAfter( 5_000 );

        List<Task<int>> readerTasks = new List<Task<int>>();
        for ( int i = 0 ; i < subscriberCount ; i++ ) {
            readerTasks.Add( readerTask( broadcastQueue.GetReader(), messageCount, $"readerTask{i}", cts.Token ) );
        }

        List<Task> tasks = new List<Task>(readerTasks) {
            writerTask( broadcastQueue.Writer, messageCount, cts.Token ),
            addReaderTask( broadcastQueue, cts.Token )
        };

        try {
            Task.WaitAll(
                tasks.ToArray()
            );
        } catch ( TaskCanceledException ) {
            _logger?.LogDebug( "Task cancelled" );
        }

        foreach ( var task in readerTasks ) {
            task.Result.Should().Be( messageCount );
            _logger?.LogDebug( "Task had result {ResultMessageId}", task.Result );
        }
    }
}