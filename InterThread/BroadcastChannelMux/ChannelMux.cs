using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using BroadcastChannel;

namespace BroadcastChannelMux;

public abstract class ChannelMux {
    protected sealed class ChannelMuxWriter<TData> : ChannelWriter<TData>, IDisposable {
        private  RemoveWriterByHashCode? _removeWriterCallback;
        internal ConcurrentQueue<TData>  Queue = new ConcurrentQueue<TData>();

        internal void AddRemoveWriterCallback( RemoveWriterByHashCode callback ) => _removeWriterCallback = callback;

        /// <inheritdoc />
        public override bool TryWrite( TData item ) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override ValueTask<bool> WaitToWriteAsync( CancellationToken cancellationToken = new CancellationToken() ) {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override bool TryComplete( Exception? error = null ) {
            return base.TryComplete( error );
        }

        /// <inheritdoc />
        public override ValueTask WriteAsync( TData item, CancellationToken cancellationToken = new CancellationToken() ) {
            return base.WriteAsync( item, cancellationToken );
        }

        /// <inheritdoc />
        public void Dispose( ) {
            _removeWriterCallback?.Invoke( this.GetHashCode() );
        }
    }
}

public class ChannelMux<T1, T2> : ChannelMux, IDisposable {
    // TODO: should handle responses? (the second type arg of BroadcastChannel)
    private readonly ChannelMuxWriter<T1> _input1 = new ChannelMuxWriter<T1>();
    private readonly ChannelMuxWriter<T2> _input2 = new ChannelMuxWriter<T2>();

    public ChannelMux( BroadcastChannelWriter<T1> channel1, BroadcastChannelWriter<T2> channel2 ) {
        _input1.AddRemoveWriterCallback( channel1.AddReader( _input1 ) );
        _input2.AddRemoveWriterCallback( channel2.AddReader( _input2 ) );
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelWriter{T}.TryWrite"/>
    public bool TryWrite( T1 item ) {
        _input1.Queue.Enqueue( item );
        return true;
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelWriter{T}.TryWrite"/>
    public bool TryWrite( T2 item ) {
        _input2.Queue.Enqueue( item );
        return true;
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    public bool TryRead( out T1? item ) {
        return _input1.Queue.TryDequeue( out item );
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    public bool TryRead( out T2? item ) {
        return _input2.Queue.TryDequeue( out item );
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.WaitToReadAsync"/>
    public virtual ValueTask<bool> WaitToReadAsync( CancellationToken ct ) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Dispose( ) {
        // URGENT: do this properly
        _input1.Dispose();
        _input2.Dispose();
    }
}

public class ChannelMux<T1, T2, T3> : ChannelMux<T1, T2>, IDisposable { // NOTE: can easily add more generic params ;; // TODO: BENCHMARK THAT THIS DOESN'T HURT PERFORMANCE!
    // TODO: should handle responses? (the second type arg of BroadcastChannel)
    private readonly ChannelMuxWriter<T3> _input = new ChannelMuxWriter<T3>();

    public ChannelMux( BroadcastChannelWriter<T1> channel1, BroadcastChannelWriter<T2> channel2, BroadcastChannelWriter<T3> channel3 )
        : base( channel1, channel2 ) {
        _input.AddRemoveWriterCallback( channel3.AddReader( channel3 ) );
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelWriter{T}.TryWrite"/>
    public bool TryWrite( T3 item ) {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    public bool TryRead( out T3 item ) {
        throw new NotImplementedException();
    }

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.WaitToReadAsync"/>
    public override ValueTask<bool> WaitToReadAsync( CancellationToken ct ) {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Dispose( ) {
        // URGENT: do this properly
        _input.Dispose();
        base.Dispose();
    }
}

public struct StructA {
    public int    Id;
    public string Name;
    public string Description;
}

public class ClassA {
    public int    Id;
    public string Name;
    public string Description;
}

public class MuxConsumerDemo {
    public static async Task UseMux( BroadcastChannelWriter<StructA> writer1, BroadcastChannelWriter<ClassA> writer2 ) {
        ChannelMux<StructA, ClassA> mux = new (writer1, writer2);
        while ( await mux.WaitToReadAsync( CancellationToken.None ) ) {
            if ( mux.TryRead( out ClassA classA ) ) {
                // do stuff
            }
            if ( mux.TryRead( out StructA structA ) ) {
                // do stuff
            }
        }
    }
}

//
// public class ChannelMux<T1,T2> {
//     
// }
//
// public class BroadcastChannel<T> {
//     private IChannelReader<T> 
//     /// <summary>Task that indicates the channel has completed.</summary>
//     private readonly TaskCompletionSource _completion;
//     /// <summary>The items in the channel.</summary>
//     private readonly ConcurrentQueue<T> _items = new ConcurrentQueue<T>();
//     /// <summary>Readers blocked reading from the channel.</summary>
//     private readonly Deque<AsyncOperation<T>> _blockedReaders = new Deque<AsyncOperation<T>>();
//     /// <summary>Whether to force continuations to be executed asynchronously from producer writes.</summary>
//     private readonly bool _runContinuationsAsynchronously;
//  
//     /// <summary>Readers waiting for a notification that data is available.</summary>
//     private AsyncOperation<bool>? _waitingReadersTail;
//     /// <summary>Set to non-null once Complete has been called.</summary>
//     private Exception? _doneWriting;
//  
//     /// <summary>Initialize the channel.</summary>
//     internal BroadcastChannel(bool runContinuationsAsynchronously)
//     {
//         _runContinuationsAsynchronously = runContinuationsAsynchronously;
//         _completion                     = new TaskCompletionSource(runContinuationsAsynchronously ? TaskCreationOptions.RunContinuationsAsynchronously : TaskCreationOptions.None);
//         Reader                          = new UnboundedChannelReader(this);
//         Writer                          = new UnboundedChannelWriter(this);
//     }
// }
//
// public class ChannelWriter<T> : System.Threading.Channels.ChannelWriter<T> {
//     public          bool TryComplete( Exception? error ) {}
//
//     public bool TryWrite( T item ) {
//         throw new NotImplementedException();
//     }
//
//     public override ValueTask<bool> WaitToWriteAsync(CancellationToken cancellationToken)
//     {
//         Exception? doneWriting = _parent._doneWriting;
//         return
//             cancellationToken.IsCancellationRequested             ? new ValueTask<bool>(Task.FromCanceled<bool>(cancellationToken)) :
//             doneWriting == null                                   ? new ValueTask<bool>(true) :
//             doneWriting != ChannelUtilities.s_doneWritingSentinel ? new ValueTask<bool>(Task.FromException<bool>(doneWriting)) :
//                                                                     default;
//     }
//  
//     public override ValueTask WriteAsync(T item, CancellationToken cancellationToken) =>
//         // Writing always succeeds (unless we've already completed writing or cancellation has been requested),
//         // so just TryWrite and return a completed task.
//         cancellationToken.IsCancellationRequested ? new ValueTask(Task.FromCanceled(cancellationToken)) :
//         TryWrite(item)                            ? default :
//                                                     new ValueTask(Task.FromException(ChannelUtilities.CreateInvalidCompletionException(_parent._doneWriting)));
//  
//     /// <summary>Gets the number of items in the channel. This should only be used by the debugger.</summary>
//     private int ItemsCountForDebugger => _parent._items.Count;
//  
//     /// <summary>Gets an enumerator the debugger can use to show the contents of the channel.</summary>
//     IEnumerator<T> IDebugEnumerable<T>.GetEnumerator() => _parent._items.GetEnumerator();
// }