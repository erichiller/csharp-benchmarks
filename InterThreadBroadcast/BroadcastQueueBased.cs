using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Benchmarks.InterThreadBroadcast; 


public class BroadcastPublisher : BackgroundService {
    private readonly BroadcastQueue<ChannelMessage, ChannelResponse> _broadcastQueue;
    
    private readonly ILogger<BroadcastPublisher> _logger;

    public BroadcastPublisher( ILogger<BroadcastPublisher> logger, BroadcastQueue<ChannelMessage, ChannelResponse> broadcastQueue ) =>
        // ( _logger, _broadcastQueue ) = ( NullLogger<BroadcastPublisher>.Instance, broadcastQueue );
    ( _logger, _broadcastQueue ) = ( logger, broadcastQueue );

    public override async Task StartAsync( CancellationToken cancellationToken ) {
        _logger.LogInformation( "Starting client streaming call at: {time}", DateTimeOffset.Now );
        // Don't pass cancellation token to the call. The call is completed in StopAsync when service stops.
        await base.StartAsync( cancellationToken );
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
        _logger.LogDebug($"Starting to WriteChannel");
        int id = 0;
        // Count until the worker exits
        while ( ! stoppingToken.IsCancellationRequested ) {
            // for ( int i = 0 ; i < WriteCount ; i++ ) {
            // _logger.LogInformation( "Adding to Channel count {count} at: {time}", count, DateTimeOffset.Now );
            _logger.LogDebug( $"Adding to Channel count {id} at: {DateTimeOffset.Now}" );
            var message = new ChannelMessage { Id = id, Property_1 = "some string" };
            // _channelWriter.TryWrite( message );
            _broadcastQueue.Write( message );

            await Task.Delay( 2000, stoppingToken );
            id++;
            // }

            _logger.LogDebug( $"Completed Writing. WaitToWriteAsync: {DateTimeOffset.Now}" );
            // break;
        }
    }

    public override async Task StopAsync( CancellationToken cancellationToken ) {
        // Debug.Assert( _clientStreamingCall != null );

        // _channelWriter.TryComplete();

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



// Client Streaming example
public class BroadcastSubscriber : BackgroundService {
    private readonly ILogger<BroadcastSubscriber>                     _logger = NullLogger<BroadcastSubscriber>.Instance;
    private readonly BroadcastQueue<ChannelMessage, ChannelResponse>       _broadcastQueue;
    private readonly BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader;

    public BroadcastSubscriber( ILogger<BroadcastSubscriber> logger, BroadcastQueue<ChannelMessage, ChannelResponse> broadcastQueue ) =>
        ( _logger, _broadcastQueue, _broadcastQueueReader ) = ( NullLogger<BroadcastSubscriber>.Instance, broadcastQueue, broadcastQueue.GetReader() );

    public override async Task StartAsync( CancellationToken cancellationToken ) {
        _logger.LogInformation( "Starting client streaming call at: {time}", DateTimeOffset.Now );
        // Don't pass cancellation token to the call. The call is completed in StopAsync when service stops.
        await base.StartAsync( cancellationToken );
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
        _logger.LogDebug( $"Starting to ReadChannel" );
        while ( !stoppingToken.IsCancellationRequested ) {
            // var read = _channelReader.ReadAllAsync( stoppingToken );
            var read = _broadcastQueueReader.ReadAll();
            foreach ( var message in read ) {
                var response = new ChannelResponse() { ReadId = message.Id };
                // await _responseChannelWriter.WriteAsync( new ChannelResponse() { ReadId = message.Id }, stoppingToken );
                _broadcastQueueReader.WriteResponse( response );
            }

            // _logger.LogInformation( "Total count: {count}", response.Count );
            // _logger.LogDebug( $"Total count: { response.Id }" );

            _logger.LogDebug( "loop over" );
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
