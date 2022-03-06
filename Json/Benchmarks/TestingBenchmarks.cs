using System;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.Json; 

[ Config( typeof(BenchmarkConfig) ) ]
public class TestingBenchmarks {
    

    // ReSharper disable once UnassignedField.Global
    // ReSharper disable once MemberCanBePrivate.Global
    [ Params( 1000 ) ]
    public int Iterations = 1;

    const string jsonString = @"{
    ""Key"": ""/ESH22"",
    ""BidPrice"": 4262,
    ""AskPrice"": 4262.25,
    ""LastPrice"": 4262.25,
    ""BidSize"": 6,
    ""AskSize"": 6,
    ""QuoteTime"": 1645774524380,
    ""TradeTime"": 1645774524012,
    ""NetChange"": -21.75,
    ""FuturePercentChange"": -0.0051,
    ""Mark"": 4262
    }
";
    private readonly ResponseContentSlimGenerationContext _responseContentSlimGenerationContextWithoutOptions = new ResponseContentSlimGenerationContext();

    [ Benchmark ]
    public ResponseContentSlim[] ResponseContentSlim_SourceGen( ) {
        ResponseContentSlim[] results = new ResponseContentSlim[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[i] = System.Text.Json.JsonSerializer.Deserialize<ResponseContentSlim>( jsonString, _responseContentSlimGenerationContextWithoutOptions.ResponseContentSlim )
                ?? throw new NullReferenceException();
        }

        return results;
    }
    
    [ Benchmark ]
    public ResponseContentSlim[] ResponseContentSlim_NoSourceGen( ) {
        ResponseContentSlim[] results = new ResponseContentSlim[ Iterations ];
        for ( int i = 0 ; i < Iterations ; i++ ) {
            results[ i ] = System.Text.Json.JsonSerializer.Deserialize<ResponseContentSlim>( jsonString )
                           ?? throw new NullReferenceException();
        }

        return results;
    }
    

}




/* *********************** TESTING ********************************* */


[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Metadata
)]
[JsonSerializable(typeof(ResponseContentSlim))]
internal partial class ResponseContentSlimGenerationContext : JsonSerializerContext
{
}






/// <summary>
/// The docs list in one part that there is a 7th property. This is an error, ignore it, no data for "7" is actually sent.
/// </summary>
public record ResponseContentSlim {
    // /// <summary>Ticker symbol in UPPERCASE</summary>
    // [JsonProperty( "0" )]
    // public string Symbol { get; set; }

    // /// <summary>Current Best Bid Price</summary>
    // // [ System.Text.Json.Serialization.JsonPropertyName( "1" ) ]
    // public decimal? BidPrice { get; set; }
    //
    // /// <summary>Current Best Ask Price</summary>
    // // [ System.Text.Json.Serialization.JsonPropertyName( "2" ) ]
    // public decimal? AskPrice { get; set; }
    //
    // /// <summary>Price at which the last trade was matched</summary>
    // // [ System.Text.Json.Serialization.JsonPropertyName( "3" ) ]
    // public decimal? LastPrice { get; set; }
    //
    // /// <summary>Number of shares for bid</summary>
    // // [ System.Text.Json.Serialization.JsonPropertyName( "4" ) ]
    // public int? BidSize { get; set; }
    //
    // /// <summary>Number of shares for ask</summary>
    // // [ System.Text.Json.Serialization.JsonPropertyName( "5" ) ]
    // public int? AskSize { get; set; }

    // /// <summary>Trade time of the last quote in milliseconds since epoch</summary>
    // [System.Text.Json.Serialization.JsonConverter(typeof(InstantUnixTimeMillisecondsConverter))]
    // // [ System.Text.Json.Serialization.JsonPropertyName( "10" ) ]
    // public Instant QuoteTime { get; set; }
    //
    // /// <summary>Trade time of the last trade in milliseconds since epoch</summary>
    // [System.Text.Json.Serialization.JsonConverter(typeof(InstantUnixTimeMillisecondsConverter))]
    // // [ System.Text.Json.Serialization.JsonPropertyName( "11" ) ]
    // public Instant TradeTime { get; set; }

    // /// <summary>
    // /// **Current Last-Prev Close**
    // /// ```
    // /// If(close>0){
    // ///     change = last – close
    // /// } else { change=0 }
    // /// ```
    // /// </summary>
    // // [ System.Text.Json.Serialization.JsonPropertyName( "19" ) ]
    // public decimal? NetChange { get; set; }
    //
    // /// <summary>Current percent change</summary>
    // // [ System.Text.Json.Serialization.JsonPropertyName( "20" ) ]
    // public decimal? FuturePercentChange { get; set; }
    //
    // /// <summary>
    // /// **Mark-to-Market**  
    // /// value is calculated daily using current prices to determine profit/loss
    // /// </summary>
    // // [ System.Text.Json.Serialization.JsonPropertyName( "24" ) ]
    // public decimal? Mark { get; set; }

    /// <summary>Ticker symbol in upper case.  example: `/ES`</summary>
    // [System.Text.Json.Serialization.JsonPropertyName("key")]
    public string Key { get; set; }
}
/*
 * 
|                                                                                          Method | Iterations | WithSourceGenerationContext |     Mean |     Error |    StdDev |    Gen 0 |   Gen 1 | Allocated |
|------------------------------------------------------------------------------------------------ |----------- |---------------------------- |---------:|----------:|----------:|---------:|--------:|----------:|
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_ResponseContentSlim_SourceGenWithoutOptions |       1000 |                        True | 2.260 ms | 0.0179 ms | 0.0168 ms | 160.1563 | 15.6250 |    736 KB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_ResponseContentSlim_SourceGenWithoutOptions |       1000 |                       False | 2.297 ms | 0.0187 ms | 0.0175 ms | 160.1563 | 15.6250 |    736 KB |

 */

