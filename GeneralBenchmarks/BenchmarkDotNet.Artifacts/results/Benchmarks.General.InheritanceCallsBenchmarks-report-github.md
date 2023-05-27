``` ini

BenchmarkDotNet=v0.13.5, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.203
  [Host]     : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.5 (7.0.523.17405), X64 RyuJIT AVX2


```
|                                          Method | Iterations | Mean [ms] | Error [ms] | StdDev [ms] | Ratio | RatioSD |     Gen0 | Allocated [B] | Alloc Ratio |
|------------------------------------------------ |----------- |----------:|-----------:|------------:|------:|--------:|---------:|--------------:|------------:|
|                         StandAloneStatic_Direct |     100000 |  1.335 ms |  0.0070 ms |   0.0065 ms |  0.99 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|                        ImplementingClass_Direct |     100000 |  1.337 ms |  0.0094 ms |   0.0088 ms |  0.99 |    0.02 | 679.6875 |     3199682 B |        1.00 |
|                        BaseStatic_FromInheritor |     100000 |  1.337 ms |  0.0037 ms |   0.0033 ms |  0.99 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|                                BaseClass_Direct |     100000 |  1.338 ms |  0.0049 ms |   0.0046 ms |  0.99 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|                      InterfaceVirtual_Overriden |     100000 |  1.339 ms |  0.0080 ms |   0.0071 ms |  0.99 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|                          InterfaceStatic_Direct |     100000 |  1.340 ms |  0.0101 ms |   0.0094 ms |  0.99 |    0.02 | 679.6875 |     3199682 B |        1.00 |
|            InterfaceStaticVirtual_DirectOnClass |     100000 |  1.344 ms |  0.0076 ms |   0.0068 ms |  0.99 |    0.02 | 679.6875 |     3199682 B |        1.00 |
|                          InheritingClass_AsBase |     100000 |  1.344 ms |  0.0064 ms |   0.0060 ms |  1.00 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|                   ImplementingClass_AsInterface |     100000 |  1.344 ms |  0.0074 ms |   0.0065 ms |  0.99 |    0.02 | 679.6875 |     3199682 B |        1.00 |
|                    InterfaceVirtual_AsInterface |     100000 |  1.348 ms |  0.0079 ms |   0.0074 ms |  1.00 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|                          StandAloneClass_Direct |     100000 |  1.351 ms |  0.0182 ms |   0.0170 ms |  1.00 |    0.00 | 679.6875 |     3199682 B |        1.00 |
|                          InheritingClass_Direct |     100000 |  1.352 ms |  0.0102 ms |   0.0091 ms |  1.00 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|           InterfaceStaticAbstract_DirectOnClass |     100000 |  1.360 ms |  0.0114 ms |   0.0107 ms |  1.01 |    0.01 | 679.6875 |     3199682 B |        1.00 |
|                               BaseStatic_Direct |     100000 |  1.385 ms |  0.0276 ms |   0.0307 ms |  1.03 |    0.02 | 679.6875 |     3199682 B |        1.00 |
|              InterfaceStaticVirtual_AsInterface |     100000 |  1.551 ms |  0.0071 ms |   0.0067 ms |  1.15 |    0.01 | 679.6875 |     3199682 B |        1.00 |
| InterfaceStaticVirtualNoImplementor_OnInterface |     100000 |  1.566 ms |  0.0065 ms |   0.0057 ms |  1.16 |    0.02 | 679.6875 |     3199682 B |        1.00 |
|           InterfaceStaticAbstract_OnImplementor |     100000 |  1.573 ms |  0.0075 ms |   0.0070 ms |  1.16 |    0.02 | 679.6875 |     3199682 B |        1.00 |
