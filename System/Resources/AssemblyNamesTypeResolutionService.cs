namespace System.Resources
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Windows.Forms;

    internal class AssemblyNamesTypeResolutionService : ITypeResolutionService
    {
        private Hashtable cachedAssemblies;
        private Hashtable cachedTypes;
        private AssemblyName[] names;
        private static string NetFrameworkPath = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), @"Microsoft.Net\Framework");

        internal AssemblyNamesTypeResolutionService(AssemblyName[] names)
        {
            this.names = names;
        }

        public Assembly GetAssembly(AssemblyName name)
        {
            return this.GetAssembly(name, true);
        }

        public Assembly GetAssembly(AssemblyName name, bool throwOnError)
        {
            Assembly assembly = null;
            if (this.cachedAssemblies == null)
            {
                this.cachedAssemblies = new Hashtable();
            }
            if (this.cachedAssemblies.Contains(name))
            {
                assembly = this.cachedAssemblies[name] as Assembly;
            }
            if (assembly == null)
            {
                assembly = Assembly.LoadWithPartialName(name.FullName);
                if (assembly != null)
                {
                    this.cachedAssemblies[name] = assembly;
                    return assembly;
                }
                if (this.names == null)
                {
                    return assembly;
                }
                for (int i = 0; i < this.names.Length; i++)
                {
                    if (name.Equals(this.names[i]))
                    {
                        try
                        {
                            assembly = Assembly.LoadFrom(this.GetPathOfAssembly(name));
                            if (assembly != null)
                            {
                                this.cachedAssemblies[name] = assembly;
                            }
                        }
                        catch
                        {
                            if (throwOnError)
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            return assembly;
        }

        public string GetPathOfAssembly(AssemblyName name)
        {
            return name.CodeBase;
        }

        public System.Type GetType(string name)
        {
            return this.GetType(name, true);
        }

        public System.Type GetType(string name, bool throwOnError)
        {
            return this.GetType(name, throwOnError, false);
        }

        public System.Type GetType(string name, bool throwOnError, bool ignoreCase)
        {
            System.Type type = null;
            if (this.cachedTypes == null)
            {
                this.cachedTypes = new Hashtable(StringComparer.Ordinal);
            }
            if (this.cachedTypes.Contains(name))
            {
                return (this.cachedTypes[name] as System.Type);
            }
            if (name.IndexOf(',') != -1)
            {
                type = System.Type.GetType(name, false, ignoreCase);
            }
            if ((type == null) && (this.names != null))
            {
                for (int i = 0; i < this.names.Length; i++)
                {
                    Assembly assembly = this.GetAssembly(this.names[i], throwOnError);
                    type = assembly.GetType(name, false, ignoreCase);
                    if (type == null)
                    {
                        int index = name.IndexOf(",");
                        if (index != -1)
                        {
                            string str = name.Substring(0, index);
                            type = assembly.GetType(str, false, ignoreCase);
                        }
                    }
                    if (type != null)
                    {
                        break;
                    }
                }
            }
            if ((type == null) && throwOnError)
            {
                throw new ArgumentException(System.Windows.Forms.SR.GetString("InvalidResXNoType", new object[] { name }));
            }
            if ((type != null) && (type.Assembly.GlobalAssemblyCache || this.IsNetFrameworkAssembly(type.Assembly.Location)))
            {
                this.cachedTypes[name] = type;
            }
            return type;
        }

        private bool IsNetFrameworkAssembly(string assemblyPath)
        {
            return ((assemblyPath != null) && assemblyPath.StartsWith(NetFrameworkPath, StringComparison.OrdinalIgnoreCase));
        }

        public void ReferenceAssembly(AssemblyName name)
        {
            throw new NotSupportedException();
        }
    }
}

