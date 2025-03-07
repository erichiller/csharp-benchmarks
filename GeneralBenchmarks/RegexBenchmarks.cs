using System;
using System.Linq;
using System.Text.RegularExpressions;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.General;
/*
 * | Method                     | MaxStringLength | Mean [us] | Error [us] | StdDev [us] | Gen0   | Allocated [KB] |
   |--------------------------- |---------------- |----------:|-----------:|------------:|-------:|---------------:|
   | IsNumericForLoop_FastExit2 | 3               |  1.392 us |  0.0273 us |   0.0355 us |      - |              - |
   | IsNumericForLoop_FastExit3 | 3               |  1.474 us |  0.0071 us |   0.0063 us |      - |              - |
   | IsNumericForLoop           | 3               |  1.485 us |  0.0177 us |   0.0166 us |      - |              - |
   | IsNumericForLoop_Binary    | 3               |  1.552 us |  0.0042 us |   0.0039 us |      - |              - |
   | IsNumericForLoop_FastExit  | 3               |  1.607 us |  0.0319 us |   0.0380 us |      - |              - |
   | IsNumericForLoop_FastExit2 | 6               |  1.929 us |  0.0112 us |   0.0105 us |      - |              - |
   | IsNumericForLoop_FastExit3 | 6               |  2.095 us |  0.0097 us |   0.0091 us |      - |              - |
   | IsNumericForLoop_Binary    | 6               |  2.158 us |  0.0182 us |   0.0170 us |      - |              - |
   | IsNumericForLoop_FastExit  | 6               |  2.259 us |  0.0077 us |   0.0072 us |      - |              - |
   | IsNumericForLoop           | 6               |  2.357 us |  0.0144 us |   0.0135 us |      - |              - |
   | IsNumericForLoop_FastExit2 | 9               |  2.457 us |  0.0107 us |   0.0100 us |      - |              - |
   | IsNumericForLoop_Binary    | 9               |  2.607 us |  0.0164 us |   0.0153 us |      - |              - |
   | IsNumericForLoop_FastExit3 | 9               |  2.805 us |  0.0092 us |   0.0082 us |      - |              - |
   | IsNumericForLoop_FastExit  | 9               |  2.857 us |  0.0132 us |   0.0124 us |      - |              - |
   | IsNumericForLoop           | 9               |  3.287 us |  0.0181 us |   0.0161 us |      - |              - |
   | IsNumeric_Int32TryParse    | 3               |  7.963 us |  0.0248 us |   0.0207 us |      - |              - |
   | IsNumeric_UInt32TryParse   | 3               |  8.448 us |  0.0365 us |   0.0342 us |      - |              - |
   | IsNumeric_Int32TryParse    | 6               |  9.225 us |  0.0307 us |   0.0272 us |      - |              - |
   | IsNumeric_UInt32TryParse   | 6               | 10.105 us |  0.0280 us |   0.0262 us |      - |              - |
   | IsNumeric_Int32TryParse    | 9               | 10.638 us |  0.0353 us |   0.0330 us |      - |              - |
   | IsNumeric_UInt32TryParse   | 9               | 11.099 us |  0.0251 us |   0.0235 us |      - |              - |
   | IsNumericLinqAll           | 3               | 18.564 us |  0.3645 us |   0.3743 us | 6.7749 |       31.25 KB |
   | IsNumericLinqAll           | 6               | 19.254 us |  0.1161 us |   0.1086 us | 6.7749 |       31.25 KB |
   | IsNumericLinqAll           | 9               | 20.868 us |  0.1334 us |   0.1248 us | 6.7749 |       31.25 KB |
   | IsNumeric_RegexIsMatch     | 3               | 31.604 us |  0.0242 us |   0.0227 us |      - |              - |
   | IsNumeric_RegexIsMatch     | 6               | 33.604 us |  0.1319 us |   0.1234 us |      - |              - |
   | IsNumeric_RegexIsMatch     | 9               | 35.025 us |  0.0306 us |   0.0271 us |      - |              - |
   
 */

// ReSharper disable ForCanBeConvertedToForeach
[ Config( typeof(BenchmarkConfig) ) ]
public partial class RegexBenchmarks {
    [ Params( 3, 6, 9 ) ]
    public int MaxStringLength;

    private const    int      _genStrings  = 1000;
    private readonly string[] _testStrings = new string[ _genStrings ];

    [ GlobalSetup ]
    public void CreateByteArray( ) {
        for ( int i = 0 ; i < _genStrings ; i++ ) {
            var stringLength = System.Random.Shared.Next( 0, MaxStringLength );
            if ( stringLength == 0 ) {
                _testStrings[ i ] = String.Empty;
                continue;
            }
            Span<char> charArray = new Span<char>( new char[ stringLength ] );
            for ( var c = 0 ; c < stringLength ; c++ ) {
                if ( i % 2 == 0 ) {
                    charArray[ c ] = ( char )System.Random.Shared.Next( ( int )'0', ( int )'9' + 1 );
                } else {
                    charArray[ c ] = ( char )System.Random.Shared.Next( ( int )'0', ( int )'z' + 1 );
                }
            }
            _testStrings[ i ] = charArray.ToString();
        }
    }

