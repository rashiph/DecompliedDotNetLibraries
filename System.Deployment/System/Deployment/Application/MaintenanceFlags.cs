namespace System.Deployment.Application
{
    using System;

    [Flags]
    internal enum MaintenanceFlags
    {
        ClearFlag = 0,
        RemoveSelected = 4,
        RestorationPossible = 1,
        RestoreSelected = 2
    }
}

