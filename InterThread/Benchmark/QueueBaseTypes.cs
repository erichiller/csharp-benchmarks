using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.InterThread.Benchmark;

/*
 
total_messages = 10_000
========================

|               Method | Mean [us] | Error [us] | StdDev [us] | Median [us] |    Gen0 |    Gen1 |   Gen2 | Allocated [B] |
|--------------------- |----------:|-----------:|------------:|------------:|--------:|--------:|-------:|--------------:|
| ConcurrentQueueBased |  375.3 us |    7.26 us |    11.31 us |    372.3 us | 71.2891 |  7.8125 | 0.4883 |      337837 B |
|   QueueWithLockBased |  916.0 us |   22.07 us |    64.38 us |    893.5 us | 82.0313 | 11.7188 | 1.9531 |      394078 B |
 
 
total_messages = 100_000
========================

|                       Method | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|----------------------------- |----------:|-----------:|------------:|---------:|---------:|---------:|--------------:|
|         ConcurrentQueueBased |  6.409 ms |  0.1257 ms |   0.1589 ms | 687.5000 | 132.8125 |        - |     3266328 B |
|           QueueWithLockBased |  9.904 ms |  0.1962 ms |   0.4388 ms | 625.0000 | 171.8750 |  78.1250 |     3500955 B |
|  Channel_SyncWrite_AsyncRead | 10.633 ms |  0.0731 ms |   0.0610 ms | 875.0000 | 875.0000 | 484.3750 |     5301660 B |
| Channel_AsyncWrite_AsyncRead | 11.588 ms |  0.0512 ms |   0.0428 ms | 890.6250 | 890.6250 | 500.0000 |     5301913 B |
|   Channel_SyncWrite_SyncRead | 14.113 ms |  0.2762 ms |   0.3070 ms | 687.5000 | 109.3750 |        - |     3267660 B |


*/

[ Config( typeof(BenchmarkConfig) ) ]
public class QueueBaseTypes {
    private int total_messages = 100_000;

    [ Benchmark ]
    public async Task ConcurrentQueueBased( ) {
        ConcurrentQueue<ChannelMessage> queue    = new ();
        int                             received = 0;

        Task producer = new Task( ( ) => {
            int i = 0;
            while ( i++ < total_messages ) {
                queue.Enqueue( new ChannelMessage {
                                   Id        = i,
                                   Property1 = @"some_text"
                               } );
            }
        } );
        Task consumer = new Task( ( ) => {
            while ( received < total_messages ) {
                if ( queue.TryDequeue( out ChannelMessage? _ ) ) {
                    received++;
                }
            }
        } );
        producer.Start();
        consumer.Start();
        await producer;
        await consumer;
        if ( received != total_messages ) {
            throw new Exception();
        }
    }

    [ Benchmark ]
    public async Task QueueWithLockBased( ) {
        Queue<ChannelMessage> queue    = new ();
        object                lockObj  = new ();
        int                   received = 0;

        Task producer = new Task( ( ) => {
            int i = 0;
            while ( i++ < total_messages ) {
                lock ( lockObj ) {
                    queue.Enqueue( new ChannelMessage {
                                       Id        = i,
                                       Property1 = @"some_text"
                                   } );
                }
            }
        } );
        Task consumer = new Task( ( ) => {
            while ( received < total_messages ) {
                lock ( lockObj ) {
                    if ( queue.TryDequeue( out ChannelMessage? _ ) ) {
                        received++;
                    }
                }
            }
        } );
        producer.Start();
        consumer.Start();
        await producer;
        await consumer;
        if ( received != total_messages ) {
            throw new Exception();
        }
    }

    [ Benchmark ]
    public async Task Channel_SyncWrite_SyncRead( ) {
        Channel<ChannelMessage> channel  = Channel.CreateUnbounded<ChannelMessage>();
        var                     reader   = channel.Reader;
        var                     writer   = channel.Writer;
        int                     received = 0;

        Task producer = new Task( ( ) => {
            int i = 0;
            while ( i++ < total_messages ) {
                writer.TryWrite( new ChannelMessage {
                                     Id        = i,
                                     Property1 = @"some_text"
                                 } );
            }
        } );
        Task consumer = new Task( ( ) => {
            while ( received < total_messages ) {
                if ( reader.TryRead( out ChannelMessage? _ ) ) {
                    received++;
                }
            }
        } );
        producer.Start();
        consumer.Start();
        await producer;
        await consumer;
        if ( received != total_messages ) {
            throw new Exception();
        }
    }

