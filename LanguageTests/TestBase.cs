using System;

using Microsoft.Extensions.Logging;

using Serilog;

using Xunit.Abstractions;

namespace Benchmarks.LanguageTests;

/// <summary>
/// Base class for tests to ease construction
/// </summary>
public class TestBase<T> {
    protected delegate void TestOutputHelperWriteLine( string msg );

    protected TestOutputHelperWriteLine _writeLine;

    protected readonly Microsoft.Extensions.Logging.ILogger _logger;

    protected static ILogger<TLogger> createLogger<TLogger>( ) {
        using Serilog.Extensions.Logging.SerilogLoggerFactory loggerFactory = new Serilog.Extensions.Logging.SerilogLoggerFactory();
        return loggerFactory.CreateLogger<TLogger>();
    }

    protected static ILogger<TLogger> configureLogging<TLogger>( ITestOutputHelper output ) {
        Log.Logger = new Serilog.LoggerConfiguration()
                     .MinimumLevel.Verbose()
                     .WriteTo.TestOutput( output,
                                          Serilog.Events.LogEventLevel.Verbose,
                                          // outputTemplate: @"{Timestamp:HH:mm:ss.fff zzz} [{Level:u3}] {ThreadId,-2} {SourceContext,-45} {Scope}{NewLine}     >> {Message:lj}{NewLine}{Exception}"
                                          outputTemplate: @"{Timestamp:HH:mm:ss.fff zzz} [{Level:u3}] {ThreadId,-2} >> {Message:lj}{NewLine}{Exception}"
                     )
                     // .WriteTo.File( "/home/eric/dev/src/github.com/erichiller/mkmrk-dotnet/src/Config.Tests/serilog.log",
                     //                      Serilog.Events.LogEventLevel.Verbose,
                     //                      outputTemplate: @"{Timestamp:HH:mm:ss.fff zzz} [{Level:u3}] {ThreadId,-2} {SourceContext,-45} {Scope}{NewLine}     >> {Message:lj}{NewLine}{Exception}"
                     // )
                     .Enrich.WithThreadId()
                     .CreateLogger()
                     .ForContext<TLogger>();
        return createLogger<TLogger>();
    }

    protected TestBase( ITestOutputHelper? output, Microsoft.Extensions.Logging.ILogger? logger = null ) {
        _logger         = logger ?? ( output is not null ? configureLogging<T>( output ) : throw new ArgumentNullException( nameof(output) ) );
        this._writeLine = output is not null ? output.WriteLine : Console.Out.WriteLine;
    }
}