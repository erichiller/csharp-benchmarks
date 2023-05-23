#if DEBUG
#define DEBUG_MUX
#endif
// #define DEBUG_BROADCAST
// #define DEBUG_CHANNEL
// #define DEBUG_OBSERVER
// # define DEBUG_THREAD_PRIORITY
#if DEBUG_MUX
#define DEBUG
#define LOG
#endif

#undef DEBUG
#undef DEBUG_MUX
#undef LOG


using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using BroadcastChannel;

namespace BroadcastChannelMux;

public abstract class ChannelMux {
    /// <summary>A waiting reader (e.g. WaitForReadAsync) if there is one.</summary>
    private AsyncOperation<bool>? _waitingReader;
    private readonly AsyncOperation<bool> _waiterSingleton;
    private readonly bool                 _runContinuationsAsynchronously;
    private readonly object               _lockObj           = new ();
    private          Exception?           _completeException = null;
    private volatile int                  _readableItems     = 0;
    private volatile int                  _closedChannels    = 0;
    private readonly int                  _totalChannels;
    private          bool                 _areAllChannelsComplete => _closedChannels >= _totalChannels;
    private volatile bool                 _isReaderWaiting = false;

    /* Testing */ // KILL
    public int _WaitToReadAsync_mark1   = 0;
    public int _WaitToReadAsync__mark2  = 0;
    public int _WaitToReadAsync__mark3  = 0;
    public int _WaitToReadAsync__mark4  = 0;
    public int _WaitToReadAsync__mark5  = 0;
    public int _WaitToReadAsync__mark6  = 0;
    public int _WaitToReadAsync__mark7  = 0;
    public int _WaitToReadAsync__mark8  = 0;
    public int _WaitToReadAsync__mark9  = 0;
    public int _WaitToReadAsync__mark10 = 0;
    public int _WaitToReadAsync__mark11 = 0;
    public int _WaitToReadAsync__mark12 = 0;
    public int _tryWrite_mark1          = 0;
    public int _tryWrite_mark2          = 0;
    public int _tryWrite_mark3          = 0;
    public int _tryWrite_mark4          = 0;
    public int _tryWrite_mark5          = 0;
    public int _tryWrite_mark6          = 0;
    public int _tryWrite_mark7          = 0;
    public int _tryWrite_mark8          = 0;

    /* End Testing */

    /// <summary>Task that indicates the channel has completed.</summary>
    private readonly TaskCompletionSource _completion;

    public Task Completion => _completion.Task;


