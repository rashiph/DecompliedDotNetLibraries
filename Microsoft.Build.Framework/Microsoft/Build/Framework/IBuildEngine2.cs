namespace Microsoft.Build.Framework
{
    using System;
    using System.Collections;

    public interface IBuildEngine2 : IBuildEngine
    {
        bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs, string toolsVersion);
        bool BuildProjectFilesInParallel(string[] projectFileNames, string[] targetNames, IDictionary[] globalProperties, IDictionary[] targetOutputsPerProject, string[] toolsVersion, bool useResultsCache, bool unloadProjectsOnCompletion);

        bool IsRunningMultipleNodes { get; }
    }
}

