using System.Globalization;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Validators;

using Microsoft.Extensions.Logging;

namespace Benchmarks.Common;

public class BenchmarkConfig : ManualConfig {
    public BenchmarkConfig( ) {
        // AddJob( new Job1(), new Job2() );
        // AddColumn( new Column1(), new Column2() );
        // AddColumnProvider( new ColumnProvider1(), new ColumnProvider2() );
        // GetExporters();
        WithSummaryStyle( new SummaryStyle(
                              cultureInfo: CultureInfo.CurrentCulture,
                              sizeUnit: SizeUnit.B,
                              printUnitsInHeader: true,
                              timeUnit: null,
                              printUnitsInContent: true,
                              maxParameterColumnWidth: 20,
                              printZeroValuesInContent: false,
                              ratioStyle: RatioStyle.Value
                          ) );
        AddExporter(
            MarkdownExporter.GitHub,
            // HtmlExporter.Default, // TODO: re-enable
            CsvExporter.Default, // TODO: re-enable
            JsonExporter.Default //, // TODO: re-enable
            // RPlotExporter.Default // TODO: re-enable
        );
        // AddLogger( new Logger1(), new Logger2() );
        AddLogger( new [] { new BenchmarkDotNet.Loggers.ConsoleLogger( ) } );
        AddDiagnoser( MemoryDiagnoser.Default );
        // AddDiagnoser( EventPipeProfiler.Default ); // generates output for profiling. TODO: re-enable
        // this.WithOptions( ConfigOptions.StopOnFirstError ) // TODO: re-enable
            // .WithOptions( ConfigOptions.JoinSummary ) // TODO: re-enable
            // ;
        Orderer = new DefaultOrderer( SummaryOrderPolicy.FastestToSlowest ); // TODO: re-enable
        // AddValidator( ExecutionValidator.FailOnError ); // TODO: re-enable
        // AddValidator( ReturnValueValidator.FailOnError ); // TODO: re-enable
        // AddHardwareCounters( HardwareCounter enum1, HardwareCounter enum2 );
        // AddFilter( new Filter1(), new Filter2() );
        // AddLogicalGroupRules( BenchmarkLogicalGroupRule enum1, BenchmarkLogicalGroupRule enum2 );
    }
    
    
    

    public static ILogger<TLogger> GetLogger<TLogger>( ) {
        System.Console.WriteLine( "GetLogger<TLogger>" );
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

// public class GenericLogger : BenchmarkDotNet.Loggers.LogCapture