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
// #define LOG
// #define DEBUG


using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using BroadcastChannel;

// ReSharper disable MemberCanBePrivate.Global

namespace BroadcastChannelMux;

public abstract class ChannelMux {
    /// <summary>A waiting reader (e.g. WaitForReadAsync) if there is one.</summary>
    private AsyncOperation<bool>? _waitingReader;
    private volatile bool                 _isReaderWaiting = false;
    private readonly AsyncOperation<bool> _waiterSingleton;
    private readonly bool                 _runContinuationsAsynchronously;
    private readonly object               _waiterLockObj                    = new ();
    private readonly object               _closedChannelLockObj             = new ();
    private          Exception?           _completeException                = null;
    private          Type?                _completeExceptionChannelDataType = null;
    private volatile bool                 _hasException                     = false;
    private volatile int                  _readableItems                    = 0;
    private volatile int                  _closedChannels                   = 0;
    private readonly int                  _totalChannels;
    private          bool                 _areAllChannelsComplete => _closedChannels >= _totalChannels;

    /* Testing */ /* KILL START */
    // ReSharper disable InconsistentNaming
    public int _WaitToReadAsync__entry                 = 0;
    public int _WaitToReadAsync__cancellationToken     = 0;
    public int _WaitToReadAsync__completeException     = 0;
    public int _WaitToReadAsync__readableItems         = 0;
    public int _WaitToReadAsync__inLock                = 0;
    public int _WaitToReadAsync__inLock_readableItems  = 0;
    public int _WaitToReadAsync__allChannelsComplete   = 0;
    public int _WaitToReadAsync__TryOwnAndReset        = 0;
    public int _WaitToReadAsync__new_eq_old            = 0;
    public int _WaitToReadAsync__TryOwnAndReset_failed = 0;
    public int _WaitToReadAsync__inLock_end            = 0;
    public int _WaitToReadAsync__readableItems_end     = 0;
    public int _WaitToReadAsync__end                   = 0;
    public int _tryWrite_enter                         = 0;
    public int _tryWrite_isComplete_or_exception       = 0;
    public int _tryWrite_no_reader_waiting             = 0;
    public int _tryWrite_in_monitor                    = 0;
    public int _tryWrite_monitor_no_waiting_reader     = 0;
    public int _tryWrite_monitor_set_booleans          = 0;
    public int _tryWrite_waiting_reader_is_not_null    = 0;
    public int _tryWrite_final                         = 0;

    public Stopwatch StopWatch { get; init; } = Stopwatch.StartNew();

    [ Conditional( "LOG" ) ]
    protected void Log<T>( params object?[] contextAndMessage ) {
        Console.WriteLine( $"[{StopWatch.ElapsedTicks:N0}]{typeof(T).Name} > {String.Join( " > ", contextAndMessage )}" );
    }

    [ Conditional( "LOG" ) ]
    protected void Log<T>( Type subType, params string[] contextAndMessage ) {
        Console.WriteLine( $"[{StopWatch.ElapsedTicks:N0}] {typeof(T).Name}<{subType.Name.Split( '_' )[ ^1 ]}> ⮕  {String.Join( " ⮕  ", contextAndMessage )}" );
    }

    [ Conditional( "LOG" ) ]
    protected void LogWarn<T>( Type subType, params string[] contextAndMessage ) {
        Console.WriteLine( $"[{StopWatch.ElapsedTicks:N0}] {typeof(T).Name}<{subType.Name.Split( '_' )[ ^1 ]}> ⮕  {String.Join( " ⮕  ", contextAndMessage )}" );
    }

    [ Conditional( "DEBUG" ) ]
    private static void DebugIncrement( ref int n ) => Interlocked.Increment( ref n );
    // ReSharper restore InconsistentNaming
    /* End Testing */ /* KILL END */

    /// <summary>Task that indicates the channel has completed.</summary>
    private TaskCompletionSource _completion;

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.Completion"/>
    public Task Completion => _completion.Task;

