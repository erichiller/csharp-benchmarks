#define DEBUG_MUX
#define LOG

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Benchmarks.InterThread.Benchmark;

using BroadcastChannel;

using BroadcastChannelMux;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Benchmarks.InterThread.ConsoleTests;

public partial class Program {
    [ Conditional( "LOG" ) ]
    private static void Log( string? msg ) => Console.WriteLine( msg );

    static async Task<int> Main( string[] args ) {
        IHost                   host;
        CancellationTokenSource cts = new CancellationTokenSource();
        ILogger<Program>        logger;
        int                     lastReadId = 0;
        CancellationToken       ct         = cts.Token;
        Stopwatch               stopwatch;

        // System.Console.WriteLine( $"Starting Program. Thread ID: {Thread.CurrentThread.ManagedThreadId}" );

        /* ************************************************************************ */

#if DEBUG_MUX
        Console.WriteLine( "DEBUG_MUX" );

        Console.WriteLine( $"args len={args.Length} ; {String.Join( ", ", args )}" );

        if ( args.Length >= 1 ) {
            switch ( args[ 0 ] ) {
                case nameof(CheckForOffsetCompletionErrors):
                    await CheckForOffsetCompletionErrors();
                    break;
                case nameof(StressTest):
                    await StressTest();
                    break;
                case nameof(SimpleTest): {
                    switch ( args.Length ) {
                        case 3 when Int32.TryParse( args[ 1 ], out int c ) && Int32.TryParse( args[ 2 ], out int messageCount ):
                            Console.WriteLine($"Running {nameof(SimpleTest)} with count={c}");
                            await SimpleTest( c, messageCount );
                            break;
                        case 2 when Int32.TryParse( args[ 1 ], out int c ):
                            Console.WriteLine($"Running {nameof(SimpleTest)} with count={c}");
                            await SimpleTest( c );
                            break;
                        case 1:
                            await SimpleTest();
                            break;
                        default:
                            throw new Exception( args.ToCommaSeparatedString() );
                            break;
                            
                    }
                    break;
                }
                case nameof(ExceptionShouldRemoveFromBroadcastChannel):
                    await ExceptionShouldRemoveFromBroadcastChannel();
                    break;
                case nameof(TypeInheritanceTestingOneSubOfOther):
                    await TypeInheritanceTestingOneSubOfOther();
                    break;
                case nameof(TypeInheritanceTestingBothSubOfSame):
                    await TypeInheritanceTestingBothSubOfSame();
                    break;
                case nameof(ChannelMuxLatencyTest):
                    await ChannelMuxLatencyTest();
                    break;
                case nameof(AsyncWaitLoopOnly_2Producer):
                    await AsyncWaitLoopOnly_2Producer();
                    break;
                case nameof(LoopTryRead2_2Producer): {
                    int? count = null;
                    if ( args.Length >= 1 && Int32.TryParse( args[ 1 ], out int c ) ) {
                        count = c;
                    }
                    await LoopTryRead2_2Producer( count );
                    break;
                }
                case nameof(LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes):
                    await LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes();
                    break;
                case nameof(ChannelMux_LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes):
                    await ChannelMux_LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes();
                    break;
                case nameof(ChannelMux_LoopTryRead2_4Producer_4Tasks_4ReferenceTypes):
                    await ChannelMux_LoopTryRead2_4Producer_4Tasks_4ReferenceTypes();
                    break;
                case nameof(ChannelMux_LoopTryRead2_8Producer_8Tasks):
                    await ChannelMux_LoopTryRead2_8Producer_8Tasks();
                    break;
                case nameof(LatencyTest):
                    await LatencyTest();
                    break;
                case nameof(ChannelComplete_WithException_ShouldThrow_UponAwait):
                    await ChannelComplete_WithException_ShouldThrow_UponAwait();
                    break;
                default:
                    Log( $"not known: {args[ 0 ]}" );
                    return 1;
                    break;
            }
            return 0;
        }
        return 1;

        // {
        //     static void producerTask<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in int totalMessages, Func<int, T> objectFactory ) {
        //         int i = 0;
        //         while ( i++ < totalMessages ) {
        //             Console.WriteLine( $"{typeof(T).Name} Producer, writing item #{i}" );
        //             writer.TryWrite( objectFactory( i ) );
        //             // if ( i % 3 == 0 ) {
        //             //     Thread.Sleep( Random.Shared.Next( 500, 1500 ) ); // milliseconds
        //             // }
        //         }
        //         writer.Complete();
        //     }
        //     int               totalMessages = 100;
        //
        //     BroadcastChannel<StructA?, IBroadcastChannelResponse> channel1       = new ();
        //     BroadcastChannel<ClassA>                              channel2       = new ();
        //     var                                                   channelReader1 = channel1.GetReader();
        //     var                                                   channelReader2 = channel2.GetReader();
        //     Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, totalMessages, i => new StructA {
        //                                                         Id   = i,
        //                                                         Name = @"some_text"
        //                                                     } ), ct );
        //     Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, totalMessages, i => new ClassA {
        //                                                         Id   = i,
        //                                                         Name = @"some_text"
        //                                                     } ), ct );
        //     int receivedCountStructA = 0;
        //     int receivedCountClassA  = 0;
        //     // cts.CancelAfter( 500 );
        //     Task reader1 = Task.Run( async ( ) => {
        //         Console.WriteLine("Reader1: begin");
        //         while ( await channelReader1.WaitToReadAsync( ct ) ) {
        //             Console.WriteLine("Reader1: awake");
        //             if ( channelReader1.TryRead( out StructA? structA ) ) {
        //                 receivedCountStructA++;
        //                 Console.WriteLine($"Reader1: read success: {{nameof(receivedCountStructA)}}: {receivedCountStructA}");
        //             }
        //         }
        //     } );
        //     Task reader2 = Task.Run( async ( ) => {
        //         Console.WriteLine("Reader2: begin");
        //         while ( await channelReader2.WaitToReadAsync( ct ) ) {
        //             Console.WriteLine("Reader2: awake");
        //             if ( channelReader2.TryRead( out ClassA? classA ) ) {
        //                 Console.WriteLine("Reader2: read success");
        //                 receivedCountClassA++;
        //                 Console.WriteLine($"Reader1: read success: {{nameof(receivedCountClassA)}}: {receivedCountClassA}");
        //             }
        //                 
        //         }
        //     } );
        //     await producer1;
        //     await producer2;
        //     await reader1;
        //     await reader2;
        //     if ( receivedCountClassA != totalMessages || receivedCountStructA != totalMessages ) {
        //         throw new System.Exception( $"Not all messages were read. {nameof(receivedCountClassA)}: {receivedCountClassA} ; {nameof(receivedCountStructA)}: {receivedCountStructA}" );
        //     }
        //
        //
        //     return 0;
        // }

        /*
            while ( await mux.WaitToReadAsync( ct ) ) {
                Log( "Awake" );
                if ( mux.TryRead( out classA ) ) {
                    receivedCount++;
                    Log( classA.ToString() );
                    receivedCountClassA++;
                }
                if ( mux.TryRead( out structA ) ) {
                    receivedCount++;
                    Log( structA.ToString() );
                    receivedCountStructA++;
                }
                
                // while ( mux.TryRead( out classA ) || mux.TryRead( out structA ) ) {
                //     if ( classA is { } ) {
                //         receivedCount++;
                //         Log( classA.ToString() );
                //         receivedCountClassA++;
                //         classA = null;
                //     }
                //     if ( structA is { } ) {
                //         receivedCount++;
                //         Log( structA.ToString() );
                //         receivedCountStructA++;
                //         structA = null;
                //     }
                // }
                
                // if ( receivedCount > ( totalMessages * 2 ) + 5 ) {
                //     Console.WriteLine( $"receivedCount exceeded {( totalMessages * 2 ) + 5}" );
                //     break;
                // }
                // if ( loopCount > maxLoopCount ) {
                //     Console.WriteLine( $"Loop count exceeded {maxLoopCount}" );
                //     break;
                // }
                loopCount++;
                Log( $"Waiting to read... loopCount: {loopCount}" );
            }
         */
        {
            stopwatch = Stopwatch.StartNew();
            BroadcastChannel<StructA?, IBroadcastChannelResponse> channel1 = new ();
            BroadcastChannel<ClassA>                              channel2 = new ();
            ChannelMux<StructA?, ClassA>                          mux      = new (channel1.Writer, channel2.Writer);
            // int                                                  totalMessages = 10;
            int totalMessages = 10_000_000;

            static void ProducerTask<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in int totalMessages, Func<int, T> objectFactory ) {
                int i = 0;
                while ( i++ < totalMessages ) {
                    Log( $"{typeof(T).Name} Producer, writing item #{i}" );
                    writer.TryWrite( objectFactory( i ) );
                    // if ( i % 3 == 0 ) {
                    //     Thread.Sleep( Random.Shared.Next( 500, 1500 ) ); // milliseconds
                    // }
                }
                writer.Complete();
            }

            Task producer1 = Task.Run( ( ) => ProducerTask( channel1.Writer, totalMessages, i => new StructA {
                                                                Id   = i,
                                                                Name = @"some_text"
                                                            } ), ct );
            Task producer2 = Task.Run( ( ) => ProducerTask( channel2.Writer, totalMessages, i => new ClassA {
                                                                Id   = i,
                                                                Name = @"some_text"
                                                            } ), ct );
            int receivedCount        = 0;
            int receivedCountStructA = 0;
            int receivedCountClassA  = 0;
            int loopCount            = 0;
            Log( "Waiting to read..." );
            while ( await mux.WaitToReadAsync( ct ) ) {
                Log( "Awake" );
                if ( mux.TryRead( out ClassA? classA ) ) {
                    receivedCount++;
                    Log( classA.ToString() );
                    receivedCountClassA++;
                }
                if ( mux.TryRead( out StructA? structA ) ) {
                    receivedCount++;
                    Log( structA.ToString() );
                    receivedCountStructA++;
                }
                loopCount++;
                Log( $"Waiting to read... loopCount: {loopCount}" );
            }
            stopwatch.Stop();
            Console.WriteLine( $"WaitToReadAsync Loop only\n\t"                                                               +
                               $"Completed in {stopwatch.ElapsedMilliseconds:N0} ms ({stopwatch.ElapsedTicks:N0} ticks).\n\t" +
                               $"receivedCount:        {receivedCount:N0}\n\t"                                                +
                               $"receivedCountStructA: {receivedCountStructA:N0}\n\t"                                         +
                               $"receivedCountClassA:  {receivedCountClassA:N0}\n\t"                                          +
                               $"loopCount:            {loopCount:N0}" );
            // await producer1;
        }
        // System.IO.File.Delete( "/home/eric/Downloads/marks.csv" );
        // for ( int iter = 0 ; iter < 2_000 ; iter++ ) {
        for ( int iter = 0 ; iter < 100_000 ; iter++ ) {
            stopwatch = Stopwatch.StartNew();
            BroadcastChannel<StructA?, IBroadcastChannelResponse> channel1 = new ();
            BroadcastChannel<ClassA>                              channel2 = new ();
            ChannelMux<StructA?, ClassA>                          mux      = new (channel1.Writer, channel2.Writer);
            // int                                                  totalMessages = 10;
            int totalMessages = 100_000;

            static void ProducerTask<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in int totalMessages, Func<int, T> objectFactory ) {
                int i = 0;
                while ( i++ < totalMessages ) {
                    Log( $"{typeof(T).Name} Producer, writing item #{i}" );
                    writer.TryWrite( objectFactory( i ) );
                    // if ( i % 3 == 0 ) {
                    //     Thread.Sleep( Random.Shared.Next( 500, 1500 ) ); // milliseconds
                    // }
                }
                writer.Complete();
            }

