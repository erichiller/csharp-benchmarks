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
 * | Method                           | Mean      | Error    | StdDev   | Ratio | RatioSD | Gen0    | Gen1   | Allocated | Alloc Ratio |
   |--------------------------------- |----------:|---------:|---------:|------:|--------:|--------:|-------:|----------:|------------:|
   | GenericListAsGenericInterface    |  39.11 us | 0.090 us | 0.080 us |  1.00 |    0.00 |  8.4229 | 0.9155 |  39.12 KB |        1.00 |
   | GenericList                      |  39.26 us | 0.166 us | 0.155 us |  1.00 |    0.01 |  8.4229 | 0.9155 |  39.12 KB |        1.00 |
   | NonGenericList                   |  56.41 us | 0.231 us | 0.193 us |  1.44 |    0.01 | 16.9067 | 2.8076 |  78.18 KB |        2.00 |
   | GenericListAsNonGenericInterface | 101.34 us | 0.831 us | 0.778 us |  2.58 |    0.02 | 59.4482 | 7.3242 | 273.49 KB |        6.99 |
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
            Utils.AssertThat( ( ( int )list[ ^1 ]! ) == ( int )_ints[ i ] );
        }
    }

    [ Benchmark ]
    public void GenericListAsNonGenericInterface( ) {
        IList list = new List<int>( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( _ints[ i ] );
            Utils.AssertThat( ( ( int )list[ ^1 ]! ) == ( int )_ints[ i ] );
        }
    }

    [ Benchmark( Baseline = true ) ]
    public void GenericList( ) {
        List<int> list = new (capacity: _iterations);
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( ( int )_ints[ i ] );
            Utils.AssertThat( list[ ^1 ] == ( int )_ints[ i ] );
        }
    }

    [ Benchmark ]
    public void GenericListAsGenericInterface( ) {
        IList<int> list = new List<int>( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( ( int )_ints[ i ] );
            Utils.AssertThat( list[ ^1 ] == ( int )_ints[ i ] );
        }
    }
}

/*
 * | Method                           | Mean     | Error    | StdDev   | Ratio | Gen0    | Gen1   | Allocated | Alloc Ratio |
   |--------------------------------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
   | GenericListAsGenericInterface    | 43.54 us | 0.199 us | 0.186 us |  1.00 | 16.9067 | 2.8076 |  78.18 KB |        1.00 |
   | GenericList                      | 43.61 us | 0.206 us | 0.183 us |  1.00 | 16.9067 | 2.8076 |  78.18 KB |        1.00 |
   | NonGenericList                   | 51.72 us | 0.609 us | 0.475 us |  1.19 | 16.9067 | 2.8076 |  78.18 KB |        1.00 |
   | GenericListAsNonGenericInterface | 76.16 us | 0.364 us | 0.341 us |  1.75 | 16.8457 | 2.8076 |  78.18 KB |        1.00 |
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
            Utils.AssertThat( ( ( string )list[ ^1 ]! ) == _strings[ i ] );
        }
    }

    [ Benchmark ]
    public void GenericListAsNonGenericInterface( ) {
        IList list = new List<string>( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( _strings[ i ] );
            Utils.AssertThat( ( ( string )list[ ^1 ]! ) == _strings[ i ] );
        }
    }

    [ Benchmark( Baseline = true, Description = "Hello?" ) ]
    public void GenericList( ) {
        List<string> list = new (capacity: _iterations);
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( _strings[ i ] );
            Utils.AssertThat( list[ ^1 ] == _strings[ i ] );
        }
    }

    [ Benchmark ]
    public void GenericListAsGenericInterface( ) {
        IList<string> list = new List<string>( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( _strings[ i ] );
            Utils.AssertThat( list[ ^1 ] == _strings[ i ] );
        }
    }
}

/*
 * | Method                           | Mean     | Error    | StdDev   | Ratio | Gen0    | Gen1   | Allocated | Alloc Ratio |
   |--------------------------------- |---------:|---------:|---------:|------:|--------:|-------:|----------:|------------:|
   | GenericListAsGenericInterface    | 51.04 us | 0.161 us | 0.151 us |  0.99 | 16.9067 | 2.8076 |  78.18 KB |        1.00 |
   | GenericList                      | 51.75 us | 0.360 us | 0.320 us |  1.00 | 16.9067 | 2.8076 |  78.18 KB |        1.00 |
   | NonGenericList                   | 54.39 us | 0.769 us | 0.642 us |  1.05 | 16.9067 | 2.8076 |  78.18 KB |        1.00 |
   | GenericListAsNonGenericInterface | 83.03 us | 0.628 us | 0.524 us |  1.60 | 16.8457 | 2.8076 |  78.18 KB |        1.00 |
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
            Utils.AssertThat( ( ( string )list[ ^1 ]! ) == ( string )_strings[ i ] );
        }
    }

    [ Benchmark ]
    public void GenericListAsNonGenericInterface( ) {
        IList list = new List<string>( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( _strings[ i ] );
            Utils.AssertThat( ( ( string )list[ ^1 ]! ) == ( string )_strings[ i ] );
        }
    }

    [ Benchmark( Baseline = true ) ]
    public void GenericList( ) {
        List<string> list = new (capacity: _iterations);
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( ( string )_strings[ i ] );
            Utils.AssertThat( list[ ^1 ] == ( string )_strings[ i ] );
        }
    }

    [ Benchmark ]
    public void GenericListAsGenericInterface( ) {
        IList<string> list = new List<string>( capacity: _iterations );
        for ( int i = 0 ; i < _iterations ; i++ ) {
            list.Add( ( string )_strings[ i ] );
            Utils.AssertThat( list[ ^1 ] == ( string )_strings[ i ] );
        }
    }
}