    private TaskCompletionSource createCompletionTask( ) => new TaskCompletionSource( _runContinuationsAsynchronously ? TaskCreationOptions.RunContinuationsAsynchronously : TaskCreationOptions.None );

    protected void resetOneChannel<TData>( ChannelMuxInput<TData> muxInput ) {
        if ( muxInput.IsClosed ) {
            Interlocked.Decrement( ref _closedChannels );
        }
        if ( _completion.Task.IsCompleted ) {
            _completion = createCompletionTask();
        }
        if ( _completeExceptionChannelDataType == typeof(TData) ) {
            _completeException = null;
            _hasException      = false;
        }
    }

    /// <summary>
    /// Return <c>Exception</c> if the entire ChannelMux and all associated ChannelReaders should be ended. (else return null)
    /// - Exits `WaitToReadAsync` & return Exception on any new calls
    /// - Ends `Completion` Task (once all items have been read).
    /// - `TryWrite` for any other channels is closed (immediately)
    /// </summary>
    /// <remarks>
    /// Note that `TryRead` will still be allowed until the queue is empty, but because `TryWrite` is ended, the queue will not continue to be added to.
    /// </remarks>
    public delegate Exception? ChannelCompleteHandler( Type reportingChannelType, Exception? exception );

    /// <inheritdoc cref="ChannelCompleteHandler" />
    public ChannelCompleteHandler? OnChannelComplete { get; init; }

