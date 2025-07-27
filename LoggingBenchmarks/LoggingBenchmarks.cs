using System;
using System.IO;
using System.Text;
using System.Threading;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace LoggingBenchmarks;

/*
 *

    **************************************************
    ** SERILOG WITHOUT TIME, BUT WITH SourceContext **
    **************************************************

   | Method                                                 | Mean       | Error   | StdDev  | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
   |------------------------------------------------------- |-----------:|--------:|--------:|------:|-------:|-------:|----------:|------------:|
   | Microsoft_Basic                                        |   337.5 ns | 1.34 ns | 1.12 ns |  0.46 | 0.0219 |      - |     104 B |        0.29 |
   | Microsoft_Basic_LoggerDefine                           |   351.0 ns | 1.81 ns | 1.70 ns |  0.48 | 0.0334 | 0.0019 |     160 B |        0.44 |
   | Serilog_Core_Basic                                     |   365.2 ns | 0.64 ns | 0.57 ns |  0.50 | 0.1917 | 0.0005 |     904 B |        2.51 |
   | Serilog_ILogger_Basic                                  |   535.9 ns | 2.19 ns | 1.94 ns |  0.73 | 0.2460 |      - |    1160 B |        3.22 |
   | Serilog_ILogger_Basic_LoggerDefine                     |   563.3 ns | 1.55 ns | 1.38 ns |  0.77 | 0.2546 |      - |    1200 B |        3.33 |
   | Serilog_Core_Formatting_Reference_Args                 |   633.4 ns | 0.67 ns | 0.56 ns |  0.86 | 0.2651 |      - |    1248 B |        3.47 |
   | Microsoft_Formatting_Reference_Args                    |   633.7 ns | 3.63 ns | 3.40 ns |  0.86 | 0.0658 | 0.0010 |     312 B |        0.87 |
   | Microsoft_Formatting_Value_Args_LoggerDefine           |   645.2 ns | 4.06 ns | 3.80 ns |  0.88 | 0.0658 | 0.0019 |     312 B |        0.87 |
   | Microsoft_Formatting_Reference_Args_LoggerDefine       |   678.9 ns | 2.83 ns | 2.64 ns |  0.93 | 0.0677 | 0.0010 |     320 B |        0.89 |
   | Serilog_Core_Formatting_Value_Args                     |   685.2 ns | 1.95 ns | 1.73 ns |  0.93 | 0.2747 |      - |    1296 B |        3.60 |
   | Microsoft_Formatting_Value_Args                        |   733.1 ns | 4.82 ns | 4.51 ns |  1.00 | 0.0763 |      - |     360 B |        1.00 |
   | Serilog_ILogger_Formatting_Reference_Args              |   884.1 ns | 2.91 ns | 2.58 ns |  1.21 | 0.3366 | 0.0010 |    1584 B |        4.40 |
   | Serilog_ILogger_Formatting_Value_Args                  |   910.3 ns | 2.20 ns | 1.83 ns |  1.24 | 0.3462 |      - |    1632 B |        4.53 |
   | Serilog_ILogger_Formatting_Reference_Args_LoggerDefine |   918.3 ns | 0.91 ns | 0.76 ns |  1.25 | 0.3376 |      - |    1592 B |        4.42 |
   | Serilog_ILogger_Formatting_Value_Args_LoggerDefine     | 1,003.5 ns | 3.79 ns | 3.54 ns |  1.37 | 0.3586 |      - |    1688 B |        4.69 |

   | Method                                             | Mean     | Error   | StdDev  | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
   |--------------------------------------------------- |---------:|--------:|--------:|------:|-------:|-------:|----------:|------------:|
   | Microsoft_Formatting_Value_Args_LoggerDefine       | 613.4 ns | 4.68 ns | 4.15 ns |  0.84 | 0.0658 | 0.0010 |     312 B |        0.87 |
   | Serilog_Core_Formatting_Value_Args                 | 681.9 ns | 3.84 ns | 2.99 ns |  0.93 | 0.2747 |      - |    1296 B |        3.60 |
   | Microsoft_Formatting_Value_Args                    | 732.1 ns | 5.58 ns | 5.22 ns |  1.00 | 0.0763 |      - |     360 B |        1.00 |
   | Serilog_ILogger_Formatting_Value_Args              | 929.5 ns | 8.65 ns | 7.22 ns |  1.27 | 0.3452 |      - |    1632 B |        4.53 |
   | Serilog_ILogger_Formatting_Value_Args_LoggerDefine | 975.5 ns | 4.27 ns | 3.99 ns |  1.33 | 0.3586 |      - |    1688 B |        4.69 |


   *************************************************
   **** Both with time, both with SourceContext ****
   *************************************************

   | Method                                                 | Mean       | Error    | StdDev   | Ratio | Gen0   | Gen1   | Allocated | Alloc Ratio |
   |------------------------------------------------------- |-----------:|---------:|---------:|------:|-------:|-------:|----------:|------------:|
   | Serilog_Core_Basic                                     |   544.4 ns | 10.66 ns | 12.28 ns |  0.41 | 0.1955 |      - |     920 B |        2.17 |
   | Serilog_ILogger_Basic                                  |   675.7 ns |  2.42 ns |  2.02 ns |  0.51 | 0.2480 |      - |    1168 B |        2.75 |
   | Serilog_ILogger_Basic_LoggerDefine                     |   716.5 ns |  3.19 ns |  2.83 ns |  0.54 | 0.2584 |      - |    1216 B |        2.87 |
   | Serilog_Core_Formatting_Reference_Args                 |   805.7 ns |  1.25 ns |  1.04 ns |  0.60 | 0.2661 |      - |    1256 B |        2.96 |
   | Serilog_Core_Formatting_Value_Args                     |   837.4 ns |  4.48 ns |  4.19 ns |  0.63 | 0.2766 | 0.0010 |    1304 B |        3.08 |
   | Microsoft_Basic                                        |   906.7 ns |  8.70 ns |  8.14 ns |  0.68 | 0.0334 |      - |     160 B |        0.38 |
   | Microsoft_Basic_LoggerDefine                           |   997.0 ns |  6.55 ns |  6.13 ns |  0.75 | 0.0458 |      - |     224 B |        0.53 |
   | Serilog_ILogger_Formatting_Reference_Args              | 1,047.5 ns |  2.61 ns |  2.04 ns |  0.79 | 0.3395 |      - |    1600 B |        3.77 |
   | Serilog_ILogger_Formatting_Value_Args                  | 1,071.8 ns |  4.18 ns |  3.71 ns |  0.80 | 0.3490 |      - |    1648 B |        3.89 |
   | Serilog_ILogger_Formatting_Reference_Args_LoggerDefine | 1,124.4 ns |  5.41 ns |  4.80 ns |  0.84 | 0.3414 |      - |    1608 B |        3.79 |
   | Serilog_ILogger_Formatting_Value_Args_LoggerDefine     | 1,166.6 ns |  4.98 ns |  4.66 ns |  0.87 | 0.3605 |      - |    1704 B |        4.02 |
   | Microsoft_Formatting_Value_Args_LoggerDefine           | 1,229.8 ns |  8.86 ns |  8.28 ns |  0.92 | 0.0782 |      - |     368 B |        0.87 |
   | Microsoft_Formatting_Reference_Args_LoggerDefine       | 1,231.1 ns | 16.80 ns | 15.71 ns |  0.92 | 0.0782 |      - |     376 B |        0.89 |
   | Microsoft_Formatting_Reference_Args                    | 1,232.0 ns |  6.30 ns |  5.89 ns |  0.92 | 0.0782 |      - |     376 B |        0.89 |
   | Microsoft_Formatting_Value_Args                        | 1,334.3 ns | 13.70 ns | 12.81 ns |  1.00 | 0.0896 |      - |     424 B |        1.00 |


   *********************************************
   **** Color off, matching formats exactly ****
   *********************************************

   | Method                                                 | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
   |------------------------------------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
   | Serilog_Core_Basic                                     |   618.8 ns | 12.37 ns | 14.73 ns |  0.46 |    0.01 | 0.2289 |      - |    1080 B |        2.55 |
   | Serilog_ILogger_Basic                                  |   702.3 ns |  2.35 ns |  2.08 ns |  0.53 |    0.01 | 0.2499 |      - |    1176 B |        2.77 |
   | Serilog_ILogger_Basic_LoggerDefine                     |   747.1 ns |  2.59 ns |  2.16 ns |  0.56 |    0.01 | 0.2594 |      - |    1224 B |        2.89 |
   | Microsoft_Basic_LoggerDefine                           |   936.0 ns | 12.39 ns | 11.59 ns |  0.70 |    0.01 | 0.0467 |      - |     224 B |        0.53 |
   | Serilog_Core_Formatting_Reference_Args                 |   936.7 ns |  2.93 ns |  2.74 ns |  0.70 |    0.01 | 0.3262 | 0.0010 |    1536 B |        3.62 |
   | Serilog_Core_Formatting_Value_Args                     |   967.5 ns |  3.99 ns |  3.73 ns |  0.73 |    0.01 | 0.3357 |      - |    1584 B |        3.74 |
   | Microsoft_Basic                                        | 1,054.5 ns | 17.51 ns | 16.38 ns |  0.79 |    0.02 | 0.0324 |      - |     160 B |        0.38 |
   | Serilog_ILogger_Formatting_Reference_Args              | 1,068.0 ns |  5.43 ns |  5.08 ns |  0.80 |    0.01 | 0.3414 |      - |    1608 B |        3.79 |
   | Serilog_ILogger_Formatting_Value_Args                  | 1,098.2 ns |  3.63 ns |  2.84 ns |  0.82 |    0.01 | 0.3510 |      - |    1656 B |        3.91 |
   | Serilog_ILogger_Formatting_Reference_Args_LoggerDefine | 1,121.3 ns | 19.27 ns | 18.03 ns |  0.84 |    0.02 | 0.3433 |      - |    1616 B |        3.81 |
   | Microsoft_Formatting_Value_Args_LoggerDefine           | 1,174.0 ns |  9.90 ns |  9.26 ns |  0.88 |    0.01 | 0.0782 |      - |     368 B |        0.87 |
   | Serilog_ILogger_Formatting_Value_Args_LoggerDefine     | 1,193.0 ns |  5.88 ns |  5.50 ns |  0.89 |    0.01 | 0.3624 |      - |    1712 B |        4.04 |
   | Microsoft_Formatting_Reference_Args_LoggerDefine       | 1,225.1 ns | 14.42 ns | 12.79 ns |  0.92 |    0.01 | 0.0782 |      - |     376 B |        0.89 |
   | Microsoft_Formatting_Reference_Args                    | 1,235.6 ns | 12.46 ns | 11.66 ns |  0.93 |    0.01 | 0.0782 | 0.0038 |     376 B |        0.89 |
   | Microsoft_Formatting_Value_Args                        | 1,333.6 ns | 18.16 ns | 16.99 ns |  1.00 |    0.02 | 0.0896 |      - |     424 B |        1.00 |

   | Method                                             | Mean       | Error    | StdDev   | Ratio | RatioSD | Gen0   | Gen1   | Allocated | Alloc Ratio |
   |--------------------------------------------------- |-----------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|------------:|
   | Serilog_Core_Formatting_Value_Args                 |   959.6 ns |  3.90 ns |  3.05 ns |  0.74 |    0.01 | 0.3357 |      - |    1584 B |        3.74 |
   | Serilog_ILogger_Formatting_Value_Args              | 1,102.8 ns |  7.80 ns |  6.91 ns |  0.86 |    0.01 | 0.3510 |      - |    1656 B |        3.91 |
   | Serilog_ILogger_Formatting_Value_Args_LoggerDefine | 1,167.5 ns |  8.03 ns |  6.71 ns |  0.91 |    0.02 | 0.3624 |      - |    1712 B |        4.04 |
   | Microsoft_Formatting_Value_Args_LoggerDefine       | 1,181.4 ns | 13.61 ns | 12.73 ns |  0.92 |    0.02 | 0.0782 | 0.0019 |     368 B |        0.87 |
   | Microsoft_Formatting_Value_Args                    | 1,290.1 ns | 23.62 ns | 20.94 ns |  1.00 |    0.02 | 0.0896 |      - |     424 B |        1.00 |



 *
 */

