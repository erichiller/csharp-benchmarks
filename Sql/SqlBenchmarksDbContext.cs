using System.Collections.Generic;

using BenchmarkDotNet.Configs;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Benchmarks.Sql;

public class SqlBenchmarksDbContext : DbContext {
    public static string ConnectionString = new ConfigurationBuilder()
                                            .AddJsonFile( @"./appsettings.json" )
                                            .Build()
                                            .GetConnectionString( "default" );
    public DbSet<SimpleTestObject>         TestObjects        { get; set; }
    public DbSet<ComplexTestObject> ComplexTestObjects { get; set; }

    // public SqlBenchmarksDbContext(){
    // var folder = Environment.SpecialFolder.LocalApplicationData;
    // var path   = Environment.GetFolderPath(folder);
    // DbPath = System.IO.Path.Join(path, "blogging.db");
    // }

    // The following configures EF to create a Sqlite database file in the
    // special "local" folder for your platform.
    protected override void OnConfiguring( DbContextOptionsBuilder options )
        => options.UseNpgsql( ConnectionString,
                              npgsqlOptions =>
                                  npgsqlOptions.UseNodaTime()
                  )
                  .UseSnakeCaseNamingConvention();
}