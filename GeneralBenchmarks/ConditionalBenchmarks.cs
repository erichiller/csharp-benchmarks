using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Benchmarks.General;

/*
 *
   | Method                   | Mean      | Error     | StdDev    | Ratio | Gen0    | Allocated | Alloc Ratio |
   |------------------------- |----------:|----------:|----------:|------:|--------:|----------:|------------:|
   | SwitchStatement          |  8.594 ms | 0.0258 ms | 0.0241 ms |  0.99 | 15.6250 | 104.19 KB |        1.00 |
   | NestedSwitchExpression   |  8.609 ms | 0.0128 ms | 0.0120 ms |  0.99 | 15.6250 | 104.19 KB |        1.00 |
   | SwitchExpression         |  8.640 ms | 0.0103 ms | 0.0086 ms |  0.99 | 15.6250 | 104.19 KB |        1.00 |
   | SwitchExpression_NoCache |  8.671 ms | 0.0378 ms | 0.0353 ms |  1.00 | 15.6250 | 104.19 KB |        1.00 |
   | NestedIf                 |  8.697 ms | 0.0370 ms | 0.0346 ms |  1.00 | 15.6250 | 104.19 KB |        1.00 |
   | SimpleIf                 | 16.850 ms | 0.0314 ms | 0.0278 ms |  1.94 |       - |  104.2 KB |        1.00 |

 */

