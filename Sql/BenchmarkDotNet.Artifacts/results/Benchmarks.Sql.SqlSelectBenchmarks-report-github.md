``` ini

BenchmarkDotNet=v0.13.1, OS=ubuntu 21.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=6.0.202
  [Host] : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT
  Select : .NET 6.0.4 (6.0.422.16404), X64 RyuJIT

Job=Select  EnvironmentVariables=Autosummarize=on  

```
|                            Method | Iterations | RangeSize |   Mean [ms] | Error [ms] | StdDev [ms] | Allocated [B] |
|---------------------------------- |----------- |---------- |------------:|-----------:|------------:|--------------:|
|    &#39;B-Tree index on normal table&#39; |        200 |      1000 |    46.08 ms |   0.897 ms |    2.331 ms |     258,968 B |
| &#39;B-Tree index on Partition Table&#39; |        200 |      1000 |    51.95 ms |   0.667 ms |    0.624 ms |     271,707 B |
|    &#39;B-Tree index on normal table&#39; |        200 |     10000 |   193.55 ms |   2.135 ms |    1.782 ms |     258,968 B |
| &#39;B-Tree index on Partition Table&#39; |        200 |     10000 |   213.97 ms |   3.103 ms |    2.902 ms |     271,237 B |
|   &#39;BRIN index on Partition Table&#39; |        200 |      1000 |   422.75 ms |   4.082 ms |    3.619 ms |     273,464 B |
|   &#39;BRIN index on Partition Table&#39; |        200 |     10000 |   435.28 ms |   4.493 ms |    3.983 ms |     275,232 B |
|    &#39;B-Tree index on normal table&#39; |        200 |    100000 | 1,548.69 ms |   6.436 ms |    6.020 ms |     262,168 B |
| &#39;B-Tree index on Partition Table&#39; |        200 |    100000 | 1,712.14 ms |   4.137 ms |    3.870 ms |     271,768 B |
|   &#39;BRIN index on Partition Table&#39; |        200 |    100000 | 4,051.18 ms |  11.916 ms |   10.563 ms |     271,768 B |
