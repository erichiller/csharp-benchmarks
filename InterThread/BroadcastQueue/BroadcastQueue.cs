using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

using Microsoft.Extensions.DependencyInjection;

namespace Benchmarks.InterThread.BroadcastQueue;

/// <summary>
/// <see cref="BroadcastQueue{TData,TResponse}"/> with a default Response type of <see cref="System.Exception"/>.
/// </summary>
/// <typeparam name="TData"></typeparam>
public class BroadcastQueue<TData> : BroadcastQueue<TData, IBroadcastQueueResponse> { } // IBroadcastQueueResponse should always be the base!

public interface IBroadcastQueueResponse {
    public System.Exception? Exception { get; init; }
}

/* ********************************************************************************************* */

#region Writer

public class BroadcastQueueWriter<TData, TResponse> : ChannelWriter<TData>, IDisposable where TResponse : IBroadcastQueueResponse {
    private readonly  ChannelReader<TResponse> _responseReader;
    internal readonly Channel<TResponse>       ResponseChannel; // URGENT: fully move this to Writer, do not store in BroadcastQueue

    /* Only 1 or 2 threads ever use _readers:
     *  1. Write: The BroadcastQueue root through AddReader 
     *  2. Read: The BroadcastQueueWriter when it enumerates
     */

    protected        ImmutableArray<( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> channelWriter)> _readers     = ImmutableArray<(BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> channelWriter)>.Empty;
    private readonly object                                                                                               _readersLock = new object();

    public virtual int ReaderCount {
        get {
            lock ( _readersLock ) {
                return _readers.Length;
            }
        }
    }

    protected internal BroadcastQueueWriter( Channel<TResponse> responseChannel ) {
        ResponseChannel = responseChannel;
        _responseReader = responseChannel.Reader;
    }

    /* ************************************************** */

    protected internal virtual void AddReader( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> writer ) {
        lock ( _readersLock ) {
            _readers = _readers.Add( ( reader, writer ) );
        }
    }

    protected internal virtual void RemoveReader( BroadcastQueueReader<TData, TResponse> reader ) {
        lock ( _readersLock ) {
            var readerData = _readers.Single( t => t.reader == reader );
            _readers = _readers.Remove( readerData );
            readerData.channelWriter.TryComplete();
        }
    }

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
    public override ValueTask<bool> WaitToWriteAsync( CancellationToken cancellationToken = default )
        => new ValueTask<bool>( true );

    /// <inheritdoc />
    /// <remarks>This runs slower than using <see cref="WaitToWriteAsync"/> and <see cref="TryWrite"/>.</remarks> 
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

#endregion Writer

/* ********************************************************************************************* */

internal interface IBroadcastQueueController<TData, TResponse> {
    public void RemoveReader( BroadcastQueueReader<TData, TResponse> reader );
}

#region Reader

public class BroadcastQueueReader<TData, TResponse> : ChannelReader<TData>, IDisposable {
    private readonly IBroadcastQueueController<TData, TResponse> _queue;
    private readonly ChannelWriter<TResponse>                    _responseWriter;
    private readonly ChannelReader<TData>                        _dataReader;

    internal BroadcastQueueReader( IBroadcastQueueController<TData, TResponse> queue, ChannelReader<TData> dataReader, ChannelWriter<TResponse> responseWriter ) {
        this._queue          = queue;
        this._responseWriter = responseWriter;
        this._dataReader     = dataReader;
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
    
    /// <summary>
    /// Removes reader from BroadcastQueue
    /// </summary>
    /// <remarks>
    /// This method is only needed because if used in Dependency Injection, it might not be disposed when done using,
    /// which means the Channel would continually be written to without being read,
    /// wasting potentially significant amounts of memory.
    /// </remarks>
    public void Complete( ) {
        this._queue.RemoveReader( this );
    }
    
    /// <inheritdoc cref="Complete"/>
    public void Dispose( ) {
        this._queue.RemoveReader( this );
    }
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
public class BroadcastQueue<TData, TResponse> : IBroadcastQueueController<TData, TResponse> where TResponse : IBroadcastQueueResponse {
    protected BroadcastQueueWriter<TData, TResponse>? _writer;

    public virtual BroadcastQueueWriter<TData, TResponse> Writer {
        get {
            _writer ??= new BroadcastQueueWriter<TData, TResponse>(
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
        /* Count does not work on _responseChannel.Reader as it is a SingleConsumerUnboundedChannel<T> which does not support Count
         * https://github.com/dotnet/runtime/blob/release/6.0/src/libraries/System.Threading.Channels/src/System/Threading/Channels/SingleConsumerUnboundedChannel.cs
         */
        return $"{nameof(BroadcastQueue<TData, TResponse>)} {{ "                                                                               +
               ( Writer.ResponseChannel.Reader.CanCount ? $"_responses {{ Count =  {Writer.ResponseChannel.Reader.Count} }}," : String.Empty ) +
               $"Reader {{ Count = {Writer.ReaderCount} }} }}";
    }
}

/* TODO : add this to Common */

public static class ValueTaskExtensions {
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>This runs faster than using <c>Task.WhenAll( ...Select( ...AsTask ) )</c></remarks>
    public static async ValueTask WhenAll( this ValueTask[] tasks ) {
        // We don't allocate the list if no task throws
        List<Exception>? exceptions = null;

        for ( var i = 0 ; i < tasks.Length ; i++ )
            try {
                await tasks[ i ].ConfigureAwait( false );
            // } catch ( TaskCanceledException ) {
            //     // TODO: is this correct?
            //     return;
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
            // } catch ( TaskCanceledException _ ) {
            //     // TODO: is this correct?
            //     return results;
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
            // } catch ( TaskCanceledException _ ) {
            //     // TODO: is this correct?
            //     return results;
            } catch ( Exception ex ) {
                exceptions ??= new List<Exception>( tasks.Length );
                exceptions.Add( ex );
            }

        return exceptions is null
            ? results
            : throw new AggregateException( exceptions );
    }
}

public static class BroadcastQueueServiceExtensions {
    public static IServiceCollection AddBroadcastQueue<TData, TResponse>( this IServiceCollection services ) where TResponse : IBroadcastQueueResponse {
        services.AddSingleton<BroadcastQueue<TData, TResponse>>();
        // URGENT: this needs testing!
        services.AddTransient<BroadcastQueueWriter<TData, TResponse>>( sp => {
            BroadcastQueue<TData, TResponse> broadcastQueue = sp.GetService<BroadcastQueue<TData, TResponse>>() ?? throw new Exception( "Host exception" ); // TODO: replace this exception with something more specific.
            return broadcastQueue.Writer;
        } );
        // URGENT: this needs testing!
        // TODO: replace this exception with something more specific.
        services.AddTransient<BroadcastQueueReader<TData, TResponse>>( sp => ( sp.GetService<BroadcastQueue<TData, TResponse>>() ?? throw new Exception( "Host exception" ) ).GetReader() );
        return services;
    }
}