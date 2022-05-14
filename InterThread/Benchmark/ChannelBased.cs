#define DEBUG
// #undef DEBUG

using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Benchmarks.InterThread.Benchmark;

public record ChannelMessage {
    public int    Id         { get; set; }
    public string Property_1 { get; set; }
}

public record ChannelResponse {
    public int ReadId { get; set; }
}

public class ChannelPublisher : BackgroundService {
    private readonly System.Threading.Channels.ChannelWriter<ChannelMessage> _channelWriter;

    private readonly ILogger<ChannelPublisher> _logger;

    public ChannelPublisher( ILogger<ChannelPublisher> logger, Channel<ChannelMessage> channel ) =>
        // ( _logger, _channelWriter ) = ( NullLogger<ChannelPublisher>.Instance, channel.Writer );
        ( _logger, _channelWriter ) = ( logger, channel.Writer );

    public override async Task StartAsync( CancellationToken cancellationToken ) {
#if DEBUG
        _logger.LogInformation( "[Thread ID: {ThreadID}] Starting client streaming call at: {time}", Thread.CurrentThread.ManagedThreadId, DateTimeOffset.Now );
#endif
        // Don't pass cancellation token to the call. The call is completed in StopAsync when service stops.
        await base.StartAsync( cancellationToken );
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
#if DEBUG
        _logger.LogDebug( "[Thread ID: {ThreadID}] Starting to WriteChannel", Thread.CurrentThread.ManagedThreadId );
#endif
        int id = 0;
        // while ( !stoppingToken.IsCancellationRequested ){ }
        // Count until the worker exits
        while ( await _channelWriter.WaitToWriteAsync( stoppingToken ) ) {
            // for ( int i = 0 ; i < WriteCount ; i++ ) {
            // _logger.LogInformation( "Adding to Channel count {count} at: {time}", count, DateTimeOffset.Now );
#if DEBUG
            _logger.LogDebug( $"[Thread ID: {Thread.CurrentThread.ManagedThreadId}] Adding to Channel count {id} at: {DateTimeOffset.Now}" );
#endif
            var message = new ChannelMessage { Id = id, Property_1 = "some string" };
            _channelWriter.TryWrite( message );

            // await Task.Delay( DELAY_MS, stoppingToken );
            id++;
            // }

#if DEBUG
            _logger.LogDebug( $"Completed Writing. WaitToWriteAsync: {DateTimeOffset.Now}" );
#endif
            // break;
        }
    }

    public override async Task StopAsync( CancellationToken cancellationToken ) {
        // Debug.Assert( _clientStreamingCall != null );

        _channelWriter.TryComplete();

        // Tell server that the client stream has finished
#if DEBUG
        _logger.LogInformation( "Finishing call at: {time}", DateTimeOffset.Now );
#endif
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
    private readonly ILogger<ChannelSubscriber> _logger;

    private readonly System.Threading.Channels.ChannelReader<ChannelMessage>  _channelReader;
    private readonly System.Threading.Channels.ChannelWriter<ChannelResponse> _responseChannelWriter;

    public ChannelSubscriber( ILogger<ChannelSubscriber> logger, Channel<ChannelMessage> channel, Channel<ChannelResponse> responseChannel ) =>
        ( _logger, _channelReader, _responseChannelWriter ) = ( logger, channel.Reader, responseChannel.Writer );

    public override async Task StartAsync( CancellationToken cancellationToken ) {
#if DEBUG
        _logger.LogInformation( "Starting client streaming call at: {time}", DateTimeOffset.Now );
#endif
        // Don't pass cancellation token to the call. The call is completed in StopAsync when service stops.
        await base.StartAsync( cancellationToken );
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
#if DEBUG
        _logger.LogDebug( $"Starting to ReadChannel" );
#endif
        while ( !stoppingToken.IsCancellationRequested ) {
#if DEBUG
            _logger.LogDebug( $"{nameof(ChannelSubscriber)} beginning to {nameof(ChannelReader<ChannelResponse>.ReadAllAsync)}" );
#endif
            var read = _channelReader.ReadAllAsync( stoppingToken );

#if DEBUG
            _logger.LogDebug( $"{nameof(ChannelSubscriber)} completed {nameof(ChannelReader<ChannelResponse>.ReadAllAsync)}" );
#endif
            await foreach ( var message in read ) {
#if DEBUG
                _logger.LogDebug( $"Read message with id: {message.Id}" );
#endif
                await _responseChannelWriter.WriteAsync( new ChannelResponse() { ReadId = message.Id }, stoppingToken );
#if DEBUG
                _logger.LogDebug( $"Wrote {nameof(ChannelResponse)} {message.Id}" );
#endif
            }

            // _logger.LogInformation( "Total count: {count}", response.Count );
            // _logger.LogDebug( $"Total count: { response.Id }" );

#if DEBUG
            _logger.LogDebug( "loop over" );
#endif
            // await Task.Delay( 1000, stoppingToken );
        }
    }

    public override async Task StopAsync( CancellationToken cancellationToken ) {
        // Debug.Assert( _clientStreamingCall != null );

        // Tell server that the client stream has finished
#if DEBUG
        _logger.LogInformation( "[Thread ID: {ThreadID}] Finishing call at: {time}", Thread.CurrentThread.ManagedThreadId, DateTimeOffset.Now );
#endif
        // await _clientStreamingCall.RequestStream.CompleteAsync();
        // _counterService.

        // Log total
        // var response = await _clientStreamingCall;
        // _logger.LogInformation( "Total count: {count}", response.Count );

        await base.StopAsync( cancellationToken );
    }
}