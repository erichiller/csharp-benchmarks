
## Interthread

```
dotnet run -c RELEASE --filter "*WithoutHost*"
```
| Method                                                | MessageCount |      Mean [us] |   Error [us] |   StdDev [us] |      Gen 0 |     Gen 1 |     Gen 2 | Allocated [B] |
|-------------------------------------------------------|--------------|---------------:|-------------:|--------------:|-----------:|----------:|----------:|--------------:|
| Channels_WithoutHost_WriterOnly                       | 20000        |       692.3 us |      6.29 us |       5.58 us |          - |         - |         - |   1,166,416 B |
| BroadcastQueue_WithoutHost_WriterOnly                 | 20000        |     1,077.0 us |      9.15 us |       7.14 us |          - |         - |         - |     906,512 B |
| Channels_WithoutHost_ReadWrite                        | 20000        |     1,543.4 us |     72.86 us |     212.52 us |          - |         - |         - |     649,840 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber    | 20000        |     2,248.4 us |     44.45 us |     101.23 us |          - |         - |         - |     646,832 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers   | 20000        |     6,729.8 us |    342.47 us |     993.58 us |          - |         - |         - |     652,160 B |
| Channels_WithoutHost_WriterOnly                       | 200000       |    11,619.8 us |    230.05 us |     273.86 us |  1000.0000 | 1000.0000 | 1000.0000 |  10,597,872 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers | 20000        |    11,894.6 us |    620.81 us |   1,810.92 us |          - |         - |         - |     662,032 B |
| BroadcastQueue_WithoutHost_WriterOnly                 | 200000       |    13,335.9 us |    261.76 us |     422.69 us |  1000.0000 |         - |         - |   8,502,864 B |
| Channels_WithoutHost_ReadWrite                        | 200000       |    13,644.3 us |    547.45 us |   1,489.39 us |  1000.0000 |         - |         - |   6,467,952 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber    | 200000       |    21,615.8 us |    585.58 us |   1,698.89 us |  1000.0000 |         - |         - |   6,930,448 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers   | 200000       |    65,217.5 us |  2,827.00 us |   8,201.64 us |  1000.0000 |         - |         - |   8,508,288 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers | 200000       |   118,791.1 us |  4,301.95 us |  12,684.40 us |  1000.0000 |         - |         - |   6,673,168 B |
| Channels_WithoutHost_ReadWrite                        | 2000000      |   129,885.9 us |  2,594.98 us |   7,403.62 us | 13000.0000 |         - |         - |  64,265,072 B |
| Channels_WithoutHost_WriterOnly                       | 2000000      |   176,477.5 us |    931.33 us |     871.17 us | 11000.0000 | 5000.0000 | 2000.0000 |  97,559,464 B |
| BroadcastQueue_WithoutHost_WriterOnly                 | 2000000      |   178,454.7 us |  1,529.93 us |   1,431.10 us | 11000.0000 | 4000.0000 | 2000.0000 |  80,785,640 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber    | 2000000      |   212,100.0 us |  3,937.67 us |   5,520.06 us | 13000.0000 |         - |         - |  64,136,048 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers   | 2000000      |   648,877.9 us | 20,404.37 us |  60,162.74 us | 13000.0000 | 2000.0000 |         - |  65,320,960 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers | 2000000      | 1,160,735.2 us | 42,265.90 us | 124,621.96 us | 13000.0000 | 1000.0000 |         - |  64,930,768 B |

