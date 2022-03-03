using System;
using System.Text.Json;

using BenchmarkDotNet.Attributes;

using Common;

using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace Benchmarks.Json;

[Config(typeof(BenchmarkConfig))]
// ReSharper disable once InconsistentNaming
public class SystemTextJson_BasicBenchmarks {
    // ReSharper disable once UnassignedField.Global
    // ReSharper disable once MemberCanBePrivate.Global
    [ Params( 1000 ) ]
    public int Iterations = 1;

    public static readonly JsonSerializerOptions SystemTextJsonOptions = new JsonSerializerOptions() {
            // Converters = { new InstantUnixTimeMillisecondsConverter(  ) }
        }
        .ConfigureForNodaTime( NodaTime.DateTimeZoneProviders.Tzdb );
    
    
    private readonly SimpleSourceGenerationContext _sourceGenerationContextWithOptions = new SimpleSourceGenerationContext(SystemTextJsonOptions);
    private readonly SimpleSourceGenerationContext _sourceGenerationContextWithoutOptions = new SimpleSourceGenerationContext();

    #region Serialize
    
    [ Benchmark ]
    public string[] SystemTextJson_Serialize_Scalars_Float( ) {
        string[] results = new string[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Serialize<ScalarsFloat>( new ScalarsFloat( 1, "Hello World", 9.127f ), SystemTextJsonOptions );
        }

        return results;
    }

    [ Benchmark ]
    public string[] SystemTextJson_Serialize_Scalars_Decimal( ) {
        string[] results = new string[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Serialize<ScalarsDecimal>( new ScalarsDecimal( 1, "Hello World", 9.127m ), SystemTextJsonOptions );
        }

        return results;
    }

    [ Benchmark ]
    public string[] SystemTextJson_Serialize_Scalars_NodaTime( ) {
        string[] results = new string[ Iterations ];
        Instant  instant = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Serialize<ScalarsNodaTime>( new ScalarsNodaTime( 1, "Hello World", instant ), SystemTextJsonOptions );
        }

