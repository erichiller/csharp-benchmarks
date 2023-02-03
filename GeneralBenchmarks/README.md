
# Results

| Method               | Mean [ms] | Error [ms] | StdDev [ms] | Ratio |     Gen0 | Allocated [B] | Alloc Ratio |
|----------------------|----------:|-----------:|------------:|------:|---------:|--------------:|------------:|
| ParseManualAll       |  4.109 ms |  0.0104 ms |   0.0092 ms |  0.51 | 234.3750 |     1120007 B |        0.24 |
| ParseIntManualV2     |  5.310 ms |  0.0183 ms |   0.0171 ms |  0.66 | 609.3750 |     2880007 B |        0.62 |
| ParseIntManual       |  6.977 ms |  0.0347 ms |   0.0324 ms |  0.87 | 609.3750 |     2880007 B |        0.62 |
| ParseLongManualV2    |  7.116 ms |  0.0478 ms |   0.0447 ms |  0.89 | 781.2500 |     3680007 B |        0.79 |
| ParseDecimalManual   |  7.805 ms |  0.0149 ms |   0.0132 ms |  0.98 | 812.5000 |     3840015 B |        0.83 |
| NumberInterfaceParse |  7.995 ms |  0.0599 ms |   0.0531 ms |  1.00 | 984.3750 |     4640015 B |        1.00 |
| ParseLongManual      |  9.606 ms |  0.0152 ms |   0.0127 ms |  1.20 | 781.2500 |     3680015 B |        0.79 |
