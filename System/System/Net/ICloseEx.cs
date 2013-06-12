namespace System.Net
{
    using System;

    internal interface ICloseEx
    {
        void CloseEx(CloseExState closeState);
    }
}

