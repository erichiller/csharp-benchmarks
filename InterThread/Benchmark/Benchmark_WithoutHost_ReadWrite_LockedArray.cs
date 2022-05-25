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
    private BroadcastQueueWithLockArrayWriter<ChannelMessage, ChannelResponse> _broadcastQueueWithLockArray;
    private BroadcastQueueLockArrayWriter<ChannelMessage, ChannelResponse>     _broadcastQueueLockArrayWriter;


    /* *************************************************************************************************************
     * BROADCAST QUEUE - No Host *********************************************************************************** 
     * ************************************************************************************************************* */

    [ IterationCleanup( Targets = new[] {
        nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayForLoopWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayForLoopWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayForLoopWriter)
    } ) ]
    public void Cleanup_RunBroadcastQueueWithoutHost_LockArrayWriter( ) {
        _broadcastQueueWithLockArray.Writer.Complete();
    }


    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayWriter( ) {
        _broadcastQueueWithLockArray   = new BroadcastQueueWithLockArrayWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1         = _broadcastQueueWithLockArray.GetReader();
        _broadcastQueueLockArrayWriter = _broadcastQueueWithLockArray.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "OneSubscriber", "BroadcastQueueAlt", "LockArrayWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayWriter( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueLockArrayWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueLockArrayWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
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

        _broadcastQueueLockArrayWriter.TryReadResponse( out ChannelResponse? response );
        if ( response?.ReadId != MessageCount ) {
            throw new Exception( $"Expected {MessageCount}, received {response?.ReadId}" );
        }
        // Console.WriteLine($"Response={response}");
    }

    /* *** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayForLoopWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayForLoopWriter( ) {
        _broadcastQueueWithLockArray   = new BroadcastQueueWithLockArrayForLoopWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1         = _broadcastQueueWithLockArray.GetReader();
        _broadcastQueueLockArrayWriter = _broadcastQueueWithLockArray.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "OneSubscriber", "BroadcastQueueAlt", "LockArrayWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayForLoopWriter( ) =>
        BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayWriter();

    /* **** TWO **** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayWriter( ) {
        _broadcastQueueWithLockArray   = new BroadcastQueueWithLockArrayWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1         = _broadcastQueueWithLockArray.GetReader();
        _broadcastQueueReader2         = _broadcastQueueWithLockArray.GetReader();
        _broadcastQueueLockArrayWriter = _broadcastQueueWithLockArray.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "TwoSubscribers", "BroadcastQueueAlt", "LockArrayWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayWriter( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueLockArrayWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueLockArrayWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
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

    /* *** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayForLoopWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayForLoopWriter( ) {
        _broadcastQueueWithLockArray   = new BroadcastQueueWithLockArrayForLoopWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1         = _broadcastQueueWithLockArray.GetReader();
        _broadcastQueueReader2         = _broadcastQueueWithLockArray.GetReader();
        _broadcastQueueLockArrayWriter = _broadcastQueueWithLockArray.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "TwoSubscribers", "BroadcastQueueAlt", "LockArrayWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayForLoopWriter( ) =>
        BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayWriter();

    /* **** THREE **** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayWriter( ) {
        _broadcastQueueWithLockArray   = new BroadcastQueueWithLockArrayWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1         = _broadcastQueueWithLockArray.GetReader();
        _broadcastQueueReader2         = _broadcastQueueWithLockArray.GetReader();
        _broadcastQueueReader3         = _broadcastQueueWithLockArray.GetReader();
        _broadcastQueueLockArrayWriter = _broadcastQueueWithLockArray.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ThreeSubscribers", "BroadcastQueueAlt", "LockArrayWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayWriter( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueLockArrayWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueLockArrayWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
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

        // _broadcastQueueLockArrayWriter.TryReadResponse( out ChannelResponse response );
        // Console.WriteLine($"Response={response}");
    }


    /* *** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayForLoopWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayForLoopWriter( ) {
        _broadcastQueueWithLockArray   = new BroadcastQueueWithLockArrayForLoopWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1         = _broadcastQueueWithLockArray.GetReader();
        _broadcastQueueReader2         = _broadcastQueueWithLockArray.GetReader();
        _broadcastQueueReader3         = _broadcastQueueWithLockArray.GetReader();
        _broadcastQueueLockArrayWriter = _broadcastQueueWithLockArray.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ThreeSubscribers", "BroadcastQueueAlt", "LockArrayWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayForLoopWriter( ) =>
        BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayWriter();
}