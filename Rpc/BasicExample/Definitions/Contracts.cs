using System;
using System.Collections.Generic;

using ProtoBuf.Grpc;

using System.Runtime.Serialization;
using System.ServiceModel;
using System.Threading.Tasks;

namespace Benchmarks.Rpc.BasicExample.Definitions;

[ DataContract ]
public class HelloReply {
    [ DataMember( Order = 1 ) ] public string Message { get; set; }
}

[ DataContract ]
public class HelloRequest {
    [ DataMember( Order = 1 ) ] public string Name { get; set; }
}

[ ServiceContract ]
public interface IGreeterService {
    [ OperationContract ]
    Task<HelloReply> SayHelloAsync(
        HelloRequest request,
        CallContext  context = default
    );
}

/* worker service */

[ ServiceContract ]
public interface ICounterService {
    [ OperationContract ]
    Task<CounterReply> IncrementCount( CallContext context = default );

    [ OperationContract ]
    Task<CounterReply> AccumulateCount( IAsyncEnumerable<CounterRequest> request, CallContext context = default );
}

[ DataContract ]
public class CounterReply {
    [ DataMember( Order = 1 ) ] public Int32 Count { get; set; } = 1;
}

[ DataContract ]
public class CounterRequest {
    [ DataMember( Order = 1 ) ] public Int32 Count { get; set; } = 1;
}