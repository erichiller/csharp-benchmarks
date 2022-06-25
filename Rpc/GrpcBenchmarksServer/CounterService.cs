using System;
using System.Collections.Generic;

using ProtoBuf.Grpc;

using System.Threading.Tasks;

using Benchmarks.Rpc.Shared;

using Grpc.Core;

using Microsoft.Extensions.Logging;

namespace Benchmarks.Rpc.GrpcBenchmarksServer;

public class CounterService : ICounterService {
    private readonly ILogger                     _logger;
    private readonly IncrementingCounter         _counter;

    public CounterService( IncrementingCounter counter, ILoggerFactory loggerFactory ) {
        _counter       = counter;
        _logger        = loggerFactory.CreateLogger<CounterService>();
    }

    public CounterReply IncrementCountSync( CallContext context ) {
        _logger.LogInformation( "Incrementing count by 1" );
        _counter.Increment( 1 );

        return new CounterReply { Count = _counter.Count };
    }
    
    public Task<CounterReply> IncrementCountAsync( CallContext context ) {
        _logger.LogInformation( "Incrementing count by 1" );
        _counter.Increment( 1 );

        return Task.FromResult( new CounterReply { Count = _counter.Count } );
    }
    public ValueTask<CounterReply> IncrementCountValueTaskAsync( CallContext context ) {
        _logger.LogInformation( "Incrementing count by 1" );
        _counter.Increment( 1 );

        return ValueTask.FromResult( new CounterReply { Count = _counter.Count } );
    }

    /* *********************************************************** */

    // public async Task<CounterReply> AccumulateCount( IAsyncEnumerable<CounterRequest> requestStream, CallContext context ) {
    //     var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new GrpcCallContextException();
    //     _logger.LogInformation( "Connection id: {ConnectionId}", httpContext.Connection.Id );
    //     await foreach ( var message in requestStream ) {
    //         _logger.LogInformation( "Incrementing count by {MessageCount}", message.Count );
    //
    //         _counter.Increment( message.Count );
    //     }
    //
    //     return new CounterReply { Count = _counter.Count };
    // }
    //
    // public Task<CounterReply> ClientToServerStream( IAsyncEnumerable<CounterRequest> requestStream, CallContext context ) {
    //     var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new GrpcCallContextException();
    //     _logger.LogInformation( "Connection id: {ConnectionId}", httpContext.Connection.Id );
    //
    //     return AccumulateCount( requestStream, context.CancellationToken );
    // }

    /* *********************************************************** */

    
    public ValueTask<CounterReply[]> ServerToClientArrayValueTaskAsync( CounterRequest request, CallContext context ) {
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new GrpcCallContextException();
        _logger.LogInformation( "Connection id: {ConnectionId}", httpContext.Connection.Id );

        var random = new Random();
        var arr    = new CounterReply[request.Count];
        int i      = -1;
        while ( i++ < ( request.Count - 1) && !context.CancellationToken.IsCancellationRequested ) {
            arr[i] = new CounterReply { Count = random.Next( 1, 10 ) };
        }

        return ValueTask.FromResult( arr );
    }
    
    public Task<CounterReply[]> ServerToClientArrayAsync( CounterRequest request, CallContext context ) {
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new GrpcCallContextException();
        _logger.LogInformation( "Connection id: {ConnectionId}", httpContext.Connection.Id );

        var random = new Random();
        var arr   = new CounterReply[request.Count];
        int i      = -1;
        while ( i++ < ( request.Count - 1) && !context.CancellationToken.IsCancellationRequested ) {
            arr[i] = new CounterReply { Count = random.Next( 1, 10 ) };
        }

        return Task.FromResult( arr );
    }

    public Task<List<CounterReply>> ServerToClientListAsync( CounterRequest request, CallContext context ) {
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new GrpcCallContextException();
        _logger.LogInformation( "Connection id: {ConnectionId}", httpContext.Connection.Id );

        var random = new Random();
        var list   = new List<CounterReply>();
        int i      = 0;
        while ( i++ <= request.Count && !context.CancellationToken.IsCancellationRequested ) {
            list.Add( new CounterReply { Count = random.Next( 1, 10 ) } );
        }

        return Task.FromResult( list );
    }

    public IEnumerable<CounterReply> ServerToClientIEnumerable( CounterRequest request, CallContext context ) {
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new GrpcCallContextException();
        _logger.LogInformation( "Connection id: {ConnectionId}", httpContext.Connection.Id );

        var random = new Random();
        int i      = 0;
        while ( i++ <= request.Count && !context.CancellationToken.IsCancellationRequested ) {
            yield return new CounterReply { Count = random.Next( 1, 10 ) };
        }
    }

    public async IAsyncEnumerable<CounterReply> ServerToClientIAsyncEnumerable( CounterRequest request, CallContext context ) {
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new GrpcCallContextException();
        _logger.LogInformation( "Connection id: {ConnectionId}", httpContext.Connection.Id );

        var random = new Random();
        int i      = 0;
        while ( i++ <= request.Count && !context.CancellationToken.IsCancellationRequested ) {
            yield return new CounterReply { Count = random.Next( 1, 10 ) };
        }
    }


    /*
     * Bidirectional Streaming
     * IAsyncEnumerable
     * https://github.com/protobuf-net/protobuf-net.Grpc/issues/234
     */
    // public IAsyncEnumerable<CounterReply> BidirectionalStream( IAsyncEnumerable<CounterRequest> requestStream, CallContext context ) {
    //     var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new GrpcCallContextException();
    //     _logger.LogInformation( "Connection id: {ConnectionId}", httpContext.Connection.Id );
    //
    //     throw new NotImplementedException(); // TODO
    // }
}

public class GrpcCallContextException : Exception { }