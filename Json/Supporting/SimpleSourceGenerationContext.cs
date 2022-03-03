using System.Text.Json.Serialization;

namespace Benchmarks.Json;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(ScalarsDecimalClass))]
[JsonSerializable(typeof(ScalarsFloatClass))]
[JsonSerializable(typeof(ScalarsFloatClassFields))]
[JsonSerializable(typeof(ScalarsNodaTimeClass))]
[JsonSerializable(typeof(ScalarsNodaTimeClassWithAttribute))]
[JsonSerializable(typeof(NestedObjectNodaTimeClass))]
[JsonSerializable(typeof(NestedObjectArrayNodaTimeClass))]
internal partial class SimpleSourceGenerationContext : JsonSerializerContext
{
}