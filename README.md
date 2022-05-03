# Use

Run all:
```powershell
sudo dotnet run -c RELEASE
```

## Commandline Arguments

### Any Categories
Run any tests that have any of the properties provided in the argument `anyCategories`
```
sudo dotnet run -c RELEASE -- --anyCategories Copy
```

### Filter

```powershell
sudo dotnet run -c RELEASE -- --filter="*SystemTextJsonDeserializationBasic*"
```

## Results

### SQL

#### SQL Inserts
| Method                                                                     | ObjectsPerSave | SaveIterations |    Mean [ms] |  Error [ms] | StdDev [ms] |  Median [ms] |      Gen 0 |      Gen 1 |     Gen 2 | Allocated [B] |
|----------------------------------------------------------------------------|----------------|----------------|-------------:|------------:|------------:|-------------:|-----------:|-----------:|----------:|--------------:|
| NpgSqlInsert_SingularCommand_TypedValue_Prepared                           | 1              | 10             |     7.389 ms |   0.3061 ms |   0.8978 ms |     7.422 ms |          - |          - |         - |       5,327 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared_Async | 1              | 10             |     7.607 ms |   0.2594 ms |   0.7483 ms |     7.572 ms |          - |          - |         - |      17,381 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared       | 1              | 10             |     7.614 ms |   0.2180 ms |   0.6324 ms |     7.609 ms |          - |          - |         - |       5,887 B |
| NpgsqlInsert_Batched_Boxed_Value                                           | 1              | 10             |     7.860 ms |   0.2671 ms |   0.7834 ms |     7.839 ms |          - |          - |         - |      17,555 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue                | 1              | 10             |     8.220 ms |   0.3287 ms |   0.9587 ms |     8.249 ms |          - |          - |         - |      18,116 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value                              | 1              | 10             |     8.283 ms |   0.2834 ms |   0.8221 ms |     8.245 ms |          - |          - |         - |      17,555 B |
| NpgSqlInsert_SingularCommand_TypedValue                                    | 1              | 10             |     8.487 ms |   0.3335 ms |   0.9782 ms |     8.488 ms |          - |          - |         - |      17,948 B |
| NpgsqlCopy                                                                 | 2              | 10             |     8.488 ms |   0.3149 ms |   0.8985 ms |     8.385 ms |          - |          - |         - |      18,653 B |
| NpgsqlCopy                                                                 | 10             | 10             |     8.493 ms |   0.3423 ms |   0.9931 ms |     8.380 ms |          - |          - |         - |      31,448 B |
| NpgsqlCopyAsync                                                            | 2              | 10             |     8.511 ms |   0.4514 ms |   1.2804 ms |     8.399 ms |          - |          - |         - |      32,567 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value                              | 2              | 10             |     8.513 ms |   0.2883 ms |   0.8317 ms |     8.467 ms |          - |          - |         - |      29,176 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue                        | 1              | 10             |     8.522 ms |   0.3300 ms |   0.9520 ms |     8.435 ms |          - |          - |         - |      17,555 B |
| NpgsqlCopyToPartitionTable                                                 | 2              | 10             |     8.622 ms |   0.3767 ms |   1.0989 ms |     8.471 ms |          - |          - |         - |      18,956 B |
| NpgsqlCopyToPartitionTable                                                 | 10             | 10             |     8.633 ms |   0.4077 ms |   1.1827 ms |     8.622 ms |          - |          - |         - |      31,786 B |
| NpgsqlInsert_Batched_TypedValue                                            | 2              | 10             |     8.714 ms |   0.2948 ms |   0.8553 ms |     8.771 ms |          - |          - |         - |      28,839 B |
| NpgsqlCopyAsync                                                            | 10             | 10             |     8.784 ms |   0.3637 ms |   1.0495 ms |     8.683 ms |     7.8125 |          - |         - |      45,433 B |
| NpgsqlInsert_Batched_Boxed_Value                                           | 2              | 10             |     8.830 ms |   0.2743 ms |   0.8001 ms |     8.864 ms |          - |          - |         - |      29,156 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue                        | 2              | 10             |     8.866 ms |   0.3034 ms |   0.8753 ms |     8.942 ms |          - |          - |         - |      29,164 B |
| NpgsqlInsert_Batched_TypedValue                                            | 1              | 10             |     8.884 ms |   0.3914 ms |   1.1417 ms |     8.913 ms |          - |          - |         - |      17,399 B |
| NpgsqlCopy                                                                 | 1              | 10             |     9.077 ms |   0.4424 ms |   1.2764 ms |     8.920 ms |          - |          - |         - |      16,988 B |
| NpgsqlCopyAsync                                                            | 1              | 10             |     9.491 ms |   0.6669 ms |   1.9558 ms |     8.711 ms |          - |          - |         - |      31,078 B |
| NpgsqlCopyToPartitionTable                                                 | 1              | 10             |     9.673 ms |   0.5675 ms |   1.6464 ms |     9.653 ms |          - |          - |         - |      17,385 B |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking                           | 2              | 10             |     9.787 ms |   0.4636 ms |   1.3226 ms |     9.707 ms |    46.8750 |          - |         - |     245,237 B |
| EfCoreInsert                                                               | 1              | 10             |     9.933 ms |   0.4946 ms |   1.4190 ms |     9.874 ms |    39.0625 |          - |         - |     215,468 B |
| EfCoreInsert                                                               | 2              | 10             |    10.320 ms |   0.6146 ms |   1.7534 ms |     9.983 ms |    62.5000 |          - |         - |     336,312 B |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking                           | 1              | 10             |    10.476 ms |   0.7894 ms |   2.3028 ms |     9.846 ms |    31.2500 |          - |         - |     169,061 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value                              | 10             | 10             |    11.432 ms |   0.5864 ms |   1.7106 ms |    11.615 ms |    15.6250 |          - |         - |     123,736 B |
| NpgsqlCopyToPartitionTable                                                 | 100            | 10             |    11.825 ms |   0.6932 ms |   2.0220 ms |    11.457 ms |    31.2500 |          - |         - |     175,842 B |
| NpgsqlInsert_Batched_Boxed_Value                                           | 10             | 10             |    12.099 ms |   0.4814 ms |   1.3811 ms |    12.002 ms |    15.6250 |          - |         - |     123,736 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue                        | 10             | 10             |    12.434 ms |   0.5515 ms |   1.6261 ms |    12.431 ms |    15.6250 |          - |         - |     123,733 B |
| NpgsqlCopy                                                                 | 100            | 10             |    12.541 ms |   0.5640 ms |   1.6182 ms |    12.360 ms |    31.2500 |          - |         - |     175,466 B |
| NpgsqlInsert_Batched_TypedValue                                            | 10             | 10             |    12.745 ms |   0.4876 ms |   1.4069 ms |    12.610 ms |    15.6250 |          - |         - |     122,136 B |
| NpgsqlCopyAsync                                                            | 100            | 10             |    13.212 ms |   0.6042 ms |   1.7529 ms |    13.273 ms |    31.2500 |          - |         - |     189,425 B |
| NpgSqlInsert_SingularCommand_TypedValue_Prepared                           | 2              | 10             |    14.093 ms |   0.5125 ms |   1.4868 ms |    13.875 ms |          - |          - |         - |       8,768 B |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking                           | 10             | 10             |    14.119 ms |   1.3719 ms |   3.9802 ms |    13.029 ms |   156.2500 |    31.2500 |         - |     854,218 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared       | 2              | 10             |    14.753 ms |   0.4039 ms |   1.1782 ms |    14.612 ms |          - |          - |         - |       9,900 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared_Async | 2              | 10             |    14.909 ms |   0.4430 ms |   1.2568 ms |    14.879 ms |          - |          - |         - |      32,240 B |
| NpgSqlInsert_SingularCommand_TypedValue                                    | 2              | 10             |    15.985 ms |   0.5169 ms |   1.5078 ms |    15.896 ms |          - |          - |         - |      35,573 B |
| EfCoreInsert                                                               | 10             | 10             |    16.264 ms |   0.8828 ms |   2.5187 ms |    16.154 ms |   250.0000 |    62.5000 |         - |   1,301,135 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue                | 2              | 10             |    16.410 ms |   0.5310 ms |   1.5321 ms |    16.597 ms |          - |          - |         - |      35,870 B |
| NpgsqlCopyWithWork                                                         | 2              | 10             |    22.587 ms |   1.1090 ms |   3.2700 ms |    22.349 ms |          - |          - |         - |      18,799 B |
| NpgsqlCopyWithWorkAsync                                                    | 2              | 10             |    22.608 ms |   0.5456 ms |   1.5829 ms |    22.554 ms |          - |          - |         - |      32,942 B |
| NpgsqlCopyWithWorkAsync                                                    | 10             | 10             |    22.608 ms |   0.5147 ms |   1.5094 ms |    22.583 ms |          - |          - |         - |      45,609 B |
| NpgsqlCopyWithWorkAsyncConfigureAwaitFalse                                 | 10             | 10             |    23.212 ms |   0.7548 ms |   2.2019 ms |    22.914 ms |          - |          - |         - |      45,665 B |
| NpgsqlCopyWithWork                                                         | 10             | 10             |    23.295 ms |   1.0845 ms |   3.1636 ms |    22.958 ms |          - |          - |         - |      31,622 B |
| NpgsqlCopyWithWorkAsyncConfigureAwaitFalse                                 | 2              | 10             |    23.424 ms |   0.7140 ms |   2.1053 ms |    23.305 ms |          - |          - |         - |      32,878 B |
| NpgsqlCopyWithWorkAsyncConfigureAwaitFalse                                 | 1              | 10             |    24.123 ms |   0.7089 ms |   2.0678 ms |    23.942 ms |          - |          - |         - |      31,363 B |
| NpgsqlCopyWithWorkAsync                                                    | 1              | 10             |    24.183 ms |   0.7393 ms |   2.1682 ms |    23.925 ms |          - |          - |         - |      31,355 B |
| NpgsqlCopyWithWorkAsync                                                    | 100            | 10             |    25.228 ms |   0.7817 ms |   2.2177 ms |    25.214 ms |    31.2500 |          - |         - |     189,586 B |
| NpgsqlCopyWithWorkAsyncConfigureAwaitFalse                                 | 100            | 10             |    25.234 ms |   1.0066 ms |   2.9204 ms |    24.442 ms |          - |          - |         - |     189,838 B |
| NpgsqlCopyWithWork                                                         | 1              | 10             |    25.257 ms |   1.4324 ms |   4.1783 ms |    25.391 ms |          - |          - |         - |      17,211 B |
| NpgsqlCopyWithWork                                                         | 100            | 10             |    25.653 ms |   1.6145 ms |   4.7097 ms |    24.583 ms |          - |          - |         - |     175,651 B |
| NpgsqlInsert_Batched_Boxed_Value                                           | 100            | 10             |    27.672 ms |   0.5437 ms |   1.0983 ms |    27.369 ms |   250.0000 |    62.5000 |         - |   1,192,568 B |
| NpgsqlInsert_Batched_TypedValue                                            | 100            | 10             |    27.709 ms |   0.5485 ms |   1.3558 ms |    27.196 ms |   230.7692 |    76.9231 |         - |   1,176,658 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value                              | 100            | 10             |    28.009 ms |   0.5511 ms |   1.2440 ms |    27.804 ms |   250.0000 |    62.5000 |         - |   1,192,568 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue                        | 100            | 10             |    28.756 ms |   0.6222 ms |   1.7752 ms |    28.220 ms |   250.0000 |    62.5000 |         - |   1,192,562 B |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking                           | 100            | 10             |    52.924 ms |   2.4648 ms |   7.0721 ms |    51.759 ms |  1272.7273 |   636.3636 |         - |   7,679,636 B |
| EfCoreInsert                                                               | 100            | 10             |    57.632 ms |   2.0989 ms |   6.1556 ms |    56.418 ms |  1888.8889 |   555.5556 |         - |  12,126,790 B |
| NpgSqlInsert_SingularCommand_TypedValue_Prepared                           | 10             | 10             |    70.377 ms |   2.4523 ms |   7.1534 ms |    69.439 ms |          - |          - |         - |      36,398 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared       | 10             | 10             |    71.967 ms |   2.4015 ms |   7.0053 ms |    71.591 ms |          - |          - |         - |      41,987 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared_Async | 10             | 10             |    73.697 ms |   3.1551 ms |   9.2533 ms |    73.615 ms |          - |          - |         - |     150,576 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared       | 1              | 100            |    75.151 ms |   2.7974 ms |   8.0712 ms |    74.618 ms |          - |          - |         - |      41,957 B |
| NpgSqlInsert_SingularCommand_TypedValue_Prepared                           | 1              | 100            |    76.903 ms |   2.9750 ms |   8.5358 ms |    76.921 ms |          - |          - |         - |      36,383 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared_Async | 1              | 100            |    77.455 ms |   3.8609 ms |  11.3234 ms |    74.846 ms |          - |          - |         - |     150,792 B |
| NpgSqlInsert_SingularCommand_TypedValue                                    | 10             | 10             |    79.085 ms |   3.3097 ms |   9.5491 ms |    78.419 ms |          - |          - |         - |     176,528 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue                | 10             | 10             |    80.709 ms |   3.3258 ms |   9.8062 ms |    79.486 ms |          - |          - |         - |     178,097 B |
| NpgsqlCopy                                                                 | 1              | 100            |    80.829 ms |   4.0552 ms |  11.8293 ms |    80.851 ms |          - |          - |         - |     168,211 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue                        | 1              | 100            |    81.071 ms |   3.5500 ms |  10.1283 ms |    79.863 ms |          - |          - |         - |     172,562 B |
| NpgsqlInsert_Batched_TypedValue                                            | 1              | 100            |    81.926 ms |   3.7293 ms |  10.8195 ms |    80.950 ms |          - |          - |         - |     170,928 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value                              | 1              | 100            |    82.569 ms |   3.2659 ms |   9.6297 ms |    81.477 ms |          - |          - |         - |     172,528 B |
| NpgsqlCopyAsync                                                            | 1              | 100            |    82.621 ms |   4.4623 ms |  12.8748 ms |    80.862 ms |          - |          - |         - |     303,965 B |
| NpgsqlInsert_Batched_Boxed_Value                                           | 1              | 100            |    84.430 ms |   3.7234 ms |  10.9787 ms |    83.940 ms |          - |          - |         - |     172,504 B |
| NpgsqlCopyToPartitionTable                                                 | 10             | 100            |    84.444 ms |   3.3412 ms |   9.5325 ms |    84.733 ms |          - |          - |         - |     315,991 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue                | 1              | 100            |    84.691 ms |   4.1951 ms |  12.2374 ms |    82.207 ms |          - |          - |         - |     178,097 B |
| NpgsqlCopyToPartitionTable                                                 | 2              | 100            |    85.341 ms |   4.6991 ms |  13.7817 ms |    83.596 ms |          - |          - |         - |     187,933 B |
| NpgSqlInsert_SingularCommand_TypedValue                                    | 1              | 100            |    86.282 ms |   3.8741 ms |  11.3009 ms |    86.690 ms |          - |          - |         - |     176,509 B |
| NpgsqlCopyAsync                                                            | 10             | 100            |    87.022 ms |   4.3142 ms |  12.4473 ms |    84.884 ms |          - |          - |         - |     450,018 B |
| NpgsqlCopyToPartitionTable                                                 | 1              | 100            |    88.900 ms |   4.5950 ms |  13.4039 ms |    87.653 ms |          - |          - |         - |     171,806 B |
| NpgsqlCopy                                                                 | 2              | 100            |    89.231 ms |   5.2517 ms |  15.1523 ms |    89.327 ms |          - |          - |         - |     184,658 B |
| NpgsqlInsert_Batched_Boxed_Value                                           | 2              | 100            |    91.654 ms |   3.4918 ms |  10.1858 ms |    89.455 ms |          - |          - |         - |     288,528 B |
| NpgsqlInsert_Batched_TypedValue                                            | 2              | 100            |    92.092 ms |   4.4397 ms |  13.0209 ms |    89.911 ms |          - |          - |         - |     285,362 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue                        | 2              | 100            |    92.487 ms |   4.5390 ms |  13.3121 ms |    90.349 ms |          - |          - |         - |     288,562 B |
| NpgsqlCopyAsync                                                            | 2              | 100            |    92.570 ms |   7.4119 ms |  21.8542 ms |    84.823 ms |          - |          - |         - |     319,798 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value                              | 2              | 100            |    93.614 ms |   4.2055 ms |  12.2010 ms |    91.807 ms |          - |          - |         - |     288,528 B |
| NpgsqlCopy                                                                 | 10             | 100            |    94.887 ms |   5.3361 ms |  15.4809 ms |    94.829 ms |          - |          - |         - |     312,895 B |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking                           | 2              | 100            |    96.493 ms |   6.7871 ms |  19.4734 ms |    94.703 ms |   333.3333 |          - |         - |   2,002,695 B |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking                           | 1              | 100            |   101.094 ms |   7.9073 ms |  22.8145 ms |    99.576 ms |   166.6667 |          - |         - |   1,246,628 B |
| EfCoreInsert                                                               | 2              | 100            |   113.414 ms |   9.7419 ms |  28.2631 ms |   105.983 ms |  2000.0000 |          - |         - |  10,190,424 B |
| EfCoreInsert                                                               | 1              | 100            |   121.109 ms |   9.7366 ms |  28.5557 ms |   114.052 ms |  1000.0000 |   166.6667 |         - |   5,351,041 B |
| NpgsqlCopyToPartitionTable                                                 | 100            | 100            |   127.854 ms |   7.5400 ms |  21.5120 ms |   125.006 ms |   250.0000 |          - |         - |   1,756,354 B |
| NpgsqlCopyAsync                                                            | 100            | 100            |   128.236 ms |   7.5311 ms |  21.8492 ms |   125.134 ms |   400.0000 |          - |         - |   1,890,426 B |
| NpgsqlInsert_Batched_TypedValue                                            | 10             | 100            |   134.447 ms |   8.0417 ms |  23.4579 ms |   133.638 ms |   250.0000 |          - |         - |   1,218,212 B |
| NpgsqlCopy                                                                 | 100            | 100            |   136.450 ms |   9.8470 ms |  28.5680 ms |   132.015 ms |   250.0000 |          - |         - |   1,753,522 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value                              | 10             | 100            |   136.510 ms |   7.7405 ms |  22.5794 ms |   132.508 ms |   200.0000 |          - |         - |   1,234,168 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue                        | 10             | 100            |   138.078 ms |   7.7998 ms |  22.8755 ms |   136.154 ms |   250.0000 |          - |         - |   1,234,212 B |
| NpgsqlInsert_Batched_Boxed_Value                                           | 10             | 100            |   138.723 ms |   7.9501 ms |  23.0648 ms |   136.624 ms |   250.0000 |          - |         - |   1,234,212 B |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking                           | 10             | 100            |   139.149 ms |  11.4348 ms |  32.6242 ms |   135.089 ms |  1400.0000 |   600.0000 |         - |   8,034,198 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared       | 2              | 100            |   144.550 ms |   5.4019 ms |  15.8427 ms |   143.770 ms |          - |          - |         - |      82,148 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared_Async | 2              | 100            |   151.648 ms |   5.3719 ms |  15.7548 ms |   151.185 ms |          - |          - |         - |     298,876 B |
| NpgSqlInsert_SingularCommand_TypedValue_Prepared                           | 2              | 100            |   151.798 ms |   5.5228 ms |  16.1974 ms |   150.853 ms |          - |          - |         - |      70,932 B |
| NpgSqlInsert_SingularCommand_TypedValue                                    | 2              | 100            |   160.169 ms |   6.7772 ms |  19.8763 ms |   159.982 ms |          - |          - |         - |     352,696 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue                | 2              | 100            |   164.546 ms |   6.6332 ms |  19.4540 ms |   162.702 ms |          - |          - |         - |     355,812 B |
| NpgsqlCopyWithWorkAsyncConfigureAwaitFalse                                 | 1              | 100            |   214.436 ms |   4.7337 ms |  13.8083 ms |   214.094 ms |          - |          - |         - |     307,456 B |
| NpgsqlCopyWithWorkAsync                                                    | 1              | 100            |   214.573 ms |   5.2939 ms |  14.8445 ms |   213.600 ms |          - |          - |         - |     305,640 B |
| NpgsqlCopyWithWorkAsyncConfigureAwaitFalse                                 | 2              | 100            |   215.386 ms |   5.6785 ms |  16.3837 ms |   213.066 ms |          - |          - |         - |     321,648 B |
| NpgsqlCopyWithWorkAsync                                                    | 2              | 100            |   219.073 ms |   5.4352 ms |  16.0259 ms |   218.970 ms |          - |          - |         - |     322,845 B |
| NpgsqlCopyWithWorkAsync                                                    | 10             | 100            |   221.900 ms |   5.7014 ms |  16.8106 ms |   221.149 ms |          - |          - |         - |     451,147 B |
| NpgsqlCopyWithWorkAsyncConfigureAwaitFalse                                 | 10             | 100            |   223.887 ms |   6.6759 ms |  19.2616 ms |   224.266 ms |          - |          - |         - |     451,155 B |
| NpgsqlCopyWithWork                                                         | 10             | 100            |   227.737 ms |   9.6472 ms |  28.4449 ms |   225.869 ms |          - |          - |         - |     314,061 B |
| NpgsqlCopyWithWork                                                         | 2              | 100            |   228.556 ms |  11.4511 ms |  33.2219 ms |   224.152 ms |          - |          - |         - |     185,877 B |
| NpgsqlCopyWithWork                                                         | 1              | 100            |   229.591 ms |   9.8735 ms |  28.8014 ms |   226.429 ms |          - |          - |         - |     169,816 B |
| EfCoreInsert                                                               | 10             | 100            |   250.450 ms |  19.9844 ms |  57.6594 ms |   236.789 ms | 10000.0000 |  1000.0000 |         - |  48,867,088 B |
| NpgsqlCopyWithWorkAsync                                                    | 100            | 100            |   252.574 ms |   8.0473 ms |  22.6976 ms |   249.936 ms |   333.3333 |          - |         - |   1,891,448 B |
| NpgsqlCopyWithWorkAsyncConfigureAwaitFalse                                 | 100            | 100            |   252.948 ms |   7.0243 ms |  20.0406 ms |   248.620 ms |   333.3333 |          - |         - |   1,890,251 B |
| NpgsqlCopyWithWork                                                         | 100            | 100            |   265.488 ms |  15.2679 ms |  45.0178 ms |   264.743 ms |   333.3333 |          - |         - |   1,753,816 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_NpgsqlValue                        | 100            | 100            |   268.199 ms |   3.4410 ms |   2.6865 ms |   267.712 ms |  2500.0000 |   500.0000 |         - |  11,925,568 B |
| NpgsqlInsert_Batched_Boxed_Value                                           | 100            | 100            |   270.015 ms |   4.3197 ms |   5.9128 ms |   268.141 ms |  2500.0000 |   500.0000 |         - |  11,923,972 B |
| NpgsqlInsert_Batched_Boxed_NpgsqlDbType_Value                              | 100            | 100            |   270.315 ms |   4.7210 ms |   5.4367 ms |   269.446 ms |  2500.0000 |   500.0000 |         - |  11,923,972 B |
| NpgsqlInsert_Batched_TypedValue                                            | 100            | 100            |   273.341 ms |   4.8191 ms |   3.7624 ms |   273.563 ms |  2500.0000 |   500.0000 |         - |  11,765,604 B |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking                           | 100            | 100            |   532.815 ms |  28.2682 ms |  82.0113 ms |   513.209 ms | 12000.0000 |  3000.0000 |         - |  75,483,952 B |
| NpgSqlInsert_SingularCommand_TypedValue_Prepared                           | 100            | 10             |   713.710 ms |  25.5601 ms |  74.1545 ms |   715.708 ms |          - |          - |         - |     346,888 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared_Async | 100            | 10             |   738.062 ms |  20.9958 ms |  61.2456 ms |   735.745 ms |          - |          - |         - |   1,483,816 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared       | 100            | 10             |   741.285 ms |  22.4201 ms |  64.6871 ms |   741.322 ms |          - |          - |         - |     402,904 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared       | 10             | 100            |   744.936 ms |  21.6972 ms |  62.9474 ms |   732.383 ms |          - |          - |         - |     402,720 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared_Async | 10             | 100            |   748.416 ms |  31.8767 ms |  92.4801 ms |   739.582 ms |          - |          - |         - |   1,483,632 B |
| NpgSqlInsert_SingularCommand_TypedValue_Prepared                           | 10             | 100            |   755.461 ms |  24.8008 ms |  71.9515 ms |   747.696 ms |          - |          - |         - |     346,888 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue                | 100            | 10             |   774.169 ms |  20.4970 ms |  58.8098 ms |   777.870 ms |          - |          - |         - |   1,777,368 B |
| NpgSqlInsert_SingularCommand_TypedValue                                    | 100            | 10             |   807.361 ms |  24.3752 ms |  69.5438 ms |   802.450 ms |          - |          - |         - |   1,761,184 B |
| NpgSqlInsert_SingularCommand_TypedValue                                    | 10             | 100            |   828.554 ms |  30.1801 ms |  87.5578 ms |   824.009 ms |          - |          - |         - |   1,761,368 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue                | 10             | 100            |   839.155 ms |  35.2622 ms | 103.4180 ms |   828.578 ms |          - |          - |         - |   1,777,368 B |
| EfCoreInsert                                                               | 100            | 100            | 1,056.022 ms |  20.4512 ms |  18.1295 ms | 1,050.361 ms | 89000.0000 | 25000.0000 | 1000.0000 | 483,597,072 B |
| NpgSqlInsert_SingularCommand_TypedValue_Prepared                           | 100            | 100            | 7,235.320 ms | 142.7902 ms | 242.4685 ms | 7,240.602 ms |          - |          - |         - |   3,442,368 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared       | 100            | 100            | 7,290.868 ms | 145.3492 ms | 300.1714 ms | 7,242.689 ms |          - |          - |         - |   4,001,832 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared_Async | 100            | 100            | 7,472.041 ms | 148.5431 ms | 335.2866 ms | 7,474.613 ms |  3000.0000 |          - |         - |  14,798,648 B |
| NpgSqlInsert_SingularCommand_TypedValue                                    | 100            | 100            | 7,880.655 ms | 156.2698 ms | 277.7694 ms | 7,916.975 ms |  3000.0000 |          - |         - |  17,601,032 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue                | 100            | 100            | 8,002.440 ms | 159.5381 ms | 356.8298 ms | 8,048.477 ms |  3000.0000 |          - |         - |  17,760,848 B |


