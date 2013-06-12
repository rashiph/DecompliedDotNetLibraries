namespace System
{
    [Serializable]
    internal enum ConfigEvents
    {
        DataAvailable = 9,
        EndDocument = 8,
        EndDTD = 2,
        EndDTDSubset = 4,
        EndEntity = 7,
        EndProlog = 5,
        LastEvent = 9,
        StartDocument = 0,
        StartDTD = 1,
        StartDTDSubset = 3,
        StartEntity = 6
    }
}