    /// <inheritdoc cref="ChannelMux" />
    protected ChannelMux( int totalChannels, bool runContinuationsAsynchronously = default ) {
        _runContinuationsAsynchronously = runContinuationsAsynchronously;
        _completion                     = createCompletionTask();
        _waiterSingleton                = new AsyncOperation<bool>( runContinuationsAsynchronously, pooled: true );
        _totalChannels                  = totalChannels;
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.WaitToReadAsync"/>
    public ValueTask<bool> WaitToReadAsync( CancellationToken cancellationToken ) {
        DebugIncrement( ref _WaitToReadAsync__entry ); // KILL
        Log<ChannelMux>( nameof(WaitToReadAsync), $"{nameof(_readableItems)}: {_readableItems}" );
        _isReaderWaiting = false;
        // Outside of the lock, check if there are any items waiting to be read.  If there are, we're done.
        if ( cancellationToken.IsCancellationRequested ) {
            Log<ChannelMux>( nameof(WaitToReadAsync), "cancellationToken.IsCancellationRequested" );
            DebugIncrement( ref _WaitToReadAsync__cancellationToken ); // KILL
            return new ValueTask<bool>( Task.FromCanceled<bool>( cancellationToken ) );
        }

        if ( _hasException && _completeException is { } ) {
            DebugIncrement( ref _WaitToReadAsync__completeException ); // KILL
            Log<ChannelMux>( nameof(WaitToReadAsync), $"_completeException is {_completeException.GetType().Name.Split( '_' )[ ^1 ]}; _readableItems is {_readableItems}" );
            // if an exception is present, return a cancelled ValueTask with the exception.
            return new ValueTask<bool>( Task.FromException<bool>( _completeException ) );
        }

        if ( _readableItems > 0 ) {
            DebugIncrement( ref _WaitToReadAsync__readableItems ); // KILL
            Log<ChannelMux>( nameof(WaitToReadAsync), $"_readableItems > 0 : {_readableItems}" );
            return new ValueTask<bool>( true );
        }
        AsyncOperation<bool>? oldWaitingReader, newWaitingReader;
        lock ( _waiterLockObj ) {
            DebugIncrement( ref _WaitToReadAsync__inLock ); // KILL
            Log<ChannelMux>( nameof(WaitToReadAsync), "in lock" );
            // Again while holding the lock, check to see if there are any items available.
            if ( _readableItems > 0 ) {
                DebugIncrement( ref _WaitToReadAsync__inLock_readableItems ); // KILL
                Log<ChannelMux>( nameof(WaitToReadAsync), "in lock", $"_readableItems > 0 : {_readableItems}" );
                return new ValueTask<bool>( true );
            }
            // There aren't any items; if we're done writing, there never will be more items.
            if ( _areAllChannelsComplete ) {
                DebugIncrement( ref _WaitToReadAsync__allChannelsComplete ); // KILL
                Log<ChannelMux>( nameof(WaitToReadAsync), "_allChannelsDoneWriting", "Exception", _completeException );
                // if an exception is present, return a cancelled ValueTask with the exception.
                return _completeException is { } exception ? new ValueTask<bool>( Task.FromException<bool>( exception ) ) : default;
            }
            // Try to use the singleton waiter.  If it's currently being used, then the channel
            // is being used erroneously, and we cancel the outstanding operation.
            oldWaitingReader = _waitingReader;
            if ( !cancellationToken.CanBeCanceled && _waiterSingleton.TryOwnAndReset() ) {
                DebugIncrement( ref _WaitToReadAsync__TryOwnAndReset ); // KILL
                Log<ChannelMux>( nameof(WaitToReadAsync), "!cancellationToken.CanBeCanceled && _waiterSingleton.TryOwnAndReset()" );
                newWaitingReader = _waiterSingleton;
                if ( newWaitingReader == oldWaitingReader ) {
                    DebugIncrement( ref _WaitToReadAsync__new_eq_old ); // KILL
                    Log<ChannelMux>( nameof(WaitToReadAsync), "newWaitingReader == oldWaitingReader" );
                    // The previous operation completed, so null out the "old" waiter
                    // so we don't end up canceling the new operation.
                    oldWaitingReader = null;
                }
            } else {
                DebugIncrement( ref _WaitToReadAsync__TryOwnAndReset_failed ); // KILL
                Log<ChannelMux>( nameof(WaitToReadAsync), "ELSE" );
                newWaitingReader = new AsyncOperation<bool>( _runContinuationsAsynchronously, cancellationToken ); // TODO: This is the source of a large number of assignments to the Small Object Heap
            }
            DebugIncrement( ref _WaitToReadAsync__inLock_end ); // KILL
            Log<ChannelMux>( nameof(WaitToReadAsync), $"newWaitingReader is {newWaitingReader}" );
            _isReaderWaiting = true;
            _waitingReader   = newWaitingReader;
        }

        if ( _readableItems > 0 ) {
            DebugIncrement( ref _WaitToReadAsync__readableItems_end ); // KILL
            Log<ChannelMux>( nameof(WaitToReadAsync), $"_readableItems > 0 : {_readableItems}" );
            return new ValueTask<bool>( true );
        }

        DebugIncrement( ref _WaitToReadAsync__end ); // KILL
        Log<ChannelMux>( nameof(WaitToReadAsync), $"oldWaitingReader is {newWaitingReader}" );
        oldWaitingReader?.TrySetCanceled( default );
        return newWaitingReader.ValueTaskOfT;
    }

    /*
     * ChannelMuxInput
     */

    protected interface IChannelMuxInput {
        /// <inheritdoc cref="P:BroadcastChannelMux.SingleProducerSingleConsumerQueue`1.IsEmpty" />
        public bool IsEmpty { get; }

        /// <summary>
        /// Whether the Channel is has had <see cref="ChannelMuxInput{TData}.TryComplete"/> called.
        /// </summary>
        public bool IsComplete { get; }

        /// <summary>
        /// Whether when the input has incremented its parent's <see cref="ChannelMux._closedChannels"/>.
        /// </summary>
        public bool IsClosed { get; }
    }

    protected sealed class ChannelMuxInput<TData> : ChannelWriter<TData>, IDisposable, IEnumerable<TData>, IChannelMuxInput {
        private readonly ChannelMux                               _parent;
        private readonly RemoveWriterByHashCode                   _removeWriterCallback;
        private readonly SingleProducerSingleConsumerQueue<TData> _queue            = new SingleProducerSingleConsumerQueue<TData>();
        private volatile bool                                     _isComplete       = false;
        private volatile bool                                     _emptyAndComplete = false;
        private volatile bool                                     _isClosed         = false; // set once the parent's _closedChannels has been incremented by this input

        internal ChannelMuxInput( BroadcastChannelWriter<TData, IBroadcastChannelResponse> channel, ChannelMux parent ) {
            _removeWriterCallback = channel.AddReader( this );
            _parent               = parent;
            _parent.Log<ChannelMuxInput<TData>>( typeof(TData), nameof(ChannelMuxInput<TData>), "constructor" );
        }

        /// <inheritdoc />
        public override bool TryWrite( TData item ) {
            DebugIncrement( ref _parent._tryWrite_enter ); // KILL
            if ( _isComplete || _parent._hasException ) {
                DebugIncrement( ref _parent._tryWrite_isComplete_or_exception ); // KILL
                return false;
            }

            _queue.Enqueue( item );
            Interlocked.Increment( ref _parent._readableItems );
            if ( !_parent._isReaderWaiting ) {
                DebugIncrement( ref _parent._tryWrite_no_reader_waiting ); // KILL
                return true;
            }
            AsyncOperation<bool>? waitingReader = null;
            if ( Monitor.TryEnter( _parent._waiterLockObj ) ) {
                DebugIncrement( ref _parent._tryWrite_in_monitor ); // KILL
                try {
                    _parent.Log<ChannelMuxInput<TData>>( typeof(TData), nameof(ChannelMuxInput<TData>), nameof(TryWrite), "in lock" );
                    waitingReader = _parent._waitingReader;
                    if ( waitingReader == null ) {
                        DebugIncrement( ref _parent._tryWrite_monitor_no_waiting_reader ); // KILL
                        _parent.Log<ChannelMuxInput<TData>>( typeof(TData), nameof(TryWrite), "waitingReader is null" );
                        return true;
                    }
                    DebugIncrement( ref _parent._tryWrite_monitor_set_booleans ); // KILL
                    _parent._isReaderWaiting = false;
                    _parent._waitingReader   = null;
                } finally {
                    // Ensure that the lock is released.
                    Monitor.Exit( _parent._waiterLockObj );
                }
            }
            if ( waitingReader != null ) {
                DebugIncrement( ref _parent._tryWrite_waiting_reader_is_not_null ); // KILL
                _parent.Log<ChannelMuxInput<TData>>( typeof(TData), nameof(TryWrite), "grabbed a waiting reader, setting result to true" );
                // Waiting reader is present, set its result so that it ends and the waiting reader continues.
                waitingReader.TrySetResult( item: true );
            }
            DebugIncrement( ref _parent._tryWrite_final ); // KILL
            return true;
        }

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


        /// <inheritdoc />
        /// <remarks>
        /// Any waiting readers will only be exited if the queue is empty.
        /// </remarks>
        public override bool TryComplete( Exception? exception = null ) {
            AsyncOperation<bool>? waitingReader = null;
            _parent.Log<ChannelMuxInput<TData>>( typeof(TData), nameof(TryComplete) );

            // If we're already marked as complete, there's nothing more to do.
            if ( _isComplete ) {
                return false;
            }

            // allow the user to ignore or modify the Exception
            exception = _parent.OnChannelComplete?.Invoke( typeof(TData), exception );
            exception?.Data.Add( nameof(ChannelMux) + " Type", typeof(TData) );
            if ( exception is { } ) {
                _parent._hasException = true;
                Interlocked.Exchange( ref _parent._completeException, exception );
                Interlocked.Exchange( ref _parent._completeExceptionChannelDataType, typeof(TData) );
            }

            lock ( _parent._closedChannelLockObj ) {
                // Mark as complete for writing.
                _isComplete = true;
                if ( !_queue.IsEmpty ) {
                    return true;
                }
                _parent.LogWarn<ChannelMuxInput<TData>>( typeof(TData), nameof(TryComplete), $"isComplete={_isComplete}", $"_queue is Empty={_queue.IsEmpty}", $"_parent._closedChannels is {_parent._closedChannels}" );
                _emptyAndComplete = true;
                Interlocked.Increment( ref _parent._closedChannels );
                _isClosed = true;
            }
            // if all channels are closed, or if this complete was reported with an exception, close everything so long as the _queue IsEmpty
            if ( ( _parent._closedChannels >= _parent._totalChannels || exception is { } ) ) {
                _parent.Log<ChannelMuxInput<TData>>( typeof(TData), nameof(TryComplete),
                                                     $"{nameof(exception)}: {( exception is { }
                                                         ? exception.GetType().Name.Split( '_' )[ ^1 ]
                                                         : "null" )}\n\t"                                                +
                                                     $"{nameof(_parent._closedChannels)}: {_parent._closedChannels}\n\t" +
                                                     $"{nameof(_parent._totalChannels)}: {_parent._totalChannels}" );

                // If we have no more items remaining, then the channel needs to be marked as completed
                // and readers need to be informed they'll never get another item.  All of that needs
                // to happen outside of the lock to avoid invoking continuations under the lock.
                _parent.Log<ChannelMuxInput<TData>>( typeof(TData), nameof(ChannelMuxInput<TData>), nameof(TryComplete), "_queue.IsEmpty is TRUE" );
                // TODO: what to do here. If one channel closes should the whole mux stop working? seems like no.
                lock ( _parent._waiterLockObj ) {
                    if ( _parent._waitingReader != null ) {
                        waitingReader            = _parent._waitingReader;
                        _parent._waitingReader   = null;
                        _parent._isReaderWaiting = false;
                    }
                }
                _parent.Log<ChannelMuxInput<TData>>( typeof(TData), nameof(ChannelMuxInput<TData>), nameof(TryComplete), "will complete task" );
                ChannelUtilities.Complete( _parent._completion, exception );
                // Complete a waiting reader if there is one (this is only encountered when _queue.IsEmpty is true
                if ( waitingReader != null ) {
                    _parent.Log<ChannelMuxInput<TData>>( typeof(TData), nameof(ChannelMuxInput<TData>), nameof(TryComplete), "waitingReader != null" );
                    if ( exception != null ) {
                        _parent.Log<ChannelMuxInput<TData>>( typeof(TData), nameof(ChannelMuxInput<TData>), nameof(TryComplete), "waitingReader != null", $"setting exception: {exception.GetType().Name.Split( '_' )[ ^1 ]}" );
                        waitingReader.TrySetException( exception );
                    } else {
                        _parent.Log<ChannelMuxInput<TData>>( typeof(TData), nameof(ChannelMuxInput<TData>), nameof(TryComplete), "waitingReader != null", $"setting result = false" );
                        waitingReader.TrySetResult( item: false );
                    }
                }
            }

            _parent.Log<ChannelMuxInput<TData>>( typeof(TData), nameof(TryComplete), $"Successfully completed the channel{( exception is { } ? " with exception" : String.Empty )}" );
            // Successfully completed the channel
            return true;
        }

