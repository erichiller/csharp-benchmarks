using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Benchmarks.InterThread.BroadcastQueue;

namespace Benchmarks.InterThread.Benchmark;

#region WriterLockedImmutableArray

// allow multiple?
public class BroadcastQueueLockedImmutableArrayWriter<TData, TResponse> : BroadcastQueueImmutableArrayWriter<TData, TResponse> where TResponse : IBroadcastQueueResponse {
    protected internal BroadcastQueueLockedImmutableArrayWriter( Channel<TResponse> responseReader ) : base( responseReader ) { }

    protected readonly object _readersLock = new object();

    public override int ReaderCount {
        get {
            lock ( _readersLock ) {
                return _readers.Length;
            }
        }
    }

    /* ************************************************** */

    protected override void AddReader( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> writer ) {
        lock ( _readersLock ) {
            _readers = _readers.Add( ( reader, writer ) );
        }
    }

    protected override void RemoveReader( BroadcastQueueReader<TData, TResponse> reader ) {
        lock ( _readersLock ) {
            _readers = _readers.Remove( _readers.Single( t => t.reader == reader ) );
        }
    }

    /* ************************************************** */

    #region Data

    /// <inheritdoc />
    public override bool TryComplete( Exception? error = null ) {
        bool result = true;
        lock ( _readersLock ) {
            foreach ( ( _, ChannelWriter<TData> channelWriter ) in _readers ) {
                result &= channelWriter.TryComplete( error );
            }
        }

        return result;
    }

    /// <inheritdoc />
    public override bool TryWrite( TData item ) {
        lock ( _readersLock ) {
            if ( _readers.Length == 1 ) {
                return _readers[ 0 ].channelWriter.TryWrite( item );
            }
            if ( _readers.Length == 0 ) { return true; } // this returns true as if it had written regardless of if there was an actual reader to read it

            bool result = true;
            foreach ( var (_, channelWriter) in _readers ) {
                result &= channelWriter.TryWrite( item );
            }

            return result;
        }
    }

    /// <inheritdoc />
    public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = default ) {
        lock ( _readersLock ) {
            if ( _readers.Length == 0 ) {
                return ValueTask.CompletedTask;
            }

            if ( _readers.Length == 1 ) {
                return _readers[ 0 ].channelWriter.WriteAsync( item, cancellationToken );
            }

            return _readers.Select( r => r.channelWriter.WriteAsync( item, cancellationToken ) ).ToArray().WhenAll();
        }
    }

    #endregion

    /* ************************************************** */
}

#endregion WriterWriterLockedImmutableArray

/* ************************************************************** */

public class BroadcastQueueWithLockedImmutableArrayWriter<TData, TResponse> : BroadcastQueueWithImmutableArrayWriter<TData, TResponse> where TResponse : IBroadcastQueueResponse {
    // private BroadcastQueueLockedImmutableArrayWriter<         TData, TResponse>? _writer; 

    public override BroadcastQueueImmutableArrayWriter<TData, TResponse> Writer {
        get {
            _writer ??= new BroadcastQueueLockedImmutableArrayWriter<TData, TResponse>(
                _responseChannel );

            return _writer;
        }
    }
}