##### With more complex `ComplexTestObject`

With more columns, `COPY` take a clear lead.

| Method                                                               | ObjectsPerSave | SaveIterations |  Mean [ms] | Error [ms] | StdDev [ms] | Median [ms] |      Gen 0 |     Gen 1 | Allocated [B] |
|----------------------------------------------------------------------|----------------|----------------|-----------:|-----------:|------------:|------------:|-----------:|----------:|--------------:|
| NpgsqlCopy                                                           | 2              | 10             |   5.979 ms |  0.1189 ms |   0.3112 ms |    5.961 ms |     7.8125 |         - |      39,648 B |
| NpgsqlCopy                                                           | 10             | 10             |   6.607 ms |  0.1401 ms |   0.4066 ms |    6.522 ms |     7.8125 |         - |      65,947 B |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking                     | 2              | 10             |   8.307 ms |  0.2130 ms |   0.6042 ms |    8.108 ms |    62.5000 |         - |     357,288 B |
| EfCoreInsert                                                         | 2              | 10             |   8.762 ms |  0.2378 ms |   0.6861 ms |    8.580 ms |    93.7500 |   15.6250 |     488,013 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared | 2              | 10             |  10.449 ms |  0.2485 ms |   0.7208 ms |   10.363 ms |          - |         - |      16,788 B |
| NpgSqlInsert_SingularCommand_Typed_NpgsqlDbType_NpgsqlValue          | 2              | 10             |  10.827 ms |  0.3179 ms |   0.8595 ms |   10.563 ms |          - |         - |      66,718 B |
| NpgSqlInsert_SingularCommand_TypedValue_Prepared                     | 2              | 10             |  10.965 ms |  0.2547 ms |   0.7390 ms |   10.984 ms |          - |         - |      13,340 B |
| NpgSqlInsert_SingularCommand_Typed_NpgsqlDbType_TypedValue           | 2              | 10             |  11.379 ms |  0.2447 ms |   0.7138 ms |   11.360 ms |          - |         - |      63,244 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue          | 2              | 10             |  11.386 ms |  0.2592 ms |   0.7436 ms |   11.355 ms |          - |         - |      64,681 B |
| NpgSqlInsert_SingularCommand_TypedValue                              | 2              | 10             |  11.691 ms |  0.2690 ms |   0.7888 ms |   11.735 ms |          - |         - |      63,236 B |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking                     | 10             | 10             |  12.791 ms |  0.2402 ms |   0.5802 ms |   12.776 ms |   296.8750 |   93.7500 |   1,398,330 B |
| EfCoreInsert                                                         | 10             | 10             |  14.690 ms |  0.2912 ms |   0.7824 ms |   14.621 ms |   421.8750 |  140.6250 |   2,043,450 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared | 10             | 10             |  51.719 ms |  1.1283 ms |   3.2913 ms |   51.210 ms |          - |         - |      71,274 B |
| NpgSqlInsert_SingularCommand_TypedValue_Prepared                     | 10             | 10             |  52.791 ms |  1.3933 ms |   3.9977 ms |   52.458 ms |          - |         - |      54,397 B |
| NpgSqlInsert_SingularCommand_TypedValue                              | 10             | 10             |  56.948 ms |  1.6744 ms |   4.7771 ms |   56.296 ms |          - |         - |     314,863 B |
| NpgSqlInsert_SingularCommand_Typed_NpgsqlDbType_TypedValue           | 10             | 10             |  57.287 ms |  1.5379 ms |   4.4372 ms |   56.967 ms |          - |         - |     314,852 B |
| NpgSqlInsert_SingularCommand_Typed_NpgsqlDbType_NpgsqlValue          | 10             | 10             |  57.724 ms |  1.7283 ms |   5.0688 ms |   57.163 ms |          - |         - |     331,672 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue          | 10             | 10             |  57.882 ms |  1.5635 ms |   4.5608 ms |   56.830 ms |          - |         - |     322,006 B |
| NpgsqlCopy                                                           | 2              | 100            |  59.376 ms |  1.4250 ms |   4.0886 ms |   58.194 ms |          - |         - |     394,348 B |
| NpgsqlCopy                                                           | 10             | 100            |  64.760 ms |  1.5839 ms |   4.5953 ms |   64.288 ms |   125.0000 |         - |     658,007 B |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking                     | 2              | 100            |  77.698 ms |  1.5477 ms |   3.8254 ms |   77.009 ms |   571.4286 |  142.8571 |   3,109,686 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared | 2              | 100            | 100.856 ms |  2.0459 ms |   5.7370 ms |  100.128 ms |          - |         - |     139,357 B |
| NpgSqlInsert_SingularCommand_TypedValue_Prepared                     | 2              | 100            | 101.461 ms |  1.9608 ms |   5.4335 ms |  100.726 ms |          - |         - |     105,698 B |
| EfCoreInsert                                                         | 2              | 100            | 107.150 ms |  2.1401 ms |   5.9656 ms |  106.530 ms |  3000.0000 |  250.0000 |  14,931,716 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue          | 2              | 100            | 110.293 ms |  2.4828 ms |   7.2029 ms |  110.000 ms |          - |         - |     643,762 B |
| NpgSqlInsert_SingularCommand_TypedValue                              | 2              | 100            | 111.390 ms |  2.2511 ms |   6.5664 ms |  111.212 ms |          - |         - |     629,325 B |
| NpgSqlInsert_SingularCommand_Typed_NpgsqlDbType_NpgsqlValue          | 2              | 100            | 112.477 ms |  2.8215 ms |   8.0954 ms |  112.921 ms |          - |         - |     662,894 B |
| NpgSqlInsert_SingularCommand_Typed_NpgsqlDbType_TypedValue           | 2              | 100            | 114.620 ms |  3.7625 ms |  11.0347 ms |  112.522 ms |          - |         - |     629,496 B |
| EfCoreInsert_NoAutoDetectChanges_NoQueryTracking                     | 10             | 100            | 123.373 ms |  2.1820 ms |   5.2699 ms |  122.051 ms |  2250.0000 |  500.0000 |  13,413,148 B |
| EfCoreInsert                                                         | 10             | 100            | 245.048 ms |  4.8435 ms |   9.3318 ms |  242.024 ms | 14500.0000 | 1000.0000 |  72,427,752 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue_Prepared | 10             | 100            | 510.520 ms |  9.5923 ms |  24.7608 ms |  513.620 ms |          - |         - |     684,200 B |
| NpgSqlInsert_SingularCommand_TypedValue_Prepared                     | 10             | 100            | 516.918 ms | 10.2037 ms |  29.1116 ms |  515.552 ms |          - |         - |     516,104 B |
| NpgSqlInsert_SingularCommand_TypedValue                              | 10             | 100            | 570.162 ms | 14.6747 ms |  42.5739 ms |  560.647 ms |          - |         - |   3,145,368 B |
| NpgSqlInsert_SingularCommand_Typed_NpgsqlDbType_NpgsqlValue          | 10             | 100            | 571.702 ms | 15.2041 ms |  44.1099 ms |  576.605 ms |          - |         - |   3,313,368 B |
| NpgSqlInsert_SingularCommand_Boxed_NpgsqlDbType_NpgsqlValue          | 10             | 100            | 580.033 ms | 16.6669 ms |  48.8812 ms |  574.946 ms |          - |         - |   3,217,368 B |
| NpgSqlInsert_SingularCommand_Typed_NpgsqlDbType_TypedValue           | 10             | 100            | 584.305 ms | 14.5471 ms |  41.9716 ms |  584.282 ms |          - |         - |   3,145,368 B |


