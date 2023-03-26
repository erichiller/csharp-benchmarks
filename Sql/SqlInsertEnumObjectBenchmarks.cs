using System;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

using NodaTime;

using Npgsql;

using NpgsqlTypes;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags

namespace Benchmarks.Sql;

/* Confirm inserted objects:
SELECT 'MAIN' AS name, count(*) FROM public.test_object_partition_table
UNION
SELECT 'part_1', count(*) FROM public.part_1
UNION
SELECT 'part_2', count(*) FROM public.part_2
UNION
SELECT 'part_3', count(*) FROM public.part_3
UNION
SELECT 'part_4', count(*) FROM public.part_4
UNION
SELECT 'part_5', count(*) FROM public.part_5
UNION
SELECT 'part_6', count(*) FROM public.part_6
UNION
SELECT 'default', count(*) FROM public.part_default
UNION
SELECT 'non-partition', count(*) FROM public.test_objects
ORDER BY name desc;
 */

[ Config( typeof(BenchmarkConfig) ) ]
public class SqlInsertEnumObjectBenchmarks {
    // [ Params( 1, 10, 100, 1000 ) ]
    // [ Params( 1, 2, 5, 10 ) ]
    [ Params( 1, 2, 10, 100 ) ]
    // [ Params( 2, 100 ) ]
    // [ Params( 10 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public int ObjectsPerSave { get; set; }

    /// <summary>
    /// The number of Insert, then save iterations.
    /// Thus the total # of objects inserted per test is
    /// <see cref="SaveIterations"/> * <see cref="ObjectsPerSave"/>
    /// </summary>
    // [ Params( 10 ) ]
    // [ Params( 100 ) ]
    // [ Params( 1000 ) ]
    [ Params( 10, 100 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public int SaveIterations { get; set; }

    private int _count = 1;

    // TODO: make partition table be a [Params(true, false)]

    [ GlobalSetup ]
    public void GlobalSetup( ) {
        using NpgsqlConnection dbConnection = SqlBenchmarksDbContext.GetDbConnection();
        using ( var cmd = new NpgsqlCommand() { Connection = dbConnection, CommandText = EnumTestObject.CreateSqlString } ) {
            cmd.ExecuteNonQuery();
        }
        // SimpleTestObject.CreatePartitionTable( dbConnection );
    }

    // private class Config : ManualConfig {
    //     public Config( ) {
    //         AddColumn( new TagColumn( "Kind", name => name.Substring( 0, 3 ) ) );
    //         AddColumn( new TagColumn( "Number", name => name.Substring( 3 ) ) );
    //     }
    // }

    /********************************************************
     * Benchmarks
     ********************************************************
     */

    /*
    |                                                          Method | ObjectsPerSave | SaveIterations | Mean [ms] | Error [ms] | StdDev [ms] | Allocated [B] |
    |---------------------------------------------------------------- |--------------- |--------------- |----------:|-----------:|------------:|--------------:|
    | NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue_EnumUnknown |             10 |            100 |  125.9 ms |    1.74 ms |     1.63 ms |     1733982 B |
    |   NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue_EnumTyped |             10 |            100 |  127.2 ms |    1.72 ms |     1.61 ms |     1832142 B |
     */

    [ Benchmark ]
    // [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "Boxed", "NpgsqlDbType" ) ]
    public void NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue_EnumUnknown( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var batch = new NpgsqlBatch( connection );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                EnumTestObject insertObject = EnumTestObject.GetNewObject( _count );
                var            cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_enum_objects ( id, name, integers, datetime, test_enum ) VALUES ( $1, $2, $3, $4, $5 )" );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue = insertObject.Id, NpgsqlDbType       = NpgsqlDbType.Integer } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue = insertObject.Name, NpgsqlDbType     = NpgsqlDbType.Text } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue = insertObject.Integers, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue = insertObject.Datetime, NpgsqlDbType = NpgsqlDbType.TimestampTz } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue = insertObject.TestEnum, NpgsqlDbType = NpgsqlDbType.Unknown } );
                batch.BatchCommands.Add( cmd );
            }

            if ( batch.ExecuteNonQuery() != ObjectsPerSave ) {
                throw new Exception();
            }
        }
    }