[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class ConditionalBenchmarks {
    // public const int ITERATIONS = 10_000;
    public const int ITERATIONS = 10;

    // enum TestEnum {
    //     Value0, Value1, Value2, Value3, Value4, Value5, Value6, Value7, Value8, Value9
    // }

    class TestValues {
        public bool IsX { get; set; } = true;

        public bool IsY {
            get {
                Thread.Sleep( TimeSpan.FromMicroseconds( 25 ) );
                return true;
            }
        }

        public bool IsZ { get; set; } = true;
        // public int  X   { get; set; } = 7;
        // public int  Y   { get; set; } = 8;
        public int ValueA { get; set; } = 9;
    }

    private static readonly TestValues _testValues  = new TestValues();
    private                 int        _incrementor = -1;

    private readonly Type[] _possibleNullableTypes = [ typeof(int?), typeof(string), typeof(double?) ];

    [ GlobalSetup ]
    public void GlobalSetup( ) {
        _incrementor = -1;
    }

    private bool doSomething( ) {
        // Thread.Sleep( TimeSpan.FromMicroseconds( 25 ) );
        _incrementor++;
        return Nullable.GetUnderlyingType( _possibleNullableTypes[ _incrementor % 3 ] ) is { }
               || _possibleNullableTypes[ _incrementor % 3 ] == typeof(string);
    }

    private bool cacheDoSomething( [ NotNull ] ref bool? doSomethingResult ) {
        if ( doSomethingResult is null ) {
            doSomethingResult = doSomething();
        }
        return doSomethingResult.Value;
    }

    // private (bool cached, Type? cachedTypeResult) cacheNullableTupleParam( [NotNull]ref (bool cached, Type? cachedTypeResult)? cachedState) {
    //     if ( cachedState is { } tpl ) {
    //         return tpl;
    //     }
    //     return cachedState = (true, typeof(int?));
    // }


    [ Benchmark( Baseline = true ) ]
    public void NestedIf( ) {
        for ( int i = 0 ; i < ITERATIONS ; i++ ) {
            bool result  = false;
            bool result2 = false;
            ( _testValues.IsX, _testValues.ValueA ) = ( i % 2 == 0, i % 7 );
            if ( _testValues.IsX ) {
                if ( _testValues.IsY ) {
                    if ( _testValues.IsZ && doSomething() ) {
                        if ( _testValues.ValueA < 3 ) {
                            ( result, result2 ) = ( true, false );
                        }
                        if ( _testValues.ValueA == 3 ) {
                            ( result, result2 ) = ( false, true );
                        }
                        if ( _testValues.ValueA > 3 ) {
                            ( result, result2 ) = ( true, true );
                        }
                    }
                }
            } else {
                if ( _testValues.IsY ) {
                    ( result, result2 ) = ( true, false );
                }
            }
            if ( result || result2 ) {
                continue;
            }
            Throw();
        }
    }

    [ Benchmark ]
    public void SimpleIf( ) {
        for ( int i = 0 ; i < ITERATIONS ; i++ ) {
            bool  result            = false;
            bool  result2           = true;
            bool? doSomethingResult = null;
            ( _testValues.IsX, _testValues.ValueA ) = ( i % 2 == 0, i % 7 );
            if ( _testValues.IsX && _testValues.IsY && _testValues.IsZ && _testValues.ValueA < 3 && cacheDoSomething( ref doSomethingResult ) ) {
                ( result, result2 ) = ( true, false );
            }
            if ( _testValues.IsX && _testValues.IsY && _testValues.IsZ && _testValues.ValueA == 3 && cacheDoSomething( ref doSomethingResult ) ) {
                ( result, result2 ) = ( false, true );
            }
            if ( _testValues.IsX && _testValues.IsY && _testValues.IsZ && _testValues.ValueA > 3 && cacheDoSomething( ref doSomethingResult ) ) {
                ( result, result2 ) = ( true, true );
            }
            if ( !_testValues.IsX && _testValues.IsY ) {
                ( result, result2 ) = ( true, false );
            }
            if ( result || result2 ) {
                continue;
            }
            Throw();
        }
    }

    [ Benchmark ]
    public void SwitchExpression( ) {
        for ( int i = 0 ; i < ITERATIONS ; i++ ) {
            ( _testValues.IsX, _testValues.ValueA ) = ( i % 2 == 0, i % 7 );
            bool? doSomethingResult = null;
            ( bool result, bool result2 ) =
                _testValues switch {
                    { IsX: true, IsY : true, IsZ: true, ValueA: < 3 } when cacheDoSomething( ref doSomethingResult ) => ( true, false ),
                    { IsX: true, IsY : true, IsZ: true, ValueA: 3 } when cacheDoSomething( ref doSomethingResult )   => ( false, true ),
                    { IsX: true, IsY : true, IsZ: true, ValueA: > 3 } when cacheDoSomething( ref doSomethingResult ) => ( true, true ),
                    { IsX: false, IsY: true }                                                                        => ( true, false ),
                    _                                                                                                => ( false, false )
                };
            if ( result || result2 ) {
                continue;
            }
            Throw();
        }
    }

    [ Benchmark ]
    public void SwitchExpression_NoCache( ) {
        for ( int i = 0 ; i < ITERATIONS ; i++ ) {
            ( _testValues.IsX, _testValues.ValueA ) = ( i % 2 == 0, i % 7 );
            ( bool result, bool result2 ) =
                _testValues switch {
                    { IsX: true, IsY : true, IsZ: true, ValueA: < 3 } when doSomething() => ( true, false ),
                    { IsX: true, IsY : true, IsZ: true, ValueA: 3 } when doSomething()   => ( false, true ),
                    { IsX: true, IsY : true, IsZ: true, ValueA: > 3 } when doSomething() => ( true, true ),
                    { IsX: false, IsY: true }                                            => ( true, false ),
                    _                                                                    => ( false, false )
                };
            if ( result || result2 ) {
                continue;
            }
            Throw();
        }
    }

    [ Benchmark ]
    public void NestedSwitchExpression( ) {
        for ( int i = 0 ; i < ITERATIONS ; i++ ) {
            ( _testValues.IsX, _testValues.ValueA ) = ( i % 2 == 0, i % 7 );
            ( bool result, bool result2 ) =
                _testValues switch {
                    { IsX: true, IsY : true, IsZ: true } when doSomething() =>
                        _testValues switch {
                            { ValueA: < 3 } => ( true, false ),
                            { ValueA: 3 }   => ( false, true ),
                            { ValueA: > 3 } => ( true, true ),
                        },
                    { IsX: false, IsY: true } => ( true, false ),
                    _                         => ( false, false )
                };
            if ( result || result2 ) {
                continue;
            }
            Throw();
        }
    }

    [ Benchmark ]
    public void SwitchStatement( ) {
        for ( int i = 0 ; i < ITERATIONS ; i++ ) {
            ( _testValues.IsX, _testValues.ValueA ) = ( i % 2 == 0, i % 7 );
            bool? doSomethingResult = null;
            bool  result;
            bool  result2;
            switch ( _testValues ) {
                case { IsX: true, IsY : true, IsZ: true, ValueA: < 3 } when cacheDoSomething( ref doSomethingResult ):
                    ( result, result2 ) = ( true, false );
                    break;
                case { IsX: true, IsY : true, IsZ: true, ValueA: 3 } when cacheDoSomething( ref doSomethingResult ):
                    ( result, result2 ) = ( false, true );
                    break;
                case { IsX: true, IsY : true, IsZ: true, ValueA: > 3 } when cacheDoSomething( ref doSomethingResult ):
                    ( result, result2 ) = ( true, true );
                    break;
                case { IsX: false, IsY: true }:
                    ( result, result2 ) = ( true, false );
                    break;
                default:
                    ( result, result2 ) = ( false, false );
                    break;
            }
            if ( result || result2 ) {
                continue;
            }
            Throw();
        }
    }


    [ DoesNotReturn ]
    [ StackTraceHidden ]
    public void Throw( ) => throw new Exception( "was false" );
}

[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class CachedFunctionCallBenchmarks {
    public const           int    ITERATIONS    = 10;
    public static readonly Type[] Types         = [ typeof(int), typeof(long), typeof(string), typeof(bool), typeof(bool?) ];
    public static readonly int    TypeArraySize = Types.Length;

    private Type? cacheTwoParam( in Type type, ref bool cached, ref Type? cachedTypeResult ) {
        if ( cached ) {
            return cachedTypeResult;
        }
        cached = true;
        return cachedTypeResult = type;
    }

    private (bool cached, Type? cachedTypeResult) cacheTupleParam( in Type type, ref (bool cached, Type? cachedTypeResult) cachedState ) {
        if ( cachedState.cached ) {
            return cachedState;
        }
        return cachedState = ( true, type );
    }

    private Type? cacheTupleParamReturnType( in Type type, ref (bool cached, Type? cachedTypeResult) cachedState ) {
        if ( cachedState.cached ) {
            return cachedState.cachedTypeResult;
        }
        cachedState = ( true, type );
        return cachedState.cachedTypeResult;
    }

    private Type? cacheStructParamReturnType( in Type type, ref CacheState cachedState ) {
        if ( cachedState.cached ) {
            return cachedState.cachedTypeResult;
        }
        cachedState.cached           = true;
        cachedState.cachedTypeResult = type;
        return cachedState.cachedTypeResult;
    }

    private Type? cacheStructParamReturnType2( in Type type, ref CacheState cachedState ) {
        if ( cachedState.cached ) {
            return cachedState.cachedTypeResult;
        }
        cachedState.cached = true;
        return cachedState.cachedTypeResult = type;
    }

    private record struct CacheState( bool cached = false, Type? cachedTypeResult = null );

    /*
     *
     */


    [ DoesNotReturn ]
    [ StackTraceHidden ]
    public void Throw( ) => throw new Exception( "was false" );

    /*
     *
     */

    [ Benchmark ]
    public void TwoParamCache( ) {
        for ( int i = 0 ; i < ITERATIONS ; i++ ) {
            for ( int t = 0 ; t < TypeArraySize ; t++ ) {
                bool  cached           = false;
                Type? cachedTypeResult = null;
                bool  x                = cacheTwoParam( Types[ t ], ref cached, ref cachedTypeResult ) == Types[ t ];
                bool  y                = cacheTwoParam( Types[ t ], ref cached, ref cachedTypeResult ) == Types[ t ];
                if ( x && y ) { continue; }
                Throw();
            }
        }
    }

    [ Benchmark ]
    public void TupleCache( ) {
        for ( int i = 0 ; i < ITERATIONS ; i++ ) {
            for ( int t = 0 ; t < TypeArraySize ; t++ ) {
                (bool cached, Type? cachedTypeResult) cacheState = ( false, null );
                bool                                  x          = cacheTupleParam( Types[ t ], ref cacheState ).cachedTypeResult == Types[ t ];
                bool                                  y          = cacheTupleParam( Types[ t ], ref cacheState ).cachedTypeResult == Types[ t ];
                if ( x && y ) { continue; }
                Throw();
            }
        }
    }

    [ Benchmark ]
    public void TupleCache_ReturnTypeOnly( ) {
        for ( int i = 0 ; i < ITERATIONS ; i++ ) {
            for ( int t = 0 ; t < TypeArraySize ; t++ ) {
                (bool cached, Type? cachedTypeResult) cacheState = ( false, null );
                bool                                  x          = cacheTupleParamReturnType( Types[ t ], ref cacheState ) == Types[ t ];
                bool                                  y          = cacheTupleParamReturnType( Types[ t ], ref cacheState ) == Types[ t ];
                if ( x && y ) { continue; }
                Throw();
            }
        }
    }

    [ Benchmark ]
    public void StructCache_ReturnTypeOnly( ) {
        for ( int i = 0 ; i < ITERATIONS ; i++ ) {
            for ( int t = 0 ; t < TypeArraySize ; t++ ) {
                CacheState cacheState = new CacheState();
                bool       x          = cacheStructParamReturnType( Types[ t ], ref cacheState ) == Types[ t ];
                bool       y          = cacheStructParamReturnType( Types[ t ], ref cacheState ) == Types[ t ];
                if ( x && y ) { continue; }
                Throw();
            }
        }
    }

    [ Benchmark ]
    public void StructCache_ReturnTypeOnly2( ) {
        for ( int i = 0 ; i < ITERATIONS ; i++ ) {
            for ( int t = 0 ; t < TypeArraySize ; t++ ) {
                CacheState cacheState = new CacheState();
                bool       x          = cacheStructParamReturnType2( Types[ t ], ref cacheState ) == Types[ t ];
                bool       y          = cacheStructParamReturnType2( Types[ t ], ref cacheState ) == Types[ t ];
                if ( x && y ) { continue; }
                Throw();
            }
        }
    }
}

/*
 * | Method          | Mean     | Error     | StdDev    | Ratio | RatioSD | Allocated | Alloc Ratio |
   |---------------- |---------:|----------:|----------:|------:|--------:|----------:|------------:|
   | ThreeInts       | 2.204 us | 0.0011 us | 0.0009 us |  1.00 |    0.02 |         - |          NA |
   | NullableBoolean | 2.210 us | 0.0423 us | 0.0453 us |  1.00 |    0.03 |         - |          NA |
   | ThreeStateEnum  | 2.616 us | 0.0007 us | 0.0007 us |  1.18 |    0.02 |         - |          NA |

 */
[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class NullableBooleanVsEnumConditional {
    private static int _iterations = 1000;

    private enum ThreeStates {
        One,
        Two,
        Three
    }

    private bool?[]       _nullableBooleans = [ true, null, false ];
    private ThreeStates[] _threeStates      = [ ThreeStates.One, ThreeStates.Two, ThreeStates.Three ];
    private int[]         _threeInts        = [ 3, 11, 22 ];

    [ Benchmark( Baseline = true ) ]
    public void NullableBoolean( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            var m = i % 3;
            Utils.AssertThat( _nullableBooleans[ m ] switch {
                                  true  => 0,
                                  null  => 1,
                                  false => 2
                              } == m );
        }
    }

    [ Benchmark ]
    public void ThreeStateEnum( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            var m = i % 3;
            Utils.AssertThat( _threeStates[ m ] switch {
                                  ThreeStates.One   => 0,
                                  ThreeStates.Two   => 1,
                                  ThreeStates.Three => 2,
                                  _                 => 4 // without this it goes much slower!!
                              } == m );
        }
    }

    [ Benchmark ]
    public void ThreeInts( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            var m = i % 3;
            Utils.AssertThat( _threeInts[ m ] switch {
                                  3  => 0,
                                  11 => 1,
                                  22 => 2,
                                  _  => 4 // without this it goes much slower!!
                              } == m );
        }
    }
}