[ MemoryDiagnoser ]
[ Orderer( SummaryOrderPolicy.FastestToSlowest ) ]
public partial class LoggingBenchmarks : IDisposable {
    private  Microsoft.Extensions.Logging.ILoggerFactory     _microsoftLoggerFactory = null!;
    internal ILogger                                         MicrosoftLogger         = null!;
    internal Serilog.ILogger                                 SerilogCoreLogger       = null!;
    internal Serilog.Core.Logger                             _serilogCoreLogger      = null!;
    private  Serilog.Extensions.Logging.SerilogLoggerFactory _serilogLoggerFactory   = null!;
    internal ILogger                                         SerilogLogger           = null!;


    [ GlobalSetup ]
    public void Setup( ) {
        Console.SetOut( TextWriter.Null );
        this.InitializeLogging();
    }

    public void InitializeLogging( ) {
        // Serilog
        var serilogConfig = new Serilog.LoggerConfiguration()
                            // .WriteTo.Sink<NullSink>();
                            // .Enrich.FromLogContext()
                            // .WriteTo.Console();
                            .WriteTo.Console( outputTemplate: @"{Timestamp:HH:mm:ss.fff} {Level:u4}: {SourceContext} {Message:l}{NewLine}", theme: ConsoleTheme.None );
        // .WriteTo.Console(outputTemplate: "{Level:u3}: {SourceContext}{NewLine}      {Message:l}{NewLine}");
        this._serilogCoreLogger    = serilogConfig.CreateLogger();
        this.SerilogCoreLogger     = _serilogCoreLogger.ForContext( "SourceContext", "TestLogger[0]" );
        this._serilogLoggerFactory = new Serilog.Extensions.Logging.SerilogLoggerFactory( SerilogCoreLogger );
        this.SerilogLogger         = _serilogLoggerFactory.CreateLogger( "TestLogger[0]" );
        // Microsoft
        // this._microsoftLoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create( loggingBuilder => loggingBuilder.AddConsole() );
        this._microsoftLoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create( loggingBuilder => loggingBuilder.AddSimpleConsole( static options => {
            // options.SingleLine = false,
            options.SingleLine = true;
            // options.IncludeScopes = true;
            options.ColorBehavior   = LoggerColorBehavior.Disabled;
            options.IncludeScopes   = false;
            options.TimestampFormat = @"HH:mm:ss.fff ";
        } ) );
        this.MicrosoftLogger = _microsoftLoggerFactory.CreateLogger( "TestLogger" );

        // 1. URGENT: TRY WITH SCOPE / CONTEXT !

        // 2. URGENT: TRY USING LOOPS -- 100 PER RUN ? for () { }
    }

