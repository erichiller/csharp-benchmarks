using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Benchmarks.Rpc.Worker.Shared;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Benchmarks.Rpc.Worker.Server;

public class ReceiveRemoteDataWorker : BackgroundService {
    private readonly ILogger<ReceiveRemoteDataWorker>                      _logger;
    private readonly Random                                                _random;
    private readonly System.Threading.Channels.ChannelWriter<CounterReply> _channelWriter;

    private const int DELAY_MS = 1000;

    public ReceiveRemoteDataWorker( ILogger<ReceiveRemoteDataWorker> logger, Channel<CounterReply> channel ) {
        _logger        = logger;
        _random        = new Random();
        _channelWriter = channel.Writer;
    }

    public override async Task StartAsync( CancellationToken cancellationToken ) {
        await base.StartAsync( cancellationToken );
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
        // Count until the worker exits
        while ( await _channelWriter.WaitToWriteAsync( stoppingToken ) ) {
            var count = _random.Next( 1, 10 );

            _logger.LogInformation( "Adding to Channel count {count} at: {time}", count, DateTimeOffset.Now );
            var reply = new CounterReply { Count = count };
            _channelWriter.TryWrite( reply );

            await Task.Delay( DELAY_MS, stoppingToken );
        }
    }

    public override async Task StopAsync( CancellationToken cancellationToken ) {
        _logger.LogInformation( "Stopping {ClassName} at: {Time}", nameof(ReceiveRemoteDataWorker), DateTimeOffset.Now );

        await base.StopAsync( cancellationToken );
    }
}