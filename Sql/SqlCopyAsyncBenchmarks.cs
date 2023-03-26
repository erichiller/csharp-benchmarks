using System;
using System.Threading.Tasks;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

using Microsoft.EntityFrameworkCore;

using NodaTime;

using Npgsql;

using NpgsqlTypes;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags

namespace Benchmarks.Sql;

[ Config( typeof(BenchmarkConfig) ) ]
public class SqlCopyAsyncBenchmarks {
    private readonly Instant _testTime = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );

    // [ Params( 1, 10, 100, 1000 ) ]
    // [ Params( 1, 2, 5, 10 ) ]
    [ Params( 1, 2, 10, 100 ) ]
    // [ Params( 2, 10 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public virtual int ObjectsPerSave { get; set; }

    private int _count = 1;

    /// <summary>
    /// The number of Insert, then save iterations.
    /// Thus the total # of objects inserted per test is
    /// <see cref="SaveIterations"/> * <see cref="ObjectsPerSave"/>
    /// </summary>
    // [ Params( 10 ) ]
    [ Params( 100 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public virtual int SaveIterations { get; set; }
    
    /// <summary>
    /// The number of Insert, then save iterations.
    /// Thus the total # of objects inserted per test is
    /// <see cref="SaveIterations"/> * <see cref="ObjectsPerSave"/>
    /// </summary>
    // [ Params( 10 ) ]
    [ Params( 100_000, 1_000_000, 10_000_000 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public int InitialTableObjects { get; set; }

    // /// <summary>
    // /// Complex Object if true.
    // /// </summary>
    // [ Params( false, true ) ]
    // // ReSharper disable once MemberCanBePrivate.Global
    // // ReSharper disable once FieldCanBeMadeReadOnly.Global
    // public bool ComplexObject;

    private SimpleTestObject _newObject => new SimpleTestObject {
        Id       = _count,
        Name     = "TestObject Name here",
        Integers = new[] { 1, 2, 3 },
        Datetime = _testTime
    };
    // TODO
    // private ComplexTestObject _newComplexObject => new ComplexTestObject {
    //     Id                   = _count,
    //     NullableString       = "TestObject Name here",
    //     NullableIntegerArray = new[] { 1, 2, 3 },
    //     Datetime             = _testTime,
    //     Decimal              = 0.100m,
    //     Float                = 0.001f,
    //     Integer              = 10,
    //     NullableDecimal      = 0.200m,
    //     NullableInteger      = 2
    // };


    [ GlobalSetup ]
    public void GlobalSetup( ) {
        using NpgsqlConnection dbConnection = SqlBenchmarksDbContext.GetDbConnection();
        using ( var cmd = new NpgsqlCommand() { Connection = dbConnection, CommandText = SimpleTestObject.CreateSqlString } ) {
            cmd.ExecuteNonQuery();
        }

        SimpleTestObject.CreatePartitionTable( dbConnection );
        
        _count = 1;
        int objectsPerCopy = 1000; /* 1000 objects per COPY */
        for ( int o = 0 ; o < ( InitialTableObjects / 1000 ) ; o++, _count++ ) {
            using var writer         = dbConnection.BeginBinaryImport( "COPY public.test_objects (id, name, integers, datetime ) FROM STDIN (FORMAT BINARY)" );
            for ( int i = 0 ; i < objectsPerCopy  ; i++, _count++ ) {
                SimpleTestObject insertObject = _newObject;
                writer.StartRow();
                writer.Write( insertObject.Id, NpgsqlDbType.Integer );
                writer.Write( insertObject.Name, NpgsqlDbType.Text );
                writer.Write( insertObject.Integers, NpgsqlDbType.Array | NpgsqlDbType.Integer );
                writer.Write( insertObject.Datetime, NpgsqlDbType.TimestampTz );
            }

            writer.Complete();
        }
    }
    
    
    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Copy", "Async" ) ]
    public async Task NpgsqlCopyAsync( ) {
        await using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();

        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            await using var writer = await connection.BeginBinaryImportAsync( "COPY public.test_objects (id, name, integers, datetime ) FROM STDIN (FORMAT BINARY)" );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = _newObject;
                await writer.StartRowAsync();
                await writer.WriteAsync( insertObject.Id, NpgsqlDbType.Integer );
                await writer.WriteAsync( insertObject.Name, NpgsqlDbType.Text );
                await writer.WriteAsync( insertObject.Integers, NpgsqlDbType.Array | NpgsqlDbType.Integer );
                await writer.WriteAsync( insertObject.Datetime, NpgsqlDbType.TimestampTz );
            }

            await writer.CompleteAsync();
        }
    }
    
    
    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Copy", "AdditionalWork" ) ]
    public void NpgsqlCopyWithWork( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        var rand = new Random();

        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var writer = connection.BeginBinaryImport( "COPY public.test_objects (id, name, integers, datetime ) FROM STDIN (FORMAT BINARY)" );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = _newObject;
                writer.StartRow();
                writer.Write( insertObject.Id, NpgsqlDbType.Integer );
                writer.Write( insertObject.Name, NpgsqlDbType.Text );
                writer.Write( insertObject.Integers, NpgsqlDbType.Array | NpgsqlDbType.Integer );
                writer.Write( insertObject.Datetime, NpgsqlDbType.TimestampTz );
            }

            int m = 0;
            for ( int i = 0 ; i < 100_000 ; i++ ) {
                m = m * rand.Next(1,999_999) % rand.Next( 1, 9 );
            }

            writer.Complete();
        }
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Copy", "AdditionalWork", "Async" ) ]
    public async Task NpgsqlCopyWithWorkAsync( ) {
        await using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        var rand = new Random();

        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            await using var writer = await connection.BeginBinaryImportAsync( "COPY public.test_objects (id, name, integers, datetime ) FROM STDIN (FORMAT BINARY)" );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = _newObject;
                await writer.StartRowAsync();
                await writer.WriteAsync( insertObject.Id, NpgsqlDbType.Integer );
                await writer.WriteAsync( insertObject.Name, NpgsqlDbType.Text );
                await writer.WriteAsync( insertObject.Integers, NpgsqlDbType.Array | NpgsqlDbType.Integer );
                await writer.WriteAsync( insertObject.Datetime, NpgsqlDbType.TimestampTz );
            }

            int m = 0;
            for ( int i = 0 ; i < 100_000 ; i++ ) {
                m = m * rand.Next(1,999_999) % rand.Next( 1, 9 );
            }

            await writer.CompleteAsync();
        }
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Copy", "AdditionalWork", "Async" ) ]
    public async Task NpgsqlCopyWithWorkAsyncConfigureAwaitFalse( ) {
        await using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        var rand = new Random();

        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            await using var writer = await connection.BeginBinaryImportAsync( "COPY public.test_objects (id, name, integers, datetime ) FROM STDIN (FORMAT BINARY)" );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = _newObject;
                await writer.StartRowAsync().ConfigureAwait( false );
                await writer.WriteAsync( insertObject.Id, NpgsqlDbType.Integer ).ConfigureAwait( false );;
                await writer.WriteAsync( insertObject.Name, NpgsqlDbType.Text ).ConfigureAwait( false );;
                await writer.WriteAsync( insertObject.Integers, NpgsqlDbType.Array | NpgsqlDbType.Integer ).ConfigureAwait( false );;
                await writer.WriteAsync( insertObject.Datetime, NpgsqlDbType.TimestampTz ).ConfigureAwait( false );;
            }

            int m = 0;
            for ( int i = 0 ; i < 100_000 ; i++ ) {
                m = m * rand.Next(1,999_999) % rand.Next( 1, 9 );
            }

            await writer.CompleteAsync().ConfigureAwait( false );;
        }
    }
}