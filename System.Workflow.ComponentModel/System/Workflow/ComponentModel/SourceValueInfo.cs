namespace System.Workflow.ComponentModel
{
    using System;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SourceValueInfo
    {
        internal SourceValueType type;
        internal DrillIn drillIn;
        internal string name;
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal SourceValueInfo(SourceValueType t, DrillIn d, string n)
        {
            this.type = t;
            this.drillIn = d;
            this.name = n;
        }
    }
}

