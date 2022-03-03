using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace Benchmarks.Json;

public class LowerCaseNamingPolicy : JsonNamingPolicy {
    [ SuppressMessage( "Globalization", "CA1308:Normalize strings to uppercase" ) ]
    public override string ConvertName( string name ) =>
        ( name ?? throw new ArgumentNullException( nameof(name) ) ).ToLowerInvariant();
}