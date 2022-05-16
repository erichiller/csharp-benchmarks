// #define DEBUG
#undef DEBUG

using System;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;
using Benchmarks.InterThread.BroadcastQueue;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Benchmarks.InterThread.Benchmark;

[ Config( typeof(BenchmarkConfig) ) ]
public class Benchmarks {
    // [ Params( 1_000, 10_000, 1_000_000 ) ]
    [ Params( 10, 10_000 ) ]
    public int MessageCount;

    /* *************************************************************************************************************
     * ************************************************************************************************************* 
     * ************************************************************************************************************* */

    private IHost               host;
    private ILogger<Benchmarks> logger;

    /* *************************************************************************************************************
     * ************************************************************************************************************* 
     * ************************************************************************************************************* */

    public static IHostBuilder CreateHostBuilder_SimpleChannelQueue( string[] args ) =>
        Host.CreateDefaultBuilder( args )
            .ConfigureServices( ( hostContext, services ) => {
                // services.AddSingleton<Channel<ChannelMessage>>( _ => Channel.CreateBounded<ChannelMessage>( 10 ) );
                // services.AddSingleton<Channel<ChannelResponse>>( _ => Channel.CreateBounded<ChannelResponse>( 10 ) );
                services.AddSingleton<Channel<ChannelMessage>>( _ => Channel.CreateUnbounded<ChannelMessage>() );
                services.AddSingleton<Channel<ChannelResponse>>( _ => Channel.CreateUnbounded<ChannelResponse>() );
                services.AddHostedService<ChannelPublisher>();
                services.AddHostedService<ChannelSubscriber>();
                services.AddLogging( logBuilder => logBuilder.AddConsole().SetMinimumLevel( LogLevel.Debug ) );
                // services.AddTransient<ResponseChecker>();
            } );

    private Channel<ChannelMessage>  _dataChannel;
    private Channel<ChannelResponse> _responseChannel;

    [ GlobalSetup( Target = nameof(SimpleBackgroundServiceMessagingUsingChannels) ) ]
    public async Task CreateHost_SimpleChannelQueue( ) {
        host = CreateHostBuilder_SimpleChannelQueue( Array.Empty<string>() )
            .Build();
        await host.StartAsync();
        logger          = host.Services.GetService<ILogger<Benchmarks>>()      ?? throw new Exception();
        _dataChannel         = host.Services.GetService<Channel<ChannelMessage>>()  ?? throw new Exception();
        _responseChannel = host.Services.GetService<Channel<ChannelResponse>>() ?? throw new Exception();
    }

    [ GlobalCleanup( Target = nameof(SimpleBackgroundServiceMessagingUsingChannels) ) ]
    public async Task StopHost_SimpleChannelQueue( ) {
        _dataChannel.Writer.Complete();
        _responseChannel.Writer.Complete();
        await host.StopAsync();
    }

    [ Benchmark ]
    public void SimpleBackgroundServiceMessagingUsingChannels( ) {
        var responseReader = _responseChannel.Reader;
        while ( true ) {
            if ( responseReader.TryRead( out ChannelResponse? result ) ) {
                if ( result.ReadId % MessageCount == 0 ) {
                    break;
                }
            }
        }
    }

    /* *************************************************************************************************************
     * ************************************************************************************************************* 
     * ************************************************************************************************************* */


    public static IHostBuilder CreateHostBuilder_BroadcastQueue<TBroadcastQueue, TData, TResponse>(
        string[]  args,
        int       subscriberCount,
        LogLevel? logLevel
    ) where TBroadcastQueue : BroadcastQueue<TData, TResponse> =>
        Host.CreateDefaultBuilder( args )
            .ConfigureServices( ( hostContext, services ) => {
                services.AddSingleton<BroadcastQueue<TData, TResponse>, TBroadcastQueue>();
                if ( logLevel is { } ll ) {
                    services.AddLogging( logBuilder => logBuilder.AddConsole().SetMinimumLevel( ll ) );
                }

                if ( subscriberCount is 1 or 2 or 3 ) {
                    services.AddHostedService<BroadcastPublisher>();
                    services.AddHostedService<BroadcastSubscriber>();
                    if ( subscriberCount > 1 ) services.AddHostedService<BroadcastSubscriberTwo>();
                    if ( subscriberCount > 2 ) services.AddHostedService<BroadcastSubscriberThree>();
                } else {
                    throw new ArgumentOutOfRangeException( nameof(subscriberCount), subscriberCount, "Must be 1 or 2 or 3" );
                }
            } );

