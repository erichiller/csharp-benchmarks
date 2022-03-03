using System.Collections.Generic;
using System.Text.Json.Serialization;

using Benchmarks.Json;

namespace Benchmarks.Json; 


[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(List<Response>))]
[JsonSerializable(typeof(DataContainer<Response>))]
[JsonSerializable(typeof(TestRootContainer))]
internal partial class LevelOneSourceGenerationContext : JsonSerializerContext
{
}