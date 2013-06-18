namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Xml;

    public class ResolveNonMSBuildProjectOutput : ResolveProjectBase
    {
        private string preresolvedProjectOutputs;
        private ITaskItem[] resolvedOutputPaths;
        private ITaskItem[] unresolvedProjectReferences;

        public override bool Execute()
        {
            if (this.GetAssemblyName == null)
            {
                this.GetAssemblyName = new GetAssemblyNameDelegate(AssemblyName.GetAssemblyName);
            }
            try
            {
                if (!base.VerifyProjectReferenceItems(base.ProjectReferences, false))
                {
                    return false;
                }
                ArrayList list = new ArrayList(base.ProjectReferences.GetLength(0));
                ArrayList list2 = new ArrayList(base.ProjectReferences.GetLength(0));
                base.CacheProjectElementsFromXml(this.PreresolvedProjectOutputs);
                foreach (ITaskItem item in base.ProjectReferences)
                {
                    ITaskItem item2;
                    base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveNonMSBuildProjectOutput.ProjectReferenceResolutionStarting", new object[] { item.ItemSpec });
                    if (this.ResolveProject(item, out item2))
                    {
                        if (item2.ItemSpec.Length > 0)
                        {
                            try
                            {
                                this.GetAssemblyName(item2.ItemSpec);
                                item2.SetMetadata("ManagedAssembly", "true");
                            }
                            catch (BadImageFormatException)
                            {
                            }
                            list.Add(item2);
                            base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveNonMSBuildProjectOutput.ProjectReferenceResolutionSuccess", new object[] { item.ItemSpec, item2.ItemSpec });
                        }
                        else
                        {
                            base.Log.LogWarningWithCodeFromResources("ResolveNonMSBuildProjectOutput.ProjectReferenceResolutionFailure", new object[] { item.ItemSpec });
                        }
                    }
                    else
                    {
                        list2.Add(item);
                        base.Log.LogMessageFromResources(MessageImportance.Low, "ResolveNonMSBuildProjectOutput.ProjectReferenceUnresolved", new object[] { item.ItemSpec });
                    }
                }
                this.ResolvedOutputPaths = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
                this.UnresolvedProjectReferences = (ITaskItem[]) list2.ToArray(typeof(ITaskItem));
            }
            catch (XmlException exception)
            {
                base.Log.LogErrorWithCodeFromResources("General.ErrorExecutingTask", new object[] { base.GetType().Name, exception.Message });
                return false;
            }
            return true;
        }

        internal bool ResolveProject(ITaskItem projectRef, out ITaskItem resolvedPath)
        {
            string projectItem = base.GetProjectItem(projectRef);
            if (projectItem != null)
            {
                resolvedPath = new TaskItem(projectItem);
                projectRef.CopyMetadataTo(resolvedPath);
                return true;
            }
            resolvedPath = null;
            return false;
        }

        internal GetAssemblyNameDelegate GetAssemblyName { get; set; }

        public string PreresolvedProjectOutputs
        {
            get
            {
                return this.preresolvedProjectOutputs;
            }
            set
            {
                this.preresolvedProjectOutputs = value;
            }
        }

        [Output]
        public ITaskItem[] ResolvedOutputPaths
        {
            get
            {
                return this.resolvedOutputPaths;
            }
            set
            {
                this.resolvedOutputPaths = value;
            }
        }

        [Output]
        public ITaskItem[] UnresolvedProjectReferences
        {
            get
            {
                return this.unresolvedProjectReferences;
            }
            set
            {
                this.unresolvedProjectReferences = value;
            }
        }

        internal delegate AssemblyName GetAssemblyNameDelegate(string path);
    }
}

