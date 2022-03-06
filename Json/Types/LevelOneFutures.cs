#nullable enable
// disable warning: Non-nullable property 'xxx' is uninitialized. Consider declaring the property as nullable.
#pragma warning disable CS8618

using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

using Newtonsoft.Json;

using NodaTime;


/*
 * Question: LevelOneFutures is a singular Quote ?
 * Yields a SUB
 */
namespace Benchmarks.Json;

public enum ExchangeId {
    UNKNOWN = 0,

    /****** CME ******/
    /// <summary>
    /// Chicago Board of Trade - Division of CME Group
    /// </summary>
    /// <seealso href="https://www.cmegroup.com/company/cbot.html"/>
    CBOT = 'c',

    /// <summary>
    /// Chicago Mercantile Exchange
    /// </summary>
    /// <seealso href="https://www.cmegroup.com/company/cme.html"/>
    CME = 'C',

    /// <summary>
    /// COMEX - Commodities Exchange, Inc, Division of CME Group
    /// <para />
    /// Acquired by CME along with <see cref="NYMEX"/> in 2008 
    /// </summary>
    /// <seealso href="https://www.cmegroup.com/company/comex.html"/>
    CMX = 'x', // 

    /// <summary>
    /// Kansas City Board of Trade , now CME
    /// </summary>
    KCBT = 'K',

    /// <summary>
    /// NYMEX Division - New York Mercantile Exchange, Division of CME Group
    /// <para />
    /// MRCI sometimes lists this as NYM or NYME
    /// </summary>
    /// <seealso href="https://www.cmegroup.com/company/nymex.html"/>
    NYMEX = 'n',

    /// <summary>
    /// CME Swaps - NYMEX
    /// </summary>
    NYMSW = 'w',

    /// <summary>
    /// Dubai Mercantile Exchange
    /// <para />
    /// Launched in 2007, DME is a joint venture between 
    /// Dubai Holding, Oman Investment Authority and CME Group
    /// DME lists the Oman Crude Oil Futures Contract (DME Oman) as its flagship contract
    /// </summary>
    /// <seealso href="https://www.dubaimerc.com/" />
    DME = 'd',

    /// <summary>
    /// Minneapolis Grain Exchange, http://www.mgex.com/
    /// </summary>
    MGE = 'G',

    MATIF = 'F', // ParisBourse SA
    SFE   = 'S', // Sydney Futures Exchange

    LIFFE = 'L', // London International Financial Futures Exchange
    EUREX = 'X', // Eurex

    /****** ICE ******/
    /// <summary>
    /// International Commodity Exchange
    /// </summary>
    ICE = 'I',

    /// <summary>
    /// Winnipeg Commodity Exchange
    /// ( Now a part of ICE )
    /// <see href="https://en.wikipedia.org/wiki/Winnipeg_Commodity_Exchange" />
    /// </summary>
    WCE = 'W',

    /// <summary>
    /// New York Board of Trade , as of 2006 its a part of ICE.
    /// </summary>
    NYBOT = 'N',
    SGX      = 's', // aka SIMEX, Singapore Exchange
    EURONEXT = 'E',


    LCE  = 'l',
    CBOE = 'B',
    JPX  = 'J', // Japan Exchange
    HKFE = 'H', // Hong Kong Financial Exchange
    KFE  = 'k',
    ISE  = 'i', // Italian Stock Exchange

    /// <summary>
    /// FEX Global - Australian Futures Exchange
    /// <see href="https://www.fexglobal.com.au/" />
    /// </summary>
    FEX = 'f'
}

public enum Service {
    ADMIN,
    LEVELONE_EQUITY,
    LEVELONE_FUTURES,
    /// <summary>
    /// This Service is used to request streaming updates for one or more accounts associated with the logged in User ID. 
    /// <para /> Common usage would involve issuing the OrderStatus API request to get all transactions for an account, and subscribing to ACCT_ACTIVITY to get any updates.
    /// </summary>
    /// <remarks>Subscription only Command</remarks>
    ACCT_ACTIVITY,

