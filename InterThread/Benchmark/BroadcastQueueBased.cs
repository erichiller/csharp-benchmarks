// #define DEBUG

#undef DEBUG
// #define DEBUG_PER_ITERATION_CONSOLE

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Benchmarks.InterThread.BroadcastQueue;

namespace Benchmarks.InterThread.Benchmark;

public class BroadcastPublisher : BackgroundService {
    // private readonly BroadcastQueue<ChannelMessage, ChannelResponse>       _broadcastQueue;
    private readonly BroadcastQueueWriter<ChannelMessage, ChannelResponse> _broadcastQueueWriter;
    private          int                                                   _id             = 0;
    public static    int                                                   MaxMessageCount = 0;
    // public static    void                                                  SetMaxMessageCount(int maxMessageCount) => MaxMessageCount = maxMessageCount;

#if DEBUG
    private readonly ILogger<BroadcastPublisher> _logger;
    private readonly Stopwatch                   _stopwatch;
#endif

    public BroadcastPublisher( ILogger<BroadcastPublisher> logger, BroadcastQueue<ChannelMessage, ChannelResponse> broadcastQueue ) {
        // _broadcastQueue       = broadcastQueue;
        _broadcastQueueWriter = broadcastQueue.Writer;
        if ( MaxMessageCount == 0 ) {
            throw new ArgumentException( $"MaxMessageCount can not be 0" );
        }
#if DEBUG
        _stopwatch = Stopwatch.StartNew();
        _logger = logger;
        _logger.LogInformation( "Constructing publisher with\n\t"                           +
                                "LastID         : {Id}\n\t"                                 +
                                "Time           : {time}\n\t"                               +
                                "Thread ID      : {ThreadId}\n\t"                           +
                                "Service Runtime:  {Runtime} ({ElapsedMilliseconds}ms)\n\t" +
                                "Reader Count   :  {ReaderCount}",
                                _id, DateTimeOffset.Now, Thread.CurrentThread.ManagedThreadId, _stopwatch.Elapsed, _stopwatch.ElapsedMilliseconds, _broadcastQueueWriter.ReaderCount );
#endif
    }

    public override async Task StartAsync( CancellationToken cancellationToken ) {
#if DEBUG
        _logger.LogInformation( "Starting publisher with\n\t"                              +
                                "LastID         : {Id}\n\t"                                +
                                "Time           : {time}\n\t"                              +
                                "Thread ID      : {ThreadId}\n\t"                          +
                                "Service Runtime: {Runtime} ({ElapsedMilliseconds}ms)\n\t" +
                                "Reader Count   : {ReaderCount}",
                                _id, DateTimeOffset.Now, Thread.CurrentThread.ManagedThreadId, _stopwatch.Elapsed, _stopwatch.ElapsedMilliseconds, _broadcastQueueWriter.ReaderCount );
#endif
        await base.StartAsync( cancellationToken );
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
        // https://github.com/dotnet/runtime/issues/36063#issuecomment-671110933 ; Fixed with await Task.Yield()
        await Task.Yield();
#if DEBUG
        _logger.LogDebug( "[Thread ID: {ThreadID}] Starting to WriteChannel", Thread.CurrentThread.ManagedThreadId );
#endif
        while ( await _broadcastQueueWriter.WaitToWriteAsync( stoppingToken ) ) {
            // if ( _id <= MaxMessageCount ) {
#if DEBUG
            _logger.LogDebug( "[Thread ID: {ThreadId}] Adding to Channel count {Id} at: {Datetime}", Thread.CurrentThread.ManagedThreadId, _id, DateTimeOffset.Now );
#endif
            var message = new ChannelMessage { Id = _id, Property1 = "some string" };
            _broadcastQueueWriter.TryWrite( message );

            if ( _id == MaxMessageCount ) {
#if DEBUG
                _logger.LogDebug( "{Type} Reached max message count ( {_id} == {MaxMessageCount} ). Returning from {Method}", nameof(BroadcastPublisher), _id, MaxMessageCount, nameof(ExecuteAsync) );
#endif
                return;
            }

            _id++;
            // }


#if DEBUG_PER_ITERATION_CONSOLE
            Console.Write( " b" );
            // await Task.Delay( 2000, stoppingToken );
            // _logger.LogDebug( $"Completed Writing. WaitToWriteAsync: {DateTimeOffset.Now}" );
#endif
        }
    }

    public override async Task StopAsync( CancellationToken cancellationToken ) {
        _broadcastQueueWriter.Complete();
#if DEBUG
        _logger.LogInformation( "End publishing with\n\tID: {LastID}\n\tTime: {time}\n\tThread ID: {ThreadId}\n\tService Runtime: {Runtime} ({ElapsedMilliseconds}ms)",
                                _id, DateTimeOffset.Now, Thread.CurrentThread.ManagedThreadId, _stopwatch.Elapsed, _stopwatch.ElapsedMilliseconds );
#endif
        await base.StopAsync( cancellationToken );
    }
}