    public static IHostBuilder CreateHostBuilder_BroadcastQueue<TBroadcastQueue>(
        int       subscriberCount,
        string[]? args     = null,
        LogLevel? logLevel = null
    ) where TBroadcastQueue : BroadcastQueue<ChannelMessage, ChannelResponse> =>
        CreateHostBuilder_BroadcastQueue<TBroadcastQueue, ChannelMessage, ChannelResponse>( args ?? Array.Empty<string>(), subscriberCount, logLevel );

    private BroadcastQueue<ChannelMessage, ChannelResponse> _broadcastQueue;

    [ GlobalSetup( Target = nameof(BroadcastQueue_Vanilla_ShouldBeSameAsSingleXChannelOptions) ) ]
    public Task CreateHost_Vanilla_ShouldBeSameAsSingleXChannelOptions( ) => CreateHost<BroadcastQueue<ChannelMessage, ChannelResponse>>( subscriberCount: 1 );

    [ GlobalSetup( Target = nameof(BroadcastQueueWithOneSubscriberAndNoChannelOptions) ) ]
    public Task CreateHostWithNoChannelOptions( ) => CreateHost<BroadcastQueueWithNoChannelOptions<ChannelMessage, ChannelResponse>>( subscriberCount: 1 );

    [ GlobalSetup( Target = nameof(BroadcastQueueWithOneSubscriberAndSingleXChannelOptions) ) ]
    public Task CreateHostWithOneSubscriberAndSingleXChannelOptions( ) => CreateHost<BroadcastQueueWithSingleXChannelOptions<ChannelMessage, ChannelResponse>>( subscriberCount: 1 );

    [ GlobalSetup( Target = nameof(BroadcastQueueWithTwoSubscribers) ) ]
    public Task CreateHostWithTwoSubscribers( ) => CreateHost<BroadcastQueue<ChannelMessage, ChannelResponse>>( subscriberCount: 2 );

    [ GlobalSetup( Target = nameof(BroadcastQueueWithThreeSubscribersAndNoChannelOptions) ) ]
    public Task CreateHostWithThreeSubscribersAndNoChannelOptions( ) => CreateHost<BroadcastQueueWithNoChannelOptions<ChannelMessage, ChannelResponse>>( subscriberCount: 3 );

    [ GlobalSetup( Target = nameof(BroadcastQueueWithThreeSubscribersAndSingleXChannelOptions) ) ]
    public Task CreateHostWithThreeSubscribersAndSingleXChannelOptions( ) => CreateHost<BroadcastQueueWithSingleXChannelOptions<ChannelMessage, ChannelResponse>>( subscriberCount: 3 );

    public async Task CreateHost<TBroadcastQueue>( int subscriberCount, LogLevel? logLevel = null ) where TBroadcastQueue : BroadcastQueue<ChannelMessage, ChannelResponse> {
        host = CreateHostBuilder_BroadcastQueue<TBroadcastQueue>( subscriberCount, logLevel: logLevel )
            .Build();
        await host.StartAsync();

        logger = host.Services.GetService<ILogger<Benchmarks>>() ?? throw new Exception();
        // cts             = new CancellationTokenSource();
        _broadcastQueue = host.Services.GetService<BroadcastQueue<ChannelMessage, ChannelResponse>>() ?? throw new Exception();

        System.Console.WriteLine( "Host is running" );
    }

    [ GlobalCleanup( Targets = new[] {
        nameof(BroadcastQueueWithOneSubscriberAndNoChannelOptions),
        nameof(BroadcastQueueWithOneSubscriberAndSingleXChannelOptions),
        nameof(BroadcastQueueWithTwoSubscribers),
        nameof(BroadcastQueueWithThreeSubscribersAndNoChannelOptions),
        nameof(BroadcastQueueWithThreeSubscribersAndSingleXChannelOptions)
    } ) ]
    public async Task StopHost( ) {
        await host.StopAsync();
    }

    [ Benchmark ]
    public Task BroadcastQueue_Vanilla_ShouldBeSameAsSingleXChannelOptions( ) => RunBroadcastQueueTest();

