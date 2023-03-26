
### JSON

#### Serialize

| Method                                                 | Iterations |       Mean |    Error |   StdDev |    Gen 0 |    Gen 1 |   Gen 2 | Allocated |
|--------------------------------------------------------|------------|-----------:|---------:|---------:|---------:|---------:|--------:|----------:|
| NewtonsoftJson_Serialize_Scalars_Float                 | 1000       | 2,465.6 us | 11.18 us | 10.46 us | 375.0000 | 125.0000 |       - |  1,828 KB |
| NewtonsoftJson_Serialize_Scalars_Decimal               | 1000       | 2,518.7 us | 48.87 us | 63.54 us | 375.0000 | 125.0000 |       - |  1,852 KB |
| NewtonsoftJson_Serialize_Scalars_NodaTime              | 1000       | 2,498.1 us |  9.23 us | 14.10 us | 464.8438 | 171.8750 |       - |  2,406 KB |
| NewtonsoftJson_Serialize_NestedObjects_NodaTime        | 1000       | 3,960.3 us | 23.17 us | 21.68 us | 492.1875 | 242.1875 |       - |  2,695 KB |
| NewtonsoftJson_Serialize_NestedObjects_Arrays_NodaTime | 1000       | 6,448.7 us | 50.39 us | 47.13 us | 539.0625 | 218.7500 | 23.4375 |  3,304 KB |
| SystemTextJson_Serialize_Scalars_Float                 | 1000       |   454.8 us |  4.28 us |  4.00 us |  62.0117 |  20.5078 |       - |    297 KB |
| SystemTextJson_Serialize_Scalars_Decimal               | 1000       |   348.0 us |  1.12 us |  1.04 us |  61.5234 |  20.5078 |       - |    313 KB |
| SystemTextJson_Serialize_Scalars_NodaTime              | 1000       |   608.9 us |  6.82 us |  6.38 us | 137.6953 |  21.4844 |       - |    656 KB |
| SystemTextJson_Serialize_NestedObjects_NodaTime        | 1000       |   920.5 us |  3.91 us |  3.46 us | 187.5000 |  93.7500 |       - |  1,109 KB |
| SystemTextJson_Serialize_NestedObjects_Arrays_NodaTime | 1000       | 1,590.2 us | 18.29 us | 16.22 us | 265.6250 | 132.8125 |       - |  1,632 KB |



#### Deserialize

##### `System.Text.Json` Deserialization Tests

Note that `record` types **are much slower than `class` types.
Also, `[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]` only `JsonSerializerContext` **do not improve performance at all**.

| Method                                              | Iterations |  Mean [us] | Error [us] | StdDev [us] |    Gen 0 |    Gen 1 | Allocated [B] |
|-----------------------------------------------------|------------|-----------:|-----------:|------------:|---------:|---------:|--------------:|
| Scalars_Decimal_Class                               | 1000       |   549.6 us |    2.59 us |     2.16 us |  21.4844 |   6.8359 |     104,025 B |
| Scalars_Float_Class                                 | 1000       |   556.3 us |    3.22 us |     2.86 us |  18.5547 |   5.8594 |      88,025 B |
| Scalars_Float_Class_SourceGen                       | 1000       |   556.3 us |    5.08 us |     4.75 us |  18.5547 |   5.8594 |      88,025 B |
| Scalars_Float_Class_Fields_SourceGen                | 1000       |   565.9 us |    2.49 us |     2.08 us |  18.5547 |   5.8594 |      88,025 B |
| Scalars_Decimal_Class_SourceGen                     | 1000       |   568.8 us |    3.26 us |     3.05 us |  21.4844 |   6.8359 |     104,025 B |
| Scalars_Decimal_Record                              | 1000       |   688.6 us |    5.67 us |     5.31 us |  66.4063 |  21.4844 |     320,025 B |
| Scalars_Float_Record                                | 1000       |   691.8 us |    4.21 us |     3.73 us |  60.5469 |  19.5313 |     288,025 B |
| Scalars_NodaTime_Class                              | 1000       |   696.9 us |    1.64 us |     1.54 us |  41.9922 |  13.6719 |     200,025 B |
| Scalars_NodaTime_Class_SourceGen                    | 1000       |   713.6 us |    9.58 us |     8.96 us |  41.9922 |  13.6719 |     200,025 B |
| Scalars_NodaTime_Class_ConverterAttribute           | 1000       |   714.8 us |    6.14 us |     5.45 us |  41.9922 |  13.6719 |     200,025 B |
| Scalars_NodaTime_ConverterAttribute_Class_SourceGen | 1000       |   728.4 us |    7.44 us |     6.96 us |  41.9922 |  13.6719 |     200,025 B |
| Scalars_NodaTime_Record                             | 1000       |   856.9 us |   10.38 us |     9.20 us |  87.8906 |  29.2969 |     416,025 B |
| NestedObjects_NodaTime_Class                        | 1000       | 1,277.3 us |    2.15 us |     2.01 us | 117.1875 |  52.7344 |     696,026 B |
| NestedObjects_NodaTime_Class_SourceGen              | 1000       | 1,305.0 us |   16.88 us |    15.79 us | 117.1875 |  48.8281 |     696,026 B |
| NestedObjects_NodaTime_Record                       | 1000       | 1,568.7 us |   18.70 us |    16.57 us | 181.6406 |  80.0781 |   1,032,026 B |
| NestedObjects_Arrays_NodaTime_Class                 | 1000       | 2,317.0 us |   21.91 us |    17.10 us | 160.1563 |  78.1250 |   1,016,027 B |
| NestedObjects_Arrays_NodaTime_Class_SourceGen       | 1000       | 2,327.5 us |   25.29 us |    22.42 us | 160.1563 |  78.1250 |   1,016,027 B |
| NestedObjects_Arrays_NodaTime_Record                | 1000       | 2,939.7 us |   58.74 us |    69.92 us | 234.3750 | 117.1875 |   1,480,027 B |


