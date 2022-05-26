using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Benchmarks.InterThread.BroadcastQueue;

namespace Benchmarks.InterThread.Benchmark;

#region LockedImmutableArrayForLoopWriter

// allow multiple?
public class BroadcastQueueLockedImmutableArrayForLoopWriter<TData, TResponse> : BroadcastQueueLockedImmutableArrayWriter<TData, TResponse> where TResponse : IBroadcastQueueResponse {
    protected internal BroadcastQueueLockedImmutableArrayForLoopWriter( Channel<TResponse> responseReader ) : base( responseReader ) { }

    /// <inheritdoc />
    public override bool TryWrite( TData item ) {
        lock ( _readersLock ) {
            if ( _readers.Length == 1 ) {
                return _readers[ 0 ].channelWriter.TryWrite( item );
            }

            bool result = true;
            for ( int i = 0 ; i < _readers.Length ; i ++ ){
                result &= _readers[i].channelWriter.TryWrite( item );
            }

            return result;
        }
    }
}

#endregion LockedImmutableArrayForLoopWriter

/* ************************************************************** */

public class BroadcastQueueWithLockedImmutableArrayForLoopWriter<TData, TResponse> : BroadcastQueueWithImmutableArrayWriter<TData, TResponse> where TResponse : IBroadcastQueueResponse {
    // private BroadcastQueueLockedImmutableArrayForLoopWriter<         TData, TResponse>? _writer; 

    public override BroadcastQueueImmutableArrayWriter<TData, TResponse> Writer {
        get {
            _writer ??= new BroadcastQueueLockedImmutableArrayForLoopWriter<TData, TResponse>(
                _responseChannel );

            return _writer;
        }
    }
}