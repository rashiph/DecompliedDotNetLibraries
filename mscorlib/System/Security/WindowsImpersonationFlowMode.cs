namespace System.Security
{
    using System;

    internal enum WindowsImpersonationFlowMode
    {
        IMP_ALWAYSFLOW = 2,
        IMP_DEFAULT = 0,
        IMP_FASTFLOW = 0,
        IMP_NOFLOW = 1
    }
}

