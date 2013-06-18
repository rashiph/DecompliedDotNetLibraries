namespace Microsoft.Build.Framework
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("8661674F-2148-4F71-A92A-49875511C528")]
    public interface ITaskItem
    {
        string ItemSpec { get; set; }
        ICollection MetadataNames { get; }
        int MetadataCount { get; }
        string GetMetadata(string metadataName);
        void SetMetadata(string metadataName, string metadataValue);
        void RemoveMetadata(string metadataName);
        void CopyMetadataTo(ITaskItem destinationItem);
        IDictionary CloneCustomMetadata();
    }
}