| Method                           | MessageCount |    Mean [us] | Error [us] | StdDev [us] |    Gen 0 |  Gen 1 | Allocated [B] |
|----------------------------------|--------------|-------------:|-----------:|------------:|---------:|-------:|--------------:|
| RunChannelsWithoutHostTest       | 10           |     3.580 us |  0.0714 us |   0.0953 us |   0.7782 | 0.0076 |       3,640 B |
| RunBroadcastQueueWithoutHostTest | 10           |     9.343 us |  0.1805 us |   0.4911 us |   1.0376 | 0.0153 |       5,187 B |
| RunChannelsWithoutHostTest       | 10000        |   808.163 us |  8.3469 us |   7.3993 us |  72.2656 | 2.9297 |     338,723 B |
| RunBroadcastQueueWithoutHostTest | 10000        | 3,446.952 us | 49.0321 us |  45.8647 us | 207.0313 | 7.8125 |     973,236 B |



`clear; dotnet run -c RELEASE --filter "*WriterOnlyTest"`:

| Method                                | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |      Gen 0 |     Gen 1 |     Gen 2 | Allocated [B] |
|---------------------------------------|--------------|----------:|-----------:|------------:|-----------:|----------:|----------:|--------------:|
| Channels_WithoutHost_WriterOnly       | 2000000      |  176.1 ms |    0.94 ms |     0.88 ms | 11000.0000 | 5000.0000 | 2000.0000 |  97,559,464 B |
| BroadcastQueue_WithoutHost_WriterOnly | 2000000      |  179.9 ms |    2.43 ms |     2.28 ms | 11000.0000 | 4000.0000 | 2000.0000 |  80,785,616 B |





| Method                                                | MessageCount |      Mean [us] |   Error [us] |  StdDev [us] |      Gen 0 |     Gen 1 |     Gen 2 | Allocated [B] |
|-------------------------------------------------------|--------------|---------------:|-------------:|-------------:|-----------:|----------:|----------:|--------------:|
| Channels_WithoutHost_WriterOnly                       | 20000        |       690.4 us |      8.00 us |      7.09 us |          - |         - |         - |   1,166,416 B |
| BroadcastQueue_WithoutHost_WriterOnly                 | 20000        |     1,012.9 us |      6.71 us |      6.59 us |          - |         - |         - |     906,512 B |
| Channels_WithoutHost_ReadWrite                        | 20000        |     1,407.5 us |     57.72 us |    158.02 us |          - |         - |         - |     649,840 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber    | 20000        |     2,302.7 us |     52.27 us |    150.80 us |          - |         - |         - |     644,624 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers   | 20000        |     7,367.9 us |    376.39 us |  1,085.98 us |          - |         - |         - |     648,192 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers | 20000        |    11,292.3 us |    676.05 us |  1,961.35 us |          - |         - |         - |     948,096 B |
| Channels_WithoutHost_WriterOnly                       | 200000       |    11,639.9 us |    222.27 us |    237.83 us |  1000.0000 | 1000.0000 | 1000.0000 |  10,597,872 B |
| Channels_WithoutHost_ReadWrite                        | 200000       |    13,319.2 us |    328.32 us |    952.52 us |  1000.0000 |         - |         - |   6,534,032 B |
| BroadcastQueue_WithoutHost_WriterOnly                 | 200000       |    13,406.2 us |    210.72 us |    266.50 us |  1000.0000 |         - |         - |   8,502,864 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber    | 200000       |    21,116.6 us |    409.21 us |  1,127.09 us |  1000.0000 |         - |         - |   6,470,064 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers   | 200000       |    66,242.7 us |  1,899.65 us |  5,480.93 us |  1000.0000 |         - |         - |   6,421,536 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers | 200000       |   113,986.3 us |  5,811.62 us | 17,135.68 us |  1000.0000 |         - |         - |   6,507,536 B |
| Channels_WithoutHost_ReadWrite                        | 2000000      |   129,256.1 us |  2,676.33 us |  7,806.97 us | 13000.0000 | 1000.0000 |         - |  66,100,848 B |
| Channels_WithoutHost_WriterOnly                       | 2000000      |   176,470.9 us |    808.58 us |    675.20 us | 11000.0000 | 5000.0000 | 2000.0000 |  97,559,464 B |
| BroadcastQueue_WithoutHost_WriterOnly                 | 2000000      |   177,115.3 us |    611.53 us |    542.11 us | 11000.0000 | 4000.0000 | 2000.0000 |  80,788,936 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber    | 2000000      |   206,770.9 us |  4,129.64 us | 11,715.09 us | 13000.0000 |         - |         - |  64,267,856 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers   | 2000000      |   675,951.4 us | 22,555.60 us | 66,505.69 us | 13000.0000 | 1000.0000 |         - |  64,664,704 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers | 2000000      | 1,306,945.2 us | 25,923.29 us | 66,916.30 us | 13000.0000 | 2000.0000 |         - |  64,799,248 B |



