using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace Benchmarks.InterThread.BroadcastQueue;

#region WriterLockArray

public class BroadcastQueueLockArrayWriter<TData, TResponse /*, TReader */> : ChannelWriter<TData>
    // where TReader : IReadOnlyCollection<( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> channelWriter)> 
{
    private readonly ChannelReader<TResponse> _responseReader;

    protected          ( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> channelWriter)[] _readers     = Array.Empty<(BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> channelWriter)>();
    protected readonly object                                                                                 _readersLock = new object();

    public virtual int ReaderCount {
        get {
            lock ( _readersLock ) {
                return _readers.Length;
            }
        }
    }


    protected internal BroadcastQueueLockArrayWriter( ChannelReader<TResponse> responseReader ) {
        _responseReader = responseReader;
    }

    /* ************************************************** */

    internal void AddReader( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> writer ) {
        ( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> channelWriter)[] newReaders
            = new ( BroadcastQueueReader<TData, TResponse> reader, ChannelWriter<TData> channelWriter)[ _readers.Length + 1 ];
        newReaders[ ^1 ] = ( reader, writer );
        // _readers        = Array.Copy( _readers, sourceIndex: 0, destinationIndex:  );
        lock ( _readersLock ) {
            // Array.Copy( sourceArray: _readers, destinationArray: newReaders, sourceIndex: 0, destinationIndex: _readers.Length );
            Array.Copy( sourceArray: _readers, destinationArray: newReaders, length: _readers.Length );
            // Console.WriteLine( $"readers.Length={_readers.Length} ; newReaders.Length={newReaders.Length}" );
            _readers = newReaders;
        }

        // Console.WriteLine( $"COMPLETE. readers.Length={_readers.Length} ; newReaders.Length={newReaders.Length}" );
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
            // Console.WriteLine( $"TryWrite( TData item )={item} ; _readers.Length={_readers.Length}" );
            if ( _readers.Length == 1 ) {
                return _readers[ 0 ].channelWriter.TryWrite( item );
            }

            bool result = true;
            // foreach ( var (reader, channelWriter) in _readers ) {
            foreach ( var (_, channelWriter) in _readers ) {
                // Console.WriteLine( $"IN LOOP. TryWrite( TData item )={item} ; reader={reader}" );
                result &= channelWriter.TryWrite( item );
            }
            
            // Console.WriteLine( $"COMPLETE. TryWrite( TData item )={item}" );
            return result;
        }
    }
    
    
    public bool TryWriteForLoop( TData item ) {
        lock ( _readersLock ) {
            Console.WriteLine( $"TryWrite( TData item )={item} ; _readers.Length={_readers.Length}" );
            if ( _readers.Length == 1 ) {
                Console.WriteLine( $"TryWrite( TData item )={item} ; _readers.Length is 1" );
                return _readers[ 0 ].channelWriter.TryWrite( item );
            }

            bool result = true;
            // foreach ( var (reader, channelWriter) in _readers ) {
            // result &= channelWriter.TryWrite( item );
                for ( int i = 0 ; i < _readers.Length ; i ++ ){
                Console.WriteLine( $"IN LOOP. TryWrite( TData item )={item} ; reader={_readers[0].reader}" );
                result &= _readers[i].channelWriter.TryWrite( item );
                
            }
            
            // Console.WriteLine( $"COMPLETE. TryWrite( TData item )={item}" );
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

#endregion WriterLockArray

/* ************* */
public class BroadcastQueueWithLockArrayWriter<TData, TResponse> : IBroadcastQueueController<TData, TResponse> where TResponse : IBroadcastQueueResponse {
    protected readonly Channel<TResponse>                              _responseChannel;
    
    
    protected BroadcastQueueLockArrayWriter<TData, TResponse>? _writer;

    public virtual BroadcastQueueLockArrayWriter<TData, TResponse> Writer {
        get {
            _writer ??= new BroadcastQueueLockArrayWriter<TData, TResponse>( _responseChannel.Reader );
            return _writer;
        }
    }
    
    

    public BroadcastQueueWithLockArrayWriter( ) {
        _responseChannel = Channel.CreateUnbounded<TResponse>(
            new UnboundedChannelOptions() { SingleReader = true, SingleWriter = false }
        );
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
        return $"{nameof(BroadcastQueueWithLockArrayWriter<TData, TResponse>)} {{ "                                                +
               ( _responseChannel.Reader.CanCount ? $"_responses {{ Count =  {_responseChannel.Reader.Count} }}," : String.Empty ) +
               "_readers {{ Count = {_readers.Count} }} }}";
    }
}

/* ******************* */


public class BroadcastQueueLockArrayForLoopWriter<TData, TResponse> : BroadcastQueueLockArrayWriter<TData, TResponse> {
    
    protected internal BroadcastQueueLockArrayForLoopWriter( ChannelReader<TResponse> responseReader ) : base( responseReader ) { }
    
    
    /// <inheritdoc />
    public override bool TryWrite( TData item ) {
        lock ( _readersLock ) {
            // Console.WriteLine( $"TryWrite( TData item )={item} ; _readers.Length={_readers.Length}" );
            if ( _readers.Length == 1 ) {
                // Console.WriteLine( $"TryWrite( TData item )={item} ; _readers.Length is 1" );
                return _readers[ 0 ].channelWriter.TryWrite( item );
            }

            bool result = true;
            // foreach ( var (reader, channelWriter) in _readers ) {
            // result &= channelWriter.TryWrite( item );
            for ( int i = 0 ; i < _readers.Length ; i ++ ){
                // Console.WriteLine( $"IN LOOP. TryWrite( TData item )={item} ; reader={_readers[0].reader}" );
                result &= _readers[i].channelWriter.TryWrite( item );
                
            }
            
            // Console.WriteLine( $"COMPLETE. TryWrite( TData item )={item}" );
            return result;
        }
    }
    
    
    // public override bool TryWrite( TData item ) {
    //     lock ( _readersLock ) {
    //         // Console.WriteLine( $"TryWrite( TData item )={item} ; _readers.Length={_readers.Length}" );
    //         if ( _readers.Length == 1 ) {
    //             return _readers[ 0 ].channelWriter.TryWrite( item );
    //         }
    //
    //         bool result = true;
    //         // foreach ( var (reader, channelWriter) in _readers ) {
    //         foreach ( var (_, channelWriter) in _readers ) {
    //             // Console.WriteLine( $"IN LOOP. TryWrite( TData item )={item} ; reader={reader}" );
    //             result &= channelWriter.TryWrite( item );
    //         }
    //         
    //         // Console.WriteLine( $"COMPLETE. TryWrite( TData item )={item}" );
    //         return result;
    //     }
    // }
    
}


public class BroadcastQueueWithLockArrayForLoopWriter<TData, TResponse> : BroadcastQueueWithLockArrayWriter<TData, TResponse> where TResponse : IBroadcastQueueResponse {

    // private BroadcastQueueLockArrayWriter<TData, TResponse>? _writer; 
    public override BroadcastQueueLockArrayWriter<TData, TResponse> Writer {
        get {
            _writer ??= new BroadcastQueueLockArrayForLoopWriter<TData, TResponse>(
                _responseChannel.Reader);

            return _writer;
        }
    }
}