        [ SuppressMessage( "ReSharper", "RedundantNullableFlowAttribute" ) ]
        public bool TryRead( [ MaybeNullWhen( false ) ] out TData? item ) {
            _parent.Log<ChannelMuxInput<TData>>( typeof(TData), nameof(TryRead), $"{nameof(_parent._readableItems)}: was {_parent._readableItems}" );
            if ( _queue.TryDequeue( out item ) ) {
                _parent.Log<ChannelMuxInput<TData>>( typeof(TData), $"{nameof(_parent._readableItems)}: was {_parent._readableItems}" );
                Interlocked.Decrement( ref _parent._readableItems );
                _parent.Log<ChannelMuxInput<TData>>( typeof(TData), $"{nameof(_parent._readableItems)}: now {_parent._readableItems}" );
                if ( _isComplete ) {
                    lock ( _parent._closedChannelLockObj ) {
                        if ( !_queue.IsEmpty || _emptyAndComplete ) {
                            return true;
                        }
                        _parent.LogWarn<ChannelMuxInput<TData>>( typeof(TData), nameof(TryRead), "isComplete=true", "_queue is Empty", _queue.IsEmpty.ToString(), $"_parent._closedChannels is {_parent._closedChannels}" );
                        _emptyAndComplete = true;
                        Interlocked.Increment( ref _parent._closedChannels );
                        _isClosed = true;
                    }
                    if ( _parent._areAllChannelsComplete || _parent._hasException ) {
                        ChannelUtilities.Complete( _parent._completion, _parent._completeException );
                    }
                }
                return true;
            }
            return false;
        }

