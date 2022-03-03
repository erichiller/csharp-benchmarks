namespace Benchmarks.Json;

public record ScalarsFloat( int Id, string Name, float Value ) {
    internal const string JSON =
        @"{
            ""Id"": 1,
            ""Name"": ""Hello World"",
            ""Value"": 9.127 
        }";
}

public class ScalarsFloatClass {
    internal const string JSON = ScalarsFloat.JSON;

    public int    Id    { get; set; }
    public string Name  { get; set; }
    public float  Value { get; set; }
}

public class ScalarsFloatClassFields {
    internal const string JSON = ScalarsFloat.JSON;

    public int    Id;
    public string Name;
    public float  Value;
}