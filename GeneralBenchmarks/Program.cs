using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace Benchmarks.General;

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
}