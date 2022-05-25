using BenchmarkDotNet.Attributes;

namespace Benchmarks.InterThread.Benchmark;

public partial class Benchmarks {
    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_SemaphoreSlimWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_SemaphoreSlimWriter( ) {
        _broadcastQueue        = new BroadcastQueueWithSemaphoreSlimWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueWriter  = _broadcastQueue.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "OneSubscriber", "BroadcastQueueAlt", "Locking", "SemaphoreSlimWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_SemaphoreSlimWriter( )
        => BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber(); // the only difference is in the setup

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_SemaphoreSlimWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_SemaphoreSlimWriter( ) {
        _broadcastQueue        = new BroadcastQueueWithSemaphoreSlimWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueReader2 = _broadcastQueue.GetReader();
        _broadcastQueueWriter  = _broadcastQueue.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "TwoSubscribers", "BroadcastQueueAlt", "Locking", "SemaphoreSlimWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_SemaphoreSlimWriter( )
        => BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers(); // the only difference is in the setup

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_SemaphoreSlimWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_SemaphoreSlimWriter( ) {
        _broadcastQueue        = new BroadcastQueueWithSemaphoreSlimWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueReader2 = _broadcastQueue.GetReader();
        _broadcastQueueReader3 = _broadcastQueue.GetReader();
        _broadcastQueueWriter  = _broadcastQueue.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ThreeSubscribers", "BroadcastQueueAlt", "Locking", "SemaphoreSlimWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_SemaphoreSlimWriter( )
        => BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers(); // the only difference is in the setup
}