    private bool _isDisposed = false;

    [ GlobalCleanup ]
    public void Cleanup( ) {
        if ( this._isDisposed ) {
            return;
        }
        _isDisposed = true;
        this._microsoftLoggerFactory.Dispose();
        this._serilogLoggerFactory.Dispose();
        this._serilogCoreLogger.Dispose();
    }

    /*
     *
     */

    [ Benchmark ]
    public void Serilog_Core_Basic( ) {
        SerilogCoreLogger.Information( "Information" );
    }

    [ Benchmark ]
    public void Serilog_ILogger_Basic( ) {
        SerilogLogger.LogInformation( "Information" );
    }

    [ Benchmark ]
    public void Microsoft_Basic( ) {
        // LoggerFactoryOptions options = new LoggerFactoryOptions(){}

        // using Microsoft.Extensions.Logging.ILoggerFactory loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create( loggingBuilder => loggingBuilder.AddConsole() );

        // using Microsoft.Extensions.Logging.LoggerFactory loggerFactory = new Microsoft.Extensions.Logging.LoggerFactory();
        // loggerFactory.AddProvider( ConsoleLoggerExtensions.AddConsole(  ) );
        MicrosoftLogger.LogInformation( "Information" );
    }
    /*
     *
     */

    [ Benchmark ]
    public void Serilog_Core_Formatting_Reference_Args( ) {
        SerilogCoreLogger.Information( "Message: '{Message}' -- arg1: {Arg1} arg2: {Arg2}", "Hello World", "1", "2" );
    }