            Task producer1 = Task.Run( ( ) => ProducerTask( channel1.Writer, totalMessages, i => new StructA {
                                                                Id   = i,
                                                                Name = @"some_text"
                                                            } ), ct );
            Task producer2 = Task.Run( ( ) => ProducerTask( channel2.Writer, totalMessages, i => new ClassA {
                                                                Id   = i,
                                                                Name = @"some_text"
                                                            } ), ct );
            int      receivedCount        = 0;
            int      receivedCountStructA = 0;
            int      receivedCountClassA  = 0;
            int      loopCount            = 0;
            ClassA?  classA               = null;
            StructA? structA              = null;
            Log( "Waiting to read..." );
            while ( await mux.WaitToReadAsync( ct ) ) {
                Log( "Awake" );
                while ( mux.TryRead( out classA ) || mux.TryRead( out structA ) ) {
                    if ( classA is { } ) {
                        receivedCount++;
                        Log( classA.ToString() );
                        receivedCountClassA++;
                        classA = null;
                    }
                    if ( structA is { } ) {
                        receivedCount++;
                        Log( structA.ToString() );
                        receivedCountStructA++;
                        structA = null;
                    }
                }
                loopCount++;
                Log( $"Waiting to read... loopCount: {loopCount}" );
            }
            stopwatch.Stop();

