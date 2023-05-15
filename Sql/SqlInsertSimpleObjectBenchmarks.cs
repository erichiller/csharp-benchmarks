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
public class SqlInsertSimpleObjectBenchmarks {

    // [ Params( 1, 10, 100, 1000 ) ]
    // [ Params( 1, 2, 5, 10 ) ]
    // [ Params( 1, 2, 10, 100 ) ]
    // [ Params( 2, 100 ) ]
    [ Params( 10 ) ]
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
    [ Params( 1000 ) ]
    // [ Params( 10, 100 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public int SaveIterations { get; set; }

    protected int _count = 1;

    // TODO: make partition table be a [Params(true, false)]

    [ GlobalSetup ]
    public void GlobalSetup( ) {
        using NpgsqlConnection dbConnection = SqlBenchmarksDbContext.GetOpenDbConnection();
        using ( var cmd = new NpgsqlCommand() { Connection = dbConnection, CommandText = SimpleTestObject.CreateSqlString } ) {
            cmd.ExecuteNonQuery();
        }
        SimpleTestObject.CreatePartitionTable( dbConnection );
    }

    private SimpleTestObject getNewObject( ) => SimpleTestObject.GetNewObject( _count );

    // TODO: make partition table be a [Params(true, false)]


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

    [ Benchmark ]
    [ BenchmarkCategory( "EfCore", "Insert" ) ]
    public void EfCoreInsert( ) {
        var db = new SqlBenchmarksDbContext();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                db.TestObjects.Add( getNewObject() );
            }

