using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace BroadcastChannelMux; 


    /// <summary>Provides internal helper methods for implementing channels.</summary>
    internal static class ChannelUtilities
    {
        /// <summary>Sentinel object used to indicate being done writing.</summary>
        internal static readonly Exception s_doneWritingSentinel = new Exception(nameof(s_doneWritingSentinel));
        /// <summary>A cached task with a Boolean true result.</summary>
        internal static readonly Task<bool> s_trueTask = Task.FromResult(result: true);
        /// <summary>A cached task with a Boolean false result.</summary>
        internal static readonly Task<bool> s_falseTask = Task.FromResult(result: false);
        /// <summary>A cached task that never completes.</summary>
        internal static readonly Task s_neverCompletingTask = new TaskCompletionSource<bool>().Task;
 
        /// <summary>Completes the specified TaskCompletionSource.</summary>
        /// <param name="tcs">The source to complete.</param>
        /// <param name="error">
        /// The optional exception with which to complete.
        /// If this is null or the DoneWritingSentinel, the source will be completed successfully.
        /// If this is an OperationCanceledException, it'll be completed with the exception's token.
        /// Otherwise, it'll be completed as faulted with the exception.
        /// </param>
        internal static void Complete(TaskCompletionSource tcs, Exception? error = null)
        {
            if (error is OperationCanceledException oce)
            {
                tcs.TrySetCanceled(oce.CancellationToken);
            }
            else if (error != null && error != s_doneWritingSentinel)
            {
                if (tcs.TrySetException(error))
                {
                    // Suppress unobserved exceptions from Completion tasks, as the exceptions will generally
                    // have been surfaced elsewhere (which may end up making a consumer not consume the completion
                    // task), and even if they weren't, they're created by a producer who will have "seen" them (in
                    // contrast to them being created by some method call failing as part of user code).
                    _ = tcs.Task.Exception;
                }
            }
            else
            {
                tcs.TrySetResult();
            }
        }
 
        /// <summary>Gets a value task representing an error.</summary>
        /// <typeparam name="T">Specifies the type of the value that would have been returned.</typeparam>
        /// <param name="error">The error.  This may be <see cref="s_doneWritingSentinel"/>.</param>
        /// <returns>The failed task.</returns>
        internal static ValueTask<T> GetInvalidCompletionValueTask<T>(Exception error)
        {
            Debug.Assert(error != null);
 
            Task<T> t =
                error == s_doneWritingSentinel ? Task.FromException<T>(CreateInvalidCompletionException()) :
                error is OperationCanceledException oce ? Task.FromCanceled<T>(oce.CancellationToken.IsCancellationRequested ? oce.CancellationToken : new CancellationToken(true)) :
                Task.FromException<T>(CreateInvalidCompletionException(error));
 
            return new ValueTask<T>(t);
        }
 
        internal static void QueueWaiter(ref AsyncOperation<bool>? tail, AsyncOperation<bool> waiter)
        {
            AsyncOperation<bool>? c = tail;
            if (c == null)
            {
                waiter.Next = waiter;
            }
            else
            {
                waiter.Next = c.Next;
                c.Next = waiter;
            }
            tail = waiter;
        }
 
        internal static void WakeUpWaiters(ref AsyncOperation<bool>? listTail, bool result, Exception? error = null)
        {
            AsyncOperation<bool>? tail = listTail;
            if (tail != null)
            {
                listTail = null;
 
                AsyncOperation<bool> head = tail.Next!;
                AsyncOperation<bool> c = head;
                do
                {
                    AsyncOperation<bool> next = c.Next!;
                    c.Next = null;
 
                    bool completed = error != null ? c.TrySetException(error) : c.TrySetResult(result);
                    Debug.Assert(completed || c.CancellationToken.CanBeCanceled);
 
                    c = next;
                }
                while (c != head);
            }
        }
 
        /// <summary>Removes all operations from the queue, failing each.</summary>
        /// <param name="operations">The queue of operations to complete.</param>
        /// <param name="error">The error with which to complete each operations.</param>
        internal static void FailOperations<T, TInner>(Deque<T> operations, Exception error) where T : AsyncOperation<TInner>
        {
            Debug.Assert(error != null);
            while (!operations.IsEmpty)
            {
                operations.DequeueHead().TrySetException(error);
            }
        }
 
        /// <summary>Creates and returns an exception object to indicate that a channel has been closed.</summary>
        internal static Exception CreateInvalidCompletionException(Exception? inner = null) =>
            inner is OperationCanceledException ? inner :
            inner != null && inner != s_doneWritingSentinel ? new ChannelClosedException(inner) :
            new ChannelClosedException();
    }