    protected ChannelMux( int totalChannels, bool runContinuationsAsynchronously = default ) {
        // TODO: actually do something with this?
        _runContinuationsAsynchronously = runContinuationsAsynchronously;
        _completion                     = new TaskCompletionSource( runContinuationsAsynchronously ? TaskCreationOptions.RunContinuationsAsynchronously : TaskCreationOptions.None );
        _waiterSingleton                = new AsyncOperation<bool>( runContinuationsAsynchronously, pooled: true );
        _totalChannels                  = totalChannels;
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.WaitToReadAsync"/>
    public ValueTask<bool> WaitToReadAsync( CancellationToken cancellationToken ) {
        DebugIncrement( ref _WaitToReadAsync_mark1 ); // KILL
        Log( nameof(WaitToReadAsync), $"{nameof(_readableItems)}: {_readableItems}" );
        // Outside of the lock, check if there are any items waiting to be read.  If there are, we're done.
        if ( cancellationToken.IsCancellationRequested ) {
            _isReaderWaiting = false;                      // URGENT: try marking as false once, then selectively marking as true.
            DebugIncrement( ref _WaitToReadAsync__mark2 ); // KILL
            return new ValueTask<bool>( Task.FromCanceled<bool>( cancellationToken ) );
        }

        if ( _readableItems > 0 ) {
            DebugIncrement( ref _WaitToReadAsync__mark3 ); // KILL
            Log( nameof(WaitToReadAsync), $"_readableItems > 0 : {_readableItems}" );
            _isReaderWaiting = false;
            return new ValueTask<bool>( true );
        }
        AsyncOperation<bool>? oldWaitingReader, newWaitingReader;
        lock ( _lockObj ) {
            DebugIncrement( ref _WaitToReadAsync__mark4 ); // KILL
            Log( nameof(WaitToReadAsync), "in lock" );
            // Again while holding the lock, check to see if there are any items available.
            if ( _readableItems > 0 ) {
                DebugIncrement( ref _WaitToReadAsync__mark5 ); // KILL
                Log( nameof(WaitToReadAsync), "in lock", $"_readableItems > 0 : {_readableItems}" );
                _isReaderWaiting = false;
                return new ValueTask<bool>( true );
            }
            // There aren't any items; if we're done writing, there never will be more items.
            if ( _areAllChannelsComplete ) {
                DebugIncrement( ref _WaitToReadAsync__mark6 ); // KILL
                Log( nameof(WaitToReadAsync), "_allChannelsDoneWriting" );
                _isReaderWaiting = false;
                // if an exception is present, return a cancelled ValueTask with the exception.
                return _completeException is { } exception ? new ValueTask<bool>( Task.FromException<bool>( exception ) ) : default;
            }
            // Try to use the singleton waiter.  If it's currently being used, then the channel
            // is being used erroneously, and we cancel the outstanding operation.
            oldWaitingReader = _waitingReader;
            if ( /* !cancellationToken.CanBeCanceled && */ _waiterSingleton.TryOwnAndReset() ) {
                DebugIncrement( ref _WaitToReadAsync__mark7 ); // KILL
                Log( nameof(WaitToReadAsync), "!cancellationToken.CanBeCanceled && _waiterSingleton.TryOwnAndReset()" );
                newWaitingReader = _waiterSingleton;
                if ( newWaitingReader == oldWaitingReader ) {
                    DebugIncrement( ref _WaitToReadAsync__mark8 ); // KILL
                    Log( nameof(WaitToReadAsync), "newWaitingReader == oldWaitingReader" );
                    // The previous operation completed, so null out the "old" waiter
                    // so we don't end up canceling the new operation.
                    oldWaitingReader = null;
                }
            } else {
                DebugIncrement( ref _WaitToReadAsync__mark9 ); // KILL
                Log( nameof(WaitToReadAsync), "ELSE" );
                newWaitingReader = new AsyncOperation<bool>( _runContinuationsAsynchronously, cancellationToken );
            }
            DebugIncrement( ref _WaitToReadAsync__mark10 ); // KILL
            Log( nameof(WaitToReadAsync), $"newWaitingReader is {newWaitingReader}" );
            _isReaderWaiting = true;
            _waitingReader   = newWaitingReader;
        }

        // KILL ??
        if ( _readableItems > 0 ) {
            DebugIncrement( ref _WaitToReadAsync__mark11 ); // KILL
            // _isReaderWaiting = false;
            Log( nameof(WaitToReadAsync), $"_readableItems > 0 : {_readableItems}" );
            return new ValueTask<bool>( true );
        }

        DebugIncrement( ref _WaitToReadAsync__mark12 ); // KILL
        Log( nameof(WaitToReadAsync), $"oldWaitingReader is {newWaitingReader}" );
        oldWaitingReader?.TrySetCanceled( default );
        return newWaitingReader.ValueTaskOfT;
    }

    [ Conditional( "LOG" ) ]
    protected void Log( params string[] contextAndMessage ) {
        Console.WriteLine( $"{this.GetType().Name} > {String.Join( " > ", contextAndMessage )}" );
    }


    [ Conditional( "DEBUG" ) ]
    private static void DebugIncrement( ref int n ) => Interlocked.Increment( ref n );


    //


    protected sealed class ChannelMuxInput<TData> : ChannelWriter<TData>, IDisposable {
        private readonly ChannelMux                               _parent;
        private readonly RemoveWriterByHashCode                   _removeWriterCallback;
        private readonly SingleProducerSingleConsumerQueue<TData> _queue      = new SingleProducerSingleConsumerQueue<TData>();
        private          bool                                     _isComplete = false; // TODO: I don't think I need volatile here

        internal ChannelMuxInput( BroadcastChannelWriter<TData, IBroadcastChannelResponse> channel, ChannelMux parent ) {
            _removeWriterCallback = channel.AddReader( this );
            _parent               = parent;
            _parent.Log( nameof(ChannelMuxInput<TData>), "constructor" );
        }

        /// <inheritdoc />
        public override bool TryWrite( TData item ) {
            _parent.Log( nameof(TryWrite) );
            DebugIncrement( ref _parent._tryWrite_mark1 ); // KILL
            if ( _isComplete ) {
                DebugIncrement( ref _parent._tryWrite_mark2 ); // KILL
                return false;
            }

            _queue.Enqueue( item );
            Interlocked.Increment( ref _parent._readableItems );
            if ( !_parent._isReaderWaiting ) {
                DebugIncrement( ref _parent._tryWrite_mark3 ); // KILL
                return true;
            }
            AsyncOperation<bool>? waitingReader = null;
            if ( Monitor.TryEnter( _parent._lockObj ) ) {
                DebugIncrement( ref _parent._tryWrite_mark4 ); // KILL
                try {
                    _parent.Log( nameof(ChannelMuxInput<TData>), nameof(TryWrite), "in lock" );
                    waitingReader = _parent._waitingReader;
                    if ( waitingReader == null ) {
                        DebugIncrement( ref _parent._tryWrite_mark5 ); // KILL
                        _parent.Log( nameof(TryWrite), "waitingReader is null" );
                        return true;
                    }
                    DebugIncrement( ref _parent._tryWrite_mark6 ); // KILL
                    _parent._isReaderWaiting = false;
                    _parent._waitingReader   = null; // URGENT: try replacing the lock with a volatile bool
                } finally {
                    // Ensure that the lock is released.
                    Monitor.Exit( _parent._lockObj );
                }
            }
            if ( waitingReader != null ) {
                DebugIncrement( ref _parent._tryWrite_mark7 ); // KILL
                _parent.Log( nameof(TryWrite), "grabbed a waiting reader, setting result to true" );
                // If we get here, we grabbed a waiting reader.
                waitingReader?.TrySetResult( item: true );
            }
            DebugIncrement( ref _parent._tryWrite_mark8 ); // KILL
            return true;
        }

        // /// <inheritdoc />
        // public override bool TryWrite( TData item ) {
        //     _parent.Log( nameof(TryWrite) );
        //     if ( _doneWriting != null ) {
        //         return false;
        //     }
        //
        //     _queue.Enqueue( item );
        //     
        //     Interlocked.Increment( ref _parent._readableItems );
        //     AsyncOperation<bool>? waitingReader = Interlocked.Exchange(
        //         ref _parent._waitingReader,
        //         null );
        //     // URGENT: try replacing the lock with a volatile bool or Interlocked exchange
        //     if ( waitingReader == null ) {
        //         _parent.Log( nameof(TryWrite), "waitingReader is null" );
        //         return true;
        //     }
        //     _parent.Log( nameof(TryWrite), "grabbed a waiting reader, setting result to true" );
        //     // If we get here, we grabbed a waiting reader.
        //     waitingReader?.TrySetResult( item: true );
        //     return true;
        // }

        /// <inheritdoc />
        /// <remarks>
        /// This will always return immediately.
        /// </remarks>
        public override ValueTask<bool> WaitToWriteAsync( CancellationToken cancellationToken = new CancellationToken() ) {
            Exception? completeException = _parent._completeException; // URGENT: maybe setting to a local is something I need to do elsewhere?
            return cancellationToken.IsCancellationRequested ? new ValueTask<bool>( Task.FromCanceled<bool>( cancellationToken ) ) :
                !_isComplete                                 ? new ValueTask<bool>( true ) :
                completeException is { }                     ? new ValueTask<bool>( Task.FromException<bool>( completeException ) ) :
                                                               default;
        }

        public override bool TryComplete( Exception? exception = null ) {
            AsyncOperation<bool>? waitingReader = null;
            bool                  completeTask  = false;
            _parent.Log( nameof(ChannelMuxInput<TData>), nameof(TryComplete) );

            // If we're already marked as complete, there's nothing more to do.
            if ( _isComplete ) {
                return false;
            }

            // Mark as complete for writing.
            _isComplete = true;
            if ( exception is { } ) {
                exception.Data.Add( nameof(ChannelMux) + " Type", typeof(TData) );
                Interlocked.Exchange( ref _parent._completeException, exception );
            }

            Interlocked.Increment( ref _parent._closedChannels );
            // if all channels are closed, or if this complete was reported with an exception, close everything
            // TODO: should a single closing exception closed the entire mux?? Could make it configurable??
            if ( _parent._closedChannels >= _parent._totalChannels || exception is { } ) {
                _parent.Log( nameof(ChannelMuxInput<TData>), nameof(TryComplete), $"{nameof(exception)}: {( exception?.ToString() ?? "null" )}" );
                _parent.Log( nameof(ChannelMuxInput<TData>), nameof(TryComplete), $"{nameof(_parent._closedChannels)}: {_parent._closedChannels}" );
                _parent.Log( nameof(ChannelMuxInput<TData>), nameof(TryComplete), $"{nameof(_parent._totalChannels)}: {_parent._totalChannels}" );

                // If we have no more items remaining, then the channel needs to be marked as completed
                // and readers need to be informed they'll never get another item.  All of that needs
                // to happen outside of the lock to avoid invoking continuations under the lock.
                if ( _queue.IsEmpty ) {
                    _parent.Log( nameof(ChannelMuxInput<TData>), nameof(TryComplete), "_queue.IsEmpty is TRUE" );
                    // TODO: what to do here. If one channel closes should the whole mux stop working? seems like no.
                    completeTask = true;
                    lock ( _parent._lockObj ) {
                        if ( _parent._waitingReader != null ) {
                            waitingReader          = _parent._waitingReader;
                            _parent._waitingReader = null;
                        }
                    }
                }
                // Complete the channel task if necessary
                if ( completeTask ) {
                    _parent.Log( nameof(ChannelMuxInput<TData>), nameof(TryComplete), "will complete task" );
                    ChannelUtilities.Complete( _parent._completion, exception );
                }

                _parent.Log( nameof(ChannelMuxInput<TData>), nameof(TryComplete), $"{nameof(waitingReader)}: {( waitingReader?.ToString() ?? "null" )}" );
                _parent.Log( nameof(ChannelMuxInput<TData>), nameof(TryComplete), $"{nameof(_parent._closedChannels)}: {_parent._closedChannels}" );
                _parent.Log( nameof(ChannelMuxInput<TData>), nameof(TryComplete), $"{nameof(_parent._totalChannels)}: {_parent._totalChannels}" );
                // Complete a waiting reader if there is one
                if ( waitingReader != null ) {
                    _parent.Log( nameof(ChannelMuxInput<TData>), nameof(TryComplete), "waitingReader != null" );
                    if ( exception != null ) {
                        waitingReader.TrySetException( exception );
                    } else {
                        waitingReader.TrySetResult( item: false );
                    }
                }
            }

            _parent.Log( nameof(ChannelMuxInput<TData>), nameof(TryComplete), $"Successfully completed the channel for type {typeof(TData).Name}" );
            // Successfully completed the channel
            return true;
        }

        [ SuppressMessage( "ReSharper", "RedundantNullableFlowAttribute" ) ]
        public bool TryRead( [ MaybeNullWhen( false ) ] out TData? item ) {
            _parent.Log( nameof(ChannelMuxInput<TData>) + $"<{typeof(TData).Name}>", nameof(TryRead), $"{nameof(_parent._readableItems)}: was {_parent._readableItems}" );
            if ( _queue.TryDequeue( out item ) ) {
                _parent.Log( nameof(ChannelMuxInput<TData>) + $"<{typeof(TData).Name}>", $"{nameof(_parent._readableItems)}: was {_parent._readableItems}" );
                Interlocked.Decrement( ref _parent._readableItems );
                _parent.Log( nameof(ChannelMuxInput<TData>) + $"<{typeof(TData).Name}>", $"{nameof(_parent._readableItems)}: now {_parent._readableItems}" );
                if ( _isComplete && _queue.IsEmpty ) {
                    ChannelUtilities.Complete( _parent._completion, _parent._completeException );
                }
                return true;
            }
            return false;
        }

        public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = default ) =>
            // Writing always succeeds (unless we've already completed writing or cancellation has been requested),
            // so just TryWrite and return a completed task.
            cancellationToken.IsCancellationRequested ? new ValueTask( Task.FromCanceled( cancellationToken ) ) :
            TryWrite( item )                          ? default :
                                                        new ValueTask( Task.FromException( ChannelUtilities.CreateInvalidCompletionException( _parent._completeException ) ) );

        /// <inheritdoc />
        public void Dispose( ) {
            _removeWriterCallback.Invoke( this.GetHashCode() );
        }
    }
}

