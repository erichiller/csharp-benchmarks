using System;
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

    [ IterationSetup( Targets = new[] {
        nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber), nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ReadAllAsync), nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_WriteAsync)
    } ) ]
    public void Setup_RunBroadcastQueueWithoutHostTest( ) {
        _broadcastQueue        = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueWriter  = _broadcastQueue.Writer;
    }

    [ IterationCleanup( Targets = new[] {
        nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ReadAllAsync),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_WriteAsync),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_NoLockWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_WriteAsync),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_NoLockWriter),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ConfigureAwaitFalse),
        nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_NoLockWriter),
    } ) ]
    public void Cleanup_RunBroadcastQueueWithoutHostTest( ) {
        _broadcastQueue.Writer.Complete();
    }

    [ Benchmark ]
    [ BenchmarkCategory( "OneSubscriber", "StandardBroadcastQueue" ) ]
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

        _broadcastQueueWriter.TryReadResponse( out ChannelResponse? response );
        if ( response?.ReadId != MessageCount ) {
            throw new Exception();
        }
        // Console.WriteLine($"Response={response}");
    }

    [ Benchmark ]
    [ BenchmarkCategory( "OneSubscriber", "BroadcastQueueAlt" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ReadAllAsync( ) {
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
            await foreach ( var result in _broadcastQueueReader1.ReadAllAsync() ) {
                if ( result.Id >= MessageCount ) {
                    var response = new ChannelResponse( result.Id, nameof(readerTask) );
                    await _broadcastQueueReader1.WriteResponseAsync( response );
                    return;
                }
            }
        }

        Task.WaitAll(
            Task.Run( readerTask ),
            Task.Run( writerTask )
        );
    }

    [ Benchmark ]
    [ BenchmarkCategory( "OneSubscriber", "BroadcastQueueAlt" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_WriteAsync( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( i <= MessageCount ) {
                await _broadcastQueueWriter.WriteAsync( new ChannelMessage() { Id = i } );
                i++;
            }
        }

        async Task readerTask( ) {
            while ( await _broadcastQueueReader1.WaitToReadAsync() ) {
                while ( _broadcastQueueReader1.TryRead( out ChannelMessage? result ) ) {
                    if ( result.Id >= MessageCount ) {
                        var response = new ChannelResponse( result.Id, nameof(readerTask) );
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
    }


    /* **** TWO **** */

    private BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader2;

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers), nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_WriteAsync), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers( ) {
        _broadcastQueue        = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueReader2 = _broadcastQueue.GetReader();
        _broadcastQueueWriter  = _broadcastQueue.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "TwoSubscribers", "StandardBroadcastQueue" ) ]
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
    }

    [ Benchmark ]
    public void BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_WriteAsync( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( i <= MessageCount ) {
                await _broadcastQueueWriter.WriteAsync( new ChannelMessage() { Id = i } );
                i++;
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

    private BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader3;

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers), nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ConfigureAwaitFalse), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers( ) {
        _broadcastQueue        = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueReader2 = _broadcastQueue.GetReader();
        _broadcastQueueReader3 = _broadcastQueue.GetReader();
        _broadcastQueueWriter  = _broadcastQueue.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ThreeSubscribers", "StandardBroadcastQueue" ) ]
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

    [ Benchmark ]
    public void BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ConfigureAwaitFalse( ) {
        async Task writerTask( ) {
            int i = 0;
            while ( await _broadcastQueueWriter.WaitToWriteAsync().ConfigureAwait( false ) ) {
                while ( _broadcastQueueWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
                    i++;
                    if ( i > MessageCount ) {
                        return;
                    }
                }
            }
        }

        async Task readerTask1( ) {
            while ( await _broadcastQueueReader1.WaitToReadAsync().ConfigureAwait( false ) ) {
                while ( _broadcastQueueReader1.TryRead( out ChannelMessage? result ) ) {
                    if ( result.Id >= MessageCount ) {
                        var response = new ChannelResponse( result.Id, nameof(readerTask1) );
                        // Console.WriteLine($"ReaderTask Complete @ {MessageCount} - BroadcastQueue");
                        await _broadcastQueueReader1.WriteResponseAsync( response ).ConfigureAwait( false );
                        return;
                    }
                }
            }
        }

        async Task readerTask2( ) {
            while ( await _broadcastQueueReader2.WaitToReadAsync().ConfigureAwait( false ) ) {
                while ( _broadcastQueueReader2.TryRead( out ChannelMessage? result ) ) {
                    if ( result.Id >= MessageCount ) {
                        var response = new ChannelResponse( result.Id, nameof(readerTask2) );
                        // Console.WriteLine($"ReaderTask Complete @ {MessageCount} - BroadcastQueue");
                        await _broadcastQueueReader2.WriteResponseAsync( response ).ConfigureAwait( false );
                        return;
                    }
                }
            }
        }

        async Task readerTask3( ) {
            while ( await _broadcastQueueReader3.WaitToReadAsync().ConfigureAwait( false ) ) {
                while ( _broadcastQueueReader3.TryRead( out ChannelMessage? result ) ) {
                    if ( result.Id >= MessageCount ) {
                        var response = new ChannelResponse( result.Id, nameof(readerTask3) );
                        // Console.WriteLine($"ReaderTask Complete @ {MessageCount} - BroadcastQueue");
                        await _broadcastQueueReader3.WriteResponseAsync( response ).ConfigureAwait( false );
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