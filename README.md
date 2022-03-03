# Use

Run:
```powershell
sudo dotnet run -c RELEASE
```

## Commandline Arguments

```
--anyCategories Copy
```

## Results

### SQL Inserts
| Method                                               | ObjectsPerSave | SaveIterations |         Mean |       Error |      StdDev |       Median |       Gen 0 |       Gen 1 |      Gen 2 |    Allocated |
|------------------------------------------------------|----------------|----------------|-------------:|------------:|------------:|-------------:|------------:|------------:|-----------:|-------------:|
| EfCoreInsert                                         | 1              | 100            |     848.3 ms |    29.37 ms |    85.69 ms |     874.5 ms |   1000.0000 |           - |          - |     5,213 KB |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking     | 1              | 100            |     809.4 ms |    27.33 ms |    78.85 ms |     835.8 ms |           - |           - |          - |     1,206 KB |
| NpgSqlInsert_SingularCommand_TypedValue              | 1              | 100            |     453.9 ms |    38.65 ms |   113.97 ms |     441.8 ms |           - |           - |          - |       174 KB |
| NpgsqlInsert_Batched_Boxed_Value                     | 1              | 100            |     449.6 ms |    47.24 ms |   132.46 ms |     430.9 ms |           - |           - |          - |       170 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value        | 1              | 100            |     434.4 ms |    42.47 ms |   125.22 ms |     407.0 ms |           - |           - |          - |       169 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue  | 1              | 100            |     441.0 ms |    44.32 ms |   128.57 ms |     402.4 ms |           - |           - |          - |       170 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue2 | 1              | 100            |     431.7 ms |    15.87 ms |    46.55 ms |     446.3 ms |           - |           - |          - |       169 KB |
| NpgsqlInsert_Batched_TypedValue                      | 1              | 100            |     434.8 ms |    41.55 ms |   122.51 ms |     418.8 ms |           - |           - |          - |       168 KB |
| NpgsqlCopy                                           | 1              | 100            |     786.9 ms |    27.22 ms |    80.27 ms |     791.5 ms |           - |           - |          - |       167 KB |
| EfCoreInsert                                         | 10             | 100            |   1,126.2 ms |    54.58 ms |   160.92 ms |   1,141.8 ms |  10000.0000 |   1000.0000 |          - |    47,706 KB |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking     | 10             | 100            |     898.9 ms |    24.23 ms |    71.44 ms |     914.1 ms |   1000.0000 |           - |          - |     7,831 KB |
| NpgSqlInsert_SingularCommand_TypedValue              | 10             | 100            |   4,229.8 ms |    84.32 ms |   216.15 ms |   4,230.3 ms |           - |           - |          - |     1,720 KB |
| NpgsqlInsert_Batched_Boxed_Value                     | 10             | 100            |     479.2 ms |    34.27 ms |   101.06 ms |     465.2 ms |           - |           - |          - |     1,206 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value        | 10             | 100            |     460.8 ms |    30.21 ms |    89.08 ms |     447.3 ms |           - |           - |          - |     1,206 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue  | 10             | 100            |     456.8 ms |    30.45 ms |    89.77 ms |     441.1 ms |           - |           - |          - |     1,206 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue2 | 10             | 100            |     452.1 ms |    29.76 ms |    87.75 ms |     436.7 ms |           - |           - |          - |     1,206 KB |
| NpgsqlInsert_Batched_TypedValue                      | 10             | 100            |     493.6 ms |    31.24 ms |    92.11 ms |     492.3 ms |           - |           - |          - |     1,191 KB |
| NpgsqlCopy                                           | 10             | 100            |     778.7 ms |    28.53 ms |    82.78 ms |     797.0 ms |           - |           - |          - |       308 KB |
| EfCoreInsert                                         | 100            | 100            |   2,154.1 ms |    72.21 ms |   212.92 ms |   2,055.1 ms |  91000.0000 |  30000.0000 |  2000.0000 |   472,283 KB |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking     | 100            | 100            |   1,217.5 ms |    25.12 ms |    69.61 ms |   1,196.6 ms |  12000.0000 |   3000.0000 |          - |    73,736 KB |
| NpgSqlInsert_SingularCommand_TypedValue              | 100            | 100            |  41,184.8 ms |   821.98 ms | 1,872.06 ms |  41,068.6 ms |   3000.0000 |           - |          - |    17,189 KB |
| NpgsqlInsert_Batched_Boxed_Value                     | 100            | 100            |     835.6 ms |    25.13 ms |    71.71 ms |     859.8 ms |   2000.0000 |           - |          - |    11,648 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value        | 100            | 100            |     820.6 ms |    22.02 ms |    64.93 ms |     837.7 ms |   2000.0000 |           - |          - |    11,647 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue  | 100            | 100            |     841.7 ms |    23.63 ms |    69.30 ms |     856.8 ms |   2000.0000 |           - |          - |    11,646 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue2 | 100            | 100            |     839.5 ms |    24.03 ms |    69.34 ms |     849.5 ms |   2000.0000 |           - |          - |    11,658 KB |
| NpgsqlInsert_Batched_TypedValue                      | 100            | 100            |     842.3 ms |    22.89 ms |    67.49 ms |     859.1 ms |   2000.0000 |           - |          - |    11,490 KB |
| NpgsqlCopy                                           | 100            | 100            |     834.1 ms |    26.46 ms |    78.02 ms |     845.0 ms |           - |           - |          - |     1,714 KB |
| EfCoreInsert                                         | 1000           | 100            |  12,457.6 ms |   116.22 ms |   108.71 ms |  12,476.2 ms | 959000.0000 | 123000.0000 | 22000.0000 | 4,713,280 KB |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking     | 1000           | 100            |   6,481.7 ms |   136.94 ms |   401.61 ms |   6,374.2 ms | 114000.0000 |  44000.0000 |  9000.0000 |   728,490 KB |
| NpgSqlInsert_SingularCommand_TypedValue              | 1000           | 100            | 387,248.3 ms | 7,664.15 ms | 8,200.56 ms | 386,677.0 ms |  37000.0000 |   3000.0000 |          - |   171,882 KB |
| NpgsqlInsert_Batched_Boxed_Value                     | 1000           | 100            |   3,850.4 ms |    76.66 ms |   142.09 ms |   3,877.9 ms |  18000.0000 |   9000.0000 |          - |   115,458 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value        | 1000           | 100            |   3,797.8 ms |    75.19 ms |   143.05 ms |   3,829.6 ms |  18000.0000 |   9000.0000 |          - |   115,449 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue  | 1000           | 100            |   3,814.8 ms |    75.83 ms |   156.61 ms |   3,822.0 ms |  18000.0000 |   9000.0000 |          - |   115,454 KB |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue2 | 1000           | 100            |   3,778.9 ms |    75.54 ms |   197.66 ms |   3,793.3 ms |  18000.0000 |   9000.0000 |          - |   115,461 KB |
| NpgsqlInsert_Batched_TypedValue                      | 1000           | 100            |   3,778.0 ms |    75.53 ms |   180.98 ms |   3,805.5 ms |  18000.0000 |   9000.0000 |          - |   113,900 KB |
| NpgsqlCopy                                           | 1000           | 100            |   1,465.1 ms |    37.71 ms |   110.60 ms |   1,441.9 ms |   3000.0000 |           - |          - |    15,777 KB |


