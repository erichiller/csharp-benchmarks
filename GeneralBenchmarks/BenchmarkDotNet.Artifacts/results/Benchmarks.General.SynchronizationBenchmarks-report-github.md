```

BenchmarkDotNet v0.13.10, Ubuntu 22.04.3 LTS (Jammy Jellyfish)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK 7.0.403
  [Host]     : .NET 7.0.13 (7.0.1323.51816), X64 RyuJIT AVX2
  Job-WFLGAF : .NET 7.0.13 (7.0.1323.51816), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method        | TaskCount | Mean     | Error    | StdDev   | Ratio | 
|-------------- |---------- |---------:|---------:|---------:|------:|-
| **Lock**          | **1**         | **17.13 ms** | **0.032 ms** | **0.027 ms** |  **1.00** | 
| SemaphoreSlim | 1         | 43.61 ms | 0.054 ms | 0.043 ms |  2.55 | 
|               |           |          |          |          |       | 
| **Lock**          | **2**         | **23.73 ms** | **0.470 ms** | **0.811 ms** |  **1.00** | 
| SemaphoreSlim | 2         | 74.56 ms | 1.489 ms | 3.109 ms |  3.13 | 
|               |           |          |          |          |       | 
| **Lock**          | **3**         | **23.18 ms** | **0.925 ms** | **2.578 ms** |  **1.00** | 
| SemaphoreSlim | 3         | 60.36 ms | 1.198 ms | 3.007 ms |  2.65 | 
|               |           |          |          |          |       | 
| **Lock**          | **4**         | **23.42 ms** | **0.561 ms** | **1.653 ms** |  **1.00** | 
| SemaphoreSlim | 4         | 55.51 ms | 1.073 ms | 1.671 ms |  2.42 | 
|               |           |          |          |          |       | 
| **Lock**          | **5**         | **22.73 ms** | **0.462 ms** | **1.332 ms** |  **1.00** | 
| SemaphoreSlim | 5         | 50.81 ms | 1.007 ms | 1.711 ms |  2.22 | 
