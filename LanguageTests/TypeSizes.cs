using System.Runtime.InteropServices;

using Benchmarks.LanguageTests;

using Microsoft.Extensions.Logging;

using NodaTime;

using Xunit;
using Xunit.Abstractions;

// ReSharper disable UnusedMember.Global
// ReSharper disable NotAccessedPositionalProperty.Global
// ReSharper disable InconsistentNaming
// ReSharper disable MemberCanBePrivate.Global

namespace LanguageTests;

public class TypeSizes : TestBase<CompilerConditionalTests> {
    /// <inheritdoc />
    public TypeSizes( ITestOutputHelper? output, ILogger? logger = null ) : base( output, logger ) { }

    [Fact]
    private void TestTypeSizes( ) {
        writeTestResults<int>( sizeOf<int?>() );
        writeTestResults<long>( sizeOf<long?>() );
        writeTestResults<float>( sizeOf<float?>() );
        writeTestResults<double>( sizeOf<double?>() );
        writeTestResults<decimal>( sizeOf<decimal?>() );
		writeTestResults<Interval>( sizeOf<Interval?>() );

        writeTestFromArrayResults<int?>();
        writeTestFromArrayResults<double?>();
        writeTestFromArrayResults<decimal?>();

        this._writeLine( String.Empty.PadLeft( 100, '*' ) );

        writeTestResults<RecordStruct_Long_Short_ByteEnum_ByteEnum_NullableDouble_NullableDecimal>();
        writeTestResults<RecordStruct_Long>();
        writeTestResults<Struct_Long_Short_ByteEnum_ByteEnum_NullableDouble_NullableDecimal>();
        writeTestResults<RecordStruct_Long_Short>();
        writeTestResults<RecordStruct_Long_Long>();
        writeTestResults<RecordStruct_Long_Decimal>();
        writeTestResults<RecordStruct_Instant_Short_ByteEnum_ByteEnum_NullableDouble_NullableDecimal>();
        writeTestResults<RecordStruct_Instant_Short_ByteEnum_>();
        writeTestResults<RecordStruct_Instant_Short_ByteEnum_ByteEnum_>();
        writeTestResults<RecordStruct_Instant_Short_ByteEnum_ByteEnum_NullableDouble>();
        writeTestResults<RecordStruct_Instant_Short_NullableDouble_NullableDecimal>();
        writeTestResults<RecordStruct_Instant_Short_Double>();
        writeTestResults<RecordStruct_Instant_Short_NullableDouble>();
    }

    private void writeTestResults<T>( double? nullableSize = null ) {
        this._writeLine( $"{typeof( T ).Name,-80} | {Marshal.SizeOf<T>(),-4} bytes | {nullableSize} bytes" ); // NOTE: can't call sizeOf<T?> from here, it strips the ? off
    }
    private void writeTestFromArrayResults<T>( ) {
        const int size = 1000000;
        long b1 = GC.GetTotalMemory(true);
        T[] array1 = new T[size];
        long b2 = GC.GetTotalMemory(true);
        array1[ 0 ] = default;
        this._output.WriteLine( "{0,-80} | {1} bytes", typeof( T ), Math.Round( ( b2 - b1 ) / ( double )size, 4 ) );
    }
    private static double sizeOf<T>( int roundDecimalPlaces = 3 ) {
        const int size = 1000000;
        long b1 = GC.GetTotalMemory(true);
        T[] array1 = new T[size];
        long b2 = GC.GetTotalMemory(true);
        array1[ 0 ] = default;
        return Math.Round( ( b2 - b1 ) / ( double )size, roundDecimalPlaces );
    }
}

public readonly record struct RecordStruct_Instant_Short_ByteEnum_ByteEnum_NullableDouble_NullableDecimal( Instant TimeProp, short ShortProp, ByteEnum ByteEnum1, ByteEnum ByteEnum2, double? DoubleProp, decimal? DecimalProp ); // 64 bytes

public readonly record struct RecordStruct_Long_Short_ByteEnum_ByteEnum_NullableDouble_NullableDecimal( long TimeProp, short ShortProp, ByteEnum ByteEnum1, ByteEnum ByteEnum, double? DoubleProp, decimal? DecimalProp ); // this drops it to 56 bytes

public readonly record struct RecordStruct_Instant_Short_ByteEnum_( Instant TimeProp, short ShortProp, ByteEnum ByteEnum1 ); // 24 bytes

public readonly record struct RecordStruct_Instant_Short_ByteEnum_ByteEnum_( Instant TimeProp, short ShortProp, ByteEnum ByteEnum1, ByteEnum ByteEnum2 ); // 24 bytes

public readonly record struct RecordStruct_Instant_Short_ByteEnum_ByteEnum_NullableDouble( Instant TimeProp, short ShortProp, ByteEnum ByteEnum1, ByteEnum ByteEnum2, double? DoubleProp ); // 40 bytes

public readonly record struct RecordStruct_Instant_Short_NullableDouble_NullableDecimal( Instant TimeProp, short ShortProp, double? DoubleProp, decimal? DecimalProp ); // 64 bytes

public readonly record struct RecordStruct_Instant_Short_Double( Instant TimeProp, short ShortProp, double DoubleProp ); // 32 bytes

public readonly record struct RecordStruct_Instant_Short_NullableDouble( Instant TimeProp, short ShortProp, double? DoubleProp ); // 40 bytes

public readonly record struct RecordStruct_Long( long TimeProp ); // 8 bytes

public readonly struct Struct_Long_Short_ByteEnum_ByteEnum_NullableDouble_NullableDecimal {
    // also 56 bytes
    public readonly long     TimeProp;
    public readonly short    ShortProp;
    public readonly ByteEnum ByteEnum1;
    public readonly ByteEnum ByteEnum2;
    public readonly double?  DoubleProp;
    public readonly decimal? DecimalProp;

    public Struct_Long_Short_ByteEnum_ByteEnum_NullableDouble_NullableDecimal( long timeProp, short shortProp, ByteEnum byteEnum2, ByteEnum byteEnum1, decimal? decimalProp, double? doubleProp ) {
        TimeProp = timeProp;
        ShortProp = shortProp;
        ByteEnum2 = byteEnum2;
        ByteEnum1 = byteEnum1;
        DecimalProp = decimalProp;
        DoubleProp = doubleProp;
    }
}; // this drops it to 56 bytes

public readonly record struct RecordStruct_Long_Short( long TimeProp, short ShortProp ); // 16 bytes

public readonly record struct RecordStruct_Long_Long( long TimeProp, long ShortProp ); // 16 bytes

public readonly record struct RecordStruct_Long_Decimal( long TimeProp, decimal ShortProp ); // 24 bytes

public enum ByteEnum : byte {
    Value1 = 1,
    Value2 = 2,
    Value3 = 3
}
