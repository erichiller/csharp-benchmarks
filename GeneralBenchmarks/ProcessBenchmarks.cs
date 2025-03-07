using System;
using System.IO;
using System.Linq;
using System.Reflection;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

using NodaTime;
using NodaTime.Extensions;

namespace Benchmarks.General;

/*
 *
 * 
   | Method   | Mean [us] | Error [us] | StdDev [us] | Ratio | Gen0    | Gen1   | Gen2   | Allocated [B] | Alloc Ratio |
   |--------- |----------:|-----------:|------------:|------:|--------:|-------:|-------:|--------------:|------------:|
   | Try1     |  607.9 us |    5.51 us |     5.16 us |  0.98 | 34.0000 | 3.0000 | 1.0000 |      183510 B |        1.00 |
   | BaseLine |  618.3 us |   10.10 us |     9.45 us |  1.00 | 34.0000 | 3.0000 | 1.0000 |      182948 B |        1.00 |
   
 */

[ Config( typeof(BenchmarkConfig) ) ]
public class ProcessBenchmarks {
    private static string formatApplicationStartupTimeToFileName( ZonedDateTime zonedDateTime ) => zonedDateTime.ToString( @"uuuuMMddHHmmssx", System.Globalization.CultureInfo.InvariantCulture );

    private static ZonedDateTime startTimeToZonedDateTime( System.DateTime startTime ) =>
        new DateTimeOffset( startTime ).ToZonedDateTime().WithZone( DateTimeZoneProviders.Tzdb.GetSystemDefault() );

    private static ZonedDateTime? getProcessStartTime( System.Diagnostics.Process process ) =>
        process is { HasExited: false, StartTime: { } startTime }
            ? startTimeToZonedDateTime( startTime )
            : null;

    [ Benchmark( Baseline = true) ]
    public string[] BaseLine( ) {
        var relatedProcessStartTimes = System.Diagnostics.Process
                                             .GetProcesses()
                                             .Where( p => p.Modules.Cast<System.Diagnostics.ProcessModule>()
                                                           .Any( m => m.ModuleName.Equals( "System.Private.CoreLib.dll", StringComparison.OrdinalIgnoreCase ) )
                                                          && p is { MainModule.FileName: { } mainModuleFileName }
                                                          && Directory.GetParent( mainModuleFileName ) is { } remoteDirectory
                                                          && remoteDirectory.GetFiles().Any( f => f.Name == Path.GetFileName( Assembly.GetEntryAssembly()?.Location ) )
                                                          && p.Id != System.Environment.ProcessId
                                             )
                                             .Select( static process => getProcessStartTime( process ) )
                                             .OfType<ZonedDateTime>()
                                             .Select( static startTime => formatApplicationStartupTimeToFileName( startTime ) )
                                             .ToArray();
        return relatedProcessStartTimes;
    }
    [ Benchmark ]
    public string[] Try1( ) {
        var relatedProcessStartTimes = System.Diagnostics.Process
                                             .GetProcesses()
                                             .Where( static p => p.Modules.Cast<System.Diagnostics.ProcessModule>()
                                                                  .Any( m => m.ModuleName.Equals( "System.Private.CoreLib.dll", StringComparison.OrdinalIgnoreCase ) )
                                                                 && p is { MainModule.FileName: { } mainModuleFileName }
                                                                 && Directory.GetParent( mainModuleFileName ) is { } remoteDirectory
                                                                 && remoteDirectory.GetFiles().Any( f => f.Name == Path.GetFileName( Assembly.GetEntryAssembly()?.Location ) )
                                                                 && p.Id != System.Environment.ProcessId
                                             )
                                             .Select( static process => getProcessStartTime( process ) )
                                             .OfType<ZonedDateTime>()
                                             .Select( static startTime => formatApplicationStartupTimeToFileName( startTime ) )
                                             .ToArray();
        return relatedProcessStartTimes;
    }
    [ Benchmark ]
    public void Try2( ) {
        var relatedProcessStartTimes = System.Diagnostics.Process
                                             .GetProcesses()
                                             .Where( static p => 
                                                         // p.Modules.Cast<System.Diagnostics.ProcessModule>()
                                                         // .Any( m => m.ModuleName.Equals( "System.Private.CoreLib.dll", StringComparison.OrdinalIgnoreCase ) )
                                                         // && 
                                                         p is { MainModule.FileName: { } mainModuleFileName }
                                                         && Directory.GetParent( mainModuleFileName ) is { } remoteDirectory
                                                         //  && remoteDirectory.GetFiles().Any( f => f.Name == Path.GetFileName( Assembly.GetEntryAssembly()?.Location ) )
                                                         && p.Id != System.Environment.ProcessId
                                             )
                                             .Select( static p => p.ToString() )
                                             .ToArray();
    }
    [ Benchmark ]
    public void GetProcessesOnly( ) {
        var relatedProcessStartTimes = System.Diagnostics.Process
                                             .GetProcesses();
    }
    [ Benchmark ]
    public void GetProcesses_FilterOnIdOnly( ) {
        var relatedProcessStartTimes = System.Diagnostics.Process
                                             .GetProcesses()
                                             .Where( static  p => p.Id != 555 )
                                             .ToArray();
    }
    [ Benchmark ]
    public void GetProcessesByName( ) {
        var relatedProcessStartTimes = System.Diagnostics.Process.GetProcessesByName( "mkmrk-cli" );
    }
}