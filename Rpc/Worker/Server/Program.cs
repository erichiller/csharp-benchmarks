using System.Threading.Channels;

using Benchmarks.Rpc.Worker.Shared;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ProtoBuf.Grpc.Server;

namespace Benchmarks.Rpc.Worker.Server;

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

/*
 * URGENT:
 * Test a server to client streaming service
 * On the server one worker process should be filling a queue (eg. Data from API)
 * and the queue should be used by the gRPC Service handler to send ( data to receiver )
 */
public class Startup {
    public void ConfigureServices( IServiceCollection services ) {
        
        
        services.AddSingleton<Channel<CounterReply>>( s => Channel.CreateBounded<CounterReply>( 5 ));
        services.AddHostedService<ReceiveRemoteDataWorker>();
        
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