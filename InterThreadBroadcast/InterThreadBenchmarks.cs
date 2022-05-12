using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;

using Benchmarks.Common;
using Benchmarks.InterThreadBroadcast;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Benchmarks.InterThreadBroadcast {
    [ Config( typeof(BenchmarkConfig) ) ]
    public class InterThreadBenchmarks {


        [ Params( 1000 ) ]
        public int MessageCount;

        public static IHostBuilder CreateHostBuilder_SimpleChannelQueue( string[] args ) =>
            Host.CreateDefaultBuilder( args )
                .ConfigureServices( ( hostContext, services ) => {
                    services.AddSingleton<Channel<ChannelMessage>>( s => Channel.CreateBounded<ChannelMessage>( 10 ) );
                    services.AddSingleton<Channel<ChannelResponse>>( s => Channel.CreateBounded<ChannelResponse>( 10 ) );
                    services.AddHostedService<ChannelSubscriber>();
                    services.AddHostedService<ChannelPublisher>();
                    // services.AddLogging( logBuilder => logBuilder.AddConsole().SetMinimumLevel( LogLevel.Debug ) );
                    // services.AddTransient<ResponseChecker>();
                } );

        private Channel<ChannelMessage>  channel;
        private Channel<ChannelResponse> responseChannel;
        private ILogger<InterThreadBenchmarks>  logger;

        private IHost host;

        // private CancellationTokenSource cts;

        [ GlobalSetup(Target = nameof(SimpleBackgroundServiceMessagingUsingChannels)) ]
        // [IterationSetup]
        public async Task CreateHost_SimpleChannelQueue( ) {
            host = InterThreadBenchmarks.CreateHostBuilder_SimpleChannelQueue( Array.Empty<string>() )
                                        .Build();
            await host.StartAsync();
            
            System.Console.WriteLine( "Host is running" );
            logger = host.Services.GetService<ILogger<InterThreadBenchmarks>>() ?? throw new Exception();
            // cts    = new CancellationTokenSource();
            channel         = host.Services.GetService<Channel<ChannelMessage>>()  ?? throw new Exception();
            responseChannel = host.Services.GetService<Channel<ChannelResponse>>() ?? throw new Exception();
        }

        [ GlobalCleanup (Target = nameof(SimpleBackgroundServiceMessagingUsingChannels)) ]
        // [IterationCleanup]
        public async Task StopHost_SimpleChannelQueue( ) {
            channel.Writer.Complete();
            await host.StopAsync();
        }

        [ Benchmark ]
        public void SimpleBackgroundServiceMessagingUsingChannels( ) {
            int lastReadId = 0;

            // var stopwatch = new Stopwatch();
            // stopwatch.Start();
            // var ct             = cts.Token;
            int iterationCounter = 0;
            var responseReader   = responseChannel.Reader;
            while ( true ) {
                // if ( responseReader.WaitToReadAsync( ct ) ) {
                    // var result = await responseReader.ReadAsync( ct );
                    if ( responseReader.TryRead( out ChannelResponse result ) ) {
                     
                        logger.LogDebug( "Read {Id} from ResponseChannel", result.ReadId );
                        lastReadId = result.ReadId;
                        iterationCounter++;
                        if ( result.ReadId % MessageCount == 0 ) {
                            break;
                        }   
                    }
                // }
            }

            // channel.Writer.Complete();
            // logger.LogInformation( "Reading Complete in {time}. Last Read ID: {id}", stopwatch.Elapsed, lastReadId );
            // logger.LogInformation( "Reading Complete. {NumberIterations}. Last Read ID: {id}", iterationCounter, lastReadId );
        }
        
        
        /* *************************************************************************************************************
         */
        
        
        public static IHostBuilder CreateHostBuilder_BroadcastQueue( string[] args, LogLevel logLevel = LogLevel.Information ) =>
            Host.CreateDefaultBuilder( args )
                .ConfigureServices( ( hostContext, services ) => {
                    services.AddSingleton<BroadcastQueue<ChannelMessage, ChannelResponse>>();
                    services.AddLogging( logBuilder => logBuilder.AddConsole().SetMinimumLevel( logLevel ) );
                    services.AddHostedService<BroadcastPublisher>();
                    services.AddHostedService<BroadcastSubscriber>();
                } );

        private BroadcastQueue<ChannelMessage, ChannelResponse> _broadcastQueue;

        [ GlobalSetup(Target = nameof(BroadcastQueue)) ]
        public async Task CreateHost( LogLevel logLevel = LogLevel.Information ) {
            host = InterThreadBenchmarks.CreateHostBuilder_BroadcastQueue( Array.Empty<string>(), logLevel )
                                        .Build();
            await host.StartAsync();
            
            logger          = host.Services.GetService<ILogger<InterThreadBenchmarks>>()      ?? throw new Exception();
            // cts             = new CancellationTokenSource();
            _broadcastQueue = host.Services.GetService<BroadcastQueue<ChannelMessage, ChannelResponse>>() ?? throw new Exception();

            System.Console.WriteLine( "Host is running" );
            int lastReadId = 0;
        }

        [ GlobalCleanup (Target = nameof(BroadcastQueue)) ]
        public async Task StopHost( ) {
            channel.Writer.Complete();
            await host.StopAsync();
        }

        [ Benchmark ]
        public void BroadcastQueue( ) {
            // Implement your benchmark here
        }
    }
}