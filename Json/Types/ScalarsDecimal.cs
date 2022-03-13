namespace Benchmarks.Json;

public record ScalarsDecimal( int Id, string Name, decimal Value ) {
    internal const string JSON = ScalarsFloatRecord.JSON;
}

public class ScalarsDecimalClass {
    internal const string JSON = ScalarsDecimal.JSON;

    public int     Id   { get; set; }
    public string  Name { get; set; }
    public decimal Value { get; set; }
}


public class ScalarsDecimalClassInitConstructor {

    public ScalarsDecimalClassInitConstructor( int id, string name, decimal value ) =>
        ( Id, Name, Value ) = ( id, name, value );

    internal const string JSON = ScalarsDecimal.JSON;
    
    public int     Id    { get; init; }
    public string  Name  { get; init; }
    public decimal Value { get; init; }
}