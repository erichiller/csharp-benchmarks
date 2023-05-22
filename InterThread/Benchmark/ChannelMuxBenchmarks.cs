using System;
using System.Threading;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

using BroadcastChannel;

using BroadcastChannelMux;

namespace Benchmarks.InterThread.Benchmark;

/*
 * 
|     Method | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |    Gen1 | Allocated [B] |
|----------- |----------:|-----------:|------------:|---------:|--------:|--------------:|
| ChannelMux |  39.35 ms |   0.754 ms |    0.869 ms | 846.1538 | 76.9231 |     4187471 B |


 ChannelMux backed by ConcurrentQueue
======================================
|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |     Gen1 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|---------:|---------:|--------------:|
|       ChannelMux_LoopTryRead |  37.76 ms |   0.696 ms |    0.617 ms | 857.1429 | 214.2857 |     4201950 B |
| ChannelMux_AsyncWaitLoopOnly |  39.57 ms |   0.525 ms |    0.466 ms | 846.1538 |  76.9231 |     4132924 B |



 ChannelMux backed by SingleProducerSingleConsumerQueue
========================================================
|                 Method | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------- |----------:|-----------:|------------:|---------:|---------:|---------:|--------------:|
|   BroadcastChannelOnly |  12.11 ms |   0.208 ms |    0.264 ms | 921.8750 | 531.2500 | 203.1250 |     5008816 B |
| ChannelMux_LoopTryRead |  34.33 ms |   0.681 ms |    1.245 ms | 866.6667 | 133.3333 |  66.6667 |     4456274 B |
|             ChannelMux |  36.24 ms |   0.419 ms |    0.392 ms | 785.7143 |  71.4286 |        - |     4085486 B |



lock replaced with SpinLock
======================================
|                 Method | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |    Gen1 | Allocated [B] |
|----------------------- |----------:|-----------:|------------:|---------:|--------:|--------------:|
| ChannelMux_LoopTryRead |  47.16 ms |   0.926 ms |    1.716 ms | 818.1818 | 90.9091 |     4192649 B |




lock in TryWrite replaced with Monitor.TryEnter
================================================
|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|       ChannelMux_LoopTryRead |  23.70 ms |   0.470 ms |    0.674 ms | 1187.5000 | 406.2500 | 312.5000 |     6701274 B |
| ChannelMux_AsyncWaitLoopOnly |  24.56 ms |   0.313 ms |    0.293 ms | 1687.5000 | 406.2500 | 187.5000 |     8332670 B |

|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|       ChannelMux_LoopTryRead |  23.37 ms |   0.218 ms |    0.204 ms | 1375.0000 | 468.7500 | 343.7500 |     6643177 B |
| ChannelMux_AsyncWaitLoopOnly |  24.97 ms |   0.401 ms |    0.376 ms | 1062.5000 |  62.5000 |        - |     5223778 B |

lock in TryWrite replaced with Monitor.TryEnter + revised Exception code
==========================================================================
|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|----------:|---------:|---------:|--------------:|
|       ChannelMux_LoopTryRead |  23.55 ms |   0.464 ms |    0.476 ms | 1125.0000 | 406.2500 | 343.7500 |     6601251 B |
| ChannelMux_AsyncWaitLoopOnly |  24.77 ms |   0.247 ms |    0.219 ms | 1187.5000 | 156.2500 |        - |     5841614 B |



 */

[ Config( typeof(BenchmarkConfig) ) ]
public class ChannelMuxBenchmarks {
    private int totalMessages = 100_000;

    static void producerTask<T>( in BroadcastChannelWriter<T, IBroadcastChannelResponse> writer, in int totalMessages, System.Func<int, T> objectFactory ) {
        int i = 0;
        while ( i++ < totalMessages ) {
            writer.TryWrite( objectFactory( i ) );
        }
        writer.Complete();
    }