            await producer1;
            await producer2;
            if ( !producer1.IsCompletedSuccessfully || !producer2.IsCompletedSuccessfully ) {
                throw new Exception( "producer failed" );
            }
            if ( receivedCountClassA != totalMessages || receivedCountStructA != totalMessages ) {
                throw new System.Exception( $"Not all messages were read. {nameof(receivedCountClassA)}: {receivedCountClassA} ; {nameof(receivedCountStructA)}: {receivedCountStructA}" );
            }
            // Console.WriteLine( /* Testing */ // KILL
            //     /* Testing */                // KILL
            //     $"\n\t"                                                         +
            //     $"_WaitToReadAsync_mark1: {mux._WaitToReadAsync_mark1}\n\t"     +
            //     $"_WaitToReadAsync__mark2: {mux._WaitToReadAsync__mark2}\n\t"   +
            //     $"_WaitToReadAsync__mark3: {mux._WaitToReadAsync__mark3}\n\t"   +
            //     $"_WaitToReadAsync__mark4: {mux._WaitToReadAsync__mark4}\n\t"   +
            //     $"_WaitToReadAsync__mark5: {mux._WaitToReadAsync__mark5}\n\t"   +
            //     $"_WaitToReadAsync__mark6: {mux._WaitToReadAsync__mark6}\n\t"   +
            //     $"_WaitToReadAsync__mark7: {mux._WaitToReadAsync__mark7}\n\t"   +
            //     $"_WaitToReadAsync__mark8: {mux._WaitToReadAsync__mark8}\n\t"   +
            //     $"_WaitToReadAsync__mark9: {mux._WaitToReadAsync__mark9}\n\t"   +
            //     $"_WaitToReadAsync__mark10: {mux._WaitToReadAsync__mark10}\n\t" +
            //     $"_WaitToReadAsync__mark11: {mux._WaitToReadAsync__mark11}\n\t" +
            //     $"_WaitToReadAsync__mark12: {mux._WaitToReadAsync__mark12}\n\t" +
            //     $"_tryWrite_mark1: {mux._tryWrite_mark1}\n\t"                   +
            //     $"_tryWrite_mark2: {mux._tryWrite_mark2}\n\t"                   +
            //     $"_tryWrite_mark3: {mux._tryWrite_mark3}\n\t"                   +
            //     $"_tryWrite_mark4: {mux._tryWrite_mark4}\n\t"                   +
            //     $"_tryWrite_mark5: {mux._tryWrite_mark5}\n\t"                   +
            //     $"_tryWrite_mark6: {mux._tryWrite_mark6}\n\t"                   +
            //     $"_tryWrite_mark7: {mux._tryWrite_mark7}\n\t"                   +
            //     $"_tryWrite_mark8: {mux._tryWrite_mark8}\n\t" );
            // System.IO.File.AppendAllText( "/home/eric/Downloads/marks.csv",
            //                               $"{stopwatch.ElapsedTicks},"                                                                                                                                                                                                                                                                                                                                                  +
            //                               $"{mux._WaitToReadAsync_mark1},{mux._WaitToReadAsync__mark2},{mux._WaitToReadAsync__mark3},{mux._WaitToReadAsync__mark4},{mux._WaitToReadAsync__mark5},{mux._WaitToReadAsync__mark6},{mux._WaitToReadAsync__mark7},{mux._WaitToReadAsync__mark8},{mux._WaitToReadAsync__mark9},{mux._WaitToReadAsync__mark10},{mux._WaitToReadAsync__mark11},{mux._WaitToReadAsync__mark12}," +
            //                               $"{mux._tryWrite_mark1},{mux._tryWrite_mark2},{mux._tryWrite_mark3},{mux._tryWrite_mark4},{mux._tryWrite_mark5},{mux._tryWrite_mark6},{mux._tryWrite_mark7},{mux._tryWrite_mark8}\n" );
            Console.WriteLine( $"{iter}. With TryRead Loop\n\t"                                                               +
                               $"Completed in {stopwatch.ElapsedMilliseconds:N0} ms ({stopwatch.ElapsedTicks:N0} ticks).\n\t" +
                               $"receivedCount:        {receivedCount:N0}\n\t"                                                +
                               $"receivedCountStructA: {receivedCountStructA:N0}\n\t"                                         +
                               $"receivedCountClassA:  {receivedCountClassA:N0}\n\t"                                          +
                               $"loopCount:            {loopCount:N0}" );
        }
#endif

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
{
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
                logger.LogDebug( "Read {Id} from ResponseChannel of reader type {ReaderType}", result.ReadId, result.ReaderType );
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
        }
#endif
#if DEBUG_CHANNEL
{
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

}
#endif
#if DEBUG_OBSERVER
{
        var dataGenerator = new DataGenerator();

        var observer = new Subscriber();
        observer.Subscribe( dataGenerator );

        for ( int i = 0 ; i < 5 ; i++ ) {
            dataGenerator.AddMessage( i );
        }
        }
#endif
        return 0;
    }
}