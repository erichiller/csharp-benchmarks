using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.Json;

public partial class SystemTextJsonDeserializationBasic {

    #region Deserialize Classes using Constructors

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Init", "Constructor" ) ]
    public ScalarsFloatClassInitConstructor[] Scalars_Float_Class_Init_Constructor( ) {
        ScalarsFloatClassInitConstructor[] results = new ScalarsFloatClassInitConstructor[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatClassInitConstructor>( ScalarsFloatClassInitConstructor.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Init", "Constructor" ) ]
    public ScalarsDecimalClassInitConstructor[] Scalars_Decimal_Class_Init_Constructor( ) {
        ScalarsDecimalClassInitConstructor[] results = new ScalarsDecimalClassInitConstructor[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsDecimalClassInitConstructor>( ScalarsDecimalClassInitConstructor.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Init", "Constructor" ) ]
    public ScalarsNodaTimeClassInitConstructor[] Scalars_NodaTime_Class_Init_Constructor( ) {
        ScalarsNodaTimeClassInitConstructor[] results = new ScalarsNodaTimeClassInitConstructor[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsNodaTimeClassInitConstructor>( ScalarsNodaTimeClassInitConstructor.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Init", "Constructor" ) ]
    public NestedObjectNodaTimeClassInitConstructor[] NestedObjects_NodaTime_Class_Init_Constructor( ) {
        NestedObjectNodaTimeClassInitConstructor[] results = new NestedObjectNodaTimeClassInitConstructor[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectNodaTimeClassInitConstructor>( NestedObjectNodaTimeClassInitConstructor.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Init", "Constructor" ) ]
    public NestedObjectArrayNodaTimeClassInitConstructor[] NestedObjects_Arrays_NodaTime_Class_Init_Constructor( ) {
        NestedObjectArrayNodaTimeClassInitConstructor[] results = new NestedObjectArrayNodaTimeClassInitConstructor[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectArrayNodaTimeClassInitConstructor>( NestedObjectArrayNodaTimeClassInitConstructor.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    #endregion
}