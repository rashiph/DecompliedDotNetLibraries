namespace Microsoft.Build.Framework
{
    using System;
    using System.Collections;

    public interface IBuildEngine
    {
        bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs);
        void LogCustomEvent(CustomBuildEventArgs e);
        void LogErrorEvent(BuildErrorEventArgs e);
        void LogMessageEvent(BuildMessageEventArgs e);
        void LogWarningEvent(BuildWarningEventArgs e);

        int ColumnNumberOfTaskNode { get; }

        bool ContinueOnError { get; }

        int LineNumberOfTaskNode { get; }

        string ProjectFileOfTaskNode { get; }
    }
}

