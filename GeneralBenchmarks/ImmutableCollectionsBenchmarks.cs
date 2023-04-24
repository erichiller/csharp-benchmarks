using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.General;
/* RESULTS:
 
 # Iterations = 100_000
|    Method | CollectionSize |    Mean [ms] | Error [ms] | StdDev [ms] | Ratio | RatioSD |        Gen0 |        Gen1 | Allocated [B] | Alloc Ratio |
|---------- |--------------- |-------------:|-----------:|------------:|------:|--------:|------------:|------------:|--------------:|------------:|
| Array_Add |             10 |     1.045 ms |  0.0103 ms |   0.0092 ms |  1.00 |    0.00 |   1359.3750 |           - |     6400002 B |        1.00 |
|  List_Add |             10 |     2.719 ms |  0.0518 ms |   0.0654 ms |  2.60 |    0.07 |   2039.0625 |           - |     9600004 B |        1.50 |
|           |                |              |            |             |       |         |             |             |               |             |
| Array_Add |            100 |     6.543 ms |  0.1307 ms |   0.3105 ms |  1.00 |    0.00 |   9007.8125 |           - |    42400007 B |        1.00 |
|  List_Add |            100 |    16.420 ms |  0.3221 ms |   0.3834 ms |  2.51 |    0.09 |   9687.5000 |           - |    45600029 B |        1.08 |
|           |                |              |            |             |       |         |             |             |               |             |
| Array_Add |           1000 |    51.306 ms |  0.2053 ms |   0.1920 ms |  1.00 |    0.00 |  85500.0000 |           - |   402400094 B |        1.00 |
|  List_Add |           1000 |   148.249 ms |  2.8585 ms |   2.6739 ms |  2.89 |    0.05 |  86000.0000 |   1250.0000 |   405600234 B |        1.01 |
|           |                |              |            |             |       |         |             |             |               |             |
| Array_Add |          10000 |   502.077 ms |  8.2309 ms |   9.7983 ms |  1.00 |    0.00 | 847000.0000 |           - |  4002400936 B |        1.00 |
|  List_Add |          10000 | 1,457.430 ms | 27.3344 ms |  30.3821 ms |  2.90 |    0.06 | 847000.0000 | 105000.0000 |  4005600936 B |        1.00 |

 # Iterations = 100
 
|                                       Method | CollectionSize |    Mean [us] | Error [us] | StdDev [us] |  Median [us] |  Ratio | RatioSD |     Gen0 |   Gen1 | Allocated [B] | Alloc Ratio |
|--------------------------------------------- |--------------- |-------------:|-----------:|------------:|-------------:|-------:|--------:|---------:|-------:|--------------:|------------:|
|                                    Array_Add |             10 |     1.050 us |  0.0045 us |   0.0040 us |     1.050 us |   1.00 |    0.00 |   1.3599 |      - |        6400 B |        1.00 |
| ImmutableArray_Array_AsSpan_ToImmutableArray |             10 |     2.323 us |  0.0450 us |   0.0615 us |     2.310 us |   2.20 |    0.07 |   2.7199 |      - |       12800 B |        2.00 |
|               ImmutableArray_CreateFrom_Span |             10 |     2.330 us |  0.0504 us |   0.1487 us |     2.286 us |   2.44 |    0.08 |   2.7199 |      - |       12800 B |        2.00 |
|              ImmutableArray_CreateFrom_Array |             10 |     2.549 us |  0.0509 us |   0.1129 us |     2.576 us |   2.47 |    0.10 |   2.7199 |      - |       12800 B |        2.00 |
|                                     List_Add |             10 |     2.620 us |  0.0231 us |   0.0180 us |     2.627 us |   2.49 |    0.02 |   2.0447 |      - |        9632 B |        1.50 |
|        ImmutableArray_Array_ToImmutableArray |             10 |     4.796 us |  0.0933 us |   0.1309 us |     4.811 us |   4.51 |    0.15 |   2.7161 |      - |       12800 B |        2.00 |
|             ImmutableArray_CreateWithBuilder |             10 |     5.485 us |  0.0993 us |   0.1220 us |     5.509 us |   5.22 |    0.14 |   5.0964 |      - |       24000 B |        3.75 |
|                     ImmutableArray_DirectAdd |             10 |    14.283 us |  0.2444 us |   0.2286 us |    14.257 us |  13.60 |    0.22 |  10.1929 |      - |       48000 B |        7.50 |
|               ImmutableList_CreateFrom_Array |             10 |    23.615 us |  0.2753 us |   0.2441 us |    23.567 us |  22.48 |    0.21 |  12.5732 |      - |       59200 B |        9.25 |
|          ImmutableList_Array_ToImmutableList |             10 |    23.624 us |  0.3815 us |   0.3569 us |    23.560 us |  22.47 |    0.36 |  12.5732 |      - |       59200 B |        9.25 |
|              ImmutableList_CreateWithBuilder |             10 |    36.961 us |  0.3109 us |   0.2908 us |    36.912 us |  35.17 |    0.32 |  11.7188 |      - |       55200 B |        8.62 |
|                      ImmutableList_DirectAdd |             10 |    70.358 us |  1.3162 us |   1.1668 us |    70.131 us |  66.99 |    1.09 |  40.7715 |      - |      192000 B |       30.00 |
|                                              |                |              |            |             |              |        |         |          |        |               |             |
|                                    Array_Add |            100 |     6.762 us |  0.1339 us |   0.2481 us |     6.731 us |   1.00 |    0.00 |   9.0103 |      - |       42400 B |        1.00 |
|               ImmutableArray_CreateFrom_Span |            100 |     9.782 us |  0.2546 us |   0.7346 us |     9.744 us |   1.52 |    0.12 |  18.0206 | 0.0153 |       84800 B |        2.00 |
| ImmutableArray_Array_AsSpan_ToImmutableArray |            100 |    10.159 us |  0.2016 us |   0.3478 us |    10.146 us |   1.50 |    0.08 |  18.0206 |      - |       84800 B |        2.00 |
|              ImmutableArray_CreateFrom_Array |            100 |    10.773 us |  0.3359 us |   0.9904 us |    10.669 us |   1.48 |    0.09 |  18.0206 | 0.0153 |       84800 B |        2.00 |
|        ImmutableArray_Array_ToImmutableArray |            100 |    11.725 us |  0.1008 us |   0.0894 us |    11.745 us |   1.73 |    0.08 |  18.0206 | 0.0153 |       84800 B |        2.00 |
|                                     List_Add |            100 |    17.312 us |  0.3126 us |   0.2924 us |    17.410 us |   2.56 |    0.13 |   9.6741 |      - |       45632 B |        1.08 |
|             ImmutableArray_CreateWithBuilder |            100 |    29.499 us |  0.5847 us |   0.9442 us |    29.346 us |   4.35 |    0.24 |  33.3252 | 0.0610 |      156800 B |        3.70 |
|          ImmutableList_Array_ToImmutableList |            100 |   186.664 us |  1.7379 us |   1.6256 us |   186.500 us |  27.59 |    1.14 | 111.8164 | 1.2207 |      527200 B |       12.43 |
|               ImmutableList_CreateFrom_Array |            100 |   193.643 us |  2.3155 us |   2.1659 us |   194.038 us |  28.62 |    1.18 | 111.8164 | 1.2207 |      527200 B |       12.43 |
|                     ImmutableArray_DirectAdd |            100 |   283.849 us |  6.0185 us |  17.7456 us |   285.021 us |  41.15 |    2.70 | 484.3750 | 0.4883 |     2280000 B |       53.77 |
|              ImmutableList_CreateWithBuilder |            100 |   591.285 us |  4.8241 us |   4.5124 us |   590.638 us |  87.40 |    3.75 | 103.5156 | 0.9766 |      487201 B |       11.49 |
|                      ImmutableList_DirectAdd |            100 | 1,523.168 us | 28.3529 us |  26.5213 us | 1,516.921 us | 225.20 |   11.52 | 736.3281 | 5.8594 |     3470402 B |       81.85 |


****

|                                       Method | Iterations | CollectionSize |   Mean [us] | Error [us] | StdDev [us] | Median [us] | Ratio | RatioSD |       Gen0 |    Gen1 | Allocated [B] | Alloc Ratio |
|--------------------------------------------- |----------- |--------------- |------------:|-----------:|------------:|------------:|------:|--------:|-----------:|--------:|--------------:|------------:|
|                                    Array_Add |      10000 |             10 |    105.3 us |    0.42 us |     0.35 us |    105.4 us |  1.00 |    0.00 |   135.9863 |       - |      640000 B |        1.00 |
|               ImmutableArray_CreateFrom_Span |      10000 |             10 |    213.4 us |    0.56 us |     0.50 us |    213.4 us |  2.03 |    0.01 |   271.9727 |       - |     1280000 B |        2.00 |
| ImmutableArray_Array_AsSpan_ToImmutableArray |      10000 |             10 |    222.1 us |    1.39 us |     1.30 us |    222.4 us |  2.11 |    0.01 |   271.9727 |       - |     1280000 B |        2.00 |
|              ImmutableArray_CreateFrom_Array |      10000 |             10 |    244.3 us |    6.07 us |    17.89 us |    231.4 us |  2.59 |    0.05 |   271.9727 |       - |     1280000 B |        2.00 |
|        ImmutableArray_Array_ToImmutableArray |      10000 |             10 |    460.2 us |    1.51 us |     1.34 us |    460.3 us |  4.37 |    0.02 |   271.9727 |       - |     1280000 B |        2.00 |
|             ImmutableArray_CreateWithBuilder |      10000 |             10 |    595.5 us |   10.78 us |    11.98 us |    595.2 us |  5.67 |    0.09 |   509.7656 |       - |     2400001 B |        3.75 |
|                     ImmutableArray_DirectAdd |      10000 |             10 |  1,344.2 us |   11.77 us |     9.82 us |  1,343.4 us | 12.77 |    0.12 |  1019.5313 |       - |     4800002 B |        7.50 |
|                                              |            |                |             |            |             |             |       |         |            |         |               |             |
|                                    Array_Add |      10000 |            100 |    608.5 us |    7.43 us |     6.20 us |    607.8 us |  1.00 |    0.00 |   900.3906 |       - |     4240001 B |        1.00 |
|               ImmutableArray_CreateFrom_Span |      10000 |            100 |    913.9 us |    4.42 us |     4.13 us |    913.7 us |  1.50 |    0.02 |  1801.7578 |  1.9531 |     8480001 B |        2.00 |
| ImmutableArray_Array_AsSpan_ToImmutableArray |      10000 |            100 |    918.4 us |   12.08 us |    10.71 us |    917.3 us |  1.51 |    0.01 |  1801.7578 |  1.9531 |     8480001 B |        2.00 |
|              ImmutableArray_CreateFrom_Array |      10000 |            100 |    960.5 us |   10.56 us |     9.37 us |    959.6 us |  1.58 |    0.03 |  1801.7578 |  1.9531 |     8480001 B |        2.00 |
|        ImmutableArray_Array_ToImmutableArray |      10000 |            100 |  1,179.5 us |    7.88 us |     7.38 us |  1,177.0 us |  1.94 |    0.03 |  1800.7813 |  1.9531 |     8480002 B |        2.00 |
|             ImmutableArray_CreateWithBuilder |      10000 |            100 |  2,745.3 us |   19.23 us |    17.05 us |  2,740.0 us |  4.51 |    0.04 |  3332.0313 |  7.8125 |    15680004 B |        3.70 |
|                     ImmutableArray_DirectAdd |      10000 |            100 | 25,475.1 us |  183.58 us |   143.33 us | 25,514.0 us | 41.94 |    0.53 | 48437.5000 | 31.2500 |   228000029 B |       53.77 |



 */