This table shows that it is not just `init`  properties, or `record` types that slow deserialization down, it is the use of *Constructors* that causes **a significant increase in memory utilization**,
and takes about 25% longer. Furthermore, Source Generators do not help.

| Method                                               | Iterations |  Mean [us] | Error [us] | StdDev [us] |   Gen 0 |   Gen 1 | Allocated [B] |
|------------------------------------------------------|------------|-----------:|-----------:|------------:|--------:|--------:|--------------:|
| Scalars_Float_Record_Init_No_Constructor             | 1000       |   555.0 us |    3.55 us |     3.15 us | 18.5547 |  5.8594 |      88,025 B |
| Scalars_Float_Class_Set_NoConstructor                | 1000       |   558.3 us |    1.97 us |     1.84 us | 18.5547 |  5.8594 |      88,025 B |
| Scalars_Float_Class_Init_NoConstructor               | 1000       |   561.5 us |    1.93 us |     1.71 us | 18.5547 |  5.8594 |      88,025 B |
| Scalars_Float_Class_Set_NoConstructor_SourceGen      | 1000       |   561.6 us |    3.38 us |     2.99 us | 18.5547 |  5.8594 |      88,025 B |
| Scalars_Float_Class_Set_NoConstructor_ReturningCount | 1000       |   563.8 us |    2.52 us |     2.35 us | 16.6016 |       - |      80,001 B |
| Scalars_Float_Class_Fields_NoConstructor_SourceGen   | 1000       |   574.1 us |    1.24 us |     1.10 us | 18.5547 |  5.8594 |      88,025 B |
| Scalars_Float_Class_Init_Constructor                 | 1000       |   708.4 us |    3.35 us |     3.13 us | 60.5469 | 19.5313 |     288,025 B |
| Scalars_Float_Record_Init_Constructor                | 1000       |   711.4 us |    4.61 us |     4.32 us | 60.5469 | 19.5313 |     288,025 B |
| Scalars_Float_Class_Set_Constructor_SourceGen        | 1000       |   747.5 us |    1.10 us |     0.92 us | 62.5000 | 20.5078 |     296,025 B |
| Scalars_Float_Class_Init_PartialConstructor          | 1000       |   969.8 us |    4.03 us |     3.36 us | 62.5000 | 19.5313 |     296,026 B |
| Scalars_Float_Class_Set_PartialConstructor_SourceGen | 1000       | 1,010.1 us |    4.40 us |     3.67 us | 56.6406 | 17.5781 |     272,027 B |


##### LevelOne


