using System.Collections.Concurrent;
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
    private readonly ChannelReader<TResponse>         _responseReader;
    private readonly BroadcastQueue<TData, TResponse> _queue;

    internal BroadcastQueueWriter( BroadcastQueue<TData, TResponse> queue, ChannelReader<TResponse> responseReader ) {
        _queue          = queue;
        _responseReader = responseReader;
    }

    /* ************************************************** */

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
    public override bool TryComplete( Exception? error = null ) => _queue.TryComplete( error );

    /// <inheritdoc />
    public override bool TryWrite( TData item ) => _queue.TryWrite( item );

    /// <inheritdoc />
    public override ValueTask<bool> WaitToWriteAsync( CancellationToken cancellationToken = default )
        => _queue.WaitToWriteAsync( cancellationToken );

    /// <inheritdoc />
    public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = default )
        => _queue.WriteAsync( item, cancellationToken );

    #endregion

    /* ************************************************** */
}

#endregion Writer

/* ********************************************************************************************* */

#region Reader

public class BroadcastQueueReader<TData, TResponse> : ChannelReader<TData> {
    private readonly BroadcastQueue<TData, TResponse> _queue;
    private readonly ChannelWriter<TResponse>         _responseWriter;
    private readonly ChannelReader<TData>             _messageReader;

    internal BroadcastQueueReader( BroadcastQueue<TData, TResponse> queue, ChannelReader<TData> messageReader, ChannelWriter<TResponse> responseWriter ) {
        this._queue          = queue;
        this._responseWriter = responseWriter;
        this._messageReader  = messageReader;
    }

    // URGENT
    private void unsubscribe( ) {
        // TODO: needs to be disposed?
        this._queue.RemoveReader( this );
    }

    /* ************************************************** */

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    public override bool TryRead( [ MaybeNullWhen( false ) ] out TData readResult ) => _messageReader.TryRead( out readResult );

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryPeek"/>
    public override bool TryPeek( [ MaybeNullWhen( false ) ] out TData readResult ) => _messageReader.TryPeek( out readResult );

    /// <inheritdoc />
    public override ValueTask<bool> WaitToReadAsync( CancellationToken ct = default ) => _messageReader.WaitToReadAsync( ct );

#pragma warning disable CS8424
    /// <inheritdoc />
    public override IAsyncEnumerable<TData> ReadAllAsync( [ EnumeratorCancellation ] CancellationToken cancellationToken = default )
        => _messageReader.ReadAllAsync( cancellationToken );
#pragma warning restore CS8424

    /// <inheritdoc cref="ChannelWriter{T}.WriteAsync" />
    public ValueTask WriteResponseAsync( TResponse response, CancellationToken cancellationToken = default ) => this._responseWriter.WriteAsync( response, cancellationToken );


    /// <inheritdoc />
    public override Task Completion => _messageReader.Completion;

    /// <inheritdoc />
    public override int Count => _messageReader.Count;

    /// <inheritdoc />
    public override bool CanCount => _messageReader.CanCount;

    /// <inheritdoc />
    public override bool CanPeek => _messageReader.CanPeek;
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
public class BroadcastQueue<TData, TResponse> : ChannelWriter<TData> {
    protected readonly Channel<TResponse> _responseChannel;

    protected readonly ConcurrentDictionary<BroadcastQueueReader<TData, TResponse>, ChannelWriter<TData>> _readers = new ();

    protected BroadcastQueueReader<TData, TResponse>? _singleReaderOnly; // KILL

    public int ReaderCount => _readers.Count;

    public BroadcastQueue( ) {
        _responseChannel = Channel.CreateUnbounded<TResponse>(
            new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false } ); // URGENT: restore?
        // _responseChannel = Channel.CreateUnbounded<TResponse>();
        Writer = new BroadcastQueueWriter<TData, TResponse>( this, _responseChannel.Reader );
    }

    protected BroadcastQueue( Channel<TResponse> responseChannel ) {
        _responseChannel = responseChannel;
        Writer           = new BroadcastQueueWriter<TData, TResponse>( this, _responseChannel.Reader );
    }

