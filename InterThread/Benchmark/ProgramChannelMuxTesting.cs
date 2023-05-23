#if DEBUG
#define DEBUG_MUX
#endif

#if DEBUG_MUX
#define DEBUG
// #define LOG
#endif

#if DEBUG_MUX

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Unicode;
using System.Threading;
using System.Threading.Tasks;

using BroadcastChannel;

using BroadcastChannelMux;

namespace Benchmarks.InterThread.Benchmark;

public partial class Program {
    private static async Task ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes( ) {
        ChannelMuxBenchmarks benchmark = new ();
        benchmark.MessageCount = 10;
        await benchmark.ChannelMux_LoopTryRead2_4Producer_1Task_1ValueType_3ReferenceTypes();
    }


    public static async Task ChannelMux_LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes( ) {
        ChannelMuxBenchmarks benchmark = new ();
        benchmark.MessageCount = 10;
        await benchmark.ChannelMux_LoopTryRead2_4Producer_4Tasks_1ValueType_3ReferenceTypes();
    }

    public static async Task ChannelMux_LoopTryRead2_4Producer_4Tasks_4ReferenceTypes( ) {
        ChannelMuxBenchmarks benchmark = new ();
        benchmark.MessageCount = 10;
        await benchmark.ChannelMux_LoopTryRead2_4Producer_4Tasks_4ReferenceTypes();
    }

    public static async Task ChannelMux_LoopTryRead2_8Producer_8Tasks( ) {
        ChannelMuxBenchmarks benchmark = new ();
        benchmark.MessageCount = 10;
        await benchmark.ChannelMux_LoopTryRead2_8Producer_8Tasks();
    }

    // private struct TimeInfo {
    //     // public timeinfo(int Seq, long Written1, long Delta1, long Written2, long Delta2) {
    //     //     this.Seq      = Seq;
    //     //     this.Written1 = Written1;
    //     //     this.Delta1   = Delta1;
    //     //     this.Written2 = Written2;
    //     //     this.Delta2   = Delta2;
    //     // }
    //     public int  Seq                                                                               { get; set; }
    //     public long Written                                                                          { get; set; }
    //     public long Delta                                                                            { get; set; }
    //     public long Written2                                                                          { get; set; }
    //     public long Delta2                                                                            { get; set; }
    //     // public void Deconstruct( out int Seq, out long Written1, out long Delta1, out long Written2, out long Delta2) {
    //     //     Seq      = this.Seq;
    //     //     Written1 = this.Written1;
    //     //     Delta1   = this.Delta1;
    //     //     Written2 = this.Written2;
    //     //     Delta2   = this.Delta2;
    //     // }
    // }

    private readonly record struct TimeInfo( int Seq, long Written, long Delta );


    static void producerTask<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in Stopwatch stopwatch, in int totalMessages, System.Func<int, T> objectFactory ) {
        int i = 0;
        Console.WriteLine( $"Producer Task Starting at {stopwatch.ElapsedTicks:N0} on {Thread.CurrentThread.ManagedThreadId}" );
        Thread.Sleep( 50 ); // startup
        while ( i++ < totalMessages ) {
            writer.TryWrite( objectFactory( i ) );
            if ( i % 10_000 == 0 ) {
                Thread.Sleep( 85 );
            }
        }
        Console.WriteLine( $"Producer Task Completing at {stopwatch.ElapsedTicks:N0} on {Thread.CurrentThread.ManagedThreadId}" );
        writer.Complete();
    }

    private static async Task LatencyTest( ) {
        BroadcastChannel<StructB?>     channel1     = new ();
        BroadcastChannel<StructC?>     channel2     = new ();
        ChannelMux<StructB?, StructC?> mux          = new (channel1.Writer, channel2.Writer);
        CancellationToken              ct           = CancellationToken.None;
        // int                            MessageCount = 10_000_000;
        int                            MessageCount = 1_000_000;
        Stopwatch                      stopwatch    = Stopwatch.StartNew();
        Task                           producer1    = Task.Run( ( ) => producerTask( channel1.Writer, stopwatch, MessageCount, i => new StructB( stopwatch.ElapsedTicks ) ), ct );
        Task                           producer2    = Task.Run( ( ) => producerTask( channel2.Writer, stopwatch, MessageCount, i => new StructC( stopwatch.ElapsedTicks ) ), ct );
        int                            received2    = 0;
        int                            received1    = 0;

        Log( $"StopWatch Frequency is: {Stopwatch.Frequency:N0}" );
        // using (StreamWriter outputFile = new StreamWriter(file.FullName)){
        Console.WriteLine( $"Beginning at Stopwatch ticks: {stopwatch.ElapsedTicks:N0} on {Thread.CurrentThread.ManagedThreadId}" );
        TimeInfo[] timeInfo1 = new TimeInfo[ MessageCount ];
        TimeInfo[] timeInfo2 = new TimeInfo[ MessageCount ];
        while ( await mux.WaitToReadAsync( ct ) ) {
            Console.WriteLine( $"[{((received1 + received2) / 2):N0}] WaitToReadAsync continued, Stopwatch ticks: {stopwatch.ElapsedTicks:N0}" );
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
        var file = new System.IO.FileInfo( System.IO.Path.Combine( System.Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ), "Downloads", "latency_test_results.csv" ) );

        await using ( StreamWriter outputFile = new StreamWriter( file.FullName ) ) {
            for ( int i = 0 ; i < MessageCount ; i++ ) {
                // if ( i % 100 == 0 ) {
                    outputFile.WriteLine( $"{timeInfo1[i].Seq},{timeInfo1[i].Written},{timeInfo1[i].Delta},{timeInfo2[i].Written},{timeInfo2[i].Delta}" );
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

#endif