        /// <inheritdoc />
        public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = default ) =>
            // Writing always succeeds (unless we've already completed writing or cancellation has been requested),
            // so just TryWrite and return a completed task.
            cancellationToken.IsCancellationRequested ? new ValueTask( Task.FromCanceled( cancellationToken ) ) :
            TryWrite( item )                          ? default :
                                                        new ValueTask( Task.FromException( ChannelUtilities.CreateInvalidCompletionException( _parent._completeException ) ) );


        /*
         * IEnumerable implementation
         */

        /// <inheritdoc />
        public bool IsEmpty => this._queue.IsEmpty;

        /// <inheritdoc />
        public bool IsComplete => this._isComplete;

        /// <inheritdoc />
        public bool IsClosed {
            get {
                lock ( _parent._closedChannelLockObj ) {
                    return this._isClosed;
                }
            }
        }

        /*
         * IEnumerable implementation
         */

        /// <inheritdoc />
        public IEnumerator<TData> GetEnumerator( ) => this._queue.GetEnumerator();

        /// <inheritdoc />
        IEnumerator IEnumerable.GetEnumerator( ) {
            return GetEnumerator();
        }

        /*
         * IDisposable implementation
         */

        private bool _isDisposed = false;

        /// <inheritdoc />
        public void Dispose( ) {
            if ( !_isDisposed ) {
                _removeWriterCallback.Invoke( this.GetHashCode() );
                _isDisposed = true;
            }
        }
    }
}

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Note that more generic parameters can easily be added by inheriting from this class and additional type params.
/// </remarks>
/// <typeparam name="T1"></typeparam>
/// <typeparam name="T2"></typeparam>
public class ChannelMux<T1, T2> : ChannelMux, IDisposable {
    private ChannelMuxInput<T1> _input1;
    private ChannelMuxInput<T2> _input2;

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

