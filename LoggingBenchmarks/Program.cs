using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

using Microsoft.Extensions.Logging;

namespace LoggingBenchmarks;

public class Program {
    static void Main( string[] args ) {
        if ( args.Length == 0 ) {
            // var output = new CacheOutputWriter( 1024 );
            // Console.SetOut( TextWriter.Null );
            using var benchmarkInstance = new LoggingBenchmarks();
            benchmarkInstance.InitializeLogging();
            benchmarkInstance.SerilogCoreLogger.Information( "logger: {Logger}. Hello world! {0} {1}", nameof(LoggingBenchmarks.SerilogCoreLogger), 1, 2 );
            benchmarkInstance.MicrosoftLogger.LogInformation( "logger: {Logger}. Hello world! {0} {1}", nameof(LoggingBenchmarks.MicrosoftLogger), 1, 2 );
            benchmarkInstance.SerilogLogger.LogInformation( "logger: {Logger}. Hello world! {0} {1}", nameof(LoggingBenchmarks.SerilogLogger), 1, 2 );

            return;
        }
        BenchmarkSwitcher
            .FromAssembly( typeof(Program).Assembly )
            .Run( args.Length > 0
                      ? args
                      : [
                          "-f", "*"
                      ],
                  ManualConfig
                      .Create( DefaultConfig.Instance )
                      .WithOptions( ConfigOptions.StopOnFirstError |
                                    ConfigOptions.JoinSummary ) );
    }
}