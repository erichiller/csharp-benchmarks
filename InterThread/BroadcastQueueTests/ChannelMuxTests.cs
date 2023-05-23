using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Benchmarks.InterThread.BroadcastQueue;
using Benchmarks.InterThread.BroadcastQueueTests;

using FluentAssertions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Xunit;
using Xunit.Abstractions;

namespace Benchmarks.InterThread.ChannelMuxTests;

// // fixture
// public class SetupFixture {
//     private IHost   host;
//     private ILogger logger;
//
//     public static IHostBuilder CreateHostBuilder_BroadcastQueue( string[] args, LogLevel? logLevel ) =>
//         Host.CreateDefaultBuilder( args )
//             .ConfigureServices( ( hostContext, services ) => {
//                 services.AddSingleton<BroadcastQueue<ChannelMessage, ChannelResponse>>();
//                 if ( logLevel is { } ll ) {
//                     services.AddLogging( logBuilder => logBuilder.AddConsole().SetMinimumLevel( ll ) );
//                 }
//             } );
//
//     private BroadcastQueue<ChannelMessage, ChannelResponse> _broadcastQueue;
//
//     public async Task CreateHost<T>( LogLevel? logLevel ) {
//         host = CreateHostBuilder_BroadcastQueue( Array.Empty<string>(), logLevel )
//             .Build();
//         await host.StartAsync();
//
//         logger = host.Services.GetService<ILogger<T>>() ?? throw new Exception();
//         // cts             = new CancellationTokenSource();
//         _broadcastQueue = host.Services.GetService<BroadcastQueue<ChannelMessage, ChannelResponse>>() ?? throw new Exception();
//
//         // Console.WriteLine( "Host is running" );
//     }
// }

public class ChannelMuxTests : TestBase<ChannelMuxTests> {
    public ChannelMuxTests( ITestOutputHelper testOutputHelper ) : base( testOutputHelper ) { }
    [ Fact ]
    public void AddingAndRemovingReadersShouldNeverError( ) {
        
        // int subscriberCount = 3;
        // // int messageCount    = 10_000;
        // int          messageCount       = 1_000;
        // var          broadcastQueue     = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        // var          cts                = new CancellationTokenSource();
        // // ( int, int ) writeIntervalRange = ( 1, 200 ); 
        // ( int, int ) writeIntervalRange = ( 1, 100 ); 
        // cts.CancelAfter( 300_000 );
        //
        // List<Task<int>> readerTasks = new List<Task<int>>();
        // for ( int i = 0 ; i < subscriberCount ; i++ ) {
        //     readerTasks.Add( readerTask( broadcastQueue.GetReader(), messageCount, $"readerTask{i}", cts.Token ) );
        // }
        //
        // var addReaderTaskRunner = addReaderTask( broadcastQueue, messageCount, cts.Token, _logger );
        //
        // List<Task> tasks = new List<Task>(
        //     readerTasks
        // ) { addReaderTaskRunner, writerTask( broadcastQueue.Writer, messageCount, cts.Token, writeIntervalRange, _logger ), };
        //
        // try {
        //     Task.WaitAll(
        //         tasks.ToArray()
        //     );
        // } catch ( System.AggregateException ex ) {
        //     bool taskCanceledException = false;
        //     foreach ( var inner in ex.InnerExceptions ) {
        //         if ( inner.GetType() == typeof(System.Threading.Tasks.TaskCanceledException) ) {
        //             _logger.LogDebug( "Task was cancelled" );
        //             taskCanceledException = true;
        //         }
        //     }
        //
        //     if ( !taskCanceledException ) {
        //         throw;
        //     }
        // }
        //
        // foreach ( var task in readerTasks ) {
        //     task.Result.Should().Be( messageCount );
        //     _logger.LogDebug( "Task had result {ResultMessageId}", task.Result );
        // }
        //
        // _logger.LogDebug( "BroadcastQueue ended with {ReaderCount} readers", broadcastQueue.Writer.ReaderCount );
        // _logger.LogDebug(
        //     "AddReaderTask created {ReaderCount} on {UniqueTaskIdCount} threads with an average interval between messages of {AverageInterval} ms",
        //     addReaderTaskRunner.Result.readerCount,
        //     addReaderTaskRunner.Result.uniqueThreadIds.Count,
        //     Math.Round(addReaderTaskRunner.Result.intervals.Average()/System.TimeSpan.TicksPerMillisecond,2) );
        // broadcastQueue.Writer.ReaderCount.Should().Be( 4 );
    }

    [ Theory ]
    [ InlineData( 1 ) ]
    [ InlineData( 2 ) ]
    [ InlineData( 3 ) ]
    public void SubscribersShouldReceiveAllMessagesInOrder( int subscriberCount ) {
        // int messageCount   = 10_000;
        // var broadcastQueue = new BroadcastQueue<ChannelMessage, ChannelResponse>();
        // var cts            = new CancellationTokenSource();
        //
        // List<Task<int>> readerTasks = new List<Task<int>>();
        // for ( int i = 0 ; i < subscriberCount ; i++ ) {
        //     readerTasks.Add( readerTask( broadcastQueue.GetReader(), messageCount, $"readerTask{i}", cts.Token ) );
        // }
        //
        // List<Task> tasks = new List<Task>( readerTasks ) { writerTask( broadcastQueue.Writer, messageCount, cts.Token ) };
        //
        //
        // Task.WaitAll(
        //     tasks.ToArray()
        // );
        // foreach ( var task in readerTasks ) {
        //     task.Result.Should().Be( messageCount );
        //     _logger.LogDebug( "Task had result {ResultMessageId}", task.Result );
        // }
    }
}