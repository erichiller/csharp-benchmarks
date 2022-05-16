using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Benchmarks.InterThread.BroadcastQueue;

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
                // URGENT. Is this needed: ??
                // services.AddHostedService<BroadcastPublisher>(); // URGENT --- publisher before subscriber = hang! What to do with  Writer if no Reader?
                // services.AddHostedService<BroadcastSubscriber>();
                // services.AddHostedService<BroadcastSubscriberTwo>();
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

public record ChannelResponse {
    public int ReadId { get; set; }
}

public class BroadcastQueueTests {
    private readonly ITestOutputHelper _testOutputHelper;
    public BroadcastQueueTests( ITestOutputHelper testOutputHelper ) {
        _testOutputHelper = testOutputHelper;
    }

    // setup
    public void Setup( ) { }
    
    // TODO
    [ Fact ]
    public async void PublisherShouldNotWriteWithoutReaders( ) { 
        int MessageCount = 500;
        _testOutputHelper.WriteLine( $"In {nameof(PublisherShouldNotWriteWithoutReaders)}" );
        IHost                        host;
        CancellationTokenSource      cts = new CancellationTokenSource();
        ILogger<BroadcastQueueTests> logger;
        // int                     lastReadId = 0;
        CancellationToken       ct         = cts.Token;
        
        
        host = SetupFixture.CreateHostBuilder_BroadcastQueue( Array.Empty<string>(), LogLevel.Debug ).Build();
        await host.StartAsync(ct);


        _testOutputHelper.WriteLine( "Host is running" );
        var broadcastQueue = host.Services.GetService<BroadcastQueue<ChannelMessage, ChannelResponse>>() ?? throw new Exception();
        
        logger = host.Services.GetService<ILogger<BroadcastQueueTests>>() ?? throw new Exception();

        var task = Task.Run( async ( ) => {
            int lastReadId = 0;
            while ( await broadcastQueue.Writer.WaitToReadResponseAsync() ) {
                if ( broadcastQueue.Writer.TryReadResponse( out var result ) ) {
                    // logger.LogDebug( "Read {Id} from ResponseChannel", result?.ReadId );
                    if ( result is { ReadId: int readId } ) {
                        lastReadId = readId;
                        // iterationCounter++;
                        if ( readId % MessageCount == 0 ) {
                            break;
                        }
                    }
                }
            }

            return lastReadId;
        } );
        task.RunSynchronously();
        Debug.Assert( task.Result == MessageCount );
    }


    // TODO
    [ Fact ]
    public async void SubscriberShouldReceiveAllMessagesInOrder( ) { }


    // TODO
    [ Fact ]
    public async void MultipleSubscribersShouldReceiveAllMessagesInOrder( ) {
        
    }
}