
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace Benchmarks.InterThread.BroadcastQueue;

#region WriterImmutableArray

public class BroadcastQueueImmutableArrayWriter<TData, TResponse /*, TReader */> : ChannelWriter<TData>
    // where TReader : IReadOnlyCollection<( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> channelWriter)> 
{
    private readonly ChannelReader<TResponse> _responseReader;

    /* Only 1 or 2 threads ever use _readers:
     *  1. Write: The BroadcastQueue root through AddReader 
     *  2. Read: The BroadcastQueueWriter when it enumerates
     */
    // URGENT: trying ImmutableArray -- THIS NEEDS TO BE TESTED FOR THREAD SAFETY
    /* URGENT
     * Try writing a test where one Task is constantly writing and another is periodically adding and removing Readers. Run it for a long time!
     */
    // URGENT:      * Reader needs to be disposable ??
    private ImmutableArray<( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> channelWriter)> _readers = ImmutableArray<(BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> channelWriter)>.Empty; // URGENT

    public int ReaderCount => _readers.Length;


    protected internal BroadcastQueueImmutableArrayWriter( ChannelReader<TResponse> responseReader ) {
        _responseReader = responseReader;
    }

    /* ************************************************** */

    internal void AddReader( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> writer ) {
        _readers= _readers.Add( ( reader, writer ) ); // URGENT
        
        // _readers = ( (_readers as ImmutableArray<( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> channelWriter)>)!).Add( ( reader, writer ) ); // URGENT

    }

    internal void RemoveReader( BroadcastQueueReader<TData, TResponse> reader ) {
        // lock ( _readersLock ) {
        // URGENT
        // }
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
    public override bool TryWrite( TData item ) {
        if ( _readers.Length == 1 ) {
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
    /// <remarks>This runs slower than using <see cref="WaitToWriteAsync"/> and <see cref="TryWrite"/>.</remarks> 
    public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = default ) {
        if ( _readers.Length == 0 ) {
            return ValueTask.CompletedTask;
        }

        if ( _readers.Length == 1 ) {
            return _readers[ 0 ].channelWriter.WriteAsync( item, cancellationToken );
        }

        return _readers.Select( r => r.channelWriter.WriteAsync( item, cancellationToken ) ).ToArray().WhenAll();
    }

    #endregion

    /* ************************************************** */
}

#endregion WriterImmutableArray


/* ************* */
public class BroadcastQueueWithImmutableArrayWriter<TData, TResponse> : IBroadcastQueueController<TData, TResponse> {
    protected readonly Channel<TResponse>                                  _responseChannel;
    public             BroadcastQueueImmutableArrayWriter<TData, TResponse> Writer { get; private init; }

    public BroadcastQueueWithImmutableArrayWriter( ) {
        _responseChannel = Channel.CreateUnbounded<TResponse>(
            new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false }
        );
        Writer = new BroadcastQueueImmutableArrayWriter<TData, TResponse>( _responseChannel.Reader );
    }

    public virtual BroadcastQueueReader<TData, TResponse> GetReader( ) {
        return GetReader( Channel.CreateUnbounded<TData>( new UnboundedChannelOptions() { SingleReader = true, SingleWriter = true } ) );
    }

    protected virtual BroadcastQueueReader<TData, TResponse> GetReader( Channel<TData> dataChannel ) {
        var reader = new BroadcastQueueReader<TData, TResponse>( this, dataChannel.Reader, _responseChannel.Writer );
        Writer.AddReader( reader, dataChannel.Writer );
        return reader;
    }


    public void RemoveReader( BroadcastQueueReader<TData, TResponse> reader ) {
        // this._readers.RemoveReader( reader, out var _ );
        this.Writer.RemoveReader( reader );
    }

    /* ************************************************** */

    /// <inheritdoc />
    public override string ToString( ) {
        /* NOTE: Count does not work on _responseChannel.Reader as it is a SingleConsumerUnboundedChannel<T> which does not support Count
         * https://github.com/dotnet/runtime/blob/release/6.0/src/libraries/System.Threading.Channels/src/System/Threading/Channels/SingleConsumerUnboundedChannel.cs
         */
        return $"{nameof(BroadcastQueueWithImmutableArrayWriter<TData, TResponse>)} {{ "                                                                   +
               ( _responseChannel.Reader.CanCount ? $"_responses {{ Count =  {_responseChannel.Reader.Count} }}," : String.Empty ) +
               "_readers {{ Count = {_readers.Count} }} }}";
    }
}