namespace Microsoft.VisualBasic.Activities.XamlIntegration
{
    using Microsoft.VisualBasic.Activities;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Windows.Markup;
    using System.Xaml;
    using System.Xml.Linq;

    internal static class VisualBasicExpressionConverter
    {
        private static readonly Regex assemblyQualifiedNamespaceRegex = new Regex("clr-namespace:(?<namespace>[^;]*);assembly=(?<assembly>.*)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static bool AssemblySatisfiesReference(AssemblyName assemblyName, AssemblyName reference)
        {
            if (reference.Name != assemblyName.Name)
            {
                return false;
            }
            if ((reference.Version != null) && !reference.Version.Equals(assemblyName.Version))
            {
                return false;
            }
            if ((reference.CultureInfo != null) && !reference.CultureInfo.Equals(assemblyName.CultureInfo))
            {
                return false;
            }
            byte[] publicKeyToken = reference.GetPublicKeyToken();
            if (publicKeyToken != null)
            {
                byte[] curKeyToken = assemblyName.GetPublicKeyToken();
                if (!AssemblyNameEqualityComparer.IsSameKeyToken(publicKeyToken, curKeyToken))
                {
                    return false;
                }
            }
            return true;
        }

        public static VisualBasicSettings CollectXmlNamespacesAndAssemblies(ITypeDescriptorContext context)
        {
            IList<Assembly> referenceAssemblies = null;
            IXamlSchemaContextProvider service = context.GetService(typeof(IXamlSchemaContextProvider)) as IXamlSchemaContextProvider;
            if ((service != null) && (service.SchemaContext != null))
            {
                referenceAssemblies = service.SchemaContext.ReferenceAssemblies;
                if ((referenceAssemblies != null) && (referenceAssemblies.Count == 0))
                {
                    referenceAssemblies = null;
                }
            }
            VisualBasicSettings settings = null;
            IXamlNamespaceResolver resolver = (IXamlNamespaceResolver) context.GetService(typeof(IXamlNamespaceResolver));
            if (resolver == null)
            {
                return null;
            }
            lock (AssemblyCache.XmlnsMappings)
            {
                foreach (System.Xaml.NamespaceDeclaration declaration in resolver.GetNamespacePrefixes())
                {
                    XmlnsMapping mapping;
                    XNamespace key = XNamespace.Get(declaration.Namespace);
                    if (!AssemblyCache.XmlnsMappings.TryGetValue(key, out mapping))
                    {
                        Match match = assemblyQualifiedNamespaceRegex.Match(declaration.Namespace);
                        if (match.Success)
                        {
                            mapping.ImportReferences = new System.Collections.Generic.HashSet<VisualBasicImportReference>();
                            VisualBasicImportReference item = new VisualBasicImportReference {
                                Assembly = match.Groups["assembly"].Value,
                                Import = match.Groups["namespace"].Value,
                                Xmlns = key
                            };
                            mapping.ImportReferences.Add(item);
                        }
                        else
                        {
                            mapping.ImportReferences = new System.Collections.Generic.HashSet<VisualBasicImportReference>();
                        }
                        AssemblyCache.XmlnsMappings[key] = mapping;
                    }
                    if (!mapping.IsEmpty)
                    {
                        if (settings == null)
                        {
                            settings = new VisualBasicSettings();
                        }
                        foreach (VisualBasicImportReference reference2 in mapping.ImportReferences)
                        {
                            if (referenceAssemblies != null)
                            {
                                VisualBasicImportReference reference3;
                                AssemblyName assemblyName = reference2.AssemblyName;
                                if (reference2.EarlyBoundAssembly != null)
                                {
                                    if (referenceAssemblies.Contains(reference2.EarlyBoundAssembly))
                                    {
                                        reference3 = reference2.Clone();
                                        reference3.EarlyBoundAssembly = reference2.EarlyBoundAssembly;
                                        settings.ImportReferences.Add(reference3);
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < referenceAssemblies.Count; i++)
                                    {
                                        if (AssemblySatisfiesReference(VisualBasicHelper.GetFastAssemblyName(referenceAssemblies[i]), assemblyName))
                                        {
                                            reference3 = reference2.Clone();
                                            reference3.EarlyBoundAssembly = referenceAssemblies[i];
                                            settings.ImportReferences.Add(reference3);
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                VisualBasicImportReference reference4 = reference2.Clone();
                                if (reference2.EarlyBoundAssembly != null)
                                {
                                    reference4.EarlyBoundAssembly = reference2.EarlyBoundAssembly;
                                }
                                settings.ImportReferences.Add(reference4);
                            }
                        }
                    }
                }
            }
            return settings;
        }

        private static class AssemblyCache
        {
            private static bool initialized = false;
            private static Dictionary<XNamespace, VisualBasicExpressionConverter.XmlnsMapping> xmlnsMappings = new Dictionary<XNamespace, VisualBasicExpressionConverter.XmlnsMapping>(new XNamespaceEqualityComparer());

            private static void CacheLoadedAssembly(Assembly assembly)
            {
                XmlnsDefinitionAttribute[] customAttributes = (XmlnsDefinitionAttribute[]) assembly.GetCustomAttributes(typeof(XmlnsDefinitionAttribute), false);
                string fullName = assembly.FullName;
                for (int i = 0; i < customAttributes.Length; i++)
                {
                    VisualBasicExpressionConverter.XmlnsMapping mapping;
                    XNamespace key = XNamespace.Get(customAttributes[i].XmlNamespace);
                    if (!xmlnsMappings.TryGetValue(key, out mapping))
                    {
                        mapping.ImportReferences = new System.Collections.Generic.HashSet<VisualBasicImportReference>();
                        xmlnsMappings[key] = mapping;
                    }
                    VisualBasicImportReference item = new VisualBasicImportReference {
                        Assembly = fullName,
                        Import = customAttributes[i].ClrNamespace,
                        Xmlns = key
                    };
                    item.EarlyBoundAssembly = assembly;
                    mapping.ImportReferences.Add(item);
                }
            }

            private static void EnsureInitialized()
            {
                if (!initialized)
                {
                    lock (xmlnsMappings)
                    {
                        if (!initialized)
                        {
                            AppDomain.CurrentDomain.AssemblyLoad += delegate (object sender, AssemblyLoadEventArgs args) {
                                Assembly loadedAssembly = args.LoadedAssembly;
                                if (loadedAssembly.IsDefined(typeof(XmlnsDefinitionAttribute), false) && !loadedAssembly.IsDynamic)
                                {
                                    lock (xmlnsMappings)
                                    {
                                        CacheLoadedAssembly(loadedAssembly);
                                    }
                                }
                            };
                            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                            {
                                if (assembly.IsDefined(typeof(XmlnsDefinitionAttribute), false) && !assembly.IsDynamic)
                                {
                                    CacheLoadedAssembly(assembly);
                                }
                            }
                            initialized = true;
                        }
                    }
                }
            }

            public static Dictionary<XNamespace, VisualBasicExpressionConverter.XmlnsMapping> XmlnsMappings
            {
                get
                {
                    EnsureInitialized();
                    return xmlnsMappings;
                }
            }

            private class XNamespaceEqualityComparer : IEqualityComparer<XNamespace>
            {
                bool IEqualityComparer<XNamespace>.Equals(XNamespace x, XNamespace y)
                {
                    return (x == y);
                }

                int IEqualityComparer<XNamespace>.GetHashCode(XNamespace x)
                {
                    return x.GetHashCode();
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct XmlnsMapping
        {
            public System.Collections.Generic.HashSet<VisualBasicImportReference> ImportReferences;
            public bool IsEmpty
            {
                get
                {
                    if (this.ImportReferences != null)
                    {
                        return (this.ImportReferences.Count == 0);
                    }
                    return true;
                }
            }
        }
    }
}