| Method                                                | MessageCount |    Mean [ms] | Error [ms] | StdDev [ms] |  Median [ms] |      Gen 0 |     Gen 1 | Allocated [B] |
|-------------------------------------------------------|--------------|-------------:|-----------:|------------:|-------------:|-----------:|----------:|--------------:|
| Channels_WithoutHost_ReadWrite                        | 20000        |     1.501 ms |  0.0742 ms |   0.2106 ms |     1.441 ms |          - |         - |     774,032 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber    | 20000        |     2.234 ms |  0.0607 ms |   0.1752 ms |     2.249 ms |          - |         - |     660,016 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers   | 20000        |     7.155 ms |  0.3358 ms |   0.9743 ms |     7.133 ms |          - |         - |     910,592 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers | 20000        |    12.195 ms |  0.5824 ms |   1.7173 ms |    12.140 ms |          - |         - |     713,872 B |
| Channels_WithoutHost_ReadWrite                        | 200000       |    13.452 ms |  0.3333 ms |   0.9456 ms |    13.457 ms |  1000.0000 |         - |   6,435,504 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber    | 200000       |    21.383 ms |  0.4225 ms |   0.9451 ms |    21.310 ms |  1000.0000 |         - |   6,930,448 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers   | 200000       |    61.882 ms |  1.6611 ms |   4.7926 ms |    62.379 ms |  1000.0000 | 1000.0000 |   6,455,360 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers | 200000       |   120.414 ms |  3.8508 ms |  11.2331 ms |   121.238 ms |  1000.0000 |         - |   6,465,232 B |
| Channels_WithoutHost_ReadWrite                        | 2000000      |   131.092 ms |  3.0776 ms |   9.0743 ms |   131.300 ms | 13000.0000 | 1000.0000 |  66,100,848 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber    | 2000000      |   205.327 ms |  4.0639 ms |  10.1206 ms |   205.460 ms | 13000.0000 |         - |  64,136,048 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers   | 2000000      |   675.262 ms | 13.4018 ms |  35.3056 ms |   673.718 ms | 13000.0000 | 2000.0000 |  64,467,904 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers | 2000000      | 1,304.218 ms | 25.7333 ms |  59.6409 ms | 1,301.418 ms | 13000.0000 | 2000.0000 |  67,161,680 B |

| Method                                                          | MessageCount |  Mean [ms] | Error [ms] | StdDev [ms] | Median [ms] |      Gen 0 |     Gen 1 | Allocated [B] |
|-----------------------------------------------------------------|--------------|-----------:|-----------:|------------:|------------:|-----------:|----------:|--------------:|
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber              | 20000        |   1.570 ms |  0.0947 ms |   0.2791 ms |    1.643 ms |          - |         - |     646,832 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ReadAllAsync | 20000        |   1.600 ms |  0.0554 ms |   0.1632 ms |    1.621 ms |          - |         - |     776,304 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber              | 200000       |  10.268 ms |  0.4574 ms |   1.2674 ms |    9.803 ms |  1000.0000 |         - |   6,420,016 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ReadAllAsync | 200000       |  15.636 ms |  0.7473 ms |   2.2034 ms |   15.123 ms |  1000.0000 |         - |   7,455,152 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber              | 2000000      |  93.100 ms |  1.8034 ms |   1.5987 ms |   93.389 ms | 13000.0000 | 1000.0000 |  64,070,064 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ReadAllAsync | 2000000      | 164.216 ms | 10.8943 ms |  32.1221 ms |  170.305 ms | 10000.0000 | 4000.0000 |  66,104,176 B |

