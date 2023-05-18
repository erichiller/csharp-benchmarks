using System;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.General;

/* 100,000
 
Instance
============
|                        Method | Iterations | Mean [ms] | Error [ms] | StdDev [ms] | Ratio | RatioSD |     Gen0 | Allocated [B] | Alloc Ratio |
|------------------------------ |----------- |----------:|-----------:|------------:|------:|--------:|---------:|--------------:|------------:|
|              BaseClass_Direct |     100000 |  1.328 ms |  0.0128 ms |   0.0120 ms |  0.98 |    0.03 | 679.6875 |     3199682 B |        1.00 |
|        InheritingClass_AsBase |     100000 |  1.332 ms |  0.0101 ms |   0.0084 ms |  0.99 |    0.03 | 679.6875 |     3199682 B |        1.00 |
|        StandAloneClass_Direct |     100000 |  1.338 ms |  0.0250 ms |   0.0317 ms |  1.00 |    0.00 | 679.6875 |     3199682 B |        1.00 |
| ImplementingClass_AsInterface |     100000 |  1.338 ms |  0.0099 ms |   0.0092 ms |  0.99 |    0.02 | 679.6875 |     3199682 B |        1.00 |
|      ImplementingClass_Direct |     100000 |  1.340 ms |  0.0223 ms |   0.0274 ms |  1.00 |    0.03 | 679.6875 |     3199682 B |        1.00 |
|    InterfaceVirtual_Overriden |     100000 |  1.341 ms |  0.0120 ms |   0.0100 ms |  0.99 |    0.03 | 679.6875 |     3199682 B |        1.00 |
|  InterfaceVirtual_AsInterface |     100000 |  1.344 ms |  0.0146 ms |   0.0137 ms |  1.00 |    0.03 | 679.6875 |     3199682 B |        1.00 |
|        InheritingClass_Direct |     100000 |  1.359 ms |  0.0267 ms |   0.0366 ms |  1.02 |    0.04 | 679.6875 |     3199682 B |        1.00 |




Static
============
|                                          Method | Iterations | Mean [ms] | Error [ms] | StdDev [ms] |     Gen0 | Allocated [B] |
|------------------------------------------------ |----------- |----------:|-----------:|------------:|---------:|--------------:|
|                               BaseStatic_Direct |     100000 |  1.324 ms |  0.0093 ms |   0.0083 ms | 679.6875 |     3199682 B |
|                         StandAloneStatic_Direct |     100000 |  1.333 ms |  0.0202 ms |   0.0179 ms | 679.6875 |     3199682 B |
|                        BaseStatic_FromInheritor |     100000 |  1.336 ms |  0.0125 ms |   0.0117 ms | 679.6875 |     3199682 B |
|           InterfaceStaticAbstract_DirectOnClass |     100000 |  1.338 ms |  0.0165 ms |   0.0146 ms | 679.6875 |     3199682 B |
|                          InterfaceStatic_Direct |     100000 |  1.338 ms |  0.0146 ms |   0.0129 ms | 679.6875 |     3199682 B |
|            InterfaceStaticVirtual_DirectOnClass |     100000 |  1.343 ms |  0.0217 ms |   0.0192 ms | 679.6875 |     3199682 B |
|              InterfaceStaticVirtual_AsInterface |     100000 |  1.538 ms |  0.0075 ms |   0.0066 ms | 679.6875 |     3199682 B |
|           InterfaceStaticAbstract_OnImplementor |     100000 |  1.542 ms |  0.0058 ms |   0.0051 ms | 679.6875 |     3199682 B |
| InterfaceStaticVirtualNoImplementor_OnInterface |     100000 |  1.549 ms |  0.0090 ms |   0.0084 ms | 679.6875 |     3199682 B |


All
============
|                                          Method | Iterations | Mean [ms] | Error [ms] | StdDev [ms] | Ratio | RatioSD |     Gen0 | Allocated [B] | Alloc Ratio |
|------------------------------------------------ |----------- |----------:|-----------:|------------:|------:|--------:|---------:|--------------:|------------:|
|                         StandAloneStatic_Direct |     100000 |  1.335 ms |  0.0070 ms |   0.0065 ms |  0.99 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|                        ImplementingClass_Direct |     100000 |  1.337 ms |  0.0094 ms |   0.0088 ms |  0.99 |    0.02 | 679.6875 |     3199682 B |        1.00 |
|                        BaseStatic_FromInheritor |     100000 |  1.337 ms |  0.0037 ms |   0.0033 ms |  0.99 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|                                BaseClass_Direct |     100000 |  1.338 ms |  0.0049 ms |   0.0046 ms |  0.99 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|                      InterfaceVirtual_Overriden |     100000 |  1.339 ms |  0.0080 ms |   0.0071 ms |  0.99 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|                          InterfaceStatic_Direct |     100000 |  1.340 ms |  0.0101 ms |   0.0094 ms |  0.99 |    0.02 | 679.6875 |     3199682 B |        1.00 |
|            InterfaceStaticVirtual_DirectOnClass |     100000 |  1.344 ms |  0.0076 ms |   0.0068 ms |  0.99 |    0.02 | 679.6875 |     3199682 B |        1.00 |
|                          InheritingClass_AsBase |     100000 |  1.344 ms |  0.0064 ms |   0.0060 ms |  1.00 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|                   ImplementingClass_AsInterface |     100000 |  1.344 ms |  0.0074 ms |   0.0065 ms |  0.99 |    0.02 | 679.6875 |     3199682 B |        1.00 |
|                    InterfaceVirtual_AsInterface |     100000 |  1.348 ms |  0.0079 ms |   0.0074 ms |  1.00 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|                          StandAloneClass_Direct |     100000 |  1.351 ms |  0.0182 ms |   0.0170 ms |  1.00 |    0.00 | 679.6875 |     3199682 B |        1.00 |
|                          InheritingClass_Direct |     100000 |  1.352 ms |  0.0102 ms |   0.0091 ms |  1.00 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|           InterfaceStaticAbstract_DirectOnClass |     100000 |  1.360 ms |  0.0114 ms |   0.0107 ms |  1.01 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|                               BaseStatic_Direct |     100000 |  1.385 ms |  0.0276 ms |   0.0307 ms |  1.03 |    0.02 | 679.6875 |     3199682 B |        1.00 |
|              InterfaceStaticVirtual_AsInterface |     100000 |  1.551 ms |  0.0071 ms |   0.0067 ms |  1.15 |    0.01 | 679.6875 |     3199682 B |        1.00 |
| InterfaceStaticVirtualNoImplementor_OnInterface |     100000 |  1.566 ms |  0.0065 ms |   0.0057 ms |  1.16 |    0.02 | 679.6875 |     3199682 B |        1.00 |
|           InterfaceStaticAbstract_OnImplementor |     100000 |  1.573 ms |  0.0075 ms |   0.0070 ms |  1.16 |    0.02 | 679.6875 |     3199682 B |        1.00 |




 */

