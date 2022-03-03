using System;
using System.Collections.Generic;
using System.Text.Json;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using NodaTime;

namespace Benchmarks.Json;

/// <summary>
/// Convert <see cref="NodaTime.Instant"/> to and from <i>UnixTimeSeconds</i> 
/// </summary>
public class InstantUnixTimeSecondsConverter : System.Text.Json.Serialization.JsonConverter<NodaTime.Instant> {
    private readonly ILogger _log;

    /// <inheritdoc cref="InstantUnixTimeSecondsConverter" />
    public InstantUnixTimeSecondsConverter( ILogger? log = null )
        // => ( this._log ) = ( log ?? NullLogger.Instance );
        => ( this._log ) = ( NullLogger.Instance ); // re-enable above for logging
        // => ( this._log ) = ( Program.GetLogger<InstantUnixTimeMillisecondsConverter>() ); // re-enable above for logging
    // {( this._log ) = ( Program.GetLogger<InstantUnixTimeMillisecondsConverter>() ); // re-enable above for logging
    // this._log.LogError( "AAAAAA" );
    // }

    /// <inheritdoc cref="InstantUnixTimeSecondsConverter" />
    public InstantUnixTimeSecondsConverter( )
        => ( this._log ) = ( NullLogger.Instance );

    /// <inheritdoc />
    public override NodaTime.Instant Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options ) {
        _log.LogDebug( "Reading NodaTime.Instant as UnixTimeSeconds from {value}", reader.GetInt32() );
        return Instant.FromUnixTimeSeconds( reader.GetInt32() );
    }

    /// <inheritdoc />
    public override void Write( Utf8JsonWriter writer, NodaTime.Instant value, JsonSerializerOptions options ) {
        _log.LogDebug( "Writing NodaTime.Instant {Instant} as UnixTimeSeconds {Seconds}", value, value.ToUnixTimeSeconds() );
        ( writer ?? throw new ArgumentNullException( nameof(writer) ) ).WriteNumberValue( value.ToUnixTimeSeconds() );
    }
}

/// <summary>
/// Convert <see cref="NodaTime.Instant"/> to and from <i>UnixTime in Milliseconds</i> 
/// </summary>
public class InstantUnixTimeMillisecondsConverter : System.Text.Json.Serialization.JsonConverter<NodaTime.Instant> {
    private readonly ILogger _log;

    /// <inheritdoc cref="InstantUnixTimeMillisecondsConverter" />
    public InstantUnixTimeMillisecondsConverter( ILogger log )
        // => ( this._log ) = ( log ?? NullLogger.Instance );
        => ( this._log ) = ( NullLogger.Instance ); // re-enable above for logging
    // => ( this._log ) = ( Program.GetLogger<InstantUnixTimeMillisecondsConverter>() ); // re-enable above for logging
    
    /// <inheritdoc cref="InstantUnixTimeMillisecondsConverter" />
    public InstantUnixTimeMillisecondsConverter( )
        => ( this._log ) = ( NullLogger.Instance );


    /// <inheritdoc />
    public override NodaTime.Instant Read( ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options ) {
        _log.LogDebug( "Reading NodaTime.Instant as UnixTimeSeconds from {value}, returning {ReturnValue}", reader.GetInt64(), Instant.FromUnixTimeMilliseconds( reader.GetInt64() ) );
        return Instant.FromUnixTimeMilliseconds( reader.GetInt64() );
    }

    /// <inheritdoc />
    public override void Write( Utf8JsonWriter writer, NodaTime.Instant value, JsonSerializerOptions options ) {
        _log.LogDebug( "Writing NodaTime.Instant {Instant} as UnixTimeSeconds {Seconds}", value, value.ToUnixTimeMilliseconds() );
        ( writer ?? throw new ArgumentNullException( nameof(writer) ) ).WriteNumberValue( value.ToUnixTimeMilliseconds() );
    }
}