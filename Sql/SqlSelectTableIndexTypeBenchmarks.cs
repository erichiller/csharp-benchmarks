using System;
using System.Threading;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

using Microsoft.EntityFrameworkCore;

using NodaTime;

using Npgsql;

using NpgsqlTypes;


namespace Benchmarks.Sql;

[ Config( typeof(BenchmarkConfig) ) ]
public class SqlSelectTableIndexTypeBenchmarks {

    [ Params( 200 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public virtual int Iterations { get; set; }


    [ Params( 1_000, 10_000, 100_000 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public virtual int RangeSize { get; set; }

    private const int _idRangeStart = 10_000;
    private       int _idRangeEnd => _idRangeStart + RangeSize;

    private static readonly Instant _instantRangeStart = SimpleTestObject.GetRandomInstant();
    private static readonly Instant _instantRangeEnd   = SimpleTestObject.GetRandomInstant(_instantRangeStart, 0);



    // private Random _rand = new ();
    // // public field
    // [ ParamsSource( nameof(RangeValues) ) ]
    // public ( int start, int end ) Range;

    // public ( int start, int end ) Range {
    //     get {
    //         int start = _rand.Next();
    //         return ( start, start + RangeSize );
    //     }
    // }

    // private int _rangeStart;
    // private int _rangeEnd;
    //
    // private ( int start, int end ) _range {
    //     get {
    //         
    //     }
    // }

    [ GlobalSetup( Target = nameof(SelectFromPartitionTableUsingBtreeIndex) ) ] // URGENT: shouldn't each method have a setup?
    public void SetupPartitionTableBtreeIndex( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        SimpleTestObject.CreatePartitionTableBtreeIndex( connection );
        Thread.Sleep( 5000 );
    }
    [ GlobalSetup( Target = nameof(SelectFromPartitionTableUsingBrinIndex) ) ] // URGENT: shouldn't each method have a setup?
    public void SetupPartitionTableBrinIndex( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        SimpleTestObject.CreatePartitionTableBrinIndex( connection );
        Thread.Sleep( 5000 );
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

    [ Benchmark(Description = "B-Tree index on normal table") ]
    [ BenchmarkCategory( "Select", "B-Tree" ) ]
    public void SelectTableUsingIndex( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < Iterations ; o++ ) {
            using ( var cmd = new NpgsqlCommand() { Connection = connection, CommandText = $"SELECT count(*) FROM public.test_objects WHERE id BETWEEN {_idRangeStart} AND {_idRangeEnd}" } ) {
                using ( var reader = cmd.ExecuteReader() ) {
                    reader.Read();
                    int count = reader.GetInt32( 0 );
                }
            }
        }
    }
    
    // [ Benchmark ]
    [ Benchmark(Description = "B-Tree index on Partition Table") ]
    [ BenchmarkCategory( "Select", "PartitionTable", "B-Tree" ) ]
    public void SelectFromPartitionTableUsingBtreeIndex( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < Iterations ; o++ ) {
            using ( var cmd = new NpgsqlCommand() { Connection = connection, CommandText = $"SELECT count(*) FROM public.test_object_partition_table WHERE id BETWEEN {_idRangeStart} AND {_idRangeEnd}" } ) {
                using ( var reader = cmd.ExecuteReader() ) {
                    reader.Read();
                    int count = reader.GetInt32( 0 );
                }
            }
        }
    }
    
    // [ Benchmark ]
    [ Benchmark(Description = "BRIN index on Partition Table") ]
    [ BenchmarkCategory( "Select", "PartitionTable", "BRIN" ) ]
    public void SelectFromPartitionTableUsingBrinIndex( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < Iterations ; o++ ) {
            using ( var cmd = new NpgsqlCommand() { Connection = connection, CommandText = $"SELECT count(*) FROM public.test_object_partition_table WHERE id BETWEEN {_idRangeStart} AND {_idRangeEnd}" } ) {
                using ( var reader = cmd.ExecuteReader() ) {
                    reader.Read();
                    int count = reader.GetInt32( 0 );
                }
            }
        }
    }
    
    [ Benchmark ]
    [ BenchmarkCategory( "Select", "B-Tree" ) ]
    public void SelectTableUsingIndexPlusSecondColumn( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < Iterations ; o++ ) {

            using ( var cmd = new NpgsqlCommand() { Connection = connection, CommandText = $@"
                SELECT count(*) FROM public.test_objects 
                WHERE 
                      id BETWEEN {_idRangeStart} AND {_idRangeEnd}
                      AND datetime BETWEEN '{_instantRangeStart}' AND '{_instantRangeEnd}'; " } ) {
                using ( var reader = cmd.ExecuteReader() ) {
                    reader.Read();
                    int count = reader.GetInt32( 0 );
                }
            }
        }
    }

    
    [ Benchmark ]
    [ BenchmarkCategory( "Select", "PartitionTable", "BRIN" ) ]
    public void SelectFromPartitionTableUsingIndexPlusSecondColumn( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < Iterations ; o++ ) {
            using ( var cmd = new NpgsqlCommand() { Connection = connection, CommandText = $@"
                SELECT count(*) FROM public.test_object_partition_table 
                WHERE 
                      id BETWEEN {_idRangeStart} AND {_idRangeEnd}
                      AND datetime BETWEEN '{_instantRangeStart}' AND '{_instantRangeEnd}'; " } ) {
                using ( var reader = cmd.ExecuteReader() ) {
                    reader.Read();
                    int count = reader.GetInt32( 0 );
                }
            }
        }
    }
    
}