    /// <summary>
    /// Replace the <see cref="Channel"/> of the same data type with <paramref name="newChannel"/>.
    ///
    /// <list type="bullet">
    ///     <item>
    ///         <description><see cref="ChannelMux._completeException"/> will be set to <c>null</c> if set by the channel being replaced.</description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="ChannelMux._hasException"/> will be set to <c>false</c> if set by the channel being replaced.</description>
    ///     </item>
    ///     <item>
    ///         <description><see cref="ChannelMux._closedChannels"/> will be decremented by 1 if the channel was closed (completed and empty).</description>
    ///     </item>
    ///     <item>
    ///         <description>The <see cref="ChannelMux.Completion"/> task will be created new if <see cref="Task.IsCompleted"/>.</description>
    ///     </item>
    ///     <item>
    ///         <description>The prior <see cref="Channel"/> will have the reader associated with this <see cref="ChannelMux"/> removed.</description>
    ///     </item>
    /// </list>
    /// </summary>
    /// <param name="newChannel">
    ///     Channel that will replace the channel of the matching type.
    /// </param>
    /// <param name="force">
    ///     If set to <c>true</c>, the channel will be replaced regardless of whether <see cref="ChannelWriter{T}.Complete"/> has been called.
    /// </param>
    /// <returns>
    ///     Any data remaining in the channel being replaced.
    /// </returns>
    /// <exception cref="ChannelNotClosedException">
    ///     If the <see cref="Channel"/> being replaced is not complete.
    ///     This can be overriden by setting <paramref name="force"/> to <c>true</c>.
    /// </exception>
    public IEnumerable<T1> ReplaceChannel( BroadcastChannelWriter<T1, IBroadcastChannelResponse> newChannel, bool force = false ) {
        if ( force || this._input1.IsComplete ) {
            this.resetOneChannel(this._input1);
            var oldMuxInput = Interlocked.Exchange( ref _input1, new ChannelMuxInput<T1>( newChannel, this ) );
            oldMuxInput.Dispose();
            return oldMuxInput;
        }
        return ChannelNotClosedException.Throw<IEnumerable<T1>>();
    }