// TODO: should handle responses? (the second type arg of BroadcastChannel)
public class ChannelMux<T1, T2> : ChannelMux, IDisposable {
    private readonly ChannelMuxInput<T1> _input1;
    private readonly ChannelMuxInput<T2> _input2;

    public ChannelMux( BroadcastChannelWriter<T1, IBroadcastChannelResponse> channel1, BroadcastChannelWriter<T2, IBroadcastChannelResponse> channel2 ) : this( channel1, channel2, totalChannels: 2 ) { }

    protected ChannelMux( BroadcastChannelWriter<T1, IBroadcastChannelResponse> channel1, BroadcastChannelWriter<T2, IBroadcastChannelResponse> channel2, int totalChannels ) : base( totalChannels: totalChannels ) {
        _input1 = new ChannelMuxInput<T1>( channel1, this );
        _input2 = new ChannelMuxInput<T2>( channel2, this );
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    [ SuppressMessage( "ReSharper", "RedundantNullableFlowAttribute" ) ]
    public bool TryRead( [ MaybeNullWhen( false ) ] out T1 item ) => _input1.TryRead( out item );

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    [ SuppressMessage( "ReSharper", "RedundantNullableFlowAttribute" ) ]
    public bool TryRead( [ MaybeNullWhen( false ) ] out T2 item ) => _input2.TryRead( out item );

    /*
     * IDisposable implementation
     */

    private bool _isDisposed = false;

    public void Dispose( ) => Dispose( true );

    [ SuppressMessage( "ReSharper", "InconsistentNaming" ) ]
    protected virtual void Dispose( bool disposing ) {
        if ( !_isDisposed ) {
            if ( disposing ) {
                _input1.Dispose();
                _input2.Dispose();
            }
            _isDisposed = true;
        }
    }
}

public class ChannelMux<T1, T2, T3> : ChannelMux<T1, T2>, IDisposable {
    // NOTE: can easily add more generic params ;; // TODO: BENCHMARK THAT additional parameters don'T HURT PERFORMANCE!
    // TODO: should handle responses? (the second type arg of BroadcastChannel)
    private readonly ChannelMuxInput<T3> _input;

