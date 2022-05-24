using System.Threading.Channels;

namespace Benchmarks.InterThread.BroadcastQueue;

public class BroadcastQueueWithNoChannelOptions<TData, TResponse> : BroadcastQueue<TData, TResponse> where TResponse : IBroadcastQueueResponse {
    public override BroadcastQueueWriter<TData, TResponse> Writer {
        get {
            _writer ??= new BroadcastQueueWriter<TData, TResponse>(
                Channel.CreateUnbounded<TResponse>() );

            return _writer;
        }
    }

    public override BroadcastQueueReader<TData, TResponse> GetReader( ) => GetReader( Channel.CreateUnbounded<TData>() );
}

public class BroadcastQueueWithSingleXChannelOptions<TData, TResponse> : BroadcastQueue<TData, TResponse> where TResponse : IBroadcastQueueResponse {
    public override BroadcastQueueWriter<TData, TResponse> Writer {
        get {
            _writer ??= new BroadcastQueueWriter<TData, TResponse>(
                Channel.CreateUnbounded<TResponse>(
                    new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false }
                ) );

            return _writer;
        }
    }

    public override BroadcastQueueReader<TData, TResponse> GetReader( ) => GetReader( Channel.CreateUnbounded<TData>( new UnboundedChannelOptions() { SingleReader = true, SingleWriter = true } ) );
}