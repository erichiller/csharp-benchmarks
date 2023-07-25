# gRPC Benchmarks


## Results

### `ListIAsyncEnumerableComparison`

**Last update: 2023 July 24**

Takeaways:
- Tt appears that `EnumerableReply` becomes *much* more efficient on really large transfers. `ListReply` stays competitive throughout
- `ValueTask` has sizeable advantages, particularly on small-sized replies

BenchmarkDotNet v0.13.6, Ubuntu 22.04.2 LTS (Jammy Jellyfish)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK 7.0.306
[Host]     : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2
DefaultJob : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2


| Method                   | Count |  Mean [us] | Error [us] | StdDev [us] | Median [us] |    Gen0 | Allocated [B] |
|--------------------------|-------|-----------:|-----------:|------------:|------------:|--------:|--------------:|
| Single                   | 1     |   192.1 us |    3.78 us |     7.01 us |    191.0 us |  1.4648 |        6878 B |
| SingleAsyncValueTask     | 1     |   196.5 us |    3.89 us |     5.81 us |    196.1 us |  1.4648 |        7205 B |
| EnumerableReply          | 1     |   208.7 us |    4.15 us |     6.70 us |    210.0 us |  1.4648 |        8234 B |
| EnumerableReply          | 10    |   212.7 us |    4.01 us |     6.24 us |    211.9 us |  1.4648 |        8693 B |
| ListReplySync            | 1     |   212.7 us |    4.20 us |     5.60 us |    211.8 us |  1.9531 |       10997 B |
| ListReplySync            | 10    |   214.3 us |    4.28 us |     5.26 us |    214.5 us |  2.4414 |       11442 B |
| ArrayReplyValueTaskAsync | 10    |   226.2 us |    4.40 us |     6.31 us |    227.4 us |  1.9531 |       10383 B |
| ArrayReplyAsync          | 1     |   227.3 us |    4.51 us |    12.03 us |    225.4 us |  1.9531 |        9761 B |
| AsyncEnumerableReply     | 1     |   227.3 us |    4.53 us |    12.48 us |    226.9 us |  1.4648 |        8465 B |
| ListReplyAsync           | 10    |   228.2 us |    4.49 us |     7.26 us |    227.1 us |  1.9531 |       11778 B |
| ArrayReplyAsync          | 10    |   229.0 us |    5.15 us |    14.62 us |    226.7 us |  1.9531 |       10369 B |
| ListReplyValueTaskAsync  | 1     |   229.5 us |    5.10 us |    13.35 us |    226.6 us |  2.4414 |       11349 B |
| ListReplyAsync           | 1     |   241.4 us |   10.13 us |    29.40 us |    229.1 us |  1.9531 |       11269 B |
| ArrayReplyValueTaskAsync | 1     |   241.9 us |    7.42 us |    20.81 us |    237.1 us |  1.9531 |        9774 B |
| SingleAsyncTask          | 1     |   242.3 us |   11.81 us |    33.11 us |    232.2 us |  0.9766 |        7175 B |
| AsyncEnumerableReply     | 10    |   290.3 us |   23.20 us |    65.07 us |    260.4 us |  1.9531 |        9781 B |
| ListReplyValueTaskAsync  | 10    |   390.3 us |   36.30 us |   107.02 us |    406.1 us |  2.4414 |       11797 B |
| SingleAsyncValueTask     | 10    | 1,785.9 us |   35.41 us |    60.13 us |  1,770.8 us | 11.7188 |       68212 B |
| SingleAsyncTask          | 10    | 1,802.7 us |   34.73 us |    76.95 us |  1,785.9 us | 11.7188 |       68144 B |
| Single                   | 10    | 1,924.0 us |   35.59 us |    58.48 us |  1,916.7 us | 11.7188 |       66939 B |

