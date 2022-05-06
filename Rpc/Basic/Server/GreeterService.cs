using System.Threading.Tasks;

using Benchmarks.Rpc.Basic.Definitions;

using ProtoBuf.Grpc;

namespace Benchmarks.Rpc.Basic.Server;

public class GreeterService : IGreeterService
{
    public Task<HelloReply> SayHelloAsync(HelloRequest request, CallContext context = default)
    {
        return Task.FromResult(
            new HelloReply
            {
                Message = $"Hello {request.Name}"
            });
    }
}