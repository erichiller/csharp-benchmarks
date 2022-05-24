using System;
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

namespace BroadcastQueueTests;

// fixture
public class SetupFixture {
    private IHost   host;
    private ILogger logger;

    public static IHostBuilder CreateHostBuilder_BroadcastQueue( string[] args, LogLevel? logLevel ) =>
        Host.CreateDefaultBuilder( args )
            .ConfigureServices( ( hostContext, services ) => {
                services.AddSingleton<BroadcastQueue<ChannelMessage, ChannelResponse>>();
                if ( logLevel is { } ll ) {
                    services.AddLogging( logBuilder => logBuilder.AddConsole().SetMinimumLevel( ll ) );
                }
            } );

    private BroadcastQueue<ChannelMessage, ChannelResponse> _broadcastQueue;

    public async Task CreateHost<T>( LogLevel? logLevel ) {
        host = CreateHostBuilder_BroadcastQueue( Array.Empty<string>(), logLevel )
            .Build();
        await host.StartAsync();

        logger = host.Services.GetService<ILogger<T>>() ?? throw new Exception();
        // cts             = new CancellationTokenSource();
        _broadcastQueue = host.Services.GetService<BroadcastQueue<ChannelMessage, ChannelResponse>>() ?? throw new Exception();

        System.Console.WriteLine( "Host is running" );
    }
}

public record ChannelMessage {
    public int    Id         { get; set; }
    public string Property_1 { get; set; }
}

public record ChannelResponse(
    int               ReadId,
    string            ReaderType,
    System.Exception? Exception = null
) : IBroadcastQueueResponse;

public class BroadcastQueueTests {
    private readonly ITestOutputHelper _testOutputHelper;

    public BroadcastQueueTests( ITestOutputHelper testOutputHelper ) {
        _testOutputHelper = testOutputHelper;
    }

    // // setup
    // public void Setup( ) { }

    //
    // static async Task<int> writerTask ( BroadcastQueue<ChannelMessage, ChannelResponse> broadcastQueue, int messageCount, CancellationToken ct ) {
    //     int lastReadId = 0;
    //     while ( await broadcastQueue.Writer.WaitToReadResponseAsync( ct ) ) {
    //         if ( broadcastQueue.Writer.TryReadResponse( out var result ) ) {
    //             // logger.LogDebug( "Read {Id} from ResponseChannel", result?.ReadId );
    //             if ( result is { ReadId: int readId } ) {
    //                 lastReadId = readId;
    //                 // iterationCounter++;
    //                 if ( readId % messageCount == 0 ) {
    //                     break;
    //                 }
    //             }
    //         }
    //     }
    //
    //     return lastReadId;
    // }
    //
    //
    //
    // TODO
    [ Fact ]
    public async void PublisherShouldWriteWithoutReaders( ) {
        _testOutputHelper.WriteLine( $"In {nameof(PublisherShouldWriteWithoutReaders)}" );
        int messageCount   = 10_000;
        var broadcastQueue = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        var cts            = new CancellationTokenSource();

        var writerTaskNoReaders = writerTask( broadcastQueue.Writer, messageCount, cts.Token ); // must create / start last so that it doesn't write into nothing.

        Task.WaitAll(
            writerTaskNoReaders
        );
        writerTaskNoReaders.Result.Should().Be( messageCount );
    }


    // TODO
    [ Fact ]
    public async void SubscriberShouldReceiveAllMessagesInOrder( ) { }


    static async Task<int> writerTask( BroadcastQueueWriter<ChannelMessage, ChannelResponse> bqWriter, int messageCount, CancellationToken ct ) {
        int i = 0;
        while ( await bqWriter.WaitToWriteAsync( ct ) ) {
            while ( bqWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
                if ( i >= messageCount ) {
                    Console.WriteLine( $"[BroadcastQueueWriter] wrote messageCount: {i}" );
                    return i;
                }

                i++;
            }
        }


        // while ( await broadcastQueue.Writer.WaitToReadResponseAsync( ct ) ) {
        //     if ( broadcastQueue.Writer.TryReadResponse( out var result ) ) {
        //         // logger.LogDebug( "Read {Id} from ResponseChannel", result?.ReadId );
        //         if ( result is { ReadId: int readId } ) {
        //             lastReadId = readId;
        //             // iterationCounter++;
        //             if ( readId % messageCount == 0 ) {
        //                 break;
        //             }
        //         }
        //     }
        // }
        //
        // return lastReadId;
        return -1;
    }

    static async Task<int> writerTaskWhichAddsRemovesReaders( BroadcastQueue<ChannelMessage, ChannelResponse> bq, int messageCount, CancellationTokenSource cts ) {
        int i        = 0;
        var bqWriter = bq.Writer;
        var ct       = cts.Token;
        while ( await bqWriter.WaitToWriteAsync( ct ) ) {
            while ( bqWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
                if ( i % 1000 == 0 ) {
                    var result = readerTask( bq.GetReader(), messageCount, "readerTask1", cts.Token ).WaitAsync( ct ).Result;
                    // URGENT:: TEST FOR DEALOCKS!!!!!
                }

                if ( i >= messageCount ) {
                    Console.WriteLine( $"[BroadcastQueueWriter] wrote messageCount: {i}" );
                    return i;
                }

                i++;
            }
        }

        return -1;
    }


    static async Task<int> readerTask( BroadcastQueueReader<ChannelMessage, ChannelResponse> bqReader, int messageCount, string taskName, CancellationToken ct ) {
        int lastMessage = -1;
        Console.WriteLine( $"[BroadcastQueueReader] start" );
        while ( await bqReader.WaitToReadAsync( ct ) ) {
            Console.WriteLine( $"[BroadcastQueueReader] start receiving" );
            while ( bqReader.TryRead( out ChannelMessage? result ) ) {
                Console.WriteLine( $"[BroadcastQueueReader] received messageCount: {result.Id}" );
                if ( result.Id != lastMessage + 1 ) {
                    Console.WriteLine( $"[BroadcastQueueReader] ERROR: {result.Id}" );
                    await bqReader.WriteResponseAsync( new ChannelResponse( result.Id, taskName, new Exception( "Unexpected sequence" ) ), ct );
                    throw new Exception(); // TODO: Assert.False?
                }


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

    [ Fact ]
    public async void MultipleSubscribersShouldReceiveAllMessagesInOrder( ) {
        int messageCount   = 10_000;
        var broadcastQueue = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        var cts            = new CancellationTokenSource();

        var readerTask1 = readerTask( broadcastQueue.GetReader(), messageCount, "readerTask1", cts.Token );
        var readerTask2 = readerTask( broadcastQueue.GetReader(), messageCount, "readerTask2", cts.Token );
        var readerTask3 = readerTask( broadcastQueue.GetReader(), messageCount, "readerTask3", cts.Token );
        var writerTask1 = writerTask( broadcastQueue.Writer, messageCount, cts.Token ); // must create / start last so that it doesn't write into nothing.


        Task.WaitAll(
            readerTask1,
            readerTask2,
            readerTask3,
            writerTask1
        );
        readerTask1.Result.Should().Be( messageCount );
        readerTask2.Result.Should().Be( messageCount );
        readerTask3.Result.Should().Be( messageCount );
    }
}