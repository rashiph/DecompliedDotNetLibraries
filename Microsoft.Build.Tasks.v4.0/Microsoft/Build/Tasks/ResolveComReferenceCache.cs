namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Reflection;

    [Serializable]
    internal sealed class ResolveComReferenceCache : StateFileBase
    {
        private string axImpLocation;
        private Hashtable componentTimestamps;
        [NonSerialized]
        private bool dirty;
        private string tlbImpLocation;

        internal ResolveComReferenceCache(string tlbImpPath, string axImpPath)
        {
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(tlbImpPath, "tlbImpPath");
            Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgumentNull(axImpPath, "axImpPath");
            this.tlbImpLocation = tlbImpPath;
            this.axImpLocation = axImpPath;
            this.componentTimestamps = new Hashtable();
        }

        internal bool ToolPathsMatchCachePaths(string tlbImpPath, string axImpPath)
        {
            return (string.Equals(this.tlbImpLocation, tlbImpPath, StringComparison.OrdinalIgnoreCase) && string.Equals(this.axImpLocation, axImpPath, StringComparison.OrdinalIgnoreCase));
        }

        internal bool Dirty
        {
            get
            {
                return this.dirty;
            }
        }

        internal DateTime this[string componentPath]
        {
            get
            {
                if (this.componentTimestamps.ContainsKey(componentPath))
                {
                    return (DateTime) this.componentTimestamps[componentPath];
                }
                return DateTime.Now;
            }
            set
            {
                if (DateTime.Compare(this[componentPath], value) != 0)
                {
                    this.componentTimestamps[componentPath] = value;
                    this.dirty = true;
                }
            }
        }
    }
}

