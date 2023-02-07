using System.Text.Json.Serialization;

namespace Benchmarks.Json;

// [ JsonSourceGenerationOptions( GenerationMode = JsonSourceGenerationMode.Metadata,  ) ]
[ JsonSourceGenerationOptions( GenerationMode = JsonSourceGenerationMode.Default  ) ]
[ JsonSerializable( typeof(ScalarsFloatRecord))]
[ JsonSerializable( typeof(ScalarsFloatClassSetConstructor) ) ]
[ JsonSerializable( typeof(ScalarsFloatClassSetPartialConstructor) ) ]

[ JsonSerializable( typeof(ScalarsFloatClass) ) ]
[ JsonSerializable( typeof(ScalarsFloatClassSetConstructor) ) ]
[ JsonSerializable( typeof(ScalarsFloatClassSetPartialConstructor) ) ]
[ JsonSerializable( typeof(ScalarsDecimal) ) ]
[ JsonSerializable( typeof(ScalarsDecimalClass) ) ]
[ JsonSerializable( typeof(ScalarsFloatClassFields) ) ]
[ JsonSerializable( typeof(ScalarsNodaTime) ) ]
[ JsonSerializable( typeof(ScalarsNodaTimeClass) ) ]
[ JsonSerializable( typeof(ScalarsNodaTimeClassWithAttribute) ) ]
[ JsonSerializable( typeof(NestedObjectNodaTime) ) ]
[ JsonSerializable( typeof(NestedObjectNodaTimeClass) ) ]
[ JsonSerializable( typeof(NestedObjectArrayNodaTime) ) ]
[ JsonSerializable( typeof(NestedObjectArrayNodaTimeClass) ) ]
internal partial class SimpleSourceGenerationContext : JsonSerializerContext { }