    /// <summary>Actives shows the day’s top most traded symbols in the four exchanges</summary>
    /// <remarks>Subscription Only</remarks>
    /// <para />
    /// - <see cref="Service.ACTIVES_NASDAQ" />  
    /// - <see cref="Service.ACTIVES_NYSE" />  
    /// - <see cref="Service.ACTIVES_OPTIONS" />  
    /// - <see cref="Service.ACTIVES_OTCBB" />  
    ACTIVES_NASDAQ,
    ACTIVES_NYSE,
    ACTIVES_OPTIONS,
    ACTIVES_OTCBB,
    /// <summary>Chart provides  streaming one minute OHLCV (Open/High/Low/Close/Volume) for a one minute period .</summary>
    /// <para />
    /// <remarks>
    /// <see cref="Command.SUBS" /> and <see cref="Command.ADD" />
    /// </remarks>
    CHART_EQUITY,
    /// <summary>`CHART_OPTIONS` and `CHART_FUTURES` are subscription only. </summary>
    CHART_FUTURES,
    CHART_OPTIONS,
    /// <summary>
    /// Chart history for equities is available via requests to the MDMS Services.
    /// Only Futures chart history is available via Streamer Server.
    /// <para />
    /// Valid Commands:  
    /// - `<see cref="Command.GET" />`
    /// </summary>
    CHART_HISTORY_FUTURES,
    NEWS_HEADLINE,
    TIMESALE_FUTURES
}

public enum Command {
    /// <summary>Log in to Streamer Server to begin subscribing for data</summary>
    LOGIN,
    STREAM,
    /// <summary>Change quality of Service of data update rate.</summary>
    QOS,
    /// <summary>Subscription</summary>
    SUBS,
    ADD,
    UNSUBS,
    VIEW,
    /// <summary>Log out of Streamer Server to end streaming session.</summary>
    LOGOUT,
    /// <summary>Subscribe to data</summary>
    GET
}

// [JsonObjectAttribute( NamingStrategyType = typeof( LowerCaseNamingStrategy ) )]
public class DataContainer<T> {
    
    [System.Text.Json.Serialization.JsonPropertyName("data")]
    public IList<T> Data { get; set; }
}

/// <summary> wrap with `DataContainer`</summary>
public record Response {
    [System.Text.Json.Serialization.JsonPropertyName("service")]
    public Service Service = Service.LEVELONE_FUTURES;

    [ Newtonsoft.Json.JsonConverter( typeof(NodaTimeUnixMillisecondTimestampConverter) ) ]
    [System.Text.Json.Serialization.JsonPropertyName("timestamp")]
    [System.Text.Json.Serialization.JsonConverter(typeof(InstantUnixTimeMillisecondsConverter))]
    public Instant Timestamp { get; set; }

    
    [System.Text.Json.Serialization.JsonPropertyName("command")]
    public Command Command = Command.SUBS;

    [System.Text.Json.Serialization.JsonPropertyName("content")]
    public IList<ResponseContent> Content { get; set; }
}

public enum TdExchangeId {
    [ EnumMember( Value = "?" ) ]
    UNKNOWN = ExchangeId.UNKNOWN,

    [ EnumMember( Value = "I" ) ]
    ICE = ExchangeId.ICE,

    [ EnumMember( Value = "E" ) ]
    CME = ExchangeId.CME,

    [ EnumMember( Value = "L" ) ]
    LIFFEUS = ExchangeId.LIFFE
}

/// <summary>
/// Indicates a symbols current trading status, Normal, Halted, Closed
/// </summary>
public enum SecurityStatus {
    Unknown = 0,
    Normal,
    Halted,
    Closed
}

/// <summary>
/// The docs list in one part that there is a 7th property. This is an error, ignore it, no data for "7" is actually sent.
/// </summary>
public record ResponseContent {
    // /// <summary>Ticker symbol in UPPERCASE</summary>
    // [JsonProperty( "0" )]
    // public string Symbol { get; set; }

    /// <summary>Current Best Bid Price</summary>
    [ JsonProperty( "1" ) ] // URGENT: remove these once done with NewtonSoft
    [ System.Text.Json.Serialization.JsonPropertyName( "1" ) ]
    public decimal? BidPrice { get; set; }

