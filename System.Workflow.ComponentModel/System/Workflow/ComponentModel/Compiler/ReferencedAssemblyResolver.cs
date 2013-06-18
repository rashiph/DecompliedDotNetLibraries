namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.Collections.Specialized;
    using System.Reflection;

    internal sealed class ReferencedAssemblyResolver
    {
        private Assembly localAssembly;
        private StringCollection referencedAssemblies = new StringCollection();
        private bool resolving;

        public ReferencedAssemblyResolver(StringCollection referencedAssemblies, Assembly localAssembly)
        {
            this.referencedAssemblies = referencedAssemblies;
            this.localAssembly = localAssembly;
        }

        private Assembly ResolveAssembly(string name)
        {
            if (!this.resolving)
            {
                if ((this.localAssembly != null) && (name == this.localAssembly.FullName))
                {
                    return this.localAssembly;
                }
                try
                {
                    this.resolving = true;
                    AssemblyName thatName = new AssemblyName(name);
                    foreach (string str in this.referencedAssemblies)
                    {
                        try
                        {
                            AssemblyName assemblyName = AssemblyName.GetAssemblyName(str);
                            if ((assemblyName != null) && ParseHelpers.AssemblyNameEquals(assemblyName, thatName))
                            {
                                try
                                {
                                    return Assembly.Load(assemblyName);
                                }
                                catch
                                {
                                    return Assembly.LoadFrom(str);
                                }
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                finally
                {
                    this.resolving = false;
                }
            }
            return null;
        }

        public Assembly ResolveEventHandler(object sender, ResolveEventArgs args)
        {
            return this.ResolveAssembly(args.Name);
        }

        internal void SetLocalAssembly(Assembly localAsm)
        {
            this.localAssembly = localAsm;
        }
    }
}

