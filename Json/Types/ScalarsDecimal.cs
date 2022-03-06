namespace Benchmarks.Json;

public record ScalarsDecimal( int Id, string Name, decimal Value ) {
    internal const string JSON = ScalarsFloat.JSON;
}

public class ScalarsDecimalClass {
    internal const string JSON = ScalarsDecimal.JSON;

    public int     Id   { get; set; }
    public string  Name { get; set; }
    public decimal Value { get; set; }
}