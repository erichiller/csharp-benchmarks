# gRPC Benchmarks


## Results

### `ListIAsyncEnumerableComparison`


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