/*
 *
 * iterations = 1
   | Method        | Mean     | Error    | StdDev   | Ratio |
   |-------------- |---------:|---------:|---------:|------:|
   | IfPerType     | 15.01 ns | 0.006 ns | 0.005 ns |  1.00 |
   | IfPerTypeCode | 20.28 ns | 0.012 ns | 0.010 ns |  1.35 |
   | IfPerInstance | 14.28 ns | 0.065 ns | 0.060 ns |  0.95 |
   
   iterations = 1,000
   | Method        | Mean     | Error    | StdDev   | Ratio |
   |-------------- |---------:|---------:|---------:|------:|
   | IfPerType     | 12.74 us | 0.005 us | 0.004 us |  1.00 |
   | IfPerTypeCode | 19.94 us | 0.038 us | 0.034 us |  1.57 |
   | IfPerInstance | 14.17 us | 0.019 us | 0.016 us |  1.11 |
   

 */

public class ConditionalOnTypeBenchmarks {
    private readonly (Type type, Object instance, TypeCode expected)[] _typeInstances = [
        ( typeof(string), "", TypeCode.String ),
        ( typeof(int), 3, TypeCode.Int32 ),
        ( typeof(double), ( double )3.0f, TypeCode.Double ),
        ( typeof(float), ( float )3, TypeCode.Single ),
        ( typeof(decimal), ( decimal )3.0, TypeCode.Decimal ),
    ];

