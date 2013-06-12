namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;

    internal class CategoryEntry
    {
        internal int[] CounterIndexes;
        internal int HelpIndex;
        internal int[] HelpIndexes;
        internal int NameIndex;

        internal CategoryEntry(NativeMethods.PERF_OBJECT_TYPE perfObject)
        {
            this.NameIndex = perfObject.ObjectNameTitleIndex;
            this.HelpIndex = perfObject.ObjectHelpTitleIndex;
            this.CounterIndexes = new int[perfObject.NumCounters];
            this.HelpIndexes = new int[perfObject.NumCounters];
        }
    }
}