##### `COPY` methods

| Method                      | ObjectsPerSave | SaveIterations | Mean [ms] | Error [ms] | StdDev [ms] |    Gen 0 | Allocated [B] |
|-----------------------------|----------------|----------------|----------:|-----------:|------------:|---------:|--------------:|
| NpgsqlCopyWithTypesAsString | 2              | 100            |  75.23 ms |   4.911 ms |    14.32 ms |        - |     190,234 B |
| NpgsqlCopy                  | 2              | 100            |  76.10 ms |   5.404 ms |    15.85 ms |        - |     190,281 B |
| NpgsqlCopy                  | 100            | 100            | 107.51 ms |   6.757 ms |    19.71 ms | 333.3333 |   1,759,355 B |
| NpgsqlCopyWithTypesAsString | 100            | 100            | 109.43 ms |   9.134 ms |    26.93 ms | 250.0000 |   1,759,418 B |

***Non-significant difference between using `NpgsqlDbType` and `string` to provide `Copy()` with database type information.***


##### `NpgsqlConnection` time

| Method                           | Iterations | Mean [ms] | Error [ms] | StdDev [ms] | Allocated [B] |
|----------------------------------|------------|----------:|-----------:|------------:|--------------:|
| NpgsqlConnectionPerIterationTime | 1000       |  56.21 ms |   1.089 ms |    2.098 ms |   1,012,584 B |
| NpgsqlConnectionSingleTime       | 1000       |  73.14 ms |   1.397 ms |    1.663 ms |   1,347,200 B |

