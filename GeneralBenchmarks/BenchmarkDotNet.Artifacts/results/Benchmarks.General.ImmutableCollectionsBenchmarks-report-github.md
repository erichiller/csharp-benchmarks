``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                                       Method | Iterations | CollectionSize |   Mean [μs] | Error [μs] | StdDev [μs] | Median [μs] | Ratio | RatioSD |       Gen0 |    Gen1 | Allocated [B] | Alloc Ratio |
|--------------------------------------------- |----------- |--------------- |------------:|-----------:|------------:|------------:|------:|--------:|-----------:|--------:|--------------:|------------:|
|                                    Array_Add |      10000 |             10 |    105.3 μs |    0.42 μs |     0.35 μs |    105.4 μs |  1.00 |    0.00 |   135.9863 |       - |      640000 B |        1.00 |
|               ImmutableArray_CreateFrom_Span |      10000 |             10 |    213.4 μs |    0.56 μs |     0.50 μs |    213.4 μs |  2.03 |    0.01 |   271.9727 |       - |     1280000 B |        2.00 |
| ImmutableArray_Array_AsSpan_ToImmutableArray |      10000 |             10 |    222.1 μs |    1.39 μs |     1.30 μs |    222.4 μs |  2.11 |    0.01 |   271.9727 |       - |     1280000 B |        2.00 |
|              ImmutableArray_CreateFrom_Array |      10000 |             10 |    244.3 μs |    6.07 μs |    17.89 μs |    231.4 μs |  2.59 |    0.05 |   271.9727 |       - |     1280000 B |        2.00 |
|        ImmutableArray_Array_ToImmutableArray |      10000 |             10 |    460.2 μs |    1.51 μs |     1.34 μs |    460.3 μs |  4.37 |    0.02 |   271.9727 |       - |     1280000 B |        2.00 |
|             ImmutableArray_CreateWithBuilder |      10000 |             10 |    595.5 μs |   10.78 μs |    11.98 μs |    595.2 μs |  5.67 |    0.09 |   509.7656 |       - |     2400001 B |        3.75 |
|                     ImmutableArray_DirectAdd |      10000 |             10 |  1,344.2 μs |   11.77 μs |     9.82 μs |  1,343.4 μs | 12.77 |    0.12 |  1019.5313 |       - |     4800002 B |        7.50 |
|                                              |            |                |             |            |             |             |       |         |            |         |               |             |
|                                    Array_Add |      10000 |            100 |    608.5 μs |    7.43 μs |     6.20 μs |    607.8 μs |  1.00 |    0.00 |   900.3906 |       - |     4240001 B |        1.00 |
|               ImmutableArray_CreateFrom_Span |      10000 |            100 |    913.9 μs |    4.42 μs |     4.13 μs |    913.7 μs |  1.50 |    0.02 |  1801.7578 |  1.9531 |     8480001 B |        2.00 |
| ImmutableArray_Array_AsSpan_ToImmutableArray |      10000 |            100 |    918.4 μs |   12.08 μs |    10.71 μs |    917.3 μs |  1.51 |    0.01 |  1801.7578 |  1.9531 |     8480001 B |        2.00 |
|              ImmutableArray_CreateFrom_Array |      10000 |            100 |    960.5 μs |   10.56 μs |     9.37 μs |    959.6 μs |  1.58 |    0.03 |  1801.7578 |  1.9531 |     8480001 B |        2.00 |
|        ImmutableArray_Array_ToImmutableArray |      10000 |            100 |  1,179.5 μs |    7.88 μs |     7.38 μs |  1,177.0 μs |  1.94 |    0.03 |  1800.7813 |  1.9531 |     8480002 B |        2.00 |
|             ImmutableArray_CreateWithBuilder |      10000 |            100 |  2,745.3 μs |   19.23 μs |    17.05 μs |  2,740.0 μs |  4.51 |    0.04 |  3332.0313 |  7.8125 |    15680004 B |        3.70 |
|                     ImmutableArray_DirectAdd |      10000 |            100 | 25,475.1 μs |  183.58 μs |   143.33 μs | 25,514.0 μs | 41.94 |    0.53 | 48437.5000 | 31.2500 |   228000029 B |       53.77 |
