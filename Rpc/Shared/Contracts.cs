using System;
using System.Collections.Generic;

using ProtoBuf.Grpc;

using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmarks.Rpc.Shared;

/* worker service */

[ ServiceContract ]
public interface ICounterService {
    [ OperationContract ]
    CounterReply IncrementCountSync( CallContext context = default );
    
    [ OperationContract ]
    Task<CounterReply> IncrementCountAsync( CallContext context = default );
    
    [ OperationContract ]
    ValueTask<CounterReply> IncrementCountValueTaskAsync( CallContext context = default );

    // [ OperationContract ]
    // Task<CounterReply> AccumulateCount( IAsyncEnumerable<CounterRequest> request, CallContext context = default );
    //
    // /// <summary>
    // /// Client sending to server
    // /// </summary>
    // [ OperationContract ]
    // Task<CounterReply> ClientToServerStream( IAsyncEnumerable<CounterRequest> data, CallContext context = default );

    /// <summary>
    /// Server sending to client
    /// </summary>
    [ OperationContract ]
    public Task<CounterReply[]> ServerToClientArrayAsync( CounterRequest request, CallContext context = default );
    
    /// <summary>
    /// Server sending to client
    /// </summary>
    [ OperationContract ]
    public ValueTask<CounterReply[]> ServerToClientArrayValueTaskAsync( CounterRequest request, CallContext context = default );

    /// <summary>
    /// Server sending to client
    /// </summary>
    [ OperationContract ]
    public Task<List<CounterReply>> ServerToClientListAsync( CounterRequest request, CallContext context = default );

    /// <summary>
    /// Server sending to client
    /// </summary>
    [ OperationContract ]
    public ValueTask<List<CounterReply>> ServerToClientListValueTaskAsync( CounterRequest request, CallContext context = default );

    /// <summary>
    /// Server sending to client
    /// </summary>
    [ OperationContract ]
    public List<CounterReply> ServerToClientListSync( CounterRequest request, CallContext context = default );
    
    /// <summary>
    /// Server sending to client
    /// </summary>
    [ OperationContract ]
    IEnumerable<CounterReply> ServerToClientIEnumerable( CounterRequest request, CallContext context = default );
    
    /// <summary>
    /// Server sending to client
    /// </summary>
    [ OperationContract ]
    IAsyncEnumerable<CounterReply> ServerToClientIAsyncEnumerable( CounterRequest request, CallContext context = default );
    
    // /// <summary>
    // /// Server sending to client and vice versa
    // /// </summary>
    // [ OperationContract ]
    // IAsyncEnumerable<CounterReply> BidirectionalStream( IAsyncEnumerable<CounterRequest> data, CallContext context = default );

}

[ DataContract ]
public class CounterReply {
    [ DataMember( Order = 1 ) ] public Int32 Count { get; set; } = 1;
}

[ DataContract ]
public record CounterRequest {
    [ DataMember( Order = 1 ) ] public Int32 Count { get; init; } = 1;
}