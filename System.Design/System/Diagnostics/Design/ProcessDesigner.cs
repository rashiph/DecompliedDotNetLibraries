namespace System.Diagnostics.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ProcessDesigner : ComponentDesigner
    {
        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            bool[] browsableSettings = new bool[0x1c];
            browsableSettings[1] = true;
            browsableSettings[2] = true;
            RuntimeComponentFilter.FilterProperties(properties, null, new string[] { 
                "SynchronizingObject", "EnableRaisingEvents", "StartInfo", "BasePriority", "HandleCount", "Id", "MainWindowHandle", "MainWindowTitle", "MaxWorkingSet", "MinWorkingSet", "NonpagedSystemMemorySize", "PagedMemorySize", "PagedSystemMemorySize", "PeakPagedMemorySize", "PeakWorkingSet", "PeakVirtualMemorySize", 
                "PriorityBoostEnabled", "PriorityClass", "PrivateMemorySize", "PrivilegedProcessorTime", "ProcessName", "ProcessorAffinity", "Responding", "StartTime", "TotalProcessorTime", "UserProcessorTime", "VirtualMemorySize", "WorkingSet"
             }, browsableSettings);
        }
    }
}

