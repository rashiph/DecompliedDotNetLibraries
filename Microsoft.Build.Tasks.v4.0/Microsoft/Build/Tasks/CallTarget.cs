namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using System;
    using System.Collections;

    [RunInMTA]
    public class CallTarget : TaskExtension
    {
        private bool runEachTargetSeparately;
        private ArrayList targetOutputs = new ArrayList();
        private string[] targets;
        private bool useResultsCache;

        public override bool Execute()
        {
            if ((this.Targets == null) || (this.Targets.Length == 0))
            {
                return true;
            }
            ArrayList targetLists = MSBuild.CreateTargetLists(this.Targets, this.RunEachTargetSeparately);
            return MSBuild.ExecuteTargets(new ITaskItem[] { null }, null, null, targetLists, false, false, base.BuildEngine3, base.Log, this.targetOutputs, this.UseResultsCache, false, null);
        }

        public bool RunEachTargetSeparately
        {
            get
            {
                return this.runEachTargetSeparately;
            }
            set
            {
                this.runEachTargetSeparately = value;
            }
        }

        [Output]
        public ITaskItem[] TargetOutputs
        {
            get
            {
                return (ITaskItem[]) this.targetOutputs.ToArray(typeof(ITaskItem));
            }
        }

        public string[] Targets
        {
            get
            {
                return this.targets;
            }
            set
            {
                this.targets = value;
            }
        }

        public bool UseResultsCache
        {
            get
            {
                return this.useResultsCache;
            }
            set
            {
                this.useResultsCache = value;
            }
        }
    }
}

