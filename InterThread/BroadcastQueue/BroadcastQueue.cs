using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Benchmarks.InterThread.BroadcastQueue;

/// <summary>
/// <see cref="BroadcastQueue{TData,TResponse}"/> with a default Response type of <see cref="System.Exception"/>.
/// </summary>
/// <typeparam name="TData"></typeparam>
public class BroadcastQueue<TData> : BroadcastQueue<TData, System.Exception> { }

/* ********************************************************************************************* */

#region Writer

// allow multiple?
public class BroadcastQueueWriter<TData, TResponse> : ChannelWriter<TData> {
    private readonly ChannelReader<TResponse> _responseReader;

    /* Only 1 or 2 threads ever user _readers:
     *  1. Write: The BroadcastQueue root through AddReader 
     *  2. Read: The BroadcastQueueWriter when it enumerates
     */
    private readonly List<( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> channelWriter)> _readers     = new (); // URGENT
    readonly         object                                                                                     _readersLock = new object();

    // private          ChannelWriter<TData>                                                                       _channelWriter; // KILL
    public int ReaderCount => _readers.Count;

    // private readonly BroadcastQueueReader<TData, TResponse>[] _readers; // URGENT
    // private BroadcastQueueReader<TData, TResponse> _reader;

    // internal BroadcastQueueWriter( BroadcastQueue<TData, TResponse> queue, ChannelReader<TResponse> responseReader ) {
    // _queue          = queue;
    internal BroadcastQueueWriter( ChannelReader<TResponse> responseReader ) {
        _responseReader = responseReader;
        // _readers        = new BroadcastQueueReader<TData, TResponse>[1];
    }

    /* ************************************************** */

    internal void AddReader( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> writer ) {
        _readers.Add( ( reader, writer ) ); // URGENT
        // _channelWriter = writer;
    }

    internal void RemoveReader( BroadcastQueueReader<TData, TResponse> reader ) {
        lock ( _readersLock ) {
            // URGENT
        }
        // TODO!!
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
        foreach ( ( _, ChannelWriter<TData> channelWriter ) in _readers ) {
            result &= channelWriter.TryComplete( error );
        }

        return result;
    }

    /// <inheritdoc />
    // [MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)] // KILL
    public override bool TryWrite( TData item ) {
        if ( _readers.Count == 1 ) {
            return _readers[ 0 ].channelWriter.TryWrite( item );
        }

        bool result = true;
        foreach ( var (_, channelWriter) in _readers ) {
            result &= channelWriter.TryWrite( item );
        }

        return result;
    }

    /// <inheritdoc />
    public override ValueTask<bool> WaitToWriteAsync( CancellationToken cancellationToken = default )
        => new ValueTask<bool>( true );

    /// <inheritdoc />
    /// <remarks>This runs slower than using <see cref="WaitToWriteAsync"/> and <see cref="TryWrite"/>. Especially when there is a single reader.</remarks> 
    public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = default ) {
        if ( _readers.Count == 0 ) {
            return ValueTask.CompletedTask;
        }

        if ( _readers.Count == 1 ) {
            return _readers[0].channelWriter.WriteAsync( item, cancellationToken );
        }

        return _readers.Select( r => r.channelWriter.WriteAsync( item, cancellationToken ) ).ToArray().WhenAll();
    }
    
    // KILL
    /// <inheritdoc />
    /// <remarks>This runs slower than using <see cref="WaitToWriteAsync"/> and <see cref="TryWrite"/>. Especially when there is a single reader.</remarks> 
    public ValueTask WriteAsyncAsTask( TData item, CancellationToken cancellationToken = default ) {
        if ( _readers.Count == 0 ) {
            return ValueTask.CompletedTask;
        }

        if ( _readers.Count == 1 ) {
            return _readers[0].channelWriter.WriteAsync( item, cancellationToken );
        }

        // return _readers.Select( r => r.channelWriter.WriteAsync( item, cancellationToken ) ).ToArray().WhenAll();
        return new ValueTask( Task.WhenAll( _readers.Select( r => r.channelWriter.WriteAsync( item, cancellationToken ).AsTask() ).ToArray()));
    }


    #endregion

    /* ************************************************** */
}

#endregion Writer

/* ********************************************************************************************* */

#region Reader

public class BroadcastQueueReader<TData, TResponse> : ChannelReader<TData> {
    private readonly BroadcastQueue<TData, TResponse> _queue;
    private readonly ChannelWriter<TResponse>         _responseWriter;
    private readonly ChannelReader<TData>             _dataReader;

    internal BroadcastQueueReader( BroadcastQueue<TData, TResponse> queue, ChannelReader<TData> dataReader, ChannelWriter<TResponse> responseWriter ) {
        this._queue          = queue;
        this._responseWriter = responseWriter;
        this._dataReader     = dataReader;
    }

    // URGENT
    private void unsubscribe( ) {
        // TODO: needs to be disposed?
        this._queue.RemoveReader( this );
    }

    /* ************************************************** */

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    public override bool TryRead( [ MaybeNullWhen( false ) ] out TData readResult ) => _dataReader.TryRead( out readResult );

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryPeek"/>
    public override bool TryPeek( [ MaybeNullWhen( false ) ] out TData readResult ) => _dataReader.TryPeek( out readResult );

