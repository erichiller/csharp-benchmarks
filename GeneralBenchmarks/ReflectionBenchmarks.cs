using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Benchmarks.General;

/*

   | Method                                                                   | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0   | Allocated | Alloc Ratio |
   |------------------------------------------------------------------------- |----------:|----------:|----------:|------:|--------:|-------:|----------:|------------:|
   | CreateTypeUsing_NonGeneric_Activator_NoParams                            |  9.313 ns | 0.0776 ns | 0.0688 ns |  1.00 |    0.01 | 0.0068 |      32 B |        1.00 |
   | CreateTypeUsing_Generic_Activator_NoParams                               | 12.484 ns | 0.1311 ns | 0.1226 ns |  1.34 |    0.02 | 0.0068 |      32 B |        1.00 |
   | GetConstructors_CallOnly                                                 | 13.652 ns | 0.2440 ns | 0.2283 ns |  1.47 |    0.03 | 0.0068 |      32 B |        1.00 |
   | GetConstructors_Any                                                      | 29.646 ns | 0.2362 ns | 0.2209 ns |  3.18 |    0.03 | 0.0068 |      32 B |        1.00 |
   | CreateTypeUsing_GetConstructor_Invoke_NoParams_2                         | 34.975 ns | 0.4317 ns | 0.3827 ns |  3.76 |    0.05 | 0.0068 |      32 B |        1.00 |
   | CreateTypeUsing_GetConstructor_Invoke_NoParams_1                         | 35.062 ns | 0.7069 ns | 0.7857 ns |  3.77 |    0.09 | 0.0068 |      32 B |        1.00 |
   | CreateTypeUsing_GetConstructors_Invoke_NoParams                          | 39.462 ns | 0.6636 ns | 0.6208 ns |  4.24 |    0.07 | 0.0136 |      64 B |        2.00 |
   | CreateTypeUsing_NonGeneric_Activator_NoParams_GetConstructors_Any_Before | 41.936 ns | 0.0890 ns | 0.0789 ns |  4.50 |    0.03 | 0.0136 |      64 B |        2.00 |

 */

[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class ConstructInstanceByReflectionBenchmarks {
    [ Benchmark( Baseline = true ) ]
    public void CreateTypeUsing_NonGeneric_Activator_NoParams( ) {
        if ( Activator.CreateInstance( typeof(TypeToCreate_ParameterlessConstructor) ) is not { } ) {
            throw new Exception();
        }
    }

    [ Benchmark ]
    public void CreateTypeUsing_Generic_Activator_NoParams( ) {
        if ( Activator.CreateInstance<TypeToCreate_ParameterlessConstructor>() is not { } ) {
            throw new Exception();
        }
    }

    /*
     *
     */

    [ Benchmark ]
    public void GetConstructors_CallOnly( ) {
        if ( Activator.CreateInstance<TypeToCreate_ParameterlessConstructor>() is not { } ) {
            throw new Exception();
        }
    }

    [ Benchmark ]
    public void GetConstructors_Any( ) {
        if ( typeof(TypeToCreate_ParameterlessConstructor)
             .GetConstructors( BindingFlags.Public | BindingFlags.Instance )
             .Any( ctor => ctor.GetParameters().Length == 0 ) is false ) {
            throw new Exception();
        }
    }

    [ Benchmark ]
    public void CreateTypeUsing_NonGeneric_Activator_NoParams_GetConstructors_Any_Before( ) {
        if ( typeof(TypeToCreate_ParameterlessConstructor)
             .GetConstructors( BindingFlags.Public | BindingFlags.Instance )
             .Any( ctor => ctor.GetParameters().Length == 0 ) is false || Activator.CreateInstance( typeof(TypeToCreate_ParameterlessConstructor) ) is not { } ) {
            throw new Exception();
        }
    }

    /*
     *
     */

    [ Benchmark ]
    public void CreateTypeUsing_GetConstructors_Invoke_NoParams( ) {
        if ( typeof(TypeToCreate_ParameterlessConstructor)
             .GetConstructors( BindingFlags.Public | BindingFlags.Instance )[ 0 ]
             .Invoke( parameters: [ ] ) is not { } ) {
            throw new Exception();
        }
    }

    [ Benchmark ]
    public void CreateTypeUsing_GetConstructor_Invoke_NoParams_1( ) {
        if ( typeof(TypeToCreate_ParameterlessConstructor)
             .GetConstructor( types: [ ] )!
             .Invoke( null ) is not { } ) {
            throw new Exception();
        }
    }

    [ Benchmark ]
    public void CreateTypeUsing_GetConstructor_Invoke_NoParams_2( ) {
        if ( typeof(TypeToCreate_ParameterlessConstructor)
             .GetConstructor( BindingFlags.Public | BindingFlags.Instance, types: [ ] )!
             .Invoke( parameters: [ ] ) is not { } ) {
            throw new Exception();
        }
    }

    [ SuppressMessage( "ReSharper", "UnusedMember.Local" ) ]
    [ SuppressMessage( "ReSharper", "InconsistentNaming" ) ]
    private class TypeToCreate_ParameterlessConstructor {
        public int     Id   { get; set; }
        public string? Name { get; set; }
    }
}

/*
 * | Method       | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
   |------------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
   | Typeof       | 2.003 us | 0.0237 us | 0.0222 us |  1.00 |    0.02 |         - |          NA |
   | CachedTypeof | 2.088 us | 0.0012 us | 0.0010 us |  1.04 |    0.01 |         - |          NA |

 */
[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class TypeofCachingBenchmarks {
    private static readonly Type[] _types      = [ typeof(int), typeof(string) ];
    private static readonly Type   _typeInt    = typeof(int);
    private static readonly Type   _typeString = typeof(string);
    private static readonly int    _iterations = 1_000;

    [ Benchmark ]
    public void CachedTypeof( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            var  type    = _types[ i % 2 ];
            bool isMatch = type == _typeInt || type == _typeString;
            Utils.AssertThat( isMatch );
        }
    }

    [ Benchmark( Baseline = true ) ]
    public void Typeof( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            var  type    = _types[ i % 2 ];
            bool isMatch = type == typeof(int) || type == typeof(string);
            Utils.AssertThat( isMatch );
        }
    }
}