    [ Benchmark ]
    public Task BroadcastQueueWithOneSubscriberAndNoChannelOptions( ) => RunBroadcastQueueTest();

    [ Benchmark ]
    public Task BroadcastQueueWithOneSubscriberAndSingleXChannelOptions( ) => RunBroadcastQueueTest();

    [ Benchmark ]
    public Task BroadcastQueueWithTwoSubscribers( ) => RunBroadcastQueueTest();

    // [ Benchmark ]
    // public Task BroadcastQueueWithThreeSubscribers( ) => RunBroadcastQueueTest();

    [ Benchmark ]
    public Task BroadcastQueueWithThreeSubscribersAndNoChannelOptions( ) => RunBroadcastQueueTest();

    [ Benchmark ]
    public Task BroadcastQueueWithThreeSubscribersAndSingleXChannelOptions( ) => RunBroadcastQueueTest();

    public async Task RunBroadcastQueueTest( ) {
        var readerCount             = _broadcastQueue.ReaderCount;
        var readersReachedReadCount = new Dictionary<string, int>( readerCount ); // must equal subscriberCount to be considered "complete"
        // Console.WriteLine($"ReadersCount={readerCount}");
        while ( await _broadcastQueue.Writer.WaitToReadResponseAsync() ) {
            while ( _broadcastQueue.Writer.TryReadResponse( out ChannelResponse? result ) ) {
                if ( result.ReadId % MessageCount == 0 ) {
                    readersReachedReadCount[ result.ReaderType ] = result.ReadId;
                    if ( readersReachedReadCount.Count == readerCount ) {
                        bool reachedTargetReadCount = true;
                        foreach ( var (readerType, readerLastReadId) in readersReachedReadCount ) {
                            if ( readerLastReadId < MessageCount ) {
                                reachedTargetReadCount = false;
                                break;
                            }
                        }

                        if ( reachedTargetReadCount ) {
                            return;
                        }
                    }
                }
            }
        }
    }

    // [ GlobalSetup( Target = nameof(RunBroadcastQueueWithoutHostTest) ) ]
    // public void CreateBroadcastQueue( ) {
    // var broadcastQueue = new BroadcastQueue<ChannelMessage, ChannelResponse>();
    // broadcastQueue.Writer
    // }
    [ Benchmark ]
    public async Task RunBroadcastQueueWithoutHostTest( ) {
        // URGENT: try this
        _broadcastQueue = new BroadcastQueue<ChannelMessage, ChannelResponse>();

        // int lastRead = 0
        // while ( true ) {
        // static async void writerTask ( BroadcastQueueWriter<ChannelMessage, ChannelResponse> writer ) {
        async Task writerTask( ) {
            var writer = _broadcastQueue.Writer;
            int i      = 0;
#if DEBUG
            Console.WriteLine( $"Start Waiting to Write on thread # {Thread.CurrentThread.ManagedThreadId}" );
#endif
            while ( await writer.WaitToWriteAsync() ) {
#if DEBUG
                Console.WriteLine( "Waiting to Write" );
#endif
                while ( writer.TryWrite( new ChannelMessage() { Id = i } ) ) {
                    // Console.WriteLine( $"Wrote {i}" );
                    i++;
#if DEBUG
                    if ( i % 10 == 0 ) {
                        Console.WriteLine( $"Wrote {i}" );
                        // await Task.Delay( 1000 );
                    }
#endif
                    if ( i > MessageCount ) {
                        return;
                    }
                }
            }
        }

        int lastIdRead = 0;

        async Task readerTask1( ) {
            var reader = _broadcastQueue.GetReader();
#if DEBUG
            Console.WriteLine( $"Start Waiting to Read on thread # {Thread.CurrentThread.ManagedThreadId}" );
#endif
            while ( await reader.WaitToReadAsync() ) {
#if DEBUG
                Console.WriteLine( "Waiting to Read" );
#endif
                while ( reader.TryRead( out ChannelMessage? result ) ) {
#if DEBUG
                    Console.WriteLine( $"Read {result.Id}" );
#endif
                    if ( result.Id >= MessageCount ) {
                        lastIdRead = result.Id;
                        var response = new ChannelResponse( result.Id, nameof(readerTask1) );
                        await reader.WriteResponseAsync( response );
#if DEBUG
                        Console.WriteLine( $"Completed Read with Id: {result.Id} ; Was waiting for messageCount: {MessageCount}" );
#endif
                        return;
                    }
                }
            }
        }

        Task.WaitAll(
            Task.Run( readerTask1 ),
            Task.Run( writerTask )
        );

#if DEBUG
        Console.WriteLine( $"Complete {nameof(RunBroadcastQueueWithoutHostTest)}. Last ID Read was: {lastIdRead}. Was waiting for messageCount: {MessageCount}" );
#endif
    }
    
    
    
    
    
