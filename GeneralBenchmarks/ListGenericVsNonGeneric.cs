using System.Collections;
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

namespace Benchmarks.General;

/*
 * | Method                           | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0    | Gen1    | Allocated | Alloc Ratio |
   |--------------------------------- |---------:|---------:|---------:|------:|--------:|--------:|--------:|----------:|------------:|
   | GenericListAsGenericInterface    | 14.78 us | 0.156 us | 0.146 us |  1.00 |    0.01 |  8.4686 |  1.0529 |  39.12 KB |        1.00 |
   | GenericList                      | 14.84 us | 0.146 us | 0.136 us |  1.00 |    0.01 |  8.4686 |  1.0529 |  39.12 KB |        1.00 |
   | GenericListAsNonGenericInterface | 79.59 us | 0.403 us | 0.357 us |  5.36 |    0.05 | 59.4482 |  8.4229 | 273.49 KB |        6.99 |
   | NonGenericList                   | 99.45 us | 1.125 us | 0.998 us |  6.70 |    0.09 | 61.8896 | 17.7002 | 312.55 KB |        7.99 |
 */
[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class GenericVsNonGenericListValueTypeHandlingWithInputValuesTyped {
    private const int _iterations = 10_000;

    [ Benchmark ]
    public void NonGenericList( ) {
        System.Collections.ArrayList list = new System.Collections.ArrayList( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( i );
        }
        Utils.AssertThat( list.Count == _iterations );
    }

    [ Benchmark ]
    public void GenericListAsNonGenericInterface( ) {
        IList list = new List<int>( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( i );
        }
        Utils.AssertThat( list.Count == _iterations );
    }

    [ Benchmark( Baseline = true ) ]
    public void GenericList( ) {
        List<int> list = new (capacity: _iterations);
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( i );
        }
        Utils.AssertThat( list.Count == _iterations );
    }

    [ Benchmark ]
    public void GenericListAsGenericInterface( ) {
        IList<int> list = new List<int>( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( i );
        }
        Utils.AssertThat( list.Count == _iterations );
    }
}

/*
 * | Method                           | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0    | Gen1   | Allocated | Alloc Ratio |
   |--------------------------------- |---------:|---------:|---------:|------:|--------:|--------:|-------:|----------:|------------:|
   | GenericList                      | 17.91 us | 0.072 us | 0.068 us |  1.00 |    0.01 |  8.4534 | 0.9155 |  39.12 KB |        1.00 |
   | GenericListAsGenericInterface    | 17.99 us | 0.196 us | 0.184 us |  1.00 |    0.01 |  8.4534 | 0.9155 |  39.12 KB |        1.00 |
   | GenericListAsNonGenericInterface | 39.77 us | 0.133 us | 0.124 us |  2.22 |    0.01 |  8.4229 | 0.9155 |  39.12 KB |        1.00 |
   | NonGenericList                   | 41.57 us | 0.477 us | 0.446 us |  2.32 |    0.03 | 16.9067 | 2.8076 |  78.18 KB |        2.00 |
 */
[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class GenericVsNonGenericListValueTypeHandlingWithInputValuesAsObjects {
    private const    int      _iterations = 10_000;
    private readonly object[] _ints       = new object[ _iterations ];

    [ GlobalSetup ]
    public void GlobalSetup( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            _ints[ i ] = i;
        }
    }

    [ Benchmark ]
    public void NonGenericList( ) {
        System.Collections.ArrayList list = new System.Collections.ArrayList( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( _ints[ i ] );
        }
        Utils.AssertThat( list.Count == _iterations );
    }

    [ Benchmark ]
    public void GenericListAsNonGenericInterface( ) {
        IList list = new List<int>( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( _ints[ i ] );
        }
        Utils.AssertThat( list.Count == _iterations );
    }

    [ Benchmark( Baseline = true ) ]
    public void GenericList( ) {
        List<int> list = new (capacity: _iterations);
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( ( int )_ints[ i ] );
        }
        Utils.AssertThat( list.Count == _iterations );
    }

    [ Benchmark ]
    public void GenericListAsGenericInterface( ) {
        IList<int> list = new List<int>( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( ( int )_ints[ i ] );
        }
        Utils.AssertThat( list.Count == _iterations );
    }
}

