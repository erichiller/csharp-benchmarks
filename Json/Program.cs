using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

using Microsoft.Extensions.Logging;

namespace Benchmarks.Json;

public class Program {
    static void Main( string[] args )
        => BenchmarkSwitcher
           .FromAssembly( typeof(Program).Assembly )
           .Run( args.Length > 0
                     ? args
                     : new[] {
                         "-f", "*"
                     },
                 ManualConfig
                     .Create( DefaultConfig.Instance )
                     .WithOptions( ConfigOptions.StopOnFirstError |
                                   ConfigOptions.JoinSummary ) );
     // static void Main( string[] args )
     //     // => new SystemTextJson_BasicBenchmarks().SystemTextJson_Deserialize_Scalars_NodaTimeWithAttribute();
     //     => new SystemTextJson_BasicBenchmarks().SystemTextJson_Deserialize_Scalars_NodaTimeWithAttribute_SourceGen();
     //     // => new LevelOneJsonBenchmarks().SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne();


    public static ILogger<TLogger> GetLogger<TLogger>( ) {
        System.Console.WriteLine("GetLogger<TLogger>");
        using var loggerFactory = LoggerFactory.Create( builder => {
            builder
                .SetMinimumLevel( LogLevel.Trace )
                // .AddFilter( "Microsoft", LogLevel.Warning )
                // .AddFilter( "System", LogLevel.Warning )
                // .AddFilter( "LoggingConsoleApp.Program", LogLevel.Debug )
                .AddConsole();
        } );

        return loggerFactory.CreateLogger<TLogger>();
    }
}