using BenchmarkDotNet.Attributes;

namespace Benchmarks.InterThread.Benchmark;

public partial class Benchmarks {
    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ReadWriteLockSlimWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ReadWriteLockSlimWriter( ) {
        _broadcastQueue        = new BroadcastQueueWithReadWriteLockSlimWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueWriter  = _broadcastQueue.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "OneSubscriber", "BroadcastQueueAlt", "ReadWriteLockSlimWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ReadWriteLockSlimWriter( )
        => BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber(); // the only difference is in the setup

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ReadWriteLockSlimWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ReadWriteLockSlimWriter( ) {
        _broadcastQueue        = new BroadcastQueueWithReadWriteLockSlimWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueReader2 = _broadcastQueue.GetReader();
        _broadcastQueueWriter  = _broadcastQueue.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "TwoSubscribers", "BroadcastQueueAlt", "ReadWriteLockSlimWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ReadWriteLockSlimWriter( )
        => BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers(); // the only difference is in the setup

    [ IterationSetup( Targets = new[] { nameof(BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ReadWriteLockSlimWriter), } ) ]
    public void Setup_BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ReadWriteLockSlimWriter( ) {
        _broadcastQueue        = new BroadcastQueueWithReadWriteLockSlimWriter<ChannelMessage, ChannelResponse>();
        _broadcastQueueReader1 = _broadcastQueue.GetReader();
        _broadcastQueueReader2 = _broadcastQueue.GetReader();
        _broadcastQueueReader3 = _broadcastQueue.GetReader();
        _broadcastQueueWriter  = _broadcastQueue.Writer;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ThreeSubscribers", "BroadcastQueueAlt", "ReadWriteLockSlimWriter" ) ]
    public void BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ReadWriteLockSlimWriter( )
        => BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers(); // the only difference is in the setup
}