| Method                                                         | MessageCount |    Mean [ms] | Error [ms] | StdDev [ms] |       Gen 0 |     Gen 1 | Allocated [B] |
|----------------------------------------------------------------|--------------|-------------:|-----------:|------------:|------------:|----------:|--------------:|
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers            | 20000        |     6.321 ms |  0.3677 ms |    1.055 ms |           - |         - |     649,664 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_WriteAsync | 20000        |    11.708 ms |  0.5557 ms |    1.585 ms |   1000.0000 |         - |   5,449,904 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers            | 200000       |    61.812 ms |  2.3264 ms |    6.637 ms |   1000.0000 |         - |   6,425,344 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_WriteAsync | 200000       |   107.269 ms |  2.2832 ms |    6.514 ms |  11000.0000 |         - |  54,446,960 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers            | 2000000      |   620.675 ms | 27.5482 ms |   81.227 ms |  12000.0000 | 3000.0000 |  66,108,288 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_WriteAsync | 2000000      | 1,114.690 ms | 22.1572 ms |   59.524 ms | 114000.0000 | 2000.0000 | 545,059,056 B |


| Method                                                                    | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |     Gen 0 |     Gen 1 | Allocated [B] |
|---------------------------------------------------------------------------|--------------|----------:|-----------:|------------:|----------:|----------:|--------------:|
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ConfigureAwaitFalse | 200000       |  120.7 ms |    2.99 ms |     8.78 ms | 1000.0000 | 1000.0000 |   6,573,072 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers                     | 200000       |  126.9 ms |    3.44 ms |    10.09 ms | 1000.0000 |         - |   6,426,576 B |


| Method                                                                      | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |      Gen 0 |     Gen 1 | Allocated [B] |
|-----------------------------------------------------------------------------|--------------|----------:|-----------:|------------:|-----------:|----------:|--------------:|
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_WriteAsync              | 200000       |  112.4 ms |    4.17 ms |    11.83 ms | 11000.0000 |         - |  54,480,176 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_WriteAsyncAsTaskWhenAll | 200000       |  115.2 ms |    4.09 ms |    11.85 ms | 15000.0000 | 1000.0000 |  73,886,416 B |



**with locks**
====

| Method                                                | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] |     Gen 0 | Allocated [B] |
|-------------------------------------------------------|--------------|----------:|-----------:|------------:|----------:|--------------:|
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber    | 200000       |  23.41 ms |   0.681 ms |    1.976 ms | 1000.0000 |   6,420,592 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers   | 200000       |  75.93 ms |   2.011 ms |    5.706 ms | 1000.0000 |   6,425,344 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers | 200000       | 140.94 ms |   3.819 ms |   11.259 ms | 1000.0000 |   6,435,216 B |


