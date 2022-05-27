using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Benchmarks.InterThread.BroadcastQueue;

namespace Benchmarks.InterThread.Benchmark;


#region WriterNoLock

// allow multiple?
public class BroadcastQueueWriterNoLock<TData, TResponse> : BroadcastQueueWriter<TData, TResponse>  where TResponse : IBroadcastQueueResponse {
    // private readonly BroadcastQueueReader<TData, TResponse>[] _readers; // URGENT
    // private BroadcastQueueReader<TData, TResponse> _reader;

    // internal BroadcastQueueWriter( BroadcastQueue<TData, TResponse> queue, ChannelReader<TResponse> responseReader ) {
    // _queue          = queue;
    protected internal BroadcastQueueWriterNoLock( Channel<TResponse> responseReader ) : base( responseReader ) { }

    /* ************************************************** */

    #region Data

    /// <inheritdoc />
    public override bool TryComplete( Exception? error = null ) {
        bool result = true;
        foreach ( ( _, ChannelWriter<TData> channelWriter ) in _readers ) {
            result &= channelWriter.TryComplete( error );
        }

        return result;
    }

    /// <inheritdoc />
    public override bool TryWrite( TData item ) {
        if ( _readers.Length == 1 ) {
            return _readers[ 0 ].channelWriter.TryWrite( item );
        }

        bool result = true;
        foreach ( var (_, channelWriter) in _readers ) {
            result &= channelWriter.TryWrite( item );
        }

        return result;
    }

    /// <inheritdoc />
    public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = default ) {
        if ( _readers.Length == 0 ) {
            return ValueTask.CompletedTask;
        }

        if ( _readers.Length == 1 ) {
            return _readers[ 0 ].channelWriter.WriteAsync( item, cancellationToken );
        }

        return _readers.Select( r => r.channelWriter.WriteAsync( item, cancellationToken ) ).ToArray().WhenAll();
    }

    #endregion

    /* ************************************************** */
}

#endregion WriterNoLock


/* ************************************************************** */

public class BroadcastQueueWithNoLockWriter<TData, TResponse> : BroadcastQueue<TData, TResponse> where TResponse : IBroadcastQueueResponse {
    public override BroadcastQueueWriter<TData, TResponse> Writer {
        get {
            _writer ??= new BroadcastQueueWriterNoLock<TData, TResponse>(
                Channel.CreateUnbounded<TResponse>(
                    new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false }
                ) );

            return _writer;
        }
    }
}