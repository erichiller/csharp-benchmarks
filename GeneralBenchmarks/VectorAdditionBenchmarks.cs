// #define VERIFY_RESULTS
using System;
using System.Runtime.Intrinsics;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.General;

/*
 * March 07, 2024
 * 
   | Method                                       | Mean [ms] | Error [ms] | StdDev [ms] | Allocated [B] |
   |--------------------------------------------- |----------:|-----------:|------------:|--------------:|
   | Vector256_Byte_Ushort_Addition_3             |  6.827 ms |  0.0025 ms |   0.0019 ms |           6 B |
   | Vector256_Byte_Ushort_Addition_2             |  6.859 ms |  0.0352 ms |   0.0329 ms |           6 B |
   | Vector256_Byte_Ushort_Addition_1             |  8.394 ms |  0.0729 ms |   0.0609 ms |          12 B |
   | Vector256_Byte_Ushort_Addition_4_FrequentSum |  8.731 ms |  0.0030 ms |   0.0026 ms |          12 B |
   | Avx2_Ushort_Verified_NoThrow_Addition        |  8.745 ms |  0.0031 ms |   0.0026 ms |          12 B |
   | Manual_Byte_Addition                         | 50.822 ms |  0.0199 ms |   0.0166 ms |          74 B |
 */

[ Config( typeof(BenchmarkConfig) ) ]
public class VectorAdditionBenchmarks {
    private byte[] _bytes = Array.Empty<byte>();

    [ GlobalSetup ]
    public void CreateByteArray( ) {
        this._bytes = new byte[ 1024 ];
        for ( int i = 0 ; i < _bytes.Length ; i++ ) {
            _bytes[ i ] = ( byte )( i % 256 );
        }
    }

#if VERIFY_RESULTS
    private void verifyResult( int result ) {
        if ( result != this._bytes.Length switch {
                           1024 => 130_560,
                           _    => throw new ArgumentException( "Unexpected byte length" )
                       } ) {
            throw new Exception( $"Incorrect result: {result}" );
        }
    }
#endif

    private const int _iterations = 100_000;

    /*
     *
     */

    [ Benchmark ]
    public int Avx2_Ushort_Verified_NoThrow_Addition( ) {
        const int sumEveryBytes = ( 2 ^ 16 ) / ( 2 ^ 8 ); // = 256
        int       vectorFinal   = 0;
        for ( int iteration = 0 ; iteration < _iterations ; iteration++ ) {
            vectorFinal = 0;
            var               byteSpan = new Span<byte>( _bytes );
            int               i        = 0;
            Vector256<ushort> results  = Vector256.Create<ushort>( 0 );
            while ( i < _bytes.Length ) {
                var (vec1, vec2) =  Vector256.Widen( System.Runtime.Intrinsics.Vector256.Create<byte>( byteSpan[ i..( i += 32 ) ] ) );
                results          += System.Runtime.Intrinsics.X86.Avx2.Add( vec1, vec2 );
                if ( i % sumEveryBytes == 0 ) {
                    vectorFinal += System.Runtime.Intrinsics.Vector256.Sum( results );
                    results     =  Vector256.Create<ushort>( 0 ); // reset
                }
            }
            vectorFinal += System.Runtime.Intrinsics.Vector256.Sum( results );
        }
#if VERIFY_RESULTS
        verifyResult( vectorFinal );
#endif
        return vectorFinal;
    }


    /*
     *
     *
     *


     */

    [ Benchmark ]
    public int Vector256_Byte_Ushort_Addition_1( ) {
        const int sumEveryBytes = ( 2 ^ 16 ) / ( 2 ^ 8 ); // = 256
        int       vectorFinal   = 0;
        for ( int iteration = 0 ; iteration < _iterations ; iteration++ ) {
            vectorFinal = 0;
            var               byteSpan = new Span<byte>( _bytes );
            int               i        = 0;
            Vector256<ushort> results  = Vector256.Create<ushort>( 0 );
            while ( i < _bytes.Length ) {
                var (vec1, vec2) =  Vector256.Widen( System.Runtime.Intrinsics.Vector256.Create<byte>( byteSpan[ i..( i += 32 ) ] ) );
                results          += vec1 + vec2;
                if ( i % sumEveryBytes == 0 ) {
                    vectorFinal += System.Runtime.Intrinsics.Vector256.Sum( results );
                    results     =  Vector256.Create<ushort>( 0 ); // reset
                }
            }
            vectorFinal += System.Runtime.Intrinsics.Vector256.Sum( results );
        }
#if VERIFY_RESULTS
        verifyResult( vectorFinal );
#endif
        return vectorFinal;
    }

