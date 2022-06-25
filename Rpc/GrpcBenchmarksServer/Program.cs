using System.Threading.Channels;

using Benchmarks.Rpc.Shared;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ProtoBuf.Grpc.Server;

namespace Benchmarks.Rpc.GrpcBenchmarksServer;

public class Program {

    public static void Main( string[] args ) {
        CreateHostBuilder( args )
            .Build()
            .Run();
    }

    public static IHostBuilder CreateHostBuilder( string[] args ) =>
        Host.CreateDefaultBuilder( args )
            .ConfigureWebHostDefaults( webBuilder => {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseUrls( @"https://*:5002" );
                // webBuilder.MapGet( "/", ( ) => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909" );
            } );
}

public class Startup {
    public void ConfigureServices( IServiceCollection services ) {
        services.AddCodeFirstGrpc();
        services.AddSingleton<IncrementingCounter>();
    }

    public void Configure( IApplicationBuilder app, IWebHostEnvironment env ) {
        if ( env.IsDevelopment() ) {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseEndpoints( endpoints => {
            endpoints.MapGrpcService<CounterService>();
            endpoints.MapGet( "/", ( ) => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909" );
        } );
    }
}