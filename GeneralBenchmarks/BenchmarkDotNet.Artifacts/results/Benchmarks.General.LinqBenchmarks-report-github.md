```

BenchmarkDotNet v0.13.12, Ubuntu 22.04.3 LTS (Jammy Jellyfish)
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK 8.0.101
  [Host]     : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2
  Job-GEZFYM : .NET 8.0.1 (8.0.123.58001), X64 RyuJIT AVX2

InvocationCount=1  UnrollFactor=1  

```
| Method         | NumElements | Mean [ms]    | Error [ms] | StdDev [ms] | Gen0        | Gen1      | Allocated [B] |
|--------------- |------------ |-------------:|-----------:|------------:|------------:|----------:|--------------:|
| StringsToArray | 10          |     8.690 ms |  0.0891 ms |   0.0790 ms |   2000.0000 |         - |    10400736 B |
| StringsToList  | 10          |    10.809 ms |  0.0931 ms |   0.0777 ms |   2000.0000 |         - |    13600736 B |
| StringsToArray | 100         |    60.198 ms |  0.2241 ms |   0.1986 ms |  17000.0000 |         - |    82400736 B |
| StringsToList  | 100         |    67.683 ms |  0.3924 ms |   0.3064 ms |  18000.0000 |         - |    85600736 B |
| StringsToArray | 1000        | 1,235.750 ms | 14.0660 ms |  12.4692 ms | 646000.0000 | 2000.0000 |  3042400736 B |
| StringsToList  | 1000        | 1,515.336 ms | 24.9726 ms |  23.3594 ms | 647000.0000 | 1000.0000 |  3045600736 B |
