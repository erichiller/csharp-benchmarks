using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.General;

[ Config( typeof(BenchmarkConfig) ) ]
public class StringByteArrayParsingBenchmarks {
    internal static Memory<byte> MessageInputBuffer = new Memory<byte>( new byte[] {
        /*
        0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08, 0x00, 0x45, 0x00,
        0x00, 0x89, 0x79, 0xfc, 0x40, 0x00, 0x40, 0x06, 0xc2, 0x70, 0x7f, 0x00, 0x00, 0x01, 0x7f, 0x00,
        0x00, 0x01, 0x1d, 0x48, 0xe5, 0x72, 0x7f, 0x8f, 0x51, 0x10, 0x89, 0x79, 0x08, 0xa4, 0x80, 0x18,
        0x02, 0x00, 0xfe, 0x7d, 0x00, 0x00, 0x01, 0x01, 0x08, 0x0a, 0x36, 0xce, 0xe2, 0xdb, 0x36, 0xce,
        0xe2, 0xdb, 
        */
        // @formatter:keep_existing_initializer_arrangement true
        0x00, 0x00, 0x00, 0x2e, 0x39, 0x39, 0x00, 0x31, 0x39, 0x30, 0x30, 0x33, 0x00, 0x33, 0x00, 0x31,
        0x36, 0x37, 0x35, 0x33, 0x34, 0x33, 0x33, 0x33, 0x33, 0x00, 0x34, 0x31, 0x35, 0x34, 0x2e, 0x35,
        0x30, 0x00, 0x34, 0x31, 0x35, 0x34, 0x2e, 0x37, 0x35, 0x00, 0x32, 0x31, 0x00, 0x32, 0x35, 0x00,
        0x30, 0x00,

        0x00, 0x00, 0x00, 0x1f, 0x39, 0x39, 0x00, 0x31, 0x39, 0x30, 0x30, 0x34, 0x00, 0x34, 0x00, 0x31,
        0x36, 0x37, 0x35, 0x33, 0x34, 0x33, 0x33, 0x33, 0x33, 0x00, 0x34, 0x31, 0x35, 0x34, 0x2e, 0x36,
        0x32, 0x35, 0x00,
        // @formatter:keep_existing_initializer_arrangement restore
    } );


    private static T getFieldValue<T>( ref Memory<byte> remainingBuffer, ref int bytesRead ) where T : INumberBase<T>, IAdditionOperators<T, T, T> {
        int endOfField = remainingBuffer.Span.IndexOf( ( byte )0 );
        T   result     = T.Parse( System.Text.Encoding.ASCII.GetString( remainingBuffer.Span.Slice( 0, endOfField ) ), NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture );
        remainingBuffer =  remainingBuffer.Slice( endOfField + 1 );
        bytesRead       += endOfField;
        return result;
    }

    private static int getFieldInt( ref Memory<byte> remainingBuffer, ref int bytesRead ) {
        int value      = 0;
        int endOfField = remainingBuffer.Span.IndexOf( ( byte )0 );
        for ( int i = endOfField - 1 ; i >= 0 ; i-- ) {
            value += ( remainingBuffer.Span[ i ] - 0x30 ) * ( int )Math.Pow( 10, endOfField - i - 1 );
        }
        remainingBuffer =  remainingBuffer.Slice( endOfField + 1 );
        bytesRead       += endOfField;
        return value;
    }

    private static long getFieldLong( ref Memory<byte> remainingBuffer, ref int bytesRead ) {
        long value      = 0;
        int  endOfField = remainingBuffer.Span.IndexOf( ( byte )0 );
        for ( int i = endOfField - 1 ; i >= 0 ; i-- ) {
            value += ( remainingBuffer.Span[ i ] - 0x30 ) * ( long )Math.Pow( 10, endOfField - i - 1 );
        }
        remainingBuffer =  remainingBuffer.Slice( endOfField + 1 );
        bytesRead       += endOfField;
        return value;
    }

    private const byte _plusSign     = 0x2b;
    private const byte _minusSign    = 0x2d;
    private const byte _decimalPlace = 0x2e;

