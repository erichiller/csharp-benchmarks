using System.Text.Json;

using NodaTime.Serialization.SystemTextJson;

namespace Benchmarks.Json; 

public static class SystemTextJsonCommon {
    
    // public static readonly JsonSerializerOptions SystemTextJsonOptions = new JsonSerializerOptions() {
    //         // Converters = { new InstantUnixTimeMillisecondsConverter(  ) }
    //     }
    //     .ConfigureForNodaTime( NodaTime.DateTimeZoneProviders.Tzdb );
    //
    //


    public static readonly JsonSerializerOptions SystemTextJsonOptions = new JsonSerializerOptions() {
        Converters = {
            new InstantUnixTimeMillisecondsConverter()
        }
    };
}