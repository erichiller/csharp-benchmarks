using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.General;

/*
|                                            Method | Mean [us] | Error [us] | StdDev [us] | Ratio | RatioSD |   Gen0 | Allocated [B] | Alloc Ratio |
|-------------------------------------------------- |----------:|-----------:|------------:|------:|--------:|-------:|--------------:|------------:|
|                                          WorkOnly |  60.35 us |   0.029 us |    0.025 us |  1.00 |    0.00 |      - |             - |          NA |
|       IsNull_NullableHashSet_TupleKey_CapacitySet |  74.49 us |   0.067 us |    0.056 us |  1.23 |    0.00 | 0.1221 |         576 B |          NA |
| IsNull_NullableDictionary_TupleKey_CapacityNotSet |  75.04 us |   0.299 us |    0.279 us |  1.24 |    0.00 | 0.3662 |        2080 B |          NA |
|                          Bool_Dictionary_TupleKey |  75.17 us |   0.291 us |    0.272 us |  1.25 |    0.00 | 0.1221 |         776 B |          NA |
|                 Bool_HashSet_TupleKey_CapacitySet |  76.07 us |   0.926 us |    0.866 us |  1.26 |    0.01 | 0.1221 |         576 B |          NA |
|                  IsNull_NullableDictionary_IntKey | 122.78 us |   0.046 us |    0.038 us |  2.03 |    0.00 |      - |         592 B |          NA |
|         IsNull_NullableHashSet_IntKey_CapacitySet | 123.03 us |   0.349 us |    0.326 us |  2.04 |    0.01 |      - |         488 B |          NA |
|    IsNull_NullableDictionary_TupleKey_CapacitySet | 124.01 us |   0.448 us |    0.419 us |  2.06 |    0.01 |      - |         776 B |          NA |
| IsNull_NullableDictionary_TupleKey_Capacity100Set | 124.07 us |   0.366 us |    0.306 us |  2.06 |    0.00 | 0.4883 |        3128 B |          NA |
|    IsNull_NullableHashSet_TupleKey_CapacityNotSet | 124.18 us |   0.498 us |    0.442 us |  2.06 |    0.01 | 0.2441 |        1552 B |          NA |
|                  Bool_NullableDictionary_TupleKey | 124.54 us |   0.692 us |    0.614 us |  2.06 |    0.01 |      - |         776 B |          NA |
|                Bool_Dictionary_IntKey_CapacitySet | 196.54 us |   0.251 us |    0.223 us |  3.26 |    0.00 |      - |         592 B |          NA |
|             Bool_Dictionary_IntKey_CapacityNotSet | 198.92 us |   1.103 us |    1.031 us |  3.30 |    0.02 | 0.2441 |        1568 B |          NA |
 */

[ Config( typeof(BenchmarkConfig) ) ]
[ SuppressMessage( "Performance", "CA1822:Mark members as static" ) ]
public class KeyPresentBenchmarks {
    private const int _iterations             = 100_000;
    private const int _innerConditionalChecks = 20;

