#undef DEBUG

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using BroadcastChannel;

namespace BroadcastChannelMux;

public abstract class ChannelMux {
    private readonly bool _runContinuationsAsynchronously;
    /// <summary>A waiting reader (e.g. WaitForReadAsync) if there is one.</summary>
    private AsyncOperation<bool>? _waitingReader;
    private readonly AsyncOperation<bool> _waiterSingleton;

    private volatile int  _readableItems  = 0;
    private volatile int  _closedChannels = 0;
    private readonly int  _totalChannels;
    private          bool _areAllChannelsComplete => _closedChannels >= _totalChannels;

    // private readonly Func<AggregateException?> _getAllExceptions;
    protected abstract AggregateException? getAllExceptions( );

    /// <summary>Task that indicates the channel has completed.</summary>
    private readonly TaskCompletionSource _completion;
    private readonly object _lockObj = new object();

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
        Log( nameof(WaitToReadAsync), $"{nameof(_readableItems)}: {_readableItems}" );
        // Outside of the lock, check if there are any items waiting to be read.  If there are, we're done.
        if ( cancellationToken.IsCancellationRequested ) {
            return new ValueTask<bool>( Task.FromCanceled<bool>( cancellationToken ) );
        }

        if ( _readableItems > 0 ) {
            Log( nameof(WaitToReadAsync), $"_readableItems > 0 : {_readableItems}" );
            return new ValueTask<bool>( true );
        }
        AsyncOperation<bool>? oldWaitingReader, newWaitingReader;
        lock ( _lockObj ) {
            Log( nameof(WaitToReadAsync), "in lock" );
            // Again while holding the lock, check to see if there are any items available.
            if ( _readableItems > 0 ) {
                Log( nameof(WaitToReadAsync), "in lock", $"_readableItems > 0 : {_readableItems}" );
                return new ValueTask<bool>( true );
            }
            // There aren't any items; if we're done writing, there never will be more items.
            if ( _areAllChannelsComplete ) {
                Log( nameof(WaitToReadAsync), "_allChannelsDoneWriting" );
                // if its not the known to be a non-error s_doneWritingSentinel, then return a cancelled ValueTask with the exception contained in s_doneWritingSentinel.
                return getAllExceptions() is { } allExceptions ? new ValueTask<bool>( Task.FromException<bool>( allExceptions ) ) : default;
            }
            // Try to use the singleton waiter.  If it's currently being used, then the channel
            // is being used erroneously, and we cancel the outstanding operation.
            oldWaitingReader = _waitingReader;
            if ( !cancellationToken.CanBeCanceled && _waiterSingleton.TryOwnAndReset() ) {
                Log( nameof(WaitToReadAsync), "!cancellationToken.CanBeCanceled && _waiterSingleton.TryOwnAndReset()" );
                newWaitingReader = _waiterSingleton;
                if ( newWaitingReader == oldWaitingReader ) {
                    Log( nameof(WaitToReadAsync), "newWaitingReader == oldWaitingReader" );
                    // The previous operation completed, so null out the "old" waiter
                    // so we don't end up canceling the new operation.
                    oldWaitingReader = null;
                }
            } else {
                Log( nameof(WaitToReadAsync), "ELSE" );
                newWaitingReader = new AsyncOperation<bool>( _runContinuationsAsynchronously, cancellationToken );
            }
            Log( nameof(WaitToReadAsync), $"newWaitingReader is {newWaitingReader}" );
            _waitingReader = newWaitingReader;
        }

        if ( _readableItems > 0 ) { // KILL ??
            Log( nameof(WaitToReadAsync), $"_readableItems > 0 : {_readableItems}" );
            return new ValueTask<bool>( true );
        }
        
