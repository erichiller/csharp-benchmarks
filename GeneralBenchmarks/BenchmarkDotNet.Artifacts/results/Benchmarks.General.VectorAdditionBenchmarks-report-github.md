```

BenchmarkDotNet v0.13.12, Ubuntu 22.04.4 LTS (Jammy Jellyfish)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK 8.0.200
  [Host]     : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.2 (8.0.224.6711), X64 RyuJIT AVX2


```
| Method                                       | Mean [ms] | Error [ms] | StdDev [ms] | Allocated [B] |
|--------------------------------------------- |----------:|-----------:|------------:|--------------:|
| Vector256_Byte_Ushort_Addition_3             |  6.827 ms |  0.0025 ms |   0.0019 ms |           6 B |
| Vector256_Byte_Ushort_Addition_2             |  6.859 ms |  0.0352 ms |   0.0329 ms |           6 B |
| Vector256_Byte_Ushort_Addition_1             |  8.394 ms |  0.0729 ms |   0.0609 ms |          12 B |
| Vector256_Byte_Ushort_Addition_4_FrequentSum |  8.731 ms |  0.0030 ms |   0.0026 ms |          12 B |
| Avx2_Ushort_Verified_NoThrow_Addition        |  8.745 ms |  0.0031 ms |   0.0026 ms |          12 B |
| Manual_Byte_Addition                         | 50.822 ms |  0.0199 ms |   0.0166 ms |          74 B |
