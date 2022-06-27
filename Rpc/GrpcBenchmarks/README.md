# gRPC Benchmarks


## Results

### `ListIAsyncEnumerableComparison`

Takeaways:
- Tt appears that `EnumerableReply` becomes *much* more efficient on really large transfers. `ListReply` stays competitive throughout
- `ValueTask` has sizeable advantages, particularly on small-sized replies


| Method               | Count |  Mean [us] | Error [us] | StdDev [us] | Median [us] |   Gen 0 | Allocated [B] |
|----------------------|-------|-----------:|-----------:|------------:|------------:|--------:|--------------:|
| Single               | 1     |   211.0 us |   10.93 us |    31.90 us |    198.6 us |  1.7090 |       8,215 B |
| ArrayReplyAsync      | 1     |   229.6 us |    7.40 us |    21.60 us |    226.0 us |  1.9531 |      11,162 B |
| ListReplyAsync       | 1     |   239.7 us |    9.11 us |    25.99 us |    234.8 us |  2.4414 |      12,671 B |
| AsyncEnumerableReply | 1     |   242.4 us |   11.32 us |    32.47 us |    231.5 us |  1.9531 |       9,879 B |
| ArrayReplyAsync      | 10    |   245.8 us |    8.00 us |    22.84 us |    243.6 us |  2.4414 |      11,696 B |
| ListReplyAsync       | 10    |   247.3 us |    6.81 us |    19.33 us |    242.5 us |  2.4414 |      13,140 B |
| SingleAsyncValueTask | 1     |   247.4 us |   16.81 us |    48.22 us |    244.7 us |  1.4648 |       8,576 B |
| EnumerableReply      | 1     |   249.1 us |   15.20 us |    42.38 us |    235.0 us |  1.9531 |       9,613 B |
| EnumerableReply      | 10    |   274.0 us |   19.29 us |    55.04 us |    253.1 us |  1.9531 |      10,087 B |
| SingleAsyncTask      | 1     |   295.7 us |   30.73 us |    90.60 us |    264.2 us |  1.4648 |       8,517 B |
| AsyncEnumerableReply | 10    |   334.2 us |   24.88 us |    71.79 us |    309.5 us |  2.4414 |      11,548 B |
| SingleAsyncValueTask | 10    | 2,054.5 us |  102.76 us |   296.48 us |  1,983.4 us | 15.6250 |      81,377 B |
| SingleAsyncTask      | 10    | 2,242.7 us |  177.22 us |   516.96 us |  2,102.5 us | 15.6250 |      81,213 B |
| Single               | 10    | 2,677.9 us |  251.28 us |   736.96 us |  2,421.8 us | 15.6250 |      80,296 B |



