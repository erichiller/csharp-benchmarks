using System;
using System.Net.Http;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;
using Benchmarks.Rpc.Shared;

using Grpc.Net.Client;

using JetBrains.Annotations;

using ProtoBuf.Grpc.Client;


namespace Benchmarks.Rpc;

[ Config( typeof(BenchmarkConfig) ) ]
public class ListIAsyncEnumerableComparison : IDisposable {
    [ UsedImplicitly ]
    // [ Params( 1, 10, 10_000, 100_000, 1_000_000 ) ]
    [ Params( 1, 10_000, 100_000 ) ]
    // [ Params( 1, 10 ) ]
    public int Count { get; set; }


    private HttpClientHandler _httpHandler = null!;
    private GrpcChannel       _channel     = null!;
    private bool              _disposed    = false;

    [ GlobalSetup ]
    public void Setup( ) {
        _httpHandler = new HttpClientHandler();
        // Return `true` to allow certificates that are untrusted/invalid
        _httpHandler.ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        _channel = GrpcChannel.ForAddress( "https://localhost:5002",
                                           new GrpcChannelOptions { HttpHandler = _httpHandler } );
    }

    [ GlobalCleanup ]
    public void Dispose( ) {
        if ( !_disposed ) {
            _httpHandler.Dispose();
        }
    }

    // [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber), nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_WriteEnumerable), nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ReadAllAsync), nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_WriteAsync) } ) ]
    // public void Setup_RunBroadcastQueueWithoutHostTest( ) {
    //     _broadcastQueue        = new BroadcastQueue<ChannelMessage, ChannelResponse>();
    //     _broadcastQueueReader1 = _broadcastQueue.GetReader();
    //     _broadcastQueueWriter  = _broadcastQueue.Writer;
    // }

    [ Benchmark ]
    [ BenchmarkCategory( "Sync", "Reply", "ReturnsSingle" ) ]
    public int Single( ) {
        var client = _channel.CreateGrpcService<ICounterService>();

        int received = 0;
        for ( int i = 0 ; i < Count ; i++ ) {
            received += client.IncrementCountSync().Count;
        }

        return received;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Async", "Reply", "ReturnsSingle", "Task" ) ]
    public async Task<int> SingleAsyncTask( ) {
        var client = _channel.CreateGrpcService<ICounterService>();

        int received = 0;
        for ( int i = 0 ; i < Count ; i++ ) {
            received += ( await client.IncrementCountAsync() ).Count;
        }

        return received;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Async", "Reply", "ReturnsSingle", "ValueTask" ) ]
    public async ValueTask<int> SingleAsyncValueTask( ) {
        var client = _channel.CreateGrpcService<ICounterService>();

        int received = 0;
        for ( int i = 0 ; i < Count ; i++ ) {
            received += ( await client.IncrementCountValueTaskAsync() ).Count;
        }

        return received;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Async", "Reply", "ReturnsMany", "Array", "Task" ) ]
    public async Task<int> ArrayReplyAsync( ) {
        var client = _channel.CreateGrpcService<ICounterService>();
        return ( await client.ServerToClientArrayAsync( new CounterRequest() { Count = Count } ) ).Length;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Async", "Reply", "ReturnsMany", "Array", "ValueTask" ) ]
    public async Task<int> ArrayReplyValueTaskAsync( ) {
        var client = _channel.CreateGrpcService<ICounterService>();
        return ( await client.ServerToClientArrayAsync( new CounterRequest() { Count = Count } ) ).Length;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Async", "Reply", "ReturnsMany", "List", "Task" ) ]
    public async Task<int> ListReplyAsync( ) {
        var client = _channel.CreateGrpcService<ICounterService>();
        return ( await client.ServerToClientListAsync( new CounterRequest() { Count = Count } ) ).Count;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Async", "Reply", "ReturnsMany", "List", "ValueTask" ) ]
    public async Task<int> ListReplyValueTaskAsync( ) {
        var client = _channel.CreateGrpcService<ICounterService>();
        return ( await client.ServerToClientListValueTaskAsync( new CounterRequest() { Count = Count } ) ).Count;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Sync", "Reply", "ReturnsMany", "List" ) ]
    public int ListReplySync( ) {
        var client = _channel.CreateGrpcService<ICounterService>();
        return client.ServerToClientListSync( new CounterRequest() { Count = Count } ).Count;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Sync", "Reply", "ReturnsMany", "IEnumerable" ) ]
    public int EnumerableReply( ) {
        var client   = _channel.CreateGrpcService<ICounterService>();
        int received = 0;
        foreach ( var reply in client.ServerToClientIEnumerable( new CounterRequest() { Count = Count } ) ) {
            received += reply.Count;
        }

        return received;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Async", "Reply", "ReturnsMany", "IAsyncEnumerable", "Task" ) ]
    public async Task<int> AsyncEnumerableReply( ) {
        var client   = _channel.CreateGrpcService<ICounterService>();
        int received = 0;
        await foreach ( var reply in client.ServerToClientIAsyncEnumerable( new CounterRequest() { Count = Count } ) ) {
            received += reply.Count;
        }

        return received;
    }
}