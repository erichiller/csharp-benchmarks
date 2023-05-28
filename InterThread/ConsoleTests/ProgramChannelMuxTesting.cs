#if DEBUG
#define DEBUG_MUX
#endif

#if DEBUG_MUX
#define DEBUG
// #define LOG
#endif

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Benchmarks.InterThread.Benchmark;

using BroadcastChannel;

using BroadcastChannelMux;

using Xunit.Abstractions;

namespace Benchmarks.InterThread.ConsoleTests;

public partial class Program {
    private const string _marksOutputCsvPath = @"/home/eric/Downloads/marks.csv";

    internal static async Task SimpleTest( ) {
        for ( int i = 0 ; i < 100 ; i++ ) {
            Console.WriteLine( $"In {nameof(SimpleTest)}" );
            Stopwatch stopwatch    = Stopwatch.StartNew();
            const int messageCount = 10_000;
            System.IO.File.Delete( _marksOutputCsvPath );
            long                                                  testStartTicks = stopwatch.ElapsedTicks;
            long                                                  testStartMs    = stopwatch.ElapsedMilliseconds;
            BroadcastChannel<StructA?, IBroadcastChannelResponse> channel1       = new ();
            BroadcastChannel<ClassA>                              channel2       = new ();
            ChannelMux<StructA?, ClassA>                          mux            = new (channel1.Writer, channel2.Writer);
           using CancellationTokenSource                               cts            = new CancellationTokenSource();
            cts.CancelAfter( 20_000 );
            CancellationToken                                     ct             = cts.Token;
            // using CancellationTokenSource cts = new CancellationTokenSource();
            // CancellationToken             ct  = cts.Token;

            Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, messageCount, static i => new StructA {
                                                                Id   = i,
                                                                Name = @"some_text"
                                                            } ), ct );
            Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, messageCount, static i => new ClassA {
                                                                Id   = i,
                                                                Name = @"some_text"
                                                            } ), ct );
            int receivedCountStructA = 0;
            int receivedCountClassA  = 0;
            try {
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
            } catch ( OperationCanceledException e ) {
                Console.WriteLine( $"Operation Cancelled {e}" );
                i = 100;
            } catch ( System.InvalidOperationException e ) {
                Console.WriteLine( $"InvalidOperationException: {e}" );
                i = 100;
            }
            if ( receivedCountClassA != messageCount || receivedCountStructA != messageCount ) {
                throw new System.Exception( $"Not all messages were read. {nameof(receivedCountClassA)}: {receivedCountClassA} ; {nameof(receivedCountStructA)}: {receivedCountStructA}" );
            }
            Console.WriteLine( $"{stopwatch.ElapsedMilliseconds:N0} ms took {stopwatch.ElapsedTicks - testStartTicks:N0} ticks ({stopwatch.ElapsedMilliseconds - testStartMs} ms)\n\t" +
                               $"{nameof(messageCount)}: {messageCount}\n\t"                                                                                                           +
                               $"{nameof(receivedCountStructA)}: {receivedCountStructA}\n\t"                                                                                           +
                               $"{nameof(receivedCountStructA)}: {receivedCountStructA}\n\t"
            );
            displayMuxDiagnostics( mux, stopwatch.ElapsedTicks - testStartTicks, writeToFile: false );
        }
        static void producerTask<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in int totalMessages, System.Func<int, T> objectFactory ) {
            Console.WriteLine(nameof(producerTask));
            for ( int i = 0; i < totalMessages; i++ ) {
                writer.TryWrite( objectFactory( i ) );
            }
            Console.WriteLine($"producerTask for type {typeof(T).Name} is {nameof(writer.Complete)}");
            writer.Complete();
        }
    }

    internal static async Task CheckForOffsetCompletionErrors( ) {
        Stopwatch stopwatch    = Stopwatch.StartNew();
        const int messageCount = 10_000;
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

            Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, messageCount, static i => new StructA {
                                                                Id   = i,
                                                                Name = @"some_text"
                                                            } ), ct );
            Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, messageCount, static i => new ClassA {
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
            if ( receivedCountClassA != messageCount || receivedCountStructA != messageCount ) {
                throw new System.Exception( $"[{i}] Not all messages were read. {nameof(receivedCountClassA)}: {receivedCountClassA} ; {nameof(receivedCountStructA)}: {receivedCountStructA}" );
            }
            if ( i % 10 == 0 ) {
                Console.WriteLine( $"[{i}] @ {stopwatch.ElapsedMilliseconds:N0} ms took {stopwatch.ElapsedTicks - testStartTicks:N0} ticks ({stopwatch.ElapsedMilliseconds - testStartMs} ms)\n\t" +
                                   $"{nameof(messageCount)}: {messageCount}\n\t"                                                                                                                   +
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
                if ( i % _msgsPerMsgGroup == 0 ) {
                    Thread.Sleep( Random.Shared.Next( 1, 7 ) );
                }
            }
            writer.Complete();
        }
    }

    private static async Task ChannelComplete_WithException_ShouldThrow_UponAwait( ) {
        var tests = new ChannelMuxTests.ChannelMuxTests( new ConsoleTestOutputHelper() );
        await tests.ChannelComplete_WithException_ShouldThrow_UponAwait( true );
    }

    private static async Task AsyncWaitLoopOnly_2Producer( ) {
        ChannelMuxBenchmarks benchmark = new () {
            MessageCount          = 10_000,
            WithCancellationToken = true
        };
        // const int testCount = 10_000;
        const int testCount = 1;
        Stopwatch stopwatch = Stopwatch.StartNew();
        for ( int i = 0 ; i < testCount ; i++ ) {
            long testStartTicks = stopwatch.ElapsedTicks;
            long testStartMs    = stopwatch.ElapsedMilliseconds;
            await benchmark.AsyncWaitLoopOnly_2Producer();
            if ( i % 100 == 0 ) {
                Console.WriteLine( $"[{i}] @ {stopwatch.ElapsedMilliseconds:N0} ms took {stopwatch.ElapsedTicks - testStartTicks:N0} ticks ({stopwatch.ElapsedMilliseconds - testStartMs} ms)" );
            }
        }
    }

    private static async Task LoopTryRead2_2Producer( int? count = 10_000 ) {
        // while ( !System.Diagnostics.Debugger.IsAttached )
            // Thread.Sleep( TimeSpan.FromMilliseconds( 100 ) );
        ChannelMuxBenchmarks benchmark = new ();
        Stopwatch            stopwatch = Stopwatch.StartNew();
        benchmark.MessageCount = 100_000;
        Console.WriteLine($"will run {count} times");
        for ( int i = 0 ; i < count ; i++ ) {
            long testStartTicks = stopwatch.ElapsedTicks;
            long testStartMs    = stopwatch.ElapsedMilliseconds;
            await benchmark.LoopTryRead2_2Producer();
            if ( i % 10 == 0 ) {
                Console.WriteLine( $"[{i}] @ {stopwatch.ElapsedMilliseconds:N0} ms took {stopwatch.ElapsedTicks - testStartTicks:N0} ticks ({stopwatch.ElapsedMilliseconds - testStartMs} ms)" );
            }
        }
    }

    private static async Task LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes( ) {
        ChannelMuxBenchmarks benchmark = new () { MessageCount = 10 };
        await benchmark.LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes();
    }


    public static async Task ChannelMux_LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes( ) {
        ChannelMuxBenchmarks benchmark = new () { MessageCount = 10 };
        await benchmark.LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes();
    }

    public static async Task ChannelMux_LoopTryRead2_4Producer_4Tasks_4ReferenceTypes( ) {
        ChannelMuxBenchmarks benchmark = new () { MessageCount = 10 };
        await benchmark.LoopTryRead2_4Producer_4Tasks_4ReferenceTypes();
    }

    public static async Task ChannelMux_LoopTryRead2_8Producer_8Tasks( ) {
        ChannelMuxBenchmarks benchmark = new () { MessageCount = 10 };
        await benchmark.LoopTryRead2_8Producer_8Tasks();
    }

    private static int _msBetweenMsgGroups = 85;
    private static int _msgsPerSecond      = 500 / 10       * 20;
    private static int _msgsPerMsgGroup    = _msgsPerSecond / ( 1000 / _msBetweenMsgGroups ); // 500 per 10 seconds

    static void producerTaskWithMsgGrouping<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in Stopwatch stopwatch, in int totalMessages, System.Func<int, T> objectFactory ) {
        int i = 0;
        Console.WriteLine( $"Producer Task Starting at {stopwatch.ElapsedTicks:N0} on {Thread.CurrentThread.ManagedThreadId}" );
        Thread.Sleep( 50 ); // startup
        while ( i++ < totalMessages ) {
            writer.TryWrite( objectFactory( i ) );
            if ( i % _msgsPerMsgGroup == 0 ) {
                Thread.Sleep( _msBetweenMsgGroups );
            }
        }
        Console.WriteLine( $"Producer Task Completing at {stopwatch.ElapsedTicks:N0} on {Thread.CurrentThread.ManagedThreadId}" );
        writer.Complete();
    }

    internal static async Task LatencyTest( ) {
        BroadcastChannel<StructB?>     channel1 = new ();
        BroadcastChannel<StructC?>     channel2 = new ();
        ChannelMux<StructB?, StructC?> mux      = new (channel1.Writer, channel2.Writer);
        CancellationToken              ct       = CancellationToken.None;
        // int                            MessageCount = 10_000_000;
        const int messageCount = 1_000_000;
        Stopwatch stopwatch    = Stopwatch.StartNew();
        Task      producer1    = Task.Run( ( ) => producerTaskWithMsgGrouping( channel1.Writer, stopwatch, messageCount, _ => new StructB( stopwatch.ElapsedTicks ) ), ct );
        Task      producer2    = Task.Run( ( ) => producerTaskWithMsgGrouping( channel2.Writer, stopwatch, messageCount, _ => new StructC( stopwatch.ElapsedTicks ) ), ct );
        int       received2    = 0;
        int       received1    = 0;

        Console.WriteLine( $"{nameof(_msBetweenMsgGroups)}: {_msBetweenMsgGroups:N0}\n" +
                           $"{nameof(_msgsPerSecond)}: {_msgsPerSecond:N0}\n"           +
                           $"{nameof(_msgsPerMsgGroup)}: {_msgsPerMsgGroup:N0}\n"       +
                           $"" );
        Log( $"StopWatch Frequency is: {Stopwatch.Frequency:N0}" );
        // using (StreamWriter outputFile = new StreamWriter(file.FullName)){
        Console.WriteLine( $"Beginning at Stopwatch ticks: {stopwatch.ElapsedTicks:N0} on {Thread.CurrentThread.ManagedThreadId}" );
        TimeInfo[] timeInfo1 = new TimeInfo[ messageCount ];
        TimeInfo[] timeInfo2 = new TimeInfo[ messageCount ];
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
        Console.WriteLine( $"{nameof(_msBetweenMsgGroups)}: {_msBetweenMsgGroups:N0}\n" +
                           $"{nameof(_msgsPerSecond)}: {_msgsPerSecond:N0}\n"           +
                           $"{nameof(_msgsPerMsgGroup)}: {_msgsPerMsgGroup:N0}\n"       +
                           $"" );
        var file = new System.IO.FileInfo( System.IO.Path.Combine( System.Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ), "Downloads", "latency_test_results.csv" ) );

        await using ( StreamWriter outputFile = new StreamWriter( file.FullName ) ) {
            for ( int i = 0 ; i < messageCount ; i++ ) {
                // if ( i % 100 == 0 ) {
                await outputFile.WriteLineAsync( $"{timeInfo1[ i ].Seq},{timeInfo1[ i ].Written},{timeInfo1[ i ].Delta},{timeInfo2[ i ].Written},{timeInfo2[ i ].Delta}" ).ConfigureAwait( false );
                // }
            }
        }
        // System.IO.File.WriteAllText( file.FullName, sb.ToString() );
        await producer1;
        await producer2;
        if ( received1 != messageCount || received2 != messageCount ) {
            throw new System.Exception( $"Not all messages were read. {nameof(received1)}: {received1} ; {nameof(received2)}: {received2}" );
        }
    }

    /*
     * Utilities
     */


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
            $"{nameof(mux._tryWrite_monitor_set_booleans)}: {mux._tryWrite_monitor_set_booleans}\n\t"                   +
            $"{nameof(mux._tryWrite_waiting_reader_is_not_null)}: {mux._tryWrite_waiting_reader_is_not_null}\n\t"       +
            $"{nameof(mux._tryWrite_final)}: {mux._tryWrite_final}\n\t" );
        if ( writeToFile ) {
            System.IO.File.AppendAllText( _marksOutputCsvPath,
                                          $"{perfTestTicks},"                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  +
                                          $"{mux._WaitToReadAsync__entry},{mux._WaitToReadAsync__cancellationToken},{mux._WaitToReadAsync__readableItems},{mux._WaitToReadAsync__inLock},{mux._WaitToReadAsync__inLock_readableItems},{mux._WaitToReadAsync__allChannelsComplete},{mux._WaitToReadAsync__TryOwnAndReset},{mux._WaitToReadAsync__new_eq_old},{mux._WaitToReadAsync__TryOwnAndReset_failed},{mux._WaitToReadAsync__completeException},{mux._WaitToReadAsync__inLock_end},{mux._WaitToReadAsync__readableItems_end},{mux._WaitToReadAsync__end}," +
                                          $"{mux._tryWrite_enter},{mux._tryWrite_isComplete_or_exception},{mux._tryWrite_no_reader_waiting},{mux._tryWrite_in_monitor},{mux._tryWrite_monitor_no_waiting_reader},{mux._tryWrite_monitor_set_booleans},{mux._tryWrite_waiting_reader_is_not_null},{mux._tryWrite_final}\n" );
        }
    }
}

