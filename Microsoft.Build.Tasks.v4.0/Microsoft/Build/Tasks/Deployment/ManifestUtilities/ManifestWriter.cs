namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using Microsoft.Build.Shared;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [ComVisible(false)]
    public static class ManifestWriter
    {
        private static Stream Serialize(Manifest manifest)
        {
            manifest.OnBeforeSave();
            MemoryStream stream = new MemoryStream();
            XmlSerializer serializer = new XmlSerializer(manifest.GetType());
            StreamWriter writer = new StreamWriter(stream);
            int tickCount = Environment.TickCount;
            serializer.Serialize((TextWriter) writer, manifest);
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "ManifestWriter.Serialize t={0}", new object[] { Environment.TickCount - tickCount }));
            writer.Flush();
            stream.Position = 0L;
            return stream;
        }

        public static void WriteManifest(Manifest manifest)
        {
            string sourcePath = manifest.SourcePath;
            if (sourcePath == null)
            {
                sourcePath = "manifest.xml";
            }
            WriteManifest(manifest, sourcePath);
        }

        public static void WriteManifest(Manifest manifest, Stream output)
        {
            int tickCount = Environment.TickCount;
            Stream s = Serialize(manifest);
            string fullName = manifest.AssemblyIdentity.GetFullName(AssemblyIdentity.FullNameFlags.All);
            if (string.IsNullOrEmpty(fullName))
            {
                fullName = manifest.GetType().Name;
            }
            Util.WriteLogFile(fullName + ".write.0-serialized.xml", s);
            string resource = "write2.xsl";
            Stream stream2 = null;
            if (manifest.GetType() == typeof(ApplicationManifest))
            {
                ApplicationManifest manifest2 = (ApplicationManifest) manifest;
                if (manifest2.TrustInfo == null)
                {
                    stream2 = XmlUtil.XslTransform(resource, s, new DictionaryEntry[0]);
                }
                else
                {
                    string temporaryFile = Microsoft.Build.Shared.FileUtilities.GetTemporaryFile();
                    manifest2.TrustInfo.Write(temporaryFile);
                    if (Util.logging)
                    {
                        try
                        {
                            File.Copy(temporaryFile, Path.Combine(Util.logPath, fullName + ".trust-file.xml"), true);
                        }
                        catch (IOException)
                        {
                        }
                        catch (ArgumentException)
                        {
                        }
                        catch (UnauthorizedAccessException)
                        {
                        }
                        catch (NotSupportedException)
                        {
                        }
                    }
                    DictionaryEntry entry = new DictionaryEntry("trust-file", temporaryFile);
                    try
                    {
                        DictionaryEntry[] entries = new DictionaryEntry[] { entry };
                        stream2 = XmlUtil.XslTransform(resource, s, entries);
                    }
                    finally
                    {
                        File.Delete(temporaryFile);
                    }
                }
            }
            else
            {
                stream2 = XmlUtil.XslTransform(resource, s, new DictionaryEntry[0]);
            }
            Util.WriteLogFile(fullName + ".write.1-transformed.xml", stream2);
            Stream stream3 = null;
            if (manifest.InputStream == null)
            {
                stream3 = stream2;
            }
            else
            {
                string str4 = Util.WriteTempFile(manifest.InputStream);
                DictionaryEntry entry2 = new DictionaryEntry("base-file", str4);
                try
                {
                    DictionaryEntry[] entryArray2 = new DictionaryEntry[] { entry2 };
                    stream3 = XmlUtil.XslTransform("merge.xsl", stream2, entryArray2);
                }
                finally
                {
                    File.Delete(str4);
                }
                Util.WriteLogFile(fullName + ".write.2-merged.xml", stream3);
            }
            Stream stream4 = ManifestFormatter.Format(stream3);
            Util.WriteLogFile(fullName + ".write.3-formatted.xml", stream4);
            Util.CopyStream(stream4, output);
            Util.WriteLog(string.Format(CultureInfo.CurrentCulture, "ManifestWriter.WriteManifest t={0}", new object[] { Environment.TickCount - tickCount }));
        }

        public static void WriteManifest(Manifest manifest, string path)
        {
            using (Stream stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.Write))
            {
                WriteManifest(manifest, stream);
            }
        }
    }
}

