using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using BenchmarkDotNet.Attributes;

using Benchmarks.Common;

namespace Benchmarks.General;
/* RESULTS:
 
 # Iterations = 100_000
|    Method | CollectionSize |    Mean [ms] | Error [ms] | StdDev [ms] | Ratio | RatioSD |        Gen0 |        Gen1 | Allocated [B] | Alloc Ratio |
|---------- |--------------- |-------------:|-----------:|------------:|------:|--------:|------------:|------------:|--------------:|------------:|
| Array_Add |             10 |     1.045 ms |  0.0103 ms |   0.0092 ms |  1.00 |    0.00 |   1359.3750 |           - |     6400002 B |        1.00 |
|  List_Add |             10 |     2.719 ms |  0.0518 ms |   0.0654 ms |  2.60 |    0.07 |   2039.0625 |           - |     9600004 B |        1.50 |
|           |                |              |            |             |       |         |             |             |               |             |
| Array_Add |            100 |     6.543 ms |  0.1307 ms |   0.3105 ms |  1.00 |    0.00 |   9007.8125 |           - |    42400007 B |        1.00 |
|  List_Add |            100 |    16.420 ms |  0.3221 ms |   0.3834 ms |  2.51 |    0.09 |   9687.5000 |           - |    45600029 B |        1.08 |
|           |                |              |            |             |       |         |             |             |               |             |
| Array_Add |           1000 |    51.306 ms |  0.2053 ms |   0.1920 ms |  1.00 |    0.00 |  85500.0000 |           - |   402400094 B |        1.00 |
|  List_Add |           1000 |   148.249 ms |  2.8585 ms |   2.6739 ms |  2.89 |    0.05 |  86000.0000 |   1250.0000 |   405600234 B |        1.01 |
|           |                |              |            |             |       |         |             |             |               |             |
| Array_Add |          10000 |   502.077 ms |  8.2309 ms |   9.7983 ms |  1.00 |    0.00 | 847000.0000 |           - |  4002400936 B |        1.00 |
|  List_Add |          10000 | 1,457.430 ms | 27.3344 ms |  30.3821 ms |  2.90 |    0.06 | 847000.0000 | 105000.0000 |  4005600936 B |        1.00 |

 # Iterations = 100
 
|                                       Method | CollectionSize |    Mean [us] | Error [us] | StdDev [us] |  Median [us] |  Ratio | RatioSD |     Gen0 |   Gen1 | Allocated [B] | Alloc Ratio |
|--------------------------------------------- |--------------- |-------------:|-----------:|------------:|-------------:|-------:|--------:|---------:|-------:|--------------:|------------:|
|                                    Array_Add |             10 |     1.050 us |  0.0045 us |   0.0040 us |     1.050 us |   1.00 |    0.00 |   1.3599 |      - |        6400 B |        1.00 |
| ImmutableArray_Array_AsSpan_ToImmutableArray |             10 |     2.323 us |  0.0450 us |   0.0615 us |     2.310 us |   2.20 |    0.07 |   2.7199 |      - |       12800 B |        2.00 |
|               ImmutableArray_CreateFrom_Span |             10 |     2.330 us |  0.0504 us |   0.1487 us |     2.286 us |   2.44 |    0.08 |   2.7199 |      - |       12800 B |        2.00 |
|              ImmutableArray_CreateFrom_Array |             10 |     2.549 us |  0.0509 us |   0.1129 us |     2.576 us |   2.47 |    0.10 |   2.7199 |      - |       12800 B |        2.00 |
|                                     List_Add |             10 |     2.620 us |  0.0231 us |   0.0180 us |     2.627 us |   2.49 |    0.02 |   2.0447 |      - |        9632 B |        1.50 |
|        ImmutableArray_Array_ToImmutableArray |             10 |     4.796 us |  0.0933 us |   0.1309 us |     4.811 us |   4.51 |    0.15 |   2.7161 |      - |       12800 B |        2.00 |
|             ImmutableArray_CreateWithBuilder |             10 |     5.485 us |  0.0993 us |   0.1220 us |     5.509 us |   5.22 |    0.14 |   5.0964 |      - |       24000 B |        3.75 |
|                     ImmutableArray_DirectAdd |             10 |    14.283 us |  0.2444 us |   0.2286 us |    14.257 us |  13.60 |    0.22 |  10.1929 |      - |       48000 B |        7.50 |
|               ImmutableList_CreateFrom_Array |             10 |    23.615 us |  0.2753 us |   0.2441 us |    23.567 us |  22.48 |    0.21 |  12.5732 |      - |       59200 B |        9.25 |
|          ImmutableList_Array_ToImmutableList |             10 |    23.624 us |  0.3815 us |   0.3569 us |    23.560 us |  22.47 |    0.36 |  12.5732 |      - |       59200 B |        9.25 |
|              ImmutableList_CreateWithBuilder |             10 |    36.961 us |  0.3109 us |   0.2908 us |    36.912 us |  35.17 |    0.32 |  11.7188 |      - |       55200 B |        8.62 |
|                      ImmutableList_DirectAdd |             10 |    70.358 us |  1.3162 us |   1.1668 us |    70.131 us |  66.99 |    1.09 |  40.7715 |      - |      192000 B |       30.00 |
|                                              |                |              |            |             |              |        |         |          |        |               |             |
|                                    Array_Add |            100 |     6.762 us |  0.1339 us |   0.2481 us |     6.731 us |   1.00 |    0.00 |   9.0103 |      - |       42400 B |        1.00 |
|               ImmutableArray_CreateFrom_Span |            100 |     9.782 us |  0.2546 us |   0.7346 us |     9.744 us |   1.52 |    0.12 |  18.0206 | 0.0153 |       84800 B |        2.00 |
| ImmutableArray_Array_AsSpan_ToImmutableArray |            100 |    10.159 us |  0.2016 us |   0.3478 us |    10.146 us |   1.50 |    0.08 |  18.0206 |      - |       84800 B |        2.00 |
|              ImmutableArray_CreateFrom_Array |            100 |    10.773 us |  0.3359 us |   0.9904 us |    10.669 us |   1.48 |    0.09 |  18.0206 | 0.0153 |       84800 B |        2.00 |
|        ImmutableArray_Array_ToImmutableArray |            100 |    11.725 us |  0.1008 us |   0.0894 us |    11.745 us |   1.73 |    0.08 |  18.0206 | 0.0153 |       84800 B |        2.00 |
|                                     List_Add |            100 |    17.312 us |  0.3126 us |   0.2924 us |    17.410 us |   2.56 |    0.13 |   9.6741 |      - |       45632 B |        1.08 |
|             ImmutableArray_CreateWithBuilder |            100 |    29.499 us |  0.5847 us |   0.9442 us |    29.346 us |   4.35 |    0.24 |  33.3252 | 0.0610 |      156800 B |        3.70 |
|          ImmutableList_Array_ToImmutableList |            100 |   186.664 us |  1.7379 us |   1.6256 us |   186.500 us |  27.59 |    1.14 | 111.8164 | 1.2207 |      527200 B |       12.43 |
|               ImmutableList_CreateFrom_Array |            100 |   193.643 us |  2.3155 us |   2.1659 us |   194.038 us |  28.62 |    1.18 | 111.8164 | 1.2207 |      527200 B |       12.43 |
|                     ImmutableArray_DirectAdd |            100 |   283.849 us |  6.0185 us |  17.7456 us |   285.021 us |  41.15 |    2.70 | 484.3750 | 0.4883 |     2280000 B |       53.77 |
|              ImmutableList_CreateWithBuilder |            100 |   591.285 us |  4.8241 us |   4.5124 us |   590.638 us |  87.40 |    3.75 | 103.5156 | 0.9766 |      487201 B |       11.49 |
|                      ImmutableList_DirectAdd |            100 | 1,523.168 us | 28.3529 us |  26.5213 us | 1,516.921 us | 225.20 |   11.52 | 736.3281 | 5.8594 |     3470402 B |       81.85 |


****

|                                       Method | Iterations | CollectionSize |   Mean [us] | Error [us] | StdDev [us] | Median [us] | Ratio | RatioSD |       Gen0 |    Gen1 | Allocated [B] | Alloc Ratio |
|--------------------------------------------- |----------- |--------------- |------------:|-----------:|------------:|------------:|------:|--------:|-----------:|--------:|--------------:|------------:|
|                                    Array_Add |      10000 |             10 |    105.3 us |    0.42 us |     0.35 us |    105.4 us |  1.00 |    0.00 |   135.9863 |       - |      640000 B |        1.00 |
|               ImmutableArray_CreateFrom_Span |      10000 |             10 |    213.4 us |    0.56 us |     0.50 us |    213.4 us |  2.03 |    0.01 |   271.9727 |       - |     1280000 B |        2.00 |
| ImmutableArray_Array_AsSpan_ToImmutableArray |      10000 |             10 |    222.1 us |    1.39 us |     1.30 us |    222.4 us |  2.11 |    0.01 |   271.9727 |       - |     1280000 B |        2.00 |
|              ImmutableArray_CreateFrom_Array |      10000 |             10 |    244.3 us |    6.07 us |    17.89 us |    231.4 us |  2.59 |    0.05 |   271.9727 |       - |     1280000 B |        2.00 |
|        ImmutableArray_Array_ToImmutableArray |      10000 |             10 |    460.2 us |    1.51 us |     1.34 us |    460.3 us |  4.37 |    0.02 |   271.9727 |       - |     1280000 B |        2.00 |
|             ImmutableArray_CreateWithBuilder |      10000 |             10 |    595.5 us |   10.78 us |    11.98 us |    595.2 us |  5.67 |    0.09 |   509.7656 |       - |     2400001 B |        3.75 |
|                     ImmutableArray_DirectAdd |      10000 |             10 |  1,344.2 us |   11.77 us |     9.82 us |  1,343.4 us | 12.77 |    0.12 |  1019.5313 |       - |     4800002 B |        7.50 |
|                                              |            |                |             |            |             |             |       |         |            |         |               |             |
|                                    Array_Add |      10000 |            100 |    608.5 us |    7.43 us |     6.20 us |    607.8 us |  1.00 |    0.00 |   900.3906 |       - |     4240001 B |        1.00 |
|               ImmutableArray_CreateFrom_Span |      10000 |            100 |    913.9 us |    4.42 us |     4.13 us |    913.7 us |  1.50 |    0.02 |  1801.7578 |  1.9531 |     8480001 B |        2.00 |
| ImmutableArray_Array_AsSpan_ToImmutableArray |      10000 |            100 |    918.4 us |   12.08 us |    10.71 us |    917.3 us |  1.51 |    0.01 |  1801.7578 |  1.9531 |     8480001 B |        2.00 |
|              ImmutableArray_CreateFrom_Array |      10000 |            100 |    960.5 us |   10.56 us |     9.37 us |    959.6 us |  1.58 |    0.03 |  1801.7578 |  1.9531 |     8480001 B |        2.00 |
|        ImmutableArray_Array_ToImmutableArray |      10000 |            100 |  1,179.5 us |    7.88 us |     7.38 us |  1,177.0 us |  1.94 |    0.03 |  1800.7813 |  1.9531 |     8480002 B |        2.00 |
|             ImmutableArray_CreateWithBuilder |      10000 |            100 |  2,745.3 us |   19.23 us |    17.05 us |  2,740.0 us |  4.51 |    0.04 |  3332.0313 |  7.8125 |    15680004 B |        3.70 |
|                     ImmutableArray_DirectAdd |      10000 |            100 | 25,475.1 us |  183.58 us |   143.33 us | 25,514.0 us | 41.94 |    0.53 | 48437.5000 | 31.2500 |   228000029 B |       53.77 |





.NET 7.0.15

| Method                                       | Iterations | CollectionSize | Mean [us]    | Error [us] | StdDev [us] | Median [us]  | Ratio  | RatioSD | Gen0       | Gen1     | Allocated [B] | Alloc Ratio |
   |--------------------------------------------- |----------- |--------------- |-------------:|-----------:|------------:|-------------:|-------:|--------:|-----------:|---------:|--------------:|------------:|
   | Array_Add                                    | 10000      | 10             |     123.3 us |    2.44 us |     5.85 us |     121.3 us |   1.00 |    0.00 |   135.9863 |        - |      640000 B |        1.00 |
   | ImmutableArray_CreateFrom_Span               | 10000      | 10             |     216.6 us |    1.79 us |     1.49 us |     216.0 us |   1.65 |    0.06 |   271.9727 |        - |     1280000 B |        2.00 |
   | ImmutableArray_Array_AsSpan_ToImmutableArray | 10000      | 10             |     220.5 us |    2.43 us |     2.27 us |     219.9 us |   1.69 |    0.08 |   271.9727 |        - |     1280000 B |        2.00 |
   | ImmutableArray_CreateFrom_Array              | 10000      | 10             |     266.0 us |    3.84 us |     3.21 us |     266.3 us |   2.02 |    0.08 |   271.9727 |        - |     1280000 B |        2.00 |
   | List_Add                                     | 10000      | 10             |     286.4 us |    2.48 us |     2.32 us |     286.9 us |   2.20 |    0.10 |   203.6133 |        - |      960032 B |        1.50 |
   | ImmutableArray_Array_ToImmutableArray        | 10000      | 10             |     569.4 us |    1.71 us |     1.52 us |     568.9 us |   4.35 |    0.16 |   271.4844 |        - |     1280001 B |        2.00 |
   | ImmutableArray_CreateWithBuilder             | 10000      | 10             |     641.4 us |    6.67 us |     5.20 us |     638.6 us |   4.87 |    0.17 |   509.7656 |        - |     2400001 B |        3.75 |
   | ImmutableArray_DirectAdd                     | 10000      | 10             |   1,619.1 us |   14.57 us |    13.63 us |   1,615.7 us |  12.45 |    0.58 |  1019.5313 |        - |     4800002 B |        7.50 |
   | ImmutableList_Array_ToImmutableList          | 10000      | 10             |   2,336.4 us |    7.63 us |     6.37 us |   2,337.8 us |  17.78 |    0.67 |  1257.8125 |        - |     5920004 B |        9.25 |
   | ImmutableList_CreateFrom_Array               | 10000      | 10             |   2,377.1 us |   15.36 us |    12.83 us |   2,378.0 us |  18.09 |    0.63 |  1257.8125 |        - |     5920004 B |        9.25 |
   | ImmutableList_CreateWithBuilder              | 10000      | 10             |   3,629.2 us |   11.78 us |    10.45 us |   3,630.2 us |  27.74 |    1.10 |  1171.8750 |        - |     5520004 B |        8.63 |
   | ImmutableList_DirectAdd                      | 10000      | 10             |   6,765.1 us |   48.67 us |    45.52 us |   6,775.3 us |  52.01 |    2.36 |  4078.1250 |        - |    19200007 B |       30.00 |
   |                                              |            |                |              |            |             |              |        |         |            |          |               |             |
   | Array_Add                                    | 10000      | 100            |     678.9 us |   13.04 us |    13.95 us |     677.5 us |   1.00 |    0.00 |   900.3906 |        - |     4240001 B |        1.00 |
   | ImmutableArray_CreateFrom_Span               | 10000      | 100            |     949.4 us |    9.92 us |     9.28 us |     945.6 us |   1.39 |    0.03 |  1800.7813 |   1.9531 |     8480002 B |        2.00 |
   | ImmutableArray_Array_AsSpan_ToImmutableArray | 10000      | 100            |     954.7 us |   13.32 us |    12.46 us |     953.6 us |   1.40 |    0.03 |  1800.7813 |   1.9531 |     8480002 B |        2.00 |
   | ImmutableArray_CreateFrom_Array              | 10000      | 100            |   1,005.7 us |   17.70 us |    16.55 us |   1,000.5 us |   1.48 |    0.03 |  1800.7813 |   1.9531 |     8480002 B |        2.00 |
   | ImmutableArray_Array_ToImmutableArray        | 10000      | 100            |   1,360.0 us |   18.45 us |    17.26 us |   1,356.8 us |   2.00 |    0.05 |  1800.7813 |   1.9531 |     8480002 B |        2.00 |
   | List_Add                                     | 10000      | 100            |   1,914.8 us |    5.02 us |     4.45 us |   1,914.2 us |   2.81 |    0.06 |   968.7500 |        - |     4560034 B |        1.08 |
   | ImmutableArray_CreateWithBuilder             | 10000      | 100            |   3,098.2 us |   23.79 us |    21.09 us |   3,100.5 us |   4.55 |    0.08 |  3332.0313 |   7.8125 |    15680004 B |        3.70 |
   | ImmutableList_Array_ToImmutableList          | 10000      | 100            |  19,702.1 us |  167.49 us |   156.67 us |  19,720.1 us |  28.95 |    0.71 | 11187.5000 | 125.0000 |    52720029 B |       12.43 |
   | ImmutableList_CreateFrom_Array               | 10000      | 100            |  19,746.3 us |  181.44 us |   169.72 us |  19,768.1 us |  29.01 |    0.52 | 11187.5000 | 125.0000 |    52720029 B |       12.43 |
   | ImmutableArray_DirectAdd                     | 10000      | 100            |  28,468.1 us |  247.91 us |   231.90 us |  28,466.9 us |  41.82 |    0.75 | 48437.5000 |  31.2500 |   228000029 B |       53.77 |
   | ImmutableList_CreateWithBuilder              | 10000      | 100            |  62,834.2 us |  172.73 us |   153.12 us |  62,816.2 us |  92.27 |    2.01 | 10250.0000 | 125.0000 |    48720117 B |       11.49 |
   | ImmutableList_DirectAdd                      | 10000      | 100            | 146,752.2 us |  594.97 us |   527.42 us | 146,761.4 us | 215.50 |    4.72 | 73750.0000 | 750.0000 |   347040234 B |       81.85 |
   
   BenchmarkDotNet v0.13.12, Ubuntu 22.04.3 LTS (Jammy Jellyfish)
   Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
   .NET SDK 8.0.101
     [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2
     DefaultJob : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2
   
   
   | Method                                       | Iterations | CollectionSize | Mean [us]    | Error [us] | StdDev [us] | Ratio  | RatioSD | Gen0       | Gen1     | Allocated [B] | Alloc Ratio |
   |--------------------------------------------- |----------- |--------------- |-------------:|-----------:|------------:|-------:|--------:|-----------:|---------:|--------------:|------------:|
   | Array_Add                                    | 10000      | 10             |     110.0 us |    1.30 us |     1.21 us |   1.00 |    0.00 |   135.9863 |        - |      640000 B |        1.00 |
   | ImmutableArray_Array_AsSpan_ToImmutableArray | 10000      | 10             |     228.6 us |    0.93 us |     0.77 us |   2.08 |    0.03 |   271.9727 |        - |     1280000 B |        2.00 |
   | ImmutableArray_CreateFrom_Array              | 10000      | 10             |     249.5 us |    2.85 us |     2.67 us |   2.27 |    0.02 |   271.9727 |        - |     1280000 B |        2.00 |
   | ImmutableArray_CreateFrom_Span               | 10000      | 10             |     255.2 us |    7.50 us |    22.10 us |   2.44 |    0.22 |   271.9727 |        - |     1280000 B |        2.00 |
   | List_Add                                     | 10000      | 10             |     296.9 us |    1.00 us |     0.88 us |   2.70 |    0.03 |   203.6133 |        - |      960032 B |        1.50 |
   | ImmutableArray_Array_ToImmutableArray        | 10000      | 10             |     352.8 us |    1.61 us |     1.51 us |   3.21 |    0.03 |   271.9727 |        - |     1280000 B |        2.00 |
   | ImmutableArray_CreateWithBuilder             | 10000      | 10             |     579.3 us |    4.04 us |     3.78 us |   5.27 |    0.07 |   509.7656 |        - |     2400001 B |        3.75 |
   | ImmutableArray_DirectAdd                     | 10000      | 10             |   1,409.8 us |    9.31 us |     7.77 us |  12.81 |    0.16 |  1019.5313 |        - |     4800001 B |        7.50 |
   | ImmutableList_CreateFrom_Array               | 10000      | 10             |   1,556.4 us |    8.87 us |     8.30 us |  14.15 |    0.14 |  1207.0313 |        - |     5680001 B |        8.88 |
   | ImmutableList_Array_ToImmutableList          | 10000      | 10             |   1,853.9 us |    4.61 us |     4.09 us |  16.86 |    0.20 |  1257.8125 |        - |     5920001 B |        9.25 |
   | ImmutableList_CreateWithBuilder              | 10000      | 10             |   2,933.2 us |   10.61 us |     9.92 us |  26.68 |    0.25 |  1171.8750 |        - |     5520003 B |        8.63 |
   | ImmutableList_DirectAdd                      | 10000      | 10             |   5,961.3 us |   22.03 us |    19.53 us |  54.21 |    0.61 |  4078.1250 |        - |    19200006 B |       30.00 |
   |                                              |            |                |              |            |             |        |         |            |          |               |             |
   | Array_Add                                    | 10000      | 100            |     639.8 us |    6.49 us |     6.07 us |   1.00 |    0.00 |   900.3906 |        - |     4240001 B |        1.00 |
   | ImmutableArray_Array_AsSpan_ToImmutableArray | 10000      | 100            |     939.7 us |    3.91 us |     3.47 us |   1.47 |    0.01 |  1801.7578 |   0.9766 |     8480001 B |        2.00 |
   | ImmutableArray_CreateFrom_Span               | 10000      | 100            |     948.9 us |    5.97 us |     5.29 us |   1.48 |    0.01 |  1801.7578 |   0.9766 |     8480001 B |        2.00 |
   | ImmutableArray_CreateFrom_Array              | 10000      | 100            |     955.5 us |    4.99 us |     4.17 us |   1.49 |    0.02 |  1801.7578 |   0.9766 |     8480001 B |        2.00 |
   | ImmutableArray_Array_ToImmutableArray        | 10000      | 100            |   1,077.5 us |    9.09 us |     8.06 us |   1.68 |    0.02 |  1800.7813 |        - |     8480001 B |        2.00 |
   | List_Add                                     | 10000      | 100            |   1,641.3 us |    5.08 us |     4.50 us |   2.56 |    0.02 |   968.7500 |        - |     4560033 B |        1.08 |
   | ImmutableArray_CreateWithBuilder             | 10000      | 100            |   3,190.4 us |   19.63 us |    16.39 us |   4.99 |    0.06 |  3332.0313 |   7.8125 |    15680003 B |        3.70 |
   | ImmutableList_CreateFrom_Array               | 10000      | 100            |  13,751.0 us |   55.31 us |    49.03 us |  21.48 |    0.21 | 11140.6250 | 140.6250 |    52480012 B |       12.38 |
   | ImmutableList_Array_ToImmutableList          | 10000      | 100            |  16,793.1 us |  129.14 us |   120.80 us |  26.25 |    0.29 | 11187.5000 |  93.7500 |    52720023 B |       12.43 |
   | ImmutableArray_DirectAdd                     | 10000      | 100            |  24,731.4 us |  239.75 us |   200.21 us |  38.65 |    0.48 | 48437.5000 |  31.2500 |   228000023 B |       53.77 |
   | ImmutableList_CreateWithBuilder              | 10000      | 100            |  46,540.2 us |   94.42 us |    78.85 us |  72.73 |    0.59 | 10272.7273 | 181.8182 |    48720067 B |       11.49 |
   | ImmutableList_DirectAdd                      | 10000      | 100            | 117,835.3 us |  566.68 us |   502.35 us | 184.03 |    1.88 | 73600.0000 | 600.0000 |   347040147 B |       81.85 |
   
   
   

 */

