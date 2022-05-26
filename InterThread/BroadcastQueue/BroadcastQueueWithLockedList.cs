using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks.InterThread.BroadcastQueue;

/* ********************************************************************************************* */

#region Writer
// URGENT: Convert to ImmutableArray. Test if lock is needed!?

public class BroadcastQueueWithLockedListWriter<TData, TResponse> : ChannelWriter<TData> where TResponse : IBroadcastQueueResponse {
    private readonly ChannelReader<TResponse> _responseReader;
    internal readonly Channel<TResponse>       ResponseChannel; // URGENT: fully move this to Writer, do not store in BroadcastQueueWithLockedList

    /* Only 1 or 2 threads ever use _readers:
     *  1. Write: The BroadcastQueueWithLockedList root through AddReader 
     *  2. Read: The BroadcastQueueWithLockedListWriter when it enumerates
     */
    protected readonly List<( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> channelWriter)> _readers     = new List<( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> channelWriter)>(); // URGENT
    protected readonly   object                                                                                     _readersLock = new object();

    public virtual int ReaderCount {
        get {
            lock ( _readersLock ) {
                return _readers.Count;
            }
        }
    }


    protected internal BroadcastQueueWithLockedListWriter( Channel<TResponse> responseChannel ) {
        ResponseChannel = responseChannel;
        _responseReader  = responseChannel.Reader;
    }

    /* ************************************************** */

    protected internal virtual void AddReader( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> writer ) {
        lock ( _readersLock ) {
            _readers.Add( ( reader, writer ) );
        }
    }

    protected internal virtual void RemoveReader( BroadcastQueueReader<TData, TResponse> reader ) {
        lock ( _readersLock ) {
            _readers.Remove( _readers.Single( t => t.reader == reader ) );
        }
    }

    // URGENT: Implement IDisposable on Reader, Writer?
    public void Dispose( ) {
        lock ( _readersLock ) {
            foreach ( var (reader, channelWriter) in _readers ) {
                channelWriter.TryComplete();
            }
        }

        ResponseChannel.Writer.TryComplete();
    }


    #region Response

    /// <summary>
    /// Return <see cref="ChannelReader{T}"/> for <typeparamref name="TResponse"/>.
    /// </summary>
    public ChannelReader<TResponse> Responses => this._responseReader;

    /// <inheritdoc cref="ChannelReader{T}.ReadAllAsync"/>
    public IAsyncEnumerable<TResponse> ReadAllResponsesAsync( CancellationToken ct ) => _responseReader.ReadAllAsync( ct );

    /// <inheritdoc cref="ChannelReader{T}.ReadAsync"/>
    public ValueTask<TResponse> ReadResponseAsync( CancellationToken ct ) => _responseReader.ReadAsync( ct );

    /// <inheritdoc cref="ChannelReader{T}.TryPeek"/>
    public bool TryPeekResponse( [ MaybeNullWhen( false ) ] out TResponse response ) => _responseReader.TryPeek( out response );

    /// <inheritdoc cref="ChannelReader{T}.TryRead"/>
    public bool TryReadResponse( [ MaybeNullWhen( false ) ] out TResponse response ) => _responseReader.TryRead( out response );

    /// <inheritdoc cref="ChannelReader{T}.WaitToReadAsync"/>
    public ValueTask<bool> WaitToReadResponseAsync( CancellationToken ct = default ) => _responseReader.WaitToReadAsync( ct );

    #endregion

    /* ************************************************** */

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
    /// <remarks>This returns <c>true</c> as if it had written regardless of if there was an actual reader to read it.</remarks>
    public override bool TryWrite( TData item ) {
        lock ( _readersLock ) {
            if ( _readers.Count == 0 ) { return true; } // this returns true as if it had written regardless of if there was an actual reader to read it

            if ( _readers.Count == 1 ) {
                return _readers[ 0 ].channelWriter.TryWrite( item );
            }

            bool result = true;
            foreach ( var (_, channelWriter) in _readers ) {
                result &= channelWriter.TryWrite( item );
            }

            return result;
        }
    }

    /// <inheritdoc />
    public override ValueTask<bool> WaitToWriteAsync( CancellationToken cancellationToken = default )
        => new ValueTask<bool>( true );

    /// <inheritdoc />
    /// <remarks>This runs slower than using <see cref="WaitToWriteAsync"/> and <see cref="TryWrite"/>.</remarks> 
    public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = default ) {
        lock ( _readersLock ) {
            if ( _readers.Count == 0 ) {
                return ValueTask.CompletedTask;
            }

            if ( _readers.Count == 1 ) {
                return _readers[ 0 ].channelWriter.WriteAsync( item, cancellationToken );
            }

            return _readers.Select( r => r.channelWriter.WriteAsync( item, cancellationToken ) ).ToArray().WhenAll();
        }
    }

    #endregion

    /* ************************************************** */
}

#endregion Writer

/* ********************************************************************************************* */

/// <summary>
/// A FIFO Queue that can have 1 publisher / writer, and 0 to many subscribers / readers.
/// With a return message channel with message type of <typeparamref name="TResponse"/>.
/// </summary>
/// <remarks>
/// If there are no readers currently, all write activity will simply return as if it was successful.
/// </remarks>
/// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels.channelwriter-1">ChannelWriter&lt;T&gt;</seealso>
/// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels.channelreader-1">ChannelReader&lt;T&gt;</seealso>
/// <seealso href="https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels.channel-1">Channel&lt;T&gt;</seealso>
public class BroadcastQueueWithLockedList<TData, TResponse> : IBroadcastQueueController<TData, TResponse> where TResponse : IBroadcastQueueResponse {
    protected BroadcastQueueWithLockedListWriter<TData, TResponse>? _writer;

    public virtual BroadcastQueueWithLockedListWriter<TData, TResponse> Writer {
        get {
            _writer ??= new BroadcastQueueWithLockedListWriter<TData, TResponse>(
                Channel.CreateUnbounded<TResponse>(
                    new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false }
                ) );

            return _writer;
        }
    }


    public virtual BroadcastQueueReader<TData, TResponse> GetReader( ) {
        return GetReader( Channel.CreateUnbounded<TData>( new UnboundedChannelOptions() { SingleReader = true, SingleWriter = true } ) );
    }

    protected virtual BroadcastQueueReader<TData, TResponse> GetReader( Channel<TData> dataChannel ) {
        var reader = new BroadcastQueueReader<TData, TResponse>( this, dataChannel.Reader, Writer.ResponseChannel.Writer );
        Writer.AddReader( reader, dataChannel.Writer );
        return reader;
    }

    public void RemoveReader( BroadcastQueueReader<TData, TResponse> reader ) {
        this.Writer.RemoveReader( reader );
    }

    /* ************************************************** */

    /// <inheritdoc />
    public override string ToString( ) {
        /* NOTE: Count does not work on _responseChannel.Reader as it is a SingleConsumerUnboundedChannel<T> which does not support Count
         * https://github.com/dotnet/runtime/blob/release/6.0/src/libraries/System.Threading.Channels/src/System/Threading/Channels/SingleConsumerUnboundedChannel.cs
         */
        return $"{nameof(BroadcastQueueWithLockedList<TData, TResponse>)} {{ "                                                                   +
               ( Writer.ResponseChannel.Reader.CanCount ? $"_responses {{ Count =  {Writer.ResponseChannel.Reader.Count} }}," : String.Empty ) +
               $"Reader {{ Count = {Writer.ReaderCount} }} }}";
    }
}