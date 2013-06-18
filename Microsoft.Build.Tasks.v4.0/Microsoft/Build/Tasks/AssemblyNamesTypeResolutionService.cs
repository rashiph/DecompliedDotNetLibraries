namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Reflection;

    internal class AssemblyNamesTypeResolutionService : ITypeResolutionService
    {
        private Hashtable cachedAssemblies;
        private Hashtable cachedTypes = new Hashtable();
        private ITaskItem[] referencePaths;

        internal AssemblyNamesTypeResolutionService(ITaskItem[] referencePaths)
        {
            this.referencePaths = referencePaths;
        }

        public Assembly GetAssembly(AssemblyName name)
        {
            throw new NotSupportedException();
        }

        public Assembly GetAssembly(AssemblyName name, bool throwOnError)
        {
            throw new NotSupportedException();
        }

        private Assembly GetAssemblyByPath(string pathToAssembly, bool throwOnError)
        {
            if (this.cachedAssemblies == null)
            {
                this.cachedAssemblies = new Hashtable();
            }
            if (!this.cachedAssemblies.Contains(pathToAssembly))
            {
                try
                {
                    this.cachedAssemblies[pathToAssembly] = Assembly.UnsafeLoadFrom(pathToAssembly);
                }
                catch
                {
                    if (throwOnError)
                    {
                        throw;
                    }
                }
            }
            return (Assembly) this.cachedAssemblies[pathToAssembly];
        }

        public string GetPathOfAssembly(AssemblyName name)
        {
            throw new NotSupportedException();
        }

        public Type GetType(string name)
        {
            return this.GetType(name, true);
        }

        public Type GetType(string name, bool throwOnError)
        {
            return this.GetType(name, throwOnError, false);
        }

        public Type GetType(string name, bool throwOnError, bool ignoreCase)
        {
            Type type = (Type) this.cachedTypes[name];
            if (this.cachedTypes.Contains(name))
            {
                return type;
            }
            Type type2 = Type.GetType(name, false, ignoreCase);
            if ((type2 == null) && (this.referencePaths != null))
            {
                foreach (ITaskItem item in this.referencePaths)
                {
                    Assembly assemblyByPath = this.GetAssemblyByPath(item.ItemSpec, throwOnError);
                    if (assemblyByPath != null)
                    {
                        type2 = assemblyByPath.GetType(name, false, ignoreCase);
                        if (type2 == null)
                        {
                            int index = name.IndexOf(",", StringComparison.Ordinal);
                            if (index != -1)
                            {
                                string str = name.Substring(0, index);
                                type2 = assemblyByPath.GetType(str, false, ignoreCase);
                            }
                        }
                        if (type2 != null)
                        {
                            break;
                        }
                    }
                }
            }
            if ((type2 == null) && throwOnError)
            {
                Microsoft.Build.Shared.ErrorUtilities.VerifyThrowArgument(false, "GenerateResource.CouldNotLoadType", name);
            }
            this.cachedTypes[name] = type2;
            return type2;
        }

        public void ReferenceAssembly(AssemblyName name)
        {
            throw new NotSupportedException();
        }
    }
}