| Method                                                                               | Iterations | LevelOneJsonFile | WithSourceGenerationContext |      Mean |     Error |    StdDev |    Median |     Gen 0 |    Gen 1 | Allocated |
|--------------------------------------------------------------------------------------|------------|------------------|-----------------------------|----------:|----------:|----------:|----------:|----------:|---------:|----------:|
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                         | 1000       | Single           | True                        |  7.090 ms | 0.1403 ms | 0.2635 ms |  6.960 ms |  632.8125 |  15.6250 |      3 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                         | 1000       | Single           | False                       |  7.209 ms | 0.0503 ms | 0.0446 ms |  7.185 ms |  632.8125 |  15.6250 |      3 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne_SourceGenWithoutOptions | 1000       | Single           | True                        |  7.278 ms | 0.0257 ms | 0.0240 ms |  7.268 ms |  632.8125 |  15.6250 |      3 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne_SourceGenWithoutOptions | 1000       | Single           | False                       |  7.407 ms | 0.0401 ms | 0.0375 ms |  7.414 ms |  632.8125 |  15.6250 |      3 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne_SourceGenWithoutOptions | 1000       | Multiple         | True                        | 49.775 ms | 0.1507 ms | 0.1336 ms | 49.722 ms | 4100.0000 | 200.0000 |     18 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                         | 1000       | Multiple         | False                       | 50.411 ms | 0.6371 ms | 0.5960 ms | 50.180 ms | 4090.9091 | 181.8182 |     18 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                         | 1000       | Multiple         | True                        | 50.712 ms | 0.2609 ms | 0.2441 ms | 50.655 ms | 4100.0000 | 200.0000 |     18 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne_SourceGenWithoutOptions | 1000       | Multiple         | False                       | 51.048 ms | 0.6626 ms | 0.5874 ms | 50.848 ms | 4100.0000 | 200.0000 |     18 MB |



#### Deserialize with Read-ahead for Type determination

##### Multiple
| Method                                                                            | Iterations |      Mean |    Error |   StdDev |      Gen 0 |     Gen 1 | Allocated |
|-----------------------------------------------------------------------------------|------------|----------:|---------:|---------:|-----------:|----------:|----------:|
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       |  29.72 ms | 0.196 ms | 0.174 ms |  4093.7500 |  187.5000 |     18 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       |  43.35 ms | 0.325 ms | 0.304 ms |  3250.0000 |  166.6667 |     15 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       |  49.88 ms | 0.302 ms | 0.282 ms |  3818.1818 |  181.8182 |     17 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       |  56.94 ms | 1.036 ms | 0.969 ms | 14444.4444 |  666.6667 |     65 MB |
| NewtonsoftJson_Deserialize_ReadAhead_LevelOne                                     | 1000       | 171.16 ms | 1.438 ms | 1.345 ms | 24000.0000 | 2000.0000 |    112 MB |

##### Single
| Method                                                                            | Iterations | LevelOneJsonFile |      Mean |     Error |    StdDev |     Gen 0 |    Gen 1 | Allocated |
|-----------------------------------------------------------------------------------|------------|------------------|----------:|----------:|----------:|----------:|---------:|----------:|
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       | Single           |  4.815 ms | 0.0790 ms | 0.0739 ms | 1335.9375 |  23.4375 |      6 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       | Single           |  6.324 ms | 0.0436 ms | 0.0341 ms |  484.3750 |  15.6250 |      2 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       | Single           |  7.143 ms | 0.0531 ms | 0.0443 ms | 2171.8750 |  85.9375 |     10 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       | Single           |  7.403 ms | 0.0151 ms | 0.0126 ms |  601.5625 |  15.6250 |      3 MB |
| NewtonsoftJson_Deserialize_ReadAhead_LevelOne                                     | 1000       | Single           | 26.456 ms | 0.0490 ms | 0.0459 ms | 4750.0000 | 156.2500 |     21 MB |




| Method                                                                            | Iterations | LevelOneJsonFile |      Mean |     Error |    StdDev |     Gen 0 |    Gen 1 | Allocated |
|-----------------------------------------------------------------------------------|------------|------------------|----------:|----------:|----------:|----------:|---------:|----------:|
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       | Single           |  5.517 ms | 0.1073 ms | 0.1395 ms | 1375.0000 |  23.4375 |      6 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       | Single           |  6.879 ms | 0.1345 ms | 0.1193 ms |  523.4375 |  15.6250 |      2 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       | Single           |  7.678 ms | 0.0674 ms | 0.0630 ms |  648.4375 |  15.6250 |      3 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       | Single           |  7.817 ms | 0.1426 ms | 0.1334 ms | 2203.1250 |  85.9375 |     10 MB |
| NewtonsoftJson_Deserialize_ReadAhead_LevelOne                                     | 1000       | Single           | 28.415 ms | 0.1826 ms | 0.1618 ms | 4750.0000 | 187.5000 |     21 MB |