    /// <inheritdoc cref="M:BroadcastChannelMux.ChannelMux`2.ReplaceChannel(BroadcastChannel.BroadcastChannelWriter{`0,BroadcastChannel.IBroadcastChannelResponse},System.Boolean)" />
    public IEnumerable<T2> ReplaceChannel( BroadcastChannelWriter<T2, IBroadcastChannelResponse> newChannel, bool force = false ) {
        if ( force || this._input2.IsComplete ) {
            this.resetOneChannel(this._input2);
            var oldMuxInput = Interlocked.Exchange( ref _input2, new ChannelMuxInput<T2>( newChannel, this ) );
            oldMuxInput.Dispose();
            return oldMuxInput;
        }
        return ChannelNotClosedException.Throw<IEnumerable<T2>>();
    }

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
    private ChannelMuxInput<T3> _input;

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

    /// <inheritdoc cref="M:BroadcastChannelMux.ChannelMux`3.ReplaceChannel(BroadcastChannel.BroadcastChannelWriter{`0,BroadcastChannel.IBroadcastChannelResponse},System.Boolean)" />
    public IEnumerable<T3> ReplaceChannel( BroadcastChannelWriter<T3, IBroadcastChannelResponse> newChannel, bool force = false ) {
        if ( force || this._input.IsComplete ) {
            this.resetOneChannel(this._input);
            var oldMuxInput = Interlocked.Exchange( ref _input, new ChannelMuxInput<T3>( newChannel, this ) );
            oldMuxInput.Dispose();
            return oldMuxInput;
        }
        return ChannelNotClosedException.Throw<IEnumerable<T3>>();
    }

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
    private ChannelMuxInput<T4> _input;

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

    /// <inheritdoc cref="M:BroadcastChannelMux.ChannelMux`4.ReplaceChannel(BroadcastChannel.BroadcastChannelWriter{`0,BroadcastChannel.IBroadcastChannelResponse},System.Boolean)" />
    public IEnumerable<T4> ReplaceChannel( BroadcastChannelWriter<T4, IBroadcastChannelResponse> newChannel, bool force = false ) {
        if ( force || this._input.IsComplete ) {
            this.resetOneChannel( this._input );
            var oldMuxInput = Interlocked.Exchange( ref _input, new ChannelMuxInput<T4>( newChannel, this ) );
            oldMuxInput.Dispose();
            return oldMuxInput;
        }
        return ChannelNotClosedException.Throw<IEnumerable<T4>>();
    }

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
    private ChannelMuxInput<T5> _input;

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