1 iteration vs 1000 iterations increased the time by about `0.01693ms`.

##### Partition Tables

_See `Results/` folder for details

| Method                            | Iterations | RangeSize |   Mean [ms] | Error [ms] | StdDev [ms] | Allocated [B] |
|-----------------------------------|------------|-----------|------------:|-----------:|------------:|--------------:|
| 'B-Tree index on normal table'    | 200        | 1000      |    46.90 ms |   0.853 ms |    1.891 ms |     258,232 B |
| 'B-Tree index on Partition Table' | 200        | 1000      |    50.96 ms |   0.986 ms |    1.316 ms |     270,842 B |
| 'B-Tree index on normal table'    | 200        | 10000     |   194.49 ms |   3.386 ms |    3.002 ms |     258,968 B |
| 'B-Tree index on Partition Table' | 200        | 10000     |   211.56 ms |   1.157 ms |    0.903 ms |     271,595 B |
| 'BRIN index on Partition Table'   | 200        | 1000      |   417.49 ms |   4.821 ms |    4.509 ms |     271,768 B |
| 'BRIN index on Partition Table'   | 200        | 10000     |   431.61 ms |   3.445 ms |    3.054 ms |     271,768 B |
| 'B-Tree index on normal table'    | 200        | 100000    | 1,542.42 ms |   6.848 ms |    6.405 ms |     262,168 B |
| 'B-Tree index on Partition Table' | 200        | 100000    | 1,703.65 ms |   1.790 ms |    1.495 ms |     271,768 B |
| 'BRIN index on Partition Table'   | 200        | 100000    | 4,082.63 ms |  64.758 ms |   60.575 ms |     272,760 B |


