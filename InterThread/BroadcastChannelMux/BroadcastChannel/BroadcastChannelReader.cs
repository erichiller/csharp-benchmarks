using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace BroadcastChannel;

// public interface IChannelReader<T> {
//     
//     /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
//     public bool TryRead( [ MaybeNullWhen( false ) ] out TData item ) => !this._isDisposed ? this._dataReader.TryRead( out item ) : ThrowHelper.ThrowObjectDisposedException( nameof(BroadcastChannelReader<TData, TResponse>), out item );
//
//     /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryPeek"/>
//     public override bool TryPeek( [ MaybeNullWhen( false ) ] out TData item ) => !this._isDisposed ? this._dataReader.TryPeek( out item ) : ThrowHelper.ThrowObjectDisposedException( nameof(BroadcastChannelReader<TData, TResponse>), out item );
//
// }

// public interface IBroadcastChannelReader<TData, in TResponse> : IDisposable where TResponse : IBroadcastChannelResponse {
//     /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
//     bool TryRead( [ MaybeNullWhen( false ) ] out TData item );
//
//     /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryPeek"/>
//     bool TryPeek( [ MaybeNullWhen( false ) ] out TData item );
//
//
//     ValueTask<bool> WaitToReadAsync( CancellationToken cancellationToken = default );
//
//
//     IAsyncEnumerable<TData> ReadAllAsync( [ EnumeratorCancellation ] CancellationToken cancellationToken = default );
//
//     /// <inheritdoc cref="ChannelWriter{T}.WriteAsync" />
//     ValueTask WriteResponseAsync( TResponse response, CancellationToken cancellationToken = default );
//
//
//     Task Completion { get; }
//
//     int Count { get; }
//
//     bool CanCount { get; }
//
//     bool CanPeek { get; }
//     // ValueTask ReadAsync(CancellationToken cancellationToken); // URGENT: Restore?
// }

// public interface IBroadcastChannelReaderSimple<TData> {
//     /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
//     bool TryRead( [ MaybeNullWhen( false ) ] out TData item );
// }

// public interface IBroadcastChannelWriterSimple<T1,T2> {
//     /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
//     bool TryWrite( T1 item );
//     /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
//     bool TryWrite( T2 item );
// }


/// <summary>
/// 
/// </summary>
/// <typeparam name="TData">Type of data the <see cref="BroadcastChannelReader{TData,TResponse}"/> will receive.</typeparam>
/// <typeparam name="TResponse">Type of data the <see cref="BroadcastChannelReader{TData,TResponse}"/> will send.</typeparam>
public class BroadcastChannelReader<TData, TResponse> : ChannelReader<TData> where TResponse : IBroadcastChannelResponse {
    private readonly RemoveWriterByHashCode _removeReader;
    private readonly ChannelWriter<TResponse>                                       _responseWriter;
    private readonly ChannelReader<TData>                                           _dataReader;
    private          bool                                                           _isDisposed;
    private readonly int                                                            _writerHash;
    private readonly ILogger<BroadcastChannelReader<TData, TResponse>>              _logger;

    internal BroadcastChannelReader(
        ChannelReader<TData>                              dataReader,
        int                                               inputDataWriterHashCode,
        ChannelWriter<TResponse>                          responseWriter,
        RemoveWriterByHashCode  removeReaderFunction,
        ILogger<BroadcastChannelReader<TData, TResponse>> logger
    ) {
        this._writerHash     = inputDataWriterHashCode;
        this._logger         = logger;
        this._removeReader   = removeReaderFunction;
        this._responseWriter = responseWriter;
        this._dataReader     = dataReader;
    }

    /// <summary>
    /// This is only for Dependency Injection purposes and should not be used by the user. Instead use <see cref="BroadcastChannelWriter{TData,TResponse}.GetReader"/>
    /// </summary>
    /// <param name="broadcastChannelWriter"></param>
    [ EditorBrowsable( EditorBrowsableState.Never ) ]
    public BroadcastChannelReader( BroadcastChannelWriter<TData, TResponse> broadcastChannelWriter ) {
        ArgumentNullException.ThrowIfNull( broadcastChannelWriter );
        ( this._dataReader, this._writerHash, this._removeReader, this._responseWriter, this._logger ) = broadcastChannelWriter.RegisterReader( this );
        this._logger.LogTrace( "Registered with Writer: {Writer}", broadcastChannelWriter );
    }

    /* ************************************************** */

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryRead"/>
    public override bool TryRead( [ MaybeNullWhen( false ) ] out TData item ) => !this._isDisposed ? this._dataReader.TryRead( out item ) : ThrowHelper.ThrowObjectDisposedException( nameof(BroadcastChannelReader<TData, TResponse>), out item );