    public ChannelMux(
        BroadcastChannelWriter<T1, IBroadcastChannelResponse> channel1,
        BroadcastChannelWriter<T2, IBroadcastChannelResponse> channel2,
        BroadcastChannelWriter<T3, IBroadcastChannelResponse> channel3
    ) : this( channel1, channel2, channel3, totalChannels: 3 ) { }

    protected ChannelMux(
        BroadcastChannelWriter<T1, IBroadcastChannelResponse> channel1,
        BroadcastChannelWriter<T2, IBroadcastChannelResponse> channel2,
        BroadcastChannelWriter<T3, IBroadcastChannelResponse> channel3,
        int                                                   totalChannels
    ) : base( channel1, channel2, totalChannels: totalChannels ) {
        _input = new ChannelMuxInput<T3>( channel3, this );
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    public bool TryRead( [ MaybeNullWhen( false ) ] out T3 item ) => _input.TryRead( out item );

    /*
     * Disposal
     */

    private bool _isDisposed = false;

    protected override void Dispose( bool disposing ) {
        if ( !_isDisposed ) {
            if ( disposing ) {
                _input.Dispose();
            }
            _isDisposed = true;
        }
        base.Dispose( disposing );
    }
}

public class ChannelMux<T1, T2, T3, T4> : ChannelMux<T1, T2, T3>, IDisposable {
    // NOTE: can easily add more generic params ;; // TODO: BENCHMARK THAT additional parameters don'T HURT PERFORMANCE!
    // TODO: should handle responses? (the second type arg of BroadcastChannel)
    private readonly ChannelMuxInput<T4> _input;

