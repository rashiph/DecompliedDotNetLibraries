namespace Microsoft.Build.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [Serializable, StructLayout(LayoutKind.Sequential)]
    public struct BuildEngineResult
    {
        private bool buildResult;
        private List<IDictionary<string, ITaskItem[]>> targetOutputsPerProject;
        public BuildEngineResult(bool result, List<IDictionary<string, ITaskItem[]>> targetOutputsPerProject)
        {
            this.buildResult = result;
            this.targetOutputsPerProject = targetOutputsPerProject;
            if (this.targetOutputsPerProject == null)
            {
                this.targetOutputsPerProject = new List<IDictionary<string, ITaskItem[]>>();
            }
        }

        public bool Result
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.buildResult;
            }
        }
        public IList<IDictionary<string, ITaskItem[]>> TargetOutputsPerProject
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.targetOutputsPerProject;
            }
        }
    }
}

