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