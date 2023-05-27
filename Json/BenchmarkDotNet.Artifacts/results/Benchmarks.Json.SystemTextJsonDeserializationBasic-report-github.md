``` ini

BenchmarkDotNet=v0.13.1, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.100
  [Host]     : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT
  DefaultJob : .NET 7.0.0 (7.0.22.51805), X64 RyuJIT


```
|                                               Method | Iterations |  Mean [μs] | Error [μs] | StdDev [μs] |    Gen 0 |    Gen 1 |   Gen 2 | Allocated [B] |
|----------------------------------------------------- |----------- |-----------:|-----------:|------------:|---------:|---------:|--------:|--------------:|
| Scalars_Float_Class_Set_NoConstructor_ReturningCount |       1000 |   561.1 μs |    8.52 μs |     7.97 μs |  16.6016 |        - |       - |      80,001 B |
|                Scalars_Float_Class_Set_NoConstructor |       1000 |   568.2 μs |    1.08 μs |     1.01 μs |  13.6719 |  12.6953 |       - |      88,025 B |
|      Scalars_Float_Class_Set_NoConstructor_SourceGen |       1000 |   569.0 μs |    1.43 μs |     1.33 μs |  13.6719 |  12.6953 |       - |      88,025 B |
|                      Scalars_Decimal_Class_SourceGen |       1000 |   579.4 μs |    1.37 μs |     1.28 μs |  16.6016 |  15.6250 |       - |     104,025 B |
|   Scalars_Float_Class_Fields_NoConstructor_SourceGen |       1000 |   580.6 μs |    1.43 μs |     1.26 μs |  13.6719 |  12.6953 |       - |      88,025 B |
|                                Scalars_Decimal_Class |       1000 |   582.2 μs |    1.38 μs |     1.29 μs |  16.6016 |  15.6250 |       - |     104,025 B |
|  Scalars_NodaTime_ConverterAttribute_Class_SourceGen |       1000 |   766.4 μs |    3.36 μs |     2.98 μs |  31.2500 |  30.2734 |       - |     200,025 B |
|                     Scalars_NodaTime_Class_SourceGen |       1000 |   769.9 μs |    2.66 μs |     2.49 μs |  31.2500 |  30.2734 |       - |     200,025 B |
|            Scalars_NodaTime_Class_ConverterAttribute |       1000 |   773.1 μs |   12.39 μs |    11.59 μs |  31.2500 |  30.2734 |       - |     200,025 B |
|                               Scalars_NodaTime_Class |       1000 |   792.8 μs |    6.87 μs |     6.43 μs |  31.2500 |  30.2734 |       - |     200,025 B |
|        Scalars_Float_Class_Set_Constructor_SourceGen |       1000 |   935.9 μs |    2.84 μs |     2.66 μs |  45.8984 |  44.9219 |       - |     288,025 B |
|                               Scalars_Decimal_Record |       1000 | 1,019.2 μs |    5.07 μs |     4.74 μs |  48.8281 |  46.8750 |       - |     312,026 B |
| Scalars_Float_Class_Set_PartialConstructor_SourceGen |       1000 | 1,178.8 μs |    4.23 μs |     3.96 μs |  41.0156 |  39.0625 |       - |     264,027 B |
|                              Scalars_NodaTime_Record |       1000 | 1,183.6 μs |    5.95 μs |     5.27 μs |  64.4531 |  62.5000 | 21.4844 |     408,048 B |
|                         NestedObjects_NodaTime_Class |       1000 | 1,690.8 μs |    8.94 μs |     8.37 μs | 132.8125 |  60.5469 |       - |     728,026 B |
|               NestedObjects_NodaTime_Class_SourceGen |       1000 | 1,710.4 μs |   17.23 μs |    16.12 μs | 115.2344 |  39.0625 | 37.1094 |     728,051 B |
|                        NestedObjects_NodaTime_Record |       1000 | 2,258.5 μs |   18.57 μs |    17.37 μs | 175.7813 |  42.9688 | 19.5313 |   1,024,090 B |
|                  NestedObjects_Arrays_NodaTime_Class |       1000 | 2,396.7 μs |    8.87 μs |     7.86 μs | 164.0625 | 160.1563 |       - |   1,048,027 B |
|                 NestedObjects_Arrays_NodaTime_Record |       1000 | 3,102.0 μs |   11.61 μs |    10.86 μs | 230.4688 | 226.5625 |       - |   1,464,028 B |
|        NestedObjects_Arrays_NodaTime_Class_SourceGen |       1000 | 3,105.7 μs |   27.83 μs |    26.03 μs | 183.5938 |  62.5000 | 23.4375 |   1,048,086 B |
