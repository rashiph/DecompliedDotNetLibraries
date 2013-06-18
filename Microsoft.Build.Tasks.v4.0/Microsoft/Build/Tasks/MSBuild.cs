namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Build.Utilities;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;

    [RunInMTA]
    public class MSBuild : TaskExtension
    {
        private bool buildInParallel;
        private ITaskItem[] projects;
        private string[] properties;
        private bool rebaseOutputs;
        private bool runEachTargetSeparately;
        private SkipNonexistentProjectsBehavior skipNonexistentProjects = SkipNonexistentProjectsBehavior.Error;
        private bool stopOnFirstFailure;
        private string[] targetAndPropertyListSeparators;
        private ArrayList targetOutputs = new ArrayList();
        private string[] targets;
        private string toolsVersion;
        private string undefineProperties;
        private bool unloadProjectsOnCompletion;
        private bool useResultsCache = true;

        private bool BuildProjectsInParallel(Hashtable propertiesTable, string[] undefinePropertiesArray, ArrayList targetLists, bool success, bool[] skipProjects)
        {
            ITaskItem[] projects = this.Projects;
            List<ITaskItem> list = new List<ITaskItem>();
            for (int i = 0; i < this.Projects.Length; i++)
            {
                if (!skipProjects[i])
                {
                    list.Add(this.Projects[i]);
                }
            }
            projects = list.ToArray();
            if ((projects.Length > 0) && !ExecuteTargets(projects, propertiesTable, undefinePropertiesArray, targetLists, this.StopOnFirstFailure, this.RebaseOutputs, base.BuildEngine3, base.Log, this.targetOutputs, this.useResultsCache, this.unloadProjectsOnCompletion, this.ToolsVersion))
            {
                success = false;
            }
            return success;
        }

        internal static ArrayList CreateTargetLists(string[] targets, bool runEachTargetSeparately)
        {
            ArrayList list = new ArrayList();
            if ((runEachTargetSeparately && (targets != null)) && (targets.Length > 0))
            {
                foreach (string str in targets)
                {
                    list.Add(new string[] { str });
                }
                return list;
            }
            list.Add(targets);
            return list;
        }

        public override bool Execute()
        {
            Hashtable hashtable;
            if ((this.Projects == null) || (this.Projects.Length == 0))
            {
                return true;
            }
            if ((this.TargetAndPropertyListSeparators != null) && (this.TargetAndPropertyListSeparators.Length > 0))
            {
                this.ExpandAllTargetsAndProperties();
            }
            if (!PropertyParser.GetTableWithEscaping(base.Log, Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("General.GlobalProperties", new object[0]), "Properties", this.Properties, out hashtable))
            {
                return false;
            }
            string[] undefineProperties = null;
            if (!string.IsNullOrEmpty(this.undefineProperties))
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "General.UndefineProperties", new object[0]);
                undefineProperties = this.undefineProperties.Split(new char[] { ';' });
                foreach (string str in undefineProperties)
                {
                    base.Log.LogMessageFromText(string.Format(CultureInfo.InvariantCulture, "  {0}", new object[] { str }), MessageImportance.Low);
                }
            }
            bool isRunningMultipleNodes = base.BuildEngine2.IsRunningMultipleNodes;
            if ((!isRunningMultipleNodes && this.stopOnFirstFailure) && this.buildInParallel)
            {
                this.buildInParallel = false;
                base.Log.LogMessageFromResources(MessageImportance.Low, "MSBuild.NotBuildingInParallel", new object[0]);
            }
            if ((isRunningMultipleNodes && this.buildInParallel) && (this.stopOnFirstFailure && !this.runEachTargetSeparately))
            {
                base.Log.LogMessageFromResources(MessageImportance.Low, "MSBuild.NoStopOnFirstFailure", new object[0]);
            }
            ArrayList targetLists = CreateTargetLists(this.Targets, this.RunEachTargetSeparately);
            bool success = true;
            ITaskItem[] projects = null;
            bool[] skipProjects = null;
            if (this.buildInParallel)
            {
                skipProjects = new bool[this.Projects.Length];
                for (int j = 0; j < skipProjects.Length; j++)
                {
                    skipProjects[j] = true;
                }
            }
            else
            {
                projects = new ITaskItem[1];
            }
            for (int i = 0; i < this.Projects.Length; i++)
            {
                ITaskItem item = this.Projects[i];
                string path = Microsoft.Build.Shared.FileUtilities.AttemptToShortenPath(item.ItemSpec);
                if (this.stopOnFirstFailure && !success)
                {
                    base.Log.LogMessageFromResources(MessageImportance.Low, "MSBuild.SkippingRemainingProjects", new object[0]);
                    break;
                }
                if (File.Exists(path) || (this.skipNonexistentProjects == SkipNonexistentProjectsBehavior.Build))
                {
                    if (Microsoft.Build.Shared.FileUtilities.IsVCProjFilename(path))
                    {
                        base.Log.LogErrorWithCodeFromResources("MSBuild.ProjectUpgradeNeededToVcxProj", new object[] { item.ItemSpec });
                        success = false;
                    }
                    else if (!this.buildInParallel)
                    {
                        projects[0] = item;
                        if (!ExecuteTargets(projects, hashtable, undefineProperties, targetLists, this.StopOnFirstFailure, this.RebaseOutputs, base.BuildEngine3, base.Log, this.targetOutputs, this.useResultsCache, this.unloadProjectsOnCompletion, this.ToolsVersion))
                        {
                            success = false;
                        }
                    }
                    else
                    {
                        skipProjects[i] = false;
                    }
                }
                else if (this.skipNonexistentProjects == SkipNonexistentProjectsBehavior.Skip)
                {
                    base.Log.LogMessageFromResources(MessageImportance.High, "MSBuild.ProjectFileNotFoundMessage", new object[] { item.ItemSpec });
                }
                else
                {
                    Microsoft.Build.Shared.ErrorUtilities.VerifyThrow(this.skipNonexistentProjects == SkipNonexistentProjectsBehavior.Error, "skipNonexistentProjects has unexpected value {0}", this.skipNonexistentProjects);
                    base.Log.LogErrorWithCodeFromResources("MSBuild.ProjectFileNotFound", new object[] { item.ItemSpec });
                    success = false;
                }
            }
            if (this.buildInParallel)
            {
                success = this.BuildProjectsInParallel(hashtable, undefineProperties, targetLists, success, skipProjects);
            }
            return success;
        }

        internal static bool ExecuteTargets(ITaskItem[] projects, Hashtable propertiesTable, string[] undefineProperties, ArrayList targetLists, bool stopOnFirstFailure, bool rebaseOutputs, IBuildEngine3 buildEngine, TaskLoggingHelper log, ArrayList targetOutputs, bool useResultsCache, bool unloadProjectsOnCompletion, string toolsVersion)
        {
            bool flag = true;
            string[] strArray = new string[projects.Length];
            string[] projectFileNames = new string[projects.Length];
            string[] strArray3 = new string[projects.Length];
            IList<IDictionary<string, ITaskItem[]>> targetOutputsPerProject = null;
            IDictionary[] globalProperties = new IDictionary[projects.Length];
            List<string>[] removeGlobalProperties = new List<string>[projects.Length];
            for (int i = 0; i < projectFileNames.Length; i++)
            {
                projectFileNames[i] = null;
                globalProperties[i] = propertiesTable;
                if (projects[i] != null)
                {
                    string path = Microsoft.Build.Shared.FileUtilities.AttemptToShortenPath(projects[i].ItemSpec);
                    strArray[i] = Path.GetDirectoryName(path);
                    projectFileNames[i] = projects[i].ItemSpec;
                    strArray3[i] = toolsVersion;
                    if (!string.IsNullOrEmpty(projects[i].GetMetadata("Properties")))
                    {
                        Hashtable hashtable;
                        if (!PropertyParser.GetTableWithEscaping(log, Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("General.OverridingProperties", new object[] { projectFileNames[i] }), "Properties", projects[i].GetMetadata("Properties").Split(new char[] { ';' }), out hashtable))
                        {
                            return false;
                        }
                        globalProperties[i] = hashtable;
                    }
                    if (undefineProperties != null)
                    {
                        removeGlobalProperties[i] = new List<string>(undefineProperties);
                    }
                    string metadata = projects[i].GetMetadata("UndefineProperties");
                    if (!string.IsNullOrEmpty(metadata))
                    {
                        string[] strArray4 = metadata.Split(new char[] { ';' });
                        if (removeGlobalProperties[i] == null)
                        {
                            removeGlobalProperties[i] = new List<string>(strArray4.Length);
                        }
                        if ((log != null) && (strArray4.Length > 0))
                        {
                            log.LogMessageFromResources(MessageImportance.Low, "General.ProjectUndefineProperties", new object[] { projectFileNames[i] });
                            foreach (string str3 in strArray4)
                            {
                                removeGlobalProperties[i].Add(str3);
                                log.LogMessageFromText(string.Format(CultureInfo.InvariantCulture, "  {0}", new object[] { str3 }), MessageImportance.Low);
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(projects[i].GetMetadata("AdditionalProperties")))
                    {
                        Hashtable hashtable2;
                        if (!PropertyParser.GetTableWithEscaping(log, Microsoft.Build.Shared.ResourceUtilities.FormatResourceString("General.AdditionalProperties", new object[] { projectFileNames[i] }), "AdditionalProperties", projects[i].GetMetadata("AdditionalProperties").Split(new char[] { ';' }), out hashtable2))
                        {
                            return false;
                        }
                        Hashtable hashtable3 = new Hashtable(StringComparer.OrdinalIgnoreCase);
                        if (globalProperties[i] != null)
                        {
                            foreach (DictionaryEntry entry in globalProperties[i])
                            {
                                if (!hashtable2.Contains(entry.Key))
                                {
                                    hashtable3.Add(entry.Key, entry.Value);
                                }
                            }
                        }
                        foreach (DictionaryEntry entry2 in hashtable2)
                        {
                            hashtable3.Add(entry2.Key, entry2.Value);
                        }
                        globalProperties[i] = hashtable3;
                    }
                    if (!string.IsNullOrEmpty(projects[i].GetMetadata("ToolsVersion")))
                    {
                        strArray3[i] = projects[i].GetMetadata("ToolsVersion");
                    }
                }
            }
            foreach (string[] strArray5 in targetLists)
            {
                if (stopOnFirstFailure && !flag)
                {
                    log.LogMessageFromResources(MessageImportance.Low, "MSBuild.SkippingRemainingTargets", new object[0]);
                    return flag;
                }
                bool flag2 = true;
                BuildEngineResult result = buildEngine.BuildProjectFilesInParallel(projectFileNames, strArray5, globalProperties, removeGlobalProperties, strArray3, true);
                flag2 = result.Result;
                targetOutputsPerProject = result.TargetOutputsPerProject;
                flag = flag && flag2;
                if (flag2)
                {
                    for (int j = 0; j < projects.Length; j++)
                    {
                        IEnumerable enumerable = (strArray5 != null) ? ((IEnumerable) strArray5) : ((IEnumerable) targetOutputsPerProject[j].Keys);
                        foreach (string str4 in enumerable)
                        {
                            if (targetOutputsPerProject[j].ContainsKey(str4))
                            {
                                ITaskItem[] c = targetOutputsPerProject[j][str4];
                                foreach (ITaskItem item in c)
                                {
                                    if (projects[j] != null)
                                    {
                                        if (rebaseOutputs)
                                        {
                                            try
                                            {
                                                item.ItemSpec = Path.Combine(strArray[j], item.ItemSpec);
                                            }
                                            catch (ArgumentException exception)
                                            {
                                                log.LogWarningWithCodeFromResources(null, projects[j].ItemSpec, 0, 0, 0, 0, "MSBuild.CannotRebaseOutputItemPath", new object[] { item.ItemSpec, exception.Message });
                                            }
                                        }
                                        projects[j].CopyMetadataTo(item);
                                        if (string.IsNullOrEmpty(item.GetMetadata("MSBuildSourceProjectFile")))
                                        {
                                            item.SetMetadata("MSBuildSourceProjectFile", projects[j].GetMetadata("FullPath"));
                                        }
                                    }
                                    if (string.IsNullOrEmpty(item.GetMetadata("MSBuildSourceTargetName")))
                                    {
                                        item.SetMetadata("MSBuildSourceTargetName", str4);
                                    }
                                }
                                targetOutputs.AddRange(c);
                            }
                        }
                    }
                }
            }
            return flag;
        }

        private void ExpandAllTargetsAndProperties()
        {
            List<string> list = new List<string>();
            List<string> list2 = new List<string>();
            if (this.Properties != null)
            {
                for (int i = 0; i < this.Properties.Length; i++)
                {
                    foreach (string str in this.Properties[i].Split(this.TargetAndPropertyListSeparators, StringSplitOptions.RemoveEmptyEntries))
                    {
                        list.Add(str);
                    }
                }
                this.Properties = list.ToArray();
            }
            if (this.Targets != null)
            {
                for (int j = 0; j < this.Targets.Length; j++)
                {
                    foreach (string str2 in this.Targets[j].Split(this.TargetAndPropertyListSeparators, StringSplitOptions.RemoveEmptyEntries))
                    {
                        list2.Add(str2);
                    }
                }
                this.Targets = list2.ToArray();
            }
        }

        public bool BuildInParallel
        {
            get
            {
                return this.buildInParallel;
            }
            set
            {
                this.buildInParallel = value;
            }
        }

        [Required]
        public ITaskItem[] Projects
        {
            get
            {
                return this.projects;
            }
            set
            {
                this.projects = value;
            }
        }

        public string[] Properties
        {
            get
            {
                return this.properties;
            }
            set
            {
                this.properties = value;
            }
        }

        public bool RebaseOutputs
        {
            get
            {
                return this.rebaseOutputs;
            }
            set
            {
                this.rebaseOutputs = value;
            }
        }

        public string RemoveProperties
        {
            get
            {
                return this.undefineProperties;
            }
            set
            {
                this.undefineProperties = value;
            }
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

        public string SkipNonexistentProjects
        {
            get
            {
                switch (this.skipNonexistentProjects)
                {
                    case SkipNonexistentProjectsBehavior.Skip:
                        return "True";

                    case SkipNonexistentProjectsBehavior.Error:
                        return "False";

                    case SkipNonexistentProjectsBehavior.Build:
                        return "Build";
                }
                Microsoft.Build.Shared.ErrorUtilities.ThrowInternalError("Unexpected case {0}", new object[] { this.skipNonexistentProjects });
                Microsoft.Build.Shared.ErrorUtilities.ThrowInternalErrorUnreachable();
                return null;
            }
            set
            {
                if (string.Equals("Build", value, StringComparison.OrdinalIgnoreCase))
                {
                    this.skipNonexistentProjects = SkipNonexistentProjectsBehavior.Build;
                }
                else
                {
                    Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgument(ConversionUtilities.CanConvertStringToBool(value), "MSBuild.InvalidSkipNonexistentProjectValue");
                    if (ConversionUtilities.ConvertStringToBool(value))
                    {
                        this.skipNonexistentProjects = SkipNonexistentProjectsBehavior.Skip;
                    }
                    else
                    {
                        this.skipNonexistentProjects = SkipNonexistentProjectsBehavior.Error;
                    }
                }
            }
        }

        public bool StopOnFirstFailure
        {
            get
            {
                return this.stopOnFirstFailure;
            }
            set
            {
                this.stopOnFirstFailure = value;
            }
        }

        public string[] TargetAndPropertyListSeparators
        {
            get
            {
                return this.targetAndPropertyListSeparators;
            }
            set
            {
                this.targetAndPropertyListSeparators = value;
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

        public string ToolsVersion
        {
            get
            {
                return this.toolsVersion;
            }
            set
            {
                this.toolsVersion = value;
            }
        }

        public bool UnloadProjectsOnCompletion
        {
            get
            {
                return this.unloadProjectsOnCompletion;
            }
            set
            {
                this.unloadProjectsOnCompletion = value;
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

        private enum SkipNonexistentProjectsBehavior
        {
            Skip,
            Error,
            Build
        }
    }
}

