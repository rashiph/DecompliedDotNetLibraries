namespace Microsoft.Build.Tasks
{
    using System;
    using System.Collections;
    using System.Xml;

    internal sealed class RuntimeSection
    {
        private ArrayList dependentAssemblies = new ArrayList();

        internal void Read(XmlTextReader reader)
        {
            while (reader.Read())
            {
                if ((reader.NodeType == XmlNodeType.EndElement) && AppConfig.StringEquals(reader.Name, "runtime"))
                {
                    return;
                }
                if ((reader.NodeType == XmlNodeType.Element) && AppConfig.StringEquals(reader.Name, "dependentAssembly"))
                {
                    DependentAssembly assembly = new DependentAssembly();
                    assembly.Read(reader);
                    if (assembly.PartialAssemblyName != null)
                    {
                        this.dependentAssemblies.Add(assembly);
                    }
                }
            }
        }

        internal DependentAssembly[] DependentAssemblies
        {
            get
            {
                return (DependentAssembly[]) this.dependentAssemblies.ToArray(typeof(DependentAssembly));
            }
        }
    }
}

