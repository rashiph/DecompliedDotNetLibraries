namespace Microsoft.Build.Framework
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;

    [ComVisible(true), Guid("ac6d5a59-f877-461b-88e3-b2f06fce0cb9")]
    public interface ITaskItem2 : ITaskItem
    {
        string EvaluatedIncludeEscaped { get; set; }
        string GetMetadataValueEscaped(string metadataName);
        void SetMetadataValueLiteral(string metadataName, string metadataValue);
        IDictionary CloneCustomMetadataEscaped();
    }
}

