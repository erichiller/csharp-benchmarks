using System;
using System.Linq;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

using Microsoft.EntityFrameworkCore;

using NodaTime;

using Npgsql;

using NpgsqlTypes;

// ReSharper disable BitwiseOperatorOnEnumWithoutFlags

namespace Benchmarks.Sql;

[ Config( typeof(BenchmarkConfig) ) ]
public class SqlInsertComplexObjectBenchmarks {
    private readonly Instant _testTime = Instant.FromDateTimeOffset( DateTimeOffset.UtcNow );

    // [ Params( 1, 10, 100, 1000 ) ]
    // [ Params( 1, 2, 5, 10 ) ]
    // [ Params( 1, 2, 10, 100 ) ]
    [ Params( 2, 10 ) ]
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
    [ Params( 10, 100 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public virtual int SaveIterations { get; set; }

    private ComplexTestObject getNewComplexObject( ) => new ComplexTestObject(
        id: _count,
        textCol: "TestObject Name here",
        tzCol: _testTime,
        intCol: 10,
        decCol1: 0.100m,
        decCol2: 0.100m,
        floatCol: 0.001f,
        textArrayCol: new[] { "hello", "world", "foo", "bar" },
        intArrayCol: new[] { 1, 2, 3 }
    );
    
    
    [ GlobalSetup ]
    public void GlobalSetup( ) {
        using var              db           = new SqlBenchmarksDbContext();
        using NpgsqlConnection dbConnection = db.Database.GetDbConnection() as NpgsqlConnection ?? throw new Exception();
        dbConnection.Open();
        dbConnection.TypeMapper.UseNodaTime();
        using ( var cmd = new NpgsqlCommand() { Connection = dbConnection, CommandText = ComplexTestObject.CreateSqlString } ) {
            cmd.ExecuteNonQuery();
        }
        // TODO
        // using ( var cmd = new NpgsqlCommand() { Connection = dbConnection, CommandText = SimpleTestObject.CreatePartitionTable } ) {
        //     cmd.ExecuteNonQuery();
        // }
    }


    /* TODO:
     --Test Query Performance: 
SELECT count(*) FROM td.time_sale_futures WHERE trade_time BETWEEN '2022-04-08' AND '2022-04-09' AND key='/ESM22';
-- VS ; 
SELECT count(*) FROM td.time_sale_futures_original WHERE trade_time BETWEEN '2022-04-08' AND '2022-04-09' AND key='/ESM22';
--did affect? part tbl, brin
     */

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
                db.ComplexTestObjects.Add( getNewComplexObject() );
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
                db.ComplexTestObjects.Add( getNewComplexObject() );
            }

            db.SaveChanges();
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Singular", "Typed" ) ]
    public void NpgSqlInsert_SingularCommand_TypedValue( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                ComplexTestObject insertObject = getNewComplexObject();
                using var cmd = new NpgsqlCommand( @"INSERT INTO public.complex_test_objects 
                                                                        ( id, text_col, tz_col, int_col, dec_col1, dec_col2, float_col, text_array_col, int_array_col  ) 
                                                                        VALUES ( $1, $2, $3, $4, $5, $6, $7, $8, $9 )", connection );
                cmd.Parameters.Add( new NpgsqlParameter<int> { TypedValue      = insertObject.Id } );
                cmd.Parameters.Add( new NpgsqlParameter<string> { TypedValue   = insertObject.TextCol } );
                cmd.Parameters.Add( new NpgsqlParameter<Instant> { TypedValue  = insertObject.TzCol } );
                cmd.Parameters.Add( new NpgsqlParameter<int> { TypedValue      = insertObject.IntCol } );
                cmd.Parameters.Add( new NpgsqlParameter<decimal> { TypedValue  = insertObject.DecCol1 } );
                cmd.Parameters.Add( new NpgsqlParameter<decimal> { TypedValue  = insertObject.DecCol2 } );
                cmd.Parameters.Add( new NpgsqlParameter<float> { TypedValue    = insertObject.FloatCol } );
                cmd.Parameters.Add( new NpgsqlParameter<string[]> { TypedValue = insertObject.TextArrayCol } );
                cmd.Parameters.Add( new NpgsqlParameter<int[]> { TypedValue    = insertObject.IntArrayCol } );
                cmd.ExecuteNonQuery();
            }
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Singular", "Typed", "Prepared" ) ]
    public void NpgSqlInsert_SingularCommand_TypedValue_Prepared( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        using var cmd = new NpgsqlCommand( @"INSERT INTO public.complex_test_objects 
                                                                        ( id, text_col, tz_col, int_col, dec_col1, dec_col2, float_col, text_array_col, int_array_col  ) 
                                                                        VALUES ( $1, $2, $3, $4, $5, $6, $7, $8, $9 )", connection );
        var idParam        = new NpgsqlParameter<int>();
        var textParam      = new NpgsqlParameter<string>();
        var tzParam        = new NpgsqlParameter<Instant>();
        var intParam       = new NpgsqlParameter<int>();
        var dec1Param      = new NpgsqlParameter<decimal>();
        var dec2Param      = new NpgsqlParameter<decimal>();
        var floatParam     = new NpgsqlParameter<float>();
        var textArrayParam = new NpgsqlParameter<string[]>();
        var intArrayParam  = new NpgsqlParameter<int[]>();
        cmd.Parameters.Add( idParam );
        cmd.Parameters.Add( textParam );
        cmd.Parameters.Add( tzParam );
        cmd.Parameters.Add( intParam );
        cmd.Parameters.Add( dec1Param );
        cmd.Parameters.Add( dec2Param );
        cmd.Parameters.Add( floatParam );
        cmd.Parameters.Add( textArrayParam );
        cmd.Parameters.Add( intArrayParam );
        cmd.Prepare();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                ComplexTestObject insertObject = getNewComplexObject();
                idParam.TypedValue        = insertObject.Id;
                textParam.TypedValue      = insertObject.TextCol;
                tzParam.TypedValue        = insertObject.TzCol;
                intParam.TypedValue       = insertObject.IntCol;
                dec1Param.TypedValue      = insertObject.DecCol1;
                dec2Param.TypedValue      = insertObject.DecCol2;
                floatParam.TypedValue     = insertObject.FloatCol;
                textArrayParam.TypedValue = insertObject.TextArrayCol;
                intArrayParam.TypedValue  = insertObject.IntArrayCol;
                cmd.ExecuteNonQuery();
            }
        }
    }

    /* TEST THESE THREE ---> */
    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Singular", "Boxed", "NpgsqlValue" ) ]
    public void NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                ComplexTestObject insertObject = getNewComplexObject();
                using var cmd = new NpgsqlCommand( @"INSERT INTO public.complex_test_objects 
                                                                        ( id, text_col, tz_col, int_col, dec_col1, dec_col2, float_col, text_array_col, int_array_col  ) 
                                                                        VALUES ( $1, $2, $3, $4, $5, $6, $7, $8, $9 )", connection );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, NpgsqlValue                      = insertObject.Id } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Text, NpgsqlValue                         = insertObject.TextCol } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.TimestampTz, NpgsqlValue                  = insertObject.TzCol } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer, NpgsqlValue                      = insertObject.IntCol } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Numeric, NpgsqlValue                      = insertObject.DecCol1 } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Numeric, NpgsqlValue                      = insertObject.DecCol2 } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Real, NpgsqlValue                       = insertObject.FloatCol } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Text, NpgsqlValue    = insertObject.TextArrayCol } );
                cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer, NpgsqlValue = insertObject.IntArrayCol } );
                cmd.ExecuteNonQuery();
            }
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Singular", "Boxed", "NpgsqlValue" ) ]
    public void NpgSqlInsert_SingularCommand_Typed_NpgsqlDbType_NpgsqlValue( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                ComplexTestObject insertObject = getNewComplexObject();
                using var cmd = new NpgsqlCommand( @"INSERT INTO public.complex_test_objects 
                                                                        ( id, text_col, tz_col, int_col, dec_col1, dec_col2, float_col, text_array_col, int_array_col  ) 
                                                                        VALUES ( $1, $2, $3, $4, $5, $6, $7, $8, $9 )", connection );
                cmd.Parameters.Add( new NpgsqlParameter<int> { NpgsqlDbType      = NpgsqlDbType.Integer, NpgsqlValue                      = insertObject.Id } );
                cmd.Parameters.Add( new NpgsqlParameter<string> { NpgsqlDbType   = NpgsqlDbType.Text, NpgsqlValue                         = insertObject.TextCol } );
                cmd.Parameters.Add( new NpgsqlParameter<Instant> { NpgsqlDbType  = NpgsqlDbType.TimestampTz, NpgsqlValue                  = insertObject.TzCol } );
                cmd.Parameters.Add( new NpgsqlParameter<int> { NpgsqlDbType      = NpgsqlDbType.Integer, NpgsqlValue                      = insertObject.IntCol } );
                cmd.Parameters.Add( new NpgsqlParameter<decimal> { NpgsqlDbType  = NpgsqlDbType.Numeric, NpgsqlValue                      = insertObject.DecCol1 } );
                cmd.Parameters.Add( new NpgsqlParameter<decimal> { NpgsqlDbType  = NpgsqlDbType.Numeric, NpgsqlValue                      = insertObject.DecCol2 } );
                cmd.Parameters.Add( new NpgsqlParameter<float> { NpgsqlDbType    = NpgsqlDbType.Real, NpgsqlValue                         = insertObject.FloatCol } );
                cmd.Parameters.Add( new NpgsqlParameter<string[]> { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Text, NpgsqlValue    = insertObject.TextArrayCol } );
                cmd.Parameters.Add( new NpgsqlParameter<int[]> { NpgsqlDbType    = NpgsqlDbType.Array | NpgsqlDbType.Integer, NpgsqlValue = insertObject.IntArrayCol } );
                cmd.ExecuteNonQuery();
            }
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Singular", "Boxed", "NpgsqlValue" ) ]
    public void NpgSqlInsert_SingularCommand_Typed_NpgsqlDbType_TypedValue( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                ComplexTestObject insertObject = getNewComplexObject();
                using var cmd = new NpgsqlCommand( @"INSERT INTO public.complex_test_objects 
                                                                        ( id, text_col, tz_col, int_col, dec_col1, dec_col2, float_col, text_array_col, int_array_col  ) 
                                                                        VALUES ( $1, $2, $3, $4, $5, $6, $7, $8, $9 )", connection );
                cmd.Parameters.Add( new NpgsqlParameter<int> { NpgsqlDbType      = NpgsqlDbType.Integer, TypedValue                      = insertObject.Id } );
                cmd.Parameters.Add( new NpgsqlParameter<string> { NpgsqlDbType   = NpgsqlDbType.Text, TypedValue                         = insertObject.TextCol } );
                cmd.Parameters.Add( new NpgsqlParameter<Instant> { NpgsqlDbType  = NpgsqlDbType.TimestampTz, TypedValue                  = insertObject.TzCol } );
                cmd.Parameters.Add( new NpgsqlParameter<int> { NpgsqlDbType      = NpgsqlDbType.Integer, TypedValue                      = insertObject.IntCol } );
                cmd.Parameters.Add( new NpgsqlParameter<decimal> { NpgsqlDbType  = NpgsqlDbType.Numeric, TypedValue                      = insertObject.DecCol1 } );
                cmd.Parameters.Add( new NpgsqlParameter<decimal> { NpgsqlDbType  = NpgsqlDbType.Numeric, TypedValue                      = insertObject.DecCol2 } );
                cmd.Parameters.Add( new NpgsqlParameter<float> { NpgsqlDbType    = NpgsqlDbType.Real, TypedValue                         = insertObject.FloatCol } );
                cmd.Parameters.Add( new NpgsqlParameter<string[]> { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Text, TypedValue    = insertObject.TextArrayCol } );
                cmd.Parameters.Add( new NpgsqlParameter<int[]> { NpgsqlDbType    = NpgsqlDbType.Array | NpgsqlDbType.Integer, TypedValue = insertObject.IntArrayCol } );
                cmd.ExecuteNonQuery();
            }
        }
    }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Insert", "Singular", "Boxed", "NpgsqlValue", "Prepared" ) ]
    public void NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        using var cmd = new NpgsqlCommand( @"INSERT INTO public.complex_test_objects 
                                                                        ( id, text_col, tz_col, int_col, dec_col1, dec_col2, float_col, text_array_col, int_array_col  ) 
                                                                        VALUES ( $1, $2, $3, $4, $5, $6, $7, $8, $9 )", connection );
        NpgsqlParameter[] parameters = new[] {
            cmd.Parameters.Add( new NpgsqlParameter<int> { NpgsqlDbType      = NpgsqlDbType.Integer } ),
            cmd.Parameters.Add( new NpgsqlParameter<string> { NpgsqlDbType   = NpgsqlDbType.Text } ),
            cmd.Parameters.Add( new NpgsqlParameter<Instant> { NpgsqlDbType  = NpgsqlDbType.TimestampTz } ),
            cmd.Parameters.Add( new NpgsqlParameter<int> { NpgsqlDbType      = NpgsqlDbType.Integer } ),
            cmd.Parameters.Add( new NpgsqlParameter<decimal> { NpgsqlDbType  = NpgsqlDbType.Numeric } ),
            cmd.Parameters.Add( new NpgsqlParameter<decimal> { NpgsqlDbType  = NpgsqlDbType.Numeric } ),
            cmd.Parameters.Add( new NpgsqlParameter<float> { NpgsqlDbType    = NpgsqlDbType.Real } ),
            cmd.Parameters.Add( new NpgsqlParameter<string[]> { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Text } ),
            cmd.Parameters.Add( new NpgsqlParameter<int[]> { NpgsqlDbType    = NpgsqlDbType.Array | NpgsqlDbType.Integer } )
        };
        cmd.Prepare();
        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                ComplexTestObject insertObject = getNewComplexObject();
                parameters[ 0 ].NpgsqlValue = insertObject.Id;
                parameters[ 1 ].NpgsqlValue = insertObject.TextCol;
                parameters[ 2 ].NpgsqlValue = insertObject.TzCol;
                parameters[ 3 ].NpgsqlValue = insertObject.IntCol;
                parameters[ 4 ].NpgsqlValue = insertObject.DecCol1;
                parameters[ 5 ].NpgsqlValue = insertObject.DecCol2;
                parameters[ 6 ].NpgsqlValue = insertObject.FloatCol;
                parameters[ 7 ].NpgsqlValue = insertObject.TextArrayCol;
                parameters[ 8 ].NpgsqlValue = insertObject.IntArrayCol;
                cmd.ExecuteNonQuery();
            }
        }
    }
    //
    // [ Benchmark ]
    // [ BenchmarkCategory( "Npgsql", "Insert", "Singular", "Boxed", "NpgsqlValue", "Prepared", "Async" ) ]
    // public async Task NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared_Async( ) {
    //     await using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
    //     connection.Open();
    //     await using var   cmd        = new NpgsqlCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )", connection );
    //     NpgsqlParameter[] parameters = new[] { cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Integer } ), cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Text } ), cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer } ), cmd.Parameters.Add( new NpgsqlParameter { NpgsqlDbType = NpgsqlDbType.TimestampTz } ) };
    //     await cmd.PrepareAsync();
    //     for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
    //         for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
    //             TestObject insertObject = _newObject;
    //             parameters[ 0 ].NpgsqlValue = insertObject.Id;
    //             parameters[ 1 ].NpgsqlValue = insertObject.Name;
    //             parameters[ 2 ].NpgsqlValue = insertObject.Integers;
    //             parameters[ 3 ].NpgsqlValue = insertObject.Datetime;
    //             await cmd.ExecuteNonQueryAsync();
    //         }
    //     }
    // }
    //
    //
    // [ Benchmark ]
    // [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "Boxed", "Value" ) ]
    // public void NpgsqlInsert_Batched_Boxed_Value( ) {
    //     using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
    //     connection.Open();
    //     for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
    //         using var batch = new NpgsqlBatch( connection );
    //         for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
    //             TestObject insertObject = _newObject;
    //             var        cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )" );
    //             cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Id } );
    //             cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Name } );
    //             cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Integers } );
    //             cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Datetime } );
    //             batch.BatchCommands.Add( cmd );
    //         }
    //
    //         batch.ExecuteNonQuery();
    //     }
    // }
    //
    // [ Benchmark ]
    // [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "Boxed", "NpgsqlDbType" ) ]
    // public void NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value( ) {
    //     using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
    //     connection.Open();
    //     for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
    //         using var batch = new NpgsqlBatch( connection );
    //         for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
    //             TestObject insertObject = _newObject;
    //             var        cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )" );
    //             cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Id, NpgsqlDbType       = NpgsqlDbType.Integer } );
    //             cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Name, NpgsqlDbType     = NpgsqlDbType.Text } );
    //             cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Integers, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer } );
    //             cmd.Parameters.Add( new NpgsqlParameter { Value = insertObject.Datetime, NpgsqlDbType = NpgsqlDbType.TimestampTz } );
    //             batch.BatchCommands.Add( cmd );
    //         }
    //
    //         batch.ExecuteNonQuery();
    //     }
    // }
    //
    // [ Benchmark ]
    // [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "Boxed", "NpgsqlValue" ) ]
    // public void NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue( ) {
    //     using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
    //     connection.Open();
    //     for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
    //         using var batch = new NpgsqlBatch( connection );
    //         for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
    //             TestObject insertObject = _newObject;
    //             var        cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )" );
    //             cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue = insertObject.Id, NpgsqlDbType       = NpgsqlDbType.Integer } );
    //             cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue = insertObject.Name, NpgsqlDbType     = NpgsqlDbType.Text } );
    //             cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue = insertObject.Integers, NpgsqlDbType = NpgsqlDbType.Array | NpgsqlDbType.Integer } );
    //             cmd.Parameters.Add( new NpgsqlParameter { NpgsqlValue = insertObject.Datetime, NpgsqlDbType = NpgsqlDbType.TimestampTz } );
    //             batch.BatchCommands.Add( cmd );
    //         }
    //
    //         batch.ExecuteNonQuery();
    //     }
    // }
    //
    // [ Benchmark ]
    // [ BenchmarkCategory( "Npgsql", "Insert", "Batched", "TypedValue" ) ]
    // public void NpgsqlInsert_Batched_TypedValue( ) {
    //     using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
    //     connection.Open();
    //     for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
    //         using var batch = new NpgsqlBatch( connection );
    //         for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
    //             TestObject insertObject = _newObject;
    //             var        cmd          = new NpgsqlBatchCommand( @"INSERT INTO public.test_objects ( id, name, integers, datetime ) VALUES ( $1, $2, $3, $4 )" );
    //             cmd.Parameters.Add( new NpgsqlParameter<int> { TypedValue     = insertObject.Id } );
    //             cmd.Parameters.Add( new NpgsqlParameter<string> { TypedValue  = insertObject.Name } );
    //             cmd.Parameters.Add( new NpgsqlParameter<int[]> { TypedValue   = insertObject.Integers } );
    //             cmd.Parameters.Add( new NpgsqlParameter<Instant> { TypedValue = insertObject.Datetime } );
    //             batch.BatchCommands.Add( cmd );
    //         }
    //
    //         batch.ExecuteNonQuery();
    //     }
    // }

    [ Benchmark ]
    [ BenchmarkCategory( "Npgsql", "Copy" ) ]
    public void NpgsqlCopy( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();

        for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
            // using var writer = connection.BeginBinaryImport( @"
            //     COPY public.complex_test_objects 
            //         ( id, text_col, tz_col, int_col, dec_col1, dec_col2, float_col, text_array_col, int_array_col )
            //     FROM STDIN (FORMAT BINARY)" );
            
            using var writer = connection.BeginBinaryImport( @"
                COPY public.complex_test_objects 
                    ( id, text_col, tz_col, int_col, dec_col1, dec_col2, text_array_col, int_array_col )
                FROM STDIN (FORMAT BINARY)" );
            // Console.WriteLine($"SaveIterations = {SaveIterations}");
            for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
                ComplexTestObject insertObject = getNewComplexObject();
                // Console.WriteLine($"ObjectsPerSave = {ObjectsPerSave}");
                // Console.WriteLine($"insertObject = {insertObject}");
                // Console.WriteLine($"TextArrayCol = {String.Join(',', insertObject.TextArrayCol)}");
                // Console.WriteLine($"IntArrayCol = {String.Join(',', insertObject.IntArrayCol)}");
                writer.StartRow();
                writer.Write( npgsqlDbType: NpgsqlDbType.Integer, value: insertObject.Id );
                writer.Write( npgsqlDbType: NpgsqlDbType.Text, value: insertObject.TextCol );
                writer.Write( npgsqlDbType: NpgsqlDbType.TimestampTz, value: insertObject.TzCol );
                writer.Write( npgsqlDbType: NpgsqlDbType.Integer, value: insertObject.IntCol );
                writer.Write( npgsqlDbType: NpgsqlDbType.Numeric, value: insertObject.DecCol1 );
                writer.Write( npgsqlDbType: NpgsqlDbType.Numeric, value: insertObject.DecCol2 );
                // writer.Write( npgsqlDbType: NpgsqlDbType.Real, value: insertObject.FloatCol );
                writer.Write( npgsqlDbType: NpgsqlDbType.Array | NpgsqlDbType.Text, value: insertObject.TextArrayCol );
                writer.Write( npgsqlDbType: NpgsqlDbType.Array | NpgsqlDbType.Integer, value: insertObject.IntArrayCol );
            }
            // Console.WriteLine($"Completing");

            writer.Complete();
        }
    }

    // [ Benchmark ]
    // [ BenchmarkCategory( "Npgsql", "Copy", "PartitionTable" ) ]
    // public void NpgsqlCopyToPartitionTable( ) {
    //     using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
    //     connection.Open();
    //
    //     for ( int o = 0 ; o < SaveIterations ; o++, _count++ ) {
    //
    // using var writer = connection.BeginBinaryImport( @"
    //             COPY public.complex_test_objects 
    //                 ( id, text_col, tz_col, int_col, dec_col1, dec_col2, float_col, text_array_col, int_array_col  )
    //             FROM STDIN (FORMAT BINARY)" );
    //         for ( int i = 0 ; i < ObjectsPerSave ; i++, _count++ ) {
    //             ComplexTestObject insertObject = getNewComplexObject();
    //             writer.StartRow();
    //             writer.Write( npgsqlDbType: NpgsqlDbType.Integer, value:insertObject.Id );
    //             writer.Write( npgsqlDbType: NpgsqlDbType.Text, value:insertObject.TextCol );
    //             writer.Write( npgsqlDbType: NpgsqlDbType.TimestampTz, value:insertObject.TzCol );
    //             writer.Write( npgsqlDbType: NpgsqlDbType.Integer, value:insertObject.IntCol );
    //             writer.Write( npgsqlDbType: NpgsqlDbType.Numeric, value:insertObject.DecCol1 );
    //             writer.Write( npgsqlDbType: NpgsqlDbType.Numeric, value:insertObject.DecCol2 );
    //             writer.Write( npgsqlDbType: NpgsqlDbType.Real, value:insertObject.FloatCol );
    //             writer.Write( npgsqlDbType: NpgsqlDbType.Array | NpgsqlDbType.Integer, value:insertObject.IntArrayCol );
    //             writer.Write( npgsqlDbType: NpgsqlDbType.Array | NpgsqlDbType.Text, value:insertObject.TextArrayCol );
    //             
    //         }
    //
    //         writer.Complete();
    //     }
    // }

    //
    // /// <inheritdoc />
    // public void Dispose( ) {
    //     this.DbConnection.Close();
    //     this.DbConnection.Dispose();
    // }
}