    [ Benchmark ]
    public bool IsNumericLinqAll( ) {
        bool allNumeric = false;
        // ReSharper disable once ForCanBeConvertedToForeach
        for ( int i = 0 ; i < _testStrings.Length ; i++ ) {
            allNumeric &= isNumeric( _testStrings[ i ] );
        }
        return allNumeric;

        static bool isNumeric( string str ) {
            return str.All( c => c >= 48 && c <= 57 );
        }
    }

    [ Benchmark ]
    public bool IsNumericForLoop( ) {
        bool allNumeric = false;
        for ( int i = 0 ; i < _testStrings.Length ; i++ ) {
            allNumeric &= isNumeric( _testStrings[ i ] );
        }
        return allNumeric;

        static bool isNumeric( string str ) {
            bool result = str.Length == 0;
            for ( int i = 0 ; i < str.Length ; i++ ) {
                result &= str[ i ] >= 48 && str[ i ] <= 57;
            }
            return result;
        }
    }

    [ Benchmark ]
    public bool IsNumericForLoop_FastExit( ) {
        bool allNumeric = false;
        for ( int i = 0 ; i < _testStrings.Length ; i++ ) {
            allNumeric &= isNumeric( _testStrings[ i ] );
        }
        return allNumeric;

        static bool isNumeric( string str ) {
            for ( int i = 0 ; i < str.Length ; i++ ) {
                if ( str[ i ] < 48 || str[ i ] > 57 ) {
                    return false;
                }
            }
            return true;
        }
    }

    [ Benchmark ]
    public bool IsNumericForLoop_FastExit2( ) {
        bool allNumeric = false;
        for ( int i = 0 ; i < _testStrings.Length ; i++ ) {
            allNumeric &= isNumeric( _testStrings[ i ] );
        }
        return allNumeric;

        static bool isNumeric( string str ) {
            for ( int i = 0 ; i < str.Length ; i++ ) {
                if ( str[ i ] > 57 || str[ i ] < 48 ) {
                    return false;
                }
            }
            return true;
        }
    }

    [ Benchmark ]
    public bool IsNumericForLoop_FastExit3( ) {
        bool allNumeric = false;
        for ( int i = 0 ; i < _testStrings.Length ; i++ ) {
            allNumeric &= isNumeric( _testStrings[ i ] );
        }
        return allNumeric;

        static bool isNumeric( string str ) {
            for ( int i = 0 ; i < str.Length ; i++ ) {
                if ( str[ i ] > 57 ) {
                    return false;
                }
                if ( str[ i ] < 48 ) {
                    return false;
                }
            }
            return true;
        }
    }

    [ Benchmark ]
    public bool IsNumericForLoop_Binary( ) {
        bool allNumeric = false;
        for ( int i = 0 ; i < _testStrings.Length ; i++ ) {
            allNumeric &= isNumeric( _testStrings[ i ] );
        }
        return allNumeric;

        static bool isNumeric( string str ) {
            for ( int i = 0 ; i < str.Length ; i++ ) {
                if ( ( str[ i ] & 0b_1111_1111_1111_0000 ) != 0b_0011_0000 ) {
                    // URGENT: incomplete - this returns true for more than 0-9
                    return false;
                }
            }
            return true;
        }
    }

    [ Benchmark ]
    public bool IsNumeric_Int32TryParse( ) {
        bool allNumeric = false;
        for ( int i = 0 ; i < _testStrings.Length ; i++ ) {
            allNumeric &= isNumeric( _testStrings[ i ] );
        }
        return allNumeric;

        static bool isNumeric( string str ) {
            return str.Length <= 9 && Int32.TryParse( str, out _ ); // less than or equal to length 9 else it might be larger than Int32.MaxValue
        }
    }

    [ Benchmark ]
    public bool IsNumeric_UInt32TryParse( ) {
        bool allNumeric = false;
        for ( int i = 0 ; i < _testStrings.Length ; i++ ) {
            allNumeric &= isNumeric( _testStrings[ i ] );
        }
        return allNumeric;

        static bool isNumeric( string str ) {
            return str.Length <= 9 && UInt32.TryParse( str, out _ ); // less than or equal to length 9 else it might be larger than Int32.MaxValue
        }
    }

    [ Benchmark ]
    public bool IsNumeric_RegexIsMatch( ) {
        bool allNumeric = false;
        for ( int i = 0 ; i < _testStrings.Length ; i++ ) {
            allNumeric &= isNumeric( _testStrings[ i ] );
        }
        return allNumeric;
        
        static bool isNumeric( string str ) => 
            isNumericPattern().IsMatch( str );
    }
    
    [ GeneratedRegex( "^[0-9]+$" ) ]
    private static partial Regex isNumericPattern( );
}