| Method                                                                        | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] | Median [ms] |     Gen 0 |     Gen 1 | Allocated [B] |
|-------------------------------------------------------------------------------|--------------|----------:|-----------:|------------:|------------:|----------:|----------:|--------------:|
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_NoLockWriter               | 200000       |  10.41 ms |   0.361 ms |    0.981 ms |    10.25 ms | 1000.0000 |         - |   6,470,072 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber                            | 200000       |  24.24 ms |   1.249 ms |    3.662 ms |    23.12 ms | 1000.0000 | 1000.0000 |   8,503,928 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ReadWriteLockSlimWriter    | 200000       |  27.89 ms |   0.862 ms |    2.500 ms |    28.21 ms | 1000.0000 | 1000.0000 |   6,406,840 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_SemaphoreSlimWriter        | 200000       |  34.12 ms |   1.272 ms |    3.691 ms |    34.56 ms | 1000.0000 |         - |   6,437,432 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_NoLockWriter              | 200000       |  61.59 ms |   2.219 ms |    6.474 ms |    62.81 ms | 1000.0000 | 1000.0000 |   6,571,408 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers                           | 200000       |  69.52 ms |   3.469 ms |   10.064 ms |    70.51 ms | 1000.0000 |         - |   7,064,720 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_SemaphoreSlimWriter       | 200000       |  70.83 ms |   4.238 ms |   12.495 ms |    71.54 ms | 1000.0000 |         - |   6,802,128 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ReadWriteLockSlimWriter   | 200000       |  71.76 ms |   3.669 ms |   10.818 ms |    73.23 ms | 1000.0000 |         - |   6,604,624 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_NoLockWriter            | 200000       | 121.28 ms |   6.099 ms |   17.983 ms |   124.36 ms | 1000.0000 | 1000.0000 |   6,574,280 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers                         | 200000       | 125.98 ms |   5.766 ms |   16.911 ms |   128.60 ms | 1000.0000 |         - |   6,739,176 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ReadWriteLockSlimWriter | 200000       | 130.87 ms |   6.002 ms |   17.698 ms |   133.48 ms | 1000.0000 |         - |   6,805,736 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_SemaphoreSlimWriter     | 200000       | 136.89 ms |   4.885 ms |   14.405 ms |   135.51 ms | 1000.0000 |         - |   6,465,256 B |



| Method                                                                     | MessageCount | Mean [ms] | Error [ms] | StdDev [ms] | Median [ms] |     Gen 0 |     Gen 1 | Allocated [B] |
|----------------------------------------------------------------------------|--------------|----------:|-----------:|------------:|------------:|----------:|----------:|--------------:|
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_NoLockWriter            | 200000       |  10.28 ms |   0.400 ms |    1.089 ms |    9.914 ms | 1000.0000 |         - |   6,536,336 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableListWriter     | 200000       |  10.81 ms |   0.373 ms |    1.047 ms |   10.549 ms | 1000.0000 | 1000.0000 |   6,536,048 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableArrayWriter    | 200000       |  11.19 ms |   0.867 ms |    2.516 ms |   10.033 ms | 1000.0000 |         - |   6,470,640 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber                         | 200000       |  22.57 ms |   0.582 ms |    1.650 ms |   22.704 ms | 1000.0000 |         - |   6,667,568 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableArrayWriter   | 200000       |  49.00 ms |   2.358 ms |    6.953 ms |   48.559 ms | 1000.0000 |         - |   6,438,528 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_NoLockWriter           | 200000       |  63.77 ms |   2.388 ms |    6.965 ms |   64.388 ms | 1000.0000 |         - |   6,438,528 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers                        | 200000       |  72.16 ms |   3.061 ms |    8.977 ms |   71.912 ms | 1000.0000 |         - |   6,933,632 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableArrayWriter | 200000       |  89.03 ms |   2.969 ms |    8.754 ms |   89.987 ms | 1000.0000 | 1000.0000 |   6,581,264 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_NoLockWriter         | 200000       | 115.57 ms |   3.879 ms |   11.375 ms |  116.381 ms | 1000.0000 |         - |   6,515,280 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers                      | 200000       | 125.11 ms |   3.809 ms |   11.051 ms |  126.074 ms | 1000.0000 |         - |   6,540,752 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableListWriter    | 200000       | 147.84 ms |   4.165 ms |   12.017 ms |  146.205 ms | 1000.0000 | 1000.0000 |   6,571,392 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableListWriter  | 200000       | 216.17 ms |   4.162 ms |   11.252 ms |  216.024 ms | 1000.0000 | 1000.0000 |   6,426,576 B |



