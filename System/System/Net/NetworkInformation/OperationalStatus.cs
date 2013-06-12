namespace System.Net.NetworkInformation
{
    using System;

    public enum OperationalStatus
    {
        Dormant = 5,
        Down = 2,
        LowerLayerDown = 7,
        NotPresent = 6,
        Testing = 3,
        Unknown = 4,
        Up = 1
    }
}

