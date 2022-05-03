// See https://aka.ms/new-console-template for more information

using System;
using System.Net.Http;
using System.Threading.Tasks;

using Grpc.Net.Client;
using ProtoBuf.Grpc.Client;
using Benchmarks.Rpc.GrpcDefinitions;


namespace Benchmarks.Rpc.Client;


internal class Program
{
    private static async Task Main(string[] args) {
        
        var httpHandler = new HttpClientHandler();
// Return `true` to allow certificates that are untrusted/invalid
        httpHandler.ServerCertificateCustomValidationCallback = 
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;

        var channel = GrpcChannel.ForAddress("https://localhost:5001",
                                             new GrpcChannelOptions { HttpHandler = httpHandler });
        // var client = new Greet.GreeterClient(channel);
        //
        //
        // // GrpcClientFactory.AllowUnencryptedHttp2 = true;
        // using var channel = GrpcChannel.ForAddress("https://localhost:7184");
        var       client  = channel.CreateGrpcService<IGreeterService>();

        var reply = await client.SayHelloAsync(
            new HelloRequest { Name = "GreeterClient" });

        Console.WriteLine($"Greeting: {reply.Message}");
        Console.WriteLine("Press any key to exit...");
        Console.ReadKey();
    }
}