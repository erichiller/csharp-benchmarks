using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Benchmarks.General;

/*
 * | Method                                  | Mean       | Error    | StdDev   | Gen0   | Allocated |
   |---------------------------------------- |-----------:|---------:|---------:|-------:|----------:|
   | LocalDirect                             |   245.3 us |  0.12 us |  0.10 us |      - |         - |
   | LocalStaticDirect                       |   245.4 us |  0.15 us |  0.14 us |      - |         - |
   | MethodDirect                            |   246.5 us |  1.65 us |  1.55 us |      - |         - |
   | MethodStaticDirect                      |   247.1 us |  0.97 us |  0.91 us |      - |         - |
   | LambdaAsParameter                       |   490.3 us |  0.14 us |  0.12 us |      - |       1 B |
   | DelegateAsParameter                     |   490.5 us |  0.15 us |  0.13 us |      - |       1 B |
   | DelegateStaticAsParameter               |   490.9 us |  0.58 us |  0.54 us |      - |       1 B |
   | FuncTttAsParameter                      |   492.5 us |  1.62 us |  1.52 us |      - |       1 B |
   | MethodAsParameter                       |   492.9 us |  1.02 us |  0.90 us | 0.9766 |    6401 B |
   | LambdaStaticAsParameter                 |   493.4 us |  1.46 us |  1.37 us |      - |       1 B |
   | LambdaDirect                            |   734.7 us |  0.16 us |  0.15 us |      - |       1 B |
   | LambdaStaticDirect                      |   735.0 us |  0.50 us |  0.47 us |      - |       1 B |
   | MethodAsDelegateAsParameter             |   735.2 us |  0.33 us |  0.29 us |      - |       1 B |
   | FuncTttStaticAsParameter                |   746.9 us | 10.45 us |  9.77 us |      - |       1 B |
   | DelegateDirect                          | 1,474.3 us |  1.51 us |  1.34 us |      - |       1 B |
   | FuncTttDirect                           | 1,474.7 us |  3.45 us |  3.06 us |      - |       1 B |
   | MethodAsDelegateDirect                  | 1,475.4 us |  2.62 us |  2.32 us |      - |       1 B |
   | FuncTttStaticDirect                     | 2,182.7 us | 15.31 us | 14.32 us |      - |       2 B |
   | DelegateStaticDirect                    | 2,187.9 us | 10.41 us |  9.74 us |      - |       3 B |
   | MethodStaticAsDelegateStaticDirect      | 2,470.9 us | 19.28 us | 17.09 us |      - |       3 B |
   | MethodStaticAsParameter                 | 2,702.1 us |  6.19 us |  5.17 us |      - |       3 B |
   | LocalStaticAsParameter                  | 2,709.3 us |  5.23 us |  4.37 us |      - |       3 B |
   | LocalAsParameter                        | 2,719.4 us | 15.60 us | 14.59 us |      - |    6406 B |
   | MethodStaticAsDelegateStaticAsParameter | 2,806.8 us | 55.68 us | 96.05 us |      - |       3 B |
 */