| Method                                                                            | Iterations | LevelOneJsonFile | WithSourceGenerationContext |       Mean |     Error |    StdDev |      Gen 0 |     Gen 1 | Allocated |
|-----------------------------------------------------------------------------------|------------|------------------|-----------------------------|-----------:|----------:|----------:|-----------:|----------:|----------:|
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       | Single           | False                       |   5.173 ms | 0.0281 ms | 0.0235 ms |  1375.0000 |   23.4375 |      6 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       | Single           | True                        |   5.253 ms | 0.0958 ms | 0.0896 ms |  1375.0000 |   23.4375 |      6 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       | Single           | True                        |   6.598 ms | 0.0257 ms | 0.0240 ms |   523.4375 |   15.6250 |      2 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       | Single           | False                       |   6.630 ms | 0.0462 ms | 0.0432 ms |   523.4375 |   15.6250 |      2 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       | Single           | False                       |   7.774 ms | 0.1102 ms | 0.1031 ms |  2203.1250 |   85.9375 |     10 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       | Single           | True                        |   7.787 ms | 0.0547 ms | 0.0485 ms |   648.4375 |   15.6250 |      3 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       | Single           | False                       |   7.824 ms | 0.0607 ms | 0.0507 ms |   640.6250 |   15.6250 |      3 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       | Single           | True                        |   7.972 ms | 0.1544 ms | 0.1586 ms |  2203.1250 |   85.9375 |     10 MB |
| NewtonsoftJson_Deserialize_ReadAhead_LevelOne                                     | 1000       | Single           | False                       |  26.348 ms | 0.1745 ms | 0.1632 ms |  4750.0000 |  187.5000 |     21 MB |
| NewtonsoftJson_Deserialize_ReadAhead_LevelOne                                     | 1000       | Single           | True                        |  28.016 ms | 0.2733 ms | 0.2556 ms |  4750.0000 |  187.5000 |     21 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       | Multiple         | True                        |  33.436 ms | 0.1659 ms | 0.1552 ms |  4466.6667 |  200.0000 |     20 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       | Multiple         | False                       |  33.844 ms | 0.2259 ms | 0.2113 ms |  4466.6667 |  200.0000 |     20 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       | Multiple         | True                        |  48.750 ms | 0.2033 ms | 0.1697 ms |  3636.3636 |  272.7273 |     16 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       | Multiple         | False                       |  48.779 ms | 0.1454 ms | 0.1289 ms |  3636.3636 |  181.8182 |     16 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       | Multiple         | False                       |  55.094 ms | 0.2562 ms | 0.2397 ms |  4300.0000 |  200.0000 |     19 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       | Multiple         | True                        |  56.605 ms | 0.1656 ms | 0.1468 ms |  4222.2222 |  111.1111 |     19 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       | Multiple         | False                       |  62.105 ms | 0.3797 ms | 0.3552 ms | 14875.0000 |  750.0000 |     67 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       | Multiple         | True                        |  62.628 ms | 0.8557 ms | 0.8004 ms | 14888.8889 |  555.5556 |     67 MB |
| NewtonsoftJson_Deserialize_ReadAhead_LevelOne                                     | 1000       | Multiple         | False                       | 174.842 ms | 1.2252 ms | 1.1461 ms | 24000.0000 | 2000.0000 |    112 MB |

***



**TESTING ONLY**

BenchmarkDotNet=v0.13.1, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.100
[Host]     : .NET 6.0.11 (6.0.1122.52304), X64 RyuJIT
DefaultJob : .NET 6.0.11 (6.0.1122.52304), X64 RyuJIT