        Log( nameof(WaitToReadAsync), $"oldWaitingReader is {newWaitingReader}" );
        oldWaitingReader?.TrySetCanceled( default );
        return newWaitingReader.ValueTaskOfT;
    }

    [ Conditional( "DEBUG" ) ]
    protected void Log( params string[] contextAndMessage ) {
        Console.WriteLine( $"{this.GetType().Name} > {String.Join( " > ", contextAndMessage )}" );
    }


    //


    protected sealed class ChannelMuxInput<TData> : ChannelWriter<TData>, IDisposable {
        private readonly ChannelMux                               _parent;
        private readonly RemoveWriterByHashCode                   _removeWriterCallback;
        private readonly SingleProducerSingleConsumerQueue<TData> _queue = new SingleProducerSingleConsumerQueue<TData>(); // TODO: convert to SingleProducerSingleConsumerQueue
        // private readonly ConcurrentQueue<TData> _queue = new ConcurrentQueue<TData>(); // TODO: convert to SingleProducerSingleConsumerQueue

        /// <summary>non-null if the channel has been marked as complete for writing.</summary>
        private volatile Exception? _doneWriting;

        internal Exception? Exception => _doneWriting == ChannelUtilities.s_doneWritingSentinel ? null : _doneWriting;

        internal ChannelMuxInput( BroadcastChannelWriter<TData, IBroadcastChannelResponse> channel, ChannelMux parent ) {
            _removeWriterCallback = channel.AddReader( this );
            _parent               = parent;
            _parent.Log( nameof(ChannelMuxInput<TData>), "constructor" );
        }

        /// <inheritdoc />
        public override bool TryWrite( TData item ) {
            _parent.Log( nameof(TryWrite) );
            if ( _doneWriting != null ) {
                return false;
            }

            _queue.Enqueue( item );
            Interlocked.Increment( ref _parent._readableItems );
            // lock ( _parent._lockObj ) { // URGENT: try Monitor.TryEnter here !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            if ( Monitor.TryEnter( _parent._lockObj ) ) {
                AsyncOperation<bool>? waitingReader = null;
                try {
                    _parent.Log( nameof(ChannelMuxInput<TData>), nameof(TryWrite), "in lock" );
                    waitingReader = _parent._waitingReader;
                    if ( waitingReader == null ) {
                        _parent.Log( nameof(TryWrite), "waitingReader is null" );
                        return true;
                    }
                    _parent._waitingReader = null; // URGENT: try replacing the lock with a volatile bool
                } finally {
                    // Ensure that the lock is released.
                    Monitor.Exit( _parent._lockObj );
                }
                _parent.Log( nameof(TryWrite), "grabbed a waiting reader, setting result to true" );
                // If we get here, we grabbed a waiting reader.
                waitingReader?.TrySetResult( item: true );
            }
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

        public bool IsComplete( ) => _doneWriting is not null;

        /// <inheritdoc />
        /// <remarks>
        /// This will always return immediately.
        /// </remarks>
        public override ValueTask<bool> WaitToWriteAsync( CancellationToken cancellationToken = new CancellationToken() ) {
            Exception? doneWriting = _doneWriting;
            return cancellationToken.IsCancellationRequested          ? new ValueTask<bool>( Task.FromCanceled<bool>( cancellationToken ) ) :
                doneWriting == null                                   ? new ValueTask<bool>( true ) :
                doneWriting != ChannelUtilities.s_doneWritingSentinel ? new ValueTask<bool>( Task.FromException<bool>( doneWriting ) ) :
                                                                        default;
        }

        public override bool TryComplete( Exception? exception = null ) {
            AsyncOperation<bool>? waitingReader = null;
            bool                  completeTask  = false;
            _parent.Log( nameof(ChannelMuxInput<TData>), nameof(TryComplete) );

            // If we're already marked as complete, there's nothing more to do.
            if ( _doneWriting != null ) {
                return false;
            }

            // Mark as complete for writing.
            _doneWriting = exception ?? ChannelUtilities.s_doneWritingSentinel;

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
                if ( waitingReader != null && _parent._closedChannels >= _parent._totalChannels ) {
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
                if ( _doneWriting != null && _queue.IsEmpty ) {
                    ChannelUtilities.Complete( _parent._completion, _doneWriting );
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
                                                        new ValueTask( Task.FromException( ChannelUtilities.CreateInvalidCompletionException( _doneWriting ) ) );

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

    /// <inheritdoc />
    protected override AggregateException? getAllExceptions( ) {
        List<Exception> exceptions = new ();
        if ( _input1.Exception is { } ) {
            exceptions.Add( _input1.Exception );
        }
        if ( _input2.Exception is { } ) {
            exceptions.Add( _input2.Exception );
        }
        return exceptions.Count != 0 ? new AggregateException( exceptions ) : null;
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    [ SuppressMessage( "ReSharper", "RedundantNullableFlowAttribute" ) ]
    public bool TryRead( [ MaybeNullWhen( false ) ] out T1? item ) => _input1.TryRead( out item );

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    [ SuppressMessage( "ReSharper", "RedundantNullableFlowAttribute" ) ]
    public bool TryRead( [ MaybeNullWhen( false ) ] out T2? item ) => _input2.TryRead( out item );

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

    public ChannelMux( BroadcastChannelWriter<T1> channel1, BroadcastChannelWriter<T2> channel2, BroadcastChannelWriter<T3> channel3 ) : this( channel1, channel2, channel3, totalChannels: 3 ) { }

    protected ChannelMux( BroadcastChannelWriter<T1> channel1, BroadcastChannelWriter<T2> channel2, BroadcastChannelWriter<T3> channel3, int totalChannels ) : base( channel1, channel2, totalChannels: totalChannels ) {
        _input = new ChannelMuxInput<T3>( channel3, this );
    }

    /// <inheritdoc />
    protected override AggregateException? getAllExceptions( ) {
        List<Exception> exceptions = new ();
        if ( base.getAllExceptions() is { } baseExceptions ) {
            exceptions.AddRange( baseExceptions.InnerExceptions );
        }
        if ( _input.Exception is { } ) {
            exceptions.Add( _input.Exception );
        }
        return exceptions.Count != 0 ? new AggregateException( exceptions ) : null;
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

/*
 * DEMO
 */

public struct StructA {
    public          int    Id;
    public          string Name;
    public          string Description;
    public override string ToString( ) => $"{nameof(StructA)} {{ Id = {Id}, Name = {Name}, Description = {Description} }}";
}

public class ClassA {
    public          int    Id;
    public          string Name        = String.Empty;
    public          string Description = String.Empty;
    public override string ToString( ) => $"{nameof(ClassA)} {{ Id = {Id}, Name = {Name}, Description = {Description} }}";
}

public class MuxConsumerDemo {
    public static async Task UseMux( BroadcastChannelWriter<StructA> writer1, BroadcastChannelWriter<ClassA> writer2 ) {
        ChannelMux<StructA, ClassA> mux = new (writer1, writer2);
        while ( await mux.WaitToReadAsync( CancellationToken.None ) ) {
            if ( mux.TryRead( out ClassA? classA ) ) {
                Console.WriteLine( classA );
                // do stuff
            }
            if ( mux.TryRead( out StructA structA ) ) {
                Console.WriteLine( structA );
                // do stuff
            }
        }
    }
}