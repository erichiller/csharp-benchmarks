using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

using BenchmarkDotNet.Attributes;

using Common;

using NodaTime.Serialization.SystemTextJson;

namespace Benchmarks.Json;

[ Config( typeof(BenchmarkConfig) ) ]
[ ReturnValueValidator(failOnError: true)]
public partial class LevelOneJsonBenchmarks {
    // ReSharper disable once UnassignedField.Global
    // ReSharper disable once MemberCanBePrivate.Global
    [ Params( 1000 ) ]
    public int Iterations = 1;


    // ReSharper disable once UnassignedField.Global
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    [ Params(
        "Single",
        "Multiple" 
    ) ]
    // public string LevelOneJsonFile = "Single";
    public string LevelOneJsonFile;


    [ Params(
        false,
        true
    ) ]
    // public bool WithSourceGenerationContext = false;
    public bool WithSourceGenerationContext;


    private readonly JsonSerializerOptions _options = new JsonSerializerOptions() {
            // NumberHandling = JsonNumberHandling.WriteAsString | JsonNumberHandling.AllowReadingFromString
            Converters = {
                new InstantUnixTimeMillisecondsConverter(), new JsonStringEnumConverter()
            },
            PropertyNamingPolicy = new LowerCaseNamingPolicy()
            // PropertyNameCaseInsensitive = true // By setting it to be case insensitive, I get around having to write a lowercase naming strategy? no! not for writes!!!! // URGENT
        }
        .ConfigureForNodaTime( NodaTime.DateTimeZoneProviders.Tzdb );

    private LevelOneSourceGenerationContext _levelOneSourceGenerationContext;

    public LevelOneJsonBenchmarks( ) {
        _levelOneSourceGenerationContext = new LevelOneSourceGenerationContext( _options );
    }


    static bool GetMoreBytesFromStream( MemoryStream stream, ref byte[] buffer, ref Utf8JsonReader reader ) {
        int bytesRead;
        // throw new Exception();
        if ( reader.BytesConsumed < buffer.Length ) {
            ReadOnlySpan<byte> leftover = buffer.AsSpan( ( int )reader.BytesConsumed );

            if ( leftover.Length == buffer.Length ) {
                Array.Resize( ref buffer, buffer.Length * 2 );
            }

            leftover.CopyTo( buffer );
            bytesRead = stream.Read( buffer.AsSpan( leftover.Length ) );
        } else {
            bytesRead = stream.Read( buffer );
        }

        reader = new Utf8JsonReader( buffer, isFinalBlock: bytesRead == 0, reader.CurrentState );
        return bytesRead != 0;
    }


    // [ Benchmark ] // this is without read-ahead type determination
    // public int SystemTextJson_JsonSerializer_Deserialize_LevelOne( ) {
    // string jsonString = System.IO.File.ReadAllText( $"./LevelOneFuturesResponse_{LevelOneJsonFile}.json" );
    //     // var results    = new DataContainer<Response>[ Iterations ];
    //     int count = 0;
    //     for ( int i = 0 ; i < Iterations ; i++ ) {
    //         foreach ( var result in (System.Text.Json.JsonSerializer.Deserialize<DataContainer<Response>>( jsonString, systemTextJsonOptions ) ?? throw new NullReferenceException()).Data ) {
    //             count++;
    //         }
    //     }
    //
    //     return count;
    // }


    [ Benchmark ]
    public int SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne( ) {
        string jsonString = System.IO.File.ReadAllText( $"./LevelOneFuturesResponse_{LevelOneJsonFile}.json" );

        int count = 0;
        for ( int i = 0 ; i < Iterations ; i++ ) {
            byte[]    bytes  = Encoding.UTF8.GetBytes( jsonString );
            using var stream = new MemoryStream( bytes );

            TestRootContainer rootContainer;
            if ( WithSourceGenerationContext ) {
                rootContainer = JsonSerializer.Deserialize<TestRootContainer>( stream, _levelOneSourceGenerationContext.TestRootContainer )
                                    ?? throw new JsonException();
            } else {
                rootContainer = JsonSerializer.Deserialize<TestRootContainer>( stream, _options )
                                    ?? throw new JsonException();
            }

            if ( rootContainer.Data is not null && rootContainer.Data.Count > 0 && rootContainer.Data.First().Service == Service.LEVELONE_FUTURES ) {
                stream.Seek( 0, SeekOrigin.Begin );
                foreach ( var result in ( ( WithSourceGenerationContext
                                              ? System.Text.Json.JsonSerializer.Deserialize<DataContainer<Response>>( stream, _levelOneSourceGenerationContext.DataContainerResponse )
                                              : System.Text.Json.JsonSerializer.Deserialize<DataContainer<Response>>( stream, _options )
                                          )
                                          ?? throw new NullReferenceException() ).Data ) {
                    // foreach ( var result in ( System.Text.Json.JsonSerializer.Deserialize<DataContainer<Response>>( stream, _options ) ?? throw new NullReferenceException() ).Data ) {
                    count++;
                }
            } else {
                throw new Exception( "Data was not detected" );
            }
        }

        return count;
    }

