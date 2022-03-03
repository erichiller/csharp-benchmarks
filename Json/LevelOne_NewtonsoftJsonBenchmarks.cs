using System;
using System.IO;
using System.Text;

using BenchmarkDotNet.Attributes;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Benchmarks.Json;

public partial class LevelOneJsonBenchmarks {
    
    
    [ Benchmark ]
    public int NewtonsoftJson_Deserialize_ReadAhead_LevelOne( ) {
        string jsonString = System.IO.File.ReadAllText( $"./LevelOneFuturesResponse_{LevelOneJsonFile}.json" );

        int count = 0;
        for ( int i = 0 ; i < Iterations ; i++ ) {
            byte[]    bytes  = Encoding.UTF8.GetBytes( jsonString );
            using var stream = new MemoryStream( bytes );
            using var reader = new StreamReader( stream, Encoding.UTF8 );

            string receivedString = reader.ReadToEnd();

            JObject receivedObject = JObject.Parse( receivedString );

            // byte[]    bytes  = Encoding.UTF8.GetBytes( jsonString );
            // using var stream = new MemoryStream( bytes );
            if ( receivedObject.ContainsKey( "data" ) && receivedObject["data"] is JArray receivedData && (string?)(receivedData.First?[ "service" ]) == nameof( Service.LEVELONE_FUTURES ) ){
                foreach ( var result in ( JsonConvert.DeserializeObject<DataContainer<Response>>( jsonString, NewtonsoftJsonBenchmarks.JsonNetSettings ) ?? throw new NullReferenceException() ).Data ) {
                    count++;
                }
            } else {
                throw new Exception();
            }
        }

        return count;
    }
    
}