    public ChannelMux(
        BroadcastChannelWriter<T1, IBroadcastChannelResponse> channel1,
        BroadcastChannelWriter<T2, IBroadcastChannelResponse> channel2,
        BroadcastChannelWriter<T3, IBroadcastChannelResponse> channel3,
        BroadcastChannelWriter<T4, IBroadcastChannelResponse> channel4
    ) : this( channel1, channel2, channel3, channel4, totalChannels: 4 ) { }

    protected ChannelMux(
        BroadcastChannelWriter<T1, IBroadcastChannelResponse> channel1,
        BroadcastChannelWriter<T2, IBroadcastChannelResponse> channel2,
        BroadcastChannelWriter<T3, IBroadcastChannelResponse> channel3,
        BroadcastChannelWriter<T4, IBroadcastChannelResponse> channel4,
        int                                                   totalChannels
    ) : base( channel1, channel2, channel3, totalChannels: totalChannels ) {
        _input = new ChannelMuxInput<T4>( channel4, this );
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    public bool TryRead( [ MaybeNullWhen( false ) ] out T4 item ) => _input.TryRead( out item );

    /*
     * Disposal
     */

    private bool _isDisposed = false;

    protected override void Dispose( bool disposing ) {
        if ( !_isDisposed ) {
            if ( disposing ) {
                _input.Dispose();
            }
            _isDisposed = true;
        }
        base.Dispose( disposing );
    }
}

public class ChannelMux<T1, T2, T3, T4, T5> : ChannelMux<T1, T2, T3, T4>, IDisposable {
    // NOTE: can easily add more generic params ;; // TODO: BENCHMARK THAT additional parameters don'T HURT PERFORMANCE!
    // TODO: should handle responses? (the second type arg of BroadcastChannel)
    private readonly ChannelMuxInput<T5> _input;