file readonly record struct StructB( long TimeSent );

file readonly record struct StructC( long TimeSent );

// /// <summary>
// /// Represents a class which can be used to provide test output.
// /// </summary>
// [SuppressMessage("ReSharper", "TemplateIsNotCompileTimeConstantProblem")]
// file class LoggerTestOutputHelper : ITestOutputHelper {
//     private readonly ILogger _logger;
//
//     public LoggerTestOutputHelper( ILogger logger ) {
//         _logger = logger;
//     }
//
//     /// <summary>Adds a line of text to the output.</summary>
//     /// <param name="message">The message</param>
//     public void WriteLine( string message ) => _logger.LogInformation( message );
//
//     /// <summary>Formats a line of text and adds it to the output.</summary>
//     /// <param name="format">The message format</param>
//     /// <param name="args">The format arguments</param>
//     public void WriteLine(string format, params object[] args) => _logger.LogInformation( String.Format( format, args ) );
// }
/// <summary>
/// Represents a class which can be used to provide test output.
/// </summary>
file class ConsoleTestOutputHelper : ITestOutputHelper {
    /// <summary>Adds a line of text to the output.</summary>
    /// <param name="message">The message</param>
    public void WriteLine( string message ) => Console.WriteLine( message );

    /// <summary>Formats a line of text and adds it to the output.</summary>
    /// <param name="format">The message format</param>
    /// <param name="args">The format arguments</param>
    public void WriteLine( string format, params object[] args ) => Console.WriteLine( format, args );
}

file readonly record struct TimeInfo( int Seq, long Written, long Delta );