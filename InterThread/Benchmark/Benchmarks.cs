// #define DEBUG

#undef DEBUG

using System;
using System.Collections.Generic;
using System.Threading;
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
public partial class Benchmarks {
    // [ Params( 200_000 ) ]
    [ Params( 20_000, 200_000, 2_000_000 ) ]
    // [ Params( 10, 10_000 ) ]
    // [ Params( 1_000_000 ) ]
    // [ Params( 2_000_000 ) ]
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
                services.AddLogging( logBuilder => logBuilder.AddConsole().SetMinimumLevel( LogLevel.Error ) );
                // services.AddLogging( logBuilder => logBuilder.AddConsole().SetMinimumLevel( LogLevel.Debug ) );
                // services.AddSingleton<Channel<ChannelMessage>>( _ => Channel.CreateBounded<ChannelMessage>( 10 ) );
                // services.AddSingleton<Channel<ChannelResponse>>( _ => Channel.CreateBounded<ChannelResponse>( 10 ) );
                services.AddSingleton<Channel<ChannelMessage>>( _ => Channel.CreateUnbounded<ChannelMessage>( new UnboundedChannelOptions(){ SingleReader  = true, SingleWriter = true}) );
                services.AddSingleton<Channel<ChannelResponse>>( _ => Channel.CreateUnbounded<ChannelResponse>(new UnboundedChannelOptions(){ SingleReader = true, SingleWriter = true}) );
                services.AddHostedService<ChannelPublisher>();
                services.AddHostedService<ChannelSubscriber>();
                // services.AddTransient<ResponseChecker>();
            } );

    private Channel<ChannelMessage>        _dataChannel;
    private ChannelReader<ChannelMessage>  _dataChannelReader;
    private ChannelWriter<ChannelMessage>  _dataChannelWriter;
    private Channel<ChannelResponse>       _responseChannel;
    private ChannelReader<ChannelResponse> _responseChannelReader;
    private ChannelWriter<ChannelResponse> _responseChannelWriter;
    
    
    public async Task createHost( ) {
        // Console.WriteLine("createHost()");
        ChannelPublisher.MaxMessageCount  = MessageCount;
        ChannelSubscriber.MaxMessageCount = MessageCount;
        host = CreateHostBuilder_SimpleChannelQueue( Array.Empty<string>() )
            .Build();
        await host.StartAsync();
        // Console.WriteLine("createHost() started.");
        logger           = host.Services.GetService<ILogger<Benchmarks>>()      ?? throw new Exception();
        _dataChannel     = host.Services.GetService<Channel<ChannelMessage>>()  ?? throw new Exception();
        _responseChannel = host.Services.GetService<Channel<ChannelResponse>>() ?? throw new Exception();
        // Console.WriteLine("createHost() complete.");
    }

    [ IterationSetup( Target = nameof(Channels_InHost) ) ]
    public void CreateHost_SimpleChannelQueue( ) 
    =>         new Task( async ( ) => {
        await createHost();
        // Thread.Sleep( 500 ); // URGENT: This makes it complete too fast!!! // CURRENTLY, THE HOST STARTUP TIME IS ESSENTIALLY MEASURED, BECAUSE IT OCCURS AND STARTS PROCESSING BEFORE THE BENCHMARK
    } ).RunSynchronously();
    // {
        // Task task = createHost();
        // task.RunSynchronously();
    // }


    [ IterationCleanup( Target = nameof(Channels_InHost) ) ]
    public void StopHost_SimpleChannelQueue( ) {
        new Task( () => {
            host.StopAsync();
            _dataChannel.Writer.Complete();
            _responseChannel.Writer.Complete();
        } ).RunSynchronously();
    }
    

    [ Benchmark ]
    public void Channels_InHost( ) {
        var responseReader = _responseChannel.Reader;
        while ( true ) {
            if ( responseReader.TryRead( out ChannelResponse? result ) ) {
                if ( result.ReadId == MessageCount ) {
                    logger.LogDebug("Read {ReadId} == {MessageCount} ", result.ReadId, MessageCount);
                    return;
                }
            }
        }
    }

    /* *************************************************************************************************************
     * ************************************************************************************************************* 
     * ************************************************************************************************************* */


    public static IHostBuilder CreateHostBuilder_BroadcastQueue<TBroadcastQueue, TData, TResponse>(
        string[] args,
        int      subscriberCount,
        int      messageCount,
        LogLevel? logLevel
    ) where TBroadcastQueue : BroadcastQueue<TData, TResponse>  where TResponse : IBroadcastQueueResponse =>
        Host.CreateDefaultBuilder( args )
            .ConfigureServices( ( hostContext, services ) => {
                services.AddSingleton<BroadcastQueue<TData, TResponse>, TBroadcastQueue>();
                // URGENT: this needs testing!
                services.AddTransient<BroadcastQueueWriter<TData, TResponse>>( sp => {
                    BroadcastQueue<TData, TResponse> _broadcastQueue = sp.GetService<BroadcastQueue<TData, TResponse>>() ?? throw new Exception("Host exception"); // TODO: replace this exception with something more specific.
                    return _broadcastQueue.Writer;
                } );
                // URGENT: this needs testing!
                services.AddTransient<BroadcastQueueReader<TData, TResponse>>( sp => {
                    BroadcastQueue<TData, TResponse> _broadcastQueue = sp.GetService<BroadcastQueue<TData, TResponse>>() ?? throw new Exception("Host exception"); // TODO: replace this exception with something more specific.
                    return _broadcastQueue.GetReader();
                } );
                // if ( logLevel is { } ll ) {
                    // Console.WriteLine($"Logging added at level {ll}");
                    services.AddLogging( logBuilder => logBuilder.AddConsole().SetMinimumLevel( logLevel ?? LogLevel.Error ) );
                // }

                if ( subscriberCount is 1 or 2 or 3 ) {
                    services.AddHostedService<BroadcastPublisher>( );
                    BroadcastPublisher.MaxMessageCount = messageCount;
                    services.AddHostedService<BroadcastSubscriber>( );
                    BroadcastSubscriber.MaxMessageCount    = messageCount;
                    if ( subscriberCount > 1 ) services.AddHostedService<BroadcastSubscriberTwo>();
                    if ( subscriberCount > 2 ) services.AddHostedService<BroadcastSubscriberThree>();
                } else {
                    throw new ArgumentOutOfRangeException( nameof(subscriberCount), subscriberCount, "Must be 1 or 2 or 3" );
                }
            } );

    public static IHostBuilder CreateHostBuilder_BroadcastQueue<TBroadcastQueue>(
        int       subscriberCount,
        int       messageCount,
        string[]? args     = null,
        LogLevel? logLevel = null
    ) where TBroadcastQueue : BroadcastQueue<ChannelMessage, ChannelResponse> =>
        CreateHostBuilder_BroadcastQueue<TBroadcastQueue, ChannelMessage, ChannelResponse>( args ?? Array.Empty<string>(), subscriberCount, messageCount, logLevel );

    private BroadcastQueue<ChannelMessage, ChannelResponse> _broadcastQueue;
    
    /* ******** */

    [ IterationSetup( Target = nameof(BroadcastQueue_InHost_Vanilla_ShouldBeSameAsSingleXChannelOptions) ) ]
    public void CreateHost_InHost_Vanilla_ShouldBeSameAsSingleXChannelOptions( ) 
        => new Task( () => CreateHost<BroadcastQueue<ChannelMessage, ChannelResponse>>( subscriberCount: 1 ) ).RunSynchronously();

    /* ******** */
    
    [ IterationSetup( Target = nameof(BroadcastQueue_InHost_OneSubscriber_NoChannelOptions) ) ]
    public void CreateHostWithNoChannelOptions( ) 
        => new Task( () => CreateHost<BroadcastQueueWithNoChannelOptions<ChannelMessage, ChannelResponse>>( subscriberCount: 1 ) ).RunSynchronously();
    
    /* ******** */
    
    [ IterationSetup( Target = nameof(BroadcastQueue_InHost_OneSubscriber_SingleXChannelOptions) ) ]
    public void CreateHostWithOneSubscriberAndSingleXChannelOptions( ) 
        => new Task( () => CreateHost<BroadcastQueueWithSingleXChannelOptions<ChannelMessage, ChannelResponse>>( subscriberCount: 1 )).RunSynchronously();

    /* ******** */
    
    [ IterationSetup( Target = nameof(BroadcastQueue_InHost_TwoSubscribers) ) ]
    public void CreateHostWithTwoSubscribers( ) => new Task( () => CreateHost<BroadcastQueue<ChannelMessage, ChannelResponse>>( subscriberCount: 2 ) ).RunSynchronously();

    /* ******** */
    
    [ IterationSetup( Target = nameof(BroadcastQueue_InHost_ThreeSubscribers_NoChannelOptions) ) ]
    public void CreateHostWithThreeSubscribersAndNoChannelOptions( ) 
        => new Task( () => CreateHost<BroadcastQueueWithNoChannelOptions<ChannelMessage, ChannelResponse>>( subscriberCount: 3 ) ).RunSynchronously();

    /* ******** */
    
    [ IterationSetup( Target = nameof(BroadcastQueue_InHost_ThreeSubscribers_SingleXChannelOptions) ) ]
    public void CreateHostWithThreeSubscribersAndSingleXChannelOptions( ) 
        => new Task( () => CreateHost<BroadcastQueueWithSingleXChannelOptions<ChannelMessage, ChannelResponse>>( subscriberCount: 3 )).RunSynchronously();

    /* ******** */
    
    public async Task CreateHost<TBroadcastQueue>( int subscriberCount, LogLevel? logLevel = null ) where TBroadcastQueue : BroadcastQueue<ChannelMessage, ChannelResponse> {
        host = CreateHostBuilder_BroadcastQueue<TBroadcastQueue>( subscriberCount, messageCount: MessageCount, logLevel: logLevel )
            .Build();
        await host.StartAsync();

        logger = host.Services.GetService<ILogger<Benchmarks>>() ?? throw new Exception();
        // cts             = new CancellationTokenSource();
        _broadcastQueue = host.Services.GetService<BroadcastQueue<ChannelMessage, ChannelResponse>>() ?? throw new Exception();

        // System.Console.WriteLine( "Host is running" );
    }

    [ IterationCleanup( Targets = new[] {
        nameof(BroadcastQueue_InHost_Vanilla_ShouldBeSameAsSingleXChannelOptions),
        nameof(BroadcastQueue_InHost_OneSubscriber_NoChannelOptions),
        nameof(BroadcastQueue_InHost_OneSubscriber_SingleXChannelOptions),
        nameof(BroadcastQueue_InHost_TwoSubscribers),
        nameof(BroadcastQueue_InHost_ThreeSubscribers_NoChannelOptions),
        nameof(BroadcastQueue_InHost_ThreeSubscribers_SingleXChannelOptions)
    } ) ]
    public void StopHost( ) {
        new Task( () => host.StopAsync() ).RunSynchronously();
    }

    /* ******** */

    [ Benchmark ]
    public Task BroadcastQueue_InHost_Vanilla_ShouldBeSameAsSingleXChannelOptions( ) => RunBroadcastQueueTest();

    /* ******** */
    
    [ Benchmark ]
    public Task BroadcastQueue_InHost_OneSubscriber_NoChannelOptions( ) => RunBroadcastQueueTest();

    /* ******** */

    [ Benchmark ]
    public Task BroadcastQueue_InHost_OneSubscriber_SingleXChannelOptions( ) => RunBroadcastQueueTest();

    /* ******** */

    [ Benchmark ]
    public Task BroadcastQueue_InHost_TwoSubscribers( ) => RunBroadcastQueueTest();

    /* ******** */

    [ Benchmark ]
    public Task BroadcastQueue_InHost_ThreeSubscribers_NoChannelOptions( ) => RunBroadcastQueueTest();

    /* ******** */

    [ Benchmark ]
    public Task BroadcastQueue_InHost_ThreeSubscribers_SingleXChannelOptions( ) => RunBroadcastQueueTest();

    /* ******** */

    public async Task RunBroadcastQueueTest( ) {
        var readerCount             = _broadcastQueue.Writer.ReaderCount;
        var readersReachedReadCount = new Dictionary<string, int>( readerCount ); // must equal subscriberCount to be considered "complete"
        // Console.WriteLine($"ReadersCount={readerCount}");
        while ( await _broadcastQueue.Writer.WaitToReadResponseAsync() ) {
            while ( _broadcastQueue.Writer.TryReadResponse( out ChannelResponse? result ) ) {
                if ( result.ReadId == MessageCount ) {
                    logger.LogInformation( "readers reached count = {ReaderType}", result.ReaderType );
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




    /* *************************************************************************************************************
     * BROADCAST QUEUE - No Host, Writer Only ********************************************************************** 
     * ************************************************************************************************************* */

    private BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader1;
    private BroadcastQueueWriter<ChannelMessage, ChannelResponse> _broadcastQueueWriter;

    [ IterationSetup( Target = nameof(BroadcastQueue_WithoutHost_WriterOnly) ) ]
    public void Setup_BroadcastQueue_WithoutHost_WriterOnly( ) {
        _broadcastQueue       = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueWriter = _broadcastQueue.Writer;
    }

    [ IterationCleanup( Target = nameof(BroadcastQueue_WithoutHost_WriterOnly) ) ]
    public void Cleanup_BroadcastQueue_WithoutHost_WriterOnly( ) {
        _broadcastQueue.Writer.Complete();
    }

    [ Benchmark ]
    public async Task BroadcastQueue_WithoutHost_WriterOnly( ) {
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
    
    /* *************************************************************************************************************
     * CHANNELS - No Host, Writer Only ***************************************************************************** 
     * ************************************************************************************************************* */

    [ IterationSetup( Target = nameof(Channels_WithoutHost_WriterOnly) ) ]
    public void Setup_Channels_WithoutHost_WriterOnly( ) {
        _dataChannel       = Channel.CreateUnbounded<ChannelMessage>();
        _dataChannelReader = _dataChannel.Reader;
        _dataChannelWriter = _dataChannel.Writer;
    }

    [ IterationCleanup( Target = nameof(Channels_WithoutHost_WriterOnly) ) ]
    public void Cleanup_Channels_WithoutHost_WriterOnly( ) {
        _dataChannelWriter.Complete();
    }
    [ Benchmark ]
    public async Task Channels_WithoutHost_WriterOnly( ) {
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