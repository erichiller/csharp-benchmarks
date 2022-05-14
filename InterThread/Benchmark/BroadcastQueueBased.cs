#define DEBUG
// #undef DEBUG

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Benchmarks.InterThread.BroadcastQueue;

namespace Benchmarks.InterThread.Benchmark;

public class BroadcastPublisher : BackgroundService {
    private readonly BroadcastQueue<ChannelMessage, ChannelResponse> _broadcastQueue;

    private readonly ILogger<BroadcastPublisher> _logger;

    private          int       _id = 0;
    private readonly Stopwatch _stopwatch;

    public BroadcastPublisher( ILogger<BroadcastPublisher> logger, BroadcastQueue<ChannelMessage, ChannelResponse> broadcastQueue ) {
        _stopwatch                   = Stopwatch.StartNew();
        ( _logger, _broadcastQueue ) = ( logger, broadcastQueue );
        _logger.LogInformation( "Constructing publisher with\n\t"                           +
                                "LastID         : {Id}\n\t"                                 +
                                "Time           : {time}\n\t"                               +
                                "Thread ID      : {ThreadId}\n\t"                           +
                                "Service Runtime:  {Runtime} ({ElapsedMilliseconds}ms)\n\t" +
                                "Reader Count   :  {ReaderCount}",
                                _id, DateTimeOffset.Now, Thread.CurrentThread.ManagedThreadId, _stopwatch.Elapsed, _stopwatch.ElapsedMilliseconds, _broadcastQueue._readers.Count );

        // ( _logger, _broadcastQueue ) = ( NullLogger<BroadcastPublisher>.Instance, broadcastQueue );
    }

    public override async Task StartAsync( CancellationToken cancellationToken ) {
#if DEBUG
        _logger.LogInformation( "[Thread ID: {ThreadID}] Starting client streaming call at: {time}", Thread.CurrentThread.ManagedThreadId, DateTimeOffset.Now );
#endif
        _logger.LogInformation( "Starting publisher with\n\t"                              +
                                "LastID         : {Id}\n\t"                                +
                                "Time           : {time}\n\t"                              +
                                "Thread ID      : {ThreadId}\n\t"                          +
                                "Service Runtime: {Runtime} ({ElapsedMilliseconds}ms)\n\t" +
                                "Reader Count   : {ReaderCount}",
                                _id, DateTimeOffset.Now, Thread.CurrentThread.ManagedThreadId, _stopwatch.Elapsed, _stopwatch.ElapsedMilliseconds, _broadcastQueue._readers.Count );
        // Don't pass cancellation token to the call. The call is completed in StopAsync when service stops.
        await base.StartAsync( cancellationToken );
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
        // https://github.com/dotnet/runtime/issues/36063#issuecomment-671110933 ; Fixed with await Task.Yield()
        await Task.Yield();
#if DEBUG
        _logger.LogDebug( "[Thread ID: {ThreadID}] Starting to WriteChannel", Thread.CurrentThread.ManagedThreadId );
#endif
        // Count until the worker exits
        // while ( !stoppingToken.IsCancellationRequested ){ }

        // return;
        // var writer = _broadcastQueue._readers[ 0 ];
        while ( await _broadcastQueue.WaitToWriteAsync( stoppingToken ) ) {
            // _logger.LogInformation( "Adding to Channel count {count} at: {time}", count, DateTimeOffset.Now );
#if DEBUG
            _logger.LogDebug( $"[Thread ID: {Thread.CurrentThread.ManagedThreadId}] Adding to Channel count {_id} at: {DateTimeOffset.Now}" );
#endif
            var message = new ChannelMessage { Id = _id, Property_1 = "some string" };
            _broadcastQueue.TryWrite( message );
            // await _broadcastQueue.WriteAsync( message, stoppingToken ); // URGENT: restore?
            _id++;
            Console.Write( " b" );
            // await Task.Delay( 2000, stoppingToken );
            // await Task.Delay( 2, stoppingToken );
#if DEBUG
            // await Task.Delay( 2000, stoppingToken );
            // _logger.LogDebug( $"Completed Writing. WaitToWriteAsync: {DateTimeOffset.Now}" ); // URGENT: restore
#endif
        }
    }

    public override async Task StopAsync( CancellationToken cancellationToken ) {
        _broadcastQueue.Complete();
#if DEBUG
        _logger.LogInformation( "Finishing call at: {time}", DateTimeOffset.Now );
#endif

        _logger.LogInformation( "End publishing with\n\tID: {LastID}\n\tTime: {time}\n\tThread ID: {ThreadId}\n\tService Runtime: {Runtime} ({ElapsedMilliseconds}ms)",
                                _id, DateTimeOffset.Now, Thread.CurrentThread.ManagedThreadId, _stopwatch.Elapsed, _stopwatch.ElapsedMilliseconds );
        await base.StopAsync( cancellationToken );
    }
}

/* **************************************************************************************************************
 ****************************************************************************************************************
 **************************************************************************************************************** */

