namespace System.Diagnostics
{
    using System;
    using System.ComponentModel;

    [TypeConverter(typeof(AlphabeticalEnumConverter))]
    public enum PerformanceCounterType
    {
        AverageBase = 0x40030402,
        AverageCount64 = 0x40020500,
        AverageTimer32 = 0x30020400,
        CounterDelta32 = 0x400400,
        CounterDelta64 = 0x400500,
        CounterMultiBase = 0x42030500,
        CounterMultiTimer = 0x22410500,
        CounterMultiTimer100Ns = 0x22510500,
        CounterMultiTimer100NsInverse = 0x23510500,
        CounterMultiTimerInverse = 0x23410500,
        CounterTimer = 0x20410500,
        CounterTimerInverse = 0x21410500,
        CountPerTimeInterval32 = 0x450400,
        CountPerTimeInterval64 = 0x450500,
        ElapsedTime = 0x30240500,
        NumberOfItems32 = 0x10000,
        NumberOfItems64 = 0x10100,
        NumberOfItemsHEX32 = 0,
        NumberOfItemsHEX64 = 0x100,
        RateOfCountsPerSecond32 = 0x10410400,
        RateOfCountsPerSecond64 = 0x10410500,
        RawBase = 0x40030403,
        RawFraction = 0x20020400,
        SampleBase = 0x40030401,
        SampleCounter = 0x410400,
        SampleFraction = 0x20c20400,
        Timer100Ns = 0x20510500,
        Timer100NsInverse = 0x21510500
    }
}

