```

BenchmarkDotNet v0.13.12, Ubuntu 22.04.3 LTS (Jammy Jellyfish)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK 8.0.101
  [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2


```
| Method                                       | Iterations | CollectionSize | Mean [μs]    | Error [μs] | StdDev [μs] | Ratio  | RatioSD | Gen0       | Gen1     | Allocated [B] | Alloc Ratio |
|--------------------------------------------- |----------- |--------------- |-------------:|-----------:|------------:|-------:|--------:|-----------:|---------:|--------------:|------------:|
| Array_Add                                    | 10000      | 10             |     110.0 μs |    1.30 μs |     1.21 μs |   1.00 |    0.00 |   135.9863 |        - |      640000 B |        1.00 |
| ImmutableArray_Array_AsSpan_ToImmutableArray | 10000      | 10             |     228.6 μs |    0.93 μs |     0.77 μs |   2.08 |    0.03 |   271.9727 |        - |     1280000 B |        2.00 |
| ImmutableArray_CreateFrom_Array              | 10000      | 10             |     249.5 μs |    2.85 μs |     2.67 μs |   2.27 |    0.02 |   271.9727 |        - |     1280000 B |        2.00 |
| ImmutableArray_CreateFrom_Span               | 10000      | 10             |     255.2 μs |    7.50 μs |    22.10 μs |   2.44 |    0.22 |   271.9727 |        - |     1280000 B |        2.00 |
| List_Add                                     | 10000      | 10             |     296.9 μs |    1.00 μs |     0.88 μs |   2.70 |    0.03 |   203.6133 |        - |      960032 B |        1.50 |
| ImmutableArray_Array_ToImmutableArray        | 10000      | 10             |     352.8 μs |    1.61 μs |     1.51 μs |   3.21 |    0.03 |   271.9727 |        - |     1280000 B |        2.00 |
| ImmutableArray_CreateWithBuilder             | 10000      | 10             |     579.3 μs |    4.04 μs |     3.78 μs |   5.27 |    0.07 |   509.7656 |        - |     2400001 B |        3.75 |
| ImmutableArray_DirectAdd                     | 10000      | 10             |   1,409.8 μs |    9.31 μs |     7.77 μs |  12.81 |    0.16 |  1019.5313 |        - |     4800001 B |        7.50 |
| ImmutableList_CreateFrom_Array               | 10000      | 10             |   1,556.4 μs |    8.87 μs |     8.30 μs |  14.15 |    0.14 |  1207.0313 |        - |     5680001 B |        8.88 |
| ImmutableList_Array_ToImmutableList          | 10000      | 10             |   1,853.9 μs |    4.61 μs |     4.09 μs |  16.86 |    0.20 |  1257.8125 |        - |     5920001 B |        9.25 |
| ImmutableList_CreateWithBuilder              | 10000      | 10             |   2,933.2 μs |   10.61 μs |     9.92 μs |  26.68 |    0.25 |  1171.8750 |        - |     5520003 B |        8.63 |
| ImmutableList_DirectAdd                      | 10000      | 10             |   5,961.3 μs |   22.03 μs |    19.53 μs |  54.21 |    0.61 |  4078.1250 |        - |    19200006 B |       30.00 |
|                                              |            |                |              |            |             |        |         |            |          |               |             |
| Array_Add                                    | 10000      | 100            |     639.8 μs |    6.49 μs |     6.07 μs |   1.00 |    0.00 |   900.3906 |        - |     4240001 B |        1.00 |
| ImmutableArray_Array_AsSpan_ToImmutableArray | 10000      | 100            |     939.7 μs |    3.91 μs |     3.47 μs |   1.47 |    0.01 |  1801.7578 |   0.9766 |     8480001 B |        2.00 |
| ImmutableArray_CreateFrom_Span               | 10000      | 100            |     948.9 μs |    5.97 μs |     5.29 μs |   1.48 |    0.01 |  1801.7578 |   0.9766 |     8480001 B |        2.00 |
| ImmutableArray_CreateFrom_Array              | 10000      | 100            |     955.5 μs |    4.99 μs |     4.17 μs |   1.49 |    0.02 |  1801.7578 |   0.9766 |     8480001 B |        2.00 |
| ImmutableArray_Array_ToImmutableArray        | 10000      | 100            |   1,077.5 μs |    9.09 μs |     8.06 μs |   1.68 |    0.02 |  1800.7813 |        - |     8480001 B |        2.00 |
| List_Add                                     | 10000      | 100            |   1,641.3 μs |    5.08 μs |     4.50 μs |   2.56 |    0.02 |   968.7500 |        - |     4560033 B |        1.08 |
| ImmutableArray_CreateWithBuilder             | 10000      | 100            |   3,190.4 μs |   19.63 μs |    16.39 μs |   4.99 |    0.06 |  3332.0313 |   7.8125 |    15680003 B |        3.70 |
| ImmutableList_CreateFrom_Array               | 10000      | 100            |  13,751.0 μs |   55.31 μs |    49.03 μs |  21.48 |    0.21 | 11140.6250 | 140.6250 |    52480012 B |       12.38 |
| ImmutableList_Array_ToImmutableList          | 10000      | 100            |  16,793.1 μs |  129.14 μs |   120.80 μs |  26.25 |    0.29 | 11187.5000 |  93.7500 |    52720023 B |       12.43 |
| ImmutableArray_DirectAdd                     | 10000      | 100            |  24,731.4 μs |  239.75 μs |   200.21 μs |  38.65 |    0.48 | 48437.5000 |  31.2500 |   228000023 B |       53.77 |
| ImmutableList_CreateWithBuilder              | 10000      | 100            |  46,540.2 μs |   94.42 μs |    78.85 μs |  72.73 |    0.59 | 10272.7273 | 181.8182 |    48720067 B |       11.49 |
| ImmutableList_DirectAdd                      | 10000      | 100            | 117,835.3 μs |  566.68 μs |   502.35 μs | 184.03 |    1.88 | 73600.0000 | 600.0000 |   347040147 B |       81.85 |
