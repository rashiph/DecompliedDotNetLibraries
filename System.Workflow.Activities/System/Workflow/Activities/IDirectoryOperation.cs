namespace System.Workflow.Activities
{
    using System;
    using System.Collections.Generic;
    using System.DirectoryServices;

    internal interface IDirectoryOperation
    {
        void GetResult(DirectoryEntry rootEntry, DirectoryEntry currentEntry, List<DirectoryEntry> response);
    }
}

