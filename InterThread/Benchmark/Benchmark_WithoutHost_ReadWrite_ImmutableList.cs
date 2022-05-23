using System;
using System.Threading.Channels;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using Benchmarks.InterThread.BroadcastQueue;

namespace Benchmarks.InterThread.Benchmark;

public class B2enchmarks {
    // kill
    private int                                                   MessageCount;
    private BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader1;
    private BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader2;
    private BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader3;
}

public partial class Benchmarks {
    private BroadcastQueueWithImmutableListWriter<ChannelMessage, ChannelResponse> _broadcastQueueWithImmutableList;
    private BroadcastQueueImmutableListWriter<ChannelMessage, ChannelResponse>     _broadcastQueueImmutableListWriter;


    /* *************************************************************************************************************
     * BROADCAST QUEUE - No Host *********************************************************************************** 
     * ************************************************************************************************************* */

    [ IterationCleanup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableListWriter), 
        nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableListWriter), nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableListWriter) } ) ]
    public void Cleanup_RunBroadcastQueueWithoutHost_ImmutableListWriter( ) {
        _broadcastQueueWithImmutableList.Writer.Complete();
    }


    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableListWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableListWriter( ) {
        _broadcastQueueWithImmutableList                    = new BroadcastQueueWithImmutableListWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1             = _broadcastQueueWithImmutableList.GetReader();
        _broadcastQueueImmutableListWriter = _broadcastQueueWithImmutableList.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "OneSubscriber", "BroadcastQueueAlt", "ImmutableListWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableListWriter( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueImmutableListWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueImmutableListWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
                    i++;
                    if ( i > MessageCount ) {
                        return;
                    }
                }
            }
        }

        async Task readerTask( ) {
            while ( await _broadcastQueueReader1.WaitToReadAsync() ) {
                while ( _broadcastQueueReader1.TryRead( out ChannelMessage? result ) ) {
                    if ( result.Id >= MessageCount ) {
                        await _broadcastQueueReader1.WriteResponseAsync( new ChannelResponse( result.Id, nameof(readerTask) ) );
                        return;
                    }
                }
            }
        }

        Task.WaitAll(
            Task.Run( readerTask ),
            Task.Run( writerTask )
        );

        _broadcastQueueImmutableListWriter.TryReadResponse( out ChannelResponse? response );
        if ( response?.ReadId != MessageCount ) {
            throw new Exception();
        }
        // Console.WriteLine($"Response={response}");
    }

    /* **** TWO **** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableListWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableListWriter( ) {
        _broadcastQueueWithImmutableList                    = new BroadcastQueueWithImmutableListWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1             = _broadcastQueueWithImmutableList.GetReader();
        _broadcastQueueReader2             = _broadcastQueueWithImmutableList.GetReader();
        _broadcastQueueImmutableListWriter = _broadcastQueueWithImmutableList.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "TwoSubscribers", "BroadcastQueueAlt", "ImmutableListWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableListWriter( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueImmutableListWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueImmutableListWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
                    i++;
                    if ( i > MessageCount ) {
                        return;
                    }
                }
            }
        }

        async Task readerTask1( ) {
            while ( await _broadcastQueueReader1.WaitToReadAsync() ) {
                while ( _broadcastQueueReader1.TryRead( out ChannelMessage? result ) ) {
                    if ( result.Id >= MessageCount ) {
                        var response = new ChannelResponse( result.Id, nameof(readerTask1) );
                        // Console.WriteLine($"ReaderTask Complete @ {MessageCount} - BroadcastQueue");
                        await _broadcastQueueReader1.WriteResponseAsync( response );
                        return;
                    }
                }
            }
        }

        async Task readerTask2( ) {
            while ( await _broadcastQueueReader2.WaitToReadAsync() ) {
                while ( _broadcastQueueReader2.TryRead( out ChannelMessage? result ) ) {
                    if ( result.Id >= MessageCount ) {
                        var response = new ChannelResponse( result.Id, nameof(readerTask2) );
                        // Console.WriteLine($"ReaderTask Complete @ {MessageCount} - BroadcastQueue");
                        await _broadcastQueueReader2.WriteResponseAsync( response );
                        return;
                    }
                }
            }
        }

        Task.WaitAll(
            Task.Run( readerTask1 ),
            Task.Run( readerTask2 ),
            Task.Run( writerTask )
        );
    }


    /* **** THREE **** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableListWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableListWriter( ) {
        _broadcastQueueWithImmutableList                    = new BroadcastQueueWithImmutableListWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1             = _broadcastQueueWithImmutableList.GetReader();
        _broadcastQueueReader2             = _broadcastQueueWithImmutableList.GetReader();
        _broadcastQueueReader3             = _broadcastQueueWithImmutableList.GetReader();
        _broadcastQueueImmutableListWriter = _broadcastQueueWithImmutableList.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ThreeSubscribers", "BroadcastQueueAlt", "ImmutableListWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableListWriter( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueImmutableListWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueImmutableListWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
                    i++;
                    if ( i > MessageCount ) {
                        return;
                    }
                }
            }
        }

        async Task readerTask1( ) {
            while ( await _broadcastQueueReader1.WaitToReadAsync() ) {
                while ( _broadcastQueueReader1.TryRead( out ChannelMessage? result ) ) {
                    if ( result.Id >= MessageCount ) {
                        var response = new ChannelResponse( result.Id, nameof(readerTask1) );
                        // Console.WriteLine($"ReaderTask Complete @ {MessageCount} - BroadcastQueue");
                        await _broadcastQueueReader1.WriteResponseAsync( response );
                        return;
                    }
                }
            }
        }

        async Task readerTask2( ) {
            while ( await _broadcastQueueReader2.WaitToReadAsync() ) {
                while ( _broadcastQueueReader2.TryRead( out ChannelMessage? result ) ) {
                    if ( result.Id >= MessageCount ) {
                        var response = new ChannelResponse( result.Id, nameof(readerTask2) );
                        // Console.WriteLine($"ReaderTask Complete @ {MessageCount} - BroadcastQueue");
                        await _broadcastQueueReader2.WriteResponseAsync( response );
                        return;
                    }
                }
            }
        }

        async Task readerTask3( ) {
            while ( await _broadcastQueueReader3.WaitToReadAsync() ) {
                while ( _broadcastQueueReader3.TryRead( out ChannelMessage? result ) ) {
                    if ( result.Id >= MessageCount ) {
                        var response = new ChannelResponse( result.Id, nameof(readerTask3) );
                        // Console.WriteLine($"ReaderTask Complete @ {MessageCount} - BroadcastQueue");
                        await _broadcastQueueReader3.WriteResponseAsync( response );
                        return;
                    }
                }
            }
        }

        Task.WaitAll(
            Task.Run( readerTask1 ),
            Task.Run( readerTask2 ),
            Task.Run( readerTask3 ),
            Task.Run( writerTask )
        );

        // _broadcastQueueImmutableListWriter.TryReadResponse( out ChannelResponse response );
        // Console.WriteLine($"Response={response}");
    }
}