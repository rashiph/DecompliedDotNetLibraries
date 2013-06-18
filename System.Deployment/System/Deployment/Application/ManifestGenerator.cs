namespace System.Deployment.Application
{
    using System;
    using System.Collections;
    using System.Deployment.Application.Manifest;
    using System.Deployment.Internal.Isolation;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using System.Xml;

    internal static class ManifestGenerator
    {
        private static object assemblyTemplateDoc;
        private const string AssemblyTemplateResource = "AssemblyTemplate.xml";
        private static object GACDetectionTempManifestAsmId = null;

        private static void AddDependencies(XmlDocument document, DependentAssembly[] dependentAssemblies)
        {
            Hashtable hashtable = new Hashtable();
            XmlNamespaceManager namespaceMgr = GetNamespaceMgr(document);
            XmlElement element = (XmlElement) document.SelectSingleNode("/asmv1:assembly", namespaceMgr);
            foreach (DependentAssembly assembly in dependentAssemblies)
            {
                if (!hashtable.Contains(assembly.Identity))
                {
                    XmlElement newChild = document.CreateElement("dependency", "urn:schemas-microsoft-com:asm.v1");
                    element.AppendChild(newChild);
                    XmlElement element3 = document.CreateElement("dependentAssembly", "urn:schemas-microsoft-com:asm.v1");
                    newChild.AppendChild(element3);
                    System.Deployment.Application.ReferenceIdentity refId = assembly.Identity;
                    System.Deployment.Application.DefinitionIdentity asmId = new System.Deployment.Application.DefinitionIdentity(refId);
                    XmlElement element4 = CreateAssemblyIdentityElement(document, asmId);
                    element3.AppendChild(element4);
                    hashtable.Add(refId, asmId);
                }
            }
        }

        private static void AddFile(XmlDocument document, XmlElement assemblyNode, System.Deployment.Application.Manifest.File file)
        {
            XmlElement newChild = document.CreateElement("file", "urn:schemas-microsoft-com:asm.v1");
            assemblyNode.AppendChild(newChild);
            newChild.SetAttributeNode("name", null).Value = file.Name;
        }

        private static void AddFiles(XmlDocument document, System.Deployment.Application.Manifest.File[] files)
        {
            XmlNamespaceManager namespaceMgr = GetNamespaceMgr(document);
            XmlElement assemblyNode = (XmlElement) document.SelectSingleNode("/asmv1:assembly", namespaceMgr);
            foreach (System.Deployment.Application.Manifest.File file in files)
            {
                AddFile(document, assemblyNode, file);
            }
        }

        private static XmlDocument CloneAssemblyTemplate()
        {
            if (assemblyTemplateDoc == null)
            {
                Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("AssemblyTemplate.xml");
                XmlDocument document = new XmlDocument();
                document.Load(manifestResourceStream);
                Interlocked.CompareExchange(ref assemblyTemplateDoc, document, null);
            }
            return (XmlDocument) ((XmlDocument) assemblyTemplateDoc).Clone();
        }

        private static XmlElement CreateAssemblyIdentityElement(XmlDocument document, System.Deployment.Application.DefinitionIdentity asmId)
        {
            XmlElement element = document.CreateElement("assemblyIdentity", "urn:schemas-microsoft-com:asm.v1");
            System.Deployment.Internal.Isolation.IDENTITY_ATTRIBUTE[] attributes = asmId.Attributes;
            StringComparison invariantCultureIgnoreCase = StringComparison.InvariantCultureIgnoreCase;
            foreach (System.Deployment.Internal.Isolation.IDENTITY_ATTRIBUTE identity_attribute in attributes)
            {
                string namespaceURI = identity_attribute.Namespace;
                string name = identity_attribute.Name;
                if (namespaceURI == null)
                {
                    if (name.Equals("name", invariantCultureIgnoreCase))
                    {
                        name = "name";
                    }
                    else if (name.Equals("version", invariantCultureIgnoreCase))
                    {
                        name = "version";
                    }
                    else if (name.Equals("processorArchitecture", invariantCultureIgnoreCase))
                    {
                        name = "processorArchitecture";
                    }
                    else if (name.Equals("publicKeyToken", invariantCultureIgnoreCase))
                    {
                        name = "publicKeyToken";
                    }
                    else if (name.Equals("type", invariantCultureIgnoreCase))
                    {
                        name = "type";
                    }
                    else if (name.Equals("culture", invariantCultureIgnoreCase))
                    {
                        name = "language";
                    }
                }
                element.SetAttribute(name, namespaceURI, identity_attribute.Value);
            }
            return element;
        }

        public static void GenerateGACDetectionManifest(System.Deployment.Application.ReferenceIdentity refId, string outputManifest)
        {
            XmlDocument document = CloneAssemblyTemplate();
            if (GACDetectionTempManifestAsmId == null)
            {
                Interlocked.CompareExchange(ref GACDetectionTempManifestAsmId, new System.Deployment.Application.DefinitionIdentity("GACDetectionTempManifest, version=1.0.0.0, type=win32"), null);
            }
            InjectIdentityXml(document, (System.Deployment.Application.DefinitionIdentity) GACDetectionTempManifestAsmId);
            AddDependencies(document, new DependentAssembly[] { new DependentAssembly(refId) });
            using (FileStream stream = System.IO.File.Open(outputManifest, FileMode.CreateNew, FileAccess.Write))
            {
                document.Save(stream);
            }
        }

        public static System.Deployment.Application.DefinitionIdentity GenerateManifest(System.Deployment.Application.ReferenceIdentity suggestedReferenceIdentity, AssemblyManifest manifest, string outputManifest)
        {
            System.Deployment.Application.DefinitionIdentity asmId = manifest.Identity;
            if (manifest.RawXmlBytes != null)
            {
                using (FileStream stream = System.IO.File.Open(outputManifest, FileMode.CreateNew, FileAccess.Write))
                {
                    stream.Write(manifest.RawXmlBytes, 0, manifest.RawXmlBytes.Length);
                    return asmId;
                }
            }
            XmlDocument document = CloneAssemblyTemplate();
            asmId = new System.Deployment.Application.DefinitionIdentity(suggestedReferenceIdentity);
            InjectIdentityXml(document, asmId);
            AddFiles(document, manifest.Files);
            AddDependencies(document, manifest.DependentAssemblies);
            using (FileStream stream2 = System.IO.File.Open(outputManifest, FileMode.CreateNew, FileAccess.Write))
            {
                document.Save(stream2);
            }
            return asmId;
        }

        private static XmlNamespaceManager GetNamespaceMgr(XmlDocument document)
        {
            XmlNamespaceManager manager = new XmlNamespaceManager(document.NameTable);
            manager.AddNamespace("asmv1", "urn:schemas-microsoft-com:asm.v1");
            manager.AddNamespace("asmv2", "urn:schemas-microsoft-com:asm.v2");
            manager.AddNamespace("dsig", "http://www.w3.org/2000/09/xmldsig#");
            return manager;
        }

        private static void InjectIdentityXml(XmlDocument document, System.Deployment.Application.DefinitionIdentity asmId)
        {
            XmlElement newChild = CreateAssemblyIdentityElement(document, asmId);
            document.DocumentElement.AppendChild(newChild);
        }
    }
}

