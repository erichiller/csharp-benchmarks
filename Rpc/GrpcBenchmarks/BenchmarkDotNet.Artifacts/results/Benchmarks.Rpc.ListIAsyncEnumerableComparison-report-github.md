``` ini

BenchmarkDotNet=v0.13.1, OS=ubuntu 21.10
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.301
  [Host]     : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT
  DefaultJob : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT


```
|                  Method |  Count |   Mean [μs] |  Error [μs] | StdDev [μs] | Median [μs] |   Gen 0 |   Gen 1 |   Gen 2 | Allocated [B] |
|------------------------ |------- |------------:|------------:|------------:|------------:|--------:|--------:|--------:|--------------:|
|           ListReplySync |      1 |    238.8 μs |    11.65 μs |    32.09 μs |    228.4 μs |  2.4414 |       - |       - |      12,355 B |
| ListReplyValueTaskAsync |      1 |    255.9 μs |    14.32 μs |    39.68 μs |    242.8 μs |  2.4414 |       - |       - |      12,684 B |
|          ListReplyAsync |      1 |    815.2 μs |    48.81 μs |   138.46 μs |    791.0 μs |       - |       - |       - |      36,744 B |
| ListReplyValueTaskAsync |  10000 |  6,875.2 μs |   130.87 μs |   270.28 μs |  6,813.7 μs | 78.1250 | 39.0625 | 39.0625 |     515,915 B |
|           ListReplySync |  10000 |  7,001.3 μs |   138.89 μs |   310.64 μs |  7,002.3 μs | 78.1250 | 39.0625 | 39.0625 |     515,417 B |
|          ListReplyAsync |  10000 |  7,063.6 μs |   140.29 μs |   393.38 μs |  7,006.6 μs | 78.1250 | 31.2500 | 31.2500 |     515,894 B |
|           ListReplySync | 100000 | 58,343.6 μs | 1,137.81 μs | 1,837.35 μs | 57,819.6 μs |       - |       - |       - |   4,542,352 B |
| ListReplyValueTaskAsync | 100000 | 58,541.6 μs | 1,152.35 μs |   899.68 μs | 58,302.1 μs |       - |       - |       - |   4,541,352 B |
|          ListReplyAsync | 100000 | 59,455.6 μs | 1,168.08 μs | 1,518.84 μs | 58,964.2 μs |       - |       - |       - |   4,539,800 B |
