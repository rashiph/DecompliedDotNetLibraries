namespace System.ServiceModel.Channels
{
    using System;

    [Flags]
    internal enum MaskingMode
    {
        None,
        Handled,
        Unhandled,
        All
    }
}

