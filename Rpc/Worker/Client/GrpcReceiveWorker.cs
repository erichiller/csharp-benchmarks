using System;
using System.Threading;
using System.Threading.Tasks;

using Benchmarks.Rpc.Worker.Shared;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Benchmarks.Rpc.Worker.Client;

// Client Streaming example
public class GrpcReceiveWorker : BackgroundService {
    private readonly ILogger<GrpcReceiveWorker>                               _logger;
    private readonly ICounterService                                   _counterService;

    public GrpcReceiveWorker( ILogger<GrpcReceiveWorker> logger, ICounterService counterService ) {
        _logger         = logger;
        _counterService = counterService;
    }

    public override async Task StartAsync( CancellationToken cancellationToken ) {
        _logger.LogInformation( "Starting client streaming call at: {time}", DateTimeOffset.Now );
        // Don't pass cancellation token to the call. The call is completed in StopAsync when service stops.
        await base.StartAsync( cancellationToken );
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
        // Count until the worker exits
        while ( !stoppingToken.IsCancellationRequested ) {
            
            
            await foreach ( var message in _counterService.ServerToClientStream( new CounterRequest() ) ) {
                _logger.LogInformation( $"Incrementing count by {message.Count}" );
            }
            
            await Task.Delay( 1000, stoppingToken );
        }
    }

    public override async Task StopAsync( CancellationToken cancellationToken ) {
        // Debug.Assert( _clientStreamingCall != null );

        // Tell server that the client stream has finished
        _logger.LogInformation( "Finishing call at: {time}", DateTimeOffset.Now );
        // await _clientStreamingCall.RequestStream.CompleteAsync();
        // _counterService.

        // Log total
        // var response = await _clientStreamingCall;
        // _logger.LogInformation( "Total count: {count}", response.Count );

        await base.StopAsync( cancellationToken );
    }


}