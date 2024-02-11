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
    public bool StringsToArray( ) {
        int i;
        for ( i = 0 ; i < _iterations ; i++ ) {
            _stringsOutput = _strings.ToArray();
        }
        return _stringsOutput is {};
    }
    [ Benchmark ]
    public bool StringsToList( ) {
        int i;
        for ( i = 0 ; i < _iterations ; i++ ) {
            _stringsOutput = _strings.ToList();
        }
        return _stringsOutput is {};
    }
}