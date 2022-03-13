using System.Text.Json.Serialization;

namespace Benchmarks.Json;

[ JsonSourceGenerationOptions( GenerationMode = JsonSourceGenerationMode.Metadata ) ]
[ JsonSerializable( typeof(ScalarsFloatClass) ) ]
[ JsonSerializable( typeof(ScalarsFloatClassSetConstructor) ) ]
[ JsonSerializable( typeof(ScalarsFloatClassSetPartialConstructor) ) ]
[ JsonSerializable( typeof(ScalarsDecimalClass) ) ]
[ JsonSerializable( typeof(ScalarsFloatClassFields) ) ]
[ JsonSerializable( typeof(ScalarsNodaTimeClass) ) ]
[ JsonSerializable( typeof(ScalarsNodaTimeClassWithAttribute) ) ]
[ JsonSerializable( typeof(NestedObjectNodaTimeClass) ) ]
[ JsonSerializable( typeof(NestedObjectArrayNodaTimeClass) ) ]
internal partial class SimpleSourceGenerationContext : JsonSerializerContext { }