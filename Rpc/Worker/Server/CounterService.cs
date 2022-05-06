using System;
using System.Collections.Generic;
using System.Threading;

using ProtoBuf.Grpc;

using System.Threading.Tasks;

using Benchmarks.Rpc.Worker.Definitions;

using Grpc.Core;

using Microsoft.Extensions.Logging;

using ProtoBuf.Grpc.Internal;

namespace Benchmarks.Rpc.Worker.Server;

public class CounterService : ICounterService {
    private readonly ILogger             _logger;
    private readonly IncrementingCounter _counter;

    public CounterService( IncrementingCounter counter, ILoggerFactory loggerFactory ) {
        _counter = counter;
        _logger  = loggerFactory.CreateLogger<CounterService>();
    }

    // public override Task<CounterReply> IncrementCount(Empty request, ServerCallContext context)
    public Task<CounterReply> IncrementCount( CallContext context ) {
        _logger.LogInformation( "Incrementing count by 1" );
        _counter.Increment( 1 );

        return Task.FromResult( new CounterReply { Count = _counter.Count } );
    }


    /*
     * Bidirectional Streaming
     * IAsyncEnumerable
     * https://github.com/protobuf-net/protobuf-net.Grpc/issues/234
     */
    public async Task<CounterReply> AccumulateCount( IAsyncEnumerable<CounterRequest> requestStream, CallContext context ) {
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new GrpcCallContextException();
        _logger.LogInformation( $"Connection id: {httpContext.Connection.Id}" );
        await foreach ( var message in requestStream ) {
            _logger.LogInformation( $"Incrementing count by {message.Count}" );

            _counter.Increment( message.Count );
        }

        return new CounterReply { Count = _counter.Count };
    }
    
    public Task<CounterReply> ClientToServerStream( IAsyncEnumerable<CounterRequest> requestStream, CallContext context ) {
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new GrpcCallContextException();
        _logger.LogInformation( $"Connection id: {httpContext.Connection.Id}" );

        return AccumulateCount( requestStream, context.CancellationToken );
    }

    public IAsyncEnumerable<CounterReply> ServerToClientStream( CounterRequest requestStream, CallContext context ) {
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new GrpcCallContextException();
        _logger.LogInformation( $"Connection id: {httpContext.Connection.Id}" );

        throw new NotImplementedException(); // URGENT
    }
    
    /*
     * Bidirectional Streaming
     * IAsyncEnumerable
     * https://github.com/protobuf-net/protobuf-net.Grpc/issues/234
     */
    public IAsyncEnumerable<CounterReply> BidirectionalStream( IAsyncEnumerable<CounterRequest> requestStream, CallContext context ) {
        var httpContext = context.ServerCallContext?.GetHttpContext() ?? throw new GrpcCallContextException();
        _logger.LogInformation( $"Connection id: {httpContext.Connection.Id}" );
        
        throw new NotImplementedException(); // TODO
    }
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
}

public class GrpcCallContextException : Exception { }