    [ Benchmark ]
    public async Task ChannelMux_AsyncWaitLoopOnly( ) {
        BroadcastChannel<StructA, IBroadcastChannelResponse> channel1 = new ();
        BroadcastChannel<ClassA>                             channel2 = new ();
        ChannelMux<StructA, ClassA>                          mux      = new (channel1.Writer, channel2.Writer);
        CancellationToken                                    ct       = CancellationToken.None;
        Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, totalMessages, i => new StructA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, totalMessages, i => new ClassA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        int     receivedCountStructA = 0;
        int     receivedCountClassA  = 0;
        
        // ReSharper disable UnusedVariable        
        ClassA? classA;
        StructA structA;
        while ( await mux.WaitToReadAsync( ct ) ) {
            if ( mux.TryRead( out classA ) ) {
                receivedCountClassA++;
            }
            if ( mux.TryRead( out structA ) ) {
                receivedCountStructA++;
            }
        }
        // ReSharper restore UnusedVariable
        await producer1;
        await producer2;
        if ( receivedCountClassA != totalMessages || receivedCountStructA != totalMessages ) {
            throw new System.Exception( $"Not all messages were read. {nameof(receivedCountClassA)}: {receivedCountClassA} ; {nameof(receivedCountStructA)}: {receivedCountStructA}" );
        }
    }

    [ Benchmark ]
    public async Task ChannelMux_LoopTryRead( ) {
        BroadcastChannel<StructA?, IBroadcastChannelResponse> channel1 = new ();
        BroadcastChannel<ClassA>                              channel2 = new ();
        ChannelMux<StructA?, ClassA>                          mux      = new (channel1.Writer, channel2.Writer);
        CancellationToken                                     ct       = CancellationToken.None;
        Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, totalMessages, i => new StructA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, totalMessages, i => new ClassA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        int      receivedCountStructA = 0;
        int      receivedCountClassA  = 0;
        StructA? structA              = null;
        while ( await mux.WaitToReadAsync( ct ) ) {
            while ( mux.TryRead( out ClassA? classA ) || mux.TryRead( out structA ) ) {
                if ( classA is { } ) {
                    receivedCountClassA++;
                }
                if ( structA is { } ) {
                    receivedCountStructA++;
                    structA = null;
                }
            }
        }
        await producer1;
        await producer2;
        if ( receivedCountClassA != totalMessages || receivedCountStructA != totalMessages ) {
            throw new System.Exception( $"Not all messages were read. {nameof(receivedCountClassA)}: {receivedCountClassA} ; {nameof(receivedCountStructA)}: {receivedCountStructA}" );
        }
    }

    [ Benchmark ]
    public async Task BroadcastChannelOnly( ) {
        BroadcastChannel<StructA?, IBroadcastChannelResponse> channel1       = new ();
        BroadcastChannel<ClassA>                              channel2       = new ();
        var                                                   channelReader1 = channel1.GetReader(); // these must be setup BEFORE the producer begins
        var                                                   channelReader2 = channel2.GetReader();
        CancellationToken                                     ct             = CancellationToken.None;
        Task producer1 = Task.Run( ( ) => producerTask( channel1.Writer, totalMessages, i => new StructA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        Task producer2 = Task.Run( ( ) => producerTask( channel2.Writer, totalMessages, i => new ClassA {
                                                            Id   = i,
                                                            Name = @"some_text"
                                                        } ), ct );
        int receivedCountStructA = 0;
        int receivedCountClassA  = 0;
        // ReSharper disable UnusedVariable
        Task reader1 = Task.Run( async ( ) => {
            while ( await channelReader1.WaitToReadAsync( ct ) ) {
                if ( channelReader1.TryRead( out StructA? structA ) ) {
                    receivedCountStructA++;
                }
            }
        } );
        Task reader2 = Task.Run( async ( ) => {
            while ( await channelReader2.WaitToReadAsync( ct ) ) {
                if ( channelReader2.TryRead( out ClassA? classA ) ) {
                    receivedCountClassA++;
                }
            }
        } );
        // ReSharper restore UnusedVariable
        await producer1;
        await producer2;
        await reader1;
        await reader2;
        if ( receivedCountClassA != totalMessages || receivedCountStructA != totalMessages ) {
            throw new System.Exception( $"Not all messages were read. {nameof(receivedCountClassA)}: {receivedCountClassA} ; {nameof(receivedCountStructA)}: {receivedCountStructA}" );
        }
    }
}

public struct StructA {
    public          int    Id;
    public          string Name;
    public          string Description;
    public override string ToString( ) => $"{nameof(StructA)} {{ Id = {Id}, Name = {Name}, Description = {Description} }}";
}

public class ClassA {
    public          int    Id;
    public          string Name        = String.Empty;
    public          string Description = String.Empty;
    public override string ToString( ) => $"{nameof(ClassA)} {{ Id = {Id}, Name = {Name}, Description = {Description} }}";
}