###### Server `shared_memory` set to `128MB`

TODO


### JSON

#### Serialize

| Method                                                 | Iterations |       Mean |    Error |   StdDev |    Gen 0 |    Gen 1 |   Gen 2 | Allocated |
|--------------------------------------------------------|------------|-----------:|---------:|---------:|---------:|---------:|--------:|----------:|
| NewtonsoftJson_Serialize_Scalars_Float                 | 1000       | 2,465.6 us | 11.18 us | 10.46 us | 375.0000 | 125.0000 |       - |  1,828 KB |
| NewtonsoftJson_Serialize_Scalars_Decimal               | 1000       | 2,518.7 us | 48.87 us | 63.54 us | 375.0000 | 125.0000 |       - |  1,852 KB |
| NewtonsoftJson_Serialize_Scalars_NodaTime              | 1000       | 2,498.1 us |  9.23 us | 14.10 us | 464.8438 | 171.8750 |       - |  2,406 KB |
| NewtonsoftJson_Serialize_NestedObjects_NodaTime        | 1000       | 3,960.3 us | 23.17 us | 21.68 us | 492.1875 | 242.1875 |       - |  2,695 KB |
| NewtonsoftJson_Serialize_NestedObjects_Arrays_NodaTime | 1000       | 6,448.7 us | 50.39 us | 47.13 us | 539.0625 | 218.7500 | 23.4375 |  3,304 KB |
| SystemTextJson_Serialize_Scalars_Float                 | 1000       |   454.8 us |  4.28 us |  4.00 us |  62.0117 |  20.5078 |       - |    297 KB |
| SystemTextJson_Serialize_Scalars_Decimal               | 1000       |   348.0 us |  1.12 us |  1.04 us |  61.5234 |  20.5078 |       - |    313 KB |
| SystemTextJson_Serialize_Scalars_NodaTime              | 1000       |   608.9 us |  6.82 us |  6.38 us | 137.6953 |  21.4844 |       - |    656 KB |
| SystemTextJson_Serialize_NestedObjects_NodaTime        | 1000       |   920.5 us |  3.91 us |  3.46 us | 187.5000 |  93.7500 |       - |  1,109 KB |
| SystemTextJson_Serialize_NestedObjects_Arrays_NodaTime | 1000       | 1,590.2 us | 18.29 us | 16.22 us | 265.6250 | 132.8125 |       - |  1,632 KB |



