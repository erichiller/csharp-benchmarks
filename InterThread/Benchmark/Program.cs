using System;
using System.Linq;
using System.Threading.Channels;

using BenchmarkDotNet.Running;

using Benchmarks.Common;


namespace Benchmarks.InterThread.Benchmark;

public class Program {
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
        } else if ( args.Contains( "memtest" ) ) {
            T printMemoryDiff<T>( Func<T> callback) {
                long memoryStart = System.GC.GetTotalMemory( true );
                var x = callback();
                Console.WriteLine($"Memory difference after allocation for {typeof(T)}: {System.GC.GetTotalMemory( true ) - memoryStart} bytes");
                return x;
            }
            var x = printMemoryDiff( Channel.CreateUnbounded<ChannelMessage> );
            printMemoryDiff( ( ) => x.Reader );
            printMemoryDiff( ( ) => x.Writer );
            printMemoryDiff( () => Channel.CreateBounded<ChannelMessage>(10) );
            printMemoryDiff( () => Channel.CreateBounded<ChannelMessage>(100) );
            printMemoryDiff( () => Channel.CreateBounded<ChannelMessage>(1000) );
        } else {
            BenchmarkSwitcher
                .FromAssembly( typeof(Program).Assembly )
                .Run( args.Length > 0
                          ? args
                          : new[] { "-f", "*" },
                      new BenchmarkConfig()
                      // new DebugBuildConfig(){}
                      // new DebugInProcessConfig() // NOTE: use to debug from the IDE
                      // ManualConfig
                      // .Create( DefaultConfig.Instance )
                      //     .WithOptions( ConfigOptions.StopOnFirstError |
                      //                   ConfigOptions.JoinSummary )
                );
        }
    }
}