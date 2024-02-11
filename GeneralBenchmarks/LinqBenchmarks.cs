using System;
using System.Collections.Generic;
using System.Linq;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.General;

/*
 * As of 2024 February 11
 * | Method         | NumElements | Mean [ms]    | Error [ms] | StdDev [ms] | Gen0        | Gen1      | Allocated [B] |
   |--------------- |------------ |-------------:|-----------:|------------:|------------:|----------:|--------------:|
   | StringsToArray | 10          |     8.690 ms |  0.0891 ms |   0.0790 ms |   2000.0000 |         - |    10400736 B |
   | StringsToList  | 10          |    10.809 ms |  0.0931 ms |   0.0777 ms |   2000.0000 |         - |    13600736 B |
   | StringsToArray | 100         |    60.198 ms |  0.2241 ms |   0.1986 ms |  17000.0000 |         - |    82400736 B |
   | StringsToList  | 100         |    67.683 ms |  0.3924 ms |   0.3064 ms |  18000.0000 |         - |    85600736 B |
   | StringsToArray | 1000        | 1,235.750 ms | 14.0660 ms |  12.4692 ms | 646000.0000 | 2000.0000 |  3042400736 B |
   | StringsToList  | 1000        | 1,515.336 ms | 24.9726 ms |  23.3594 ms | 647000.0000 | 1000.0000 |  3045600736 B |
   
   
   ==== Without ToArray ====
   | Method                | NumElements | Mean [ms]   | Error [ms] | StdDev [ms] | Gen0         | Gen1      | Allocated [B] |
   |---------------------- |------------ |------------:|-----------:|------------:|-------------:|----------:|--------------:|
   | StaticSelectLambda    | 10          |    23.88 ms |   0.279 ms |    0.233 ms |   12000.0000 |         - |    56800736 B |
   | NonStaticSelectLambda | 10          |    24.15 ms |   0.368 ms |    0.344 ms |   12000.0000 |         - |    56800736 B |
   | NonStaticSelectLambda | 100         |   183.31 ms |   0.755 ms |    0.631 ms |  103000.0000 | 3000.0000 |   488800736 B |
   | StaticSelectLambda    | 100         |   183.65 ms |   0.823 ms |    0.729 ms |  103000.0000 | 3000.0000 |   488800736 B |
   | StaticSelectLambda    | 1000        | 2,424.12 ms |  43.624 ms |   40.806 ms | 1498000.0000 | 1000.0000 |  7048800736 B |
   | NonStaticSelectLambda | 1000        | 2,615.54 ms |  52.296 ms |   66.137 ms | 1498000.0000 | 1000.0000 |  7048800736 B |
   


 */

[ Config( typeof(BenchmarkConfig) ) ]
public class LinqBenchmarks {
    
    [ Params( 10, 100, 1_000 ) ] // , 5_000
    public int NumElements { get; set; }
    private const int _iterations = 100_000;

    private IEnumerable<string>  _strings = Array.Empty<string>();
    private IEnumerable<string>? _stringsOutput = null;
    
    [ IterationSetup ]
    public void Setup( ) {
        _strings = Enumerable.Range( 0, NumElements ).Select( x => x.ToString() );
    }

    [ Benchmark ]
    [ BenchmarkCategory( "ToNewEnumerableType" ) ]
    public bool StringsToArray( ) {
        int i;
        for ( i = 0 ; i < _iterations ; i++ ) {
            _stringsOutput = _strings.ToArray();
        }
        return _stringsOutput is {};
    }
    [ Benchmark ]
    [ BenchmarkCategory( "ToNewEnumerableType" ) ]
    public bool StringsToList( ) {
        int i;
        for ( i = 0 ; i < _iterations ; i++ ) {
            _stringsOutput = _strings.ToList();
        }
        return _stringsOutput is {};
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "SelectLambdaInvocation" ) ]
    public bool StaticSelectLambda( ) {
        int i;
        for ( i = 0 ; i < _iterations ; i++ ) {
            _stringsOutput = _strings.Select( static x => $"{x}-mod" ).ToArray(); // the ToArray() is required, otherwise no work is done.
        }
        return _stringsOutput is {};
    }
    [ Benchmark ]
    [ BenchmarkCategory( "SelectLambdaInvocation" ) ]
    public bool NonStaticSelectLambda( ) {
        int i;
        for ( i = 0 ; i < _iterations ; i++ ) {
            _stringsOutput = _strings.Select( x => $"{x}-mod" ).ToArray(); // the ToArray() is required, otherwise no work is done.
        }
        return _stringsOutput is { };
    }
}