            db.SaveChanges();
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "EfCore", "Insert", "NoQueryTracking" ) ]
    public void EfCoreInsert_NoAutoDetectChanges_NoQueryTracking( ) {
        var db = new SqlBenchmarksDbContext { ChangeTracker = { AutoDetectChangesEnabled = false, QueryTrackingBehavior = QueryTrackingBehavior.NoTracking } };
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                db.TestObjects.Add( getNewObject() );
            }

            db.SaveChanges();
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Singular", "Typed" ) ]
    public void NpgSqlInsert_SingularCommand_TypedValue( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = getNewObject();
                using var        cmd          = new NpgsqlCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )", connection );
                cmd.Parameters.Add( new NpgsqlParameter<int> { TypedValue     = insertObject.Id } );
                cmd.Parameters.Add( new NpgsqlParameter<string> { TypedValue  = insertObject.Name } );
                cmd.Parameters.Add( new NpgsqlParameter<int[]> { TypedValue   = insertObject.Integers } );
                cmd.Parameters.Add( new NpgsqlParameter<Instant> { TypedValue = insertObject.Datetime } );
                cmd.ExecuteNonQuery();
            }
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Singular", "Typed", "Prepared" ) ]
    public void NpgSqlInsert_SingularCommand_TypedValue_Prepared( ) {
        using NpgsqlConnection connection    = SqlBenchmarksDbContext.GetOpenDbConnection();
        using var              cmd           = new NpgsqlCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )", connection );
        var                    idParam       = new NpgsqlParameter<int>();
        var                    nameParam     = new NpgsqlParameter<string>();
        var                    integersParam = new NpgsqlParameter<int[]>();
        var                    datetimeParam = new NpgsqlParameter<Instant>();
        cmd.Parameters.Add( idParam );
        cmd.Parameters.Add( nameParam );
        cmd.Parameters.Add( integersParam );
        cmd.Parameters.Add( datetimeParam );
        cmd.Prepare();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = getNewObject();
                idParam.TypedValue       = insertObject.Id;
                nameParam.TypedValue     = insertObject.Name;
                integersParam.TypedValue = insertObject.Integers;
                datetimeParam.TypedValue = insertObject.Datetime;
                cmd.ExecuteNonQuery();
            }
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Singular", "Boxed", "NpgsqlValue" ) ]
    public void NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = getNewObject();
                using var        cmd          = new NpgsqlCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )", connection );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, NpgsqlValue                      = insertObject.Id } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Text, NpgsqlValue                         = insertObject.Name } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer, NpgsqlValue = insertObject.Integers } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.TimestampTz, NpgsqlValue                  = insertObject.Datetime } );
                cmd.ExecuteNonQuery();
            }
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Singular", "Boxed", "NpgsqlValue", "Prepared" ) ]
    public void NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        using var              cmd        = new NpgsqlCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )", connection );
        NpgsqlParameter[] parameters = new[] {
            cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer } ),
            cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Text } ),
            cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer } ),
            cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.TimestampTz } )
        };
        cmd.Prepare();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = getNewObject();
                parameters[ 0 ].NpgsqlValue = insertObject.Id;
                parameters[ 1 ].NpgsqlValue = insertObject.Name;
                parameters[ 2 ].NpgsqlValue = insertObject.Integers;
                parameters[ 3 ].NpgsqlValue = insertObject.Datetime;
                cmd.ExecuteNonQuery();
            }
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Singular", "Boxed", "NpgsqlValue", "Prepared", "Async" ) ]
    public async Task NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared_Async( ) {
        await using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        await using var        cmd        = new NpgsqlCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )", connection );
        NpgsqlParameter[] parameters = new[] {
            cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer } ),
            cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Text } ),
            cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer } ),
            cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.TimestampTz } )
        };
        await cmd.PrepareAsync();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = getNewObject();
                parameters[ 0 ].NpgsqlValue = insertObject.Id;
                parameters[ 1 ].NpgsqlValue = insertObject.Name;
                parameters[ 2 ].NpgsqlValue = insertObject.Integers;
                parameters[ 3 ].NpgsqlValue = insertObject.Datetime;
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }


    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "Boxed", "Value" ) ]
    public void NpgsqlInsert_Batched_Boxed_Value( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var batch = new NpgsqlBatch( connection );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = getNewObject();
                var              cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )" );
                cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Id } );
                cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Name } );
                cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Integers } );
                cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Datetime } );
                batch.BatchCommands.Add( cmd );
            }

            batch.ExecuteNonQuery();
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "Boxed", "NpgsqlDbType" ) ]
    public void NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var batch = new NpgsqlBatch( connection );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = getNewObject();
                var              cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )" );
                cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Id, NpgsqlDbType       = NpgsqlDbType.Integer } );
                cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Name, NpgsqlDbType     = NpgsqlDbType.Text } );
                cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Integers, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer } );
                cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Datetime, NpgsqlDbType = NpgsqlDbType.TimestampTz } );
                batch.BatchCommands.Add( cmd );
            }

            batch.ExecuteNonQuery();
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "Boxed", "NpgsqlDbType" ) ]
    public void NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value_LessDefinedVars( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var batch = new NpgsqlBatch( connection );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = getNewObject();
                batch.BatchCommands.Add( new NpgsqlBatchCommand {
                    CommandText = @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )",
                    Parameters = {
                        new NpgsqlParameter { Value = insertObject.Id, NpgsqlDbType       = NpgsqlDbType.Integer },
                        new NpgsqlParameter { Value = insertObject.Name, NpgsqlDbType     = NpgsqlDbType.Text },
                        new NpgsqlParameter { Value = insertObject.Integers, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer },
                        new NpgsqlParameter { Value = insertObject.Datetime, NpgsqlDbType = NpgsqlDbType.TimestampTz }
                    }
                } );
            }

            batch.ExecuteNonQuery();
        }
    }
    // [ Benchmark ]
    // [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "Boxed", "NpgsqlDbType" ) ]
    // public void NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue_LessDefinedVars( ) {
    //     using NpgsqlConnection connection = SqlBenchmarksDbContext.GetDbConnection();
    //     for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
    //         using var batch = new NpgsqlBatch( connection );
    //         for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
    //             SimpleTestObject insertObject = getNewObject();
    //             batch.BatchCommands.Add( new NpgsqlBatchCommand {
    //                 CommandText = @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )",
    //                 Parameters = {
    //                     new NpgsqlParameter { NpgsqlValue = insertObject.Id, NpgsqlDbType       = NpgsqlDbType.Integer },
    //                     new NpgsqlParameter { NpgsqlValue = insertObject.Name, NpgsqlDbType     = NpgsqlDbType.Text },
    //                     new NpgsqlParameter { NpgsqlValue = insertObject.Integers, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer },
    //                     new NpgsqlParameter { NpgsqlValue = insertObject.Datetime, NpgsqlDbType = NpgsqlDbType.TimestampTz }
    //                 }
    //             } );
    //         }
    //
    //         batch.ExecuteNonQuery();
    //     }
    // }

    /* Note: -- this does not work:
     * ---> System.InvalidOperationException: The parameter already belongs to a collection
     */
    // [ Benchmark ]
    // [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "Boxed", "NpgsqlDbType" ) ]
    // public void NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value_LessDefinedVars_ReuseNpgsqlParameter( ) {
    //     using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
    //     connection.Open();
    //     connection.TypeMapper.UseNodaTime();
    //
    //     var npgsqlParameters = new NpgsqlParameter[] {
    //         new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer },
    //         new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Text },
    //         new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer },
    //         new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.TimestampTz }
    //     };
    //     for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
    //         using var batch = new NpgsqlBatch( connection );
    //         for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
    //             SimpleTestObject insertObject = getNewObject();
    //             npgsqlParameters[ 0 ].Value = insertObject.Id;
    //             npgsqlParameters[ 1 ].Value = insertObject.Name;
    //             npgsqlParameters[ 2 ].Value = insertObject.Integers;
    //             npgsqlParameters[ 3 ].Value = insertObject.Datetime;
    //             batch.BatchCommands.Add( new NpgsqlBatchCommand {
    //                                          CommandText = @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )",
    //                                          Parameters = {  npgsqlParameters[ 0 ], npgsqlParameters[ 1 ],  npgsqlParameters[ 2 ],  npgsqlParameters[ 3 ] }
    //                                      } );
    //         }
    //
    //         batch.ExecuteNonQuery();
    //     }
    // }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "Boxed", "NpgsqlValue" ) ]
    public void NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var batch = new NpgsqlBatch( connection );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = getNewObject();
                var              cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )" );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue = insertObject.Id, NpgsqlDbType       = NpgsqlDbType.Integer } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue = insertObject.Name, NpgsqlDbType     = NpgsqlDbType.Text } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue = insertObject.Integers, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue = insertObject.Datetime, NpgsqlDbType = NpgsqlDbType.TimestampTz } );
                batch.BatchCommands.Add( cmd );
            }
            batch.ExecuteNonQuery();
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "TypedValue" ) ]
    public void NpgsqlInsert_Batched_TypedValue( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var batch = new NpgsqlBatch( connection );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = getNewObject();
                var              cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )" );
                cmd.Parameters.Add( new NpgsqlParameter<int> { TypedValue     = insertObject.Id } );
                cmd.Parameters.Add( new NpgsqlParameter<string> { TypedValue  = insertObject.Name } );
                cmd.Parameters.Add( new NpgsqlParameter<int[]> { TypedValue   = insertObject.Integers } );
                cmd.Parameters.Add( new NpgsqlParameter<Instant> { TypedValue = insertObject.Datetime } );
                batch.BatchCommands.Add( cmd );
            }

            batch.ExecuteNonQuery();
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Copy" ) ]
    public void NpgsqlCopy( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var writer = connection.BeginBinaryImport( "COPY public.test_objects (id, name, integers, datetime ) FROM STDIN (FORMAT BINARY)" );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = getNewObject();
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
    [ BenchmarkCategory( "Npgsql", "Copy" ) ]
    public void NpgsqlCopyWithTypesAsString( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var writer = connection.BeginBinaryImport( "COPY public.test_objects (id, name, integers, datetime ) FROM STDIN (FORMAT BINARY)" );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = getNewObject();
                writer.StartRow();
                writer.Write( insertObject.Id, dataTypeName: "integer" );
                writer.Write( insertObject.Name, dataTypeName: "text" );
                writer.Write( insertObject.Integers, "integer[]" );
                writer.Write( insertObject.Datetime, "timestamp with time zone" );
            }

            writer.Complete();
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Copy", "PartitionTable" ) ]
    public void NpgsqlCopyToPartitionTable( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            using var writer = connection.BeginBinaryImport( "COPY public.test_object_partition_table (id, name, integers, datetime ) FROM STDIN (FORMAT BINARY)" );
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                SimpleTestObject insertObject = getNewObject();
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