
// #define DEBUG
#undef DEBUG

using System.Linq;

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

using Microsoft.Extensions.Logging;

namespace Benchmarks.Json;

public class Program {
#if DEBUG
    static void Main( string[] args ) {
        var simpleBenchmarks = new SystemTextJsonSerializationBasic() {
            Iterations = 1
        };
        // var output = new SystemTextJsonSerializationBasic() {
        //     Iterations = 1
        // }.SystemTextJson_Deserialize_Scalars_FloatFields_SourceGen().First();
        // // URGENT -- try this. Is anything being returned?
        // System.Console.WriteLine( $"{output.Id} , {output.Name} , {output.Value}" );
        // System.Console.WriteLine(simpleBenchmarks.SystemTextJson_Deserialize_Scalars_FloatFields_SourceGen()
        //                                    .Select( output => $"{output.Id} , {output.Name} , {output.Value}" )
        //                                    .First() );
        //
        // System.Console.WriteLine(simpleBenchmarks.SystemTextJson_Deserialize_Scalars_Float_SourceGen()
        //                                          .Select( output => $"{output.Id} , {output.Name} , {output.Value}" )
        //                                          .First() );
        
        System.Console.WriteLine(simpleBenchmarks.SystemTextJson_Deserialize_Scalars_NodaTime_InstantAsTimestamp()
                                                 .Select( output => $"{output.Id} , {output.Name} , {output.Value}" )
                                                 .First() );
        System.Console.WriteLine(simpleBenchmarks.SystemTextJson_Deserialize_Scalars_NodaTime_SourceGen()
                                                 .Select( output => $"{output.Id} , {output.Name} , {output.Value}" )
                                                 .First() );
        System.Console.WriteLine(simpleBenchmarks.SystemTextJson_Deserialize_Scalars_NodaTimeWithAttribute()
                                                 .Select( output => $"{output.Id} , {output.Name} , {output.Value}" )
                                                 .First() );
        System.Console.WriteLine(simpleBenchmarks.SystemTextJson_Deserialize_Scalars_NodaTimeWithAttribute_SourceGen()
                                                 .Select( output => $"{output.Id} , {output.Name} , {output.Value}" )
                                                 .First() );
        //
        // var levelOneBenchmarks = new LevelOneJsonBenchmarks() {
        //     Iterations = 1,
        //     WithSourceGenerationContext = true,
        //     LevelOneJsonFile = "Multiple"
        // };
        // System.Console.WriteLine(levelOneBenchmarks.SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne_SourceGenWithoutOptions()
        //                                            // .Select( output => output is null ? "null" : $"{output.Service} , {output.Timestamp} , {output.Command}" )
        //                                            // .First()
        //     );
        //
        // System.Console.WriteLine(levelOneBenchmarks.SystemTextJson_JsonSerializer_ReadAhead_Deserialize_ResponseContentSlim_SourceGenWithoutOptions()
        //                                            // .Select( output => output is null ? "null" : $"{output.Service} , {output.Timestamp} , {output.Command}" )
        //                                            // .First()
        //                                            );
        
        // URGENT -- try this. Is anything being returned?
    }
    //     // => new SystemTextJsonSerializationBasic().SystemTextJson_Deserialize_Scalars_NodaTimeWithAttribute();
    //     => new SystemTextJsonSerializationBasic(){Iterations=1}.SystemTextJson_Deserialize_Scalars_NodaTimeWithAttribute_SourceGen();
    //     // => new LevelOneJsonBenchmarks().SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne();
#else
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
#endif

}