| Method                                               | Iterations |  Mean [us] | Error [us] | StdDev [us] |    Gen 0 |    Gen 1 | Allocated [B] |
|------------------------------------------------------|------------|-----------:|-----------:|------------:|---------:|---------:|--------------:|
| Scalars_Float_Record_Init_NoConstructor              | 1000       |   552.7 us |    1.86 us |     1.45 us |  18.5547 |   5.8594 |      88,025 B |
| Scalars_Float_Class_Init_NoConstructor               | 1000       |   553.4 us |    0.85 us |     0.80 us |  18.5547 |   5.8594 |      88,025 B |
| Scalars_Float_Class_Set_NoConstructor                | 1000       |   559.8 us |    1.24 us |     1.16 us |  18.5547 |   5.8594 |      88,025 B |
| Scalars_Float_Class_Set_NoConstructor_ReturningCount | 1000       |   561.8 us |    1.50 us |     1.33 us |  16.6016 |        - |      80,001 B |
| Scalars_Decimal_Class                                | 1000       |   562.2 us |    1.36 us |     1.14 us |  21.4844 |   6.8359 |     104,025 B |
| Scalars_Decimal_Class_SourceGen                      | 1000       |   567.2 us |    1.79 us |     1.58 us |  21.4844 |   6.8359 |     104,025 B |
| Scalars_Float_Class_Fields_NoConstructor_SourceGen   | 1000       |   574.5 us |    8.25 us |     7.72 us |  18.5547 |   5.8594 |      88,025 B |
| Scalars_Float_Class_Set_NoConstructor_SourceGen      | 1000       |   576.7 us |    9.07 us |     8.49 us |  18.5547 |   5.8594 |      88,025 B |
| Scalars_Decimal_Record                               | 1000       |   710.2 us |    2.73 us |     2.55 us |  66.4063 |  21.4844 |     320,025 B |
| Scalars_Float_Record_Init_Constructor                | 1000       |   712.9 us |    4.13 us |     3.45 us |  60.5469 |  19.5313 |     288,025 B |
| Scalars_Float_Class_Init_Constructor                 | 1000       |   713.8 us |    2.43 us |     2.15 us |  60.5469 |  19.5313 |     288,025 B |
| Scalars_Decimal_Class_Init_Constructor               | 1000       |   719.1 us |    8.07 us |     7.55 us |  66.4063 |  21.4844 |     320,025 B |
| Scalars_NodaTime_Class_SourceGen                     | 1000       |   731.0 us |    2.16 us |     1.92 us |  41.9922 |  13.6719 |     200,025 B |
| Scalars_NodaTime_Class_ConverterAttribute            | 1000       |   734.8 us |    6.18 us |     5.78 us |  41.9922 |  13.6719 |     200,025 B |
| Scalars_NodaTime_Class                               | 1000       |   735.0 us |    3.08 us |     2.40 us |  41.9922 |  13.6719 |     200,025 B |
| Scalars_NodaTime_ConverterAttribute_Class_SourceGen  | 1000       |   746.1 us |    3.46 us |     3.07 us |  41.9922 |  13.6719 |     200,025 B |
| Scalars_Float_Class_Set_Constructor_SourceGen        | 1000       |   797.0 us |    4.30 us |     4.02 us |  62.5000 |  20.5078 |     296,025 B |
| Scalars_NodaTime_Record                              | 1000       |   878.4 us |    3.01 us |     2.81 us |  87.8906 |  29.2969 |     416,025 B |
| Scalars_NodaTime_Class_Init_Constructor              | 1000       |   885.6 us |    1.41 us |     1.10 us |  87.8906 |  29.2969 |     416,025 B |
| Scalars_Float_Class_Init_PartialConstructor          | 1000       |   972.7 us |    2.60 us |     2.30 us |  62.5000 |  19.5313 |     296,026 B |
| Scalars_Float_Class_Set_PartialConstructor_SourceGen | 1000       | 1,031.5 us |   13.58 us |    12.70 us |  56.6406 |  17.5781 |     272,027 B |
| NestedObjects_NodaTime_Class_SourceGen               | 1000       | 1,343.0 us |   10.32 us |     9.65 us | 115.2344 |  52.7344 |     696,026 B |
| NestedObjects_NodaTime_Class                         | 1000       | 1,363.8 us |   25.81 us |    25.35 us | 117.1875 |  52.7344 |     696,026 B |
| NestedObjects_NodaTime_Class_Init_Constructor        | 1000       | 1,583.8 us |    4.01 us |     3.75 us | 167.9688 |  72.2656 |     904,026 B |
| NestedObjects_NodaTime_Record                        | 1000       | 1,596.7 us |    4.31 us |     4.03 us | 183.5938 |  80.0781 |   1,032,026 B |
| NestedObjects_Arrays_NodaTime_Class                  | 1000       | 2,381.1 us |   14.25 us |    11.90 us | 160.1563 |  78.1250 |   1,016,027 B |
| NestedObjects_Arrays_NodaTime_Class_SourceGen        | 1000       | 2,508.4 us |   15.05 us |    13.34 us | 160.1563 |  78.1250 |   1,016,027 B |
| NestedObjects_Arrays_NodaTime_Class_Init_Constructor | 1000       | 2,670.7 us |   10.35 us |     9.69 us | 195.3125 |  97.6563 |   1,224,027 B |
| NestedObjects_Arrays_NodaTime_Record                 | 1000       | 2,888.0 us |   19.29 us |    18.04 us | 234.3750 | 117.1875 |   1,480,030 B |