### Serialize
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



### Deserialize
| Method                                                   | Iterations |       Mean |    Error |   StdDev |    Gen 0 |    Gen 1 |   Gen 2 | Allocated |
|----------------------------------------------------------|------------|-----------:|---------:|---------:|---------:|---------:|--------:|----------:|
| NewtonsoftJson_Deserialize_Scalars_Float                 | 1000       | 3,332.6 us | 14.01 us | 10.94 us | 734.3750 | 234.3750 |       - |  3,375 KB |
| NewtonsoftJson_Deserialize_Scalars_Decimal               | 1000       | 3,324.3 us | 18.16 us | 16.99 us | 718.7500 | 273.4375 |       - |  3,344 KB |
| NewtonsoftJson_Deserialize_Scalars_NodaTime              | 1000       | 3,471.5 us |  9.48 us |  8.86 us | 867.1875 |   3.9063 |       - |  4,000 KB |
| NewtonsoftJson_Deserialize_NestedObjects_NodaTime        | 1000       | 5,566.5 us | 35.59 us | 33.29 us | 960.9375 |  54.6875 |       - |  4,578 KB |
| NewtonsoftJson_Deserialize_NestedObjects_Arrays_NodaTime | 1000       | 9,110.4 us | 40.24 us | 35.67 us | 921.8750 | 203.1250 | 46.8750 |  5,586 KB |
| SystemTextJson_Deserialize_Scalars_Float                 | 1000       |   687.2 us |  2.46 us |  2.30 us |  60.5469 |  19.5313 |       - |    281 KB |
| SystemTextJson_Deserialize_Scalars_Decimal               | 1000       |   690.4 us |  1.49 us |  1.32 us |  66.4063 |  21.4844 |       - |    313 KB |
| SystemTextJson_Deserialize_Scalars_NodaTime              | 1000       |   991.1 us |  4.09 us |  3.82 us | 156.2500 |  50.7813 |       - |    727 KB |
| SystemTextJson_Deserialize_NestedObjects_NodaTime        | 1000       | 1,717.7 us |  4.09 us |  3.82 us | 250.0000 | 117.1875 |       - |  1,328 KB |
| SystemTextJson_Deserialize_NestedObjects_Arrays_NodaTime | 1000       | 3,085.4 us |  7.64 us |  7.15 us | 339.8438 | 167.9688 |       - |  2,086 KB |


### Deserialize with Read-ahead for Type determination

#### Multiple
| Method                                                                            | Iterations |      Mean |    Error |   StdDev |      Gen 0 |     Gen 1 | Allocated |
|-----------------------------------------------------------------------------------|------------|----------:|---------:|---------:|-----------:|----------:|----------:|
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       |  29.72 ms | 0.196 ms | 0.174 ms |  4093.7500 |  187.5000 |     18 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       |  43.35 ms | 0.325 ms | 0.304 ms |  3250.0000 |  166.6667 |     15 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       |  49.88 ms | 0.302 ms | 0.282 ms |  3818.1818 |  181.8182 |     17 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       |  56.94 ms | 1.036 ms | 0.969 ms | 14444.4444 |  666.6667 |     65 MB |
| NewtonsoftJson_Deserialize_ReadAhead_LevelOne                                     | 1000       | 171.16 ms | 1.438 ms | 1.345 ms | 24000.0000 | 2000.0000 |    112 MB |

#### Single
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
