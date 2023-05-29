using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BroadcastChannel;

/// <summary>
/// Utility class for throwing exceptions in an efficient way.
/// <para />
/// While there are <a href="https://stackoverflow.com/questions/1980044/when-should-i-use-a-throwhelper-method-instead-of-throwing-directly">arguments against this</a>
/// the consensus amongst .NET runtime developers is that it is the correct way to go.
/// The runtime docs say <a href="https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/ThrowHelper.cs#L33">
/// <i>It is very important we do this for generic classes because we can easily generate the same code multiple times for different instantiation.</i></a>
/// <br/>
/// To facilitate use in methods or locations that must return a value, use one of the Generic helper methods, eg <c>public static T ThrowException{T}</c>,
/// See <a href="https://learn.microsoft.com/en-us/dotnet/communitytoolkit/diagnostics/throwhelper#:~:text=within%20expressions%20that%20require%20a%20return%20type%20of%20a%20specific%20type">.NET Community Toolkit - ThrowHelper</a>
///
/// <para/>There is one large downside, <b>the stack trace will start at the ThrowHelper method, not the method that called it</b>.
/// <b><ul>URGENT: is this still true? see the <see cref="StackTraceHiddenAttribute"/> attribute. If that attribute fixes this issue, be sure to apply it to <b>ALL</b> other ThrowHelper methods (eg. those on customer user-defined exceptions )!!</ul></b>
/// <para />
/// For background see:
///         <list type="bullet">
///     <item>
///         <description>https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/ThrowHelper.cs</description>
///     </item>
///     <item>
///         <description>https://learn.microsoft.com/en-us/dotnet/communitytoolkit/diagnostics/throwhelper</description>
///     </item>
///     <item>
///         <description>https://dunnhq.com/posts/2022/throw-helper/</description>
///     </item>
/// </list>
/// </summary>
[ StackTraceHidden ]
public static class ThrowHelper {
    /// <summary>
    /// Throw <see cref="NotImplementedException"/>
    /// </summary>
    [ DoesNotReturn ]
    public static void ThrowNotImplementedException( string? message, Exception? innerException = null )
        => throw new NotImplementedException( message, innerException );

    /// <summary>
    /// Throw <see cref="NotImplementedException"/>
    /// </summary>
    [ DoesNotReturn ]
    public static T ThrowNotImplementedException<T>( string? message, Exception? innerException = null )
        => throw new NotImplementedException( message, innerException );

    // /// <summary>
    // /// Throw <see cref="ArgumentException"/>
    // /// </summary>
    // [ DoesNotReturn ]
    // public static void ThrowArgumentException( string message, string parameterName, Exception? innerException = null )
    //     => throw new ArgumentException( message, parameterName, innerException );

    // /// <summary>
    // /// Throw <see cref="ArgumentNullException"/>
    // /// </summary>
    // // [ return: NotNullIfNotNull( nameof(paramValue) ) ]
    // public static T ThrowArgumentNullIfNullElseReturn<T>( T? paramValue, [ CallerArgumentExpression( nameof(paramValue) ) ] string parameterName = "<<ERROR>>" ) =>
    //     paramValue ?? ThrowArgumentNullException<T>( parameterName );

    // /// <summary>
    // /// Throw <see cref="ArgumentNullException"/>
    // /// </summary>
    // [ DoesNotReturn ]
    // [ SuppressMessage( "ReSharper", "EntityNameCapturedOnly.Global" ) ]
    // public static T ThrowArgumentNullException<T>( T? paramValue, [ CallerArgumentExpression( nameof(paramValue) ) ] string parameterName = "<<ERROR>>" ) =>
    //     throw new ArgumentNullException( parameterName );
    /// <summary>
    /// Throw <see cref="ArgumentNullException"/>
    /// </summary>
    // [ DoesNotReturn ]
    // public static T ThrowArgumentNullException<T>( string? parameterName, Dictionary<object, object?>? data = null ) {
    //     if ( data is null ) {
    //         throw new ArgumentNullException( parameterName );
    //     }
    //     ArgumentNullException e = new ArgumentNullException( parameterName );
    //     e.Data.Add( data );
    //     throw e;
    // }

