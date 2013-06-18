namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("D1A19408-BB6B-43eb-BB6F-E7CF6AF047D7")]
    public interface IDefineEvent
    {
        [return: MarshalAs(UnmanagedType.Interface)]
        object AddEvent(string code, int startLine);
    }
}

