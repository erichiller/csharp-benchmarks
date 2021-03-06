using System.Text.Json.Serialization;

namespace Benchmarks.Json;

public record ScalarsNodaTime( int Id, string Name, NodaTime.Instant Value ) {
    // internal const string JSON = @"{
    //         ""Id"": 1,
    //         ""Name"": ""Hello World"",
    //         ""Value"": ""2022-03-05T09:05:07Z"" 
    //     }";
    internal const string JSON = @"{
            ""Id"": 1,
            ""Name"": ""Hello World"",
            ""Value"": 1645774524703
        }";
}

public class ScalarsNodaTimeClass {
    internal const string           JSON                  = ScalarsNodaTime.JSON;
    // internal const string           JsonInstantAsUnixtime = ScalarsNodaTime.JsonInstantAsUnixtime;
    public         int              Id    { get; set; }
    public         string           Name  { get; set; }
    public         NodaTime.Instant Value { get; set; }
}

public class ScalarsNodaTimeClassWithAttribute {
    /*language=json*/
    internal const string JSON =
        @"{
            ""Id"": 1,
            ""Name"": ""Hello World"",
            ""Value"": 1645774524703
        }";
    public int    Id   { get; set; }
    public string Name { get; set; }

    [ JsonConverter( typeof(InstantUnixTimeMillisecondsConverter) ) ]
    public NodaTime.Instant Value { get; set; }
}

public class ScalarsNodaTimeClassInitConstructor {
    internal const string JSON = ScalarsNodaTime.JSON;
    public ScalarsNodaTimeClassInitConstructor( int id, string name, NodaTime.Instant value ) =>
        ( Id, Name, Value ) = ( id, name, value );
    public int    Id   { get; init; }
    public string Name { get; init; }
    public NodaTime.Instant Value { get; init; }
}