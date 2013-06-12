namespace System.Runtime.ConstrainedExecution
{
    using System;

    [Serializable]
    public enum Consistency
    {
        MayCorruptProcess,
        MayCorruptAppDomain,
        MayCorruptInstance,
        WillNotCorruptState
    }
}