#### Deserialize

##### `System.Text.Json` Deserialization Tests

Note that `record` types **are much slower than `class` types.
Also, `[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]` only `JsonSerializerContext` **do not improve performance at all**.

| Method                                              | Iterations |  Mean [us] | Error [us] | StdDev [us] |    Gen 0 |    Gen 1 | Allocated [B] |
|-----------------------------------------------------|------------|-----------:|-----------:|------------:|---------:|---------:|--------------:|
| Scalars_Decimal_Class                               | 1000       |   549.6 us |    2.59 us |     2.16 us |  21.4844 |   6.8359 |     104,025 B |
| Scalars_Float_Class                                 | 1000       |   556.3 us |    3.22 us |     2.86 us |  18.5547 |   5.8594 |      88,025 B |
| Scalars_Float_Class_SourceGen                       | 1000       |   556.3 us |    5.08 us |     4.75 us |  18.5547 |   5.8594 |      88,025 B |
| Scalars_Float_Class_Fields_SourceGen                | 1000       |   565.9 us |    2.49 us |     2.08 us |  18.5547 |   5.8594 |      88,025 B |
| Scalars_Decimal_Class_SourceGen                     | 1000       |   568.8 us |    3.26 us |     3.05 us |  21.4844 |   6.8359 |     104,025 B |
| Scalars_Decimal_Record                              | 1000       |   688.6 us |    5.67 us |     5.31 us |  66.4063 |  21.4844 |     320,025 B |
| Scalars_Float_Record                                | 1000       |   691.8 us |    4.21 us |     3.73 us |  60.5469 |  19.5313 |     288,025 B |
| Scalars_NodaTime_Class                              | 1000       |   696.9 us |    1.64 us |     1.54 us |  41.9922 |  13.6719 |     200,025 B |
| Scalars_NodaTime_Class_SourceGen                    | 1000       |   713.6 us |    9.58 us |     8.96 us |  41.9922 |  13.6719 |     200,025 B |
| Scalars_NodaTime_Class_ConverterAttribute           | 1000       |   714.8 us |    6.14 us |     5.45 us |  41.9922 |  13.6719 |     200,025 B |
| Scalars_NodaTime_ConverterAttribute_Class_SourceGen | 1000       |   728.4 us |    7.44 us |     6.96 us |  41.9922 |  13.6719 |     200,025 B |
| Scalars_NodaTime_Record                             | 1000       |   856.9 us |   10.38 us |     9.20 us |  87.8906 |  29.2969 |     416,025 B |
| NestedObjects_NodaTime_Class                        | 1000       | 1,277.3 us |    2.15 us |     2.01 us | 117.1875 |  52.7344 |     696,026 B |
| NestedObjects_NodaTime_Class_SourceGen              | 1000       | 1,305.0 us |   16.88 us |    15.79 us | 117.1875 |  48.8281 |     696,026 B |
| NestedObjects_NodaTime_Record                       | 1000       | 1,568.7 us |   18.70 us |    16.57 us | 181.6406 |  80.0781 |   1,032,026 B |
| NestedObjects_Arrays_NodaTime_Class                 | 1000       | 2,317.0 us |   21.91 us |    17.10 us | 160.1563 |  78.1250 |   1,016,027 B |
| NestedObjects_Arrays_NodaTime_Class_SourceGen       | 1000       | 2,327.5 us |   25.29 us |    22.42 us | 160.1563 |  78.1250 |   1,016,027 B |
| NestedObjects_Arrays_NodaTime_Record                | 1000       | 2,939.7 us |   58.74 us |    69.92 us | 234.3750 | 117.1875 |   1,480,027 B |


