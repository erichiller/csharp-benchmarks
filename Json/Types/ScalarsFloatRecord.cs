namespace Benchmarks.Json;

public record ScalarsFloatRecord( int Id, string Name, float Value ) {
    internal const string JSON =
        @"{
            ""Id"": 1,
            ""Name"": ""Hello World"",
            ""Value"": 9.127 
        }";
}

public class ScalarsFloatClass {
    internal const string JSON = ScalarsFloatRecord.JSON;

    public int    Id    { get; set; }
    public string Name  { get; set; }
    public float  Value { get; set; }
}

public class ScalarsFloatClassWithInitProperties {
    internal const string JSON = ScalarsFloatRecord.JSON;

    public int    Id    { get; init; }
    public string Name  { get; init; }
    public float  Value { get; init; }
}

public class ScalarsFloatClassFields {
    internal const string JSON = ScalarsFloatRecord.JSON;

    public int    Id;
    public string Name;
    public float  Value;
}


public record ScalarsFloatRecordInitNoConstructor {
    internal const string JSON = ScalarsFloatRecord.JSON;

    public int    Id    { get; init; }
    public string Name  { get; init; }
    public float  Value { get; init; }
}


public class ScalarsFloatClassInitConstructor {
    internal const string JSON = ScalarsFloatRecord.JSON;

    public ScalarsFloatClassInitConstructor( int id, string name, float value ) =>
        ( Id, Name, Value ) = ( id, name, value );

    public int    Id    { get; init; }
    public string Name  { get; init; }
    public float  Value { get; init; }
}

public class ScalarsFloatClassInitPartialConstructor {
    internal const string JSON = ScalarsFloatRecord.JSON;

    public ScalarsFloatClassInitPartialConstructor( int id ) =>
        ( Id ) = ( id );

    public int    Id    { get; init; }
    public string Name  { get; init; }
    public float  Value { get; init; }
}


public class ScalarsFloatClassSetConstructor {
    internal const string JSON = ScalarsFloatRecord.JSON;

    public ScalarsFloatClassSetConstructor( int id, string name, float value ) =>
        ( Id, Name, Value ) = ( id, name, value );

    public int    Id    { get; set; }
    public string Name  { get; set; }
    public float  Value { get; set; }
}

public class ScalarsFloatClassSetPartialConstructor {
    internal const string JSON = ScalarsFloatRecord.JSON;

    public ScalarsFloatClassSetPartialConstructor( int id ) =>
        ( Id ) = ( id );

    public int    Id    { get; set; }
    public string Name  { get; set; }
    public float  Value { get; set; }
}