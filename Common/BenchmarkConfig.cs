using System;
using System.Globalization;
using System.Linq;

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Reports;

using Microsoft.Extensions.Logging;

using ILogger = BenchmarkDotNet.Loggers.ILogger;

namespace Benchmarks.Common;

public class BenchmarkConfig : ManualConfig {
    
    public BenchmarkConfig( ) {
        // AddJob( new Job1(), new Job2() );
        // AddColumn( new Column1(), new Column2() );
        // AddColumnProvider( new ColumnProvider1(), new ColumnProvider2() );
        // GetExporters();
        
        // UnionRule = ConfigUnionRule.AlwaysUseLocal;
        WithSummaryStyle( new SummaryStyle(
                              // cultureInfo: CultureInfo.CurrentCulture,
                              cultureInfo: CultureInfo.CreateSpecificCulture("en-US"),
                              sizeUnit: SizeUnit.B,
                              printUnitsInHeader: true,
                              timeUnit: null,
                              printUnitsInContent: true,
                              maxParameterColumnWidth: 20,
                              printZeroValuesInContent: false,
                              ratioStyle: RatioStyle.Value
                          ) );
        // if ( this.GetExporters().Any( exporter => exporter.GetType() == typeof(MarkdownExporter)) )
        AddExporter(
            MarkdownExporter.GitHub,
            // HtmlExporter.Default, // TODO: re-enable
            CsvExporter.Default, // TODO: re-enable
            JsonExporter.Default //, // TODO: re-enable
            // RPlotExporter.Default // TODO: re-enable
        );
        // AddJob( new Job( "Select" ) { Environment = { EnvironmentVariables = new[] { new EnvironmentVariable( "Autosummarize", "on" ) } } } );
        
        // AddLogger( new Logger1(), new Logger2() );
        AddLogger( new ILogger[] { new BenchmarkDotNet.Loggers.ConsoleLogger( ) } );
        // AddColumn( RankColumn.Arabic );
        AddDiagnoser( MemoryDiagnoser.Default );
        // AddDiagnoser( EventPipeProfiler.Default ); // generates output for profiling. TODO: re-enable
        this.WithOptions( ConfigOptions.StopOnFirstError ) // TODO: re-enable
            .WithOptions( ConfigOptions.JoinSummary ) // TODO: re-enable
            ;
        Orderer = new DefaultOrderer( SummaryOrderPolicy.FastestToSlowest ); // TODO: re-enable // URGENT!
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


/*
 * Sample: IntroConfigSource
 * You can define own config attribute.
 * https://benchmarkdotnet.org/articles/configs/configs.html#sample-introconfigsource
 */
/// <summary>
/// Dry-x64 jobs for specific jits
/// </summary>
public class MyConfigSourceAttribute : Attribute, IConfigSource
{
    public IConfig Config { get; }

    // public MyConfigSourceAttribute(params Jit[] jits)
    // {
    //     var jobs = jits
    //                .Select(jit => new Job(Job.Dry) { Environment = { Jit = jit, Platform = Platform.X64 } })
    //                .ToArray();
    //     Config = ManualConfig.CreateEmpty().AddJob(jobs);
    // }
}