// #define DEBUG

#undef DEBUG
// #define DEBUG_BROADCAST
// #define DEBUG_CHANNEL
// #define DEBUG_OBSERVER
// # define DEBUG_THREAD_PRIORITY


using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Extensions;
using BenchmarkDotNet.Running;

using Benchmarks.Common;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Benchmarks.InterThread.BroadcastQueue;

namespace Benchmarks.InterThread.Benchmark;

public class Program {
#if DEBUG
    // public class ResponseChecker {
    //     private ChannelReader<ChannelResponse> _reader;
    //
    //     public ResponseChecker( Channel<ChannelResponse> responseChannel ) {
    //         _reader = responseChannel.Reader;
    //     }
    //
    //     public async Task<bool> WaitForId( int id ) {
    //         int lastId = 0;
    //         while ( _reader.TryRead(  ) ) {
    //             
    //             var message = await this._reader.ReadAsync();
    //             lastId = message.ReadId;
    //             System.Console.WriteLine( $"Last ID Read: {lastId}" );
    //         }
    //
    //         return false;
    //     }
    // }


    static async Task<int> Main( string[] args ) {
        IHost                   host;
        CancellationTokenSource cts = new CancellationTokenSource();
        ILogger<Program>        logger;
        int                     lastReadId = 0;
        CancellationToken       ct = cts.Token;
        Stopwatch               stopwatch;

        // System.Console.WriteLine( $"Starting Program. Thread ID: {Thread.CurrentThread.ManagedThreadId}" );

        /* ************************************************************************ */

#if DEBUG_THREAD_PRIORITY
        Console.WriteLine($"PRE: Current Thread Priority is {Thread.CurrentThread.Priority}");
        Thread.CurrentThread.Priority = ThreadPriority.Highest;
        Console.WriteLine($"POST: Current Thread Priority is {Thread.CurrentThread.Priority}");
        // var loggerFactory = LoggerFactory.Create( builder => builder.AddConsole().SetMinimumLevel( LogLevel.Debug ));
        // logger = loggerFactory.CreateLogger<Program>();

        Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
        // Process.GetCurrentProcess().TrySetPriority( ProcessPriorityClass.High, BenchmarkDotNet.Loggers.ConsoleLogger.Default );

        int i = 0;
        while ( i < 20 ) {
            Thread.Sleep( 1000 );
            i++;
        }

#endif

#if DEBUG_BROADCAST

        // const int readCount = 100;
        // const int readCount = 500;
        // const int readCount = 10_000;
        const int readCount = 100_000;
        const int subscriberCount = 3;

        var benchmarks = new Benchmarks() { MessageCount = readCount };

        #region In Host

        // await benchmarks.CreateHost<BroadcastQueue<ChannelMessage, ChannelResponse>>( subscriberCount: 3, LogLevel.Debug );
        // await benchmarks.BroadcastQueue_InHost_ThreeSubscribers_NoChannelOptions();
        // benchmarks.StopHost();
        //
        // benchmarks = new Benchmarks() { MessageCount = readCount };
        // benchmarks.CreateHostWithNoChannelOptions();
        // await benchmarks.BroadcastQueue_InHost_OneSubscriber_NoChannelOptions();
        // benchmarks.StopHost();

        #endregion

        #region WriterOnly

        // benchmarks.Setup_RunBroadcastQueueWithoutHostWriterOnlyTest();
        // await benchmarks.BroadcastQueue_WithoutHost_WriterOnly();
        // benchmarks.Cleanup_RunBroadcastQueueWithoutHostWriterOnlyTest();
        // benchmarks.Setup_RunChannelsWithoutHostWriterOnlyTest();
        // await benchmarks.Channels_WithoutHost_WriterOnly();
        // benchmarks.Cleanup_RunChannelsWithoutHostWriterOnlyTest();

        #endregion

        #region WithoutHost
        
        benchmarks.Setup_RunBroadcastQueueWithoutHostTest();
        benchmarks.BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_WriteAsync();
        benchmarks.Cleanup_RunBroadcastQueueWithoutHostTest();
        
        // benchmarks.Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers();
        // benchmarks.BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers();
        // benchmarks.Cleanup_RunBroadcastQueueWithoutHostTest();

        // benchmarks.Setup_RunBroadcastQueueWithoutHostTest();
        // benchmarks.BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber();
        // benchmarks.Cleanup_RunBroadcastQueueWithoutHostTest();
        // benchmarks.Setup_Channels_WithoutHost_ReadWrite();
        // benchmarks.Channels_WithoutHost_ReadWrite();
        // benchmarks.Cleanup_Channels_WithoutHost_ReadWrite();

        #endregion

        return 0;

        /* **** */

        host = Benchmarks.CreateHostBuilder_BroadcastQueue<BroadcastQueue<ChannelMessage, ChannelResponse>>( subscriberCount: subscriberCount, messageCount: readCount, logLevel: LogLevel.Debug ).Build();
        await host.StartAsync( ct );


        System.Console.WriteLine( "Host is running" );
        var broadcastQueue = host.Services.GetService<BroadcastQueue<ChannelMessage, ChannelResponse>>() ?? throw new Exception();

        logger = host.Services.GetService<ILogger<Program>>() ?? throw new Exception();
        Console.CancelKeyPress += ( object? sender, ConsoleCancelEventArgs args ) => {
            Console.WriteLine( $"Last Read ID: {lastReadId}" );
            cts.Cancel();
        };
        var readersReachedReadCount = new Dictionary<string, int>(); // must equal subscriberCount to be considered "complete"
        logger.LogInformation( "Broadcast Queue={BroadcastQueueWithOneSubscriber}", broadcastQueue );
        stopwatch = Stopwatch.StartNew();
        Console.WriteLine( "STARTING PRGM LOOP" );
        while ( !ct.IsCancellationRequested ) {
            if ( broadcastQueue.Writer.TryReadResponse( out var result ) ) {
                Console.Write( "p" );
                logger.LogDebug( "Read {Id} from ResponseChannel of reader type {ReaderType}", result.ReadId, result.ReaderType ); // URGENT: uncomment this line
                lastReadId = result.ReadId;
                if ( result.ReadId >= readCount ) {
                    readersReachedReadCount[ result.ReaderType ] = result.ReadId;
                    if ( readersReachedReadCount.Count == subscriberCount ) {
                        bool reachedTargetReadCount = true;
                        foreach ( var (readerType, readerLastReadId) in readersReachedReadCount ) {
                            if ( readerLastReadId < readCount ) {
                                reachedTargetReadCount = false;
                                break;
                            }
                        }

                        if ( reachedTargetReadCount ) {
                            break;
                        }
                    }
                }
            }
        }

        logger.LogInformation( "Reading Complete in {time}. Last Read ID: {id}", stopwatch.Elapsed, lastReadId );
        foreach ( var (readerType, readerLastReadId) in readersReachedReadCount ) {
            Console.WriteLine( $"Reader '{readerType}' reached ID: {readerLastReadId}" );
        }

        await host.StopAsync( ct );

        // var channel = Channel.CreateUnbounded<ChannelMessage>();
        // var writer  = new ChannelPublisher( channel.Writer );
        // var reader  = new ChannelSubscriber( channel.Reader );


        // await reader.ReadChannel( cts.Token );
        // await writer.WriteToChannel( cts.Token );
#endif
#if DEBUG_CHANNEL
        var benchmarks = new Benchmarks() { MessageCount = 100_000 };

        /* **** */
        benchmarks.CreateHost_SimpleChannelQueue();
        benchmarks.Channels_InHost();
        benchmarks.StopHost_SimpleChannelQueue();
        
        return 0;
        
        host = Benchmarks.CreateHostBuilder_SimpleChannelQueue( args ).Build();
        await host.StartAsync();

        System.Console.WriteLine( $"Host is running. Thread ID: {Thread.CurrentThread.ManagedThreadId}" );
        var channel = host.Services.GetService<Channel<ChannelMessage>>()  ?? throw new Exception();
        var responseChannel = host.Services.GetService<Channel<ChannelResponse>>() ?? throw new Exception();
        logger = host.Services.GetService<ILogger<Program>>() ?? throw new Exception();
        Console.CancelKeyPress += ( object sender, ConsoleCancelEventArgs args ) => {
            Console.WriteLine( $"Last Read ID: {lastReadId}" );
            cts.Cancel();
        };
        stopwatch = Stopwatch.StartNew();
        var responseReader = responseChannel.Reader;
        while ( true ) {
            if ( await responseReader.WaitToReadAsync( ct ) ) {
                var result = await responseReader.ReadAsync( ct );
                logger.LogDebug( "Read {Id} from ResponseChannel", result.ReadId );
                lastReadId = result.ReadId;
                if ( result.ReadId >= 500 ) {
                    break;
                }
            }
        }

        channel.Writer.Complete();
        logger.LogInformation( "Reading Complete in {time}. Last Read ID: {id}", stopwatch.Elapsed, lastReadId );
        await host.StopAsync();

        // var channel = Channel.CreateUnbounded<ChannelMessage>();
        // var writer  = new ChannelPublisher( channel.Writer );
        // var reader  = new ChannelSubscriber( channel.Reader );


        // await reader.ReadChannel( cts.Token );
        // await writer.WriteToChannel( cts.Token );


#endif
#if DEBUG_OBSERVER
        var dataGenerator = new DataGenerator();

        var observer = new Subscriber();
        observer.Subscribe( dataGenerator );

        for ( int i = 0 ; i < 5 ; i++ ) {
            dataGenerator.AddMessage( i );
        }
#endif


        // var output = new SystemTextJsonSerializationBasic() {
        //     Iterations = 1
        // }.SystemTextJson_Deserialize_Scalars_FloatFields_SourceGen().First();
        // // URGENT -- try this. Is anything being returned?
        // System.Console.WriteLine( $"{output.Id} , {output.Name} , {output.Value}" );
        // System.Console.WriteLine(simpleBenchmarks.SystemTextJson_Deserialize_Scalars_FloatFields_SourceGen()
        //                                    .Select( output => $"{output.Id} , {output.Name} , {output.Value}" )
        //                                    .First() );
        //
        // System.Console.WriteLine(simpleBenchmarks.SystemTextJson_Deserialize_Scalars_Float_SourceGen()
        //                                          .Select( output => $"{output.Id} , {output.Name} , {output.Value}" )
        //                                          .First() );

        // System.Console.WriteLine(simpleBenchmarks.SystemTextJson_Deserialize_Scalars_NodaTime_InstantAsTimestamp()
        //                                          .Select( output => $"{output.Id} , {output.Name} , {output.Value}" )
        //                                          .First() );
        // System.Console.WriteLine(simpleBenchmarks.SystemTextJson_Deserialize_Scalars_NodaTime_SourceGen()
        //                                          .Select( output => $"{output.Id} , {output.Name} , {output.Value}" )
        //                                          .First() );
        // System.Console.WriteLine(simpleBenchmarks.SystemTextJson_Deserialize_Scalars_NodaTimeWithAttribute()
        //                                          .Select( output => $"{output.Id} , {output.Name} , {output.Value}" )
        //                                          .First() );
        // System.Console.WriteLine(simpleBenchmarks.SystemTextJson_Deserialize_Scalars_NodaTimeWithAttribute_SourceGen()
        //                                          .Select( output => $"{output.Id} , {output.Name} , {output.Value}" )
        //                                          .First() );
        //
        // var levelOneBenchmarks = new LevelOneJsonBenchmarks() {
        //     Iterations = 1,
        //     WithSourceGenerationContext = true,
        //     LevelOneJsonFile = "Multiple"
        // };
        // System.Console.WriteLine(levelOneBenchmarks.SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne_SourceGenWithoutOptions()
        //                                            // .Select( output => output is null ? "null" : $"{output.Service} , {output.Timestamp} , {output.Command}" )
        //                                            // .First()
        //     );
        //
        // System.Console.WriteLine(levelOneBenchmarks.SystemTextJson_JsonSerializer_ReadAhead_Deserialize_ResponseContentSlim_SourceGenWithoutOptions()
        //                                            // .Select( output => output is null ? "null" : $"{output.Service} , {output.Timestamp} , {output.Command}" )
        //                                            // .First()
        //                                            );

        // URGENT -- try this. Is anything being returned?
        return 0;
    }
    //     // => new SystemTextJsonSerializationBasic().SystemTextJson_Deserialize_Scalars_NodaTimeWithAttribute();
    //     => new SystemTextJsonSerializationBasic(){Iterations=1}.SystemTextJson_Deserialize_Scalars_NodaTimeWithAttribute_SourceGen();
    //     // => new LevelOneJsonBenchmarks().SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne();
#else
    static void Main( string[] args ) {
        if ( args.Contains( "test" ) ) {
            var benchmarks = new Benchmarks() { MessageCount = 10_000 };

            benchmarks.Setup_BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayWriter();
            benchmarks.BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayWriter();
            benchmarks.Cleanup_RunBroadcastQueueWithoutHost_LockArrayWriter();
            
            benchmarks.Setup_BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayForLoopWriter();
            benchmarks.BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayForLoopWriter();
            benchmarks.Cleanup_RunBroadcastQueueWithoutHost_LockArrayWriter();
            
            // benchmarks.Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayWriter();
            // benchmarks.BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayWriter();
            // benchmarks.Cleanup_RunBroadcastQueueWithoutHost_LockArrayWriter();
            
            // benchmarks.Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayForLoopWriter();
            // benchmarks.BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayForLoopWriter();
            // benchmarks.Cleanup_RunBroadcastQueueWithoutHost_LockArrayWriter();

        } else {
            BenchmarkSwitcher
                .FromAssembly( typeof(Program).Assembly )
                .Run( args.Length > 0
                          ? args
                          : new[] { "-f", "*" },
                      // new BenchmarkConfig()
                      // new DebugInProcessConfig()
                      ManualConfig
                          .Create( DefaultConfig.Instance )
                          .WithOptions( ConfigOptions.StopOnFirstError |
                                        ConfigOptions.JoinSummary )
                );
        }
    }
#endif
}