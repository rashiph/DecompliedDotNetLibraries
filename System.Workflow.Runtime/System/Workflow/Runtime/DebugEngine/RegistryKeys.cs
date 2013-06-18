namespace System.Workflow.Runtime.DebugEngine
{
    using System;

    internal static class RegistryKeys
    {
        internal static readonly string DebuggerSubKey = (ProductRootRegKey + @"\Debugger");
        internal static readonly string ProductRootRegKey = @"SOFTWARE\Microsoft\Net Framework Setup\NDP\v4.0\Setup\Windows Workflow Foundation";
    }
}

