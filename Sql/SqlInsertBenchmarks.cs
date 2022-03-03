using System;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;

using Common;

using Microsoft.EntityFrameworkCore;

using NodaTime;

using Npgsql;

using NpgsqlTypes;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags

namespace Benchmarks.Sql;
/*
|                                               Method | ObjectsPerSave | SaveIterations |         Mean |       Error |      StdDev |       Median |       Gen 0 |       Gen 1 |      Gen 2 |    Allocated |
|----------------------------------------------------- |--------------- |--------------- |-------------:|------------:|------------:|-------------:|------------:|------------:|-----------:|-------------:|
|                                         EfCoreInsert |              1 |            100 |     848.3 ms |    29.37 ms |    85.69 ms |     874.5 ms |   1000.0000 |           - |          - |     5,213 KB |
|     EfCoreInsert_NoAutoDetectChanges_NoQueryTracking |              1 |            100 |     809.4 ms |    27.33 ms |    78.85 ms |     835.8 ms |           - |           - |          - |     1,206 KB |
|              NpgSqlInsert_SingularCommand_TypedValue |              1 |            100 |     453.9 ms |    38.65 ms |   113.97 ms |     441.8 ms |           - |           - |          - |       174 KB |
|                     NpgsqlInsert_Batched_Boxed_Value |              1 |            100 |     449.6 ms |    47.24 ms |   132.46 ms |     430.9 ms |           - |           - |          - |       170 KB |
|        NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value |              1 |            100 |     434.4 ms |    42.47 ms |   125.22 ms |     407.0 ms |           - |           - |          - |       169 KB |
|  NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue |              1 |            100 |     441.0 ms |    44.32 ms |   128.57 ms |     402.4 ms |           - |           - |          - |       170 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue2 |              1 |            100 |     431.7 ms |    15.87 ms |    46.55 ms |     446.3 ms |           - |           - |          - |       169 KB |
|                      NpgsqlInsert_Batched_TypedValue |              1 |            100 |     434.8 ms |    41.55 ms |   122.51 ms |     418.8 ms |           - |           - |          - |       168 KB |
|                                           NpgsqlCopy |              1 |            100 |     786.9 ms |    27.22 ms |    80.27 ms |     791.5 ms |           - |           - |          - |       167 KB |
|                                         EfCoreInsert |             10 |            100 |   1,126.2 ms |    54.58 ms |   160.92 ms |   1,141.8 ms |  10000.0000 |   1000.0000 |          - |    47,706 KB |
|     EfCoreInsert_NoAutoDetectChanges_NoQueryTracking |             10 |            100 |     898.9 ms |    24.23 ms |    71.44 ms |     914.1 ms |   1000.0000 |           - |          - |     7,831 KB |
|              NpgSqlInsert_SingularCommand_TypedValue |             10 |            100 |   4,229.8 ms |    84.32 ms |   216.15 ms |   4,230.3 ms |           - |           - |          - |     1,720 KB |
|                     NpgsqlInsert_Batched_Boxed_Value |             10 |            100 |     479.2 ms |    34.27 ms |   101.06 ms |     465.2 ms |           - |           - |          - |     1,206 KB |
|        NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value |             10 |            100 |     460.8 ms |    30.21 ms |    89.08 ms |     447.3 ms |           - |           - |          - |     1,206 KB |
|  NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue |             10 |            100 |     456.8 ms |    30.45 ms |    89.77 ms |     441.1 ms |           - |           - |          - |     1,206 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue2 |             10 |            100 |     452.1 ms |    29.76 ms |    87.75 ms |     436.7 ms |           - |           - |          - |     1,206 KB |
|                      NpgsqlInsert_Batched_TypedValue |             10 |            100 |     493.6 ms |    31.24 ms |    92.11 ms |     492.3 ms |           - |           - |          - |     1,191 KB |
|                                           NpgsqlCopy |             10 |            100 |     778.7 ms |    28.53 ms |    82.78 ms |     797.0 ms |           - |           - |          - |       308 KB |
|                                         EfCoreInsert |            100 |            100 |   2,154.1 ms |    72.21 ms |   212.92 ms |   2,055.1 ms |  91000.0000 |  30000.0000 |  2000.0000 |   472,283 KB |
|     EfCoreInsert_NoAutoDetectChanges_NoQueryTracking |            100 |            100 |   1,217.5 ms |    25.12 ms |    69.61 ms |   1,196.6 ms |  12000.0000 |   3000.0000 |          - |    73,736 KB |
|              NpgSqlInsert_SingularCommand_TypedValue |            100 |            100 |  41,184.8 ms |   821.98 ms | 1,872.06 ms |  41,068.6 ms |   3000.0000 |           - |          - |    17,189 KB |
|                     NpgsqlInsert_Batched_Boxed_Value |            100 |            100 |     835.6 ms |    25.13 ms |    71.71 ms |     859.8 ms |   2000.0000 |           - |          - |    11,648 KB |
|        NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value |            100 |            100 |     820.6 ms |    22.02 ms |    64.93 ms |     837.7 ms |   2000.0000 |           - |          - |    11,647 KB |
|  NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue |            100 |            100 |     841.7 ms |    23.63 ms |    69.30 ms |     856.8 ms |   2000.0000 |           - |          - |    11,646 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue2 |            100 |            100 |     839.5 ms |    24.03 ms |    69.34 ms |     849.5 ms |   2000.0000 |           - |          - |    11,658 KB |
|                      NpgsqlInsert_Batched_TypedValue |            100 |            100 |     842.3 ms |    22.89 ms |    67.49 ms |     859.1 ms |   2000.0000 |           - |          - |    11,490 KB |
|                                           NpgsqlCopy |            100 |            100 |     834.1 ms |    26.46 ms |    78.02 ms |     845.0 ms |           - |           - |          - |     1,714 KB |
|                                         EfCoreInsert |           1000 |            100 |  12,457.6 ms |   116.22 ms |   108.71 ms |  12,476.2 ms | 959000.0000 | 123000.0000 | 22000.0000 | 4,713,280 KB |
|     EfCoreInsert_NoAutoDetectChanges_NoQueryTracking |           1000 |            100 |   6,481.7 ms |   136.94 ms |   401.61 ms |   6,374.2 ms | 114000.0000 |  44000.0000 |  9000.0000 |   728,490 KB |
|              NpgSqlInsert_SingularCommand_TypedValue |           1000 |            100 | 387,248.3 ms | 7,664.15 ms | 8,200.56 ms | 386,677.0 ms |  37000.0000 |   3000.0000 |          - |   171,882 KB |
|                     NpgsqlInsert_Batched_Boxed_Value |           1000 |            100 |   3,850.4 ms |    76.66 ms |   142.09 ms |   3,877.9 ms |  18000.0000 |   9000.0000 |          - |   115,458 KB |
|        NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value |           1000 |            100 |   3,797.8 ms |    75.19 ms |   143.05 ms |   3,829.6 ms |  18000.0000 |   9000.0000 |          - |   115,449 KB |
|  NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue |           1000 |            100 |   3,814.8 ms |    75.83 ms |   156.61 ms |   3,822.0 ms |  18000.0000 |   9000.0000 |          - |   115,454 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue2 |           1000 |            100 |   3,778.9 ms |    75.54 ms |   197.66 ms |   3,793.3 ms |  18000.0000 |   9000.0000 |          - |   115,461 KB |
|                      NpgsqlInsert_Batched_TypedValue |           1000 |            100 |   3,778.0 ms |    75.53 ms |   180.98 ms |   3,805.5 ms |  18000.0000 |   9000.0000 |          - |   113,900 KB |
|                                           NpgsqlCopy |           1000 |            100 |   1,465.1 ms |    37.71 ms |   110.60 ms |   1,441.9 ms |   3000.0000 |           - |          - |    15,777 KB |
 */