public class BroadcastSubscriber : BackgroundService {
    private readonly ILogger<BroadcastSubscriber>                          _logger;
    private readonly BroadcastQueue<ChannelMessage, ChannelResponse>       _broadcastQueue;
    // private readonly BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader;
    private  BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader; // URGENT: Restore

    private          int       _lastId = 0;
    private readonly Stopwatch _stopwatch;

    public BroadcastSubscriber( ILogger<BroadcastSubscriber> logger, BroadcastQueue<ChannelMessage, ChannelResponse> broadcastQueue )
        // => ( _logger, _broadcastQueue, _broadcastQueueReader ) = ( NullLogger<BroadcastSubscriber>.Instance, broadcastQueue, broadcastQueue.GetReader() );
        // => ( _logger, _broadcastQueue, _broadcastQueueReader, _stopwatch ) = ( logger, broadcastQueue, broadcastQueue.GetReader(), Stopwatch.StartNew() );
    {
        ( _logger, _broadcastQueue, _stopwatch ) = ( logger, broadcastQueue, Stopwatch.StartNew() );
        // ( _logger, _broadcastQueue, _broadcastQueueReader, _stopwatch ) = ( logger, broadcastQueue, broadcastQueue.GetReader(), Stopwatch.StartNew() ); // URGENT: RESTORE
        _logger.LogInformation( "Constructing subscriber with\n\t"                         +
                                "LastID         : {LastID}\n\t"                            +
                                "Time           : {time}\n\t"                              +
                                "Thread ID      : {ThreadId}\n\t"                          +
                                "Service Runtime: {Runtime} ({ElapsedMilliseconds}ms)\n\t" +
                                "Reader Count   : {ReaderCount}",
                                _lastId, DateTimeOffset.Now, Thread.CurrentThread.ManagedThreadId, _stopwatch.Elapsed, _stopwatch.ElapsedMilliseconds, _broadcastQueue._readers.Count );
    }

//     public override async Task StartAsync( CancellationToken cancellationToken ) {
// #if DEBUG
//         _logger.LogInformation( "{MethodName} client streaming call at: {Time}", nameof(StartAsync), DateTimeOffset.Now );
// #endif
//         Console.WriteLine( $"[Thread ID: {Thread.CurrentThread.ManagedThreadId}] StartAsync" );
//         // Don't pass cancellation token to the call. The call is completed in StopAsync when service stops.
//         await base.StartAsync( cancellationToken );
//     }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
        await Task.Delay( 2000, stoppingToken );
        _broadcastQueueReader = _broadcastQueue.GetReader();
        _logger.LogInformation( "Starting subscriber with\n\tLastID: {LastID}\n\tTime: {time}\n\tThread ID: {ThreadId}\n\tService Runtime:  {Runtime} ({ElapsedMilliseconds}ms)",
                                _lastId, DateTimeOffset.Now, Thread.CurrentThread.ManagedThreadId, _stopwatch.Elapsed, _stopwatch.ElapsedMilliseconds );
        // https://github.com/dotnet/runtime/issues/36063#issuecomment-671110933 ; Fixed with await Task.Yield()
        await Task.Yield();
#if DEBUG
        _logger.LogDebug( "ExecuteAsync() - Starting to Read" );
#endif
        while ( await _broadcastQueueReader.WaitToReadAsync( stoppingToken ) ) {
            await foreach ( var message in _broadcastQueueReader.ReadAllAsync( stoppingToken ) ) {
                Console.Write( "S" );
#if DEBUG
                _logger.LogDebug( "[Thread ID: {ThreadID}] Received message: {Message}", Thread.CurrentThread.ManagedThreadId, message );
#endif
                _lastId = message.Id;
                var response = new ChannelResponse() { ReadId = message.Id };
                await _broadcastQueueReader.WriteResponseAsync( response, stoppingToken );
            }

#if DEBUG
            _logger.LogDebug( "loop over" );
            // Console.WriteLine("DEBUG MODE DETECTED -- Task.Delay");
            // await Task.Delay( 1000, stoppingToken );
#endif
        }
    }

    public override async Task StopAsync( CancellationToken cancellationToken ) {
#if DEBUG
        _logger.LogInformation( "[Thread ID: {ThreadID}] Finishing call at: {time}", Thread.CurrentThread.ManagedThreadId, DateTimeOffset.Now );
#endif
        _logger.LogInformation( "End subscription with\n\tLastID: {LastID}\n\tTime: {time}\n\tThread ID: {ThreadId}\n\tService Runtime:  {Runtime} ({ElapsedMilliseconds}ms)",
                                _lastId, DateTimeOffset.Now, Thread.CurrentThread.ManagedThreadId, _stopwatch.Elapsed, _stopwatch.ElapsedMilliseconds );
        await base.StopAsync( cancellationToken );
    }
}

public class BroadcastSubscriberTwo : BroadcastSubscriber {
    public BroadcastSubscriberTwo( ILogger<BroadcastSubscriberTwo> logger, BroadcastQueue<ChannelMessage, ChannelResponse> broadcastQueue ) : base( logger, broadcastQueue ) { }
}