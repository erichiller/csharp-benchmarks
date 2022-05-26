using System;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using Benchmarks.InterThread.BroadcastQueue;

namespace Benchmarks.InterThread.Benchmark;

// public class B2enchmarks {
//     // kill
//     private int                                                   MessageCount;
//     private BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader1;
//     private BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader2;
//     private BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader3;
// }

public partial class Benchmarks {
    private BroadcastQueueWithLockedList<ChannelMessage, ChannelResponse> _broadcastQueueWithLockedList;
    private BroadcastQueueWithLockedListWriter<ChannelMessage, ChannelResponse> _broadcastQueueLockedListWriter;


    /* *************************************************************************************************************
     * BROADCAST QUEUE - No Host *********************************************************************************** 
     * ************************************************************************************************************* */

    [ IterationCleanup( Targets = new[] {
        nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedListWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedListWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedListWriter)
    } ) ]
    public void Cleanup_RunBroadcastQueueWithoutHost_LockedListWriter( ) {
        _broadcastQueueWithLockedList.Writer.Complete();
    }


    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedListWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedListWriter( ) {
        _broadcastQueueWithLockedList   = new BroadcastQueueWithLockedList<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1         = _broadcastQueueWithLockedList.GetReader();
        _broadcastQueueLockedListWriter = _broadcastQueueWithLockedList.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "OneSubscriber", "BroadcastQueueAlt", "LockedListWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedListWriter( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueLockedListWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueLockedListWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
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
                        // Console.WriteLine( $"result.Id ({result.Id} ) >= MessageCount {MessageCount}" );
                        await _broadcastQueueReader1.WriteResponseAsync( new ChannelResponse( result.Id, nameof(readerTask) ) );
                        // Console.WriteLine( $"WriteResponseAsync( {result.Id} )" );
                        return;
                    }
                }
            }
        }

        Task.WaitAll(
            Task.Run( readerTask ),
            Task.Run( writerTask )
        );

        _broadcastQueueLockedListWriter.TryReadResponse( out ChannelResponse? response );
        if ( response?.ReadId != MessageCount ) {
            throw new Exception( $"Expected {MessageCount}, received {response?.ReadId}" );
        }
        // Console.WriteLine($"Response={response}");
    }

    /* **** TWO **** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedListWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedListWriter( ) {
        _broadcastQueueWithLockedList  = new BroadcastQueueWithLockedList<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1         = _broadcastQueueWithLockedList.GetReader();
        _broadcastQueueReader2         = _broadcastQueueWithLockedList.GetReader();
        _broadcastQueueLockedListWriter = _broadcastQueueWithLockedList.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "TwoSubscribers", "BroadcastQueueAlt", "LockedListWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedListWriter( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueLockedListWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueLockedListWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
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

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedListWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedListWriter( ) {
        _broadcastQueueWithLockedList  = new BroadcastQueueWithLockedList<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1         = _broadcastQueueWithLockedList.GetReader();
        _broadcastQueueReader2         = _broadcastQueueWithLockedList.GetReader();
        _broadcastQueueReader3         = _broadcastQueueWithLockedList.GetReader();
        _broadcastQueueLockedListWriter = _broadcastQueueWithLockedList.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ThreeSubscribers", "BroadcastQueueAlt", "LockedListWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedListWriter( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueLockedListWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueLockedListWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
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

        // _broadcastQueueLockedListWriter.TryReadResponse( out ChannelResponse response );
        // Console.WriteLine($"Response={response}");
    }
}