    [ Benchmark ]
    public void Serilog_ILogger_Formatting_Reference_Args( ) {
        SerilogLogger.LogInformation( "Message: '{Message}' -- arg1: {Arg1} arg2: {Arg2}", "Hello World", "1", "2" );
    }

    [ Benchmark ]
    public void Microsoft_Formatting_Reference_Args( ) {
        MicrosoftLogger.LogInformation( "Message: '{Message}' -- arg1: {Arg1} arg2: {Arg2}", "Hello World", "1", "2" );
    }

    /*
     *
     */

    [ Benchmark ]
    public void Serilog_Core_Formatting_Value_Args( ) {
        // [08:05:34 INF] Message: 'Hello World' -- arg1: 1 arg2: 2
        SerilogCoreLogger.Information( "Message: '{Message}' -- arg1: {Arg1} arg2: {Arg2}", "Hello World", 1, 2 );
        // _serilogCoreLogger.Dispose();
        // Thread.Sleep( 50 );
        // throw new Exception("KILLED");
        //
        // _serilogCoreLogger.Information( "Information" );
        //
        // _serilogLogger.LogInformation( "Information" );
        //
        // _microsoftLogger.LogInformation( "Information" );
        // throw new Exception( "dead" );
    }

    [ Benchmark ]
    public void Serilog_ILogger_Formatting_Value_Args( ) {
        // [08:06:24 INF] Message: 'Hello World' -- arg1: 1 arg2: 2
        SerilogLogger.LogInformation( "Message: '{Message}' -- arg1: {Arg1} arg2: {Arg2}", "Hello World", 1, 2 );
        // throw new Exception();
    }

