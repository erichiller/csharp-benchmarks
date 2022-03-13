using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.Json;

[ Config( typeof(BenchmarkConfig) ) ]
public partial class SystemTextJsonDeserializationBasic {
    
    // ReSharper disable once UnassignedField.Global
    // ReSharper disable once MemberCanBePrivate.Global
    [ Params( 1000 ) ]
    public int Iterations;

    private static readonly JsonSerializerOptions _systemTextJsonOptions = SystemTextJsonCommon.SystemTextJsonOptions;
    
    
    private readonly SimpleSourceGenerationContext _sourceGenerationContextWithFieldOptions = new SimpleSourceGenerationContext(
        new JsonSerializerOptions() {
            IncludeFields = true
        } );
    private readonly SimpleSourceGenerationContext _sourceGenerationContextWithOptions    = new SimpleSourceGenerationContext( _systemTextJsonOptions );
    private readonly SimpleSourceGenerationContext _sourceGenerationContextWithoutOptions = new SimpleSourceGenerationContext();

    #region Deserialize Records

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Record", "Init", "Constructor" ) ]
    public ScalarsFloatRecord[] Scalars_Float_Record_Init_Constructor( ) {
        ScalarsFloatRecord[] results = new ScalarsFloatRecord[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatRecord>( ScalarsFloatRecord.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Record", "Init", "NoConstructor" ) ]
    public ScalarsFloatRecordInitNoConstructor[] Scalars_Float_Record_Init_NoConstructor( ) {
        ScalarsFloatRecordInitNoConstructor[] results = new ScalarsFloatRecordInitNoConstructor[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatRecordInitNoConstructor>( ScalarsFloatRecordInitNoConstructor.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Record", "Init", "Constructor" ) ]
    public ScalarsDecimal[] Scalars_Decimal_Record( ) {
        ScalarsDecimal[] results = new ScalarsDecimal[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsDecimal>( ScalarsDecimal.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    
    /* ScalarsNodaTime */
    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Record", "Init", "Constructor" ) ]
    public ScalarsNodaTime[] Scalars_NodaTime_Record( ) {
        ScalarsNodaTime[] results = new ScalarsNodaTime[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsNodaTime>( ScalarsNodaTime.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Record", "Init", "Constructor" ) ]
    public NestedObjectNodaTime[] NestedObjects_NodaTime_Record( ) {
        NestedObjectNodaTime[] results = new NestedObjectNodaTime[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectNodaTime>( NestedObjectNodaTime.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Record" , "Init", "Constructor" ) ]
    public NestedObjectArrayNodaTime[] NestedObjects_Arrays_NodaTime_Record( ) {
        NestedObjectArrayNodaTime[] results = new NestedObjectArrayNodaTime[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectArrayNodaTime>( NestedObjectArrayNodaTime.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    #endregion
    
    
    
    #region Deserialize Classes

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Set", "NoConstructor" ) ]
    public ScalarsFloatClass[] Scalars_Float_Class_Set_NoConstructor( ) {
        ScalarsFloatClass[] results = new ScalarsFloatClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatClass>( ScalarsFloatClass.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class" , "Set", "NoConstructor" ) ]
    public int Scalars_Float_Class_Set_NoConstructor_ReturningCount( ) { // TODO: this is just testing the test methodology, should it just count or should it return objects?
        int count = 0;
        
        for ( int i = 0 ; i < Iterations ; i++ ) {
            _ = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatClass>( ScalarsFloatClass.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
            count++;
        }

        return count;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Init" , "NoConstructor" ) ]
    public ScalarsFloatClassWithInitProperties[] Scalars_Float_Class_Init_NoConstructor( ) {
        ScalarsFloatClassWithInitProperties[] results = new ScalarsFloatClassWithInitProperties[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatClassWithInitProperties>( ScalarsFloatClassWithInitProperties.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Init" , "NoConstructor" ) ]
    public ScalarsFloatClassInitPartialConstructor[] Scalars_Float_Class_Init_PartialConstructor( ) {
        ScalarsFloatClassInitPartialConstructor[] results = new ScalarsFloatClassInitPartialConstructor[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatClassInitPartialConstructor>( ScalarsFloatClassInitPartialConstructor.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class" ) ]
    public ScalarsDecimalClass[] Scalars_Decimal_Class( ) {
        ScalarsDecimalClass[] results = new ScalarsDecimalClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsDecimalClass>( ScalarsDecimalClass.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    
    /* ScalarsNodaTime */
    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class" ) ]
    public ScalarsNodaTimeClass[] Scalars_NodaTime_Class( ) {
        ScalarsNodaTimeClass[] results = new ScalarsNodaTimeClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsNodaTimeClass>( ScalarsNodaTimeClass.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class" ) ]
    public NestedObjectNodaTimeClass[] NestedObjects_NodaTime_Class( ) {
        NestedObjectNodaTimeClass[] results = new NestedObjectNodaTimeClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectNodaTimeClass>( NestedObjectNodaTimeClass.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class" ) ]
    public NestedObjectArrayNodaTimeClass[] NestedObjects_Arrays_NodaTime_Class( ) {
        NestedObjectArrayNodaTimeClass[] results = new NestedObjectArrayNodaTimeClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectArrayNodaTimeClass>( NestedObjectArrayNodaTimeClass.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }
    
    #endregion

    
    
    #region Deserialize Classes with SourceGeneratorContext

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Set", "NoConstructor", "SourceGen" ) ]
    public ScalarsFloatClass[] Scalars_Float_Class_Set_NoConstructor_SourceGen( ) {
        ScalarsFloatClass[] results = new ScalarsFloatClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatClass>( ScalarsFloatClass.JSON, _sourceGenerationContextWithoutOptions.ScalarsFloatClass ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Fields", "NoConstructor", "SourceGen" ) ]
    public ScalarsFloatClassFields[] Scalars_Float_Class_Fields_NoConstructor_SourceGen( ) {
        ScalarsFloatClassFields[] results = new ScalarsFloatClassFields[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatClassFields>( ScalarsFloatClassFields.JSON, _sourceGenerationContextWithFieldOptions.ScalarsFloatClassFields ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Set", "NoConstructor", "SourceGen" ) ]
    public ScalarsDecimalClass[] Scalars_Decimal_Class_SourceGen( ) {
        ScalarsDecimalClass[] results = new ScalarsDecimalClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsDecimalClass>( ScalarsDecimalClass.JSON, _sourceGenerationContextWithoutOptions.ScalarsDecimalClass ) ?? throw new NullReferenceException();
        }

        return results;
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Set", "NoConstructor", "SourceGen" ) ]
    public ScalarsNodaTimeClass[] Scalars_NodaTime_Class_SourceGen( ) {
        ScalarsNodaTimeClass[] results = new ScalarsNodaTimeClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsNodaTimeClass>( ScalarsNodaTimeClass.JSON, _sourceGenerationContextWithOptions.ScalarsNodaTimeClass ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Set", "NoConstructor", "SourceGen", "ConverterAttribute" ) ]
    public ScalarsNodaTimeClassWithAttribute[] Scalars_NodaTime_ConverterAttribute_Class_SourceGen( ) {
        ScalarsNodaTimeClassWithAttribute[] results = new ScalarsNodaTimeClassWithAttribute[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsNodaTimeClassWithAttribute>( ScalarsNodaTimeClassWithAttribute.JSON, _sourceGenerationContextWithoutOptions.ScalarsNodaTimeClassWithAttribute ) ?? throw new NullReferenceException(); // URGENT: why doesn't the attribute work?
        }

        return results;
    }

    /// <summary>
    /// Using a class, but without SourceGenerator // JsonSerializerContext
    /// Uses a <see cref="JsonConverterAttribute"/> on <see cref="ScalarsNodaTimeClassWithAttribute.Value"/> rather than <see cref="JsonSerializerOptions"/>
    /// </summary>
    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Set", "NoConstructor", "SourceGen", "ConverterAttribute" ) ]
    public ScalarsNodaTimeClassWithAttribute[] Scalars_NodaTime_Class_ConverterAttribute( ) {
        ScalarsNodaTimeClassWithAttribute[] results = new ScalarsNodaTimeClassWithAttribute[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsNodaTimeClassWithAttribute>( ScalarsNodaTimeClassWithAttribute.JSON ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Set", "NoConstructor", "SourceGen" ) ]
    public NestedObjectNodaTimeClass[] NestedObjects_NodaTime_Class_SourceGen( ) {
        NestedObjectNodaTimeClass[] results = new NestedObjectNodaTimeClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectNodaTimeClass>( NestedObjectNodaTimeClass.JSON, _sourceGenerationContextWithOptions.NestedObjectNodaTimeClass ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "System.Text.Json", "Deserialize", "Class", "Set", "NoConstructor", "SourceGen" ) ]
    public NestedObjectArrayNodaTimeClass[] NestedObjects_Arrays_NodaTime_Class_SourceGen( ) {
        NestedObjectArrayNodaTimeClass[] results = new NestedObjectArrayNodaTimeClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectArrayNodaTimeClass>( NestedObjectArrayNodaTimeClass.JSON, _sourceGenerationContextWithOptions.NestedObjectArrayNodaTimeClass ) ?? throw new NullReferenceException();
        }

        return results;
    }

    #endregion
}