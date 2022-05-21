using System.Threading.Channels;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using Benchmarks.InterThread.BroadcastQueue;

namespace Benchmarks.InterThread.Benchmark;

public partial class Benchmarks {
    

    // URGENT: nothing has been checking the Response Channel!
    /* *************************************************************************************************************
     * BROADCAST QUEUE - No Host *********************************************************************************** 
     * ************************************************************************************************************* */
    
    [ IterationSetup( Target = nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber) ) ]
    public void Setup_RunBroadcastQueueWithoutHostTest( ) {
        _broadcastQueue       = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueWriter = _broadcastQueue.Writer;
    }

    [ IterationCleanup( Targets = new[] {
        nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers)
    } ) ]
    public void Cleanup_RunBroadcastQueueWithoutHostTest( ) {
        _broadcastQueue.Writer.Complete();
    }

    [ Benchmark ]
    public void BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
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
                        var response = new ChannelResponse( result.Id, nameof(readerTask) );
                        // Console.WriteLine($"ReaderTask Complete @ {MessageCount} - BroadcastQueue");
                        await _broadcastQueueReader1.WriteResponseAsync( response );
                        return;
                    }
                }
            }
        }

        Task.WaitAll(
            Task.Run( readerTask ),
            Task.Run( writerTask )
        );

        // _broadcastQueueWriter.TryReadResponse( out ChannelResponse response );
        // Console.WriteLine($"Response={response}");
    }

    private BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader2;

    [ IterationSetup( Target = nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers) ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers( ) {
        _broadcastQueue        = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueReader2 = _broadcastQueue.GetReader();
        _broadcastQueueWriter  = _broadcastQueue.Writer;
    }
    
    [ Benchmark ]
    public void BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
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

        // _broadcastQueueWriter.TryReadResponse( out ChannelResponse response );
        // Console.WriteLine($"Response={response}");
    }

    private BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader3;

    [ IterationSetup( Target = nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers) ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers( ) {
        _broadcastQueue        = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueReader2 = _broadcastQueue.GetReader();
        _broadcastQueueReader3 = _broadcastQueue.GetReader();
        _broadcastQueueWriter  = _broadcastQueue.Writer;
    }

    [ Benchmark ]
    public void BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueWriter.WaitToWriteAsync() ) {
                while ( _broadcastQueueWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
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

        // _broadcastQueueWriter.TryReadResponse( out ChannelResponse response );
        // Console.WriteLine($"Response={response}");
    }

    

    /* *************************************************************************************************************
     * CHANNELS - No Host ****************************************************************************************** 
     * ************************************************************************************************************* */
    
    
    [ IterationSetup( Target = nameof(Channels_WithoutHost_ReadWrite) ) ]
    public void Setup_Channels_WithoutHost_ReadWrite( ) {
        _dataChannel           = Channel.CreateUnbounded<ChannelMessage>();
        _dataChannelReader     = _dataChannel.Reader;
        _dataChannelWriter     = _dataChannel.Writer;
        _responseChannel       = Channel.CreateUnbounded<ChannelResponse>();
        _responseChannelReader = _responseChannel.Reader;
        _responseChannelWriter = _responseChannel.Writer;
    }

    [ IterationCleanup( Target = nameof(Channels_WithoutHost_ReadWrite) ) ]
    public void Cleanup_Channels_WithoutHost_ReadWrite( ) {
        _dataChannelWriter.Complete();
    }

    [ Benchmark ]
    public void Channels_WithoutHost_ReadWrite( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _dataChannelWriter.WaitToWriteAsync() ) {
                while ( _dataChannelWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
                    i++;
                    if ( i > MessageCount ) {
                        return;
                    }
                }
            }
        }


        async Task readerTask1( ) {
            while ( await _dataChannelReader.WaitToReadAsync() ) {
                while ( _dataChannelReader.TryRead( out ChannelMessage? result ) ) {
                    if ( result.Id >= MessageCount ) {
                        var response = new ChannelResponse( result.Id, nameof(readerTask1) );
                        // Console.WriteLine($"ReaderTask Complete @ {MessageCount} - Channels");
                        await _responseChannelWriter.WriteAsync( response );
                        return;
                    }
                }
            }
        }

        Task.WaitAll(
            Task.Run( readerTask1 ),
            Task.Run( writerTask )
        );

        // _responseChannelReader.TryRead( out ChannelResponse response );
        // Console.WriteLine($"Response={response}");
    }
}