    /// <inheritdoc cref="System.Threading.Channels.ChannelReader{T}.TryPeek"/>
    public override bool TryPeek( [ MaybeNullWhen( false ) ] out TData item ) => !this._isDisposed ? this._dataReader.TryPeek( out item ) : ThrowHelper.ThrowObjectDisposedException( nameof(BroadcastChannelReader<TData, TResponse>), out item );


    public override ValueTask<bool> WaitToReadAsync( CancellationToken cancellationToken = default ) =>
        !this._isDisposed
            ? this._dataReader.WaitToReadAsync( cancellationToken )
            : ThrowHelper.ThrowObjectDisposedException<ValueTask<bool>>( nameof(BroadcastChannelReader<TData, TResponse>) );

#pragma warning disable CS8424

    public override IAsyncEnumerable<TData> ReadAllAsync( [ EnumeratorCancellation ] CancellationToken cancellationToken = default )
        => !this._isDisposed ? this._dataReader.ReadAllAsync( cancellationToken ) : ThrowHelper.ThrowObjectDisposedException<IAsyncEnumerable<TData>>( nameof(BroadcastChannelReader<TData, TResponse>) );
#pragma warning restore CS8424

    /// <inheritdoc cref="ChannelWriter{T}.WriteAsync" />
    public ValueTask WriteResponseAsync( TResponse response, CancellationToken cancellationToken = default ) => this._responseWriter.WriteAsync( response, cancellationToken );


    public override Task Completion => !this._isDisposed ? this._dataReader.Completion : ThrowHelper.ThrowObjectDisposedException<Task>( nameof(BroadcastChannelReader<TData, TResponse>) );


    public override int Count => !this._isDisposed ? this._dataReader.Count : ThrowHelper.ThrowObjectDisposedException<int>( nameof(BroadcastChannelReader<TData, TResponse>) );


    public override bool CanCount => !this._isDisposed ? this._dataReader.CanCount : ThrowHelper.ThrowObjectDisposedException<bool>( nameof(BroadcastChannelReader<TData, TResponse>) );


    public override bool CanPeek => !this._isDisposed ? this._dataReader.CanPeek : ThrowHelper.ThrowObjectDisposedException<bool>( nameof(BroadcastChannelReader<TData, TResponse>) );


    /* *
     * As long as I have Dispose (){ if (_disposed) ... } it doesn't matter if a transient service is disposed by the dependent instance, because the _disposed check will only allow it to be disposed once. MAKE SURE THAT AFTER THE DISPOSAL METHODS CANT BE CALLED
     */

    /// <summary>
    /// Removes reader from BroadcastChannel
    /// </summary>
    /// <remarks>
    /// This method is only needed because if used in Dependency Injection, it might not be disposed when done using,
    /// which means the Channel would continually be written to without being read,
    /// wasting potentially significant amounts of memory.
    /// <p/>
    /// While the documentation says that a dependent/requesting type should never Dispose of an injected type
    /// that was created by the ServiceProvider (and the factory pattern can not be used with Open Generic Types),
    /// it is still ok (and really <b>MUST</b> be done) for the dependent type to Dispose this <see cref="BroadcastChannelReader{TData,TResponse}"/>,
    /// as the Disposed status is tracked and it will not be disposed of twice.
    /// </remarks>
    public void Dispose( ) {
        this.Dispose( true );
        GC.SuppressFinalize( this );
    }

    // ReSharper disable once InconsistentNaming
    /// <inheritdoc cref="Dispose"/>
    protected virtual void Dispose( bool disposing ) {
        this._logger.LogTrace( "Dispose({Disposing}) {Type}", disposing, this.ToString()
        );
        if ( this._isDisposed ) {
            return;
        }

        if ( disposing ) {
            this._removeReader( _writerHash );
        }

        this._isDisposed = true;
    }


    // NULL checking is required here, as this could be called from within the constructor before the properties are set.
    // ReSharper disable ConditionalAccessQualifierIsNonNullableAccordingToAPIContract

    public override string ToString( ) => //this.GetType().GenericTypeShortDescriptor( useShortGenericName: false )} // TODO: RESTORE
        this.GetType().Name + " [Hash: {this.GetHashCode()}] [ChannelReader: {this._dataReader?.GetHashCode()}] [ChannelWriter: {this._responseWriter?.GetHashCode()}]";
    // ReSharper restore ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
}

/// <remarks>
/// <see cref="BroadcastChannelReader{TData}"/> with a default Response type of <see cref="IBroadcastChannelResponse"/> potentially containing an <see cref="System.Exception"/>.
/// </remarks>
public class BroadcastChannelReader<TData> : BroadcastChannelReader<TData, IBroadcastChannelResponse> {
    [ EditorBrowsable( EditorBrowsableState.Never ) ]
    public BroadcastChannelReader( BroadcastChannelWriter<TData> broadcastChannelWriter ) : base( broadcastChannelWriter ) { }
}