| Method                   | Count   |      Mean [us] |   Error [us] |  StdDev [us] |    Median [us] |      Gen 0 |     Gen 1 |   Gen 2 | Allocated [B] |
|--------------------------|---------|---------------:|-------------:|-------------:|---------------:|-----------:|----------:|--------:|--------------:|
| ArrayReplyAsync          | 10      |       217.4 us |      4.29 us |      8.38 us |       217.0 us |     2.4414 |         - |       - |      11,755 B |
| ArrayReplyValueTaskAsync | 10      |       217.7 us |      3.91 us |      6.95 us |       217.2 us |     2.4414 |         - |       - |      11,757 B |
| AsyncEnumerableReply     | 1       |       218.1 us |      5.94 us |     17.23 us |       216.9 us |     1.9531 |         - |       - |       9,872 B |
| EnumerableReply          | 10      |       218.2 us |      4.31 us |      9.54 us |       218.7 us |     1.9531 |         - |       - |      10,066 B |
| ListReplyAsync           | 1       |       227.5 us |      4.50 us |     12.47 us |       225.9 us |     2.4414 |         - |       - |      12,707 B |
| ListReplyAsync           | 10      |       227.9 us |      4.52 us |     12.21 us |       225.0 us |     2.4414 |         - |       - |      13,124 B |
| EnumerableReply          | 1       |       236.1 us |     13.85 us |     39.95 us |       220.1 us |     1.9531 |         - |       - |       9,618 B |
| ArrayReplyValueTaskAsync | 1       |       242.3 us |     10.38 us |     29.28 us |       232.7 us |     1.9531 |         - |       - |      11,126 B |
| AsyncEnumerableReply     | 10      |       267.7 us |      5.78 us |     16.67 us |       264.8 us |     2.4414 |         - |       - |      11,669 B |
| ArrayReplyAsync          | 1       |       745.1 us |     44.29 us |    124.93 us |       740.6 us |          - |         - |       - |      38,080 B |
| ArrayReplyValueTaskAsync | 10000   |     7,281.6 us |    223.79 us |    642.11 us |     7,143.3 us |   156.2500 |   46.8750 |       - |     819,321 B |
| ArrayReplyAsync          | 10000   |     7,503.3 us |    209.02 us |    606.42 us |     7,425.3 us |   156.2500 |   46.8750 |       - |     819,112 B |
| ListReplyAsync           | 10000   |     7,531.6 us |    265.52 us |    778.73 us |     7,289.0 us |    78.1250 |   39.0625 | 39.0625 |     516,145 B |
| EnumerableReply          | 10000   |     8,021.1 us |    269.42 us |    794.39 us |     7,843.2 us |    78.1250 |   39.0625 | 39.0625 |     512,967 B |
| AsyncEnumerableReply     | 10000   |    40,069.1 us |    662.78 us |    587.53 us |    40,011.7 us |   461.5385 |         - |       - |   2,467,198 B |
| EnumerableReply          | 100000  |    59,805.6 us |    557.99 us |    685.26 us |    59,818.9 us |          - |         - |       - |   4,517,736 B |
| ListReplyAsync           | 100000  |    59,917.4 us |  1,145.80 us |  1,850.26 us |    59,241.2 us |          - |         - |       - |   4,541,176 B |
| ArrayReplyValueTaskAsync | 100000  |    63,989.4 us |  1,436.37 us |  4,212.63 us |    64,769.8 us |          - |         - |       - |   9,065,512 B |
| ArrayReplyAsync          | 100000  |    64,651.5 us |  1,571.00 us |  4,557.76 us |    66,809.9 us |  1000.0000 |         - |       - |   9,062,544 B |
| AsyncEnumerableReply     | 100000  |   402,521.6 us |  7,854.72 us |  9,045.51 us |   401,779.3 us |  5000.0000 |         - |       - |  25,963,072 B |
| EnumerableReply          | 1000000 |   613,603.5 us |  5,676.07 us |  5,309.40 us |   611,198.1 us |  4000.0000 | 1000.0000 |       - |  40,868,520 B |
| ListReplyAsync           | 1000000 |   617,004.3 us |  7,829.98 us |  7,324.17 us |   616,580.1 us |  4000.0000 | 1000.0000 |       - |  40,874,152 B |
| ArrayReplyValueTaskAsync | 1000000 |   715,743.4 us |  6,033.19 us |  5,348.27 us |   715,781.6 us |  9000.0000 | 2000.0000 |       - |  85,082,680 B |
| ArrayReplyAsync          | 1000000 |   725,062.3 us | 10,742.88 us |  9,523.28 us |   727,284.5 us |  9000.0000 | 3000.0000 |       - |  85,083,176 B |
| AsyncEnumerableReply     | 1000000 | 4,071,709.0 us | 71,112.24 us | 55,519.76 us | 4,067,725.7 us | 48000.0000 |         - |       - | 225,797,912 B |


| Method                  | Count  |   Mean [us] |  Error [us] | StdDev [us] | Median [us] |   Gen 0 |   Gen 1 |   Gen 2 | Allocated [B] |
|-------------------------|--------|------------:|------------:|------------:|------------:|--------:|--------:|--------:|--------------:|
| ListReplySync           | 1      |    238.8 us |    11.65 us |    32.09 us |    228.4 us |  2.4414 |       - |       - |      12,355 B |
| ListReplyValueTaskAsync | 1      |    255.9 us |    14.32 us |    39.68 us |    242.8 us |  2.4414 |       - |       - |      12,684 B |
| ListReplyAsync          | 1      |    815.2 us |    48.81 us |   138.46 us |    791.0 us |       - |       - |       - |      36,744 B |
| ListReplyValueTaskAsync | 10000  |  6,875.2 us |   130.87 us |   270.28 us |  6,813.7 us | 78.1250 | 39.0625 | 39.0625 |     515,915 B |
| ListReplySync           | 10000  |  7,001.3 us |   138.89 us |   310.64 us |  7,002.3 us | 78.1250 | 39.0625 | 39.0625 |     515,417 B |
| ListReplyAsync          | 10000  |  7,063.6 us |   140.29 us |   393.38 us |  7,006.6 us | 78.1250 | 31.2500 | 31.2500 |     515,894 B |
| ListReplySync           | 100000 | 58,343.6 us | 1,137.81 us | 1,837.35 us | 57,819.6 us |       - |       - |       - |   4,542,352 B |
| ListReplyValueTaskAsync | 100000 | 58,541.6 us | 1,152.35 us |   899.68 us | 58,302.1 us |       - |       - |       - |   4,541,352 B |
| ListReplyAsync          | 100000 | 59,455.6 us | 1,168.08 us | 1,518.84 us | 58,964.2 us |       - |       - |       - |   4,539,800 B |
