``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.202
  [Host]     : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.4 (7.0.423.11508), X64 RyuJIT AVX2


```
|                Method | CompressionLevel | CompressionMethod |    Mean [ms] | Error [ms] | StdDev [ms] |  Median [ms] | Allocated [B] |
|---------------------- |----------------- |------------------ |-------------:|-----------:|------------:|-------------:|--------------:|
| CompressFileToArchive |          Fastest |            Brotli |     11.03 ms |   0.216 ms |    0.289 ms |     10.93 ms |        5893 B |
| CompressFileToArchive |          Fastest |           Deflate |     29.97 ms |   0.318 ms |    0.557 ms |     29.83 ms |        6010 B |
| CompressFileToArchive |          Optimal |            Brotli |     31.77 ms |   0.301 ms |    0.281 ms |     31.73 ms |        5980 B |
| CompressFileToArchive |          Fastest |               Zip |     36.21 ms |   0.261 ms |    0.232 ms |     36.18 ms |        7093 B |
| CompressFileToArchive |          Fastest |              Gzip |     36.28 ms |   0.241 ms |    0.225 ms |     36.21 ms |        6093 B |
| CompressFileToArchive |          Optimal |           Deflate |     65.10 ms |   1.298 ms |    2.470 ms |     63.73 ms |        6101 B |
| CompressFileToArchive |          Optimal |              Gzip |     70.37 ms |   0.361 ms |    0.320 ms |     70.22 ms |        6193 B |
| CompressFileToArchive |          Optimal |               Zip |     70.58 ms |   0.342 ms |    0.303 ms |     70.60 ms |        7193 B |
| CompressFileToArchive |     SmallestSize |           Deflate |    124.04 ms |   1.517 ms |    1.267 ms |    124.48 ms |        6381 B |
| CompressFileToArchive |     SmallestSize |              Gzip |    130.18 ms |   1.742 ms |    1.629 ms |    129.70 ms |        6474 B |
| CompressFileToArchive |     SmallestSize |               Zip |    130.79 ms |   0.773 ms |    0.723 ms |    130.77 ms |        7482 B |
| CompressFileToArchive |     SmallestSize |            Brotli | 17,552.17 ms |  25.259 ms |   23.628 ms | 17,547.91 ms |        7760 B |
