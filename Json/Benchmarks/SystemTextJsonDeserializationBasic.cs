using System;
using System.Text.Json;
using System.Text.Json.Serialization;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.Json;

[ Config( typeof(BenchmarkConfig) ) ]
public class SystemTextJsonDeserializationBasic {
    
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
    public ScalarsFloat[] Scalars_Float_Record( ) {
        ScalarsFloat[] results = new ScalarsFloat[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloat>( ScalarsFloat.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public ScalarsDecimal[] Scalars_Decimal_Record( ) {
        ScalarsDecimal[] results = new ScalarsDecimal[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsDecimal>( ScalarsDecimal.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    
    /* ScalarsNodaTime */
    [ Benchmark ]
    public ScalarsNodaTime[] Scalars_NodaTime_Record( ) {
        ScalarsNodaTime[] results = new ScalarsNodaTime[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsNodaTime>( ScalarsNodaTime.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }
    
    [ Benchmark ]
    public NestedObjectNodaTime[] NestedObjects_NodaTime_Record( ) {
        NestedObjectNodaTime[] results = new NestedObjectNodaTime[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectNodaTime>( NestedObjectNodaTime.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
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
    public ScalarsFloatClass[] Scalars_Float_Class( ) {
        ScalarsFloatClass[] results = new ScalarsFloatClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatClass>( ScalarsFloatClass.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public int Scalars_Float_Class_ReturningCount( ) {
        int count = 0;
        for ( int i = 0 ; i < Iterations ; i++ ) {
            count++;
        }

        return count;
    }

    [ Benchmark ]
    public ScalarsFloatClassWithInitProperties[] Scalars_Float_Class_WithInitProperties( ) {
        ScalarsFloatClassWithInitProperties[] results = new ScalarsFloatClassWithInitProperties[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatClassWithInitProperties>( ScalarsFloatClassWithInitProperties.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public ScalarsDecimalClass[] Scalars_Decimal_Class( ) {
        ScalarsDecimalClass[] results = new ScalarsDecimalClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsDecimalClass>( ScalarsDecimalClass.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    
    /* ScalarsNodaTime */
    [ Benchmark ]
    public ScalarsNodaTimeClass[] Scalars_NodaTime_Class( ) {
        ScalarsNodaTimeClass[] results = new ScalarsNodaTimeClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsNodaTimeClass>( ScalarsNodaTimeClass.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }
    
    [ Benchmark ]
    public NestedObjectNodaTimeClass[] NestedObjects_NodaTime_Class( ) {
        NestedObjectNodaTimeClass[] results = new NestedObjectNodaTimeClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectNodaTimeClass>( NestedObjectNodaTimeClass.JSON, _systemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
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
    public ScalarsFloatClass[] Scalars_Float_Class_SourceGen( ) {
        ScalarsFloatClass[] results = new ScalarsFloatClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatClass>( ScalarsFloatClass.JSON, _sourceGenerationContextWithoutOptions.ScalarsFloatClass ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public ScalarsFloatClassFields[] Scalars_Float_Class_Fields_SourceGen( ) {
        ScalarsFloatClassFields[] results = new ScalarsFloatClassFields[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatClassFields>( ScalarsFloatClassFields.JSON, _sourceGenerationContextWithFieldOptions.ScalarsFloatClassFields ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public ScalarsDecimalClass[] Scalars_Decimal_Class_SourceGen( ) {
        ScalarsDecimalClass[] results = new ScalarsDecimalClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsDecimalClass>( ScalarsDecimalClass.JSON, _sourceGenerationContextWithoutOptions.ScalarsDecimalClass ) ?? throw new NullReferenceException();
        }

        return results;
    }
    
    [ Benchmark ]
    public ScalarsNodaTimeClass[] Scalars_NodaTime_Class_SourceGen( ) {
        ScalarsNodaTimeClass[] results = new ScalarsNodaTimeClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsNodaTimeClass>( ScalarsNodaTimeClass.JSON, _sourceGenerationContextWithOptions.ScalarsNodaTimeClass ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
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
    public ScalarsNodaTimeClassWithAttribute[] Scalars_NodaTime_Class_ConverterAttribute( ) {
        ScalarsNodaTimeClassWithAttribute[] results = new ScalarsNodaTimeClassWithAttribute[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsNodaTimeClassWithAttribute>( ScalarsNodaTimeClassWithAttribute.JSON ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public NestedObjectNodaTimeClass[] NestedObjects_NodaTime_Class_SourceGen( ) {
        NestedObjectNodaTimeClass[] results = new NestedObjectNodaTimeClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectNodaTimeClass>( NestedObjectNodaTimeClass.JSON, _sourceGenerationContextWithOptions.NestedObjectNodaTimeClass ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public NestedObjectArrayNodaTimeClass[] NestedObjects_Arrays_NodaTime_Class_SourceGen( ) {
        NestedObjectArrayNodaTimeClass[] results = new NestedObjectArrayNodaTimeClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectArrayNodaTimeClass>( NestedObjectArrayNodaTimeClass.JSON, _sourceGenerationContextWithOptions.NestedObjectArrayNodaTimeClass ) ?? throw new NullReferenceException();
        }

        return results;
    }

    #endregion
}