    /*
     * 
     */

    [ Benchmark ]
    public int Vector256_Byte_Ushort_Addition_2( ) {
        const int sumEveryBytes = ( 2 ^ 16 ) / ( 2 ^ 8 ); // = 256
        int       vectorFinal   = 0;
        for ( int iteration = 0 ; iteration < _iterations ; iteration++ ) {
            vectorFinal = 0;
            var               byteSpan = new Span<byte>( _bytes );
            int               i        = 0;
            int               modCount = 0;
            Vector256<ushort> results  = Vector256.Create<ushort>( 0 );
            while ( i < _bytes.Length ) {
                var (vec1, vec2) =  Vector256.Widen( System.Runtime.Intrinsics.Vector256.Create<byte>( byteSpan[ i..( i += 32 ) ] ) );
                results          += vec1 + vec2;
                if ( modCount == sumEveryBytes ) {
                    vectorFinal += System.Runtime.Intrinsics.Vector256.Sum( results );
                    results     =  Vector256.Create<ushort>( 0 ); // reset
                    modCount    =  0;
                } else {
                    modCount++;
                }
            }
            vectorFinal += System.Runtime.Intrinsics.Vector256.Sum( results );
        }
#if VERIFY_RESULTS
        verifyResult( vectorFinal );
#endif
        return vectorFinal;
    }

    [ Benchmark ]
    public int Vector256_Byte_Ushort_Addition_3( ) {
        const int sumEveryBytes = ( 2 ^ 16 ) / ( 2 ^ 8 ); // = 256
        int       vectorFinal   = 0;
        for ( int iteration = 0 ; iteration < _iterations ; iteration++ ) {
            vectorFinal = 0;
            var               byteSpan = new Span<byte>( _bytes );
            int               i        = 0;
            int               modCount = 0;
            Vector256<ushort> results  = Vector256.Create<ushort>( 0 );
            while ( i < _bytes.Length ) {
                var (vec1, vec2) =  Vector256.Widen( System.Runtime.Intrinsics.Vector256.Create<byte>( byteSpan[ i..( i += 32 ) ] ) );
                results          += vec1 + vec2;
                if ( modCount == sumEveryBytes ) {
                    vectorFinal += System.Runtime.Intrinsics.Vector256.Sum( results );
                    results     =  Vector256.Create<ushort>( 0 ); // reset
                    modCount    =  0;
                    continue;
                }
                modCount++;
            }
            vectorFinal += System.Runtime.Intrinsics.Vector256.Sum( results );
        }
#if VERIFY_RESULTS
        verifyResult( vectorFinal );
#endif
        return vectorFinal;
    }

    [ Benchmark ]
    public int Vector256_Byte_Ushort_Addition_4_FrequentSum( ) {
        int vectorFinal = 0;
        for ( int iteration = 0 ; iteration < _iterations ; iteration++ ) {
            vectorFinal = 0;
            var byteSpan = new Span<byte>( _bytes );
            int i        = 0;
            while ( i < _bytes.Length ) {
                var (vec1, vec2) =  Vector256.Widen( System.Runtime.Intrinsics.Vector256.Create<byte>( byteSpan[ i..( i += 32 ) ] ) );
                vectorFinal      += System.Runtime.Intrinsics.Vector256.Sum( vec1 + vec2 );
            }
        }
#if VERIFY_RESULTS
        verifyResult( vectorFinal );
#endif
        return vectorFinal;
    }

    [ Benchmark ]
    public int Manual_Byte_Addition( ) {
        int manualFinal = 0;
        for ( int iteration = 0 ; iteration < _iterations ; iteration++ ) {
            manualFinal = _bytes[ 0 ];
            for ( int i = 1 ; i < _bytes.Length ; i++ ) {
                manualFinal += _bytes[ i ];
            }
        }
#if VERIFY_RESULTS
        verifyResult( vectorFinal );
#endif
        return manualFinal;
    }
}