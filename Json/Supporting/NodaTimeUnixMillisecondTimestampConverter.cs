#nullable enable
using System;

using Newtonsoft.Json;

using NodaTime;
using NodaTime.Text;

// using Serilog;

namespace Benchmarks {

    /// <summary>
    /// Converts into <see cref="NodaTime.Instant"/> to/from UnixTimeStamp in Milliseconds.
    /// </summary>
    public class NodaTimeUnixMillisecondTimestampConverter : Newtonsoft.Json.Converters.DateTimeConverterBase {
        /// <inheritdoc />
        public override void WriteJson( JsonWriter writer, object? value, JsonSerializer serializer ) {
            _ = writer ?? throw new ArgumentNullException( nameof(writer) );
            if (value == null) {
                writer.WriteRawValue( Instant.FromUnixTimeSeconds( 0 ) + "" );
            } else {
                writer.WriteRawValue( ((Instant)value).ToUnixTimeMilliseconds() + "" );
            }
        }

        /// <inheritdoc />
        public override object? ReadJson( JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer ) {
            _ = reader ?? throw new ArgumentNullException( nameof(reader) );
            // Log.Verbose("NodaTimeUnixMillisecondTimestampConverter.ReadJson\n" + 
            //             "\tReceived Type    : {objectType}\n" +
            //             "\tReader Value     : {readerValue}\n" +
            //             "\tReader ValueType : {readerValueType}",
            //             objectType , reader.Value, reader.ValueType);
            if (reader.Value == null) { 
                // Log.Warning("NodaTimeUnixMillisecondTimestampConverter.ReadJson - reader.Value is NULL");
                return null; 
            }
            
            if (reader.Value is long value ) {
                // Log.Verbose("NodaTimeUnixMillisecondTimestampConverter.ReadJson - returning Instant from long");
                return Instant.FromUnixTimeMilliseconds( value );
            }
            if (reader.Value is string readerValue ) {
                try {
                    // Log.Verbose( "NodaTimeUnixMillisecondTimestampConverter.ReadJson - returning Instant from string" );
                    // return InstantPattern.General.Parse((string)reader.Value).Value;
                    return InstantPattern.CreateWithInvariantCulture( @"uuuu'-'MM'-'dd'T'HH':'mm':'ss'+0000'" ).Parse( readerValue ).Value ;
                } catch (InvalidPatternException ex) {
                    ex.Data.Add( "ReaderValue", readerValue );
                    ex.Data.Add( "Type", objectType?.Name );
                    // Log.Error( ex, "An error occurred parsing Instant of type String = {value}", reader.Value );
                    throw;
                }
            }
            return reader.Value;
        }
    }
}