    public ChannelMux(
        BroadcastChannelWriter<T1, IBroadcastChannelResponse> channel1,
        BroadcastChannelWriter<T2, IBroadcastChannelResponse> channel2,
        BroadcastChannelWriter<T3, IBroadcastChannelResponse> channel3,
        BroadcastChannelWriter<T4, IBroadcastChannelResponse> channel4,
        BroadcastChannelWriter<T5, IBroadcastChannelResponse> channel5
    ) : this( channel1, channel2, channel3, channel4, channel5, totalChannels: 5 ) { }

    protected ChannelMux(
        BroadcastChannelWriter<T1, IBroadcastChannelResponse> channel1,
        BroadcastChannelWriter<T2, IBroadcastChannelResponse> channel2,
        BroadcastChannelWriter<T3, IBroadcastChannelResponse> channel3,
        BroadcastChannelWriter<T4, IBroadcastChannelResponse> channel4,
        BroadcastChannelWriter<T5, IBroadcastChannelResponse> channel5,
        int                                                   totalChannels
    ) : base( channel1, channel2, channel3, channel4, totalChannels: totalChannels ) {
        _input = new ChannelMuxInput<T5>( channel5, this );
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    public bool TryRead( [ MaybeNullWhen( false ) ] out T5 item ) => _input.TryRead( out item );

    /*
     * Disposal
     */

    private bool _isDisposed = false;

    protected override void Dispose( bool disposing ) {
        if ( !_isDisposed ) {
            if ( disposing ) {
                _input.Dispose();
            }
            _isDisposed = true;
        }
        base.Dispose( disposing );
    }
}

public class ChannelMux<T1, T2, T3, T4, T5, T6> : ChannelMux<T1, T2, T3, T4, T5>, IDisposable {
    // NOTE: can easily add more generic params ;; // TODO: BENCHMARK THAT additional parameters don'T HURT PERFORMANCE!
    // TODO: should handle responses? (the second type arg of BroadcastChannel)
    private readonly ChannelMuxInput<T6> _input;

