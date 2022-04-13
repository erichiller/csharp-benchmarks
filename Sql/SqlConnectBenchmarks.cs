using System;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

using Microsoft.EntityFrameworkCore;

using NodaTime;

using Npgsql;

using NpgsqlTypes;


namespace Benchmarks.Sql;

[ Config( typeof(BenchmarkConfig) ) ]
public class SqlConnectBenchmarks {

    [ Params( 1000 ) ]
    // ReSharper disable once MemberCanBePrivate.Global
    // ReSharper disable once FieldCanBeMadeReadOnly.Global
    public virtual int Iterations { get; set; }

    /********************************************************
     * Benchmarks
     ********************************************************
     */


    [ Benchmark ]
    public void NpgsqlConnectionPerIterationTime( ) {
        using NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString );
        connection.Open();
        for ( int o = 0 ; o < Iterations ; o++ ) {
            using ( var cmd = new NpgsqlCommand() { Connection = connection, CommandText = $"SELECT 1+1 as sumof;" } ) {
                using ( var reader = cmd.ExecuteReader() ) {
                    reader.Read();
                    int count = reader.GetInt32( 0 );
                }
            }
        }
    }

    [ Benchmark ]
    public void NpgsqlConnectionSingleTime( ) {
        for ( int o = 0 ; o < Iterations ; o++ ) {
            using ( NpgsqlConnection connection = new NpgsqlConnection( SqlBenchmarksDbContext.ConnectionString ) ) {
                connection.Open();
                using ( var cmd = new NpgsqlCommand() { Connection = connection, CommandText = $"SELECT 1+1 as sumof;" } ) {
                    using ( var reader = cmd.ExecuteReader() ) {
                        reader.Read();
                        int count = reader.GetInt32( 0 );
                    }
                }
            }
        }
    }
    
}