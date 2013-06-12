namespace System.Runtime
{
    using System;

    [Serializable]
    public enum GCLatencyMode
    {
        Batch,
        Interactive,
        LowLatency,
        SustainedLowLatency
    }
}

