namespace Benchmarks.Json;

public record NestedObjectArrayNodaTime( int Id, string Name, ScalarsNodaTime[] Value ) {
    internal const string JSON =
        $@"{{
            ""Id"": 1,
            ""Name"": ""Hello World"",
            ""Value"": [
                {ScalarsNodaTime.JSON},
                {ScalarsNodaTime.JSON}
            ]
        }}";
}

public class NestedObjectArrayNodaTimeClass {
    internal const string JSON = NestedObjectArrayNodaTime.JSON;
    
    public int                    Id    { get; set; }
    public string                 Name  { get; set; }
    public ScalarsNodaTimeClass[] Value { get; set; }
}

public class NestedObjectArrayNodaTimeClassInitConstructor {
    internal const string JSON = NestedObjectArrayNodaTime.JSON;
    
    public NestedObjectArrayNodaTimeClassInitConstructor( int id, string name, ScalarsNodaTimeClass[] value ) =>
        ( Id, Name, Value ) = ( id, name, value );
    public int    Id   { get; init; }
    public string Name { get; init; }
    public ScalarsNodaTimeClass[] Value { get; init; }
}