/*
 * | Method                           | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0    | Gen1   | Allocated | Alloc Ratio |
   |--------------------------------- |---------:|---------:|---------:|------:|--------:|--------:|-------:|----------:|------------:|
   | GenericList                      | 28.82 us | 0.261 us | 0.244 us |  1.00 |    0.01 | 16.9373 | 2.8076 |  78.18 KB |        1.00 |
   | GenericListAsGenericInterface    | 28.94 us | 0.217 us | 0.203 us |  1.00 |    0.01 | 16.9373 | 2.8076 |  78.18 KB |        1.00 |
   | NonGenericList                   | 41.49 us | 0.262 us | 0.233 us |  1.44 |    0.01 | 16.9067 | 2.8076 |  78.18 KB |        1.00 |
   | GenericListAsNonGenericInterface | 57.87 us | 0.273 us | 0.242 us |  2.01 |    0.02 | 16.9067 | 2.8076 |  78.18 KB |        1.00 |
 */
[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class GenericVsNonGenericListReferenceTypeHandlingWithInputValuesTyped {
    private const    int      _iterations = 10_000;
    private readonly string[] _strings    = new string[ _iterations ];

    [ GlobalSetup ]
    public void GlobalSetup( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            _strings[ i ] = i.ToString();
        }
    }

    [ Benchmark ]
    public void NonGenericList( ) {
        System.Collections.ArrayList list = new System.Collections.ArrayList( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( _strings[ i ] );
        }
        Utils.AssertThat( list.Count == _iterations );
    }

    [ Benchmark ]
    public void GenericListAsNonGenericInterface( ) {
        IList list = new List<string>( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( _strings[ i ] );
        }
        Utils.AssertThat( list.Count == _iterations );
    }

    [ Benchmark( Baseline = true ) ]
    public void GenericList( ) {
        List<string> list = new (capacity: _iterations);
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( _strings[ i ] );
        }
        Utils.AssertThat( list.Count == _iterations );
    }

    [ Benchmark ]
    public void GenericListAsGenericInterface( ) {
        IList<string> list = new List<string>( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( _strings[ i ] );
        }
        Utils.AssertThat( list.Count == _iterations );
    }
}

/*
 * | Method                           | Mean     | Error    | StdDev   | Ratio | RatioSD | Gen0    | Gen1   | Allocated | Alloc Ratio |
   |--------------------------------- |---------:|---------:|---------:|------:|--------:|--------:|-------:|----------:|------------:|
   | GenericListAsGenericInterface    | 31.34 us | 0.224 us | 0.209 us |  0.99 |    0.02 | 16.9067 | 2.8076 |  78.18 KB |        1.00 |
   | GenericList                      | 31.75 us | 0.573 us | 0.589 us |  1.00 |    0.03 | 16.9067 | 2.8076 |  78.18 KB |        1.00 |
   | NonGenericList                   | 44.14 us | 0.185 us | 0.173 us |  1.39 |    0.03 | 16.9067 | 2.8076 |  78.18 KB |        1.00 |
   | GenericListAsNonGenericInterface | 57.68 us | 0.275 us | 0.257 us |  1.82 |    0.03 | 16.9067 | 2.8076 |  78.18 KB |        1.00 |
 */
[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public class GenericVsNonGenericListReferenceTypeHandlingWithInputValuesAsObjects {
    private const    int      _iterations = 10_000;
    private readonly object[] _strings    = new object[ _iterations ];

    [ GlobalSetup ]
    public void GlobalSetup( ) {
        for ( int i = 0 ; i < _iterations ; i++ ) {
            _strings[ i ] = i.ToString();
        }
    }

    [ Benchmark ]
    public void NonGenericList( ) {
        System.Collections.ArrayList list = new System.Collections.ArrayList( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( _strings[ i ] );
        }
        Utils.AssertThat( list.Count == _iterations );
    }

    [ Benchmark ]
    public void GenericListAsNonGenericInterface( ) {
        IList list = new List<string>( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( _strings[ i ] );
        }
        Utils.AssertThat( list.Count == _iterations );
    }

    [ Benchmark( Baseline = true ) ]
    public void GenericList( ) {
        List<string> list = new (capacity: _iterations);
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( ( string )_strings[ i ] );
        }
        Utils.AssertThat( list.Count == _iterations );
    }

    [ Benchmark ]
    public void GenericListAsGenericInterface( ) {
        IList<string> list = new List<string>( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( ( string )_strings[ i ] );
        }
        Utils.AssertThat( list.Count == _iterations );
    }
}