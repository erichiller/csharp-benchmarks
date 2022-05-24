using BenchmarkDotNet.Attributes;

namespace Benchmarks.InterThread.Benchmark;

public partial class Benchmarks {
    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_NoLockWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_NoLockWriter( ) {
        _broadcastQueue        = new BroadcastQueueWithNoLockWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueWriter  = _broadcastQueue.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "OneSubscriber", "BroadcastQueueAlt", "NoLockWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_NoLockWriter( )
        => BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber(); // the only difference is in the setup

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_NoLockWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_NoLockWriter( ) {
        _broadcastQueue        = new BroadcastQueueWithNoLockWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueReader2 = _broadcastQueue.GetReader();
        _broadcastQueueWriter  = _broadcastQueue.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "TwoSubscribers", "BroadcastQueueAlt", "NoLockWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_NoLockWriter( )
        => BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers(); // the only difference is in the setup

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_NoLockWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_NoLockWriter( ) {
        _broadcastQueue        = new BroadcastQueueWithNoLockWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueReader2 = _broadcastQueue.GetReader();
        _broadcastQueueReader3 = _broadcastQueue.GetReader();
        _broadcastQueueWriter  = _broadcastQueue.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ThreeSubscribers", "BroadcastQueueAlt", "NoLockWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_NoLockWriter( )
        => BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers(); // the only difference is in the setup
}