    [ Benchmark ]
    // [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "Boxed", "NpgsqlDbType" ) ]
    public void NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue_EnumTyped( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var batch = new NpgsqlBatch( connection );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                EnumTestObject insertObject = EnumTestObject.GetNewObject( _count );
                var            cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_enum_objects ( id, name, integers, datetime, test_enum ) VALUES ( $1, $2, $3, $4, $5 )" );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue          = insertObject.Id, NpgsqlDbType       = NpgsqlDbType.Integer } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue          = insertObject.Name, NpgsqlDbType     = NpgsqlDbType.Text } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue          = insertObject.Integers, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue          = insertObject.Datetime, NpgsqlDbType = NpgsqlDbType.TimestampTz } );
                cmd.Parameters.Add( new NpgsqlParameter<TestEnum> { TypedValue = insertObject.TestEnum /*, this won't work ->  NpgsqlDbType = NpgsqlDbType.Unknown */ } );
                batch.BatchCommands.Add( cmd );
            }

            if ( batch.ExecuteNonQuery() != ObjectsPerSave ) {
                throw new Exception();
            }
        }
    }

    [ Benchmark ]
    // [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "Boxed", "NpgsqlDbType" ) ]
    public void NpgsqlInsert_Batched_Typed_NpgsqlDbType_TypedValue_EnumTyped( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var batch = new NpgsqlBatch( connection );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                EnumTestObject insertObject = EnumTestObject.GetNewObject( _count );
                var            cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_enum_objects ( id, name, integers, datetime, test_enum ) VALUES ( $1, $2, $3, $4, $5 )" );
                cmd.Parameters.Add( new NpgsqlParameter<int> { TypedValue      = insertObject.Id, NpgsqlDbType       = NpgsqlDbType.Integer } );
                cmd.Parameters.Add( new NpgsqlParameter<string> { TypedValue   = insertObject.Name, NpgsqlDbType     = NpgsqlDbType.Text } );
                cmd.Parameters.Add( new NpgsqlParameter<int[]> { TypedValue    = insertObject.Integers, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer } );
                cmd.Parameters.Add( new NpgsqlParameter<Instant> { TypedValue  = insertObject.Datetime, NpgsqlDbType = NpgsqlDbType.TimestampTz } );
                cmd.Parameters.Add( new NpgsqlParameter<TestEnum> { TypedValue = insertObject.TestEnum } );
                batch.BatchCommands.Add( cmd );
            }


            if ( batch.ExecuteNonQuery() != ObjectsPerSave ) {
                throw new Exception();
            }
        }
    }

    [ Benchmark ]
    // [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "Boxed", "NpgsqlDbType" ) ]
    public void NpgsqlInsert_Batched_Typed_TypedValue_EnumTyped( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var batch = new NpgsqlBatch( connection );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                EnumTestObject insertObject = EnumTestObject.GetNewObject( _count );
                var            cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_enum_objects ( id, name, integers, datetime, test_enum ) VALUES ( $1, $2, $3, $4, $5 )" );
                cmd.Parameters.Add( new NpgsqlParameter<int> { TypedValue      = insertObject.Id, } );
                cmd.Parameters.Add( new NpgsqlParameter<string> { TypedValue   = insertObject.Name } );
                cmd.Parameters.Add( new NpgsqlParameter<int[]> { TypedValue    = insertObject.Integers } );
                cmd.Parameters.Add( new NpgsqlParameter<Instant> { TypedValue  = insertObject.Datetime } );
                cmd.Parameters.Add( new NpgsqlParameter<TestEnum> { TypedValue = insertObject.TestEnum } );
                batch.BatchCommands.Add( cmd );
            }


            if ( batch.ExecuteNonQuery() != ObjectsPerSave ) {
                throw new Exception();
            }
        }
    }
}