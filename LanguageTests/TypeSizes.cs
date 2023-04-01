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

    [ Fact ]
    private void TestTypeSizes( ) {
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

    private void writeTestResults<T>( ) {
        this._writeLine( $"{typeof(T).Name,-80} : {Marshal.SizeOf<T>()} bytes" );
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
        TimeProp    = timeProp;
        ShortProp   = shortProp;
        ByteEnum2   = byteEnum2;
        ByteEnum1   = byteEnum1;
        DecimalProp = decimalProp;
        DoubleProp  = doubleProp;
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