    /// <inheritdoc />
    public override ValueTask<bool> WaitToReadAsync( CancellationToken ct = default ) => _dataReader.WaitToReadAsync( ct );

#pragma warning disable CS8424
    /// <inheritdoc />
    public override IAsyncEnumerable<TData> ReadAllAsync( [ EnumeratorCancellation ] CancellationToken cancellationToken = default )
        => _dataReader.ReadAllAsync( cancellationToken );
#pragma warning restore CS8424

    /// <inheritdoc cref="ChannelWriter{T}.WriteAsync" />
    public ValueTask WriteResponseAsync( TResponse response, CancellationToken cancellationToken = default ) => this._responseWriter.WriteAsync( response, cancellationToken );


    /// <inheritdoc />
    public override Task Completion => _dataReader.Completion;

    /// <inheritdoc />
    public override int Count => _dataReader.Count;

    /// <inheritdoc />
    public override bool CanCount => _dataReader.CanCount;

    /// <inheritdoc />
    public override bool CanPeek => _dataReader.CanPeek;
}

#endregion Reader

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
public class BroadcastQueue<TData, TResponse> {
    protected readonly Channel<TResponse> _responseChannel;

    public BroadcastQueue( ) {
        _responseChannel = Channel.CreateUnbounded<TResponse>(
            new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false } );
        Writer = new BroadcastQueueWriter<TData, TResponse>( _responseChannel.Reader );
    }

    protected BroadcastQueue( Channel<TResponse> responseChannel ) {
        _responseChannel = responseChannel;
        Writer           = new BroadcastQueueWriter<TData, TResponse>( _responseChannel.Reader );
    }

    public virtual BroadcastQueueReader<TData, TResponse> GetReader( ) {
        return GetReader( Channel.CreateUnbounded<TData>( new UnboundedChannelOptions() { SingleReader = true, SingleWriter = true } ) );
    }

    protected virtual BroadcastQueueReader<TData, TResponse> GetReader( Channel<TData> dataChannel ) {
        var reader = new BroadcastQueueReader<TData, TResponse>( this, dataChannel.Reader, _responseChannel.Writer );
        Writer.AddReader( reader, dataChannel.Writer );
        return reader;
    }

    public BroadcastQueueWriter<TData, TResponse> Writer { get; private init; }

    internal void RemoveReader( BroadcastQueueReader<TData, TResponse> reader ) {
        // this._readers.RemoveReader( reader, out var _ );
        this.Writer.RemoveReader( reader );
    }

    /* ************************************************** */

    /// <inheritdoc />
    public override string ToString( ) {
        /* NOTE: Count does not work on _responseChannel.Reader as it is a SingleConsumerUnboundedChannel<T> which does not support Count
         * https://github.com/dotnet/runtime/blob/release/6.0/src/libraries/System.Threading.Channels/src/System/Threading/Channels/SingleConsumerUnboundedChannel.cs
         */
        return $"{nameof(BroadcastQueue<TData, TResponse>)} {{ "                                                                   +
               ( _responseChannel.Reader.CanCount ? $"_responses {{ Count =  {_responseChannel.Reader.Count} }}," : String.Empty ) +
               "_readers {{ Count = {_readers.Count} }} }}";
    }
}

/* TODO : add this to Common */

public static class ValueTaskExtensions {
    public static async ValueTask WhenAll( this ValueTask[] tasks ) {
        // URGENT: benchmark this vs. using .AsTask()
        // We don't allocate the list if no task throws
        List<Exception>? exceptions = null;

        for ( var i = 0 ; i < tasks.Length ; i++ )
            try {
                await tasks[ i ].ConfigureAwait( false );
            } catch ( TaskCanceledException _ ) {
                // TODO: is this correct?
                return;
            } catch ( Exception ex ) {
                exceptions ??= new List<Exception>( tasks.Length );
                exceptions.Add( ex );
            }

        // return exceptions is null
        // ? results
        if ( exceptions is not null ) {
            throw new AggregateException( exceptions );
        }
    }

    public static async ValueTask<T[]> WhenAll<T>( this ValueTask<T>[] tasks ) {
        // URGENT: benchmark this vs. using .AsTask()
        // We don't allocate the list if no task throws
        List<Exception>? exceptions = null;

        var results = new T[ tasks.Length ];
        for ( var i = 0 ; i < tasks.Length ; i++ )
            try {
                results[ i ] = await tasks[ i ].ConfigureAwait( false );
            } catch ( TaskCanceledException _ ) {
                // TODO: is this correct?
                return results;
            } catch ( Exception ex ) {
                exceptions ??= new List<Exception>( tasks.Length );
                exceptions.Add( ex );
            }

        return exceptions is null
            ? results
            : throw new AggregateException( exceptions );
    }

    /// <summary>
    /// For a given array of <see cref="ValueTask"/>s and their results into a single return.
    /// </summary>
    public static async ValueTask<bool> WhenAllAnd( this ValueTask<bool>[] tasks ) {
        // URGENT: benchmark this vs. using .AsTask()
        // We don't allocate the list if no task throws
        List<Exception>? exceptions = null;

        bool results = true;
        for ( var i = 0 ; i < tasks.Length ; i++ )
            try {
                results &= await tasks[ i ].ConfigureAwait( false );
            } catch ( TaskCanceledException _ ) {
                // TODO: is this correct?
                return results;
            } catch ( Exception ex ) {
                exceptions ??= new List<Exception>( tasks.Length );
                exceptions.Add( ex );
            }

        return exceptions is null
            ? results
            : throw new AggregateException( exceptions );
    }
}