    [ Benchmark( Baseline = true ) ]
    public void Microsoft_Formatting_Value_Args( ) {
        // info: TestLogger[0]
        //       Message: 'Hello World' -- arg1: 1 arg2: 2
        MicrosoftLogger.LogInformation( "Message: '{Message}' -- arg1: {Arg1} arg2: {Arg2}", "Hello World", 1, 2 );
        // throw new Exception();
    }

    /*
     *
     */

    [ LoggerMessage( EventId = 99, Level = LogLevel.Information, Message = "Message: 'Hello World' -- arg1: 1 arg2: 2" ) ]
    static partial void noArgLog( ILogger logger );


    [ LoggerMessage( EventId = 100, Level = LogLevel.Information, Message = "Message: '{Message}' -- arg1: {Arg1} arg2: {Arg2}" ) ]
    static partial void basicLogger( ILogger logger, string message, string arg1, string arg2 );


    [ LoggerMessage( EventId = 101, Level = LogLevel.Information, Message = "Message: '{Message}' -- arg1: {Arg1} arg2: {Arg2}" ) ]
    static partial void referenceArgLogger( ILogger logger, string message, string arg1, string arg2 );


    [ LoggerMessage( EventId = 102, Level = LogLevel.Information, Message = "Message: '{Message}' -- arg1: {Arg1} arg2: {Arg2}" ) ]
    static partial void valueArgLogger( ILogger logger, string message, ushort arg1, ushort arg2 );


    /*
     *
     */

    [ Benchmark ]
    public void Serilog_ILogger_Basic_LoggerDefine( ) {
        noArgLog( SerilogLogger );
    }

    [ Benchmark ]
    public void Microsoft_Basic_LoggerDefine( ) {
        noArgLog( MicrosoftLogger );
    }

    /*
     *
     */

    [ Benchmark ]
    public void Serilog_ILogger_Formatting_Reference_Args_LoggerDefine( ) {
        referenceArgLogger( SerilogLogger, "Hello World", "1", "2" );
    }

    [ Benchmark ]
    public void Microsoft_Formatting_Reference_Args_LoggerDefine( ) {
        referenceArgLogger( MicrosoftLogger, "Hello World", "1", "2" );
    }

    /*
     *
     */

    [ Benchmark ]
    public void Serilog_ILogger_Formatting_Value_Args_LoggerDefine( ) {
        valueArgLogger( SerilogLogger, "Hello World", 1, 2 );
    }

    [ Benchmark ]
    public void Microsoft_Formatting_Value_Args_LoggerDefine( ) {
        valueArgLogger( MicrosoftLogger, "Hello World", 1, 2 );
    }

    /*
     *
     */

    public void Dispose( ) {
        this.Cleanup();
    }
}

// public class NullSink : ILogEventSink {
//     /// <inheritdoc />
//     public void Emit( LogEvent logEvent ) {
//         // throw new System.NotImplementedException();
//     }
// }
internal class CacheOutputWriter : TextWriter, ILogEventSink {
    private readonly int       _cacheSize;
    private readonly string?[] _log;
    private          int       _logSequence = -1;

    public override Encoding Encoding { get; } = Encoding.UTF8;

    public CacheOutputWriter( int cacheSize ) {
        this._cacheSize = cacheSize;
        this._log       = new string?[ this._cacheSize ];
    }

    internal string?[] Output {
        get {
            int logSequence = this._logSequence;
            if ( logSequence < 0 ) {
                return [ ];
            }
            string?[] outputArray = new string[ Int32.Min( logSequence, this._cacheSize ) ];
            for ( int i = Int32.Max( logSequence - this._cacheSize, 0 ), o = 0 ; i < logSequence ; i++, o++ ) {
                outputArray[ o ] = this._log[ ( i + 1 ) % this._cacheSize ];
            }
            return outputArray;
        }
    }

    public override void Write( string? value ) {
        var logSequence = Interlocked.Increment( ref this._logSequence );
        this._log[ logSequence % this._cacheSize ] = value;
    }

    /// <inheritdoc />
    public override void WriteLine( string? value ) => this.Write( value );

    /// <inheritdoc />
    void ILogEventSink.Emit( LogEvent logEvent ) {
        var logSequence = Interlocked.Increment( ref this._logSequence );
        this._log[ logSequence % this._cacheSize ] = logEvent.RenderMessage();
    }
}