[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class FunctionAsParameterBenchmarks {
    private const           int      _iterations = 10_000;
    private const           int      _calls      = 100;
    private static readonly string[] _strings    = new string[ _iterations ];

    public FunctionAsParameterBenchmarks( ) {
        this._doWorkMethodAsDelegate        = this.doWorkMethod;
    }

    [ GlobalSetup ]
    public void GlobalSetup( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            _strings[ i ] = i.ToString();
        }
    }


    private static void doWork( DoWorkDelegateType f ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Utils.AssertThat( f( _strings[ i ], _strings[ i ] ) );
        }
    }

    private static void doWorkFuncTttParam( Func<string, string, bool> f ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Utils.AssertThat( f( _strings[ i ], _strings[ i ] ) );
        }
    }


    private delegate bool DoWorkDelegateType( string a, string b );

    private readonly        DoWorkDelegateType _doWorkDelegate       = ( string        a, string b ) => a == b;
    private static readonly DoWorkDelegateType _doWorkDelegateStatic = static ( string a, string b ) => a == b;

    private readonly        DoWorkDelegateType _doWorkMethodAsDelegate;
    private static readonly DoWorkDelegateType _doWorkMethodStaticAsDelegateStatic = doWorkMethodStatic;

    private readonly        Func<string, string, bool> _doWorkFuncTtt       = ( string        a, string b ) => a == b;
    private static readonly Func<string, string, bool> _doWorkFuncTttStatic = static ( string a, string b ) => a == b;

    private        bool doWorkMethod( string       a, string b ) => a == b;
    private static bool doWorkMethodStatic( string a, string b ) => a == b;


    [ Benchmark ]
    [ Category( "Direct" ) ]
    [ SuppressMessage( "ReSharper", "ConvertToLocalFunction", Justification = "This is what is being benchmarked" ) ]
    public void LambdaDirect( ) {
        Func<string, string, bool> f = ( string a, string b ) => a == b;
        for ( int c = 0 ; c < _calls ; c++ ) {
            for ( int i = 0 ; i < _iterations ; i++ ) {
                Utils.AssertThat( f( _strings[ i ], _strings[ i ] ) );
            }
        }
    }

    [ Benchmark ]
    [ Category( "AsParameter" ) ]
    public void LambdaAsParameter( ) {
        for ( int i = 0 ; i < _calls ; i++ ) {
            doWork( ( string a, string b ) => a == b );
        }
    }

    [ Benchmark ]
    [ Category( "Direct" ) ]
    [ SuppressMessage( "ReSharper", "ConvertToLocalFunction", Justification = "This is what is being benchmarked" ) ]
    public void LambdaStaticDirect( ) {
        Func<string, string, bool> f = static ( string a, string b ) => a == b;
        for ( int c = 0 ; c < _calls ; c++ ) {
            for ( int i = 0 ; i < _iterations ; i++ ) {
                Utils.AssertThat( f( _strings[ i ], _strings[ i ] ) );
            }
        }
    }

    [ Benchmark ]
    [ Category( "AsParameter" ) ]
    public void LambdaStaticAsParameter( ) {
        for ( int c = 0 ; c < _calls ; c++ ) {
            doWork( static ( string a, string b ) => a == b );
        }
    }

    [ Benchmark ]
    [ Category( "Direct" ) ]
    public void LocalDirect( ) {
        for ( int c = 0 ; c < _calls ; c++ ) {
            for ( int i = 0 ; i < _iterations ; i++ ) {
                Utils.AssertThat( DoWorkLocal( _strings[ i ], _strings[ i ] ) );
            }
        }
        bool DoWorkLocal( string a, string b ) => a == b;
    }

    [ Benchmark ]
    [ Category( "AsParameter" ) ]
    public void LocalAsParameter( ) {
        for ( int c = 0 ; c < _calls ; c++ ) {
            doWork( DoWorkLocal );
        }
        bool DoWorkLocal( string a, string b ) => a == b;
    }

    [ Benchmark ]
    [ Category( "Direct" ) ]
    public void LocalStaticDirect( ) {
        for ( int c = 0 ; c < _calls ; c++ ) {
            for ( int i = 0 ; i < _iterations ; i++ ) {
                Utils.AssertThat( DoWorkLocalStatic( _strings[ i ], _strings[ i ] ) );
            }
        }
        static bool DoWorkLocalStatic( string a, string b ) => a == b;
    }

    [ Benchmark ]
    [ Category( "AsParameter" ) ]
    public void LocalStaticAsParameter( ) {
        for ( int c = 0 ; c < _calls ; c++ ) {
            doWork( DoWorkLocalStatic );
        }
        static bool DoWorkLocalStatic( string a, string b ) => a == b;
    }

    [ Benchmark ]
    [ Category( "Direct" ) ]
    public void DelegateDirect( ) {
        for ( int c = 0 ; c < _calls ; c++ ) {
            for ( int i = 0 ; i < _iterations ; i++ ) {
                Utils.AssertThat( _doWorkDelegate( _strings[ i ], _strings[ i ] ) );
            }
        }
    }

    [ Benchmark ]
    [ Category( "AsParameter" ) ]
    public void DelegateAsParameter( ) {
        for ( int i = 0 ; i < _calls ; i++ ) {
            doWork( _doWorkDelegate );
        }
    }


    [ Benchmark ]
    [ Category( "Direct" ) ]
    public void MethodAsDelegateDirect( ) {
        for ( int c = 0 ; c < _calls ; c++ ) {
            for ( int i = 0 ; i < _iterations ; i++ ) {
                Utils.AssertThat( _doWorkMethodAsDelegate( _strings[ i ], _strings[ i ] ) );
            }
        }
    }

    [ Benchmark ]
    [ Category( "AsParameter" ) ]
    public void MethodAsDelegateAsParameter( ) {
        for ( int i = 0 ; i < _calls ; i++ ) {
            doWork( _doWorkMethodAsDelegate );
        }
    }

    [ Benchmark ]
    [ Category( "Direct" ) ]
    public void MethodStaticAsDelegateStaticDirect( ) {
        for ( int c = 0 ; c < _calls ; c++ ) {
            for ( int i = 0 ; i < _iterations ; i++ ) {
                Utils.AssertThat( _doWorkMethodStaticAsDelegateStatic( _strings[ i ], _strings[ i ] ) );
            }
        }
    }

    [ Benchmark ]
    [ Category( "AsParameter" ) ]
    public void MethodStaticAsDelegateStaticAsParameter( ) {
        for ( int i = 0 ; i < _calls ; i++ ) {
            doWork( _doWorkMethodStaticAsDelegateStatic );
        }
    }

    [ Benchmark ]
    [ Category( "Direct" ) ]
    public void DelegateStaticDirect( ) {
        for ( int c = 0 ; c < _calls ; c++ ) {
            for ( int i = 0 ; i < _iterations ; i++ ) {
                Utils.AssertThat( _doWorkDelegateStatic( _strings[ i ], _strings[ i ] ) );
            }
        }
    }

    [ Benchmark ]
    [ Category( "AsParameter" ) ]
    public void DelegateStaticAsParameter( ) {
        for ( int i = 0 ; i < _calls ; i++ ) {
            doWork( _doWorkDelegateStatic );
        }
    }

    [ Benchmark ]
    [ Category( "Direct" ) ]
    public void FuncTttDirect( ) {
        for ( int c = 0 ; c < _calls ; c++ ) {
            for ( int i = 0 ; i < _iterations ; i++ ) {
                Utils.AssertThat( _doWorkFuncTtt( _strings[ i ], _strings[ i ] ) );
            }
        }
    }

    [ Benchmark ]
    [ Category( "AsParameter" ) ]
    public void FuncTttAsParameter( ) {
        for ( int i = 0 ; i < _calls ; i++ ) {
            doWorkFuncTttParam( _doWorkFuncTtt );
        }
    }

    [ Benchmark ]
    [ Category( "Direct" ) ]
    public void FuncTttStaticDirect( ) {
        for ( int c = 0 ; c < _calls ; c++ ) {
            for ( int i = 0 ; i < _iterations ; i++ ) {
                Utils.AssertThat( _doWorkFuncTttStatic( _strings[ i ], _strings[ i ] ) );
            }
        }
    }

    [ Benchmark ]
    [ Category( "AsParameter" ) ]
    public void FuncTttStaticAsParameter( ) {
        for ( int i = 0 ; i < _calls ; i++ ) {
            doWorkFuncTttParam( _doWorkFuncTttStatic );
        }
    }

    [ Benchmark ]
    [ Category( "Direct" ) ]
    public void MethodDirect( ) {
        for ( int c = 0 ; c < _calls ; c++ ) {
            for ( int i = 0 ; i < _iterations ; i++ ) {
                Utils.AssertThat( doWorkMethod( _strings[ i ], _strings[ i ] ) );
            }
        }
    }

    [ Benchmark ]
    [ Category( "AsParameter" ) ]
    public void MethodAsParameter( ) {
        for ( int i = 0 ; i < _calls ; i++ ) {
            doWork( doWorkMethod );
        }
    }

    [ Benchmark ]
    [ Category( "Direct" ) ]
    public void MethodStaticDirect( ) {
        for ( int c = 0 ; c < _calls ; c++ ) {
            for ( int i = 0 ; i < _iterations ; i++ ) {
                Utils.AssertThat( doWorkMethodStatic( _strings[ i ], _strings[ i ] ) );
            }
        }
    }

    [ Benchmark ]
    [ Category( "AsParameter" ) ]
    public void MethodStaticAsParameter( ) {
        for ( int i = 0 ; i < _calls ; i++ ) {
            doWork( doWorkMethodStatic );
        }
    }
}