    [ Benchmark ]
    public async Task Channel_AsyncWrite_AsyncRead( ) {
        Channel<ChannelMessage> channel  = Channel.CreateUnbounded<ChannelMessage>();
        var                     reader   = channel.Reader;
        var                     writer   = channel.Writer;
        int                     received = 0;
        
        var producer = Produce();
        var consumer = Consume();
        await producer;
        await consumer;
        if ( received != total_messages ) {
            throw new Exception();
        }

        async Task Produce( ) {
            int i = 0;
            while ( i++ < total_messages ) {
                await writer.WriteAsync( new ChannelMessage {
                    Id        = i,
                    Property1 = @"some_text"
                } );
            }
            writer.Complete();
        }

        async Task Consume( ) {
            while ( received < total_messages ) {
                await reader.WaitToReadAsync();
                _ = await reader.ReadAsync();
                received++;
            }
        }
    }

    [ Benchmark ]
    public async Task Channel_SyncWrite_AsyncRead( ) {
        Channel<ChannelMessage> channel  = Channel.CreateUnbounded<ChannelMessage>();
        var                     reader   = channel.Reader;
        var                     writer   = channel.Writer;
        int                     received = 0;
        
        var producer = Produce();
        var consumer = Consume();
        await producer;
        await consumer;
        if ( received != total_messages ) {
            throw new Exception();
        }

        Task Produce( ) {
            int i = 0;
            while ( i++ < total_messages ) {
                writer.TryWrite( new ChannelMessage {
                    Id        = i,
                    Property1 = @"some_text"
                } );
            }
            writer.Complete();
            return Task.CompletedTask;
        }

        async Task Consume( ) {
            while ( received < total_messages ) {
                await reader.WaitToReadAsync();
                _ = await reader.ReadAsync();
                received++;
            }
        }
    }
    [ Benchmark ]
    public async Task Channel_SyncWrite_AsyncReadWait_SyncRead( ) {
        Channel<ChannelMessage> channel  = Channel.CreateUnbounded<ChannelMessage>();
        var                     reader   = channel.Reader;
        var                     writer   = channel.Writer;
        int                     received = 0;
        
        var producer = Produce();
        var consumer = Consume();
        await producer;
        await consumer;
        if ( received != total_messages ) {
            throw new Exception();
        }

        Task Produce( ) {
            int i = 0;
            while ( i++ < total_messages ) {
                writer.TryWrite( new ChannelMessage {
                    Id        = i,
                    Property1 = @"some_text"
                } );
            }
            writer.Complete();
            return Task.CompletedTask;
        }

        async Task Consume( ) {
            while ( received < total_messages ) {
                await reader.WaitToReadAsync();
                if ( reader.TryRead( out ChannelMessage? _ ) ) {
                    received++;
                }
            }
        }
    }

    // [ Benchmark ]
    // public void SpanWithLockBased( ) { // uses 100% CPU -- takes forever
    //     Memory<ChannelMessage?> queue    = new (new ChannelMessage[ total_messages ]);
    //     object                  lockObj  = new ();
    //     int                     received = 0;
    //
    //     Task producer = new Task( ( ) => {
    //         int i = 0;
    //         while ( i++ < total_messages ) {
    //             lock ( lockObj ) {
    //                 queue.Span[ i ] = ( new ChannelMessage {
    //                     Id        = i,
    //                     Property1 = @"some_text"
    //                 } );
    //             }
    //         }
    //     } );
    //     Task consumer = new Task( ( ) => {
    //         while ( received < total_messages ) {
    //             lock ( lockObj ) {
    //                 if ( queue.Span[ received ] is { } ) {
    //                     received++;
    //                 }
    //             }
    //         }
    //     } );
    //     producer.Start();
    //     consumer.Start();
    // }
}