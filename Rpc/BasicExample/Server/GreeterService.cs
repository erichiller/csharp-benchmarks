using System.Threading.Tasks;

using Benchmarks.Rpc.BasicExample.Definitions;

using ProtoBuf.Grpc;

namespace Benchmarks.Rpc.BasicExample.Server;

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