[ Config( typeof(BenchmarkConfig) ) ]
[ SuppressMessage( "Performance", "CA1822:Mark members as static" ) ]
public class ImmutableCollectionsBenchmarks {
    [ Params( // 100,
              10_000
              // 100_000
    ) ]
    [ SuppressMessage( "ReSharper", "UnassignedField.Global" ) ] // TODO: also try, iterations = 1, CollectionSize = 1_000_000 or 100_000
    public int Iterations;

    [ Params( 10, 100
                  // , 1_000
                  // , 10_000
                  ) ]
    [ SuppressMessage( "ReSharper", "UnassignedField.Global" ) ] // TODO: also try, iterations = 1, CollectionSize = 1_000_000 or 100_000
    public int CollectionSize;


    /*
     * ImmutableArray
     */

    [ Benchmark( Baseline = true ) ]
    [ BenchmarkCategory( "Array" ) ]
    public int[] Array_Add( ) {
        int[] collection = Array.Empty<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            collection = new int[ CollectionSize ];
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection[ i ] = i;
            }
        }
        return collection;
    }

    /*
     * List
     */

    [ Benchmark() ]
    [ BenchmarkCategory( "List" ) ]
    public List<int> List_Add( ) {
        List<int> collection = new List<int>( 0 );
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            collection = new List<int>( CollectionSize );
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection.Add( i );
            }
        }
        return collection;
    }

    /*
     * ImmutableArray
     */

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableArray" ) ]
    public ImmutableArray<int> ImmutableArray_DirectAdd( ) {
        var collection = ImmutableArray<int>.Empty;
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            collection = ImmutableArray<int>.Empty;
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection = collection.Add( i );
            }
        }
        return collection;
    }

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableArray" ) ]
    public ImmutableArray<int> ImmutableArray_CreateWithBuilder( ) {
        var                         output = ImmutableArray.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = ImmutableArray.CreateBuilder<int>();
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection.Add( i );
            }
            output = collection.ToImmutable();
        }
        return output;
    }

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableArray" ) ]
    public ImmutableArray<int> ImmutableArray_CreateFrom_Array( ) {
        var   output = ImmutableArray.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = new int[ CollectionSize ];
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection[ i ] = i;
            }
            output = ImmutableArray.Create( collection );
        }
        return output;
    }

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableArray" ) ]
    public ImmutableArray<int> ImmutableArray_CreateFrom_Span( ) {
        var   output = ImmutableArray.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = new int[ CollectionSize ];
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection[ i ] = i;
            }
            output = ImmutableArray.Create( collection.AsSpan() );
        }
        return output;
    }


    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableArray" ) ]
    public ImmutableArray<int> ImmutableArray_Array_ToImmutableArray( ) {
        var   output = ImmutableArray.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = new int[ CollectionSize ];
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection[ i ] = i;
            }
            output = collection.ToImmutableArray();
        }
        return output;
    }


    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableArray" ) ]
    public ImmutableArray<int> ImmutableArray_Array_AsSpan_ToImmutableArray( ) {
        var   output = ImmutableArray.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = new int[ CollectionSize ];
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection[ i ] = i;
            }
            output = collection.AsSpan().ToImmutableArray();
        }
        return output;
    }

    /*
     * ImmutableList
     */

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableList" ) ]
    public ImmutableList<int> ImmutableList_DirectAdd( ) {
        var collection = ImmutableList<int>.Empty;
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            collection = ImmutableList<int>.Empty;
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection = collection.Add( i );
            }
        }
        return collection;
    }

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableList" ) ]
    public ImmutableList<int> ImmutableList_CreateWithBuilder( ) {
        var                        output = ImmutableList.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = ImmutableList.CreateBuilder<int>();
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection.Add( i );
            }
            output = collection.ToImmutable();
        }
        return output;
    }

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableList" ) ]
    public ImmutableList<int> ImmutableList_CreateFrom_Array( ) {
        var   output = ImmutableList.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = new int[ CollectionSize ];
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection[ i ] = i;
            }
            output = ImmutableList.Create( collection );
        }
        return output;
    }

    [ Benchmark() ]
    [ BenchmarkCategory( "ImmutableList" ) ]
    public ImmutableList<int> ImmutableList_Array_ToImmutableList( ) {
        var   output = ImmutableList.Create<int>();
        for ( int iteration = 0 ; iteration < Iterations ; iteration++ ) {
            var collection = new int[ CollectionSize ];
            for ( int i = 0 ; i < CollectionSize ; i++ ) {
                collection[ i ] = i;
            }
            output = collection.ToImmutableList();
        }
        return output;
    }
}