using System;
using System.Threading;
using System.Threading.Tasks;

using Benchmarks.InterThread.BroadcastQueue;

namespace Benchmarks.InterThread.Benchmark;

public static class DeadlockTest {
    static async Task<int> writerTask( BroadcastQueueWriter<ChannelMessage, ChannelResponse> bqWriter, int messageCount, CancellationToken ct ) {
        int i = 0;
        while ( await bqWriter.WaitToWriteAsync( ct ) ) {
            while ( bqWriter.TryWrite( new ChannelMessage() { Id = i } ) ) {
                i++;
                if ( i > messageCount ) {
                    Console.WriteLine($"[BroadcastQueueWriter] wrote messageCount: {i}");
                    return i;
                }
            }
        }


        // while ( await broadcastQueue.Writer.WaitToReadResponseAsync( ct ) ) {
        //     if ( broadcastQueue.Writer.TryReadResponse( out var result ) ) {
        //         // logger.LogDebug( "Read {Id} from ResponseChannel", result?.ReadId );
        //         if ( result is { ReadId: int readId } ) {
        //             lastReadId = readId;
        //             // iterationCounter++;
        //             if ( readId % messageCount == 0 ) {
        //                 break;
        //             }
        //         }
        //     }
        // }
        //
        // return lastReadId;
        return -1;
    }


    static async Task readerTask( BroadcastQueueReader<ChannelMessage, ChannelResponse> bqReader, int messageCount, string taskName, CancellationToken ct ) {
        int lastMessage = -1;
        Console.WriteLine($"[BroadcastQueueReader] start");
        while ( await bqReader.WaitToReadAsync( ct ) ) {
            Console.WriteLine($"[BroadcastQueueReader] start receiving");
            while ( bqReader.TryRead( out ChannelMessage? result ) ) {
                Console.WriteLine($"[BroadcastQueueReader] received messageCount: {result.Id}");
                if ( result.Id != lastMessage + 1 ) {
                    Console.WriteLine($"[BroadcastQueueReader] ERROR: {result.Id}");
                    await bqReader.WriteResponseAsync( new ChannelResponse( result.Id, taskName, new Exception( "Unexpected sequence" ) ), ct );
                    throw new Exception(); // TODO: Assert.False?
                }


                if ( result.Id >= messageCount ) {
                    await bqReader.WriteResponseAsync( new ChannelResponse( result.Id, taskName ), ct );
                    return;
                }
                lastMessage++;

            }
        }

        await bqReader.WriteResponseAsync( new ChannelResponse( -1, taskName, new Exception( "Incomplete sequence" ) ), ct );
    }

    public static void Do( int messageCount ) {
        var broadcastQueue = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        var cts            = new CancellationTokenSource();

        var readerTask1 = readerTask( broadcastQueue.GetReader(), messageCount, "readerTask1", cts.Token );
        var readerTask2 = readerTask( broadcastQueue.GetReader(), messageCount, "readerTask2", cts.Token );
        var readerTask3 = readerTask( broadcastQueue.GetReader(), messageCount, "readerTask3", cts.Token );
        var writerTask1 = writerTask( broadcastQueue.Writer, messageCount, cts.Token );


        Task.WaitAll(
            readerTask1,
            readerTask2,
            readerTask3,
            writerTask1
        );
    }
}