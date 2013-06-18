namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;

    internal abstract class Resolver
    {
        protected bool compareProcessorArchitecture;
        protected Microsoft.Build.Shared.FileExists fileExists;
        protected GetAssemblyName getAssemblyName;
        protected GetAssemblyRuntimeVersion getRuntimeVersion;
        protected string searchPathElement;
        protected Version targetedRuntimeVersion;
        protected ProcessorArchitecture targetProcessorArchitecture;

        protected Resolver(string searchPathElement, GetAssemblyName getAssemblyName, Microsoft.Build.Shared.FileExists fileExists, GetAssemblyRuntimeVersion getRuntimeVersion, Version targetedRuntimeVesion, ProcessorArchitecture targetedProcessorArchitecture, bool compareProcessorArchitecture)
        {
            this.searchPathElement = searchPathElement;
            this.getAssemblyName = getAssemblyName;
            this.fileExists = fileExists;
            this.getRuntimeVersion = getRuntimeVersion;
            this.targetedRuntimeVersion = targetedRuntimeVesion;
            this.targetProcessorArchitecture = targetedProcessorArchitecture;
            this.compareProcessorArchitecture = compareProcessorArchitecture;
        }

        protected bool FileMatchesAssemblyName(AssemblyNameExtension assemblyName, bool isPrimaryProjectReference, bool wantSpecificVersion, bool allowMismatchBetweenFusionNameAndFileName, string pathToCandidateAssembly, ResolutionSearchLocation searchLocation)
        {
            searchLocation.FileNameAttempted = pathToCandidateAssembly;
            if (!allowMismatchBetweenFusionNameAndFileName)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pathToCandidateAssembly);
                if (string.Compare(assemblyName.Name, fileNameWithoutExtension, StringComparison.CurrentCultureIgnoreCase) != 0)
                {
                    if (searchLocation != null)
                    {
                        if (fileNameWithoutExtension.Length > 0)
                        {
                            searchLocation.AssemblyName = new AssemblyNameExtension(fileNameWithoutExtension);
                            searchLocation.Reason = NoMatchReason.FusionNamesDidNotMatch;
                        }
                        else
                        {
                            searchLocation.Reason = NoMatchReason.TargetHadNoFusionName;
                        }
                    }
                    return false;
                }
            }
            bool flag = (assemblyName != null) && assemblyName.IsSimpleName;
            if (this.fileExists(pathToCandidateAssembly))
            {
                if (!this.compareProcessorArchitecture)
                {
                    if (((assemblyName == null) && isPrimaryProjectReference) && !wantSpecificVersion)
                    {
                        return true;
                    }
                    if ((isPrimaryProjectReference && !wantSpecificVersion) && flag)
                    {
                        return true;
                    }
                }
                AssemblyNameExtension that = null;
                try
                {
                    that = this.getAssemblyName(pathToCandidateAssembly);
                }
                catch (FileLoadException)
                {
                }
                if (searchLocation != null)
                {
                    searchLocation.AssemblyName = that;
                }
                if (that != null)
                {
                    if (((this.compareProcessorArchitecture && (that.AssemblyName.ProcessorArchitecture != this.targetProcessorArchitecture)) && ((this.targetProcessorArchitecture != ProcessorArchitecture.None) && (that.AssemblyName.ProcessorArchitecture != ProcessorArchitecture.None))) && ((this.targetProcessorArchitecture != ProcessorArchitecture.MSIL) && (that.AssemblyName.ProcessorArchitecture != ProcessorArchitecture.MSIL)))
                    {
                        searchLocation.Reason = NoMatchReason.ProcessorArchitectureDoesNotMatch;
                        return false;
                    }
                    bool flag2 = (wantSpecificVersion && (assemblyName != null)) && assemblyName.Equals(that);
                    bool flag3 = (!wantSpecificVersion && (assemblyName != null)) && assemblyName.PartialNameCompare(that);
                    if (flag2 || flag3)
                    {
                        return true;
                    }
                    if (searchLocation != null)
                    {
                        searchLocation.Reason = NoMatchReason.FusionNamesDidNotMatch;
                    }
                }
                else if (searchLocation != null)
                {
                    searchLocation.Reason = NoMatchReason.TargetHadNoFusionName;
                }
            }
            else if (searchLocation != null)
            {
                searchLocation.Reason = NoMatchReason.FileNotFound;
            }
            return false;
        }

        public abstract bool Resolve(AssemblyNameExtension assemblyName, string rawFileNameCandidate, bool isPrimaryProjectReference, bool wantSpecificVersion, string[] executableExtensions, string hintPath, string assemblyFolderKey, ArrayList assembliesConsideredAndRejected, out string foundPath, out bool userRequestedSpecificFile);
        protected bool ResolveAsFile(string fullPath, AssemblyNameExtension assemblyName, bool isPrimaryProjectReference, bool wantSpecificVersion, bool allowMismatchBetweenFusionNameAndFileName, ArrayList assembliesConsideredAndRejected)
        {
            ResolutionSearchLocation searchLocation = null;
            if (assembliesConsideredAndRejected != null)
            {
                searchLocation = new ResolutionSearchLocation {
                    FileNameAttempted = fullPath,
                    SearchPath = this.searchPathElement
                };
            }
            if (this.FileMatchesAssemblyName(assemblyName, isPrimaryProjectReference, wantSpecificVersion, allowMismatchBetweenFusionNameAndFileName, fullPath, searchLocation))
            {
                return true;
            }
            if (assembliesConsideredAndRejected != null)
            {
                assembliesConsideredAndRejected.Add(searchLocation);
            }
            return false;
        }

        protected string ResolveFromDirectory(AssemblyNameExtension assemblyName, bool isPrimaryProjectReference, bool wantSpecificVersion, string[] executableExtensions, string directory, ArrayList assembliesConsideredAndRejected)
        {
            if (assemblyName == null)
            {
                return null;
            }
            string path = null;
            if (directory != null)
            {
                string str3;
                string name = assemblyName.Name;
                for (int i = 0; i < executableExtensions.Length; i++)
                {
                    string str4 = name + executableExtensions[i];
                    try
                    {
                        str3 = Path.Combine(directory, str4);
                    }
                    catch (Exception exception)
                    {
                        if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception))
                        {
                            throw;
                        }
                        throw new InvalidParameterValueException("SearchPaths", directory + (directory.EndsWith(@"\", StringComparison.OrdinalIgnoreCase) ? string.Empty : @"\") + str4, exception.Message);
                    }
                    if (this.ResolveAsFile(str3, assemblyName, isPrimaryProjectReference, wantSpecificVersion, false, assembliesConsideredAndRejected))
                    {
                        path = str3;
                    }
                }
                if (path != null)
                {
                    if ((this.targetProcessorArchitecture != ProcessorArchitecture.MSIL) && (this.targetProcessorArchitecture != ProcessorArchitecture.None))
                    {
                        return path;
                    }
                    AssemblyNameExtension extension = this.getAssemblyName(path);
                    if ((extension != null) && ((extension.AssemblyName.ProcessorArchitecture == ProcessorArchitecture.MSIL) || (extension.AssemblyName.ProcessorArchitecture == ProcessorArchitecture.None)))
                    {
                        return path;
                    }
                }
                string strB = Path.GetExtension(name);
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(name);
                if (((strB != null) && (strB.Length > 0)) && ((fileNameWithoutExtension != null) && (fileNameWithoutExtension.Length > 0)))
                {
                    for (int j = 0; j < executableExtensions.Length; j++)
                    {
                        if (string.Compare(executableExtensions[j], strB, StringComparison.CurrentCultureIgnoreCase) == 0)
                        {
                            str3 = Path.Combine(directory, name);
                            AssemblyNameExtension extension2 = new AssemblyNameExtension(fileNameWithoutExtension);
                            if (this.ResolveAsFile(str3, extension2, isPrimaryProjectReference, wantSpecificVersion, false, assembliesConsideredAndRejected))
                            {
                                path = str3;
                            }
                        }
                    }
                }
                if (path != null)
                {
                    if ((this.targetProcessorArchitecture != ProcessorArchitecture.MSIL) && (this.targetProcessorArchitecture != ProcessorArchitecture.None))
                    {
                        return path;
                    }
                    AssemblyNameExtension extension3 = this.getAssemblyName(path);
                    if ((extension3 == null) || ((extension3.AssemblyName.ProcessorArchitecture != ProcessorArchitecture.MSIL) && (extension3.AssemblyName.ProcessorArchitecture != ProcessorArchitecture.MSIL)))
                    {
                        return path;
                    }
                }
            }
            return path;
        }

        public string SearchPath
        {
            get
            {
                return this.searchPathElement;
            }
        }
    }
}

