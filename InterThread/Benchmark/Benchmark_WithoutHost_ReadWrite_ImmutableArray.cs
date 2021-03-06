using System;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using Benchmarks.InterThread.BroadcastQueue;

namespace Benchmarks.InterThread.Benchmark;

public partial class Benchmarks {
    private BroadcastQueueWithImmutableArrayWriter<ChannelMessage, ChannelResponse> _broadcastQueueWithImmutableArray;
    private BroadcastQueueImmutableArrayWriter<ChannelMessage, ChannelResponse>     _broadcastQueueImmutableArrayWriter;


    /* *************************************************************************************************************
     * BROADCAST QUEUE - No Host *********************************************************************************** 
     * ************************************************************************************************************* */

    [ IterationCleanup( Targets = new[] {
        nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableArrayWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableArrayWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableArrayWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedImmutableArrayWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedImmutableArrayWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedImmutableArrayWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedImmutableArrayForLoopWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedImmutableArrayForLoopWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedImmutableArrayForLoopWriter)
    } ) ]
    public void Cleanup_RunBroadcastQueueWithoutHost_ImmutableArrayWriter( ) {
        _broadcastQueueWithImmutableArray.Writer.Complete();
    }


    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableArrayWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableArrayWriter( ) {
        _broadcastQueueWithImmutableArray   = new BroadcastQueueWithImmutableArrayWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueImmutableArrayWriter = _broadcastQueueWithImmutableArray.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "OneSubscriber", "BroadcastQueueAlt", "ImmutableArrayWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableArrayWriter( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueImmutableArrayWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueImmutableArrayWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
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

        _broadcastQueueImmutableArrayWriter.TryReadResponse( out ChannelResponse? response );
        if ( response?.ReadId != MessageCount ) {
            throw new Exception();
        }
        // Console.WriteLine($"Response={response}");
    }
    
    /* *** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedImmutableArrayWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedImmutableArrayWriter( ) {
        _broadcastQueueWithImmutableArray   = new BroadcastQueueWithLockedImmutableArrayWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueImmutableArrayWriter = _broadcastQueueWithImmutableArray.Writer;
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "OneSubscriber", "BroadcastQueueAlt", "Locking", "ImmutableArrayWriter", "LockedImmutableArrayWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedImmutableArrayWriter( ) =>
        BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableArrayWriter();
    
    /* **** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedImmutableArrayForLoopWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedImmutableArrayForLoopWriter( ) {
        _broadcastQueueWithImmutableArray   = new BroadcastQueueWithLockedImmutableArrayForLoopWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueReader2              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueReader3              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueImmutableArrayWriter = _broadcastQueueWithImmutableArray.Writer;
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "OneSubscriber", "BroadcastQueueAlt", "Locking", "ImmutableArrayWriter", "ForLoop" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedImmutableArrayForLoopWriter( ) =>
        BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableArrayWriter();

    /* **** TWO **** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableArrayWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableArrayWriter( ) {
        _broadcastQueueWithImmutableArray   = new BroadcastQueueWithImmutableArrayWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueReader2              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueImmutableArrayWriter = _broadcastQueueWithImmutableArray.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "TwoSubscribers", "BroadcastQueueAlt", "ImmutableArrayWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableArrayWriter( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueImmutableArrayWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueImmutableArrayWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
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

    /* **** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedImmutableArrayWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedImmutableArrayWriter( ) {
        _broadcastQueueWithImmutableArray   = new BroadcastQueueWithLockedImmutableArrayWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueReader2              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueImmutableArrayWriter = _broadcastQueueWithImmutableArray.Writer;
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "TwoSubscribers", "BroadcastQueueAlt", "Locking", "ImmutableArrayWriter", "LockedImmutableArrayWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedImmutableArrayWriter( ) =>
        BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableArrayWriter();
    
    /* **** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedImmutableArrayForLoopWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedImmutableArrayForLoopWriter( ) {
        _broadcastQueueWithImmutableArray   = new BroadcastQueueWithLockedImmutableArrayForLoopWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueReader2              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueReader3              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueImmutableArrayWriter = _broadcastQueueWithImmutableArray.Writer;
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "TwoSubscribers", "BroadcastQueueAlt", "Locking", "ImmutableArrayWriter", "ForLoop" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedImmutableArrayForLoopWriter( ) =>
        BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableArrayWriter();

    /* **** THREE **** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableArrayWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableArrayWriter( ) {
        _broadcastQueueWithImmutableArray   = new BroadcastQueueWithImmutableArrayWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueReader2              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueReader3              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueImmutableArrayWriter = _broadcastQueueWithImmutableArray.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ThreeSubscribers", "BroadcastQueueAlt", "ImmutableArrayWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableArrayWriter( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueImmutableArrayWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueImmutableArrayWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
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

        // _broadcastQueueImmutableArrayWriter.TryReadResponse( out ChannelResponse response );
        // Console.WriteLine($"Response={response}");
    }
    
    
    /* **** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedImmutableArrayWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedImmutableArrayWriter( ) {
        _broadcastQueueWithImmutableArray   = new BroadcastQueueWithLockedImmutableArrayWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueReader2              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueReader3              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueImmutableArrayWriter = _broadcastQueueWithImmutableArray.Writer;
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "ThreeSubscribers", "BroadcastQueueAlt", "Locking", "ImmutableArrayWriter", "LockedImmutableArrayWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedImmutableArrayWriter( ) =>
        BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableArrayWriter();
    
    /* **** */

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedImmutableArrayForLoopWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedImmutableArrayForLoopWriter( ) {
        _broadcastQueueWithImmutableArray   = new BroadcastQueueWithLockedImmutableArrayForLoopWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueReader2              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueReader3              = _broadcastQueueWithImmutableArray.GetReader();
        _broadcastQueueImmutableArrayWriter = _broadcastQueueWithImmutableArray.Writer;
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "ThreeSubscribers", "BroadcastQueueAlt", "Locking", "ImmutableArrayWriter", "ForLoop" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedImmutableArrayForLoopWriter( ) =>
        BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableArrayWriter();

}

// TODO: last item: see if using an interface as the field slows things down.
// Then I could parameterize the BroadcastQueue variant
// Plus parameterize the number of subscribers!