/*
   | Method                                                | Mean      | Error     | StdDev    | Allocated |
   |------------------------------------------------------ |----------:|----------:|----------:|----------:|
   | InstanceIsListOnly                                    |  3.192 us | 0.0014 us | 0.0012 us |         - |
   | GenericTypeDefOnly                                    |  4.414 us | 0.0023 us | 0.0022 us |         - |
   | AssignableToIList_InstancePreCheck_GenericTypeDef_50p |  6.835 us | 0.0950 us | 0.0842 us |         - |
   | AssignableToIList_InstancePreCheck_50p                |  7.185 us | 0.0137 us | 0.0128 us |         - |
   | AssignableToIListOnly                                 |  9.255 us | 0.1829 us | 0.1711 us |         - |
   | IsArrayAssignableToIList                              | 10.435 us | 0.0150 us | 0.0140 us |         - |

 */

[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class ListCheckBenchmarks {
    private static readonly int       _iterations = 1_000;
    private static readonly Type[]    _types      = [ typeof(string[]), typeof(List<string>), typeof(string[]), typeof(List<string>), typeof(IList<string>) ];
    private static readonly object?[] _instance   = [ Array.Empty<string>(), new List<int>(), null, null, new List<string>() ];
    private static readonly bool[]    _isList     = [ true, true, false, false, true ];


    [ Benchmark ]
    public void InstanceIsListOnly( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            bool isList = _instance[ i % 2 ] is IList;
            Utils.AssertThat( isList );
        }
    }

    [ Benchmark ]
    public void GenericTypeDefOnly( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            // ReSharper disable once UnusedVariable
            int  mod    = i % 4; // only run one mod calc
            Type type   = _types[ 1 ];
            bool isList = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
            Utils.AssertThat( isList );
        }
    }

    [ Benchmark ]
    public void AssignableToIList_InstancePreCheck_GenericTypeDef_50p( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            int  mod    = i % 4; // only run one mod calc
            Type type   = _types[ mod ];
            bool isList = _instance[ mod ] is IList || ( type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>) ) || type.IsAssignableTo( typeof(IList) );
            Utils.AssertThat( isList );
        }
    }

    [ Benchmark ]
    public void AssignableToIList_InstancePreCheck_50p( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            int  mod    = i % 4; // only run one mod calc
            bool isList = _instance[ mod ] is IList || _types[ mod ].IsAssignableTo( typeof(IList) );
            Utils.AssertThat( isList );
        }
    }

    [ Benchmark ]
    public void AssignableToIListOnly( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            bool isList = _types[ i % 2 ].IsAssignableTo( typeof(IList) );
            Utils.AssertThat( isList );
        }
    }

    [ Benchmark ]
    public void IsArrayAssignableToIList( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            var  type   = _types[ i % 2 ];
            bool isList = type.IsArray || type.IsAssignableTo( typeof(IList) );
            Utils.AssertThat( isList );
        }
    }
}