This table shows that it is not just `init`  properties, or `record` types that slow deserialization down, it is the use of *Constructors* that causes **a significant increase in memory utilization**,
and takes about 25% longer. Furthermore, Source Generators do not help.

| Method                                               | Iterations |  Mean [us] | Error [us] | StdDev [us] |   Gen 0 |   Gen 1 | Allocated [B] |
|------------------------------------------------------|------------|-----------:|-----------:|------------:|--------:|--------:|--------------:|
| Scalars_Float_Record_Init_No_Constructor             | 1000       |   555.0 us |    3.55 us |     3.15 us | 18.5547 |  5.8594 |      88,025 B |
| Scalars_Float_Class_Set_NoConstructor                | 1000       |   558.3 us |    1.97 us |     1.84 us | 18.5547 |  5.8594 |      88,025 B |
| Scalars_Float_Class_Init_NoConstructor               | 1000       |   561.5 us |    1.93 us |     1.71 us | 18.5547 |  5.8594 |      88,025 B |
| Scalars_Float_Class_Set_NoConstructor_SourceGen      | 1000       |   561.6 us |    3.38 us |     2.99 us | 18.5547 |  5.8594 |      88,025 B |
| Scalars_Float_Class_Set_NoConstructor_ReturningCount | 1000       |   563.8 us |    2.52 us |     2.35 us | 16.6016 |       - |      80,001 B |
| Scalars_Float_Class_Fields_NoConstructor_SourceGen   | 1000       |   574.1 us |    1.24 us |     1.10 us | 18.5547 |  5.8594 |      88,025 B |
| Scalars_Float_Class_Init_Constructor                 | 1000       |   708.4 us |    3.35 us |     3.13 us | 60.5469 | 19.5313 |     288,025 B |
| Scalars_Float_Record_Init_Constructor                | 1000       |   711.4 us |    4.61 us |     4.32 us | 60.5469 | 19.5313 |     288,025 B |
| Scalars_Float_Class_Set_Constructor_SourceGen        | 1000       |   747.5 us |    1.10 us |     0.92 us | 62.5000 | 20.5078 |     296,025 B |
| Scalars_Float_Class_Init_PartialConstructor          | 1000       |   969.8 us |    4.03 us |     3.36 us | 62.5000 | 19.5313 |     296,026 B |
| Scalars_Float_Class_Set_PartialConstructor_SourceGen | 1000       | 1,010.1 us |    4.40 us |     3.67 us | 56.6406 | 17.5781 |     272,027 B |


##### LevelOne


| Method                                                                               | Iterations | LevelOneJsonFile | WithSourceGenerationContext |      Mean |     Error |    StdDev |    Median |     Gen 0 |    Gen 1 | Allocated |
|--------------------------------------------------------------------------------------|------------|------------------|-----------------------------|----------:|----------:|----------:|----------:|----------:|---------:|----------:|
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                         | 1000       | Single           | True                        |  7.090 ms | 0.1403 ms | 0.2635 ms |  6.960 ms |  632.8125 |  15.6250 |      3 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                         | 1000       | Single           | False                       |  7.209 ms | 0.0503 ms | 0.0446 ms |  7.185 ms |  632.8125 |  15.6250 |      3 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne_SourceGenWithoutOptions | 1000       | Single           | True                        |  7.278 ms | 0.0257 ms | 0.0240 ms |  7.268 ms |  632.8125 |  15.6250 |      3 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne_SourceGenWithoutOptions | 1000       | Single           | False                       |  7.407 ms | 0.0401 ms | 0.0375 ms |  7.414 ms |  632.8125 |  15.6250 |      3 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne_SourceGenWithoutOptions | 1000       | Multiple         | True                        | 49.775 ms | 0.1507 ms | 0.1336 ms | 49.722 ms | 4100.0000 | 200.0000 |     18 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                         | 1000       | Multiple         | False                       | 50.411 ms | 0.6371 ms | 0.5960 ms | 50.180 ms | 4090.9091 | 181.8182 |     18 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                         | 1000       | Multiple         | True                        | 50.712 ms | 0.2609 ms | 0.2441 ms | 50.655 ms | 4100.0000 | 200.0000 |     18 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne_SourceGenWithoutOptions | 1000       | Multiple         | False                       | 51.048 ms | 0.6626 ms | 0.5874 ms | 50.848 ms | 4100.0000 | 200.0000 |     18 MB |