| Method                                                                                  | MessageCount |    Mean [ms] | Error [ms] | StdDev [ms] |  Median [ms] |      Gen 0 |     Gen 1 |     Gen 2 | Allocated [B] |
|-----------------------------------------------------------------------------------------|--------------|-------------:|-----------:|------------:|-------------:|-----------:|----------:|----------:|--------------:|
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableArrayWriter                 | 20000        |     1.504 ms |  0.1056 ms |   0.3115 ms |     1.617 ms |          - |         - |         - |     646,840 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayForLoopWriter               | 20000        |     2.424 ms |  0.0917 ms |   0.2675 ms |     2.455 ms |          - |         - |         - |     644,344 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber                                      | 20000        |     2.475 ms |  0.1030 ms |   0.3037 ms |     2.427 ms |          - |         - |         - |     651,384 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedImmutableArrayWriter           | 20000        |     2.580 ms |  0.1096 ms |   0.3215 ms |     2.606 ms |          - |         - |         - |     644,632 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayWriter                      | 20000        |     2.629 ms |  0.1182 ms |   0.3466 ms |     2.688 ms |          - |         - |         - |     677,144 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedImmutableArrayForLoopWriter    | 20000        |     4.534 ms |  0.0901 ms |   0.2260 ms |     4.510 ms |          - |         - |         - |   1,182,712 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableArrayWriter                | 20000        |     5.520 ms |  0.3861 ms |   1.1264 ms |     5.310 ms |          - |         - |         - |     662,864 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayWriter                     | 20000        |     6.209 ms |  0.3918 ms |   1.1304 ms |     6.258 ms |          - |         - |         - |     645,712 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayForLoopWriter              | 20000        |     6.592 ms |  0.3686 ms |   1.0809 ms |     6.527 ms |          - |         - |         - |     745,424 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedImmutableArrayWriter          | 20000        |     6.706 ms |  0.3948 ms |   1.1328 ms |     6.670 ms |          - |         - |         - |     652,176 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers                                     | 20000        |     7.401 ms |  0.4707 ms |   1.3582 ms |     7.333 ms |          - |         - |         - |     642,320 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedImmutableArrayForLoopWriter   | 20000        |     7.527 ms |  0.4218 ms |   1.1688 ms |     7.607 ms |          - |         - |         - |     917,840 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableArrayWriter              | 20000        |     9.464 ms |  0.5899 ms |   1.7299 ms |     9.139 ms |          - |         - |         - |     795,816 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayForLoopWriter            | 20000        |    10.076 ms |  0.5126 ms |   1.5033 ms |    10.013 ms |          - |         - |         - |     665,704 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayWriter                   | 20000        |    10.148 ms |  0.5355 ms |   1.5536 ms |    10.323 ms |          - |         - |         - |     651,048 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedImmutableArrayForLoopWriter | 20000        |    10.916 ms |  0.4867 ms |   1.4349 ms |    10.660 ms |          - |         - |         - |     662,056 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableArrayWriter                 | 200000       |    11.208 ms |  0.7336 ms |   2.0931 ms |    10.676 ms |  1000.0000 | 1000.0000 |         - |   6,470,072 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedImmutableArrayWriter        | 20000        |    11.559 ms |  0.5483 ms |   1.6167 ms |    11.421 ms |          - |         - |         - |     650,024 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers                                   | 20000        |    12.150 ms |  0.5767 ms |   1.6824 ms |    12.226 ms |          - |         - |         - |     647,368 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedImmutableArrayWriter           | 200000       |    23.289 ms |  1.2558 ms |   3.7028 ms |    23.538 ms |  1000.0000 |         - |         - |   6,420,312 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayForLoopWriter               | 200000       |    24.083 ms |  1.0429 ms |   3.0749 ms |    23.571 ms |  1000.0000 | 1000.0000 |         - |   6,470,072 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber                                      | 200000       |    24.343 ms |  1.0000 ms |   2.9486 ms |    23.466 ms |  1000.0000 |         - |         - |   6,436,856 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayWriter                      | 200000       |    24.474 ms |  1.0082 ms |   2.9727 ms |    24.050 ms |  1000.0000 | 1000.0000 |         - |   6,437,432 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableArrayWriter                | 200000       |    46.016 ms |  3.0506 ms |   8.8989 ms |    47.351 ms |  1000.0000 |         - |         - |   6,538,928 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedImmutableArrayForLoopWriter    | 200000       |    48.087 ms |  0.9562 ms |   2.3094 ms |    48.270 ms |  1000.0000 | 1000.0000 | 1000.0000 |  10,675,064 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayForLoopWriter              | 200000       |    51.989 ms |  3.1124 ms |   8.9799 ms |    53.436 ms |  1000.0000 |         - |         - |   6,455,952 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedImmutableArrayWriter          | 200000       |    57.674 ms |  2.5673 ms |   7.4481 ms |    57.561 ms |  1000.0000 |         - |         - |   6,488,592 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayWriter                     | 200000       |    60.823 ms |  1.8107 ms |   5.2820 ms |    61.367 ms |  1000.0000 |         - |         - |   6,934,224 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers                                     | 200000       |    72.716 ms |  2.6794 ms |   7.7307 ms |    72.900 ms |  1000.0000 |         - |         - |   6,686,096 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedImmutableArrayForLoopWriter   | 200000       |    80.174 ms |  2.9605 ms |   8.5888 ms |    82.461 ms |  1000.0000 |         - |         - |   8,804,944 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayWriter                   | 200000       |    86.336 ms |  4.2120 ms |  12.4191 ms |    86.563 ms |  1000.0000 | 1000.0000 |         - |   7,199,848 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableArrayWriter              | 200000       |    87.291 ms |  5.5249 ms |  16.2037 ms |    89.493 ms |  1000.0000 |         - |         - |   7,001,448 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayForLoopWriter            | 200000       |    89.295 ms |  5.7017 ms |  16.6321 ms |    88.452 ms |  1000.0000 |         - |         - |   7,462,312 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_ImmutableArrayWriter                 | 2000000      |    94.351 ms |  1.8833 ms |   5.4338 ms |    92.878 ms | 13000.0000 | 1000.0000 |         - |  64,036,856 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedImmutableArrayWriter        | 200000       |   100.449 ms |  5.5294 ms |  16.1295 ms |   102.340 ms |  1000.0000 | 1000.0000 |         - |   6,837,768 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedImmutableArrayForLoopWriter | 200000       |   105.495 ms |  4.1540 ms |  12.2481 ms |   108.313 ms |  1000.0000 |         - |         - |   6,673,192 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers                                   | 200000       |   126.702 ms |  3.6719 ms |  10.8266 ms |   126.309 ms |  1000.0000 | 1000.0000 |         - |   6,555,816 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedImmutableArrayWriter           | 2000000      |   196.546 ms |  8.3818 ms |  24.3171 ms |   193.909 ms | 13000.0000 | 1000.0000 |         - |  64,268,152 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayWriter                      | 2000000      |   218.800 ms |  8.3027 ms |  23.9553 ms |   218.349 ms | 13000.0000 | 2000.0000 |         - |  65,055,192 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber                                      | 2000000      |   223.302 ms |  8.6685 ms |  25.4231 ms |   224.359 ms | 13000.0000 | 1000.0000 |         - |  65,054,904 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockArrayForLoopWriter               | 2000000      |   234.111 ms |  7.5077 ms |  22.1365 ms |   235.860 ms | 13000.0000 |         - |         - |  64,136,056 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_ImmutableArrayWriter                | 2000000      |   466.052 ms | 16.3582 ms |  47.9759 ms |   469.114 ms | 13000.0000 |         - |         - |  64,664,720 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_LockedImmutableArrayForLoopWriter    | 2000000      |   554.602 ms | 11.0247 ms |  24.6582 ms |   557.311 ms | 12000.0000 | 6000.0000 | 3000.0000 |  99,689,224 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayForLoopWriter              | 2000000      |   614.000 ms | 26.7853 ms |  78.9770 ms |   626.379 ms | 12000.0000 | 1000.0000 |         - |  67,157,328 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedImmutableArrayWriter          | 2000000      |   614.827 ms | 22.3945 ms |  66.0307 ms |   618.809 ms | 13000.0000 | 1000.0000 |         - |  64,796,240 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockArrayWriter                     | 2000000      |   625.737 ms | 21.1097 ms |  62.2423 ms |   631.327 ms | 13000.0000 | 1000.0000 |         - |  65,320,976 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers                                     | 2000000      |   699.581 ms | 23.1937 ms |  68.3871 ms |   710.554 ms | 13000.0000 | 2000.0000 |         - |  67,157,328 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_LockedImmutableArrayForLoopWriter   | 2000000      |   783.863 ms | 24.7281 ms |  72.1331 ms |   779.269 ms | 12000.0000 | 5000.0000 | 2000.0000 |  82,376,608 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_ImmutableArrayWriter              | 2000000      |   944.963 ms | 36.6522 ms | 108.0699 ms |   945.641 ms | 12000.0000 | 4000.0000 |         - |  66,636,968 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayWriter                   | 2000000      | 1,070.472 ms | 41.2936 ms | 121.7552 ms | 1,111.525 ms | 13000.0000 | 1000.0000 |         - |  65,324,904 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedImmutableArrayForLoopWriter | 2000000      | 1,079.690 ms | 33.4936 ms |  98.7567 ms | 1,083.017 ms | 13000.0000 | 1000.0000 |         - |  64,537,256 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockArrayForLoopWriter            | 2000000      | 1,081.039 ms | 39.7930 ms | 117.3305 ms | 1,069.451 ms | 13000.0000 | 1000.0000 |         - |  64,668,488 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_LockedImmutableArrayWriter        | 2000000      | 1,094.212 ms | 33.9013 ms |  99.4266 ms | 1,099.929 ms | 13000.0000 | 1000.0000 |         - |  67,685,992 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers                                   | 2000000      | 1,222.305 ms | 41.3897 ms | 122.0386 ms | 1,224.263 ms | 13000.0000 | 1000.0000 |         - |  66,636,968 B |