/* **************************************************************************************************************
 ****************************************************************************************************************
 **************************************************************************************************************** */

public class BroadcastSubscriber : BackgroundService {
    private readonly BroadcastQueueReader<ChannelMessage, ChannelResponse> _broadcastQueueReader;
    public static    int                                                   MaxMessageCount = 0;

    protected virtual string thisTypeName { get; } = nameof(BroadcastSubscriber);

#if DEBUG
    private          int                          _lastId = 0;
    private readonly ILogger<BroadcastSubscriber> _logger;
    private readonly Stopwatch                    _stopwatch;
#endif
    public BroadcastSubscriber( ILogger<BroadcastSubscriber> logger, BroadcastQueue<ChannelMessage, ChannelResponse> broadcastQueue ) {
        _broadcastQueueReader = broadcastQueue.GetReader();
        if ( MaxMessageCount == 0 ) {
            throw new ArgumentException( $"MaxMessageCount can not be 0" );
        }
#if DEBUG
        _logger = logger;
        _stopwatch = Stopwatch.StartNew();
        _logger.LogInformation( "Constructing subscriber with\n\t" +
                                "LastID         : {LastID}\n\t"    +
                                "Time           : {time}\n\t"      +
                                "Thread ID      : {ThreadId}\n\t"  +
                                "Service Runtime: {Runtime} ({ElapsedMilliseconds}ms)\n\t",
                                _lastId, DateTimeOffset.Now, Thread.CurrentThread.ManagedThreadId, _stopwatch.Elapsed, _stopwatch.ElapsedMilliseconds );
#endif
    }

    protected override async Task ExecuteAsync( CancellationToken stoppingToken ) {
        // https://github.com/dotnet/runtime/issues/36063#issuecomment-671110933 ; Fixed with await Task.Yield()
        await Task.Yield();
#if DEBUG
        _logger.LogInformation( "Starting subscriber with\n\tLastID: {LastID}\n\tTime: {time}\n\tThread ID: {ThreadId}\n\tService Runtime:  {Runtime} ({ElapsedMilliseconds}ms)",
                                _lastId, DateTimeOffset.Now, Thread.CurrentThread.ManagedThreadId, _stopwatch.Elapsed, _stopwatch.ElapsedMilliseconds );
        _logger.LogDebug( "ExecuteAsync() - Starting to Read" );
#endif
        while ( await _broadcastQueueReader.WaitToReadAsync( stoppingToken ) ) {
#if DEBUG
            _logger.LogDebug( "ExecuteAsync() - ReadAllAsync beginning" );
#endif

            await foreach ( var message in _broadcastQueueReader.ReadAllAsync( stoppingToken ) ) {
#if DEBUG_PER_ITERATION_CONSOLE
                Console.Write( "S" );
#endif
#if DEBUG
                _logger.LogDebug( "[Thread ID: {ThreadID}] Received message: {Message}", Thread.CurrentThread.ManagedThreadId, message );
                _lastId = message.Id;
#endif
                if ( message.Id == MaxMessageCount ) {
                    var response = new ChannelResponse( message.Id, thisTypeName );
                    await _broadcastQueueReader.WriteResponseAsync( response, stoppingToken );
#if DEBUG
                    _logger.LogDebug( "{Type} Reached max message count ( {_id} == {MaxMessageCount} ). Returning from {Method}", thisTypeName, message.Id, MaxMessageCount, nameof(ExecuteAsync) );
#endif
                    // return;
                }
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
        _logger.LogInformation( "End subscription with\n\tLastID: {LastID}\n\tTime: {time}\n\tThread ID: {ThreadId}\n\tService Runtime:  {Runtime} ({ElapsedMilliseconds}ms)",
                                _lastId, DateTimeOffset.Now, Thread.CurrentThread.ManagedThreadId, _stopwatch.Elapsed, _stopwatch.ElapsedMilliseconds );
#endif
        await base.StopAsync( cancellationToken );
    }
}

public class BroadcastSubscriberTwo : BroadcastSubscriber {
    protected override string thisTypeName { get; } = nameof(BroadcastSubscriberTwo);

    public BroadcastSubscriberTwo( ILogger<BroadcastSubscriberTwo> logger, BroadcastQueue<ChannelMessage, ChannelResponse> broadcastQueue ) :
        base( logger, broadcastQueue ) { }
}

public class BroadcastSubscriberThree : BroadcastSubscriber {
    protected override string thisTypeName { get; } = nameof(BroadcastSubscriberThree);

    public BroadcastSubscriberThree( ILogger<BroadcastSubscriberThree> logger, BroadcastQueue<ChannelMessage, ChannelResponse> broadcastQueue ) :
        base( logger, broadcastQueue ) { }
}

/* ************************************************************** */
