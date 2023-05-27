``` ini

BenchmarkDotNet=v0.13.4, OS=ubuntu 22.04
Intel Core i5-8600K CPU 3.60GHz (Coffee Lake), 1 CPU, 6 logical and 6 physical cores
.NET SDK=7.0.102
  [Host]     : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2
  DefaultJob : .NET 7.0.2 (7.0.222.60605), X64 RyuJIT AVX2


```
|                                     Method | Mean [ms] | Error [ms] | StdDev [ms] | Ratio | RatioSD |      Gen0 | Allocated [B] | Alloc Ratio |
|------------------------------------------- |----------:|-----------:|------------:|------:|--------:|----------:|--------------:|------------:|
|                                  ParseOnly |  42.03 ms |   0.422 ms |    0.395 ms |  1.00 |    0.00 | 2666.6667 |    12800078 B |        1.00 |
|                     ParseWithDateTimeTicks |  46.12 ms |   0.163 ms |    0.153 ms |  1.10 |    0.01 | 2636.3636 |    12800085 B |        1.00 |
|                       ParseWithDateTimeUtc |  46.37 ms |   0.080 ms |    0.075 ms |  1.10 |    0.01 | 2636.3636 |    12800085 B |        1.00 |
|  ParseWithNodaTimeZonedClockInstantToTicks |  49.40 ms |   0.383 ms |    0.359 ms |  1.18 |    0.02 | 2636.3636 |    12800085 B |        1.00 |
|        ParseWithNodaTimeSystemClockInstant |  49.60 ms |   0.132 ms |    0.124 ms |  1.18 |    0.01 | 3000.0000 |    14400085 B |        1.12 |
|         ParseWithNodaTimeZonedClockInstant |  49.78 ms |   0.345 ms |    0.322 ms |  1.18 |    0.01 | 3000.0000 |    14400085 B |        1.12 |
| ParseWithNodaTimeSystemClockInstantToTicks |  49.86 ms |   0.297 ms |    0.278 ms |  1.19 |    0.01 | 2636.3636 |    12800085 B |        1.00 |
|   ParseWithNodaTimeZonedClockZonedDateTime |  58.24 ms |   0.110 ms |    0.103 ms |  1.39 |    0.01 | 3333.3333 |    16000104 B |        1.25 |
|                          ParseWithDateTime |  62.64 ms |   0.120 ms |    0.101 ms |  1.49 |    0.01 | 2625.0000 |    12800117 B |        1.00 |
