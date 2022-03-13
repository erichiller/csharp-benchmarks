using System;
using System.Text.Json;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace Benchmarks.Json;

[ Config( typeof(BenchmarkConfig) ) ]
public partial class SystemTextJsonSerializationBasic {
    // ReSharper disable once UnassignedField.Global
    // ReSharper disable once MemberCanBePrivate.Global
    [ Params( 1000 ) ]
    public int Iterations;

    private static readonly JsonSerializerOptions _systemTextJsonOptions = SystemTextJsonCommon.SystemTextJsonOptions;


    #region Serialize

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Init", "Constructor" ) ]
    public string[] SystemTextJson_Serialize_Scalars_Float( ) {
        string[] results = new string[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Serialize<ScalarsFloatRecord>( new ScalarsFloatRecord( 1, "Hello World", 9.127f ), _systemTextJsonOptions );
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Init", "Constructor" ) ]
    public string[] SystemTextJson_Serialize_Scalars_Decimal( ) {
        string[] results = new string[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Serialize<ScalarsDecimal>( new ScalarsDecimal( 1, "Hello World", 9.127m ), _systemTextJsonOptions );
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Init", "Constructor" ) ]
    public string[] SystemTextJson_Serialize_Scalars_NodaTime( ) {
        string[] results = new string[ Iterations ];
        Instant  instant = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Serialize<ScalarsNodaTime>( new ScalarsNodaTime( 1, "Hello World", instant ), _systemTextJsonOptions );
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Init", "Constructor" ) ]
    public string[] SystemTextJson_Serialize_NestedObjects_NodaTime( ) {
        string[] results = new string[ Iterations ];
        Instant  instant = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Serialize<NestedObjectNodaTime>( new NestedObjectNodaTime( 1, "Parent Object", new ScalarsNodaTime( 1, "Hello World", instant ) ), _systemTextJsonOptions );
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Init", "Constructor" ) ]
    public string[] SystemTextJson_Serialize_NestedObjects_Arrays_NodaTime( ) {
        string[] results = new string[ Iterations ];
        Instant  instant = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Serialize<NestedObjectArrayNodaTime>( new NestedObjectArrayNodaTime( 1, "Parent Object", new[] {
                                                                                                         new ScalarsNodaTime( 1, "Hello World", instant ), new ScalarsNodaTime( 1, "Hello World", instant ),
                                                                                                     }
                                                                                                 ), _systemTextJsonOptions );
        }

        return results;
    }

    #endregion
}