    internal static decimal GetFieldDecimal( ref Memory<byte> remainingBuffer, ref int bytesRead ) {
        int     whole         = 0;
        int     fractional    = 0;
        int     decimalPlaces = 0;
        bool    isNegative    = false;

        int position = 0;
        if ( remainingBuffer.Span[ position ] is _minusSign ) {
            isNegative = true;
            position++;
        } else if ( remainingBuffer.Span[ position ] is _plusSign ) {
            position++;
        }

        while ( remainingBuffer.Span[ position ] != 0x00 && position < 10 ) {
            if ( remainingBuffer.Span[ position ] is _decimalPlace ) {
                position++;
                decimalPlaces = 1;
                continue;
            }
            if ( decimalPlaces != 0 ) {
                fractional = ( fractional * 10 ) + ( remainingBuffer.Span[ position ] - 0x30 );
                decimalPlaces++;
            } else {
                whole = ( whole * 10 ) + ( remainingBuffer.Span[ position ] - 0x30 );
            }
            // Console.WriteLine($"position=[{position}] {System.Text.Encoding.ASCII.GetString(remainingBuffer.Span.Slice(position,1))} (0x{remainingBuffer.Span[ position ]:x}) ; whole={whole} ; fractional={fractional} ; decimalPlaces={decimalPlaces}");
            position++;
        }
        decimal value = fractional > 0 ? new decimal( whole + ( fractional / Math.Pow( 10, decimalPlaces - 1 ) ) ) : new decimal( whole );
        if ( isNegative ) {
            value *= -1;
        }
        remainingBuffer =  remainingBuffer.Slice( position + 1 );
        bytesRead       += position;
        return value;
    }

    internal static int GetFieldIntV2( ref Memory<byte> remainingBuffer, ref int bytesRead ) {
        bool isNegative = false;
        int  value      = 0;

        int position = 0;
        if ( remainingBuffer.Span[ position ] is _minusSign ) {
            isNegative = true;
            position++;
        } else if ( remainingBuffer.Span[ position ] is _plusSign ) {
            position++;
        }

        while ( remainingBuffer.Span[ position ] != 0x00 && position < 10 ) {
            value = ( value * 10 ) + ( remainingBuffer.Span[ position ] - 0x30 );
            // Console.WriteLine($"position=[{position}] {System.Text.Encoding.ASCII.GetString(remainingBuffer.Span.Slice(position,1))} (0x{remainingBuffer.Span[ position ]:x}) ");
            position++;
        }
        if ( isNegative ) {
            value *= -1;
        }
        remainingBuffer =  remainingBuffer.Slice( position + 1 );
        bytesRead       += position;
        return value;
    }

    internal static long GetFieldLongV2( ref Memory<byte> remainingBuffer, ref int bytesRead ) {
        bool isNegative = false;
        long value      = 0;

        int position = 0;
        if ( remainingBuffer.Span[ position ] is _minusSign ) {
            isNegative = true;
            position++;
        } else if ( remainingBuffer.Span[ position ] is _plusSign ) {
            position++;
        }

        while ( remainingBuffer.Span[ position ] != 0x00 && position < 10 ) {
            value = ( value * 10 ) + ( remainingBuffer.Span[ position ] - 0x30 );
            // Console.WriteLine($"position=[{position}] {System.Text.Encoding.ASCII.GetString(remainingBuffer.Span.Slice(position,1))} (0x{remainingBuffer.Span[ position ]:x}) ");
            position++;
        }
        if ( isNegative ) {
            value *= -1;
        }
        remainingBuffer =  remainingBuffer.Slice( position + 1 );
        bytesRead       += position;
        return value;
    }

    [SuppressMessage("ReSharper", "NotAccessedPositionalProperty.Global")]
    public record Message( int MessageType, int RequestId, int TickType, long UnixTimeSeconds, decimal Price );


    private const int _iterations = 10_000;

