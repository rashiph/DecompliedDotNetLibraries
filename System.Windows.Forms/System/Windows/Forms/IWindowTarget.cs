namespace System.Windows.Forms
{
    using System;

    public interface IWindowTarget
    {
        void OnHandleChange(IntPtr newHandle);
        void OnMessage(ref Message m);
    }
}

