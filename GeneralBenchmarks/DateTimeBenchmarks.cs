using System;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

using NodaTime;

namespace Benchmarks.General;

/*
 * As of 2023 February 07
 * 
    |                                     Method | Mean [ms] | Error [ms] | StdDev [ms] | Ratio | RatioSD |      Gen0 | Allocated [B] | Alloc Ratio |
    |------------------------------------------- |----------:|-----------:|------------:|------:|--------:|----------:|--------------:|------------:|
    |                                  ParseOnly |  42.03 ms |   0.422 ms |    0.395 ms |  1.00 |    0.00 | 2666.6667 |    12800078 B |        1.00 |
    |                     ParseWithDateTimeTicks |  46.12 ms |   0.163 ms |    0.153 ms |  1.10 |    0.01 | 2636.3636 |    12800085 B |        1.00 |
    |                       ParseWithDateTimeUtc |  46.37 ms |   0.080 ms |    0.075 ms |  1.10 |    0.01 | 2636.3636 |    12800085 B |        1.00 |
    |  ParseWithNodaTimeZonedClockInstantToTicks |  49.40 ms |   0.383 ms |    0.359 ms |  1.18 |    0.02 | 2636.3636 |    12800085 B |        1.00 |
    |        ParseWithNodaTimeSystemClockInstant |  49.60 ms |   0.132 ms |    0.124 ms |  1.18 |    0.01 | 3000.0000 |    14400085 B |        1.12 |
    |         ParseWithNodaTimeZonedClockInstant |  49.78 ms |   0.345 ms |    0.322 ms |  1.18 |    0.01 | 3000.0000 |    14400085 B |        1.12 |
    | ParseWithNodaTimeSystemClockInstantToTicks |  49.86 ms |   0.297 ms |    0.278 ms |  1.19 |    0.01 | 2636.3636 |    12800085 B |        1.00 |
    |   ParseWithNodaTimeZonedClockZonedDateTime |  58.24 ms |   0.110 ms |    0.103 ms |  1.39 |    0.01 | 3333.3333 |    16000104 B |        1.25 |
    |                          ParseWithDateTime |  62.64 ms |   0.120 ms |    0.101 ms |  1.49 |    0.01 | 2625.0000 |    12800117 B |        1.00 |
 */

[ Config( typeof(BenchmarkConfig) ) ]
public class DateTimeBenchmarks {
    private const int _iterations = 100_000;

    private static readonly ZonedClock _zonedClock = new NodaTime.ZonedClock(
        NodaTime.SystemClock.Instance,
        NodaTime.DateTimeZoneProviders.Tzdb.GetSystemDefault(),
        NodaTime.CalendarSystem.Iso );


    [ SuppressMessage( "ReSharper", "NotAccessedPositionalProperty.Global" ) ]
    public record MessageWithTicksTimestamp( int MessageType, int RequestId, int TickType, long UnixTimeSeconds, decimal Price, long ReceivedTime );

    [ SuppressMessage( "ReSharper", "NotAccessedPositionalProperty.Global" ) ]
    public record MessageWithDateTime( int MessageType, int RequestId, int TickType, long UnixTimeSeconds, decimal Price, DateTime ReceivedTime );

    [ SuppressMessage( "ReSharper", "NotAccessedPositionalProperty.Global" ) ]
    public record MessageWithInstant( int MessageType, int RequestId, int TickType, long UnixTimeSeconds, decimal Price, Instant ReceivedTime );

    [ SuppressMessage( "ReSharper", "NotAccessedPositionalProperty.Global" ) ]
    public record MessageWithZonedDateTime( int MessageType, int RequestId, int TickType, long UnixTimeSeconds, decimal Price, ZonedDateTime ReceivedTime );

    [ Benchmark( Baseline = true ) ]
    public MessageWithTicksTimestamp? ParseOnly( ) {
        MessageWithTicksTimestamp? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = StringByteArrayParsingBenchmarks.MessageInputBuffer;
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
                int     messageType     = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     requestId       = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     tickType        = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = StringByteArrayParsingBenchmarks.GetFieldLongV2( ref remainingBuffer, ref parsedLength );
                decimal price           = StringByteArrayParsingBenchmarks.GetFieldDecimal( ref remainingBuffer, ref parsedLength );
                message             =  new MessageWithTicksTimestamp( messageType, requestId, tickType, unixTimeSeconds, price, 0 );
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }

    [ Benchmark ]
    public MessageWithTicksTimestamp? ParseWithDateTimeTicks( ) {
        MessageWithTicksTimestamp? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = StringByteArrayParsingBenchmarks.MessageInputBuffer;
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
                int     messageType     = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     requestId       = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     tickType        = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = StringByteArrayParsingBenchmarks.GetFieldLongV2( ref remainingBuffer, ref parsedLength );
                decimal price           = StringByteArrayParsingBenchmarks.GetFieldDecimal( ref remainingBuffer, ref parsedLength );
                message             =  new (messageType, requestId, tickType, unixTimeSeconds, price, System.DateTime.UtcNow.Ticks);
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }

