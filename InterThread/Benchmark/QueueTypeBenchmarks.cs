using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Channels;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

using BroadcastChannelMux;

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
|                                                                     Method | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 |     Gen1 |     Gen2 | Allocated [B] |
|--------------------------------------------------------------------------- |----------:|-----------:|------------:|---------:|---------:|---------:|--------------:|
|                                          SingleProducerSingleConsumerQueue |  4.411 ms |  0.0766 ms |   0.0716 ms | 687.5000 |  78.1250 |        - |     3233036 B |
| Channel_SyncWrite_AsyncReadWait_SyncReadLoop_SingleProducer_SingleConsumer |  5.015 ms |  0.0922 ms |   0.0863 ms | 570.3125 | 546.8750 | 281.2500 |     4254436 B |
|                                                       ConcurrentQueueBased |  6.370 ms |  0.1245 ms |   0.1164 ms | 687.5000 | 125.0000 |        - |     3265626 B |
|                               Channel_SyncWrite_AsyncReadWait_SyncReadLoop |  8.046 ms |  0.0634 ms |   0.0562 ms | 890.6250 | 890.6250 | 500.0000 |     5301806 B |
|                                                         QueueWithLockBased |  9.952 ms |  0.1986 ms |   0.4442 ms | 640.6250 |  93.7500 |  46.8750 |     3409784 B |
|                                   Channel_SyncWrite_AsyncReadWait_SyncRead |  9.975 ms |  0.1399 ms |   0.1374 ms | 890.6250 | 890.6250 | 500.0000 |     5301947 B |
|                                                Channel_SyncWrite_AsyncRead | 10.662 ms |  0.0177 ms |   0.0165 ms | 890.6250 | 890.6250 | 500.0000 |     5301843 B |
|                                               Channel_AsyncWrite_AsyncRead | 11.634 ms |  0.0523 ms |   0.0489 ms | 890.6250 | 890.6250 | 500.0000 |     5301950 B |
|                                                 Channel_SyncWrite_SyncRead | 14.067 ms |  0.2458 ms |   0.2299 ms | 687.5000 |  78.1250 |        - |     3237339 B |

*/

[ Config( typeof(BenchmarkConfig) ) ]
public class QueueTypeBenchmarks {
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
    public async Task SingleProducerSingleConsumerQueue( ) {
        SingleProducerSingleConsumerQueue<ChannelMessage> queue    = new ();
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
    [ Benchmark ]
    public async Task Channel_SyncWrite_AsyncReadWait_SyncReadLoop( ) {
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
                while( reader.TryRead( out ChannelMessage? _ ) ) {
                    received++;
                }
            }
        }
    }
    [ Benchmark ]
    public async Task Channel_SyncWrite_AsyncReadWait_SyncReadLoop_SingleProducer_SingleConsumer( ) {
        Channel<ChannelMessage> channel  = Channel.CreateUnbounded<ChannelMessage>( new UnboundedChannelOptions() {
            SingleReader = true,
            SingleWriter = true
        });
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
                while( reader.TryRead( out ChannelMessage? _ ) ) {
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