    /// <inheritdoc cref="M:BroadcastChannelMux.ChannelMux`5.ReplaceChannel(BroadcastChannel.BroadcastChannelWriter{`0,BroadcastChannel.IBroadcastChannelResponse},System.Boolean)" />
    public IEnumerable<T5> ReplaceChannel( BroadcastChannelWriter<T5, IBroadcastChannelResponse> newChannel, bool force = false ) {
        if ( force || this._input.IsComplete ) {
            this.resetOneChannel( this._input );
            var oldMuxInput = Interlocked.Exchange( ref _input, new ChannelMuxInput<T5>( newChannel, this ) );
            oldMuxInput.Dispose();
            return oldMuxInput;
        }
        return ChannelNotClosedException.Throw<IEnumerable<T5>>();
    }

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
    private ChannelMuxInput<T6> _input;

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

    /// <inheritdoc cref="M:BroadcastChannelMux.ChannelMux`6.ReplaceChannel(BroadcastChannel.BroadcastChannelWriter{`0,BroadcastChannel.IBroadcastChannelResponse},System.Boolean)" />
    public IEnumerable<T6> ReplaceChannel( BroadcastChannelWriter<T6, IBroadcastChannelResponse> newChannel, bool force = false ) {
        if ( force || this._input.IsComplete ) {
            this.resetOneChannel( this._input );
            var oldMuxInput = Interlocked.Exchange( ref _input, new ChannelMuxInput<T6>( newChannel, this ) );
            oldMuxInput.Dispose();
            return oldMuxInput;
        }
        return ChannelNotClosedException.Throw<IEnumerable<T6>>();
    }

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
    private ChannelMuxInput<T7> _input;

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

    /// <inheritdoc cref="M:BroadcastChannelMux.ChannelMux`7.ReplaceChannel(BroadcastChannel.BroadcastChannelWriter{`0,BroadcastChannel.IBroadcastChannelResponse},System.Boolean)" />
    public IEnumerable<T7> ReplaceChannel( BroadcastChannelWriter<T7, IBroadcastChannelResponse> newChannel, bool force = false ) {
        if ( force || this._input.IsComplete ) {
            this.resetOneChannel( this._input );
            var oldMuxInput = Interlocked.Exchange( ref _input, new ChannelMuxInput<T7>( newChannel, this ) );
            oldMuxInput.Dispose();
            return oldMuxInput;
        }
        return ChannelNotClosedException.Throw<IEnumerable<T7>>();
    }

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
    private ChannelMuxInput<T8> _input;

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

    /// <inheritdoc cref="M:BroadcastChannelMux.ChannelMux`8.ReplaceChannel(BroadcastChannel.BroadcastChannelWriter{`0,BroadcastChannel.IBroadcastChannelResponse},System.Boolean)" />
    public IEnumerable<T8> ReplaceChannel( BroadcastChannelWriter<T8, IBroadcastChannelResponse> newChannel, bool force = false ) {
        if ( force || this._input.IsComplete ) {
            this.resetOneChannel( this._input );
            var oldMuxInput = Interlocked.Exchange( ref _input, new ChannelMuxInput<T8>( newChannel, this ) );
            oldMuxInput.Dispose();
            return oldMuxInput;
        }
        return ChannelNotClosedException.Throw<IEnumerable<T8>>();
    }

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

/// <summary>
/// <see cref="Channel"/> is not closed, mutate actions not available.
/// </summary>
public class ChannelNotClosedException : Exception {
    /// <inheritdoc cref="ChannelNotClosedException" />
    private ChannelNotClosedException( string? msg = null ) : base( msg ?? "Channel is not closed, mutate actions not available." ) { }

    /// <inheritdoc cref="ChannelNotClosedException" />
    [ DoesNotReturn ]
    [ StackTraceHidden ]
    public static void Throw( ) {
        throw new ChannelNotClosedException();
    }

    /// <inheritdoc cref="ChannelNotClosedException" />
    [ DoesNotReturn ]
    [ StackTraceHidden ]
    public static TReturn Throw<TReturn>( ) {
        throw new ChannelNotClosedException();
    }
}