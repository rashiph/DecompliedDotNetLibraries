namespace System.Drawing.Imaging
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void PlayRecordCallback(EmfPlusRecordType recordType, int flags, int dataSize, IntPtr recordData);
}

