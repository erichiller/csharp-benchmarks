``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.202
  [Host]     : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2


```
|                                            Method | Mean [μs] | Error [μs] | StdDev [μs] | Ratio | RatioSD |   Gen0 | Allocated [B] | Alloc Ratio |
|-------------------------------------------------- |----------:|-----------:|------------:|------:|--------:|-------:|--------------:|------------:|
|                                          WorkOnly |  60.35 μs |   0.029 μs |    0.025 μs |  1.00 |    0.00 |      - |             - |          NA |
|       IsNull_NullableHashSet_TupleKey_CapacitySet |  74.49 μs |   0.067 μs |    0.056 μs |  1.23 |    0.00 | 0.1221 |         576 B |          NA |
| IsNull_NullableDictionary_TupleKey_CapacityNotSet |  75.04 μs |   0.299 μs |    0.279 μs |  1.24 |    0.00 | 0.3662 |        2080 B |          NA |
|                          Bool_Dictionary_TupleKey |  75.17 μs |   0.291 μs |    0.272 μs |  1.25 |    0.00 | 0.1221 |         776 B |          NA |
|                 Bool_HashSet_TupleKey_CapacitySet |  76.07 μs |   0.926 μs |    0.866 μs |  1.26 |    0.01 | 0.1221 |         576 B |          NA |
|                  IsNull_NullableDictionary_IntKey | 122.78 μs |   0.046 μs |    0.038 μs |  2.03 |    0.00 |      - |         592 B |          NA |
|         IsNull_NullableHashSet_IntKey_CapacitySet | 123.03 μs |   0.349 μs |    0.326 μs |  2.04 |    0.01 |      - |         488 B |          NA |
|    IsNull_NullableDictionary_TupleKey_CapacitySet | 124.01 μs |   0.448 μs |    0.419 μs |  2.06 |    0.01 |      - |         776 B |          NA |
| IsNull_NullableDictionary_TupleKey_Capacity100Set | 124.07 μs |   0.366 μs |    0.306 μs |  2.06 |    0.00 | 0.4883 |        3128 B |          NA |
|    IsNull_NullableHashSet_TupleKey_CapacityNotSet | 124.18 μs |   0.498 μs |    0.442 μs |  2.06 |    0.01 | 0.2441 |        1552 B |          NA |
|                  Bool_NullableDictionary_TupleKey | 124.54 μs |   0.692 μs |    0.614 μs |  2.06 |    0.01 |      - |         776 B |          NA |
|                Bool_Dictionary_IntKey_CapacitySet | 196.54 μs |   0.251 μs |    0.223 μs |  3.26 |    0.00 |      - |         592 B |          NA |
|             Bool_Dictionary_IntKey_CapacityNotSet | 198.92 μs |   1.103 μs |    1.031 μs |  3.30 |    0.02 | 0.2441 |        1568 B |          NA |
