using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

using BenchmarkDotNet.Configs;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

using NodaTime;

namespace Benchmarks.Sql;

public class SqlBenchmarksDbContext : DbContext
{
    public static string            ConnectionString = new ConfigurationBuilder()
        .AddJsonFile( @"./appsettings.json" )
        .Build()
        .GetConnectionString( "default" );
    public        DbSet<TestObject> TestObjects { get; set; }

    // public SqlBenchmarksDbContext(){
        // var folder = Environment.SpecialFolder.LocalApplicationData;
        // var path   = Environment.GetFolderPath(folder);
        // DbPath = System.IO.Path.Join(path, "blogging.db");
    // }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseNpgsql(ConnectionString,
                             npgsqlOptions =>
                                 npgsqlOptions.UseNodaTime()
        ) 
                  .UseSnakeCaseNamingConvention()
    ;
}

public record TestObject
{
    [Key]
    public int Id { get;           init; }
    public string Name     { get;  init; }
    public int[]  Integers { get;  init; }
    public Instant Datetime { get; init;}

    public static string CreateSqlString = @"
        DROP TABLE IF EXISTS public.test_objects;
        CREATE TABLE public.test_objects
        (
            id integer NOT NULL,
            name text,
            integers int[],
            datetime timestamptz,
            CONSTRAINT pk_test_object_id PRIMARY KEY (id)
        )
            TABLESPACE pg_default;";
}
