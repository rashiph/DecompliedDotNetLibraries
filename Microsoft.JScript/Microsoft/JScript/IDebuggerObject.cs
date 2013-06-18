namespace Microsoft.JScript
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("8E93D770-6168-4b68-B896-A71B74C7076A")]
    public interface IDebuggerObject
    {
        bool IsCOMObject();
        bool IsEqual(IDebuggerObject o);
        bool HasEnumerableMember(string name);
        bool IsScriptFunction();
        bool IsScriptObject();
    }
}

