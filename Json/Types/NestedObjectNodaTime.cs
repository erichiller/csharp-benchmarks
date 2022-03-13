namespace Benchmarks.Json;

public record NestedObjectNodaTime( int Id, string Name, ScalarsNodaTime Value ) {
    internal const string JSON =
        $@"{{
            ""Id"": 1,
            ""Name"": ""Hello World"",
            ""Value"": {ScalarsNodaTime.JSON}
        }}";
}
public class NestedObjectNodaTimeClass {
    internal const string JSON = NestedObjectNodaTime.JSON;
    
    public int                  Id    { get; set; }
    public string               Name  { get; set; }
    public ScalarsNodaTimeClass Value { get; set; }
}
public class NestedObjectNodaTimeClassInitConstructor {
    internal const string JSON = NestedObjectNodaTime.JSON;
    
    public NestedObjectNodaTimeClassInitConstructor( int id, string name, ScalarsNodaTimeClass value ) =>
        ( Id, Name, Value ) = ( id, name, value );
    public int Id { get; init; }
    public string Name { get; init; }
    public ScalarsNodaTimeClass Value { get; init; }
}