[ Config( typeof(BenchmarkConfig) ) ]
public class InheritanceCallsBenchmarks {
    [ Params( // 100,
        100_000
        // 100_000
    ) ]
    [ SuppressMessage( "ReSharper", "UnassignedField.Global" ) ] // TODO: also try, iterations = 1, CollectionSize = 1_000_000 or 100_000
    public int Iterations;

    [ Benchmark( Baseline = true ) ]
    [ BenchmarkCategory( "Instance" ) ]
    public string StandAloneClass_Direct( ) {
        var    testClass = new StandAloneClass();
        string result    = String.Empty;
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            result = testClass.DoStuff( iteration );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Instance" ) ]
    public string BaseClass_Direct( ) {
        string result    = string.Empty;
        var    testClass = new BaseClass();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            result = testClass.DoStuff( iteration );
        }

        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Instance" ) ]
    public string InheritingClass_Direct( ) {
        string result    = string.Empty;
        var    testClass = new InheritingClass();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            result = testClass.DoStuff( iteration );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Instance" ) ]
    public string InheritingClass_AsBase( ) {
        string    result    = string.Empty;
        BaseClass testClass = new InheritingClass();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            result = testClass.DoStuff( iteration );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Instance" ) ]
    public string ImplementingClass_Direct( ) {
        string result    = string.Empty;
        var    testClass = new ImplementingClass();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            result = testClass.DoStuff( iteration );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Instance" ) ]
    public string ImplementingClass_AsInterface( ) {
        string          result    = string.Empty;
        IImplementation testClass = new ImplementingClass();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            result = testClass.DoStuff( iteration );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Instance" ) ]
    public string InterfaceVirtual_AsInterface( ) {
        string          result    = string.Empty;
        IImplementation testClass = new ImplementingClass();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            result = testClass.DoStuffOnInterface( iteration );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Instance" ) ]
    public string InterfaceVirtual_Overriden( ) {
        string result    = string.Empty;
        var    testClass = new ImplementingClass();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            result = testClass.DoStuffOnInterface( iteration );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Static" ) ]
    public string InterfaceStatic_Direct( ) {
        string result = string.Empty;
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            result = IImplementation.DoStuffStaticOnInterface( iteration );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Static" ) ]
    public string InterfaceStaticVirtual_AsInterface( ) {
        return doWorkWithTypeParameter<ImplementingClass>();

        string doWorkWithTypeParameter<T>( ) where T : IImplementation {
            string result = String.Empty;
            for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
                result = T.DoStuffStaticOnInterfaceVirtual( iteration );
            }
            return result;
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Static" ) ]
    public string InterfaceStaticVirtual_DirectOnClass( ) {
        string result = string.Empty;
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            result = ImplementingClass.DoStuffStaticOnInterfaceVirtual( iteration );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Static" ) ]
    public string InterfaceStaticVirtualNoImplementor_OnInterface( ) {
        return doWorkWithTypeParameter<ImplementingClass>();

        string doWorkWithTypeParameter<T>( ) where T : IImplementation {
            string result = string.Empty;
            for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
                result = T.DoStuffStaticOnInterfaceVirtualNoImplementor( iteration );
            }
            return result;
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Static" ) ]
    public string InterfaceStaticAbstract_OnImplementor( ) {
       return doWorkWithTypeParameter<ImplementingClass>();

        string doWorkWithTypeParameter<T>( ) where T : IImplementation {
            string result = string.Empty;
            for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
                result = T.DoStuffStaticOnInterfaceAbstract( iteration );
            }
            return result;
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Static" ) ]
    public string InterfaceStaticAbstract_DirectOnClass( ) {
        string result = string.Empty;
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            result = ImplementingClass.DoStuffStaticOnInterfaceAbstract( iteration );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Static" ) ]
    public string BaseStatic_FromInheritor( ) {
        string result = String.Empty;
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            result = InheritingClass.DoStuffStatic( iteration );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Static" ) ]
    public string BaseStatic_Direct( ) {
        string result = String.Empty;
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            result = BaseClass.DoStuffStatic( iteration );
        }
        return result;
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Static" ) ]
    public string StandAloneStatic_Direct( ) {
        string result = String.Empty;
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            result = StandAloneClass.DoStuffStatic( iteration );
        }
        return result;
    }


    //


    private class ImplementingClass : IImplementation {
        public string DoStuff( int param1 ) {
            return param1.ToString();
        }

        public string DoStuffOnInterface( int param1 ) {
            return param1.ToString();
        }

        public static string DoStuffStaticOnInterfaceVirtual( int param1 ) {
            return param1.ToString();
        }

        public static string DoStuffStaticOnInterfaceAbstract( int param1 ) {
            return param1.ToString();
        }
    }

    interface IImplementation {
        string DoStuff( int param1 );

        string DoStuffOnInterface( int param1 ) {
            return param1.ToString();
        }

        static string DoStuffStaticOnInterface( int param1 ) {
            return param1.ToString();
        }

        static virtual string DoStuffStaticOnInterfaceVirtual( int param1 ) {
            return param1.ToString();
        }

        static virtual string DoStuffStaticOnInterfaceVirtualNoImplementor( int param1 ) {
            return param1.ToString();
        }

        static abstract string DoStuffStaticOnInterfaceAbstract( int param1 );
        // URGENT: static methods too?
    }

    private class StandAloneClass {
        public string DoStuff( int param1 ) {
            return param1.ToString();
        }

        public static string DoStuffStatic( int param1 ) {
            return param1.ToString();
        }
    }


    private class BaseClass {
        public virtual string DoStuff( int param1 ) {
            return param1.ToString();
        }

        public static string DoStuffStatic( int param1 ) {
            return param1.ToString();
        }
    }

    private class InheritingClass : BaseClass {
        public override string DoStuff( int param1 ) {
            return param1.ToString();
        }
    }
}