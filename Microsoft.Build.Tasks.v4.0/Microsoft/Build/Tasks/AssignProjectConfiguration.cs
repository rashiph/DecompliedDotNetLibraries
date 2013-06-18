namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Xml;

    public class AssignProjectConfiguration : ResolveProjectBase
    {
        private ITaskItem[] assignedProjects;
        private const string attrConfiguration = "Configuration";
        private const string attrFullConfiguration = "FullConfiguration";
        private const string attrPlatform = "Platform";
        private const string attrSetConfiguration = "SetConfiguration";
        private const string attrSetPlatform = "SetPlatform";
        private const string buildProjectInSolutionAttribute = "BuildProjectInSolution";
        private const string buildReferenceMetadataName = "BuildReference";
        private static readonly char[] configPlatformSeparator = new char[] { '|' };
        private string currentProjectConfiguration;
        private string currentProjectPlatform;
        private IDictionary<string, string> defaultToVcxMap;
        private string defaultToVcxPlatformMapping;
        private bool mappingsPopulated;
        private bool onlyReferenceAndBuildProjectsEnabledInSolutionConfiguration;
        private string outputType;
        private const string referenceOutputAssemblyMetadataName = "ReferenceOutputAssembly";
        private bool resolveConfigurationPlatformUsingMappings;
        private bool shouldUnsetParentConfigurationAndPlatform;
        private string solutionConfigurationContents;
        private ITaskItem[] unassignedProjects;
        private IDictionary<string, string> vcxToDefaultMap;
        private string vcxToDefaultPlatformMapping;

        public override bool Execute()
        {
            try
            {
                if (!base.VerifyProjectReferenceItems(base.ProjectReferences, true))
                {
                    return false;
                }
                ArrayList list = new ArrayList(base.ProjectReferences.GetLength(0));
                ArrayList list2 = new ArrayList(base.ProjectReferences.GetLength(0));
                if (!string.IsNullOrEmpty(this.SolutionConfigurationContents))
                {
                    base.CacheProjectElementsFromXml(this.SolutionConfigurationContents);
                }
                foreach (ITaskItem item in base.ProjectReferences)
                {
                    ITaskItem item2;
                    if (this.ResolveProject(item, out item2))
                    {
                        list.Add(item2);
                        base.Log.LogMessageFromResources(MessageImportance.Low, "AssignProjectConfiguration.ProjectConfigurationResolutionSuccess", new object[] { item.ItemSpec, item2.GetMetadata("FullConfiguration") });
                    }
                    else
                    {
                        if (this.ShouldUnsetParentConfigurationAndPlatform)
                        {
                            string metadata = item.GetMetadata("GlobalPropertiesToRemove");
                            if (!string.IsNullOrEmpty(metadata))
                            {
                                metadata = metadata + ";";
                            }
                            if (item is ITaskItem2)
                            {
                                ((ITaskItem2) item).SetMetadataValueLiteral("GlobalPropertiesToRemove", metadata + "Configuration;Platform");
                            }
                            else
                            {
                                item.SetMetadata("GlobalPropertiesToRemove", Microsoft.Build.Shared.EscapingUtilities.Escape(metadata + "Configuration;Platform"));
                            }
                        }
                        list2.Add(item);
                        base.Log.LogMessageFromResources(MessageImportance.Low, "AssignProjectConfiguration.ProjectConfigurationUnresolved", new object[] { item.ItemSpec });
                    }
                }
                this.AssignedProjects = (ITaskItem[]) list.ToArray(typeof(ITaskItem));
                this.UnassignedProjects = (ITaskItem[]) list2.ToArray(typeof(ITaskItem));
            }
            catch (XmlException exception)
            {
                base.Log.LogErrorWithCodeFromResources("General.ErrorExecutingTask", new object[] { base.GetType().Name, exception.Message });
                return false;
            }
            return true;
        }

        private void PopulateMappingDictionary(IDictionary<string, string> map, string mappingList)
        {
            foreach (string str in mappingList.Split(new char[] { ';' }))
            {
                string[] strArray2 = str.Split(new char[] { '=' });
                if ((strArray2 == null) || (strArray2.Length != 2))
                {
                    base.Log.LogErrorFromResources("AssignProjectConfiguration.IllegalMappingString", new object[] { str.Trim(), mappingList });
                }
                else
                {
                    map.Add(strArray2[0], strArray2[1]);
                }
            }
        }

        internal bool ResolveProject(ITaskItem projectRef, out ITaskItem resolvedProjectWithConfiguration)
        {
            XmlElement projectConfigurationElement = null;
            string innerText = null;
            if (!string.IsNullOrEmpty(this.SolutionConfigurationContents))
            {
                projectConfigurationElement = base.GetProjectElement(projectRef);
                if (projectConfigurationElement != null)
                {
                    innerText = projectConfigurationElement.InnerText;
                }
            }
            if ((innerText == null) && this.ResolveConfigurationPlatformUsingMappings)
            {
                if (!this.mappingsPopulated)
                {
                    this.SetupDefaultPlatformMappings();
                }
                string str2 = null;
                if (string.Equals(projectRef.GetMetadata("Extension"), ".vcxproj", StringComparison.OrdinalIgnoreCase))
                {
                    if (this.defaultToVcxMap.TryGetValue(this.CurrentProjectPlatform, out str2))
                    {
                        innerText = this.CurrentProjectConfiguration + configPlatformSeparator[0] + str2;
                    }
                }
                else if (this.vcxToDefaultMap.TryGetValue(this.CurrentProjectPlatform, out str2))
                {
                    innerText = this.CurrentProjectConfiguration + configPlatformSeparator[0] + str2;
                }
            }
            SetBuildInProjectAndReferenceOutputAssemblyMetadata(this.onlyReferenceAndBuildProjectsEnabledInSolutionConfiguration, projectRef, projectConfigurationElement);
            if ((innerText != null) && !string.IsNullOrEmpty(innerText))
            {
                resolvedProjectWithConfiguration = projectRef;
                resolvedProjectWithConfiguration.SetMetadata("FullConfiguration", innerText);
                string[] strArray = innerText.Split(configPlatformSeparator);
                resolvedProjectWithConfiguration.SetMetadata("SetConfiguration", "Configuration=" + strArray[0]);
                resolvedProjectWithConfiguration.SetMetadata("Configuration", strArray[0]);
                if (strArray.Length > 1)
                {
                    resolvedProjectWithConfiguration.SetMetadata("SetPlatform", "Platform=" + strArray[1]);
                    resolvedProjectWithConfiguration.SetMetadata("Platform", strArray[1]);
                }
                else
                {
                    resolvedProjectWithConfiguration.SetMetadata("SetPlatform", "Platform=");
                }
                return true;
            }
            resolvedProjectWithConfiguration = null;
            return false;
        }

        internal static void SetBuildInProjectAndReferenceOutputAssemblyMetadata(bool onlyReferenceAndBuildProjectsEnabledInSolutionConfiguration, ITaskItem resolvedProjectWithConfiguration, XmlElement projectConfigurationElement)
        {
            if (((projectConfigurationElement != null) && (resolvedProjectWithConfiguration != null)) && onlyReferenceAndBuildProjectsEnabledInSolutionConfiguration)
            {
                bool result = false;
                if (bool.TryParse(projectConfigurationElement.GetAttribute("BuildProjectInSolution"), out result) && !result)
                {
                    string metadata = resolvedProjectWithConfiguration.GetMetadata("BuildReference");
                    string str3 = resolvedProjectWithConfiguration.GetMetadata("ReferenceOutputAssembly");
                    if (metadata.Length == 0)
                    {
                        resolvedProjectWithConfiguration.SetMetadata("BuildReference", "false");
                    }
                    if (str3.Length == 0)
                    {
                        resolvedProjectWithConfiguration.SetMetadata("ReferenceOutputAssembly", "false");
                    }
                }
            }
        }

        private void SetupDefaultPlatformMappings()
        {
            this.vcxToDefaultMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            this.defaultToVcxMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (!string.IsNullOrEmpty(this.VcxToDefaultPlatformMapping))
            {
                this.PopulateMappingDictionary(this.vcxToDefaultMap, this.VcxToDefaultPlatformMapping);
            }
            if (!string.IsNullOrEmpty(this.DefaultToVcxPlatformMapping))
            {
                this.PopulateMappingDictionary(this.defaultToVcxMap, this.DefaultToVcxPlatformMapping);
            }
            this.mappingsPopulated = true;
        }

        [Output]
        public ITaskItem[] AssignedProjects
        {
            get
            {
                return this.assignedProjects;
            }
            set
            {
                this.assignedProjects = value;
            }
        }

        public string CurrentProjectConfiguration
        {
            get
            {
                return this.currentProjectConfiguration;
            }
            set
            {
                this.currentProjectConfiguration = value;
            }
        }

        public string CurrentProjectPlatform
        {
            get
            {
                return this.currentProjectPlatform;
            }
            set
            {
                this.currentProjectPlatform = value;
            }
        }

        public string DefaultToVcxPlatformMapping
        {
            get
            {
                if (this.defaultToVcxPlatformMapping == null)
                {
                    this.defaultToVcxPlatformMapping = "AnyCPU=Win32;X86=Win32;X64=X64;Itanium=Itanium";
                }
                return this.defaultToVcxPlatformMapping;
            }
            set
            {
                this.defaultToVcxPlatformMapping = value;
                if ((this.defaultToVcxPlatformMapping != null) && (this.defaultToVcxPlatformMapping.Length == 0))
                {
                    this.defaultToVcxPlatformMapping = null;
                }
            }
        }

        public bool OnlyReferenceAndBuildProjectsEnabledInSolutionConfiguration
        {
            get
            {
                return this.onlyReferenceAndBuildProjectsEnabledInSolutionConfiguration;
            }
            set
            {
                this.onlyReferenceAndBuildProjectsEnabledInSolutionConfiguration = value;
            }
        }

        public string OutputType
        {
            get
            {
                return this.outputType;
            }
            set
            {
                this.outputType = value;
            }
        }

        public bool ResolveConfigurationPlatformUsingMappings
        {
            get
            {
                return this.resolveConfigurationPlatformUsingMappings;
            }
            set
            {
                this.resolveConfigurationPlatformUsingMappings = value;
            }
        }

        public bool ShouldUnsetParentConfigurationAndPlatform
        {
            get
            {
                return this.shouldUnsetParentConfigurationAndPlatform;
            }
            set
            {
                this.shouldUnsetParentConfigurationAndPlatform = value;
            }
        }

        public string SolutionConfigurationContents
        {
            get
            {
                return this.solutionConfigurationContents;
            }
            set
            {
                this.solutionConfigurationContents = value;
            }
        }

        [Output]
        public ITaskItem[] UnassignedProjects
        {
            get
            {
                return this.unassignedProjects;
            }
            set
            {
                this.unassignedProjects = value;
            }
        }

        public string VcxToDefaultPlatformMapping
        {
            get
            {
                if (this.vcxToDefaultPlatformMapping == null)
                {
                    if (string.Equals("Library", this.OutputType, StringComparison.OrdinalIgnoreCase))
                    {
                        this.vcxToDefaultPlatformMapping = "Win32=AnyCPU;X64=X64;Itanium=Itanium";
                    }
                    else
                    {
                        this.vcxToDefaultPlatformMapping = "Win32=X86;X64=X64;Itanium=Itanium";
                    }
                }
                return this.vcxToDefaultPlatformMapping;
            }
            set
            {
                this.vcxToDefaultPlatformMapping = value;
                if ((this.vcxToDefaultPlatformMapping != null) && (this.vcxToDefaultPlatformMapping.Length == 0))
                {
                    this.vcxToDefaultPlatformMapping = null;
                }
            }
        }
    }
}