    public virtual BroadcastQueueReader<TData, TResponse> GetReader( ) {
        // var            dataChannelOptions = new UnboundedChannelOptions() { SingleReader = true, SingleWriter = true };// URGENT: restore?
        // Channel<TData> dataChannel = Channel.CreateUnbounded<TData>( dataChannelOptions );// URGENT: restore?
        // Channel<TData> dataChannel = Channel.CreateUnbounded<TData>();
        return GetReader( Channel.CreateUnbounded<TData>( new UnboundedChannelOptions() { SingleReader = true, SingleWriter = true } ) );
    }

    protected virtual BroadcastQueueReader<TData, TResponse> GetReader( Channel<TData> dataChannel ) {
        var reader = new BroadcastQueueReader<TData, TResponse>( this, dataChannel.Reader, _responseChannel.Writer );
        _singleReaderOnly = reader;
        _readers.TryAdd( reader, dataChannel.Writer );
        return reader;
    }

    public BroadcastQueueWriter<TData, TResponse> Writer { get; private init; }

    internal void RemoveReader( BroadcastQueueReader<TData, TResponse> reader ) {
        this._readers.TryRemove( reader, out var _ );
    }

    /* ************************************************** */

    #region Data

    /// <inheritdoc />
    public override bool TryComplete( Exception? error = null ) {
        bool result = true;
        foreach ( var (_, channelWriter) in _readers ) {
            result &= channelWriter.TryComplete( error );
        }

        return result;
    }

    /// <inheritdoc />
    /// <remarks>Will return <c>true</c> if there are no readers present.</remarks>
    public override bool TryWrite( TData item ) {
        bool result = true;
        foreach ( var (_, channelWriter) in _readers ) {
            result &= channelWriter.TryWrite( item );
        }

        return result;
    }

    /// <inheritdoc />
    public override ValueTask<bool> WaitToWriteAsync( CancellationToken cancellationToken = default ) {
        if ( _readers.Count == 0 ) {
            /* NOTE: I am not 100% sure this is the desired result
             * There are no readers, and the data is not being saved to anywhere, so should this return False?
             * The docs for WaitToWriteAsync say:
             *  return "true result when space is available to write an item"
             *  return "false result when no further writing will be permitted."
             * Alternative:
             *  - Prevent return until 1 or more readers are connected? **<<THIS IS THE CURRENT BEHAVIOR>>**
             *  - Throw an exception
             *  - Save all written data to a channel
             *      The problem with saving it to a channel is what to do with it when a reader _does_ connect.
             *      There may be several readers connecting, but whatever the first one was would get the historical data
             *      The second+ would not.
             *          Alternatively,
             *          Save all data to a limited size Concurrent-safe collection and have this always receive data
             *          Then use this to feed any new readers the last X data.
             *          *This is entirely unlike how Channel<T> works and is more like a message bus, eg. RabbitMQ*
             */
            // Console.WriteLine( $"_readers.Count == 0 ; Waiting for subscriber to connect." );
            while ( _readers.Count == 0 ) {
                /* Pause and wait for a reader to connect. */
                /* NOTE: I don't know why, but NOT awaiting this keeps the CPU from being pegged at 100% (But it doesn't actually pause for 500ms)
                 * see: https://stackoverflow.com/a/28413138/377252
                 */
                Task.Delay( 1000, cancellationToken );
            }

            return ValueTask.FromResult( true );
        }

        if ( _readers.Count == 1 ) {
            return _readers.Single().Value.WaitToWriteAsync( cancellationToken );
        }

        return _readers.Select( r => r.Value.WaitToWriteAsync( cancellationToken ) ).ToArray().WhenAllAnd();
    }

    /// <inheritdoc />
    /// <remarks>If there are no readers present, nothing will be written</remarks>
    public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = default ) {
        if ( _readers.Count == 0 ) {
            return ValueTask.CompletedTask;
        }

        if ( _readers.Count == 1 ) {
            return _readers.Single().Value.WriteAsync( item, cancellationToken );
            // return _readers[ 0 ].WriteAsync( item, cancellationToken );
        }

        return _readers.Select( r => r.Value.WriteAsync( item, cancellationToken ) ).ToArray().WhenAll();
    }

    #endregion

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
            } catch (TaskCanceledException _) { // TODO: is this correct?
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
            } catch (TaskCanceledException _) { // TODO: is this correct?
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
            } catch (TaskCanceledException _) { // TODO: is this correct?
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