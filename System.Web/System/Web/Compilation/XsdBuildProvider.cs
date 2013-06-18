namespace System.Web.Compilation
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Data.Design;
    using System.IO;
    using System.Reflection;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Xml;

    [BuildProviderAppliesTo(BuildProviderAppliesTo.Code)]
    internal class XsdBuildProvider : System.Web.Compilation.BuildProvider
    {
        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            string namespaceFromVirtualPath = Util.GetNamespaceFromVirtualPath(base.VirtualPathObject);
            XmlDocument document = new XmlDocument();
            using (Stream stream = base.OpenStream())
            {
                document.Load(stream);
            }
            string outerXml = document.OuterXml;
            CodeCompileUnit compileUnit = new CodeCompileUnit();
            CodeNamespace namespace2 = new CodeNamespace(namespaceFromVirtualPath);
            compileUnit.Namespaces.Add(namespace2);
            if (CompilationUtil.IsCompilerVersion35OrAbove(assemblyBuilder.CodeDomProvider.GetType()))
            {
                TypedDataSetGenerator.GenerateOption none = TypedDataSetGenerator.GenerateOption.None;
                none |= TypedDataSetGenerator.GenerateOption.HierarchicalUpdate;
                none |= TypedDataSetGenerator.GenerateOption.LinqOverTypedDatasets;
                Hashtable customDBProviders = null;
                TypedDataSetGenerator.Generate(outerXml, compileUnit, namespace2, assemblyBuilder.CodeDomProvider, customDBProviders, none);
            }
            else
            {
                TypedDataSetGenerator.Generate(outerXml, compileUnit, namespace2, assemblyBuilder.CodeDomProvider);
            }
            if (TypedDataSetGenerator.ReferencedAssemblies != null)
            {
                bool flag2 = CompilationUtil.IsCompilerVersion35(assemblyBuilder.CodeDomProvider.GetType());
                foreach (Assembly assembly in TypedDataSetGenerator.ReferencedAssemblies)
                {
                    if (flag2)
                    {
                        AssemblyName name = assembly.GetName();
                        if (name.Name == "System.Data.DataSetExtensions")
                        {
                            name.Version = new Version(3, 5, 0, 0);
                            CompilationSection.RecordAssembly(name.FullName, assembly);
                        }
                    }
                    assemblyBuilder.AddAssemblyReference(assembly);
                }
            }
            assemblyBuilder.AddCodeCompileUnit(this, compileUnit);
        }
    }
}