    [ Benchmark ]
    public int SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne( ) {
        int    count      = 0;
        string jsonString = System.IO.File.ReadAllText( $"./LevelOneFuturesResponse_{LevelOneJsonFile}.json" );

        for ( int i = 0 ; i < Iterations ; i++ ) {
            byte[]    bytes  = Encoding.UTF8.GetBytes( jsonString );
            using var stream = new MemoryStream( bytes );

            using JsonDocument jDoc = JsonDocument.Parse( stream ); // ParseAsync(  );
            JsonElement        root = jDoc.RootElement;
            if ( root.TryGetProperty( "data", out JsonElement dataElement ) && dataElement.ValueKind == JsonValueKind.Array ) {
                /* NOTE: removed for now as nothing else runs this code.
                 * It is only necessary if multiple kinds of Responses are within DataContainer. Are they ??
                 * For now, move forward assuming that all Responses within DataContainer are the same Type and watch/ see if I get any exceptions.
                 */
                // foreach ( JsonElement arrayElement in dataElement.EnumerateArray() ) {
                var arrayElement = dataElement.EnumerateArray().First();
                if ( arrayElement.ValueKind == JsonValueKind.Object ) {
                    if ( arrayElement.TryGetProperty( "service", out JsonElement service ) && service.GetString() == nameof(Service.LEVELONE_FUTURES) ) {
                        stream.Seek( 0, SeekOrigin.Begin );
                        // foreach ( var result in ( System.Text.Json.JsonSerializer.Deserialize<DataContainer<Response>>( stream, _options ) ?? throw new NullReferenceException() ).Data ) {
                        // foreach ( var result in ( dataElement.Deserialize<List<Response>>( _options ) ?? throw new NullReferenceException() ) ) {
                        foreach ( var result in ( ( WithSourceGenerationContext
                                         ? dataElement.Deserialize( _levelOneSourceGenerationContext.ListResponse )
                                         : dataElement.Deserialize<List<Response>>( _options )
                                     ) ?? throw new NullReferenceException() ) ) {
                            count++;
                        }
                    } else {
                        throw new JsonException( "could not find 'service'" );
                    }
                    // } else {
                    //     throw new JsonException( $"not an object, found {arrayElement.ValueKind}" );
                    // }
                }
            } else {
                throw new JsonException( $"Not an array, Unable to find 'data' or is of an invalid kind. Found {dataElement.GetRawText()} with a ValueKind of {dataElement.ValueKind}" );
            }
        }

        return count;
    }
    

    private static bool checkIfDataServiceLevelOne( MemoryStream stream ) {
        var buffer = new byte[ 4096 ];

        // Fill the buffer.
        // For this snippet, we're assuming the stream is open and has data.
        /* TODO: If it might be closed or empty, check if the return value is 0 */
        stream.Read( buffer );

        // We set isFinalBlock to false since we expect more data in a subsequent read from the stream.
        var reader = new Utf8JsonReader( buffer, isFinalBlock: false, state: default );


        // Search for "data" property name
        while ( reader.TokenType != JsonTokenType.PropertyName || !reader.ValueTextEquals( "data" ) ) {
            if ( !reader.Read() ) {
                GetMoreBytesFromStream( stream, ref buffer, ref reader );
            }
        }

        // Search for "service" property name
        while ( reader.TokenType != JsonTokenType.PropertyName || !reader.ValueTextEquals( "service" ) ) {
            if ( !reader.Read() ) {
                GetMoreBytesFromStream( stream, ref buffer, ref reader );
            }
        }

        while ( !reader.Read() ) {
            GetMoreBytesFromStream( stream, ref buffer, ref reader );
        }

        return reader.GetString() == nameof(Service.LEVELONE_FUTURES);
    }

