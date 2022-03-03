using System;
using System.IO;
using System.Text;

using BenchmarkDotNet.Attributes;

using Common;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using NodaTime;
using NodaTime.Serialization.JsonNet;

namespace Benchmarks.Json;

[ Config( typeof(BenchmarkConfig) ) ]
public class NewtonsoftJsonBenchmarks {
    public static readonly JsonSerializerSettings JsonNetSettings = new JsonSerializerSettings().ConfigureForNodaTime( DateTimeZoneProviders.Tzdb );

    // ReSharper disable once UnassignedField.Global
    // ReSharper disable once MemberCanBePrivate.Global
    [ Params( 1000 ) ]
    public int Iterations;
    
    [ Benchmark ]
    public string[] NewtonsoftJson_Serialize_Scalars_Float( ) {
        string[] results = new string[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = JsonConvert.SerializeObject( new ScalarsFloat( 1, "Hello World", 9.127f ), JsonNetSettings );
        }

        return results;
    }

    [ Benchmark ]
    public string[] NewtonsoftJson_Serialize_Scalars_Decimal( ) {
        string[] results = new string[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = JsonConvert.SerializeObject( new ScalarsDecimal( 1, "Hello World", 9.127m ), JsonNetSettings );
        }

        return results;
    }

    [ Benchmark ]
    public string[] NewtonsoftJson_Serialize_Scalars_NodaTime( ) {
        string[] results = new string[ Iterations ];
        Instant  instant = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = JsonConvert.SerializeObject( new ScalarsNodaTime( 1, "Hello World", instant ), JsonNetSettings );
        }

        return results;
    }

    [ Benchmark ]
    public string[] NewtonsoftJson_Serialize_NestedObjects_NodaTime( ) {
        string[] results = new string[ Iterations ];
        Instant  instant = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = JsonConvert.SerializeObject( new NestedObjectNodaTime( 1, "Parent Object", new ScalarsNodaTime( 1, "Hello World", instant ) ), JsonNetSettings );
        }

        return results;
    }

    [ Benchmark ]
    public string[] NewtonsoftJson_Serialize_NestedObjects_Arrays_NodaTime( ) {
        string[] results = new string[ Iterations ];
        Instant  instant = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = JsonConvert.SerializeObject( new NestedObjectArrayNodaTime( 1, "Parent Object", new[] {
                                                                new ScalarsNodaTime( 1, "Hello World", instant ), new ScalarsNodaTime( 1, "Hello World", instant ),
                                                            }
                                                        ), JsonNetSettings );
        }

        return results;
    }


    [ Benchmark ]
    public ScalarsFloat[] NewtonsoftJson_Deserialize_Scalars_Float( ) {
        ScalarsFloat[] results = new ScalarsFloat[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = JsonConvert.DeserializeObject<ScalarsFloat>( ScalarsFloat.JSON, JsonNetSettings ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public ScalarsDecimal[] NewtonsoftJson_Deserialize_Scalars_Decimal( ) {
        ScalarsDecimal[] results = new ScalarsDecimal[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = JsonConvert.DeserializeObject<ScalarsDecimal>( ScalarsDecimal.JSON, JsonNetSettings ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public ScalarsNodaTime[] NewtonsoftJson_Deserialize_Scalars_NodaTime( ) {
        ScalarsNodaTime[] results = new ScalarsNodaTime[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = JsonConvert.DeserializeObject<ScalarsNodaTime>( ScalarsNodaTime.JSON, JsonNetSettings ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public NestedObjectNodaTime[] NewtonsoftJson_Deserialize_NestedObjects_NodaTime( ) {
        NestedObjectNodaTime[] results = new NestedObjectNodaTime[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = JsonConvert.DeserializeObject<NestedObjectNodaTime>( NestedObjectNodaTime.JSON, JsonNetSettings ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public NestedObjectArrayNodaTime[] NewtonsoftJson_Deserialize_NestedObjects_Arrays_NodaTime( ) {
        NestedObjectArrayNodaTime[] results = new NestedObjectArrayNodaTime[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = JsonConvert.DeserializeObject<NestedObjectArrayNodaTime>( NestedObjectArrayNodaTime.JSON, JsonNetSettings ) ?? throw new NullReferenceException();
        }

        return results;
    }

}