| Method                                               | Iterations |  Mean [us] | Error [us] | StdDev [us] |    Gen 0 |    Gen 1 |   Gen 2 | Allocated [B] |
|------------------------------------------------------|------------|-----------:|-----------:|------------:|---------:|---------:|--------:|--------------:|
| Scalars_Float_Class_Fields_NoConstructor_SourceGen   | 1000       |   556.2 us |    0.68 us |     0.63 us |  13.6719 |  12.6953 |       - |      88,025 B |
| Scalars_Float_Class_Set_NoConstructor_ReturningCount | 1000       |   559.0 us |    1.53 us |     1.35 us |  16.6016 |        - |       - |      80,001 B |
| Scalars_Decimal_Class_SourceGen                      | 1000       |   565.3 us |    1.01 us |     0.95 us |  15.6250 |  14.6484 |       - |     104,025 B |
| Scalars_Decimal_Class                                | 1000       |   566.0 us |    2.06 us |     1.93 us |  16.6016 |  15.6250 |       - |     104,025 B |
| Scalars_Float_Class_Set_NoConstructor_SourceGen      | 1000       |   566.9 us |    2.35 us |     2.20 us |  13.6719 |  12.6953 |       - |      88,025 B |
| Scalars_Float_Class_Set_NoConstructor                | 1000       |   574.0 us |    1.74 us |     1.45 us |  13.6719 |  12.6953 |       - |      88,025 B |
| Scalars_NodaTime_Class_ConverterAttribute            | 1000       |   717.0 us |    1.45 us |     1.36 us |  41.9922 |  13.6719 |       - |     200,025 B |
| Scalars_NodaTime_ConverterAttribute_Class_SourceGen  | 1000       |   762.1 us |    2.92 us |     2.73 us |  31.2500 |  30.2734 |       - |     200,025 B |
| Scalars_NodaTime_Class                               | 1000       |   770.3 us |    2.60 us |     2.30 us |  31.2500 |  30.2734 |       - |     200,025 B |
| Scalars_NodaTime_Class_SourceGen                     | 1000       |   770.7 us |    1.45 us |     1.28 us |  31.2500 |  30.2734 |       - |     200,025 B |
| Scalars_Float_Class_Set_Constructor_SourceGen        | 1000       |   916.3 us |    3.25 us |     3.04 us |  45.8984 |  44.9219 |       - |     288,025 B |
| Scalars_Decimal_Record                               | 1000       |   986.9 us |   13.74 us |    12.85 us |  48.8281 |  46.8750 |       - |     312,026 B |
| Scalars_Float_Class_Set_PartialConstructor_SourceGen | 1000       | 1,179.2 us |    3.64 us |     3.04 us |  41.0156 |  39.0625 |       - |     264,027 B |
| Scalars_NodaTime_Record                              | 1000       | 1,208.4 us |   16.60 us |    15.53 us |  64.4531 |  62.5000 | 21.4844 |     408,048 B |
| NestedObjects_NodaTime_Class_SourceGen               | 1000       | 1,671.0 us |   11.11 us |    10.39 us | 115.2344 |  39.0625 | 37.1094 |     728,051 B |
| NestedObjects_NodaTime_Class                         | 1000       | 1,691.9 us |    9.95 us |     9.31 us | 134.7656 |  58.5938 |       - |     728,026 B |
| NestedObjects_NodaTime_Record                        | 1000       | 1,801.8 us |   23.46 us |    21.94 us | 179.6875 | 119.1406 |       - |   1,024,026 B |
| NestedObjects_Arrays_NodaTime_Class                  | 1000       | 2,416.3 us |    8.33 us |     6.95 us | 164.0625 | 160.1563 |       - |   1,048,027 B |
| NestedObjects_Arrays_NodaTime_Class_SourceGen        | 1000       | 3,139.8 us |   34.08 us |    31.87 us | 183.5938 |  62.5000 | 23.4375 |   1,048,082 B |
| NestedObjects_Arrays_NodaTime_Record                 | 1000       | 3,249.5 us |   38.12 us |    35.65 us | 230.4688 | 226.5625 |       - |   1,464,028 B |