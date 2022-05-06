// See https://aka.ms/new-console-template for more information

using System;
using System.Net.Http;
using System.Net.Security;
using System.Threading;

using ProtoBuf.Grpc.ClientFactory;

using Benchmarks.Rpc.Worker.Definitions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace Benchmarks.Rpc.Worker.Client;

internal class Program {
    public static void Main( string[] args ) {
        CreateHostBuilder( args ).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder( string[] args ) =>
        Host.CreateDefaultBuilder( args )
            .ConfigureServices( ( hostContext, services ) => {
                services.AddHostedService<GrpcWorker>();
                services.AddCodeFirstGrpcClient<ICounterService>( clientFactoryOptions => {
                    clientFactoryOptions.Address = new Uri( "https://localhost:5002" );
                    clientFactoryOptions.ChannelOptionsActions.Add( channelOptions => {
                        channelOptions.HttpHandler = new SocketsHttpHandler() {
                            // keeps connection alive
                            PooledConnectionIdleTimeout = Timeout.InfiniteTimeSpan,
                            KeepAlivePingDelay          = TimeSpan.FromSeconds( 60 ),
                            KeepAlivePingTimeout        = TimeSpan.FromSeconds( 30 ),
                            SslOptions = new SslClientAuthenticationOptions {
                                RemoteCertificateValidationCallback = delegate { return true; }
                            },
                            // allows channel to add additional HTTP/2 connections
                            EnableMultipleHttp2Connections = true
                        };
                    } );
                } );
            } );
}