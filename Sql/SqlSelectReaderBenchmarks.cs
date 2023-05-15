using System;
using System.Threading;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

using NodaTime;

using Npgsql;

using NpgsqlTypes;


namespace Benchmarks.Sql;

/* RESULTS:
 *
    |                                                         Method | Iterations | Mean [ms] | Error [ms] | StdDev [ms] |      Gen0 | Allocated [B] |
    |--------------------------------------------------------------- |----------- |----------:|-----------:|------------:|----------:|--------------:|
    | 'Generic NpgsqlDataReader methods, eg. GetFieldValue<Int32>()' |        200 |  587.3 ms |   11.41 ms |    12.21 ms | 4000.0000 |    80221792 B |
    |                              'Method-per-type, eg. GetInt32()' |        200 |  591.8 ms |   11.61 ms |    11.92 ms | 4000.0000 |    80223264 B |
    |              'Null check plus Method-per-type, eg. GetInt32()' |        200 |  604.5 ms |   11.79 ms |    14.91 ms | 4000.0000 |    80218112 B |
 */

[ Config( typeof(BenchmarkConfig) ) ]
public class SqlSelectReaderBenchmarks {

    [ Params( 200 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public virtual int Iterations { get; set; }

    public int TableObjectCount { get; set; } = 10_000;

    [ GlobalSetup ]
    public void PartitionTableSetup( ) {
        using NpgsqlConnection connection = SqlBenchmarksDbContext.GetOpenDbConnection();
        SimpleTestObject.CreateTableAndPopulate( connection, startId: 0, createCount: TableObjectCount );
    }


    /********************************************************
     * Benchmarks
     ********************************************************
     */

    [ Benchmark(Description = "Method-per-type, eg. GetInt32()") ]
    [ BenchmarkCategory( "Select", "Reader") ]
    public void ReadUsingTypeSpecificReadMethod( ) {
        int                    readCount  = 0;
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < Iterations ; o++ ) {
            using var cmd    = new NpgsqlCommand { Connection = connection, CommandText = "SELECT id, name FROM public.test_objects" };
            using var reader = cmd.ExecuteReader();
            while ( reader.Read() ) {
                int    id   = reader.GetInt32( 0 );
                string text = reader.GetString( 1 );
                readCount++;
            }
        }
        if ( readCount != TableObjectCount * Iterations ) {
            throw new Exception( $"Expected {nameof(readCount)} to be {TableObjectCount * Iterations}, readCount value is {readCount}" );
        }
    }
    
    [ Benchmark(Description = "Generic NpgsqlDataReader methods, eg. GetFieldValue<Int32>()") ]
    [ BenchmarkCategory( "Select", "Reader") ]
    public void ReadUsingGenericReadMethod( ) {
        int                    readCount  = 0;
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < Iterations ; o++ ) {
            using var cmd    = new NpgsqlCommand { Connection = connection, CommandText = "SELECT id, name FROM public.test_objects" };
            using var reader = cmd.ExecuteReader();
            while ( reader.Read() ) {
                int    id   = reader.GetFieldValue<Int32>( 0 );
                string text = reader.GetFieldValue<string>( 1 );
                readCount++;
            }
        }
        if ( readCount != TableObjectCount * Iterations ) {
            throw new Exception( $"Expected {nameof(readCount)} to be {TableObjectCount * Iterations}, readCount value is {readCount}" );
        }
    }
    
    [ Benchmark(Description = "Null check plus Method-per-type, eg. GetInt32()") ]
    [ BenchmarkCategory( "Select", "Reader") ]
    public void ReadUsingTypeSpecificReadWithNullChckMethod( ) {
        int                    readCount  = 0;
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < Iterations ; o++ ) {
            using var cmd    = new NpgsqlCommand { Connection = connection, CommandText = "SELECT id, name FROM public.test_objects" };
            using var reader = cmd.ExecuteReader();
            while ( reader.Read() ) {
                int?    id   = reader.IsDBNull( 0 ) ? null : reader.GetInt32( 0 );
                string? text = reader.IsDBNull( 0 ) ? null : reader.GetString( 1 );
                readCount++;
            }
        }
        if ( readCount != TableObjectCount * Iterations ) {
            throw new Exception( $"Expected {nameof(readCount)} to be {TableObjectCount * Iterations}, readCount value is {readCount}" );
        }
    }
    
}