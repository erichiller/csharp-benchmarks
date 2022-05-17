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
    // [ Params( 10, 10_000 ) ]
    [ Params( 1_000_000 ) ]
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

    private Channel<ChannelMessage>       _dataChannel;
    private ChannelReader<ChannelMessage> _dataChannelReader;
    private ChannelWriter<ChannelMessage> _dataChannelWriter;
    private Channel<ChannelResponse>      _responseChannel;

    [ GlobalSetup( Target = nameof(SimpleBackgroundServiceMessagingUsingChannels) ) ]
    public async Task CreateHost_SimpleChannelQueue( ) {
        host = CreateHostBuilder_SimpleChannelQueue( Array.Empty<string>() )
            .Build();
        await host.StartAsync();
        logger           = host.Services.GetService<ILogger<Benchmarks>>()      ?? throw new Exception();
        _dataChannel     = host.Services.GetService<Channel<ChannelMessage>>()  ?? throw new Exception();
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


    // URGENT: nothing has been checking the Response Channel!


    [ Benchmark ]
    public void RunBroadcastQueueWithoutHostTest( ) {
        _broadcastQueue = new BroadcastQueue<ChannelMessage, ChannelResponse>();

        async Task writerTask( ) {
            var writer = _broadcastQueue.Writer;
            int i      = 0;
            while ( await writer.WaitToWriteAsync() ) {
                while ( writer.TryWrite( new ChannelMessage() { Id = i } ) ) {
                    // Console.WriteLine( $"Wrote {i}" );
                    i++;
                    if ( i > MessageCount ) {
                        return;
                    }
                }
            }
        }

        async Task readerTask1( ) {
            var reader = _broadcastQueue.GetReader();
            while ( await reader.WaitToReadAsync() ) {
                while ( reader.TryRead( out ChannelMessage? result ) ) {
                    if ( result.Id >= MessageCount ) {
                        var response = new ChannelResponse( result.Id, nameof(readerTask1) );
                        await reader.WriteResponseAsync( response );
                        return;
                    }
                }
            }
        }

        Task.WaitAll(
            Task.Run( readerTask1 ),
            Task.Run( writerTask )
        );
    }


    [ Benchmark ]
    public void RunChannelsWithoutHostTest( ) {
        var dataChannel     = Channel.CreateUnbounded<ChannelMessage>();
        var dataReader      = dataChannel.Reader;
        var responseChannel = Channel.CreateUnbounded<ChannelResponse>();
        var responseReader  = responseChannel.Reader;

        async Task writerTask( ) {
            var dataWriter = dataChannel.Writer;
            int i          = 0;
            while ( await dataWriter.WaitToWriteAsync() ) {
                while ( dataWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
                    i++;
                    if ( i > MessageCount ) {
                        return;
                    }
                }
            }
        }


        async Task readerTask1( ) {
            var responseWriter = responseChannel.Writer;
            while ( await dataReader.WaitToReadAsync() ) {
                while ( dataReader.TryRead( out ChannelMessage? result ) ) {
                    if ( result.Id >= MessageCount ) {
                        var response = new ChannelResponse( result.Id, nameof(readerTask1) );
                        await responseWriter.WriteAsync( response );
                        return;
                    }
                }
            }
        }

        Task.WaitAll(
            Task.Run( readerTask1 ),
            Task.Run( writerTask )
        );
    }

    // [ Benchmark ]
    // public void RunBroadcastQueueWithoutHostReaderOnlyTest( ) {
    //     _broadcastQueue = new BroadcastQueue<ChannelMessage, ChannelResponse>();
    //
    //     async Task readerTask1( ) {
    //         var reader = _broadcastQueue.GetReader();
    //         while ( await reader.WaitToReadAsync() ) {
    //             while ( reader.TryRead( out ChannelMessage? result ) ) {
    //                 if ( result.Id >= MessageCount ) {
    //                     var response = new ChannelResponse( result.Id, nameof(readerTask1) );
    //                     await reader.WriteResponseAsync( response );
    //                     return;
    //                 }
    //             }
    //         }
    //     }
    //
    //     Task.WaitAll(
    //         Task.Run( readerTask1 )
    //     );
    // }
    //
    // [ Benchmark ]
    // public void RunChannelsWithoutHostReaderOnlyTest( ) {
    //     var dataChannel     = Channel.CreateUnbounded<ChannelMessage>();
    //     var dataReader      = dataChannel.Reader;
    //     var responseChannel = Channel.CreateUnbounded<ChannelResponse>();
    //     var responseReader  = responseChannel.Reader;
    //
    //
    //     async Task readerTask1( ) {
    //         var responseWriter = responseChannel.Writer;
    //         while ( await dataReader.WaitToReadAsync() ) {
    //             while ( dataReader.TryRead( out ChannelMessage? result ) ) {
    //                 if ( result.Id >= MessageCount ) {
    //                     var response = new ChannelResponse( result.Id, nameof(readerTask1) );
    //                     await responseWriter.WriteAsync( response );
    //                     return;
    //                 }
    //             }
    //         }
    //     }
    //
    //     Task.WaitAll(
    //         Task.Run( readerTask1 )
    //     );
    // }

    private BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader;
    private BroadcastQueueWriter<ChannelMessage, ChannelResponse> _broadcastQueueWriter;

    [ IterationSetup( Target = nameof(RunBroadcastQueueWithoutHostWriterOnlyTest) ) ]
    public void Setup_RunBroadcastQueueWithoutHostWriterOnlyTest( ) {
        _broadcastQueue       = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader = _broadcastQueue.GetReader();
        _broadcastQueueWriter = _broadcastQueue.Writer;
    }

    [ IterationCleanup( Target = nameof(RunBroadcastQueueWithoutHostWriterOnlyTest) ) ]
    public void Cleanup_RunBroadcastQueueWithoutHostWriterOnlyTest( ) {
        _broadcastQueue.Writer.Complete();
    }

    [ Benchmark ]
    public async Task RunBroadcastQueueWithoutHostWriterOnlyTest( ) {
        int i = 0;
        while ( await _broadcastQueueWriter.WaitToWriteAsync() ) {
            while ( _broadcastQueueWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
                i++;
                if ( i > MessageCount ) {
                    return;
                }
            }
        }
    }

    [ IterationSetup( Target = nameof(RunChannelsWithoutHostWriterOnlyTest) ) ]
    public void Setup_RunChannelsWithoutHostWriterOnlyTest( ) {
        _dataChannel       = Channel.CreateUnbounded<ChannelMessage>();
        _dataChannelReader = _dataChannel.Reader;
        _dataChannelWriter = _dataChannel.Writer;
    }

    [ IterationCleanup( Target = nameof(RunChannelsWithoutHostWriterOnlyTest) ) ]
    public void Cleanup_RunChannelsWithoutHostWriterOnlyTest( ) {
        _dataChannelWriter.Complete();
    }
    [ Benchmark ]
    public async Task RunChannelsWithoutHostWriterOnlyTest( ) {
        int i          = 0;
        while ( await _dataChannelWriter.WaitToWriteAsync() ) {
            while ( _dataChannelWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
                i++;
                if ( i > MessageCount ) {
                    return;
                }
            }
        }
    }
}