    public ChannelMux(
        BroadcastChannelWriter<T1, IBroadcastChannelResponse> channel1,
        BroadcastChannelWriter<T2, IBroadcastChannelResponse> channel2,
        BroadcastChannelWriter<T3, IBroadcastChannelResponse> channel3,
        BroadcastChannelWriter<T4, IBroadcastChannelResponse> channel4,
        BroadcastChannelWriter<T5, IBroadcastChannelResponse> channel5,
        BroadcastChannelWriter<T6, IBroadcastChannelResponse> channel6
    ) : this( channel1, channel2, channel3, channel4, channel5, channel6, totalChannels: 6 ) { }

    protected ChannelMux(
        BroadcastChannelWriter<T1, IBroadcastChannelResponse> channel1,
        BroadcastChannelWriter<T2, IBroadcastChannelResponse> channel2,
        BroadcastChannelWriter<T3, IBroadcastChannelResponse> channel3,
        BroadcastChannelWriter<T4, IBroadcastChannelResponse> channel4,
        BroadcastChannelWriter<T5, IBroadcastChannelResponse> channel5,
        BroadcastChannelWriter<T6, IBroadcastChannelResponse> channel6,
        int                                                   totalChannels
    ) : base( channel1, channel2, channel3, channel4, channel5, totalChannels: totalChannels ) {
        _input = new ChannelMuxInput<T6>( channel6, this );
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    public bool TryRead( [ MaybeNullWhen( false ) ] out T6 item ) => _input.TryRead( out item );

    /*
     * Disposal
     */

    private bool _isDisposed = false;

    protected override void Dispose( bool disposing ) {
        if ( !_isDisposed ) {
            if ( disposing ) {
                _input.Dispose();
            }
            _isDisposed = true;
        }
        base.Dispose( disposing );
    }
}

public class ChannelMux<T1, T2, T3, T4, T5, T6, T7> : ChannelMux<T1, T2, T3, T4, T5, T6>, IDisposable {
    // NOTE: can easily add more generic params ;; // TODO: BENCHMARK THAT additional parameters don'T HURT PERFORMANCE!
    // TODO: should handle responses? (the second type arg of BroadcastChannel)
    private readonly ChannelMuxInput<T7> _input;

    public ChannelMux(
        BroadcastChannelWriter<T1, IBroadcastChannelResponse> channel1,
        BroadcastChannelWriter<T2, IBroadcastChannelResponse> channel2,
        BroadcastChannelWriter<T3, IBroadcastChannelResponse> channel3,
        BroadcastChannelWriter<T4, IBroadcastChannelResponse> channel4,
        BroadcastChannelWriter<T5, IBroadcastChannelResponse> channel5,
        BroadcastChannelWriter<T6, IBroadcastChannelResponse> channel6,
        BroadcastChannelWriter<T7, IBroadcastChannelResponse> channel7
    ) : this( channel1, channel2, channel3, channel4, channel5, channel6, channel7, totalChannels: 7 ) { }

