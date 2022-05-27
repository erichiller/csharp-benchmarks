using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Benchmarks.InterThread.BroadcastQueue;

namespace Benchmarks.InterThread.Benchmark;


#region WriterSemaphoreSlim

// allow multiple?
public class BroadcastQueueWriterSemaphoreSlim<TData, TResponse> : BroadcastQueueWriter<TData, TResponse>  where TResponse : IBroadcastQueueResponse {
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim( 1, 1 );
    protected internal BroadcastQueueWriterSemaphoreSlim( Channel<TResponse> responseReader ) : base( responseReader ) { }


    public override int ReaderCount {
        get {
            _semaphore.Wait();
            try {
                return _readers.Length;
            } finally {
                _semaphore.Release();
            }
        }
    }

    /* ************************************************** */

    protected override void AddReader( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> writer ) {
        _semaphore.Wait();
        try {
            _readers.Add( ( reader, writer ) );
        } finally {
            _semaphore.Release();
        }
    }

    protected override void RemoveReader( BroadcastQueueReader<TData, TResponse> reader ) {
        _semaphore.Wait();
        try {
            // do
        } finally {
            _semaphore.Release();
        }
    }

    /* ************************************************** */

    #region Data

    /// <inheritdoc />
    public override bool TryComplete( Exception? error = null ) {
        bool result = true;
        _semaphore.Wait();
        try {
            foreach ( ( _, ChannelWriter<TData> channelWriter ) in _readers ) {
                result &= channelWriter.TryComplete( error );
            }
        } finally {
            _semaphore.Release();
        }

        return result;
    }

    /// <inheritdoc />
    public override bool TryWrite( TData item ) {
        _semaphore.Wait();
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
            _semaphore.Release();
        }
    }

    /// <inheritdoc />
    public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = default ) {
        _semaphore.Wait( cancellationToken );
        try {
            if ( _readers.Length == 0 ) {
                return ValueTask.CompletedTask;
            }

            if ( _readers.Length == 1 ) {
                return _readers[ 0 ].channelWriter.WriteAsync( item, cancellationToken );
            }

            return _readers.Select( r => r.channelWriter.WriteAsync( item, cancellationToken ) ).ToArray().WhenAll();
        } finally {
            _semaphore.Release();
        }
    }

    #endregion

    /* ************************************************** */
}

#endregion WriterWriterSemaphoreSlim


/* ************************************************************** */

public class BroadcastQueueWithSemaphoreSlimWriter<TData, TResponse> : BroadcastQueue<TData, TResponse> where TResponse : IBroadcastQueueResponse {
    public override BroadcastQueueWriter<TData, TResponse> Writer {
        get {
            _writer ??= new BroadcastQueueWriterSemaphoreSlim<TData, TResponse>(
                Channel.CreateUnbounded<TResponse>(
                    new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false }
                ) );

            return _writer;
        }
    }
}