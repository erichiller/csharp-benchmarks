```

BenchmarkDotNet v0.13.6, Ubuntu 22.04.2 LTS (Jammy Jellyfish)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK 7.0.306
  [Host]     : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.9 (7.0.923.32018), X64 RyuJIT AVX2


```
|                   Method | Count |  Mean [μs] | Error [μs] | StdDev [μs] | Median [μs] |    Gen0 | Allocated [B] |
|------------------------- |------ |-----------:|-----------:|------------:|------------:|--------:|--------------:|
|                   Single |     1 |   192.1 μs |    3.78 μs |     7.01 μs |    191.0 μs |  1.4648 |        6878 B |
|     SingleAsyncValueTask |     1 |   196.5 μs |    3.89 μs |     5.81 μs |    196.1 μs |  1.4648 |        7205 B |
|          EnumerableReply |     1 |   208.7 μs |    4.15 μs |     6.70 μs |    210.0 μs |  1.4648 |        8234 B |
|          EnumerableReply |    10 |   212.7 μs |    4.01 μs |     6.24 μs |    211.9 μs |  1.4648 |        8693 B |
|            ListReplySync |     1 |   212.7 μs |    4.20 μs |     5.60 μs |    211.8 μs |  1.9531 |       10997 B |
|            ListReplySync |    10 |   214.3 μs |    4.28 μs |     5.26 μs |    214.5 μs |  2.4414 |       11442 B |
| ArrayReplyValueTaskAsync |    10 |   226.2 μs |    4.40 μs |     6.31 μs |    227.4 μs |  1.9531 |       10383 B |
|          ArrayReplyAsync |     1 |   227.3 μs |    4.51 μs |    12.03 μs |    225.4 μs |  1.9531 |        9761 B |
|     AsyncEnumerableReply |     1 |   227.3 μs |    4.53 μs |    12.48 μs |    226.9 μs |  1.4648 |        8465 B |
|           ListReplyAsync |    10 |   228.2 μs |    4.49 μs |     7.26 μs |    227.1 μs |  1.9531 |       11778 B |
|          ArrayReplyAsync |    10 |   229.0 μs |    5.15 μs |    14.62 μs |    226.7 μs |  1.9531 |       10369 B |
|  ListReplyValueTaskAsync |     1 |   229.5 μs |    5.10 μs |    13.35 μs |    226.6 μs |  2.4414 |       11349 B |
|           ListReplyAsync |     1 |   241.4 μs |   10.13 μs |    29.40 μs |    229.1 μs |  1.9531 |       11269 B |
| ArrayReplyValueTaskAsync |     1 |   241.9 μs |    7.42 μs |    20.81 μs |    237.1 μs |  1.9531 |        9774 B |
|          SingleAsyncTask |     1 |   242.3 μs |   11.81 μs |    33.11 μs |    232.2 μs |  0.9766 |        7175 B |
|     AsyncEnumerableReply |    10 |   290.3 μs |   23.20 μs |    65.07 μs |    260.4 μs |  1.9531 |        9781 B |
|  ListReplyValueTaskAsync |    10 |   390.3 μs |   36.30 μs |   107.02 μs |    406.1 μs |  2.4414 |       11797 B |
|     SingleAsyncValueTask |    10 | 1,785.9 μs |   35.41 μs |    60.13 μs |  1,770.8 μs | 11.7188 |       68212 B |
|          SingleAsyncTask |    10 | 1,802.7 μs |   34.73 μs |    76.95 μs |  1,785.9 μs | 11.7188 |       68144 B |
|                   Single |    10 | 1,924.0 μs |   35.59 μs |    58.48 μs |  1,916.7 μs | 11.7188 |       66939 B |