    private const int iterations = 1_000;

    [ Benchmark( Baseline = true ) ]
    public void IfPerType( ) {
        for ( int i = 0 ; i < iterations ; i++ ) {
            for ( int t = 0 ; t < _typeInstances.Length ; t++ ) {
                var tpl = _typeInstances[ t ];
                if ( tpl.type == typeof(string) ) {
                    Utils.AssertThat( tpl.expected == TypeCode.String );
                    continue;
                }
                if ( tpl.type == typeof(int) || tpl.type == typeof(int?) ) {
                    Utils.AssertThat( tpl.expected == TypeCode.Int32 );
                    continue;
                }
                if ( tpl.type == typeof(double) || tpl.type == typeof(double?) ) {
                    Utils.AssertThat( tpl.expected == TypeCode.Double );
                    continue;
                }
                if ( tpl.type == typeof(float) || tpl.type == typeof(float?) ) {
                    Utils.AssertThat( tpl.expected == TypeCode.Single );
                    continue;
                }
                if ( tpl.type == typeof(decimal) || tpl.type == typeof(decimal?) ) {
                    Utils.AssertThat( tpl.expected == TypeCode.Decimal );
                    continue;
                }
                throw new Exception();
            }
        }
    }

    [ Benchmark ]
    public void IfPerTypeCode( ) {
        for ( int i = 0 ; i < iterations ; i++ ) {
            for ( int t = 0 ; t < _typeInstances.Length ; t++ ) {
                var tpl = _typeInstances[ t ];
                Utils.AssertThat( Type.GetTypeCode( tpl.type ) switch {
                                      TypeCode.String  => TypeCode.String,
                                      TypeCode.Int32   => TypeCode.Int32,
                                      TypeCode.Double  => TypeCode.Double,
                                      TypeCode.Single  => TypeCode.Single,
                                      TypeCode.Decimal => TypeCode.Decimal,
                                      _                => throw new Exception()
                                  } == tpl.expected
                );
            }
        }
    }

    [ Benchmark ]
    public void IfPerInstance( ) {
        for ( int i = 0 ; i < iterations ; i++ ) {
            for ( int t = 0 ; t < _typeInstances.Length ; t++ ) {
                var tpl = _typeInstances[ t ];
                Utils.AssertThat( tpl.instance switch {
                                      string  => TypeCode.String,
                                      int     => TypeCode.Int32,
                                      double  => TypeCode.Double,
                                      float   => TypeCode.Single,
                                      decimal => TypeCode.Decimal,
                                      _       => throw new Exception()
                                  } == tpl.expected
                );
            }
        }
    }
}