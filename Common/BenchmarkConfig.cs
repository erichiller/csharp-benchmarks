using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Exporters.Json;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Validators;

namespace Common;

public class BenchmarkConfig : ManualConfig {
    public BenchmarkConfig( ) {
        // AddJob( new Job1(), new Job2() );
        // AddColumn( new Column1(), new Column2() );
        // AddColumnProvider( new ColumnProvider1(), new ColumnProvider2() );
        AddExporter( MarkdownExporter.GitHub, HtmlExporter.Default, CsvExporter.Default,
                     JsonExporter.Default,
                     RPlotExporter.Default // TODO: re-enable
        );
        // AddLogger( new Logger1(), new Logger2() );
        // AddDiagnoser( new MemoryDiagnoser(new MemoryDiagnoserConfig( true ) {}) );
        AddDiagnoser( MemoryDiagnoser.Default );
        AddDiagnoser( EventPipeProfiler.Default ); // generates output for profiling. TODO: re-enable
        this.WithOptions( ConfigOptions.StopOnFirstError )
            .WithOptions( ConfigOptions.JoinSummary )
            ;
        // this.WithOrderers(SummaryOrderPolicy.FastestToSlowest );
        // SummaryOrderPolicy = SummaryOrderPolicy.FastestToSlowest;
        Orderer = new DefaultOrderer( SummaryOrderPolicy.FastestToSlowest );
        // AddAnalyser(  );
        AddValidator( ExecutionValidator.FailOnError );
        // AddValidator( ReturnValueValidator.FailOnError );
        // AddHardwareCounters( HardwareCounter enum1, HardwareCounter enum2 );
        // AddFilter( new Filter1(), new Filter2() );
        // AddLogicalGroupRules( BenchmarkLogicalGroupRule enum1, BenchmarkLogicalGroupRule enum2 );
    }
}