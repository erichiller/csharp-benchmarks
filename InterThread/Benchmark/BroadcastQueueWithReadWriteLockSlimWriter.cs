using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Benchmarks.InterThread.BroadcastQueue;

namespace Benchmarks.InterThread.Benchmark;


#region WriterReadWriteLock

// allow multiple?
public class BroadcastQueueWriterReadWriteLock<TData, TResponse> : BroadcastQueueWriter<TData, TResponse>  where TResponse : IBroadcastQueueResponse {
    private ReaderWriterLockSlim _rwLockSlim = new ReaderWriterLockSlim();
    protected internal BroadcastQueueWriterReadWriteLock( Channel<TResponse> responseReader ) : base( responseReader ) { }


    public override int ReaderCount {
        get {
            _rwLockSlim.ExitReadLock();
            try {
                return _readers.Length;
            } finally {
                _rwLockSlim.ExitReadLock();
            }
        }
    }

    /* ************************************************** */

    protected override void AddReader( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> writer ) {
        _rwLockSlim.EnterWriteLock();
        try {
            _readers.Add( ( reader, writer ) );
        } finally {
            _rwLockSlim.ExitWriteLock();
        }
    }

    protected override void RemoveReader( BroadcastQueueReader<TData, TResponse> reader ) {
        _rwLockSlim.EnterWriteLock();
        try {
            // do
        } finally {
            _rwLockSlim.ExitWriteLock();
        }
    }

    /* ************************************************** */

    #region Data

    /// <inheritdoc />
    public override bool TryComplete( Exception? error = null ) {
        bool result = true;
        _rwLockSlim.EnterReadLock();
        try {
            foreach ( ( _, ChannelWriter<TData> channelWriter ) in _readers ) {
                result &= channelWriter.TryComplete( error );
            }
        } finally {
            _rwLockSlim.ExitReadLock();
        }

        return result;
    }

    /// <inheritdoc />
    public override bool TryWrite( TData item ) {
        _rwLockSlim.EnterReadLock();
        try {
            if ( _readers.Length == 1 ) {
                return _readers[ 0 ].channelWriter.TryWrite( item );
            }

            bool result = true;
            foreach ( var (_, channelWriter) in _readers ) {
                result &= channelWriter.TryWrite( item );
            }

            return result;
        } finally {
            _rwLockSlim.ExitReadLock();
        }
    }

    /// <inheritdoc />
    public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = default ) {
        _rwLockSlim.EnterReadLock();
        try {
            if ( _readers.Length == 0 ) {
                return ValueTask.CompletedTask;
            }

            if ( _readers.Length == 1 ) {
                return _readers[ 0 ].channelWriter.WriteAsync( item, cancellationToken );
            }

            return _readers.Select( r => r.channelWriter.WriteAsync( item, cancellationToken ) ).ToArray().WhenAll();
        } finally {
            _rwLockSlim.ExitReadLock();
        }
    }

    #endregion

    /* ************************************************** */
}

#endregion WriterWriterReadWriteLock


/* ************************************************************** */

public class BroadcastQueueWithReadWriteLockSlimWriter<TData, TResponse> : BroadcastQueue<TData, TResponse> where TResponse : IBroadcastQueueResponse {
    public override BroadcastQueueWriter<TData, TResponse> Writer {
        get {
            _writer ??= new BroadcastQueueWriterReadWriteLock<TData, TResponse>(
                Channel.CreateUnbounded<TResponse>(
                    new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false }
                ) );

            return _writer;
        }
    }
}