#### Deserialize with Read-ahead for Type determination

##### Multiple
| Method                                                                            | Iterations |      Mean |    Error |   StdDev |      Gen 0 |     Gen 1 | Allocated |
|-----------------------------------------------------------------------------------|------------|----------:|---------:|---------:|-----------:|----------:|----------:|
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       |  29.72 ms | 0.196 ms | 0.174 ms |  4093.7500 |  187.5000 |     18 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       |  43.35 ms | 0.325 ms | 0.304 ms |  3250.0000 |  166.6667 |     15 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       |  49.88 ms | 0.302 ms | 0.282 ms |  3818.1818 |  181.8182 |     17 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       |  56.94 ms | 1.036 ms | 0.969 ms | 14444.4444 |  666.6667 |     65 MB |
| NewtonsoftJson_Deserialize_ReadAhead_LevelOne                                     | 1000       | 171.16 ms | 1.438 ms | 1.345 ms | 24000.0000 | 2000.0000 |    112 MB |

##### Single
| Method                                                                            | Iterations | LevelOneJsonFile |      Mean |     Error |    StdDev |     Gen 0 |    Gen 1 | Allocated |
|-----------------------------------------------------------------------------------|------------|------------------|----------:|----------:|----------:|----------:|---------:|----------:|
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       | Single           |  4.815 ms | 0.0790 ms | 0.0739 ms | 1335.9375 |  23.4375 |      6 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       | Single           |  6.324 ms | 0.0436 ms | 0.0341 ms |  484.3750 |  15.6250 |      2 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       | Single           |  7.143 ms | 0.0531 ms | 0.0443 ms | 2171.8750 |  85.9375 |     10 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       | Single           |  7.403 ms | 0.0151 ms | 0.0126 ms |  601.5625 |  15.6250 |      3 MB |
| NewtonsoftJson_Deserialize_ReadAhead_LevelOne                                     | 1000       | Single           | 26.456 ms | 0.0490 ms | 0.0459 ms | 4750.0000 | 156.2500 |     21 MB |




| Method                                                                            | Iterations | LevelOneJsonFile |      Mean |     Error |    StdDev |     Gen 0 |    Gen 1 | Allocated |
|-----------------------------------------------------------------------------------|------------|------------------|----------:|----------:|----------:|----------:|---------:|----------:|
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       | Single           |  5.517 ms | 0.1073 ms | 0.1395 ms | 1375.0000 |  23.4375 |      6 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       | Single           |  6.879 ms | 0.1345 ms | 0.1193 ms |  523.4375 |  15.6250 |      2 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       | Single           |  7.678 ms | 0.0674 ms | 0.0630 ms |  648.4375 |  15.6250 |      3 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       | Single           |  7.817 ms | 0.1426 ms | 0.1334 ms | 2203.1250 |  85.9375 |     10 MB |
| NewtonsoftJson_Deserialize_ReadAhead_LevelOne                                     | 1000       | Single           | 28.415 ms | 0.1826 ms | 0.1618 ms | 4750.0000 | 187.5000 |     21 MB |




| Method                                                                            | Iterations | LevelOneJsonFile | WithSourceGenerationContext |       Mean |     Error |    StdDev |      Gen 0 |     Gen 1 | Allocated |
|-----------------------------------------------------------------------------------|------------|------------------|-----------------------------|-----------:|----------:|----------:|-----------:|----------:|----------:|
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       | Single           | False                       |   5.173 ms | 0.0281 ms | 0.0235 ms |  1375.0000 |   23.4375 |      6 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       | Single           | True                        |   5.253 ms | 0.0958 ms | 0.0896 ms |  1375.0000 |   23.4375 |      6 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       | Single           | True                        |   6.598 ms | 0.0257 ms | 0.0240 ms |   523.4375 |   15.6250 |      2 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       | Single           | False                       |   6.630 ms | 0.0462 ms | 0.0432 ms |   523.4375 |   15.6250 |      2 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       | Single           | False                       |   7.774 ms | 0.1102 ms | 0.1031 ms |  2203.1250 |   85.9375 |     10 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       | Single           | True                        |   7.787 ms | 0.0547 ms | 0.0485 ms |   648.4375 |   15.6250 |      3 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       | Single           | False                       |   7.824 ms | 0.0607 ms | 0.0507 ms |   640.6250 |   15.6250 |      3 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       | Single           | True                        |   7.972 ms | 0.1544 ms | 0.1586 ms |  2203.1250 |   85.9375 |     10 MB |
| NewtonsoftJson_Deserialize_ReadAhead_LevelOne                                     | 1000       | Single           | False                       |  26.348 ms | 0.1745 ms | 0.1632 ms |  4750.0000 |  187.5000 |     21 MB |
| NewtonsoftJson_Deserialize_ReadAhead_LevelOne                                     | 1000       | Single           | True                        |  28.016 ms | 0.2733 ms | 0.2556 ms |  4750.0000 |  187.5000 |     21 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       | Multiple         | True                        |  33.436 ms | 0.1659 ms | 0.1552 ms |  4466.6667 |  200.0000 |     20 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne | 1000       | Multiple         | False                       |  33.844 ms | 0.2259 ms | 0.2113 ms |  4466.6667 |  200.0000 |     20 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       | Multiple         | True                        |  48.750 ms | 0.2033 ms | 0.1697 ms |  3636.3636 |  272.7273 |     16 MB |
| SystemTextJson_JsonDocument_ReadAhead_Deserialize_JsonSerializerSingle_LevelOne   | 1000       | Multiple         | False                       |  48.779 ms | 0.1454 ms | 0.1289 ms |  3636.3636 |  181.8182 |     16 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       | Multiple         | False                       |  55.094 ms | 0.2562 ms | 0.2397 ms |  4300.0000 |  200.0000 |     19 MB |
| SystemTextJson_JsonSerializer_ReadAhead_Deserialize_LevelOne                      | 1000       | Multiple         | True                        |  56.605 ms | 0.1656 ms | 0.1468 ms |  4222.2222 |  111.1111 |     19 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       | Multiple         | False                       |  62.105 ms | 0.3797 ms | 0.3552 ms | 14875.0000 |  750.0000 |     67 MB |
| SystemTextJson_Utf8JsonReader_ReadAhead_Deserialize_JsonSerializerPer_LevelOne    | 1000       | Multiple         | True                        |  62.628 ms | 0.8557 ms | 0.8004 ms | 14888.8889 |  555.5556 |     67 MB |
| NewtonsoftJson_Deserialize_ReadAhead_LevelOne                                     | 1000       | Multiple         | False                       | 174.842 ms | 1.2252 ms | 1.1461 ms | 24000.0000 | 2000.0000 |    112 MB |

***