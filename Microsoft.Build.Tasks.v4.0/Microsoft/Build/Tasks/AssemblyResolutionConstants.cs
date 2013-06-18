namespace Microsoft.Build.Tasks
{
    using System;

    internal static class AssemblyResolutionConstants
    {
        public const string assemblyFoldersExSentinel = "{registry:";
        public const string assemblyFoldersSentinel = "{assemblyfolders}";
        public const string candidateAssemblyFilesSentinel = "{candidateassemblyfiles}";
        public const string frameworkPathSentinel = "{targetframeworkdirectory}";
        public const string gacSentinel = "{gac}";
        public const string hintPathSentinel = "{hintpathfromitem}";
        public const string rawFileNameSentinel = "{rawfilename}";
    }
}

