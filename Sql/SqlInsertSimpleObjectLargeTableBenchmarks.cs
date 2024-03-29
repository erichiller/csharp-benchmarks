﻿using System;
using System.Collections.Generic;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using NpgsqlTypes;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags

namespace Benchmarks.Sql;

[ Config( typeof(BenchmarkConfig) ) ]
public abstract class SqlInsertsSimpleObjectLargeTableBenchmarksBase {
    
    // [ Params( 1, 10, 100, 1000 ) ]
    // [ Params( 1, 2, 5, 10 ) ]
    // [ Params( 1, 2, 10, 100 ) ]
    // [ Params( 2, 10 ) ]
    [ Params( 100 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public int ObjectsPerSave { get; set; }

    /// <summary>
    /// The number of Insert, then save iterations.
    /// Thus the total # of objects inserted per test is
    /// <see cref="SaveIterations"/> * <see cref="ObjectsPerSave"/>
    /// </summary>
    // [ Params( 10 ) ]
    [ Params( 1000 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public int SaveIterations { get; set; }
    
    /// <summary>
    /// The number of Insert, then save iterations.
    /// Thus the total # of objects inserted per test is
    /// <see cref="SaveIterations"/> * <see cref="ObjectsPerSave"/>
    /// </summary>
    [ Params( 
        // 100_000, 1_000_000, 
        100_000_000 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public int InitialTableObjects { get; set; }

}

public class SqlInsertSimpleObjectLargeTableBenchmarks : SqlInsertsSimpleObjectLargeTableBenchmarksBase{

    private int                             _count;
    
    public readonly List<TestObjectPartitionTableB> PreGenerated_TestObjectPartitionTableB;

    [ GlobalSetup(Targets= new []{ nameof(NonPartitionTableSimpleTestObjectCopy) } ) ]
    public void SimpleTestObjectNonPartitionTableSetup( ) {
        using NpgsqlConnection dbConnection = SqlBenchmarksDbContext.GetOpenDbConnection();
        using ( var cmd = new NpgsqlCommand() { Connection = dbConnection, CommandText = SimpleTestObject.CreateSqlString } ) {
            cmd.ExecuteNonQuery();
        }

        _count = 1;
        if ( InitialTableObjects != 0 ) {
            int objectsPerCopy = 1000; /* 1000 objects per COPY */
            if ( InitialTableObjects % objectsPerCopy != 0 ) {
                throw new Exception();
            }
            for ( int o = 0 ; o < ( InitialTableObjects / objectsPerCopy ) ; o++ ) {
                using var writer = dbConnection.BeginBinaryImport( "COPY public.test_objects (id, name, integers, datetime ) FROM STDIN (FORMAT BINARY)" );
                for ( int i = 0 ; i < objectsPerCopy ; i++, _count++ ) {
                    SimpleTestObject insertObject = SimpleTestObject.GetNewObject(_count);
                    writer.StartRow();
                    writer.Write( insertObject.Id, NpgsqlDbType.Integer );
                    writer.Write( insertObject.Name, NpgsqlDbType.Text );
                    writer.Write( insertObject.Integers, NpgsqlDbType.Array | NpgsqlDbType.Integer );
                    writer.Write( insertObject.Datetime, NpgsqlDbType.TimestampTz );
                    
                }

                writer.Complete();
            }
        }
    }
    
    [ Benchmark(Description = "Copy to Non-Partition Table SimpleTestObject") ]
    [ BenchmarkCategory( "Npgsql", "Copy" ) ]
    public void NonPartitionTableSimpleTestObjectCopy( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++ ) {
            using var writer = connection.BeginBinaryImport( "COPY public.test_objects (id, name, integers, datetime ) FROM STDIN (FORMAT BINARY)" );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = SimpleTestObject.GetNewObject(_count);
                writer.StartRow();
                writer.Write( insertObject.Id, NpgsqlDbType.Integer );
                writer.Write( insertObject.Name, NpgsqlDbType.Text );
                writer.Write( insertObject.Integers, NpgsqlDbType.Array | NpgsqlDbType.Integer );
                writer.Write( insertObject.Datetime, NpgsqlDbType.TimestampTz );
            }

            writer.Complete();
        }
    }
    
    
    /* ********************************
     * TestObjectB
     **********************************/
    
    //
    // [ GlobalSetup(Targets= new []{ nameof(NonPartitionTableTestObjectBCopy) } ) ]
    // public void GlobalSetup( ) {
    //     using var              db           = new SqlBenchmarksDbContext();
    //     using NpgsqlConnection dbConnection = db.Database.GetDbConnection() as NpgsqlConnection ?? throw new Exception();
    //     dbConnection.Open();
    //     dbConnection.TypeMapper.UseNodaTime();
    //     using ( var cmd = new NpgsqlCommand() { Connection = dbConnection, CommandText = SimpleTestObject.CreateSqlString } ) {
    //         cmd.ExecuteNonQuery();
    //     }
    //
    //     _count = 1;
    //     if ( InitialTableObjects != 0 ) {
    //         int objectsPerCopy = 1000; /* 1000 objects per COPY */
    //         if ( InitialTableObjects % objectsPerCopy != 0 ) {
    //             throw new Exception();
    //         }
    //         var objects = TestObjectPartitionTableB.GetNewObjects( InitialTableObjects );
    //         for ( int o = 0 ; o < ( InitialTableObjects / objectsPerCopy ) ; o++ ) {
    //             using var writer = dbConnection.BeginBinaryImport( "COPY public.test_objects_b (event_timestamp, event_key, numeric_value, smallint_value ) FROM STDIN (FORMAT BINARY)" );
    //             for ( int i = 0 ; i < objectsPerCopy ; i++, _count++ ) {
    //                 writer.StartRow();
    //                 writer.Write( objects[ _count ].EventTimestamp, NpgsqlDbType.TimestampTz );
    //                 writer.Write( objects[ _count ].EventKey, NpgsqlDbType.Text );
    //                 writer.Write( objects[ _count ].NumericValue, NpgsqlDbType.Numeric );
    //                 writer.Write( objects[ _count ].SmallintValue, NpgsqlDbType.Smallint );
    //                 
    //             }
    //
    //             writer.Complete();
    //         }
    //     }
    // }
    //
    // [ Benchmark(Description = "Copy to Non-Partition Table TestObjectB") ]
    // [ BenchmarkCategory( "Npgsql", "Copy" ) ]
    // public void NonPartitionTableTestObjectBCopy( ) {
    //     using NpgsqlConnection dbConnection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
    //     dbConnection.Open();
    //     dbConnection.TypeMapper.UseNodaTime(); // KILL ?
    //     for ( int o = 0 ; o < SaveIterations ; o++ ) {            
    //         using var writer = dbConnection.BeginBinaryImport( "COPY public.partition_table_b (event_timestamp, event_key, numeric_value, smallint_value ) FROM STDIN (FORMAT BINARY)" );
    //         for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
    //             writer.StartRow();
    //             writer.Write( objects[objectCount].EventTimestamp, NpgsqlDbType.TimestampTz );
    //             writer.Write( objects[objectCount].EventKey, NpgsqlDbType.Text );
    //             writer.Write( objects[objectCount].NumericValue, NpgsqlDbType.Numeric );
    //             writer.Write( objects[objectCount].SmallintValue, NpgsqlDbType.Smallint );
    //         }
    //         writer.Complete();
    //     }
    // }
    
    
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
    
    
    /* Partition Tables */
    
    

    [ GlobalSetup (Targets = new []{ nameof(NpgsqlCopyToPartitionTable)}) ]
    public void PartitionTableSetup( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();

        _count = 1;
        if ( InitialTableObjects != 0 ) {
            _count = SimpleTestObject.CreateAndPopulatePartitionTableIfNotExists( connection, startId: _count, createCount: InitialTableObjects );
        }
    }

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

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Copy", "PartitionTable" ) ]
    public void NpgsqlCopyToPartitionTable( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++ ) {
            using var writer = connection.BeginBinaryImport( "COPY public.test_object_partition_table (id, name, integers, datetime ) FROM STDIN (FORMAT BINARY)" );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = SimpleTestObject.GetNewObject( _count );
                writer.StartRow();
                writer.Write( insertObject.Id, NpgsqlDbType.Integer );
                writer.Write( insertObject.Name, NpgsqlDbType.Text );
                writer.Write( insertObject.Integers, NpgsqlDbType.Array | NpgsqlDbType.Integer );
                writer.Write( insertObject.Datetime, NpgsqlDbType.TimestampTz );
            }

            writer.Complete();
        }
    }
}