    [ Benchmark ]
    public MessageWithDateTime? ParseWithDateTime( ) {
        MessageWithDateTime? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = StringByteArrayParsingBenchmarks.MessageInputBuffer;
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
                int     messageType     = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     requestId       = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     tickType        = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = StringByteArrayParsingBenchmarks.GetFieldLongV2( ref remainingBuffer, ref parsedLength );
                decimal price           = StringByteArrayParsingBenchmarks.GetFieldDecimal( ref remainingBuffer, ref parsedLength );
                message             =  new (messageType, requestId, tickType, unixTimeSeconds, price, System.DateTime.Now);
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }

    [ Benchmark ]
    public MessageWithDateTime? ParseWithDateTimeUtc( ) {
        MessageWithDateTime? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = StringByteArrayParsingBenchmarks.MessageInputBuffer;
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
                int     messageType     = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     requestId       = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     tickType        = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = StringByteArrayParsingBenchmarks.GetFieldLongV2( ref remainingBuffer, ref parsedLength );
                decimal price           = StringByteArrayParsingBenchmarks.GetFieldDecimal( ref remainingBuffer, ref parsedLength );
                message             =  new (messageType, requestId, tickType, unixTimeSeconds, price, System.DateTime.UtcNow);
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }

    [ Benchmark ]
    public MessageWithTicksTimestamp? ParseWithNodaTimeSystemClockInstantToTicks( ) {
        MessageWithTicksTimestamp? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = StringByteArrayParsingBenchmarks.MessageInputBuffer;
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
                int     messageType     = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     requestId       = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     tickType        = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = StringByteArrayParsingBenchmarks.GetFieldLongV2( ref remainingBuffer, ref parsedLength );
                decimal price           = StringByteArrayParsingBenchmarks.GetFieldDecimal( ref remainingBuffer, ref parsedLength );
                message             =  new (messageType, requestId, tickType, unixTimeSeconds, price, NodaTime.SystemClock.Instance.GetCurrentInstant().ToUnixTimeTicks());
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }

    [ Benchmark ]
    public MessageWithInstant? ParseWithNodaTimeSystemClockInstant( ) {
        MessageWithInstant? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = StringByteArrayParsingBenchmarks.MessageInputBuffer;
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
                int     messageType     = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     requestId       = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     tickType        = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = StringByteArrayParsingBenchmarks.GetFieldLongV2( ref remainingBuffer, ref parsedLength );
                decimal price           = StringByteArrayParsingBenchmarks.GetFieldDecimal( ref remainingBuffer, ref parsedLength );
                message             =  new (messageType, requestId, tickType, unixTimeSeconds, price, NodaTime.SystemClock.Instance.GetCurrentInstant());
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }

    [ Benchmark ]
    public MessageWithTicksTimestamp? ParseWithNodaTimeZonedClockInstantToTicks( ) {
        MessageWithTicksTimestamp? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = StringByteArrayParsingBenchmarks.MessageInputBuffer;
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
                int     messageType     = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     requestId       = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     tickType        = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = StringByteArrayParsingBenchmarks.GetFieldLongV2( ref remainingBuffer, ref parsedLength );
                decimal price           = StringByteArrayParsingBenchmarks.GetFieldDecimal( ref remainingBuffer, ref parsedLength );
                message             =  new (messageType, requestId, tickType, unixTimeSeconds, price, _zonedClock.GetCurrentInstant().ToUnixTimeTicks());
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }

    [ Benchmark ]
    public MessageWithInstant? ParseWithNodaTimeZonedClockInstant( ) {
        MessageWithInstant? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = StringByteArrayParsingBenchmarks.MessageInputBuffer;
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
                int     messageType     = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     requestId       = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     tickType        = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = StringByteArrayParsingBenchmarks.GetFieldLongV2( ref remainingBuffer, ref parsedLength );
                decimal price           = StringByteArrayParsingBenchmarks.GetFieldDecimal( ref remainingBuffer, ref parsedLength );
                message             =  new (messageType, requestId, tickType, unixTimeSeconds, price, _zonedClock.GetCurrentInstant());
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }

    [ Benchmark ]
    public MessageWithZonedDateTime? ParseWithNodaTimeZonedClockZonedDateTime( ) {
        MessageWithZonedDateTime? message = null;
        for ( int i = 0 ; i < _iterations ; i++ ) {
            Memory<byte> buffer              = StringByteArrayParsingBenchmarks.MessageInputBuffer;
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
                int     messageType     = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     requestId       = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                int     tickType        = StringByteArrayParsingBenchmarks.GetFieldIntV2( ref remainingBuffer, ref parsedLength );
                long    unixTimeSeconds = StringByteArrayParsingBenchmarks.GetFieldLongV2( ref remainingBuffer, ref parsedLength );
                decimal price           = StringByteArrayParsingBenchmarks.GetFieldDecimal( ref remainingBuffer, ref parsedLength );
                message             =  new (messageType, requestId, tickType, unixTimeSeconds, price, _zonedClock.GetCurrentZonedDateTime());
                currentMessageStart += sizeof(int) + length;
            }
        }
        return message;
    }
}