    protected ChannelMux(
        BroadcastChannelWriter<T1, IBroadcastChannelResponse> channel1,
        BroadcastChannelWriter<T2, IBroadcastChannelResponse> channel2,
        BroadcastChannelWriter<T3, IBroadcastChannelResponse> channel3,
        BroadcastChannelWriter<T4, IBroadcastChannelResponse> channel4,
        BroadcastChannelWriter<T5, IBroadcastChannelResponse> channel5,
        BroadcastChannelWriter<T6, IBroadcastChannelResponse> channel6,
        BroadcastChannelWriter<T7, IBroadcastChannelResponse> channel7,
        int                                                   totalChannels
    ) : base( channel1, channel2, channel3, channel4, channel5, channel6, totalChannels: totalChannels ) {
        _input = new ChannelMuxInput<T7>( channel7, this );
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    public bool TryRead( [ MaybeNullWhen( false ) ] out T7 item ) => _input.TryRead( out item );

    /*
     * Disposal
     */

    private bool _isDisposed = false;

    protected override void Dispose( bool disposing ) {
        if ( !_isDisposed ) {
            if ( disposing ) {
                _input.Dispose();
            }
            _isDisposed = true;
        }
        base.Dispose( disposing );
    }
}


public class ChannelMux<T1, T2, T3, T4, T5, T6, T7, T8> : ChannelMux<T1, T2, T3, T4, T5, T6, T7>, IDisposable {
    // NOTE: can easily add more generic params ;; // TODO: BENCHMARK THAT additional parameters don'T HURT PERFORMANCE!
    // TODO: should handle responses? (the second type arg of BroadcastChannel)
    private readonly ChannelMuxInput<T8> _input;

    public ChannelMux(
        BroadcastChannelWriter<T1, IBroadcastChannelResponse> channel1,
        BroadcastChannelWriter<T2, IBroadcastChannelResponse> channel2,
        BroadcastChannelWriter<T3, IBroadcastChannelResponse> channel3,
        BroadcastChannelWriter<T4, IBroadcastChannelResponse> channel4,
        BroadcastChannelWriter<T5, IBroadcastChannelResponse> channel5,
        BroadcastChannelWriter<T6, IBroadcastChannelResponse> channel6,
        BroadcastChannelWriter<T7, IBroadcastChannelResponse> channel7,
        BroadcastChannelWriter<T8, IBroadcastChannelResponse> channel8
    ) : this( channel1, channel2, channel3, channel4, channel5, channel6, channel7, channel8, totalChannels: 8 ) { }

    protected ChannelMux(
        BroadcastChannelWriter<T1, IBroadcastChannelResponse> channel1,
        BroadcastChannelWriter<T2, IBroadcastChannelResponse> channel2,
        BroadcastChannelWriter<T3, IBroadcastChannelResponse> channel3,
        BroadcastChannelWriter<T4, IBroadcastChannelResponse> channel4,
        BroadcastChannelWriter<T5, IBroadcastChannelResponse> channel5,
        BroadcastChannelWriter<T6, IBroadcastChannelResponse> channel6,
        BroadcastChannelWriter<T7, IBroadcastChannelResponse> channel7,
        BroadcastChannelWriter<T8, IBroadcastChannelResponse> channel8,
        int                                                   totalChannels
    ) : base( channel1, channel2, channel3, channel4, channel5, channel6, channel7, totalChannels: totalChannels ) {
        _input = new ChannelMuxInput<T8>( channel8, this );
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    public bool TryRead( [ MaybeNullWhen( false ) ] out T8 item ) => _input.TryRead( out item );

    /*
     * Disposal
     */

    private bool _isDisposed = false;

    protected override void Dispose( bool disposing ) {
        if ( !_isDisposed ) {
            if ( disposing ) {
                _input.Dispose();
            }
            _isDisposed = true;
        }
        base.Dispose( disposing );
    }
}