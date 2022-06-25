// See https://aka.ms/new-console-template for more information

using System.ServiceModel;

using ProtoBuf;
using ProtoBuf.Meta;

namespace Benchmarks.Rpc.GrpcReflectionExample;

internal class Program {
    public static async Task<int> Main( string[] args ) {
        Console.WriteLine( "Hello, World!" );

        // var generator2 = new ProtoBuf.Reflection.CSharpCodeGenerator();
        
        var generator = new ProtoBuf.Grpc.Reflection.SchemaGenerator
        {
            ProtoSyntax = ProtoSyntax.Proto3
        };

        var schema = generator.GetSchema<ICalculator>(); // there is also a non-generic overload that takes Type

        using (var writer = new System.IO.StreamWriter("services.proto"))
        {
            await writer.WriteAsync(schema);
        }
        
        return 0;
    }
}

[ ProtoContract ]
public class Foo {
    [ ProtoMember( 1 ) ] public int Id { get; set; }

    [ ProtoMember( 2 ) ] public string Name { get; set; }

    // [ProtoMember(2)]
    [ ProtoMember( 3 ) ] public double Value       { get; set; }
    [ ProtoMember( 4 ) ] public string Description { get; set; }
}


// [ServiceContract(Name = "Hyper.Calculator")]
[ServiceContract]
public interface ICalculator
{
    ValueTask<MultiplyResult> MultiplyAsync(MultiplyRequest request);
}
        
[ProtoContract]
public class MultiplyRequest
{
    [ProtoMember(1)]
    public int X { get; set; }

    [ProtoMember(2)]
    public int Y { get; set; }
}

[ProtoContract]
public class MultiplyResult
{
    [ProtoMember(1)]
    public int Result { get; set; }
}