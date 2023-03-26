using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.General;
/*
|            Method | Mean [us] | Error [us] | StdDev [us] | Ratio | Allocated [B] | Alloc Ratio |
|------------------ |----------:|-----------:|------------:|------:|--------------:|------------:|
|   InParams_Return |  151.2 us |    0.27 us |     0.21 us |  0.92 |             - |          NA |
|      SimpleReturn |  165.0 us |    1.01 us |     0.90 us |  1.00 |             - |          NA |
| InParams_OutParam |  169.4 us |    0.70 us |     0.65 us |  1.03 |             - |          NA |
 */

[ Config( typeof(BenchmarkConfig) ) ]
[ SuppressMessage( "Performance", "CA1822:Mark members as static" ) ]
public class ReferenceParameterBenchmarks {
    private const int _iterations = 100_000;
    private const int _size       = 20;

    [ Benchmark( Baseline = true ) ]
    public int SimpleReturn( ) {
        static int simpleReturnWork( int xi, int yi, int zi ) {
            return ( ( xi == 1
                ? -yi
                : yi + 1 ) + ( ( zi / 2 ) - 1 ) );
        }
        
        int result = 0;
        int z      = _size * 2;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            int x = i % 2;
            int y = i % _size;
            result += simpleReturnWork( x, y, z );
        }
        return result;
    }
    //
    //
    // [ Benchmark ]
    // public int InParams_Return( ) {
    //     int result = 0;
    //     int z      = _size * 2;
    //     for ( int i = 0 ; i < _iterations ; i++ ) {
    //         int x = i % 2;
    //         int y = i % _size;
    //         result += inParams_ReturnWork( in x, in y, in z );
    //     }
    //     return result;
    // }
    //
    // private int inParams_ReturnWork( in int x, in int y, in int z ) {
    //     return ( ( x == 1
    //         ? -y
    //         : y + 1 ) + ( ( z / 2 ) - 1 ) );
    // }

    [ Benchmark ]
    public int InParams_Return( ) {
        static int workInternalFunc( in int xi, in int yi, in int zi ) =>
            ( ( xi == 1
                ? -yi
                : yi + 1 ) + ( ( zi / 2 ) - 1 ) );

        int result = 0;
        int z      = _size * 2;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            int x = i % 2;
            int y = i % _size;
            result += workInternalFunc( in x, in y, in z );
        }
        return result;
    }
    
    [ Benchmark ]
    public int InParams_OutParam( ) {
        static void workInternalFunc( in int xi, in int yi, in int zi, out int fResult ) =>
            fResult = ( ( xi == 1
                         ? -yi
                         : yi + 1 ) + ( ( zi / 2 ) - 1 ) );

        int result = 0;
        int z      = _size * 2;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            int x = i % 2;
            int y = i % _size;
            workInternalFunc( in x, in y, in z, out int fOut );
            result += fOut;
        }
        return result;
    }
}