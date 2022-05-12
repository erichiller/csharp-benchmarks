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
    public bool TryReadResponse( out TResponse? response ) => _responseReader.TryRead( out response );

    /// <inheritdoc cref="ChannelReader{T}.WaitToReadAsync"/>
    public ValueTask<bool> WaitToReadResponseAsync( CancellationToken ct ) => _responseReader.WaitToReadAsync( ct );

    #endregion

    /* ************************************************** */

    /* ************************************************** */

    #region Data

    /// <inheritdoc />
    public override bool TryComplete( Exception? error = null ) => _dataWriter.TryComplete( error );

    /// <inheritdoc />
    public override bool TryWrite( TData item ) => _dataWriter.TryWrite( item );

    /// <inheritdoc />
    public override ValueTask<bool> WaitToWriteAsync( CancellationToken cancellationToken = default )
        => _dataWriter.WaitToWriteAsync( cancellationToken );

    /// <inheritdoc />
    public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = default )
        => _dataWriter.WriteAsync( item, cancellationToken );

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
    public ValueTask WriteResponseAsync( TResponse response ) => this._responseWriter.WriteAsync( response );


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
        foreach ( var (reader, channelWriter) in _readers ) {
            result &= channelWriter.TryComplete( error ); 
        }

        return result;
    }

    /// <inheritdoc />
    public override bool TryWrite( TData item ) {
        bool result = true;
        foreach ( var (reader, channelWriter) in _readers ) {
            result &= channelWriter.TryWrite( item ); 
        }

        return result;
    }

    /// <inheritdoc />
    public override async ValueTask<bool> WaitToWriteAsync( CancellationToken cancellationToken = default ) {
        bool result = true;
        foreach ( var (reader, channelWriter) in _readers ) {
            result &= await channelWriter.WaitToWriteAsync( cancellationToken );
        }

        return result;
    }

    /// <inheritdoc />
    public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = default ) {
        if ( _readers.Count == 1 ) {
            return _readers.Single().Value.WriteAsync( item, cancellationToken );
        }

        _readers.Select( r => r.Value.WriteAsync( item, cancellationToken ) ).ToArray().WhenAll();
    }

    #endregion

    // internal TData Read( BroadcastQueueReader<TData, TResponse> reader ) {
    //     if ( this._messages.Count > this._readers[ reader ] + 1 ) {
    //         // read here
    //     } else {
    //         // nothing is available to be read
    //         // TODO: block??
    //     }
    //
    //     this._readers[ reader ] += 1;
    //     var message = this._messages[ this._readers[ reader ] ];
    //     if ( allHaveReadLast() ) {
    //         decrementAllNextMessagePosition();
    //         this._messages.RemoveAt( 0 );
    //     }
    //
    //     return message;
    // }

    // internal IEnumerable<TData> ReadAll( BroadcastQueueReader<TData, TResponse> reader ) {
    //     List<TData> responses = new List<TData>();
    //     while ( true ) {
    //         if ( this._messages.Count > this._readers[ reader ] + 1 ) { } else {
    //             break;
    //             // nothing is available to be read
    //             // TODO: block??
    //         }
    //
    //         this._readers[ reader ] += 1;
    //         responses.Add( this._messages[ this._readers[ reader ] ] );
    //         if ( allHaveReadLast() ) {
    //             decrementAllNextMessagePosition();
    //             this._messages.RemoveAt( 0 );
    //         }
    //     }
    //
    //     return responses;
    // }

    // public async ValueTask<bool> WaitToReadAsync( BroadcastQueueReader<TData, TResponse> reader, CancellationToken ct ) 


    // public bool TryRead( BroadcastQueueReader<TData, TResponse> reader, [ MaybeNullWhen( false ) ] out TData? response ) {
    //     if ( this._messages.Count > this._readers[ reader ] + 1 ) {
    //         this._readers[ reader ] += 1;
    //         var message = this._messages[ this._readers[ reader ] ];
    //         if ( allHaveReadLast() ) {
    //             decrementAllNextMessagePosition();
    //             this._messages.RemoveAt( 0 );
    //         }
    //
    //         response = message;
    //         return true;
    //         // read here
    //     }
    //
    //     // nothing is available to be read
    //     response = default(TData);
    //
    //     return false;
    // }

    // internal void Write( TData data ) {
    //     this._messages.Add( data );
    //     // TODO: notify readers
    // }
    //
    // internal void WriteAll( IEnumerable<TData> data ) {
    //     this._messages.AddRange( data );
    //     // TODO: notify readers
    // }

    // internal void WriteResponse( TResponse response ) => _responses.Enqueue( response );

    // internal void WriteResponse( BroadcastQueueReader<TData, TResponse> reader, TResponse response ) {
    //     this._responses.Add( new BroadcastQueueResponse<TData, TResponse>( reader, response ) );
    //     // TODO: notify readers
    // }

    // internal void WriteAllResponses( BroadcastQueueReader<TData, TResponse> reader, IEnumerable<TResponse> responses ) {
    //     this._responses.AddRange( responses.Select( response => new BroadcastQueueResponse<TData, TResponse>( reader, response ) ) );
    //     // TODO: notify readers
    // }
    //
    //
    // private bool allHaveReadLast( ) {
    //     foreach ( var (reader, nextMessage) in this._readers ) {
    //         if ( nextMessage == 0 ) {
    //             return false;
    //         }
    //     }
    //
    //     return true;
    // }
    //
    // private void decrementAllNextMessagePosition( ) {
    //     var readers = this._readers.Keys;
    //     foreach ( var reader in readers ) {
    //         this._readers[ reader ] -= 1;
    //     }
    // }


    /// <inheritdoc />
    public override string ToString( ) {
        return $"{nameof(BroadcastQueue<TData, TResponse>)} {{ _responses {{ Count = {_responseReader.Count} }}, _readers {{ Count = {_readers.Count} }} }}";
    }
}


/* TODO : add this to Common */

public static class ValueTaskExtensions {
    // public static ValueTask<T[]> WhenAll<T>( this ValueTask<T>[] tasks ) => WhenAll( tasks ); 
    // public static async ValueTask<T[]> WhenAll<T>( params ValueTask<T>[] tasks ) {
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
}