/*
 * | Method                  | Mean        | Error     | StdDev    | Gen0   | Allocated |
   |------------------------ |------------:|----------:|----------:|-------:|----------:|
   | TypeEqualsIList         |   0.7214 ns | 0.0030 ns | 0.0026 ns |      - |         - |
   | InstanceIsList          |   0.7312 ns | 0.0024 ns | 0.0022 ns |      - |         - |
   | TypeIsAssignableToIList |  17.0230 ns | 0.0186 ns | 0.0174 ns |      - |         - |
   | TypeGetInterfaceIList   |  72.5801 ns | 0.1296 ns | 0.1212 ns |      - |         - |
   | ImplementsIListT        | 121.0737 ns | 2.2522 ns | 2.1067 ns | 0.0391 |     184 B |
 */
[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class ListDictCheckBenchmarks {
    private static readonly object _listInstance = new List<string>();

    private static readonly object _dictionaryKvInstance         = new Dictionary<string, string>();
    private static readonly object _readOnlyDictionaryKvInstance = ReadOnlyDictionary<string, string>.Empty;

    private static readonly Type _listInterfaceType   = typeof(IList);
    private static readonly Type _listTType           = typeof(List<>);
    private static readonly Type _listTInterfaceType  = typeof(IList<>);
    private static readonly Type _listTInterfaceType2 = typeof(IList<>);

    private static readonly Type _dictionaryKvInterfaceType         = typeof(IDictionary<,>);
    private static readonly Type _readOnlyDictionaryKvInterfaceType = typeof(IReadOnlyDictionary<,>);


    [ Benchmark ]
    public void InstanceIsList( ) {
        // do 1 success and one fail.
        Utils.AssertThat( _listInstance is IList ^ _dictionaryKvInstance is IList );
    }

    [ Benchmark ]
    public void TypeEqualsIList( ) {
        Utils.AssertThat( ( _listTInterfaceType == _listTInterfaceType2 ) ^ ( _dictionaryKvInterfaceType == _listInterfaceType ) );
    }

    [ Benchmark ]
    public void TypeIsAssignableToIList( ) {
        Utils.AssertThat( _listTType.IsAssignableTo( _listInterfaceType ) ^ _dictionaryKvInterfaceType.IsAssignableTo( _listInterfaceType ) );
    }

    [ Benchmark ]
    public void TypeGetInterfaceIList( ) {
        Utils.AssertThat( ( _listTType.GetInterface( _listInterfaceType.Name ) is { } ) ^ ( _dictionaryKvInterfaceType.GetInterface( _listInterfaceType.Name ) is { } ) );
    }

    [Benchmark]
    public void ImplementsIListT( ) {
        Utils.AssertThat( Implements(_listTType, _listInterfaceType )  ^ Implements( _dictionaryKvInterfaceType, _listInterfaceType )  );
    }

    public static bool Implements( Type type, Type checkType ) {
        ArgumentNullException.ThrowIfNull( checkType );
        if ( type.IsInterface && !checkType.IsInterface ) {
            // an interface can't implement a concrete type
            throw new ArgumentException( $"An interface can not implement a concrete type. {nameof(type)}={type.FullName} is an interface and {nameof(checkType)}={checkType.FullName} is not" );
        }

        if ( type == checkType ) {
            return true;
        }

        // for when the checkType is a generic type definition
        if ( checkType.IsGenericTypeDefinition ) {
            Type? genericTypeLoopVar = type;
            do {
                // checks BaseType recursively
                if ( genericTypeLoopVar.IsGenericType && genericTypeLoopVar.GetGenericTypeDefinition() == checkType ) {
                    return true;
                }
            } while ( ( genericTypeLoopVar = genericTypeLoopVar.BaseType ) is { } );
            return type.GetInterfaces().Any( i => i.IsGenericType && i.GetGenericTypeDefinition() == checkType ); // checks interface types
        }

        // for closed generic types or non-generic types
        if ( checkType.IsInterface && type.GetInterfaces().Contains( checkType ) ) {
            return true;
        }

        return type.IsSubclassOf( checkType );
    }
}

/*
 * | Method                                   | PctEnumT | Mean       | Error   | StdDev  | Gen0     | Allocated |
   |----------------------------------------- |--------- |-----------:|--------:|--------:|---------:|----------:|
   | CheckWithNullable_GetUnderlyingType_Only | P5       |   729.8 us | 1.31 us | 1.23 us |  40.0391 | 187.53 KB |
   | CheckWithNullable_GetTypeConverter       | P5       |   731.2 us | 1.65 us | 1.55 us |  33.2031 | 156.27 KB |
   | CheckWithNullable_GetUnderlyingType_2    | P5       |   739.6 us | 1.42 us | 1.26 us |  40.0391 | 187.53 KB |
   | CheckWithNullable_GetUnderlyingType_1    | P5       |   743.6 us | 3.85 us | 3.21 us |  40.0391 | 187.53 KB |
   | CheckWithNullable_Reflection             | P5       |   779.7 us | 1.11 us | 0.98 us |  40.0391 | 187.53 KB |
   | CheckWithNullable_Reflection_2           | P5       |   840.5 us | 2.73 us | 2.55 us |  46.8750 | 218.77 KB |
   | CheckWithNullable_GetUnderlyingType_2    | P20      | 1,670.7 us | 2.10 us | 1.86 us | 115.2344 | 531.32 KB |
   | CheckWithNullable_GetUnderlyingType_1    | P20      | 1,672.8 us | 4.07 us | 3.81 us | 115.2344 | 531.32 KB |
   | CheckWithNullable_Reflection             | P20      | 1,690.6 us | 9.14 us | 8.55 us | 115.2344 | 531.32 KB |
   | CheckWithNullable_GetUnderlyingType_Only | P20      | 1,695.1 us | 8.21 us | 7.68 us | 115.2344 | 531.32 KB |
   | CheckWithNullable_GetTypeConverter       | P20      | 1,729.0 us | 2.18 us | 2.04 us |  87.8906 | 406.29 KB |
   | CheckWithNullable_Reflection_2           | P20      | 2,155.2 us | 3.68 us | 3.44 us | 140.6250 | 656.32 KB |
 *
 *
 */

[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class NullableEnumCheckBenchmarks {
    private static readonly int? _iterations = 1_000;

    public enum PercentageEnumType {
        // P100 = 1, // 100%  of the time the value will be a Nullable Enum.
        // P80  = 2,
        // P60  = 3,
        // P40  = 4,
        P20 = 5, // 1/5 (20%) of the time the value will be a Nullable Enum.
        P5  = 20 // 1/20 (5%) of the time the value will be a Nullable Enum.
    }

    [ ParamsAllValues ] public PercentageEnumType PctEnumT { get; set; }


    private readonly (Type type, string str)[] _types = new (Type type, string str)[ 20 ];

    [ GlobalSetup ]
    public void Setup( ) {
        (Type type, string str)[] typesSource = [
            ( typeof(PercentageEnumType?), nameof(PercentageEnumType.P20) ), // 1
            ( typeof(PercentageEnumType), nameof(PercentageEnumType.P20) ),  // 2
            ( typeof(string), "a string" ),                                  // 3
            ( typeof(int), "3" ),                                            // 4
            ( typeof(System.Uri), "http://localhost.com" ),                  // 5. something else to use the TypeConverter system
            ( typeof(string), "a string" ),                                  // 6
            ( typeof(int?), "3" ),                                           // 7
            ( typeof(string), "a string" ),                                  // 8
            ( typeof(int), "3" ),                                            // 9
            ( typeof(string), "a string" ),                                  // 10
            ( typeof(int?), "3" ),                                           // 11
            ( typeof(string), "a string" ),                                  // 12
            ( typeof(int), "3" ),                                            // 13
            ( typeof(int?), "3" ),                                           // 14
            ( typeof(string), "a string" ),                                  // 15
            ( typeof(int), "3" ),                                            // 16
            ( typeof(int?), "3" ),                                           // 17
            ( typeof(string), "a string" ),                                  // 18
            ( typeof(int), "3" ),                                            // 19
            ( typeof(System.Uri), "http://localhost.com" ),                  // 20. something else to use the TypeConverter system
        ];
        for ( int i = 0 ; i < typesSource.Length ; i++ ) {
            _types[ i ] = typesSource[ i % ( int )PctEnumT ];
        }
    }

    [ Benchmark ]
    public void CheckWithNullable_GetUnderlyingType_Only( ) {
        int typesLength = _types.Length;
        for ( int loop = 0 ; loop < _iterations ; loop++ ) {
            for ( int i = 0 ; i < typesLength ; i++ ) {
                ( Type destinationType, string inputValue ) = _types[ i ];
                if ( destinationType == typeof(int) || destinationType == typeof(int?) ) {
                    Int32.TryParse( inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if ( destinationType == typeof(string) ) {
                    continue;
                }
                if ( destinationType is { IsEnum : true } ) {
                    Enum.TryParse( destinationType, inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if (
                    Nullable.GetUnderlyingType( destinationType ) is { IsEnum: true } underlyingType
                ) {
                    Enum.TryParse( underlyingType, inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if ( TypeDescriptor.GetConverter( destinationType ) is { } converter && converter.CanConvertFrom( typeof(string) ) ) {
                    var parsed = converter.ConvertFrom( inputValue );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                Utils.Throw( "Invalid" );
            }
        }
    }

    [ Benchmark ]
    public void CheckWithNullable_GetUnderlyingType_1( ) {
        int typesLength = _types.Length;
        for ( int loop = 0 ; loop < _iterations ; loop++ ) {
            for ( int i = 0 ; i < typesLength ; i++ ) {
                ( Type destinationType, string inputValue ) = _types[ i ];
                if ( destinationType == typeof(int) || destinationType == typeof(int?) ) {
                    Int32.TryParse( inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if ( destinationType == typeof(string) ) {
                    continue;
                }
                if ( destinationType is { IsEnum : true } ) {
                    Enum.TryParse( destinationType, inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if (
                    destinationType is { IsGenericType                          : true }
                    && Nullable.GetUnderlyingType( destinationType ) is { IsEnum: true } underlyingType
                ) {
                    Enum.TryParse( underlyingType, inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if ( TypeDescriptor.GetConverter( destinationType ) is { } converter && converter.CanConvertFrom( typeof(string) ) ) {
                    var parsed = converter.ConvertFrom( inputValue );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                Utils.Throw( "Invalid" );
            }
        }
    }

    [ Benchmark ]
    public void CheckWithNullable_GetUnderlyingType_2( ) {
        int typesLength = _types.Length;
        for ( int loop = 0 ; loop < _iterations ; loop++ ) {
            for ( int i = 0 ; i < typesLength ; i++ ) {
                ( Type destinationType, string inputValue ) = _types[ i ];
                if ( destinationType == typeof(int) || destinationType == typeof(int?) ) {
                    Int32.TryParse( inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if ( destinationType == typeof(string) ) {
                    continue;
                }
                if ( destinationType is { IsEnum : true } ) {
                    Enum.TryParse( destinationType, inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if (
                    destinationType is { IsValueType                            : true, IsGenericType: true }
                    && Nullable.GetUnderlyingType( destinationType ) is { IsEnum: true } underlyingType
                ) {
                    Enum.TryParse( underlyingType, inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if ( TypeDescriptor.GetConverter( destinationType ) is { } converter && converter.CanConvertFrom( typeof(string) ) ) {
                    var parsed = converter.ConvertFrom( inputValue );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                Utils.Throw( "Invalid" );
            }
        }
    }

    [ Benchmark ]
    public void CheckWithNullable_Reflection( ) {
        int typesLength = _types.Length;
        for ( int loop = 0 ; loop < _iterations ; loop++ ) {
            for ( int i = 0 ; i < typesLength ; i++ ) {
                ( Type destinationType, string inputValue ) = _types[ i ];
                if ( destinationType == typeof(int) || destinationType == typeof(int?) ) {
                    Int32.TryParse( inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if ( destinationType == typeof(string) ) {
                    continue;
                }
                if ( destinationType is { IsEnum : true } ) {
                    Enum.TryParse( destinationType, inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if (
                    destinationType is { IsValueType : true, IsGenericType: true }
                    && destinationType.GetGenericTypeDefinition() == typeof(Nullable<>)
                ) {
                    Enum.TryParse( destinationType.GenericTypeArguments[ 0 ], inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if ( TypeDescriptor.GetConverter( destinationType ) is { } converter && converter.CanConvertFrom( typeof(string) ) ) {
                    var parsed = converter.ConvertFrom( inputValue );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                Utils.Throw( "Invalid" );
            }
        }
    }

    [ Benchmark ]
    public void CheckWithNullable_Reflection_2( ) {
        int typesLength = _types.Length;
        for ( int loop = 0 ; loop < _iterations ; loop++ ) {
            for ( int i = 0 ; i < typesLength ; i++ ) {
                ( Type destinationType, string inputValue ) = _types[ i ];
                if ( destinationType == typeof(int) || destinationType == typeof(int?) ) {
                    Int32.TryParse( inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if ( destinationType == typeof(string) ) {
                    continue;
                }
                if ( destinationType is { IsEnum : true } ) {
                    Enum.TryParse( destinationType, inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if (
                    destinationType is { IsValueType : true, IsGenericType: true, GenericTypeArguments: [ { } ] }
                ) {
                    Enum.TryParse( destinationType.GenericTypeArguments[ 0 ], inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if ( TypeDescriptor.GetConverter( destinationType ) is { } converter && converter.CanConvertFrom( typeof(string) ) ) {
                    var parsed = converter.ConvertFrom( inputValue );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                Utils.Throw( "Invalid" );
            }
        }
    }

    [ Benchmark ]
    public void CheckWithNullable_GetTypeConverter( ) {
        int typesLength = _types.Length;
        for ( int loop = 0 ; loop < _iterations ; loop++ ) {
            for ( int i = 0 ; i < typesLength ; i++ ) {
                ( Type destinationType, string inputValue ) = _types[ i ];
                if ( destinationType == typeof(int) || destinationType == typeof(int?) ) {
                    Int32.TryParse( inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if ( destinationType == typeof(string) ) {
                    continue;
                }
                if ( destinationType is { IsEnum : true } ) {
                    Enum.TryParse( destinationType, inputValue, out var parsed );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                if ( TypeDescriptor.GetConverter( destinationType ) is { } converter && converter.CanConvertFrom( typeof(string) ) ) {
                    var parsed = converter.ConvertFrom( inputValue );
                    Utils.AssertThat( parsed is { } );
                    continue;
                }
                Utils.Throw( "Invalid" );
            }
        }
    }
}

/*
 * | Method     | Mean     | Error    | StdDev   | Ratio | Gen0   | Allocated | Alloc Ratio |
   |----------- |---------:|---------:|---------:|------:|-------:|----------:|------------:|
   | GeneralSet | 13.80 us | 0.101 us | 0.094 us |  1.00 | 3.3875 |  15.63 KB |        1.00 |
   | PerTypeSet | 15.46 us | 0.045 us | 0.038 us |  1.12 | 3.3875 |  15.63 KB |        1.00 |

 */
[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class SetListBenchmarks {
    private readonly IList[] _lists = [
        Enumerable.Range( 0, _iterations ).ToList(),
        Enumerable.Range( 0, _iterations ).Select( static s => s.ToString() ).ToList(),
        Enumerable.Range( 0, _iterations ).Select( static s => ( float )s / 2.1 ).ToList()
    ];
    const int _iterations = 1000;

    [ Benchmark( Baseline = true ) ]
    public void GeneralSet( ) {
        for ( int listIndex = 0 ; listIndex < _iterations ; listIndex++ ) {
            IList   list  = _lists[ listIndex % _lists.Length ];
            object? value = list[ listIndex ];
            list[ listIndex ] = value;
        }
    }

    [ Benchmark ]
    public void PerTypeSet( ) {
        for ( int listIndex = 0 ; listIndex < _iterations ; listIndex++ ) {
            IList   list  = _lists[ listIndex % _lists.Length ];
            object? value = list[ listIndex ];
            _ = list switch {
                    // {IsReadOnly: true } =>
                    IList<int> intList when value is int intValue           => intList[ listIndex ]   = intValue,
                    IList<string> typedList when value is string typedValue => typedList[ listIndex ] = typedValue,
                    _                                                       => list[ listIndex ]      = value
                };
        }
    }
}

/*
 * | Method                    | Mean      | Error     | StdDev    | Ratio | Gen0   | Allocated | Alloc Ratio |
   |-------------------------- |----------:|----------:|----------:|------:|-------:|----------:|------------:|
   | NullableDirect            |  5.011 us | 0.0064 us | 0.0060 us |  0.12 |      - |         - |        0.00 |
   | NullableGetUnderlyingType | 42.055 us | 0.4825 us | 0.4277 us |  1.00 | 2.4414 |   11617 B |        1.00 |

 */
[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class NullableTypeEquivalenceBenchmarks {
    private static readonly Type[] _types = [
        typeof(int), typeof(string), typeof(string),
        typeof(int?), typeof(string), typeof(int), typeof(int?),
        typeof(double), typeof(decimal), typeof(decimal?), typeof(double?)
    ];
    private readonly bool[] _typeIsInt     = [ true, false, false, true, false, true, true, false, false, false, false ];
    private readonly bool[] _typeIsDouble  = [ false, false, false, false, false, false, false, true, false, false, true ];
    private readonly bool[] _typeIsDecimal = [ false, false, false, false, false, false, false, false, true, true, false ];

    private const int _count = 11;

    private static readonly int _iterations = 1_000;

    [ Benchmark ]
    public void NullableDirect( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            var  type      = _types[ i % _count ];
            bool isInt     = type == typeof(int)     || type == typeof(int?);
            bool isDouble  = type == typeof(double)  || type == typeof(double?);
            bool isDecimal = type == typeof(decimal) || type == typeof(decimal?);
            Utils.AssertThat( isInt        == _typeIsInt[ i     % _count ]
                              && isDouble  == _typeIsDouble[ i  % _count ]
                              && isDecimal == _typeIsDecimal[ i % _count ] );
        }
    }

    [ Benchmark( Baseline = true ) ]
    public void NullableGetUnderlyingType( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            var type = _types[ i % _count ];
            type = type.IsValueType ? Nullable.GetUnderlyingType( type ) ?? type : type;
            bool isInt     = type == typeof(int);
            bool isDouble  = type == typeof(double);
            bool isDecimal = type == typeof(decimal);
            Utils.AssertThat( isInt        == _typeIsInt[ i     % _count ]
                              && isDouble  == _typeIsDouble[ i  % _count ]
                              && isDecimal == _typeIsDecimal[ i % _count ] );
        }
    }
}

/*
 * | Method                        | Mean     | Error   | StdDev  | Gen0    | Allocated |
   |------------------------------ |---------:|--------:|--------:|--------:|----------:|
   | GenericTypeArgumentsProperty  | 467.0 us | 0.88 us | 0.82 us | 37.1094 | 171.88 KB |
   | GetGenericTypeArgumentsMethod | 474.0 us | 1.76 us | 1.56 us | 37.1094 | 171.88 KB |
 */
[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class GetGenericTypeArgumentsBenchmarks {
    private const int _iterations = 1_000;
    private readonly Type[] _types = [
        typeof(List<string>),
        typeof(Dictionary<string, string>),
        typeof(List<int>),
        typeof(List<double>),
        typeof(Dictionary<string, int>),
    ];

    [ Benchmark ]
    public void GenericTypeArgumentsProperty( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            for ( int t = 0 ; t < _types.Length ; t++ ) {
                Utils.AssertThat( _types[ t ].GenericTypeArguments.Length is 1 or 2 );
            }
        }
    }

    [ Benchmark ]
    public void GetGenericTypeArgumentsMethod( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            for ( int t = 0 ; t < _types.Length ; t++ ) {
                Utils.AssertThat( _types[ t ].GetGenericArguments().Length is 1 or 2 );
            }
        }
    }
}

/*
 *
   | Method                                                               | Mean     | Error   | StdDev  | Gen0    | Allocated |
   |--------------------------------------------------------------------- |---------:|--------:|--------:|--------:|----------:|
   | NullableGetUnderlyingType_only                                       | 218.3 us | 0.79 us | 0.70 us | 13.4277 |   62.5 KB |
   | IsValueType_NullableGetUnderlyingType                                | 222.1 us | 1.07 us | 0.95 us | 13.4277 |   62.5 KB |
   | IsGeneric_CompareOpenGeneric_NullableGetUnderlyingType               | 228.4 us | 0.73 us | 0.68 us | 13.4277 |   62.5 KB |
   | IsValueType_IsGeneric_CompareOpenGeneric_NullableGetUnderlyingType_A | 229.9 us | 0.17 us | 0.15 us | 13.4277 |   62.5 KB |
   | IsValueType_IsGeneric_CompareOpenGeneric_NullableGetUnderlyingType_B | 237.5 us | 1.91 us | 1.79 us | 13.4277 |   62.5 KB |
   | IsValueType_IsGeneric_NullableGetUnderlyingType_A                    | 253.8 us | 0.65 us | 0.61 us | 13.1836 |   62.5 KB |
 */
[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class NullableValueTypeDetectionBenchmarks {
    private const    int    _iterations     = 1_000;
    private readonly Type[] _types          = [ typeof(string), typeof(string), typeof(int?), typeof(int), typeof(int), typeof(bool), typeof(bool?) ];
    private readonly bool[] _typeIsNullable = [ false, false, true, false, false, false, true ];

    [ Benchmark ]
    public void NullableGetUnderlyingType_only( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            for ( int t = 0 ; t < _types.Length ; t++ ) {
                Type type       = _types[ t ];
                bool isNullable = Nullable.GetUnderlyingType( type ) is { IsGenericType: false };
                Utils.AssertThat( isNullable == _typeIsNullable[ t ] );
            }
        }
    }

    [ Benchmark ]
    public void IsGeneric_CompareOpenGeneric_NullableGetUnderlyingType( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            for ( int t = 0 ; t < _types.Length ; t++ ) {
                Type type = _types[ t ];
                bool isNullable = type.IsGenericType
                                  && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                                  && Nullable.GetUnderlyingType( type ) is { IsGenericType: false };
                Utils.AssertThat( isNullable == _typeIsNullable[ t ] );
            }
        }
    }

    [ Benchmark ]
    public void IsValueType_IsGeneric_CompareOpenGeneric_NullableGetUnderlyingType_A( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            for ( int t = 0 ; t < _types.Length ; t++ ) {
                Type type = _types[ t ];
                bool isNullable = type.IsValueType
                                  && type.IsGenericType
                                  && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                                  && Nullable.GetUnderlyingType( type ) is { IsGenericType: false };
                Utils.AssertThat( isNullable == _typeIsNullable[ t ] );
            }
        }
    }

    [ Benchmark ]
    public void IsValueType_NullableGetUnderlyingType( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            for ( int t = 0 ; t < _types.Length ; t++ ) {
                Type type = _types[ t ];
                bool isNullable = type.IsValueType
                                  && Nullable.GetUnderlyingType( type ) is { IsGenericType: false };
                Utils.AssertThat( isNullable == _typeIsNullable[ t ] );
            }
        }
    }

    [ Benchmark ]
    public void IsValueType_IsGeneric_NullableGetUnderlyingType_A( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            for ( int t = 0 ; t < _types.Length ; t++ ) {
                Type type = _types[ t ];
                bool isNullable = type.IsValueType
                                  && type.IsGenericType
                                  // && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                                  && Nullable.GetUnderlyingType( type ) is { IsGenericType: false };
                Utils.AssertThat( isNullable == _typeIsNullable[ t ] );
            }
        }
    }

    [ Benchmark ]
    public void IsValueType_IsGeneric_CompareOpenGeneric_NullableGetUnderlyingType_B( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            for ( int t = 0 ; t < _types.Length ; t++ ) {
                Type type = _types[ t ];
                bool isNullable = type is { IsValueType: true, IsGenericType: true }
                                  && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                                  && Nullable.GetUnderlyingType( type ) is { IsGenericType: false };
                Utils.AssertThat( isNullable == _typeIsNullable[ t ] );
            }
        }
    }
}

/*
 * | Method                 | Mean     | Error   | StdDev  | Gen0    | Gen1    | Allocated |
   |----------------------- |---------:|--------:|--------:|--------:|--------:|----------:|
   | DirectParse            | 172.7 us | 0.74 us | 0.69 us | 60.3027 | 23.9258 | 312.52 KB |
   | SecondHand             | 177.4 us | 1.17 us | 1.10 us | 60.3027 | 23.9258 | 312.52 KB |
   | SecondHandWithTryParse | 185.9 us | 0.45 us | 0.43 us | 60.3027 | 23.9258 | 312.52 KB |

 */

[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class ParseSecondHand {
    private const    int      _iterations = 10_000;
    private readonly string[] _strings    = new string[ _iterations ];

    [ GlobalSetup ]
    public void GlobalSetup( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            _strings[ i ] = i.ToString();
        }
    }

    [ Benchmark ]
    public void SecondHand( ) {
        object[] results = new object[ _iterations ];
        for ( int i = 0 ; i < _iterations ; i++ ) {
            results[ i ] = parse( _strings[ i ] );
        }
        Utils.AssertThat( ( int )results[ 99 ] == 99 );

        static object parse( string s ) {
            return Int32.Parse( s );
        }
    }

    [ Benchmark ]
    public void SecondHandWithTryParse( ) {
        object[] results = new object[ _iterations ];
        for ( int i = 0 ; i < _iterations ; i++ ) {
            results[ i ] = parse( _strings[ i ] );
        }
        Utils.AssertThat( ( int )results[ 99 ] == 99 );

        static object parse( string s ) {
            return Int32.TryParse( s, out int result ) ? result : 99;
        }
    }

    [ Benchmark ]
    public void DirectParse( ) {
        object[] results = new object[ _iterations ];
        for ( int i = 0 ; i < _iterations ; i++ ) {
            results[ i ] = Int32.Parse( _strings[ i ] );
        }
        Utils.AssertThat( ( int )results[ 99 ] == 99 );
    }
}

public static class Utils {
    [ StackTraceHidden ]
    [ DoesNotReturn ]
    public static void Throw( string message ) => throw new Exception( message );

    [ StackTraceHidden ]
    public static void AssertThat( [ DoesNotReturnIf( false ) ] bool condition, [ CallerArgumentExpression( nameof(condition) ) ] string argName = "unknown" ) {
        if ( !condition ) {
            Utils.Throw( $"{argName} was {condition}" );
        }
    }

    [ StackTraceHidden ]
    public static void ThrowIfNotEqual( int a, int b, [ CallerArgumentExpression( nameof(a) ) ] string aName = "a", [ CallerArgumentExpression( nameof(b) ) ] string bName = "b" ) {
        if ( a != b ) {
            Utils.Throw( $"{aName}={a} was not equal to {bName}={b}" );
        }
    }
}