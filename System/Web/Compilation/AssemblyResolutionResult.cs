namespace System.Web.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    internal class AssemblyResolutionResult
    {
        internal ICollection<BuildErrorEventArgs> Errors { get; set; }

        internal ICollection<string> ResolvedFiles { get; set; }

        internal ICollection<string> ResolvedFilesWithWarnings { get; set; }

        internal ICollection<Assembly> UnresolvedAssemblies { get; set; }

        internal ICollection<BuildWarningEventArgs> Warnings { get; set; }
    }
}