[Config(typeof(BenchmarkConfig))]
public class SqlInsertBenchmarks {
    private readonly Instant _testTime = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );

    [ Params( 1, 10, 100, 1000 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public int ObjectsPerSave;

    private int _count = 1;

    /// <summary>
    /// The number of Insert, then save iterations.
    /// Thus the total # of objects inserted per test is
    /// <see cref="SaveIterations"/> * <see cref="ObjectsPerSave"/>
    /// </summary>
    [ Params( 100 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public int SaveIterations;

    private TestObject _newObject => new TestObject {
        Id   = _count,
        Name = "TestObject Name here",
        Integers = new[] {
            1, 2, 3
        },
        Datetime = _testTime
    };


    public SqlInsertBenchmarks( ) {
        ObjectsPerSave = 1;
        SaveIterations = 100;
    }

    // private class Config : ManualConfig {
    //     public Config( ) {
    //         AddColumn( new TagColumn( "Kind", name => name.Substring( 0, 3 ) ) );
    //         AddColumn( new TagColumn( "Number", name => name.Substring( 3 ) ) );
    //     }
    // }


    [ GlobalSetup ]
    public void GlobalSetup( ) {
        using var              db           = new SqlBenchmarksDbContext();
        using NpgsqlConnection dbConnection = db.Database.GetDbConnection() as NpgsqlConnection ?? throw new Exception();
        dbConnection.Open();
        dbConnection.TypeMapper.UseNodaTime();
        using ( var cmd = new NpgsqlCommand() {
                   Connection = dbConnection, CommandText = TestObject.CreateSqlString
               } ) {
            cmd.ExecuteNonQuery();
        }

        _count = 1;
    }

    /********************************************************
     * Benchmarks
     ********************************************************
     */

    [ Benchmark ]
    public void EfCoreInsert( ) {
        var db = new SqlBenchmarksDbContext();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                db.TestObjects.Add( new TestObject {
                    Id   = _count,
                    Name = "TestObject Name here",
                    Integers = new[] {
                        1, 2, 3
                    },
                    Datetime = _testTime
                } );
            }

            db.SaveChanges();
        }
    }

    [ Benchmark ]
    public void EfCoreInsert_NoAutoDetectChanges_NoQueryTracking( ) {
        var db = new SqlBenchmarksDbContext {
            ChangeTracker = {
                AutoDetectChangesEnabled = false, QueryTrackingBehavior = QueryTrackingBehavior.NoTracking
            }
        };
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                db.TestObjects.Add( _newObject );
            }

            db.SaveChanges();
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert" ) ]
    public void NpgSqlInsert_SingularCommand_TypedValue( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                TestObject insertObject = _newObject;
                using var  cmd          = new NpgsqlCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )", connection );
                cmd.Parameters.Add( new NpgsqlParameter<int> {
                    TypedValue = insertObject.Id
                } );
                cmd.Parameters.Add( new NpgsqlParameter<string> {
                    TypedValue = insertObject.Name
                } );
                cmd.Parameters.Add( new NpgsqlParameter<int[]> {
                    TypedValue = insertObject.Integers
                } );
                cmd.Parameters.Add( new NpgsqlParameter<Instant> {
                    TypedValue = insertObject.Datetime
                } );
                cmd.ExecuteNonQuery();
            }
        }
    }


    [ Benchmark ]
    public void NpgsqlInsert_Batched_Boxed_Value( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var batch = new NpgsqlBatch( connection );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                TestObject insertObject = _newObject;
                var        cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )" );
                cmd.Parameters.Add( new NpgsqlParameter {
                    Value = insertObject.Id
                } );
                cmd.Parameters.Add( new NpgsqlParameter {
                    Value = insertObject.Name
                } );
                cmd.Parameters.Add( new NpgsqlParameter {
                    Value = insertObject.Integers
                } );
                cmd.Parameters.Add( new NpgsqlParameter {
                    Value = insertObject.Datetime
                } );
                batch.BatchCommands.Add( cmd );
            }

            batch.ExecuteNonQuery();
        }
    }

    [ Benchmark ]
    public void NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var batch = new NpgsqlBatch( connection );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                TestObject insertObject = _newObject;
                var        cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )" );
                cmd.Parameters.Add( new NpgsqlParameter {
                    Value = insertObject.Id, NpgsqlDbType = NpgsqlDbType.Integer
                } );
                cmd.Parameters.Add( new NpgsqlParameter {
                    Value = insertObject.Name, NpgsqlDbType = NpgsqlDbType.Text
                } );
                cmd.Parameters.Add( new NpgsqlParameter {
                    Value = insertObject.Integers, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer
                } );
                cmd.Parameters.Add( new NpgsqlParameter {
                    Value = insertObject.Datetime, NpgsqlDbType = NpgsqlDbType.TimestampTz
                } );
                batch.BatchCommands.Add( cmd );
            }

            batch.ExecuteNonQuery();
        }
    }

    [ Benchmark ]
    public void NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var batch = new NpgsqlBatch( connection );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                TestObject insertObject = _newObject;
                var        cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )" );
                cmd.Parameters.Add( new NpgsqlParameter {
                    NpgsqlValue = insertObject.Id, NpgsqlDbType = NpgsqlDbType.Integer
                } );
                cmd.Parameters.Add( new NpgsqlParameter {
                    NpgsqlValue = insertObject.Name, NpgsqlDbType = NpgsqlDbType.Text
                } );
                cmd.Parameters.Add( new NpgsqlParameter {
                    NpgsqlValue = insertObject.Integers, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer
                } );
                cmd.Parameters.Add( new NpgsqlParameter {
                    NpgsqlValue = insertObject.Datetime, NpgsqlDbType = NpgsqlDbType.TimestampTz
                } );
                batch.BatchCommands.Add( cmd );
            }

            batch.ExecuteNonQuery();
        }
    }

    [ Benchmark ]
    public void NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue2( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var batch = new NpgsqlBatch( connection );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                TestObject insertObject = _newObject;
                var        cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )" );
                cmd.Parameters.Add( new NpgsqlParameter {
                    NpgsqlValue = insertObject.Id, NpgsqlDbType = NpgsqlDbType.Integer
                } );
                cmd.Parameters.Add( new NpgsqlParameter {
                    NpgsqlValue = insertObject.Name, NpgsqlDbType = NpgsqlDbType.Text
                } );
                cmd.Parameters.Add( new NpgsqlParameter {
                    NpgsqlValue = insertObject.Integers, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer
                } );
                cmd.Parameters.Add( new NpgsqlParameter {
                    NpgsqlValue = insertObject.Datetime, NpgsqlDbType = NpgsqlDbType.TimestampTz
                } );
                batch.BatchCommands.Add( cmd );
            }

            batch.ExecuteNonQuery();
        }
    }

    [ Benchmark ]
    public void NpgsqlInsert_Batched_TypedValue( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var batch = new NpgsqlBatch( connection );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                TestObject insertObject = _newObject;
                var        cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )" );
                cmd.Parameters.Add( new NpgsqlParameter<int> {
                    TypedValue = insertObject.Id
                } );
                cmd.Parameters.Add( new NpgsqlParameter<string> {
                    TypedValue = insertObject.Name
                } );
                cmd.Parameters.Add( new NpgsqlParameter<int[]> {
                    TypedValue = insertObject.Integers
                } );
                cmd.Parameters.Add( new NpgsqlParameter<Instant> {
                    TypedValue = insertObject.Datetime
                } );
                batch.BatchCommands.Add( cmd );
            }

            batch.ExecuteNonQuery();
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Copy" ) ]
    public void NpgsqlCopy( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();

        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var writer = connection.BeginBinaryImport( "COPY public.test_objects (id, name, integers, datetime ) FROM STDIN (FORMAT BINARY)" );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                TestObject insertObject = _newObject;
                writer.StartRow();
                writer.Write( insertObject.Id, NpgsqlDbType.Integer );
                writer.Write( insertObject.Name, NpgsqlDbType.Text );
                writer.Write( insertObject.Integers, NpgsqlDbType.Array | NpgsqlDbType.Integer );
                writer.Write( insertObject.Datetime, NpgsqlDbType.TimestampTz );
            }

            writer.Complete();
        }
    }

    //
    // /// <inheritdoc />
    // public void Dispose( ) {
    //     this.DbConnection.Close();
    //     this.DbConnection.Dispose();
    // }
}