    [ Benchmark ]
    public int SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne( ) {
        int    count      = 0;
        string jsonString = System.IO.File.ReadAllText( $"./LevelOneFuturesResponse_{LevelOneJsonFile}.json" );

        for ( int i = 0 ; i < Iterations ; i++ ) {
            byte[]    bytes  = Encoding.UTF8.GetBytes( jsonString );
            using var stream = new MemoryStream( bytes );

            if ( checkIfDataServiceLevelOne( stream ) ) {
                stream.Seek( 0, SeekOrigin.Begin );
                foreach ( var result in ( ( WithSourceGenerationContext
                                              ? System.Text.Json.JsonSerializer.Deserialize<DataContainer<Response>>( stream, _levelOneSourceGenerationContext.DataContainerResponse )
                                              : System.Text.Json.JsonSerializer.Deserialize<DataContainer<Response>>( stream, _options )
                                          )
                                          ?? throw new NullReferenceException() ).Data ) {
                    count++;
                }
            } else {
                throw new Exception();
            }
        }


        return count;
    }


    [ Benchmark ]
    public int SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne( ) {
        JsonSerializerOptions options = _options;

        int count = 0;

        string jsonString = System.IO.File.ReadAllText( $"./LevelOneFuturesResponse_{LevelOneJsonFile}.json" );

        for ( int i = 0 ; i < Iterations ; i++ ) {
            byte[]    bytes  = Encoding.UTF8.GetBytes( jsonString );
            using var stream = new MemoryStream( bytes );

            var buffer = new byte[ 4096 ];

            // Fill the buffer.
            // For this snippet, we're assuming the stream is open and has data.
            /* TODO: If it might be closed or empty, check if the return value is 0 */
            stream.Read( buffer );


            // We set isFinalBlock to false since we expect more data in a subsequent read from the stream.
            var reader = new Utf8JsonReader( buffer, isFinalBlock: false, state: default );


            // Search for "data" property name
            while ( reader.TokenType != JsonTokenType.PropertyName || !reader.ValueTextEquals( "data" ) ) {
                if ( !reader.Read() ) {
                    GetMoreBytesFromStream( stream, ref buffer, ref reader );
                }
            }


            while ( !reader.Read() ) {
                GetMoreBytesFromStream( stream, ref buffer, ref reader );
            }

            // List<Response> results = new ();


            const int startObjectDepth = 2;

            // Utf8JsonReader? lastObjectToken
            // JsonReaderState? lastObjectReaderState = null;
            int? lastObjectPosition = null;
            while ( !( reader.TokenType is JsonTokenType.EndObject or JsonTokenType.EndArray && reader.CurrentDepth == 0 )
                    && ( reader.Read() || ( GetMoreBytesFromStream( stream, ref buffer, ref reader ) && reader.Read() ) )
                  ) {
                if ( reader.TokenType == JsonTokenType.StartObject && reader.CurrentDepth == startObjectDepth ) {
                    lastObjectPosition = ( int )reader.TokenStartIndex;
                }

                if ( lastObjectPosition is { } objectStartPosition && reader.CurrentDepth == ( startObjectDepth + 1 ) && reader.TokenType == JsonTokenType.PropertyName && reader.ValueTextEquals( "service" ) ) {
                    while ( !reader.Read() ) {
                        GetMoreBytesFromStream( stream, ref buffer, ref reader );
                        reader.Read();
                    }


                    reader.Read();

                    int                currentBytesConsumed = ( int )reader.BytesConsumed;
                    byte[]             objectReaderBuffer   = new byte[ buffer.Length ];
                    ReadOnlySpan<byte> leftover             = buffer.AsSpan( objectStartPosition );

                    if ( leftover.Length == buffer.Length ) {
                        Array.Resize( ref buffer, buffer.Length * 2 );
                    }

                    leftover.CopyTo( objectReaderBuffer );
                    int bytesRead = stream.Read( objectReaderBuffer.AsSpan( leftover.Length ) );
                    stream.Seek( -bytesRead, SeekOrigin.Current );
                    Utf8JsonReader objectReader = new Utf8JsonReader( objectReaderBuffer, isFinalBlock: reader.IsFinalBlock, state: default );
                    
                    if ( ( WithSourceGenerationContext
                            ? System.Text.Json.JsonSerializer.Deserialize<Response>( ref objectReader, _levelOneSourceGenerationContext.Response )
                            : System.Text.Json.JsonSerializer.Deserialize<Response>( ref objectReader, _options )
                        ) is not null ) {
                        count++;
                    } else {
                        throw new Exception();
                    }

                    lastObjectPosition = null;
                }
            }
        }

        return count;
    }
}