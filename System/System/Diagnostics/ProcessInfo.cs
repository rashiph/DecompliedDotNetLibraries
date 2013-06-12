namespace System.Diagnostics
{
    using System;
    using System.Collections;

    internal class ProcessInfo
    {
        public int basePriority;
        public int handleCount;
        public int mainModuleId;
        public long pageFileBytes;
        public long pageFileBytesPeak;
        public long poolNonpagedBytes;
        public long poolPagedBytes;
        public long privateBytes;
        public int processId;
        public string processName;
        public int sessionId;
        public ArrayList threadInfoList = new ArrayList();
        public long virtualBytes;
        public long virtualBytesPeak;
        public long workingSet;
        public long workingSetPeak;
    }
}