    [ Benchmark ]
    public Message? ParseIntManual( ) {
        Message? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = MessageInputBuffer;
            int          currentMessageStart = 0;
            while ( buffer.Length > currentMessageStart + sizeof(int) ) {
                int length = ( buffer.Span[ currentMessageStart ]       << 24 )
                             + ( buffer.Span[ currentMessageStart + 1 ] << 16 )
                             + ( buffer.Span[ currentMessageStart + 2 ] << 8 )
                             + buffer.Span[ currentMessageStart + 3 ];
                Memory<byte> remainingBuffer = buffer.Slice( currentMessageStart + sizeof(int) );
                if ( remainingBuffer.Span.Length < length ) {
                    throw new Exception();
                }
                int     parsedLength    = 0;
                int     messageType     = getFieldInt( ref remainingBuffer, ref parsedLength );
                int     requestId       = getFieldInt( ref remainingBuffer, ref parsedLength );
                int     tickType        = getFieldInt( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = getFieldValue<long>( ref remainingBuffer, ref parsedLength );
                decimal price           = getFieldValue<decimal>( ref remainingBuffer, ref parsedLength );
                message             =  new Message( messageType, requestId, tickType, unixTimeSeconds, price );
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }

    // Definitely slower!
    [ Benchmark( Baseline = true ) ]
    public Message? NumberInterfaceParse( ) {
        Message? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = MessageInputBuffer;
            int          currentMessageStart = 0;
            while ( buffer.Length > currentMessageStart + sizeof(int) ) {
                int length = ( buffer.Span[ currentMessageStart ]       << 24 )
                             + ( buffer.Span[ currentMessageStart + 1 ] << 16 )
                             + ( buffer.Span[ currentMessageStart + 2 ] << 8 )
                             + buffer.Span[ currentMessageStart + 3 ];
                Memory<byte> remainingBuffer = buffer.Slice( currentMessageStart + sizeof(int) );
                if ( remainingBuffer.Span.Length < length ) {
                    throw new Exception();
                }
                int     parsedLength    = 0;
                int     messageType     = getFieldValue<int>( ref remainingBuffer, ref parsedLength );
                int     requestId       = getFieldValue<int>( ref remainingBuffer, ref parsedLength );
                int     tickType        = getFieldValue<int>( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = getFieldValue<long>( ref remainingBuffer, ref parsedLength );
                decimal price           = getFieldValue<decimal>( ref remainingBuffer, ref parsedLength );
                message             =  new Message( messageType, requestId, tickType, unixTimeSeconds, price );
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }

    [ Benchmark ]
    public Message? ParseDecimalManual( ) {
        Message? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = MessageInputBuffer;
            int          currentMessageStart = 0;
            while ( buffer.Length > currentMessageStart + sizeof(int) ) {
                int length = ( buffer.Span[ currentMessageStart ]       << 24 )
                             + ( buffer.Span[ currentMessageStart + 1 ] << 16 )
                             + ( buffer.Span[ currentMessageStart + 2 ] << 8 )
                             + buffer.Span[ currentMessageStart + 3 ];
                Memory<byte> remainingBuffer = buffer.Slice( currentMessageStart + sizeof(int) );
                if ( remainingBuffer.Span.Length < length ) {
                    throw new Exception();
                }
                int     parsedLength    = 0;
                int     messageType     = getFieldValue<int>( ref remainingBuffer, ref parsedLength );
                int     requestId       = getFieldValue<int>( ref remainingBuffer, ref parsedLength );
                int     tickType        = getFieldValue<int>( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = getFieldValue<long>( ref remainingBuffer, ref parsedLength );
                decimal price           = GetFieldDecimal( ref remainingBuffer, ref parsedLength );
                message             =  new Message( messageType, requestId, tickType, unixTimeSeconds, price );
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }

    [ Benchmark ]
    public Message? ParseLongManual( ) {
        Message? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = MessageInputBuffer;
            int          currentMessageStart = 0;
            while ( buffer.Length > currentMessageStart + sizeof(int) ) {
                int length = ( buffer.Span[ currentMessageStart ]       << 24 )
                             + ( buffer.Span[ currentMessageStart + 1 ] << 16 )
                             + ( buffer.Span[ currentMessageStart + 2 ] << 8 )
                             + buffer.Span[ currentMessageStart + 3 ];
                Memory<byte> remainingBuffer = buffer.Slice( currentMessageStart + sizeof(int) );
                if ( remainingBuffer.Span.Length < length ) {
                    throw new Exception();
                }
                int     parsedLength    = 0;
                int     messageType     = getFieldValue<int>( ref remainingBuffer, ref parsedLength );
                int     requestId       = getFieldValue<int>( ref remainingBuffer, ref parsedLength );
                int     tickType        = getFieldValue<int>( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = getFieldLong( ref remainingBuffer, ref parsedLength );
                decimal price           = getFieldValue<decimal>( ref remainingBuffer, ref parsedLength );
                message             =  new Message( messageType, requestId, tickType, unixTimeSeconds, price );
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }

    [ Benchmark ]
    public Message? ParseLongManualV2( ) {
        Message? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = MessageInputBuffer;
            int          currentMessageStart = 0;
            while ( buffer.Length > currentMessageStart + sizeof(int) ) {
                int length = ( buffer.Span[ currentMessageStart ]       << 24 )
                             + ( buffer.Span[ currentMessageStart + 1 ] << 16 )
                             + ( buffer.Span[ currentMessageStart + 2 ] << 8 )
                             + buffer.Span[ currentMessageStart + 3 ];
                Memory<byte> remainingBuffer = buffer.Slice( currentMessageStart + sizeof(int) );
                if ( remainingBuffer.Span.Length < length ) {
                    throw new Exception();
                }
                int     parsedLength    = 0;
                int     messageType     = getFieldValue<int>( ref remainingBuffer, ref parsedLength );
                int     requestId       = getFieldValue<int>( ref remainingBuffer, ref parsedLength );
                int     tickType        = getFieldValue<int>( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = GetFieldLongV2( ref remainingBuffer, ref parsedLength );
                decimal price           = getFieldValue<decimal>( ref remainingBuffer, ref parsedLength );
                message             =  new Message( messageType, requestId, tickType, unixTimeSeconds, price );
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }

    [ Benchmark ]
    public Message? ParseIntManualV2( ) {
        Message? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = MessageInputBuffer;
            int          currentMessageStart = 0;
            while ( buffer.Length > currentMessageStart + sizeof(int) ) {
                int length = ( buffer.Span[ currentMessageStart ]       << 24 )
                             + ( buffer.Span[ currentMessageStart + 1 ] << 16 )
                             + ( buffer.Span[ currentMessageStart + 2 ] << 8 )
                             + buffer.Span[ currentMessageStart + 3 ];
                Memory<byte> remainingBuffer = buffer.Slice( currentMessageStart + sizeof(int) );
                if ( remainingBuffer.Span.Length < length ) {
                    throw new Exception();
                }
                int     parsedLength    = 0;
                int     messageType     = GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     requestId       = GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     tickType        = GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = getFieldValue<long>( ref remainingBuffer, ref parsedLength );
                decimal price           = getFieldValue<decimal>( ref remainingBuffer, ref parsedLength );
                message             =  new Message( messageType, requestId, tickType, unixTimeSeconds, price );
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }

    [ Benchmark ]
    public Message? ParseManualAll( ) {
        Message? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = MessageInputBuffer;
            int          currentMessageStart = 0;
            while ( buffer.Length > currentMessageStart + sizeof(int) ) {
                int length = ( buffer.Span[ currentMessageStart ]       << 24 )
                             + ( buffer.Span[ currentMessageStart + 1 ] << 16 )
                             + ( buffer.Span[ currentMessageStart + 2 ] << 8 )
                             + buffer.Span[ currentMessageStart + 3 ];
                Memory<byte> remainingBuffer = buffer.Slice( currentMessageStart + sizeof(int) );
                if ( remainingBuffer.Span.Length < length ) {
                    throw new Exception();
                }
                int     parsedLength    = 0;
                int     messageType     = GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     requestId       = GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     tickType        = GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = GetFieldLongV2( ref remainingBuffer, ref parsedLength );
                decimal price           = GetFieldDecimal( ref remainingBuffer, ref parsedLength );
                message             =  new Message( messageType, requestId, tickType, unixTimeSeconds, price );
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }
}