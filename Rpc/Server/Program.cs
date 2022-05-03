using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

using ProtoBuf.Grpc.Server;

namespace Benchmarks.Rpc.Server;

public class Program {

    public static int Main( string[] args ) {
        
        var builder = WebApplication.CreateBuilder(args);
        
        

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
        builder.Services.AddCodeFirstGrpc();
        builder.WebHost.UseUrls( @"https://*:5001" );
               // .UseKestrel( opt => {
               //     opt.g
               // });

        var app = builder.Build();

// Configure the HTTP request pipeline.
        app.MapGrpcService<GreeterService>();
        app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
        return 0;
    }
}