    [ Benchmark( Baseline = true ) ]
    public int WorkOnly( ) {
        int result = 0;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            result += i % 2;
        }
        return result;
    }

    [ Benchmark ]
    public int Bool_Dictionary_IntKey_CapacitySet( ) {
        int                   result                  = 0;
        Dictionary<int, bool> checkDict               = new (_innerConditionalChecks);
        bool                  boolSwitch              = false;
        int                   i                       = 0;
        int                   x                       = 0;
        int                   conditionalChecks1      = 0;
        int                   conditionalChecks2False = 0;
        int                   conditionalChecks2True  = 0;
        for ( ; i < _iterations ; i++ ) {
            x = i % _innerConditionalChecks;
            if ( boolSwitch == false ) {
                conditionalChecks1++;
                if ( checkDict.ContainsKey( x ) ) {
                    conditionalChecks2True++;
                    boolSwitch = true;
                } else {
                    conditionalChecks2False++;
                    checkDict[ x ] = true;
                }
            }
            result += i % 2;
        }
        if ( !( x == 19 && result == 50_000 && conditionalChecks1 == ( _innerConditionalChecks + 1 ) && conditionalChecks2True == 1 && conditionalChecks2False == _innerConditionalChecks ) ) {
            throw new Exception( $"i = {i} ; {nameof(x)} = {x} ; result = {result} ; checkDict = {checkDict} ; \n" +
                                 $"conditionalChecks1 = {conditionalChecks1} ; conditionalChecks2False = {conditionalChecks2False} ; conditionalChecks2True = {conditionalChecks2True}" );
        }
        return result;
    }

    [ Benchmark ]
    public int Bool_Dictionary_IntKey_CapacityNotSet( ) {
        int                   result                  = 0;
        Dictionary<int, bool> checkDict               = new ();
        bool                  boolSwitch              = false;
        int                   i                       = 0;
        int                   x                       = 0;
        int                   conditionalChecks1      = 0;
        int                   conditionalChecks2False = 0;
        int                   conditionalChecks2True  = 0;
        for ( ; i < _iterations ; i++ ) {
            x = i % _innerConditionalChecks;
            if ( boolSwitch == false ) {
                conditionalChecks1++;
                if ( checkDict.ContainsKey( x ) ) {
                    conditionalChecks2True++;
                    boolSwitch = true;
                } else {
                    conditionalChecks2False++;
                    checkDict[ x ] = true;
                }
            }
            result += i % 2;
        }
        if ( !( x == 19 && result == 50_000 && conditionalChecks1 == ( _innerConditionalChecks + 1 ) && conditionalChecks2True == 1 && conditionalChecks2False == _innerConditionalChecks ) ) {
            throw new Exception( $"i = {i} ; {nameof(x)} = {x} ; result = {result} ; checkDict = {checkDict} ; \n" +
                                 $"conditionalChecks1 = {conditionalChecks1} ; conditionalChecks2False = {conditionalChecks2False} ; conditionalChecks2True = {conditionalChecks2True}" );
        }
        return result;
    }

    [ Benchmark ]
    public int IsNull_NullableDictionary_IntKey( ) {
        int                    result                  = 0;
        Dictionary<int, bool>? checkDict               = new (_innerConditionalChecks);
        int                    i                       = 0;
        int                    x                       = 0;
        int                    conditionalChecks1      = 0;
        int                    conditionalChecks2False = 0;
        int                    conditionalChecks2True  = 0;
        for ( ; i < _iterations ; i++ ) {
            if ( checkDict is not null ) {
                conditionalChecks1++;
                x = i % _innerConditionalChecks;
                if ( checkDict.ContainsKey( x ) ) {
                    conditionalChecks2True++;
                    checkDict = null;
                } else {
                    conditionalChecks2False++;
                    checkDict[ x ] = true;
                }
            }
            result += i % 2;
        }
        if ( !( x == 0 && result == 50_000 && conditionalChecks1 == ( _innerConditionalChecks + 1 ) && conditionalChecks2True == 1 && conditionalChecks2False == _innerConditionalChecks ) ) {
            throw new Exception( $"i = {i} ; j = {x} ; result = {result} ; checkDict = {checkDict} ; \n" +
                                 $"conditionalChecks1 = {conditionalChecks1} ; conditionalChecks2False = {conditionalChecks2False} ; conditionalChecks2True = {conditionalChecks2True}" );
        }
        return result;
    }

    [ Benchmark ]
    public int Bool_Dictionary_TupleKey( ) {
        int                          result                  = 0;
        bool                         boolSwitch              = false;
        Dictionary<(int, int), bool> checkDict               = new (_innerConditionalChecks);
        int                          innerConditionalChecks  = _innerConditionalChecks / 2; // divide by two since it is going to be innerConditionalChecks * 2 (addition of j)
        int                          i                       = 0;
        int                          x                       = 0;
        int                          y                       = 0;
        int                          conditionalChecks1      = 0;
        int                          conditionalChecks2False = 0;
        int                          conditionalChecks2True  = 0;
        for ( ; i < _iterations ; i++ ) {
            if ( boolSwitch == false ) {
                conditionalChecks1++;
                x = i < 10 ? 1 : 2;
                y = i % innerConditionalChecks;
                if ( checkDict?.ContainsKey( ( x, y ) ) ?? true ) {
                    conditionalChecks2True++;
                    boolSwitch = true;
                } else {
                    conditionalChecks2False++;
                    checkDict[ ( x, y ) ] = true;
                }
            }
            result += i % 2;
        }
        if ( !( x == 2 && y == 0 && result == 50_000 && conditionalChecks1 == ( _innerConditionalChecks + 1 ) && conditionalChecks2True == 1 && conditionalChecks2False == _innerConditionalChecks ) ) {
            throw new Exception( $"i = {i} ; {nameof(x)} = {x} ; {nameof(y)} = {y} ; result = {result} ; checkDict = {checkDict} ; \n" +
                                 $"conditionalChecks1 = {conditionalChecks1} ; conditionalChecks2False = {conditionalChecks2False} ; conditionalChecks2True = {conditionalChecks2True}" );
        }
        return result;
    }

    [ Benchmark ]
    public int Bool_NullableDictionary_TupleKey( ) {
        int                           result                  = 0;
        bool                          boolSwitch              = false;
        Dictionary<(int, int), bool>? checkDict               = new (_innerConditionalChecks);
        int                           innerConditionalChecks  = _innerConditionalChecks / 2; // divide by two since it is going to be innerConditionalChecks * 2 (addition of j)
        int                           i                       = 0;
        int                           x                       = 0;
        int                           y                       = 0;
        int                           conditionalChecks1      = 0;
        int                           conditionalChecks2False = 0;
        int                           conditionalChecks2True  = 0;
        for ( ; i < _iterations ; i++ ) {
            if ( boolSwitch == false ) {
                conditionalChecks1++;
                x = i < 10 ? 1 : 2;
                y = i % innerConditionalChecks;
                if ( checkDict?.ContainsKey( ( x, y ) ) ?? true ) {
                    conditionalChecks2True++;
                    checkDict  = null;
                    boolSwitch = true;
                } else {
                    conditionalChecks2False++;
                    checkDict[ ( x, y ) ] = true;
                }
            }
            result += i % 2;
        }
        if ( !( x == 2 && y == 0 && result == 50_000 && conditionalChecks1 == ( _innerConditionalChecks + 1 ) && conditionalChecks2True == 1 && conditionalChecks2False == _innerConditionalChecks ) ) {
            throw new Exception( $"i = {i} ; {nameof(x)} = {x} ; {nameof(y)} = {y} ; result = {result} ; checkDict = {checkDict} ; \n" +
                                 $"conditionalChecks1 = {conditionalChecks1} ; conditionalChecks2False = {conditionalChecks2False} ; conditionalChecks2True = {conditionalChecks2True}" );
        }
        return result;
    }

    [ Benchmark ]
    public int IsNull_NullableDictionary_TupleKey_CapacitySet( ) {
        int                           result                  = 0;
        Dictionary<(int, int), bool>? checkDict               = new (_innerConditionalChecks);
        int                           innerConditionalChecks  = _innerConditionalChecks / 2; // divide by two since it is going to be innerConditionalChecks * 2 (addition of j)
        int                           i                       = 0;
        int                           x                       = 0;
        int                           y                       = 0;
        int                           conditionalChecks1      = 0;
        int                           conditionalChecks2False = 0;
        int                           conditionalChecks2True  = 0;
        for ( ; i < _iterations ; i++ ) {
            if ( checkDict is not null ) {
                conditionalChecks1++;
                x = i < 10 ? 1 : 2;
                y = i % innerConditionalChecks;
                if ( checkDict.ContainsKey( ( x, y ) ) ) {
                    conditionalChecks2True++;
                    checkDict = null;
                } else {
                    conditionalChecks2False++;
                    checkDict[ ( x, y ) ] = true;
                }
            }
            result += i % 2;
        }
        if ( !( x == 2 && y == 0 && result == 50_000 && conditionalChecks1 == ( _innerConditionalChecks + 1 ) && conditionalChecks2True == 1 && conditionalChecks2False == _innerConditionalChecks ) ) {
            throw new Exception( $"i = {i} ; {nameof(x)} = {x} ; {nameof(y)} = {y} ; result = {result} ; checkDict = {checkDict} ; \n" +
                                 $"conditionalChecks1 = {conditionalChecks1} ; conditionalChecks2False = {conditionalChecks2False} ; conditionalChecks2True = {conditionalChecks2True}" );
        }
        return result;
    }

    [ Benchmark ]
    public int IsNull_NullableDictionary_TupleKey_CapacityNotSet( ) {
        int                           result                  = 0;
        Dictionary<(int, int), bool>? checkDict               = new ();
        int                           innerConditionalChecks  = _innerConditionalChecks / 2; // divide by two since it is going to be innerConditionalChecks * 2 (addition of j)
        int                           i                       = 0;
        int                           x                       = 0;
        int                           y                       = 0;
        int                           conditionalChecks1      = 0;
        int                           conditionalChecks2False = 0;
        int                           conditionalChecks2True  = 0;
        for ( ; i < _iterations ; i++ ) {
            if ( checkDict is not null ) {
                conditionalChecks1++;
                x = i < 10 ? 1 : 2;
                y = i % innerConditionalChecks;
                if ( checkDict.ContainsKey( ( x, y ) ) ) {
                    conditionalChecks2True++;
                    checkDict = null;
                } else {
                    conditionalChecks2False++;
                    checkDict[ ( x, y ) ] = true;
                }
            }
            result += i % 2;
        }
        if ( !( x == 2 && y == 0 && result == 50_000 && conditionalChecks1 == ( _innerConditionalChecks + 1 ) && conditionalChecks2True == 1 && conditionalChecks2False == _innerConditionalChecks ) ) {
            throw new Exception( $"i = {i} ; {nameof(x)} = {x} ; {nameof(y)} = {y} ; result = {result} ; checkDict = {checkDict} ; \n" +
                                 $"conditionalChecks1 = {conditionalChecks1} ; conditionalChecks2False = {conditionalChecks2False} ; conditionalChecks2True = {conditionalChecks2True}" );
        }
        return result;
    }

    [ Benchmark ]
    public int IsNull_NullableDictionary_TupleKey_Capacity100Set( ) {
        int                           result                  = 0;
        Dictionary<(int, int), bool>? checkDict               = new (100);
        int                           innerConditionalChecks  = _innerConditionalChecks / 2; // divide by two since it is going to be innerConditionalChecks * 2 (addition of j)
        int                           i                       = 0;
        int                           x                       = 0;
        int                           y                       = 0;
        int                           conditionalChecks1      = 0;
        int                           conditionalChecks2False = 0;
        int                           conditionalChecks2True  = 0;
        for ( ; i < _iterations ; i++ ) {
            if ( checkDict is not null ) {
                conditionalChecks1++;
                x = i < 10 ? 1 : 2;
                y = i % innerConditionalChecks;
                if ( checkDict.ContainsKey( ( x, y ) ) ) {
                    conditionalChecks2True++;
                    checkDict = null;
                } else {
                    conditionalChecks2False++;
                    checkDict[ ( x, y ) ] = true;
                }
            }
            result += i % 2;
        }
        if ( !( x == 2 && y == 0 && result == 50_000 && conditionalChecks1 == ( _innerConditionalChecks + 1 ) && conditionalChecks2True == 1 && conditionalChecks2False == _innerConditionalChecks ) ) {
            throw new Exception( $"i = {i} ; {nameof(x)} = {x} ; {nameof(y)} = {y} ; result = {result} ; checkDict = {checkDict} ; \n" +
                                 $"conditionalChecks1 = {conditionalChecks1} ; conditionalChecks2False = {conditionalChecks2False} ; conditionalChecks2True = {conditionalChecks2True}" );
        }
        return result;
    }

    [ Benchmark ]
    public int Bool_HashSet_TupleKey_CapacitySet( ) {
        int                 result                  = 0;
        HashSet<(int, int)> checkDict               = new (_innerConditionalChecks);
        bool                doInner                 = true;
        int                 innerConditionalChecks  = _innerConditionalChecks / 2; // divide by two since it is going to be innerConditionalChecks * 2 (addition of j)
        int                 i                       = 0;
        int                 x                       = 0;
        int                 y                       = 0;
        int                 conditionalChecks1      = 0;
        int                 conditionalChecks2False = 0;
        int                 conditionalChecks2True  = 0;
        for ( ; i < _iterations ; i++ ) {
            if ( doInner ) {
                conditionalChecks1++;
                x = i < 10 ? 1 : 2;
                y = i % innerConditionalChecks;
                if ( checkDict.Contains( ( x, y ) ) ) {
                    conditionalChecks2True++;
                    doInner = false;
                } else {
                    conditionalChecks2False++;
                    checkDict.Add( ( x, y ) );
                }
            }
            result += i % 2;
        }
        if ( !( x == 2 && y == 0 && result == 50_000 && conditionalChecks1 == ( _innerConditionalChecks + 1 ) && conditionalChecks2True == 1 && conditionalChecks2False == _innerConditionalChecks ) ) {
            throw new Exception( $"i = {i} ; {nameof(x)} = {x} ; {nameof(y)} = {y} ; result = {result} ; checkDict = {checkDict} ; \n" +
                                 $"conditionalChecks1 = {conditionalChecks1} ; conditionalChecks2False = {conditionalChecks2False} ; conditionalChecks2True = {conditionalChecks2True}" );
        }
        return result;
    }

    [ Benchmark ]
    public int IsNull_NullableHashSet_IntKey_CapacitySet( ) {
        int           result                  = 0;
        HashSet<int>? checkHashSet            = new (_innerConditionalChecks);
        int           i                       = 0;
        int           x                       = 0;
        int           conditionalChecks1      = 0;
        int           conditionalChecks2False = 0;
        int           conditionalChecks2True  = 0;
        for ( ; i < _iterations ; i++ ) {
            if ( checkHashSet is not null ) {
                conditionalChecks1++;
                x = i % _innerConditionalChecks;
                if ( checkHashSet.Contains( x ) ) {
                    conditionalChecks2True++;
                    checkHashSet = null;
                } else {
                    conditionalChecks2False++;
                    checkHashSet.Add( x );
                }
            }
            result += i % 2;
        }
        if ( !( x == 0 && result == 50_000 && conditionalChecks1 == ( _innerConditionalChecks + 1 ) && conditionalChecks2True == 1 && conditionalChecks2False == _innerConditionalChecks ) ) {
            throw new Exception( $"i = {i} ; {nameof(x)} = {x} ; result = {result} ; checkDict = {checkHashSet} ; \n" +
                                 $"conditionalChecks1 = {conditionalChecks1} ; conditionalChecks2False = {conditionalChecks2False} ; conditionalChecks2True = {conditionalChecks2True}" );
        }
        return result;
    }

    [ Benchmark ]
    public int IsNull_NullableHashSet_TupleKey_CapacitySet( ) {
        int                  result                  = 0;
        HashSet<(int, int)>? checkDict               = new (_innerConditionalChecks);
        int                  innerConditionalChecks  = _innerConditionalChecks / 2; // divide by two since it is going to be innerConditionalChecks * 2 (addition of j)
        int                  i                       = 0;
        int                  x                       = 0;
        int                  y                       = 0;
        int                  conditionalChecks1      = 0;
        int                  conditionalChecks2False = 0;
        int                  conditionalChecks2True  = 0;
        for ( ; i < _iterations ; i++ ) {
            if ( checkDict is not null ) {
                conditionalChecks1++;
                x = i < 10 ? 1 : 2;
                y = i % innerConditionalChecks;
                if ( checkDict.Contains( ( x, y ) ) ) {
                    conditionalChecks2True++;
                    checkDict = null;
                } else {
                    conditionalChecks2False++;
                    checkDict.Add( ( x, y ) );
                }
            }
            result += i % 2;
        }
        if ( !( x == 2 && y == 0 && result == 50_000 && conditionalChecks1 == ( _innerConditionalChecks + 1 ) && conditionalChecks2True == 1 && conditionalChecks2False == _innerConditionalChecks ) ) {
            throw new Exception( $"i = {i} ; {nameof(x)} = {x} ; {nameof(y)} = {y} ; result = {result} ; checkDict = {checkDict} ; \n" +
                                 $"conditionalChecks1 = {conditionalChecks1} ; conditionalChecks2False = {conditionalChecks2False} ; conditionalChecks2True = {conditionalChecks2True}" );
        }
        return result;
    }

    [ Benchmark ]
    public int IsNull_NullableHashSet_TupleKey_CapacityNotSet( ) {
        int                  result                  = 0;
        HashSet<(int, int)>? checkDict               = new ();
        int                  innerConditionalChecks  = _innerConditionalChecks / 2; // divide by two since it is going to be innerConditionalChecks * 2 (addition of j)
        int                  i                       = 0;
        int                  x                       = 0;
        int                  y                       = 0;
        int                  conditionalChecks1      = 0;
        int                  conditionalChecks2False = 0;
        int                  conditionalChecks2True  = 0;
        for ( ; i < _iterations ; i++ ) {
            if ( checkDict is not null ) {
                conditionalChecks1++;
                x = i < 10 ? 1 : 2;
                y = i % innerConditionalChecks;
                if ( checkDict.Contains( ( x, y ) ) ) {
                    conditionalChecks2True++;
                    checkDict = null;
                } else {
                    conditionalChecks2False++;
                    checkDict.Add( ( x, y ) );
                }
            }
            result += i % 2;
        }
        if ( !( x == 2 && y == 0 && result == 50_000 && conditionalChecks1 == ( _innerConditionalChecks + 1 ) && conditionalChecks2True == 1 && conditionalChecks2False == _innerConditionalChecks ) ) {
            throw new Exception( $"i = {i} ; {nameof(x)} = {x} ; {nameof(y)} = {y} ; result = {result} ; checkDict = {checkDict} ; \n" +
                                 $"conditionalChecks1 = {conditionalChecks1} ; conditionalChecks2False = {conditionalChecks2False} ; conditionalChecks2True = {conditionalChecks2True}" );
        }
        return result;
    }
}