    // URGENT: nothing has been checking the Response Channel!
    
    [ Benchmark ]
    public async Task RunChannelsWithoutHostTest( ) {
        // URGENT: try this
        var dataChannel           = Channel.CreateUnbounded<ChannelMessage>();
        var dataReader     = dataChannel.Reader;
        var responseChannel       = Channel.CreateUnbounded<ChannelResponse>();
        var responseReader = responseChannel.Reader;

        // int lastRead = 0
        // while ( true ) {
        // static async void writerTask ( BroadcastQueueWriter<ChannelMessage, ChannelResponse> writer ) {
        async Task writerTask( ) {
            var dataWriter = dataChannel.Writer;
            int i          = 0;
#if DEBUG
            Console.WriteLine( $"Start Waiting to Write on thread # {Thread.CurrentThread.ManagedThreadId}" );
#endif
            while ( await dataWriter.WaitToWriteAsync() ) {
#if DEBUG
                Console.WriteLine( "Waiting to Write" );
#endif
                while ( dataWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
                    // Console.WriteLine( $"Wrote {i}" );
                    i++;
#if DEBUG
                    if ( i % 100 == 0 ) {
                        Console.WriteLine( $"Wrote {i}" );
                        // await Task.Delay( 1000 );
                    }
#endif
                    if ( i > MessageCount ) {
                        return;
                    }
                }
            }
        }

        int lastIdRead = 0;

        async Task readerTask1( ) {
            var responseWriter = responseChannel.Writer;
#if DEBUG
            Console.WriteLine( $"Start Waiting to Read on thread # {Thread.CurrentThread.ManagedThreadId}" );
#endif
            while ( await dataReader.WaitToReadAsync() ) {
#if DEBUG
                Console.WriteLine( "Waiting to Read" );
#endif
                while ( dataReader.TryRead( out ChannelMessage? result ) ) {
#if DEBUG
                    Console.WriteLine( $"Read {result.Id}" );
#endif
                    if ( result.Id >= MessageCount ) {
                        lastIdRead = result.Id;
                        var response = new ChannelResponse( result.Id, nameof(readerTask1) );
                        await responseWriter.WriteAsync( response );
#if DEBUG
                        Console.WriteLine( $"Completed {nameof(readerTask1)}. Read Id: {result.Id} ; Was waiting for messageCount: {MessageCount}" );
#endif
                        return;
                    }
                }
            }
        }

        Task.WaitAll(
            Task.Run( writerTask ),
            Task.Run( readerTask1 )
        );

#if DEBUG
        if ( responseReader.TryRead( out ChannelResponse? lastResponse ) ) {
            Console.WriteLine($"Complete. Read Response with ID {lastResponse.ReadId} from {lastResponse.ReaderType}");
            if ( lastResponse.ReadId != MessageCount ) {
                throw new Exception( $"Received {lastResponse.ReadId} but expected {MessageCount}" );
            }
        } else {
            throw new Exception();
        }

        if ( dataReader.TryRead( out ChannelMessage? firstUnreadDataMessage ) ) {
            int lastDataIdInQueue = firstUnreadDataMessage.Id;
            await foreach ( var message in dataReader.ReadAllAsync() ) {
                lastDataIdInQueue = message.Id;
            }

            Console.WriteLine($"Complete. Last Data (unread) started with ID {firstUnreadDataMessage.Id} and ended with {lastDataIdInQueue}");
        }

        Console.WriteLine( $"Complete {nameof(RunChannelsWithoutHostTest)}. Last ID Read was: {lastIdRead}. Was waiting for messageCount: {MessageCount}" );
#endif
        // await Task.Delay( 1000 );
    }
}