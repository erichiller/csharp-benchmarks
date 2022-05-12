using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Benchmarks.InterThreadBroadcast;

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
    public bool TryPeekResponse( out TResponse? response ) => _responseReader.TryPeek( out response );

    /// <inheritdoc cref="ChannelReader{T}.TryRead"/>
    public bool TryReadResponse( [MaybeNullWhen(false)] out TResponse? response ) => _responseReader.TryRead( out response );

    /// <inheritdoc cref="ChannelReader{T}.WaitToReadAsync"/>
    public ValueTask<bool> WaitToReadResponseAsync( CancellationToken ct ) => _responseReader.WaitToReadAsync( ct );

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
/// With a return message channel.
///
/// // TODO : just call Broadcast ?
/// SEE: https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels.channelwriter-1?view=net-6.0
/// https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels.channelreader-1?view=net-6.0
/// https://docs.microsoft.com/en-us/dotnet/api/system.threading.channels.channel-1?view=net-6.0
/// </summary>
public class BroadcastQueue<TData, TResponse> : ChannelWriter<TData> {
    // private System.Collections.Concurrent.<T>
    // private ConcurrentQueue<TData> _messages = new ();

    // private ConcurrentQueue<TResponse> _responses = new ();

    private Channel<TResponse>       _responseChannel;
    private ChannelReader<TResponse> _responseReader;
    // private Channel<TData>                         _dataChannel;
    // private ChannelWriter<TData>                   _dataWriter;

    // reader => next message to be read position
    private readonly Dictionary<BroadcastQueueReader<TData, TResponse>, ChannelWriter<TData>> _readers = new ();

    public BroadcastQueue( ) {
        UnboundedChannelOptions responseChannelOptions = new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false };
        _responseChannel = Channel.CreateUnbounded<TResponse>( responseChannelOptions );
        _responseReader  = _responseChannel.Reader;
        Writer           = new BroadcastQueueWriter<TData, TResponse>( this, _responseReader );
    }

    public BroadcastQueueReader<TData, TResponse> GetReader( ) {
        var            dataChannelOptions = new UnboundedChannelOptions() { SingleReader = true, SingleWriter = true };
        Channel<TData> dataChannel        = Channel.CreateUnbounded<TData>( dataChannelOptions );
        var            reader             = new BroadcastQueueReader<TData, TResponse>( this, dataChannel.Reader, _responseChannel.Writer );
        _readers.Add( reader, dataChannel.Writer );
        return reader;
    }

    public BroadcastQueueWriter<TData, TResponse> Writer { get; }

    internal void RemoveReader( BroadcastQueueReader<TData, TResponse> reader ) {
        this._readers.Remove( reader );
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
    public override bool TryWrite( TData item ) {
        bool result = true;
        foreach ( var (_, channelWriter) in _readers ) {
            result &= channelWriter.TryWrite( item );
        }

        return result;
    }

    /// <inheritdoc />
    public override ValueTask<bool> WaitToWriteAsync( CancellationToken cancellationToken = default ) {
        if ( _readers.Count == 1 ) {
            return _readers.Single().Value.WaitToWriteAsync( cancellationToken );
        }

        return _readers.Select( r => r.Value.WaitToWriteAsync( cancellationToken ) ).ToArray().WhenAllAnd();
    }
    // public override async ValueTask<bool> WaitToWriteAsync( CancellationToken cancellationToken = default ) {
    //     bool result = true;
    //     foreach ( var (reader, channelWriter) in _readers ) {
    //         result &= await channelWriter.WaitToWriteAsync( cancellationToken );
    //     }
    //
    //     return result;
    // }

    /// <inheritdoc />
    public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = default ) {
        if ( _readers.Count == 1 ) {
            return _readers.Single().Value.WriteAsync( item, cancellationToken );
        }

        return _readers.Select( r => r.Value.WriteAsync( item, cancellationToken ) ).ToArray().WhenAll();
    }

    #endregion

    /// <inheritdoc />
    public override string ToString( ) {
        /* NOTE: Count does not work on _responseReader.
         * My suspicion is that it is a SingleConsumerUnboundedChannel<T>
         * which does not support Count
         * https://source.dot.net/#System.Threading.Channels/System/Threading/Channels/SingleConsumerUnboundedChannel.cs
         */
        // return $"{nameof(BroadcastQueue<TData, TResponse>)} {{ _responses {{ Count = {_responseReader.Count} }}, _readers {{ Count = {_readers.Count} }} }}";
        return $"{nameof(BroadcastQueue<TData, TResponse>)} {{ _readers {{ Count = {_readers.Count} }} }}";
    }
}

/* TODO : add this to Common */

public static class ValueTaskExtensions {
    public static async ValueTask WhenAll( this ValueTask[] tasks ) {
        // We don't allocate the list if no task throws
        List<Exception>? exceptions = null;

        for ( var i = 0 ; i < tasks.Length ; i++ )
            try {
                await tasks[ i ].ConfigureAwait( false );
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
        // We don't allocate the list if no task throws
        List<Exception>? exceptions = null;

        var results = new T[ tasks.Length ];
        for ( var i = 0 ; i < tasks.Length ; i++ )
            try {
                results[ i ] = await tasks[ i ].ConfigureAwait( false );
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
        // We don't allocate the list if no task throws
        List<Exception>? exceptions = null;

        bool results = true;
        for ( var i = 0 ; i < tasks.Length ; i++ )
            try {
                results &= await tasks[ i ].ConfigureAwait( false );
            } catch ( Exception ex ) {
                exceptions ??= new List<Exception>( tasks.Length );
                exceptions.Add( ex );
            }

        return exceptions is null
            ? results
            : throw new AggregateException( exceptions );
    }
}