    /// <summary>
    /// Throw <see cref="ArgumentException"/>
    /// </summary>
    [ DoesNotReturn ]
    public static void ThrowArgumentException<T>( T argument, [ CallerArgumentExpression( nameof(argument) ) ] string parameterName = "<<ERROR>>", Exception? innerException = null )
        => throw new ArgumentException( $"{parameterName} had an invalid value: '{argument}'", parameterName, innerException );
    
    /// <summary>
    /// Throw <see cref="ArgumentException"/>
    /// </summary>
    [ DoesNotReturn ]
    public static TReturn ThrowArgumentException<TArg,TReturn>( TArg argument, string? expected, [ CallerArgumentExpression( nameof(argument) ) ] string parameterName = "<<ERROR>>", Exception? innerException = null )
        => throw new ArgumentException( $"{parameterName} had an invalid value: '{argument}', Expected: {expected}", parameterName, innerException );

    /// <summary>
    /// Throw <see cref="ArgumentException"/>
    /// </summary>
    [ DoesNotReturn ]
    public static T ThrowArgumentException<T>( string message, T argument, [ CallerArgumentExpression( nameof(argument) ) ] string parameterName = "<<ERROR>>", Exception? innerException = null )
        => throw new ArgumentException( $"{parameterName} had an invalid value: '{argument}': {message}", parameterName, innerException );

    /// <summary>
    /// Throw <see cref="ArgumentException"/>, merges multiple <paramref name="parameterNames"/> into the form <c>(Param1, Param2)</c>.
    /// </summary>
    [ DoesNotReturn ]
    public static void ThrowArgumentException( string message, params string[] parameterNames )
        => throw new ArgumentException( message: message, paramName: $"({String.Join( ", ", parameterNames )})" );

    /// <summary>
    /// Throw <see cref="ArgumentException"/>, merges multiple <paramref name="parameterNamesAndValues"/> into the form <c>(Param1 = Value1, Param2 = Value2)</c>.
    /// </summary>
    [ DoesNotReturn ]
    public static void ThrowArgumentException( string message, Dictionary<string, object> parameterNamesAndValues )
        => throw new ArgumentException( message: message, paramName: $"({String.Join( ", ", parameterNamesAndValues.Select( kv => $"{kv.Key} = {kv.Value}" ) )})" );

    /// <summary>
    /// Throw <see cref="InvalidCastException"/>, supplying the destination type for the message
    /// </summary>
    [ DoesNotReturn ]
    public static TCast ThrowInvalidCastException<TInput, TCast>( TInput? variable, [ CallerArgumentExpression( "variable" ) ] string? variableName = null )
        => throw new InvalidCastException( $"Unable to cast {variableName} of type {typeof(TInput).Name} and value {variableName} to type {typeof(TCast).Name}" );

    // /// <inheritdoc cref="ObjectDisposedException" />
    // [ DoesNotReturn ]
    // public static TReturn ThrowObjectDisposedException<TReturn>( TReturn objectDisposed ) =>
    //     throw new ObjectDisposedException(typeof(TReturn).GenericTypeShortDescriptor(  ));

    /// <inheritdoc cref="ObjectDisposedException" />
    [ DoesNotReturn ]
    public static void ThrowObjectDisposedException( string typeNameDisposed ) =>
        throw new ObjectDisposedException( typeNameDisposed );

    /// <inheritdoc cref="ObjectDisposedException" />
    [ DoesNotReturn ]
    public static TReturn ThrowObjectDisposedException<TReturn>( string typeNameDisposed ) =>
        throw new ObjectDisposedException( typeNameDisposed );

    /// <inheritdoc cref="ObjectDisposedException" />
    [ DoesNotReturn ]
    public static bool ThrowObjectDisposedException<TOut>( string typeNameDisposed, out TOut data ) =>
        throw new ObjectDisposedException( typeNameDisposed );

    // /// <inheritdoc cref="InvalidTimeZoneException" />
    // [ DoesNotReturn ]
    // public static NodaTime.DateTimeZone ThrowInvalidTimeZoneException( string message ) =>
    //     throw new InvalidTimeZoneException( message );

    /// <inheritdoc cref="InvalidDataException" />
    [ DoesNotReturn ]
    public static void ThrowInvalidDataException( string message ) =>
        throw new InvalidDataException( message );
}