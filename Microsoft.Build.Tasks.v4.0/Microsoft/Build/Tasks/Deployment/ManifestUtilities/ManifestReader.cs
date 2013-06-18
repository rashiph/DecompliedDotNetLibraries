namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml;
    using System.Xml.Serialization;

    [ComVisible(false)]
    public static class ManifestReader
    {
        private static Manifest Deserialize(Stream s)
        {
            s.Position = 0L;
            XmlTextReader reader = new XmlTextReader(s);
            do
            {
                reader.Read();
            }
            while (reader.NodeType != XmlNodeType.Element);
            string str = typeof(Util).Namespace;
            Type type = Type.GetType(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[] { str, reader.Name }));
            s.Position = 0L;
            XmlSerializer serializer = new XmlSerializer(type);
            int tickCount = Environment.TickCount;
            Manifest manifest = (Manifest) serializer.Deserialize(s);
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "ManifestReader.Deserialize t={0}", new object[] { Environment.TickCount - tickCount }));
            return manifest;
        }

        private static Manifest Deserialize(string path)
        {
            using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return Deserialize(stream);
            }
        }

        internal static ComInfo[] GetComInfo(string path)
        {
            XmlDocument xmlDocument = GetXmlDocument(path);
            XmlNamespaceManager namespaceManager = XmlNamespaces.GetNamespaceManager(xmlDocument.NameTable);
            string fileName = Path.GetFileName(path);
            List<ComInfo> list = new List<ComInfo>();
            foreach (XmlNode node in xmlDocument.SelectNodes("/asmv1:assembly/asmv1:file[asmv1:typelib or asmv1:comClass]", namespaceManager))
            {
                XmlNode node2 = node.SelectSingleNode("@name", namespaceManager);
                string componentFileName = (node2 != null) ? node2.Value : null;
                foreach (XmlNode node3 in node.SelectNodes("asmv1:comClass/@clsid", namespaceManager))
                {
                    list.Add(new ComInfo(fileName, componentFileName, node3.Value, null));
                }
                foreach (XmlNode node4 in node.SelectNodes("asmv1:typelib/@tlbid", namespaceManager))
                {
                    list.Add(new ComInfo(fileName, componentFileName, null, node4.Value));
                }
            }
            return list.ToArray();
        }

        private static XmlDocument GetXmlDocument(string path)
        {
            using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] buffer = new byte[2];
                stream.Read(buffer, 0, 2);
                stream.Position = 0L;
                if ((buffer[0] == 0x4d) && (buffer[1] == 90))
                {
                    Stream inStream = EmbeddedManifestReader.Read(path);
                    if (inStream == null)
                    {
                        throw new BadImageFormatException(null, path);
                    }
                    XmlDocument document = new XmlDocument();
                    document.Load(inStream);
                    return document;
                }
                XmlDocument document2 = new XmlDocument();
                document2.Load(stream);
                return document2;
            }
        }

        private static Manifest ReadEmbeddedManifest(string path)
        {
            Stream s = EmbeddedManifestReader.Read(path);
            if (s == null)
            {
                return null;
            }
            Util.WriteLogFile(Path.GetFileNameWithoutExtension(path) + ".embedded.xml", s);
            Manifest manifest = ReadManifest(s, false);
            manifest.SourcePath = path;
            return manifest;
        }

        public static Manifest ReadManifest(Stream input, bool preserveStream)
        {
            return ReadManifest(null, input, preserveStream);
        }

        public static Manifest ReadManifest(string path, bool preserveStream)
        {
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            string manifestType = null;
            if (path.EndsWith(".application", StringComparison.Ordinal))
            {
                manifestType = "DeployManifest";
            }
            else if (path.EndsWith(".exe.manifest", StringComparison.Ordinal))
            {
                manifestType = "ApplicationManifest";
            }
            return ReadManifest(manifestType, path, preserveStream);
        }

        public static Manifest ReadManifest(string manifestType, Stream input, bool preserveStream)
        {
            Stream stream;
            int tickCount = Environment.TickCount;
            string resource = "read2.xsl";
            Manifest manifest = null;
            if (manifestType != null)
            {
                DictionaryEntry entry = new DictionaryEntry("manifest-type", manifestType);
                DictionaryEntry[] entries = new DictionaryEntry[] { entry };
                stream = XmlUtil.XslTransform(resource, input, entries);
            }
            else
            {
                stream = XmlUtil.XslTransform(resource, input, new DictionaryEntry[0]);
            }
            try
            {
                stream.Position = 0L;
                manifest = Deserialize(stream);
                if (manifest.GetType() == typeof(ApplicationManifest))
                {
                    ApplicationManifest manifest2 = (ApplicationManifest) manifest;
                    manifest2.TrustInfo = new TrustInfo();
                    manifest2.TrustInfo.ReadManifest(input);
                }
                if (preserveStream)
                {
                    input.Position = 0L;
                    manifest.InputStream = new MemoryStream();
                    Util.CopyStream(input, manifest.InputStream);
                }
                stream.Position = 0L;
                string fullName = manifest.AssemblyIdentity.GetFullName(AssemblyIdentity.FullNameFlags.All);
                if (string.IsNullOrEmpty(fullName))
                {
                    fullName = manifest.GetType().Name;
                }
                Util.WriteLogFile(fullName + ".read.xml", stream);
            }
            finally
            {
                stream.Close();
            }
            Util.WriteLog(string.Format(CultureInfo.InvariantCulture, "ManifestReader.ReadManifest t={0}", new object[] { Environment.TickCount - tickCount }));
            manifest.OnAfterLoad();
            return manifest;
        }

        public static Manifest ReadManifest(string manifestType, string path, bool preserveStream)
        {
            Manifest manifest = null;
            using (Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                byte[] buffer = new byte[2];
                stream.Read(buffer, 0, 2);
                stream.Position = 0L;
                if ((buffer[0] == 0x4d) && (buffer[1] == 90))
                {
                    return ReadEmbeddedManifest(path);
                }
                manifest = ReadManifest(manifestType, stream, preserveStream);
                manifest.SourcePath = path;
            }
            return manifest;
        }
    }
}