    /// <summary>Current Best Ask Price</summary>
    [ JsonProperty( "2" ) ]
    [ System.Text.Json.Serialization.JsonPropertyName( "2" ) ]
    public decimal? AskPrice { get; set; }

    /// <summary>Price at which the last trade was matched</summary>
    [ JsonProperty( "3" ) ]
    [ System.Text.Json.Serialization.JsonPropertyName( "3" ) ]
    public decimal? LastPrice { get; set; }

    /// <summary>Number of shares for bid</summary>
    [ JsonProperty( "4" ) ]
    [ System.Text.Json.Serialization.JsonPropertyName( "4" ) ]
    public int? BidSize { get; set; }

    /// <summary>Number of shares for ask</summary>
    [ JsonProperty( "5" ) ]
    [ System.Text.Json.Serialization.JsonPropertyName( "5" ) ]
    public int? AskSize { get; set; }

    /// <summary>Exchange with the best ask</summary>
    [ JsonProperty( "6" ) ]
    public TdExchangeId? AskID { get; set; }

    /// <summary>Exchange with the best bid</summary>
    [ JsonProperty( "7" ) ]
    public TdExchangeId? BidID { get; set; }

    /// <summary>Aggregated shares traded throughout the day, including pre/post market hours.</summary>
    [ JsonProperty( "8" ) ]
    public decimal? TotalVolume { get; set; }

    /// <summary>Number of shares traded with last trade</summary>
    [ JsonProperty( "9" ) ]
    public int? LastSize { get; set; }

    /// <summary>Trade time of the last quote in milliseconds since epoch</summary>
    [ JsonProperty( "10" ) ]
    [ Newtonsoft.Json.JsonConverter( typeof(NodaTimeUnixMillisecondTimestampConverter) ) ]
    [System.Text.Json.Serialization.JsonConverter(typeof(InstantUnixTimeMillisecondsConverter))]
    [ System.Text.Json.Serialization.JsonPropertyName( "10" ) ]
    public Instant QuoteTime { get; set; }

    /// <summary>Trade time of the last trade in milliseconds since epoch</summary>
    [ JsonProperty( "11" ) ]
    [ Newtonsoft.Json.JsonConverter( typeof(NodaTimeUnixMillisecondTimestampConverter) ) ]
    [System.Text.Json.Serialization.JsonConverter(typeof(InstantUnixTimeMillisecondsConverter))]
    [ System.Text.Json.Serialization.JsonPropertyName( "11" ) ]
    public Instant TradeTime { get; set; }

    /// <summary>Day's high trade price</summary>
    [ JsonProperty( "12" ) ]
    public decimal? HighPrice { get; set; }

    /// <summary>Day's low trade price</summary>
    [ JsonProperty( "13" ) ]
    public decimal? LowPrice { get; set; }

    /// <summary>Previous day’s closing price</summary>
    [ JsonProperty( "14" ) ]
    public decimal? ClosePrice { get; set; }

    /// <summary>
    /// Primary "listing" Exchange
    /// 
    /// | Abbrev | Exchange 
    /// | -------|----------
    /// | `I`    | ICE      
    /// | `E`    | CME
    /// | `L`    | LIFFEUS
    /// </summary>
    [ JsonProperty( "15" ) ]
    public TdExchangeId? ExchangeID { get; set; }

    /// <summary>Description of the product</summary>
    [ JsonProperty( "16" ) ]
    public string? Description { get; set; }

    /// <summary>Exchange where last trade was executed</summary>
    [ JsonProperty( "17" ) ]
    public TdExchangeId? LastID { get; set; }

    /// <summary>Day's Open Price</summary>
    [ JsonProperty( "18" ) ]
    public decimal? OpenPrice { get; set; }

    /// <summary>
    /// **Current Last-Prev Close**
    /// ```
    /// If(close>0){
    ///     change = last – close
    /// } else { change=0 }
    /// ```
    /// </summary>
    [ JsonProperty( "19" ) ]
    [ System.Text.Json.Serialization.JsonPropertyName( "19" ) ]
    public decimal? NetChange { get; set; }

