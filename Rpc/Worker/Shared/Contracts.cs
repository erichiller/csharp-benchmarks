using System;
using System.Collections.Generic;

using ProtoBuf.Grpc;

using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading;
using System.Threading.Tasks;

namespace Benchmarks.Rpc.Worker.Shared;

/* worker service */

[ ServiceContract ]
public interface ICounterService {
    [ OperationContract ]
    Task<CounterReply> IncrementCount( CallContext context = default );

    [ OperationContract ]
    Task<CounterReply> AccumulateCount( IAsyncEnumerable<CounterRequest> request, CallContext context = default );
    
    /// <summary>
    /// Client sending to server
    /// </summary>
    [ OperationContract ]
    Task<CounterReply> ClientToServerStream( IAsyncEnumerable<CounterRequest> data, CallContext context = default );
    
    /// <summary>
    /// Server sending to client
    /// </summary>
    [ OperationContract ]
    IAsyncEnumerable<CounterReply> ServerToClientStream( CounterRequest data, CallContext context = default );
    
    /// <summary>
    /// Server sending to client and vice versa
    /// </summary>
    [ OperationContract ]
    IAsyncEnumerable<CounterReply> BidirectionalStream( IAsyncEnumerable<CounterRequest> data, CallContext context = default );

}

[ DataContract ]
public class CounterReply {
    [ DataMember( Order = 1 ) ] public Int32 Count { get; set; } = 1;
}

[ DataContract ]
public record CounterRequest {
    [ DataMember( Order = 1 ) ] public Int32 Count { get; init; } = 1;
}