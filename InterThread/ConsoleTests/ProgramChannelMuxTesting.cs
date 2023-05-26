#if DEBUG
#define DEBUG_MUX
#endif

#if DEBUG_MUX
#define DEBUG
// #define LOG
#endif

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Benchmarks.InterThread.Benchmark;

using BroadcastChannel;

using BroadcastChannelMux;

using Microsoft.Extensions.Logging;

using Xunit.Abstractions;

namespace Benchmarks.InterThread.ConsoleTests;

public partial class Program {
    private const string _marksOutputCsvPath = @"/home/eric/Downloads/marks.csv";

    private static async Task CheckForOffsetCompletionErrors( ) {
        Stopwatch stopwatch    = Stopwatch.StartNew();
        int       MessageCount = 10_000;
        System.IO.File.Delete( _marksOutputCsvPath );
        for ( int i = 0 ; i < 10_000 ; i++ ) {
            long                                                  testStartTicks = stopwatch.ElapsedTicks;
            long                                                  testStartMs    = stopwatch.ElapsedMilliseconds;
            BroadcastChannel<StructA?, IBroadcastChannelResponse> channel1       = new ();
            BroadcastChannel<ClassA>                              channel2       = new ();
            ChannelMux<StructA?, ClassA>                          mux            = new (channel1.Writer, channel2.Writer);
            CancellationToken                                     ct             = CancellationToken.None;
            // using CancellationTokenSource cts = new CancellationTokenSource();
            // CancellationToken             ct  = cts.Token;

            Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, MessageCount, i => new StructA {
                                                                Id   = i,
                                                                Name = @"some_text"
                                                            } ), ct );
            Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, MessageCount, i => new ClassA {
                                                                Id   = i,
                                                                Name = @"some_text"
                                                            } ), ct );
            int receivedCountStructA = 0;
            int receivedCountClassA  = 0;
            while ( await mux.WaitToReadAsync( ct ) ) {
                while ( ( mux.TryRead( out ClassA? classA ), mux.TryRead( out StructA? structA ) ) != ( false, false ) ) {
                    if ( classA is { } ) {
                        receivedCountClassA++;
                    }
                    if ( structA is { } ) {
                        receivedCountStructA++;
                    }
                }
            }
            await producer1;
            await producer2;
            if ( receivedCountClassA != MessageCount || receivedCountStructA != MessageCount ) {
                throw new System.Exception( $"[{i}] Not all messages were read. {nameof(receivedCountClassA)}: {receivedCountClassA} ; {nameof(receivedCountStructA)}: {receivedCountStructA}" );
            }
            if ( i % 10 == 0 ) {
                Console.WriteLine( $"[{i}] @ {stopwatch.ElapsedMilliseconds:N0} ms took {stopwatch.ElapsedTicks - testStartTicks:N0} ticks ({stopwatch.ElapsedMilliseconds - testStartMs} ms)\n\t" +
                                   $"{nameof(MessageCount)}: {MessageCount}\n\t"                                                                                                                   +
                                   $"{nameof(receivedCountStructA)}: {receivedCountStructA}\n\t"                                                                                                   +
                                   $"{nameof(receivedCountStructA)}: {receivedCountStructA}\n\t"
                );
                displayMuxDiagnostics( mux, stopwatch.ElapsedTicks - testStartTicks, writeToFile: true );
                // Thread.Sleep( 5_000 );
            }
            // break;
        }

        static void producerTask<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in int totalMessages, System.Func<int, T> objectFactory ) {
            int i = 0;
            // Thread.Sleep( Random.Shared.Next( 10, 70 ) ); // startup
            while ( i++ < totalMessages ) {
                writer.TryWrite( objectFactory( i ) );
                if ( i % msgsPerMsgGroup == 0 ) {
                    Thread.Sleep( Random.Shared.Next( 1, 7 ) );
                }
            }
            writer.Complete();
        }
    }
    
    
    /// <summary>
    /// Represents a class which can be used to provide test output.
    /// </summary>
    [SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
    public class LoggerTestOutputHelper : ITestOutputHelper {
        private readonly ILogger _logger;

        public LoggerTestOutputHelper( ILogger logger ) {
            _logger = logger;
        }

        /// <summary>Adds a line of text to the output.</summary>
        /// <param name="message">The message</param>
        public void WriteLine( string message ) => _logger.LogInformation( message );

        /// <summary>Formats a line of text and adds it to the output.</summary>
        /// <param name="format">The message format</param>
        /// <param name="args">The format arguments</param>
        public void WriteLine(string format, params object[] args) => _logger.LogInformation( String.Format( format, args ) );
    }
    /// <summary>
    /// Represents a class which can be used to provide test output.
    /// </summary>
    public class ConsoleTestOutputHelper : ITestOutputHelper {

        /// <summary>Adds a line of text to the output.</summary>
        /// <param name="message">The message</param>
        public void WriteLine( string message ) => Console.WriteLine( message );

        /// <summary>Formats a line of text and adds it to the output.</summary>
        /// <param name="format">The message format</param>
        /// <param name="args">The format arguments</param>
        public void WriteLine(string format, params object[] args) =>  Console.WriteLine( String.Format( format, args ) );
    }

    private static async Task ChannelComplete_WithException_ShouldThrow_UponAwait( ) {
        var tests = new ChannelMuxTests.ChannelMuxTests( new ConsoleTestOutputHelper(  ) );
        await tests.ChannelComplete_WithException_ShouldThrow_UponAwait(true);
    }
    
    private static void displayMuxDiagnostics( ChannelMux mux, long perfTestTicks, bool writeToFile ) {
        Console.WriteLine( /* Testing */ // KILL
            /* Testing */                // KILL
            $"\n\t"                                                                                                     +
            $"{nameof(mux._WaitToReadAsync__entry)}: {mux._WaitToReadAsync__entry}\n\t"                                 +
            $"{nameof(mux._WaitToReadAsync__cancellationToken)}: {mux._WaitToReadAsync__cancellationToken}\n\t"         +
            $"{nameof(mux._WaitToReadAsync__readableItems)}: {mux._WaitToReadAsync__readableItems}\n\t"                 +
            $"{nameof(mux._WaitToReadAsync__inLock)}: {mux._WaitToReadAsync__inLock}\n\t"                               +
            $"{nameof(mux._WaitToReadAsync__inLock_readableItems)}: {mux._WaitToReadAsync__inLock_readableItems}\n\t"   +
            $"{nameof(mux._WaitToReadAsync__allChannelsComplete)}: {mux._WaitToReadAsync__allChannelsComplete}\n\t"     +
            $"{nameof(mux._WaitToReadAsync__TryOwnAndReset)}: {mux._WaitToReadAsync__TryOwnAndReset}\n\t"               +
            $"{nameof(mux._WaitToReadAsync__new_eq_old)}: {mux._WaitToReadAsync__new_eq_old}\n\t"                       +
            $"{nameof(mux._WaitToReadAsync__TryOwnAndReset_failed)}: {mux._WaitToReadAsync__TryOwnAndReset_failed}\n\t" +
            $"{nameof(mux._WaitToReadAsync__completeException)}: {mux._WaitToReadAsync__completeException}\n\t"         +
            $"{nameof(mux._WaitToReadAsync__inLock_end)}: {mux._WaitToReadAsync__inLock_end}\n\t"                       +
            $"{nameof(mux._WaitToReadAsync__readableItems_end)}: {mux._WaitToReadAsync__readableItems_end}\n\t"         +
            $"{nameof(mux._WaitToReadAsync__end)}: {mux._WaitToReadAsync__end}\n\t"                                     +
            $"{nameof(mux._tryWrite_enter)}: {mux._tryWrite_enter}\n\t"                                                 +
            $"{nameof(mux._tryWrite_isComplete_or_exception)}: {mux._tryWrite_isComplete_or_exception}\n\t"             +
            $"{nameof(mux._tryWrite_no_reader_waiting)}: {mux._tryWrite_no_reader_waiting}\n\t"                         +
            $"{nameof(mux._tryWrite_in_monitor)}: {mux._tryWrite_in_monitor}\n\t"                                       +
            $"{nameof(mux._tryWrite_monitor_no_waiting_reader)}: {mux._tryWrite_monitor_no_waiting_reader}\n\t"         +
            $"{nameof(mux._tryWrite_monitor_set_bools)}: {mux._tryWrite_monitor_set_bools}\n\t"                         +
            $"{nameof(mux._tryWrite_waiting_reader_is_not_null)}: {mux._tryWrite_waiting_reader_is_not_null}\n\t"       +
            $"{nameof(mux._tryWrite_final)}: {mux._tryWrite_final}\n\t" );
        if ( writeToFile ) {
            System.IO.File.AppendAllText( _marksOutputCsvPath,
                                          $"{perfTestTicks},"                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  +
                                          $"{mux._WaitToReadAsync__entry},{mux._WaitToReadAsync__cancellationToken},{mux._WaitToReadAsync__readableItems},{mux._WaitToReadAsync__inLock},{mux._WaitToReadAsync__inLock_readableItems},{mux._WaitToReadAsync__allChannelsComplete},{mux._WaitToReadAsync__TryOwnAndReset},{mux._WaitToReadAsync__new_eq_old},{mux._WaitToReadAsync__TryOwnAndReset_failed},{mux._WaitToReadAsync__completeException},{mux._WaitToReadAsync__inLock_end},{mux._WaitToReadAsync__readableItems_end},{mux._WaitToReadAsync__end}," +
                                          $"{mux._tryWrite_enter},{mux._tryWrite_isComplete_or_exception},{mux._tryWrite_no_reader_waiting},{mux._tryWrite_in_monitor},{mux._tryWrite_monitor_no_waiting_reader},{mux._tryWrite_monitor_set_bools},{mux._tryWrite_waiting_reader_is_not_null},{mux._tryWrite_final}\n" );
        }
    }

    private static async Task AsyncWaitLoopOnly_2Producer( ) {
        ChannelMuxBenchmarks benchmark = new () {
            MessageCount          = 10_000,
            WithCancellationToken = true
        };
        Stopwatch stopwatch = Stopwatch.StartNew();
        for ( int i = 0 ; i < 10_000 ; i++ ) {
            long testStartTicks = stopwatch.ElapsedTicks;
            long testStartMs    = stopwatch.ElapsedMilliseconds;
            await benchmark.AsyncWaitLoopOnly_2Producer();
            if ( i % 100 == 0 ) {
                Console.WriteLine( $"[{i}] @ {stopwatch.ElapsedMilliseconds:N0} ms took {stopwatch.ElapsedTicks - testStartTicks:N0} ticks ({stopwatch.ElapsedMilliseconds - testStartMs} ms)" );
            }
        }
    }

    private static async Task LoopTryRead2_2Producer( ) {
        while ( !System.Diagnostics.Debugger.IsAttached )
            Thread.Sleep( TimeSpan.FromMilliseconds( 100 ) );
        ChannelMuxBenchmarks benchmark = new ();
        Stopwatch            stopwatch = Stopwatch.StartNew();
        benchmark.MessageCount = 100_000;
        for ( int i = 0 ; i < 10_000 ; i++ ) {
            long testStartTicks = stopwatch.ElapsedTicks;
            long testStartMs    = stopwatch.ElapsedMilliseconds;
            await benchmark.LoopTryRead2_2Producer();
            if ( i % 10 == 0 ) {
                Console.WriteLine( $"[{i}] @ {stopwatch.ElapsedMilliseconds:N0} ms took {stopwatch.ElapsedTicks - testStartTicks:N0} ticks ({stopwatch.ElapsedMilliseconds - testStartMs} ms)" );
            }
        }
    }

    private static async Task LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes( ) {
        ChannelMuxBenchmarks benchmark = new ();
        benchmark.MessageCount = 10;
        await benchmark.LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes();
    }


    public static async Task ChannelMux_LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes( ) {
        ChannelMuxBenchmarks benchmark = new ();
        benchmark.MessageCount = 10;
        await benchmark.LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes();
    }

    public static async Task ChannelMux_LoopTryRead2_4Producer_4Tasks_4ReferenceTypes( ) {
        ChannelMuxBenchmarks benchmark = new ();
        benchmark.MessageCount = 10;
        await benchmark.LoopTryRead2_4Producer_4Tasks_4ReferenceTypes();
    }

    public static async Task ChannelMux_LoopTryRead2_8Producer_8Tasks( ) {
        ChannelMuxBenchmarks benchmark = new ();
        benchmark.MessageCount = 10;
        await benchmark.LoopTryRead2_8Producer_8Tasks();
    }

    private readonly record struct TimeInfo( int Seq, long Written, long Delta );

    private static int msBetweenMsgGroups = 85;
    private static int msgsPerSecond      = 500 / 10      * 20;
    private static int msgsPerMsgGroup    = msgsPerSecond / ( 1000 / msBetweenMsgGroups ); // 500 per 10 seconds

    static void producerTask<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in Stopwatch stopwatch, in int totalMessages, System.Func<int, T> objectFactory ) {
        int i = 0;
        Console.WriteLine( $"Producer Task Starting at {stopwatch.ElapsedTicks:N0} on {Thread.CurrentThread.ManagedThreadId}" );
        Thread.Sleep( 50 ); // startup
        while ( i++ < totalMessages ) {
            writer.TryWrite( objectFactory( i ) );
            if ( i % msgsPerMsgGroup == 0 ) {
                Thread.Sleep( msBetweenMsgGroups );
            }
        }
        Console.WriteLine( $"Producer Task Completing at {stopwatch.ElapsedTicks:N0} on {Thread.CurrentThread.ManagedThreadId}" );
        writer.Complete();
    }

    private static async Task LatencyTest( ) {
        BroadcastChannel<StructB?>     channel1 = new ();
        BroadcastChannel<StructC?>     channel2 = new ();
        ChannelMux<StructB?, StructC?> mux      = new (channel1.Writer, channel2.Writer);
        CancellationToken              ct       = CancellationToken.None;
        // int                            MessageCount = 10_000_000;
        int       MessageCount = 1_000_000;
        Stopwatch stopwatch    = Stopwatch.StartNew();
        Task      producer1    = Task.Run( ( ) => producerTask( channel1.Writer, stopwatch, MessageCount, i => new StructB( stopwatch.ElapsedTicks ) ), ct );
        Task      producer2    = Task.Run( ( ) => producerTask( channel2.Writer, stopwatch, MessageCount, i => new StructC( stopwatch.ElapsedTicks ) ), ct );
        int       received2    = 0;
        int       received1    = 0;

        Console.WriteLine( $"{nameof(msBetweenMsgGroups)}: {msBetweenMsgGroups:N0}\n" +
                           $"{nameof(msgsPerSecond)}: {msgsPerSecond:N0}\n"           +
                           $"{nameof(msgsPerMsgGroup)}: {msgsPerMsgGroup:N0}\n"       +
                           $"" );
        Log( $"StopWatch Frequency is: {Stopwatch.Frequency:N0}" );
        // using (StreamWriter outputFile = new StreamWriter(file.FullName)){
        Console.WriteLine( $"Beginning at Stopwatch ticks: {stopwatch.ElapsedTicks:N0} on {Thread.CurrentThread.ManagedThreadId}" );
        TimeInfo[] timeInfo1 = new TimeInfo[ MessageCount ];
        TimeInfo[] timeInfo2 = new TimeInfo[ MessageCount ];
        while ( await mux.WaitToReadAsync( ct ) ) {
            Console.WriteLine( $"[{( ( received1 + received2 ) / 2 ):N0}] WaitToReadAsync continued, Stopwatch ticks: {stopwatch.ElapsedTicks:N0}" );
            while ( ( mux.TryRead( out StructB? structB ), mux.TryRead( out StructC? structC ) ) != ( false, false ) ) {
                long elapsed = stopwatch.ElapsedTicks;
                if ( structB is { TimeSent: var ts1 } ) {
                    timeInfo1[ received1 ] = new (Seq: received1, Written: ts1, Delta: ( elapsed - ts1 ));
                    // long tickDelta = ( elapsed - ts1 );
                    // Log( $"{nameof(structB)} ticks since message: {tickDelta:N0} ticks ({( tickDelta / Stopwatch.Frequency / 1000 ):N3}) ms" );
                    received1++;
                }
                if ( structC is { TimeSent: var ts2 } ) {
                    timeInfo2[ received2 ] = new (Seq: received2, Written: ts2, Delta: ( elapsed - ts2 ));
                    // long tickDelta = ( elapsed - ts2 );
                    // Log( $"{nameof(structC)} ticks since message: {tickDelta:N0} ticks ({( tickDelta / Stopwatch.Frequency / 1000 ):N3}) ms" );
                    received2++;
                }
            }
        }
        Console.WriteLine( $"Finished Reading at Stopwatch ticks: {stopwatch.ElapsedTicks:N0} on {Thread.CurrentThread.ManagedThreadId}" );
        // StringBuilder sb = new ();
        Console.WriteLine( $"{timeInfo1[ 0 ].Written:N0} to {timeInfo2[ ^1 ].Written:N0}" );
        Console.WriteLine( $"{nameof(msBetweenMsgGroups)}: {msBetweenMsgGroups:N0}\n" +
                           $"{nameof(msgsPerSecond)}: {msgsPerSecond:N0}\n"           +
                           $"{nameof(msgsPerMsgGroup)}: {msgsPerMsgGroup:N0}\n"       +
                           $"" );
        var file = new System.IO.FileInfo( System.IO.Path.Combine( System.Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ), "Downloads", "latency_test_results.csv" ) );

        await using ( StreamWriter outputFile = new StreamWriter( file.FullName ) ) {
            for ( int i = 0 ; i < MessageCount ; i++ ) {
                // if ( i % 100 == 0 ) {
                outputFile.WriteLine( $"{timeInfo1[ i ].Seq},{timeInfo1[ i ].Written},{timeInfo1[ i ].Delta},{timeInfo2[ i ].Written},{timeInfo2[ i ].Delta}" );
                // }
            }
        }
        // System.IO.File.WriteAllText( file.FullName, sb.ToString() );
        await producer1;
        await producer2;
        if ( received1 != MessageCount || received2 != MessageCount ) {
            throw new System.Exception( $"Not all messages were read. {nameof(received1)}: {received1} ; {nameof(received2)}: {received2}" );
        }
    }
}

file readonly record struct StructB( long TimeSent );

file readonly record struct StructC( long TimeSent );