| Method                                                                | MessageCount |  Mean [ms] | Error [ms] | StdDev [ms] |      Gen 0 |     Gen 1 | Allocated [B] |
|-----------------------------------------------------------------------|--------------|-----------:|-----------:|------------:|-----------:|----------:|--------------:|
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber_WriteEnumerable    | 2000000      |   192.6 ms |    3.70 ms |     9.43 ms | 21000.0000 | 1000.0000 | 105,054,976 B |
| BroadcastQueue_WithoutHost_ReadWrite_OneSubscriber                    | 2000000      |   202.0 ms |   12.26 ms |    35.95 ms | 13000.0000 |         - |  64,070,072 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers_WriteEnumerable   | 2000000      |   618.5 ms |   12.07 ms |    31.80 ms | 22000.0000 |         - | 104,072,280 B |
| BroadcastQueue_WithoutHost_ReadWrite_TwoSubscribers                   | 2000000      |   653.1 ms |   21.02 ms |    61.98 ms | 13000.0000 | 1000.0000 |  64,270,608 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers_WriteEnumerable | 2000000      | 1,047.9 ms |   20.57 ms |    28.84 ms | 22000.0000 | 1000.0000 | 104,437,552 B |
| BroadcastQueue_WithoutHost_ReadWrite_ThreeSubscribers                 | 2000000      | 1,243.2 ms |   24.71 ms |    59.69 ms | 13000.0000 | 1000.0000 |  64,223,144 B |



