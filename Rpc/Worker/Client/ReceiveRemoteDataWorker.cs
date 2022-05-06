using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Benchmarks.Rpc.Worker.Definitions;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ProtoBuf.Grpc;

namespace Benchmarks.Rpc.Worker.Client;

// Client Streaming example
public class ReceiveRemoteDataWorker : BackgroundService {
    private readonly ILogger<ReceiveRemoteDataWorker> _logger;
    private readonly ICounterService _counterService;
    private readonly Random          _random;

    public ReceiveRemoteDataWorker( ILogger<ReceiveRemoteDataWorker> logger, ICounterService counterService ) {
        _logger         = logger;
        _counterService = counterService;
        _random         = new Random();
    }

    public override async Task StartAsync( CancellationToken cancellationToken ) {
        await base.StartAsync( cancellationToken );
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
        // Debug.Assert( _clientStreamingCall != null );

        // Count until the worker exits
        while ( !stoppingToken.IsCancellationRequested ) {

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
    public async IAsyncEnumerable<CounterRequest> SendAsync( [ EnumeratorCancellation ] CancellationToken cancellationToken = default ) {
        try {
            while ( true ) {
                if ( cancellationToken.IsCancellationRequested ) {
                    break;
                }
                var count = _random.Next( 1, 10 );

                _logger.LogInformation( "Sending count {count} at: {time}", count, DateTimeOffset.Now );
                var request = new CounterRequest { Count = count };
                Console.WriteLine( $"Sending request: {request}" );

                yield return request;

                //Look busy
                try {
                    // important: depending on timing , we *might not get here*; this
                    // will only be invoked if the CT isn't *already* cancelled when
                    // MoveNextAsync gets invoked
                    await Task.Delay( 1000, cancellationToken );
                } catch ( Exception ex ) {
                    Console.WriteLine( $"Delay exited: {ex.Message}" );
                }
            }
        } finally {
            Console.WriteLine( "writer is FINISHED" );
        }
    }
    
    
    static readonly Func<CallContext, ValueTask> s_pumpQueue = async ctx =>
    {
        var writer = ctx.As<System.Threading.Channels.ChannelWriter<CounterRequest>>();
        try
        {
            for (int i = 0; i < 5; i++)
            {
                var item = new CounterRequest { Count = i };
                await writer.WriteAsync(item, ctx.CancellationToken);
                Console.WriteLine($"[d:sent] {item}");
                await Task.Delay(TimeSpan.FromSeconds(0.5));
            }
            writer.Complete();
            Console.WriteLine("[d:client all done sending!]");
        }
        catch (Exception ex) { writer.TryComplete(ex); }
    };
    
    
    

    static async Task TestDuplex(IDuplex duplex, [CallerMemberName] string? caller = null)
    {
        Console.WriteLine($"testing duplex ({caller}) - manual");
        await foreach (var item in duplex.SomeDuplexApiAsync(Rand(10, TimeSpan.FromSeconds(1))))
        {
            Console.WriteLine($"[rec] {item.Result}");
        }

        Console.WriteLine($"testing duplex ({caller}) - auto duplex");
        var channel = System.Threading.Channels.Channel.CreateBounded<MultiplyRequest>(5);
        var ctx     = new CallContext(state: channel.Writer);
        var result  = duplex.SomeDuplexApiAsync(channel.AsAsyncEnumerable(ctx.CancellationToken));
        await ctx.FullDuplexAsync<MultiplyResult>(s_pumpQueue, result, s_handleResult);
    }
    
    
    

}