        return results;
    }

    [ Benchmark ]
    public string[] SystemTextJson_Serialize_NestedObjects_NodaTime( ) {
        string[] results = new string[ Iterations ];
        Instant  instant = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Serialize<NestedObjectNodaTime>( new NestedObjectNodaTime( 1, "Parent Object", new ScalarsNodaTime( 1, "Hello World", instant ) ), SystemTextJsonOptions );
        }

        return results;
    }

    [ Benchmark ]
    public string[] SystemTextJson_Serialize_NestedObjects_Arrays_NodaTime( ) {
        string[] results = new string[ Iterations ];
        Instant  instant = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Serialize<NestedObjectArrayNodaTime>( new NestedObjectArrayNodaTime( 1, "Parent Object", new[] {
                                                                                                         new ScalarsNodaTime( 1, "Hello World", instant ), new ScalarsNodaTime( 1, "Hello World", instant ),
                                                                                                     }
                                                                                                 ), SystemTextJsonOptions );
        }

        return results;
    }
    
    #endregion
    
    #region Deserialize

    [ Benchmark ]
    public ScalarsFloat[] SystemTextJson_Deserialize_Scalars_Float( ) {
        ScalarsFloat[] results = new ScalarsFloat[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloat>( ScalarsFloat.JSON, SystemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public ScalarsDecimal[] SystemTextJson_Deserialize_Scalars_Decimal( ) {
        ScalarsDecimal[] results = new ScalarsDecimal[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsDecimal>( ScalarsDecimal.JSON, SystemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public ScalarsNodaTime[] SystemTextJson_Deserialize_Scalars_NodaTime( ) {
        ScalarsNodaTime[] results = new ScalarsNodaTime[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsNodaTime>( ScalarsNodaTime.JSON, SystemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public NestedObjectNodaTime[] SystemTextJson_Deserialize_NestedObjects_NodaTime( ) {
        NestedObjectNodaTime[] results = new NestedObjectNodaTime[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectNodaTime>( NestedObjectNodaTime.JSON, SystemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }

    [ Benchmark ]
    public NestedObjectArrayNodaTime[] SystemTextJson_Deserialize_NestedObjects_Arrays_NodaTime( ) {
        NestedObjectArrayNodaTime[] results = new NestedObjectArrayNodaTime[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectArrayNodaTime>( NestedObjectArrayNodaTime.JSON, SystemTextJsonOptions ) ?? throw new NullReferenceException();
        }

        return results;
    }
    
    #endregion
    
    #region SourceGeneratorContext
    
    
    [ Benchmark ]
    public ScalarsFloatClass[] SystemTextJson_Deserialize_Scalars_Float_SourceGen( ) {
        ScalarsFloatClass[] results = new ScalarsFloatClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatClass>( ScalarsFloatClass.JSON, _sourceGenerationContextWithoutOptions.ScalarsFloatClass ) ?? throw new NullReferenceException();
        }
    
        return results;
    }
    
    [ Benchmark ]
    public ScalarsFloatClassFields[] SystemTextJson_Deserialize_Scalars_FloatFields_SourceGen( ) {
        ScalarsFloatClassFields[] results = new ScalarsFloatClassFields[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsFloatClassFields>( ScalarsFloatClassFields.JSON, _sourceGenerationContextWithoutOptions.ScalarsFloatClassFields ) ?? throw new NullReferenceException();
        }
    
        return results;
    }
    
    [ Benchmark ]
    public ScalarsDecimalClass[] SystemTextJson_Deserialize_Scalars_Decimal_SourceGen( ) {
        ScalarsDecimalClass[] results = new ScalarsDecimalClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsDecimalClass>( ScalarsDecimalClass.JSON, _sourceGenerationContextWithoutOptions.ScalarsDecimalClass ) ?? throw new NullReferenceException();
        }
    
        return results;
    }
    
    [ Benchmark ]
    public ScalarsNodaTimeClass[] SystemTextJson_Deserialize_Scalars_NodaTime_SourceGen( ) {
        ScalarsNodaTimeClass[] results = new ScalarsNodaTimeClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsNodaTimeClass>( ScalarsNodaTimeClass.JSON, _sourceGenerationContextWithOptions.ScalarsNodaTimeClass ) ?? throw new NullReferenceException();
        }
    
        return results;
    }
    [ Benchmark ]
    public ScalarsNodaTimeClassWithAttribute[] SystemTextJson_Deserialize_Scalars_NodaTimeWithAttribute_SourceGen( ) {
        ScalarsNodaTimeClassWithAttribute[] results = new ScalarsNodaTimeClassWithAttribute[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsNodaTimeClassWithAttribute>( ScalarsNodaTimeClassWithAttribute.JSON, _sourceGenerationContextWithoutOptions.ScalarsNodaTimeClassWithAttribute ) ?? throw new NullReferenceException(); // URGENT: why doesn't the attribute work?
        }
    
        return results;
    }
    [ Benchmark ]
    public ScalarsNodaTimeClassWithAttribute[] SystemTextJson_Deserialize_Scalars_NodaTimeWithAttribute( ) {
        ScalarsNodaTimeClassWithAttribute[] results = new ScalarsNodaTimeClassWithAttribute[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ScalarsNodaTimeClassWithAttribute>( ScalarsNodaTimeClassWithAttribute.JSON, SystemTextJsonOptions ) ?? throw new NullReferenceException();
        }
    
        return results;
    }
    
    [ Benchmark ]
    public NestedObjectNodaTimeClass[] SystemTextJson_Deserialize_NestedObjects_NodaTime_SourceGen( ) {
        NestedObjectNodaTimeClass[] results = new NestedObjectNodaTimeClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectNodaTimeClass>( NestedObjectNodaTimeClass.JSON, _sourceGenerationContextWithOptions.NestedObjectNodaTimeClass ) ?? throw new NullReferenceException();
        }
    
        return results;
    }
    
    [ Benchmark ]
    public NestedObjectArrayNodaTimeClass[] SystemTextJson_Deserialize_NestedObjects_Arrays_NodaTime_SourceGen( ) {
        NestedObjectArrayNodaTimeClass[]   results                 = new NestedObjectArrayNodaTimeClass[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<NestedObjectArrayNodaTimeClass>( NestedObjectArrayNodaTimeClass.JSON, _sourceGenerationContextWithOptions.NestedObjectArrayNodaTimeClass ) ?? throw new NullReferenceException();
        }
    
        return results;
    }
    
    
    #endregion
}