    /// <summary>Current percent change</summary>
    [ JsonProperty( "20" ) ]
    [ System.Text.Json.Serialization.JsonPropertyName( "20" ) ]
    public decimal? FuturePercentChange { get; set; }

    /// <summary>Name of exchange</summary>
    [ JsonProperty( "21" ) ]
    public string? ExchangeName { get; set; }

    /// <summary>Indicates a symbols current trading status, Normal, Halted, Closed</summary>
    [ JsonProperty( "22" ) ]
    public SecurityStatus? SecurityStatus { get; set; }

    /// <summary>The total number of futures ontracts that are not closed or delivered on a particular day</summary>
    [ JsonProperty( "23" ) ]
    public int? OpenInterest { get; set; }

    /// <summary>
    /// **Mark-to-Market**  
    /// value is calculated daily using current prices to determine profit/loss
    /// </summary>
    [ JsonProperty( "24" ) ]
    [ System.Text.Json.Serialization.JsonPropertyName( "24" ) ]
    public decimal? Mark { get; set; }

    /// <summary>Minimum price movement</summary>
    [ JsonProperty( "25" ) ]
    public decimal? Tick { get; set; }

    /// <summary>
    /// Minimum amount that the price of the market can change
    /// Tick * multiplier field from database
    /// </summary>
    [ JsonProperty( "26" ) ]
    public decimal? TickAmount { get; set; }

    /// <summary>Futures product</summary>
    [ JsonProperty( "27" ) ]
    public string? Product { get; set; }

    /// <summary>Display in fraction or decimal format.</summary>
    [ JsonProperty( "28" ) ]
    public string? FuturePriceFormat { get; set; }

    /// <summary></summary>
    [ JsonProperty( "29" ) ]
    public string? FutureTradingHours { get; set; }

    /// <summary>Flag to indicate if this future contract is tradable</summary>
    [ JsonProperty( "30" ) ]
    public bool? FutureIsTradable { get; set; }

    /// <summary>Point value</summary>
    [ JsonProperty( "31" ) ]
    public decimal? FutureMultiplier { get; set; }

    /// <summary>Indicates if this contract is active</summary>
    [ JsonProperty( "32" ) ]
    public bool? FutureIsActive { get; set; }

    /// <summary>Closing price</summary>
    [ JsonProperty( "33" ) ]
    public decimal? FutureSettlementPrice { get; set; }

    /// <summary>
    /// Symbol of the active contract
    /// Loaded from IPF File from Database
    /// </summary>
    [ JsonProperty( "34" ) ]
    public string? FutureActiveSymbol { get; set; }

    /// <summary>
    /// Expiration Date of this contract
    /// Milliseconds since epoch
    /// </summary>
    [ JsonProperty( "35" ) ]
    [ Newtonsoft.Json.JsonConverter( typeof(NodaTimeUnixMillisecondTimestampConverter) ) ]
    public Instant? FutureExpirationDate { get; set; }


    // "assetMainType":"FUTURE"

    public bool? Delayed { get; set; }

    /// <summary>Ticker symbol in upper case.  example: `/ES`</summary>
    [System.Text.Json.Serialization.JsonPropertyName("key")]
    public string Key { get; set; }
}

public record TestWssResponse {
    [System.Text.Json.Serialization.JsonPropertyName("service")]
    public Service Service = Service.LEVELONE_FUTURES;

    [System.Text.Json.Serialization.JsonPropertyName("timestamp")]
    [System.Text.Json.Serialization.JsonConverter(typeof(InstantUnixTimeMillisecondsConverter))]
    public Instant Timestamp { get; set; }

    
    [System.Text.Json.Serialization.JsonPropertyName("command")]
    public Command Command = Command.SUBS;

    [System.Text.Json.Serialization.JsonPropertyName("requestid")]
    public string? RequestId { get; set; }
}



public record TestRootContainer {
    [System.Text.Json.Serialization.JsonPropertyName("data")]
    public IList<TestWssResponse>? Data { get; set; }
}



#pragma warning restore CS8618