| Method                   | Count  |       Mean [us] |      Error [us] |     StdDev [us] |     Median [us] |        Gen0 |      Gen1 |     Gen2 | Allocated [B] |
|--------------------------|--------|----------------:|----------------:|----------------:|----------------:|------------:|----------:|---------:|--------------:|
| Single                   | 1      |        193.4 us |         3.75 us |         5.26 us |        192.8 us |      1.4648 |         - |        - |        6877 B |
| SingleAsyncTask          | 1      |        204.5 us |         7.49 us |        20.62 us |        201.9 us |      1.4648 |         - |        - |        7160 B |
| ArrayReplyValueTaskAsync | 1      |        210.3 us |         5.63 us |        15.98 us |        206.0 us |      1.9531 |         - |        - |        9774 B |
| ListReplyAsync           | 1      |        221.5 us |         8.26 us |        22.18 us |        215.0 us |      1.9531 |         - |        - |       11275 B |
| EnumerableReply          | 1      |        229.1 us |         5.38 us |        14.44 us |        224.9 us |      1.4648 |         - |        - |        8237 B |
| ListReplyValueTaskAsync  | 1      |        246.5 us |        18.26 us |        52.68 us |        223.0 us |      2.4414 |         - |        - |       11295 B |
| AsyncEnumerableReply     | 1      |        258.4 us |        24.31 us |        70.92 us |        223.8 us |      1.4648 |         - |        - |        8477 B |
| ArrayReplyAsync          | 1      |        277.3 us |        30.69 us |        87.56 us |        238.4 us |      1.9531 |         - |        - |        9804 B |
| ListReplySync            | 1      |        310.2 us |        32.61 us |        93.56 us |        282.5 us |      1.9531 |         - |        - |       11026 B |
| SingleAsyncValueTask     | 1      |        360.7 us |        26.14 us |        76.26 us |        361.9 us |      1.4648 |         - |        - |        7198 B |
| ListReplySync            | 10000  |      4,589.6 us |        91.42 us |       125.13 us |      4,557.0 us |     78.1250 |   70.3125 |  39.0625 |      514334 B |
| EnumerableReply          | 10000  |      4,680.6 us |        89.46 us |       252.32 us |      4,610.1 us |     78.1250 |   70.3125 |  39.0625 |      511604 B |
| ArrayReplyAsync          | 10000  |      4,751.7 us |       196.66 us |       570.55 us |      4,530.6 us |    156.2500 |   54.6875 |        - |      818202 B |
| ListReplyAsync           | 10000  |      4,794.3 us |       128.82 us |       361.22 us |      4,708.0 us |     78.1250 |   70.3125 |  39.0625 |      514864 B |
| ArrayReplyValueTaskAsync | 10000  |      4,797.7 us |       237.99 us |       690.46 us |      4,531.0 us |    156.2500 |   54.6875 |        - |      818179 B |
| ListReplyValueTaskAsync  | 10000  |      4,981.4 us |       174.51 us |       500.71 us |      4,883.6 us |     78.1250 |   70.3125 |  39.0625 |      514605 B |
| AsyncEnumerableReply     | 10000  |     21,315.9 us |     1,178.49 us |     3,474.80 us |     21,846.9 us |    250.0000 |         - |        - |     1453413 B |
| ListReplyValueTaskAsync  | 100000 |     41,222.3 us |       802.47 us |       858.64 us |     40,990.1 us |    538.4615 |  384.6154 | 153.8462 |     4518038 B |
| ListReplySync            | 100000 |     42,054.4 us |       693.26 us |       648.48 us |     42,036.1 us |    500.0000 |  333.3333 | 166.6667 |     4517321 B |
| ListReplyAsync           | 100000 |     42,101.5 us |       455.26 us |       425.85 us |     42,068.7 us |    500.0000 |  333.3333 | 166.6667 |     4519551 B |
| EnumerableReply          | 100000 |     43,533.1 us |       862.83 us |     1,393.30 us |     43,040.8 us |    454.5455 |  272.7273 |  90.9091 |     4512921 B |
| ArrayReplyValueTaskAsync | 100000 |     54,169.7 us |     1,070.68 us |     2,258.43 us |     53,841.4 us |   1250.0000 |  750.0000 | 375.0000 |     9059973 B |
| ArrayReplyAsync          | 100000 |     54,172.8 us |       994.80 us |     1,868.48 us |     54,071.0 us |   1250.0000 |  875.0000 | 375.0000 |     9059478 B |
| AsyncEnumerableReply     | 100000 |    187,201.0 us |    11,170.10 us |    32,935.28 us |    175,253.9 us |   3000.0000 |         - |        - |    14440949 B |
| SingleAsyncTask          | 10000  |  1,752,645.0 us |    41,205.98 us |   114,181.56 us |  1,724,654.9 us |  14000.0000 |         - |        - |    67796536 B |
| SingleAsyncValueTask     | 10000  |  1,828,393.9 us |    36,180.90 us |    87,380.94 us |  1,802,766.6 us |  14000.0000 |         - |        - |    67785168 B |
| Single                   | 10000  |  2,005,063.7 us |    39,411.77 us |    75,933.09 us |  1,991,484.4 us |  14000.0000 |         - |        - |    66684640 B |
| SingleAsyncValueTask     | 100000 | 20,227,699.4 us |   816,513.77 us | 2,276,114.03 us | 19,416,722.8 us | 145000.0000 | 3000.0000 |        - |   678001768 B |
| SingleAsyncTask          | 100000 | 22,139,506.4 us | 1,060,706.45 us | 3,094,130.50 us | 21,846,278.6 us | 145000.0000 | 3000.0000 |        - |   678168704 B |
| Single                   | 100000 | 26,282,441.6 us | 1,733,005.64 us | 5,109,806.24 us | 24,815,868.5 us | 143000.0000 | 2000.0000 |        - |   666834752 B |
