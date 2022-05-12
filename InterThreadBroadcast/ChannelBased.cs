using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Benchmarks.InterThreadBroadcast;

public record ChannelMessage {
    public int    Id         { get; set; }
    public string Property_1 { get; set; }
}

public record ChannelResponse {
    public int    ReadId         { get; set; }
}

public class ChannelPublisher : BackgroundService {
    private readonly System.Threading.Channels.ChannelWriter<ChannelMessage> _channelWriter;
    
    private readonly ILogger<ChannelPublisher> _logger;

    public ChannelPublisher( ILogger<ChannelPublisher> logger, Channel<ChannelMessage> channel ) =>
        ( _logger, _channelWriter ) = ( NullLogger<ChannelPublisher>.Instance, channel.Writer );
    // ( _logger, _channelWriter ) = ( logger, channel.Writer );

    public override async Task StartAsync( CancellationToken cancellationToken ) {
        _logger.LogInformation( "Starting client streaming call at: {time}", DateTimeOffset.Now );
        // Don't pass cancellation token to the call. The call is completed in StopAsync when service stops.
        await base.StartAsync( cancellationToken );
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
        _logger.LogDebug($"Starting to WriteChannel");
        int id = 0;
        // Count until the worker exits
        while ( await _channelWriter.WaitToWriteAsync( stoppingToken ) ) {
            // for ( int i = 0 ; i < WriteCount ; i++ ) {
                // _logger.LogInformation( "Adding to Channel count {count} at: {time}", count, DateTimeOffset.Now );
                _logger.LogDebug( $"Adding to Channel count {id} at: {DateTimeOffset.Now}" );
                var message = new ChannelMessage { Id = id, Property_1 = "some string" };
                _channelWriter.TryWrite( message );

                // await Task.Delay( DELAY_MS, stoppingToken );
                id++;
            // }

            _logger.LogDebug( $"Completed Writing. WaitToWriteAsync: {DateTimeOffset.Now}" );
            // break;
        }
    }

    public override async Task StopAsync( CancellationToken cancellationToken ) {
        // Debug.Assert( _clientStreamingCall != null );

        _channelWriter.TryComplete();

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
public class ChannelSubscriber : BackgroundService {
    private readonly ILogger<ChannelSubscriber> _logger = NullLogger<ChannelSubscriber>.Instance;

    private readonly System.Threading.Channels.ChannelReader<ChannelMessage> _channelReader;
    private readonly System.Threading.Channels.ChannelWriter<ChannelResponse> _responseChannelWriter;

    public ChannelSubscriber( Channel<ChannelMessage> channel, Channel<ChannelResponse> responseChannel ) =>
        ( _channelReader, _responseChannelWriter ) = ( channel.Reader, responseChannel.Writer );

    public override async Task StartAsync( CancellationToken cancellationToken ) {
        _logger.LogInformation( "Starting client streaming call at: {time}", DateTimeOffset.Now );
        // Don't pass cancellation token to the call. The call is completed in StopAsync when service stops.
        await base.StartAsync( cancellationToken );
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
        _logger.LogDebug( $"Starting to ReadChannel" );
        while ( !stoppingToken.IsCancellationRequested ) {
            _logger.LogDebug($"{nameof(ChannelSubscriber)} beginning to {nameof(ChannelReader<ChannelResponse>.ReadAllAsync)}");
            var read = _channelReader.ReadAllAsync( stoppingToken );
            
            _logger.LogDebug($"{nameof(ChannelSubscriber)} completed {nameof(ChannelReader<ChannelResponse>.ReadAllAsync)}");
            await foreach ( var message in read ) {
                _logger.LogDebug( $"Read message with id: { message.Id }" );
                await _responseChannelWriter.WriteAsync( new ChannelResponse() { ReadId = message.Id }, stoppingToken );
                _logger.LogDebug( $"Wrote {nameof(ChannelResponse)} { message.Id }" );
            }

            // _logger.LogInformation( "Total count: {count}", response.Count );
            // _logger.LogDebug( $"Total count: { response.Id }" );

            _logger.LogDebug("loop over");
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