[ Config( typeof(BenchmarkConfig) ) ]
[ SuppressMessage( "Performance", "CA1822:Mark members as static" ) ]
public class ImmutableCollectionsBenchmarks {
    [ Params( // 100,
              10_000
              // 100_000
    ) ]
    [ SuppressMessage( "ReSharper", "UnassignedField.Global" ) ] // TODO: also try, iterations = 1, CollectionSize = 1_000_000 or 100_000
    public int Iterations;

    [ Params( 10, 100
                  // , 1_000
                  // , 10_000
                  ) ]
    [ SuppressMessage( "ReSharper", "UnassignedField.Global" ) ] // TODO: also try, iterations = 1, CollectionSize = 1_000_000 or 100_000
    public int CollectionSize;


    /*
     * ImmutableArray
     */

    [ Benchmark( Baseline = true ) ]
    [ BenchmarkCategory( "Array" ) ]
    public int[] Array_Add( ) {
        int[] collection = Array.Empty<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            collection = new int[ CollectionSize ];
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection[ i ] = i;
            }
        }
        return collection;
    }

    /*
     * List
     */

    [ Benchmark() ]
    [ BenchmarkCategory( "List" ) ]
    public List<int> List_Add( ) {
        List<int> collection = new List<int>( 0 );
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            collection = new List<int>( CollectionSize );
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection.Add( i );
            }
        }
        return collection;
    }

    /*
     * ImmutableArray
     */

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableArray" ) ]
    public ImmutableArray<int> ImmutableArray_DirectAdd( ) {
        var collection = ImmutableArray<int>.Empty;
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            collection = ImmutableArray<int>.Empty;
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection = collection.Add( i );
            }
        }
        return collection;
    }

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableArray" ) ]
    public ImmutableArray<int> ImmutableArray_CreateWithBuilder( ) {
        var                         output = ImmutableArray.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = ImmutableArray.CreateBuilder<int>();
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection.Add( i );
            }
            output = collection.ToImmutable();
        }
        return output;
    }

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableArray" ) ]
    public ImmutableArray<int> ImmutableArray_CreateFrom_Array( ) {
        var   output = ImmutableArray.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = new int[ CollectionSize ];
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection[ i ] = i;
            }
            output = ImmutableArray.Create( collection );
        }
        return output;
    }

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableArray" ) ]
    public ImmutableArray<int> ImmutableArray_CreateFrom_Span( ) {
        var   output = ImmutableArray.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = new int[ CollectionSize ];
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection[ i ] = i;
            }
            output = ImmutableArray.Create( collection.AsSpan() );
        }
        return output;
    }


    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableArray" ) ]
    public ImmutableArray<int> ImmutableArray_Array_ToImmutableArray( ) {
        var   output = ImmutableArray.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = new int[ CollectionSize ];
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection[ i ] = i;
            }
            output = collection.ToImmutableArray();
        }
        return output;
    }


    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableArray" ) ]
    public ImmutableArray<int> ImmutableArray_Array_AsSpan_ToImmutableArray( ) {
        var   output = ImmutableArray.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = new int[ CollectionSize ];
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection[ i ] = i;
            }
            output = collection.AsSpan().ToImmutableArray();
        }
        return output;
    }

    /*
     * ImmutableList
     */

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableList" ) ]
    public ImmutableList<int> ImmutableList_DirectAdd( ) {
        var collection = ImmutableList<int>.Empty;
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            collection = ImmutableList<int>.Empty;
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection = collection.Add( i );
            }
        }
        return collection;
    }

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableList" ) ]
    public ImmutableList<int> ImmutableList_CreateWithBuilder( ) {
        var                        output = ImmutableList.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = ImmutableList.CreateBuilder<int>();
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection.Add( i );
            }
            output = collection.ToImmutable();
        }
        return output;
    }

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableList" ) ]
    public ImmutableList<int> ImmutableList_CreateFrom_Array( ) {
        var   output = ImmutableList.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = new int[ CollectionSize ];
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection[ i ] = i;
            }
            output = ImmutableList.Create( collection );
        }
        return output;
    }

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableList" ) ]
    public ImmutableList<int> ImmutableList_Array_ToImmutableList( ) {
        var   output = ImmutableList.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = new int[ CollectionSize ];
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection[ i ] = i;
            }
            output = collection.ToImmutableList();
        }
        return output;
    }
}