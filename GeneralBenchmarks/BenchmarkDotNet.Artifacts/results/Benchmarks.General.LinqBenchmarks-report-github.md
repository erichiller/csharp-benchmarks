```

BenchmarkDotNet v0.13.12, Ubuntu 22.04.3 LTS (Jammy Jellyfish)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK 8.0.101
  [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2
  Job-QIJQRJ : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method                | NumElements | Mean [ms]   | Error [ms] | StdDev [ms] | Gen0         | Gen1      | Allocated [B] |
|---------------------- |------------ |------------:|-----------:|------------:|-------------:|----------:|--------------:|
| StaticSelectLambda    | 10          |    23.88 ms |   0.279 ms |    0.233 ms |   12000.0000 |         - |    56800736 B |
| NonStaticSelectLambda | 10          |    24.15 ms |   0.368 ms |    0.344 ms |   12000.0000 |         - |    56800736 B |
| NonStaticSelectLambda | 100         |   183.31 ms |   0.755 ms |    0.631 ms |  103000.0000 | 3000.0000 |   488800736 B |
| StaticSelectLambda    | 100         |   183.65 ms |   0.823 ms |    0.729 ms |  103000.0000 | 3000.0000 |   488800736 B |
| StaticSelectLambda    | 1000        | 2,424.12 ms |  43.624 ms |   40.806 ms | 1498000.0000 | 1000.0000 |  7048800736 B |
| NonStaticSelectLambda | 1000        | 2,615.54 ms |  52.296 ms |   66.137 ms | 1498000.0000 | 1000.0000 |  7048800736 B |
