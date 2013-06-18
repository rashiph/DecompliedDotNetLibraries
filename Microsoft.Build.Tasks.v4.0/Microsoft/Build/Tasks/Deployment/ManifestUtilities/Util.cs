namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml;

    internal static class Util
    {
        private static int[] clrVersion2;
        private static readonly char[] fileNameInvalidChars = new char[] { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };
        private static readonly ItemComparer itemComparer;
        private static StreamWriter logFileWriter = null;
        internal static readonly bool logging = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("VSPLOG"));
        internal static readonly string logPath = GetLogPath();
        private static readonly string[] platforms;
        private static readonly string[] processorArchitectures;
        internal static readonly string Schema = Environment.GetEnvironmentVariable("VSPSCHEMA");

        static Util()
        {
            int[] numArray = new int[4];
            numArray[0] = 2;
            numArray[2] = 0xc627;
            clrVersion2 = numArray;
            platforms = new string[] { "AnyCPU", "x86", "x64", "Itanium" };
            processorArchitectures = new string[] { "msil", "x86", "amd64", "ia64" };
            itemComparer = new ItemComparer();
        }

        public static string ByteArrayToHex(byte[] a)
        {
            if (a == null)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(a.Length);
            foreach (byte num in a)
            {
                builder.Append(num.ToString("X02", CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }

        public static string ByteArrayToString(byte[] a)
        {
            if (a == null)
            {
                return null;
            }
            StringBuilder builder = new StringBuilder(a.Length);
            foreach (byte num in a)
            {
                builder.Append(Convert.ToChar(num));
            }
            return builder.ToString();
        }

        public static int CompareFrameworkVersions(string versionA, string versionB)
        {
            Version version = ConvertFrameworkVersionToString(versionA);
            Version version2 = ConvertFrameworkVersionToString(versionB);
            return version.CompareTo(version2);
        }

        public static Version ConvertFrameworkVersionToString(string version)
        {
            if (version.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                return new Version(version.Substring(1));
            }
            return new Version(version);
        }

        public static int CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[0x4000];
            int num = 0;
            int count = 0;
            do
            {
                count = input.Read(buffer, 0, 0x4000);
                output.Write(buffer, 0, count);
                num += count;
            }
            while (count > 0);
            output.Flush();
            input.Position = 0L;
            output.Position = 0L;
            return num;
        }

        public static string FilterNonprintableChars(string value)
        {
            StringBuilder builder = new StringBuilder(value);
            int startIndex = 0;
            while (startIndex < builder.Length)
            {
                if (builder[startIndex] < ' ')
                {
                    builder.Remove(startIndex, 1);
                }
                else
                {
                    startIndex++;
                }
            }
            return builder.ToString();
        }

        public static string GetAssemblyPath()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string GetClrVersion()
        {
            Version version = Environment.Version;
            version = new Version(version.Major, version.Minor, version.Build, 0);
            return version.ToString();
        }

        public static string GetClrVersion(string targetFrameworkVersion)
        {
            if (string.IsNullOrEmpty(targetFrameworkVersion))
            {
                return GetClrVersion();
            }
            Version version = null;
            Version version2 = null;
            if (targetFrameworkVersion.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                version2 = new Version(targetFrameworkVersion.Substring(1));
            }
            else
            {
                version2 = new Version(targetFrameworkVersion);
            }
            Version version3 = Environment.Version;
            if (version2.Major >= version3.Major)
            {
                version = new Version(version3.Major, version3.Minor, version3.Build, 0);
            }
            else
            {
                version = new Version(clrVersion2[0], clrVersion2[1], clrVersion2[2], clrVersion2[3]);
            }
            return version.ToString();
        }

        public static Stream GetEmbeddedResourceStream(string name)
        {
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", new object[] { typeof(Microsoft.Build.Tasks.Deployment.ManifestUtilities.Util).Namespace, name }));
        }

        public static string GetEmbeddedResourceString(string name)
        {
            StreamReader reader = new StreamReader(GetEmbeddedResourceStream(name));
            return reader.ReadToEnd();
        }

        public static void GetFileInfo(string path, out string hash, out long length)
        {
            FileInfo info = new FileInfo(path);
            length = info.Length;
            Stream inputStream = null;
            try
            {
                inputStream = info.OpenRead();
                byte[] inArray = new SHA1CryptoServiceProvider().ComputeHash(inputStream);
                hash = Convert.ToBase64String(inArray);
            }
            finally
            {
                if (inputStream != null)
                {
                    inputStream.Close();
                }
            }
        }

        private static string GetLogPath()
        {
            if (!logging)
            {
                return null;
            }
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Microsoft\VisualStudio\8.0\VSPLOG");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }

        public static string GetRegisteredOrganization()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", false);
            if (key != null)
            {
                string str = (string) key.GetValue("RegisteredOrganization");
                if (str != null)
                {
                    str = str.Trim();
                }
                if (!string.IsNullOrEmpty(str))
                {
                    return str;
                }
            }
            return null;
        }

        public static bool IsValidAssemblyName(string value)
        {
            return IsValidFileName(value);
        }

        public static bool IsValidCulture(string value)
        {
            if (!string.Equals(value, "neutral", StringComparison.OrdinalIgnoreCase))
            {
                if (string.Equals(value, "*", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
                try
                {
                    new CultureInfo(value);
                }
                catch (ArgumentException)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsValidFileName(string value)
        {
            return (value.IndexOfAny(fileNameInvalidChars) < 0);
        }

        internal static bool IsValidFrameworkVersion(string value)
        {
            if (value.StartsWith("v", StringComparison.OrdinalIgnoreCase))
            {
                return IsValidVersion(value.Substring(1), 2);
            }
            return IsValidVersion(value, 2);
        }

        public static bool IsValidVersion(string value, int octets)
        {
            Version version;
            try
            {
                version = new Version(value);
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
            catch (OverflowException)
            {
                return false;
            }
            if ((octets >= 1) && (version.Major < 0))
            {
                return false;
            }
            if ((octets >= 2) && (version.Minor < 0))
            {
                return false;
            }
            if ((octets >= 3) && (version.Build < 0))
            {
                return false;
            }
            if ((octets >= 4) && (version.Revision < 0))
            {
                return false;
            }
            return true;
        }

        public static string PlatformToProcessorArchitecture(string platform)
        {
            for (int i = 0; i < platforms.Length; i++)
            {
                if (string.Compare(platform, platforms[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return processorArchitectures[i];
                }
            }
            return null;
        }

        private static ITaskItem[] RemoveDuplicateItems(ITaskItem[] items)
        {
            if (items == null)
            {
                return null;
            }
            if (items.Length <= 1)
            {
                return items;
            }
            Hashtable hashtable = new Hashtable();
            foreach (ITaskItem item in items)
            {
                if (!string.IsNullOrEmpty(item.ItemSpec))
                {
                    string key = null;
                    AssemblyIdentity identity = new AssemblyIdentity(item.ItemSpec);
                    if (identity.IsStrongName)
                    {
                        key = identity.GetFullName(AssemblyIdentity.FullNameFlags.All);
                    }
                    else
                    {
                        key = Path.GetFullPath(item.ItemSpec).ToUpperInvariant();
                    }
                    if (!hashtable.Contains(key))
                    {
                        hashtable.Add(key, item);
                    }
                }
            }
            ITaskItem[] array = new ITaskItem[hashtable.Count];
            hashtable.Values.CopyTo(array, 0);
            return array;
        }

        public static ITaskItem[] SortItems(ITaskItem[] items)
        {
            ITaskItem[] array = RemoveDuplicateItems(items);
            if (array != null)
            {
                Array.Sort(array, itemComparer);
            }
            return array;
        }

        public static void WriteFile(string path, Stream s)
        {
            WriteFile(path, new StreamReader(s).ReadToEnd());
        }

        public static void WriteFile(string path, string s)
        {
            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.Write(s);
            }
        }

        public static void WriteLog(string text)
        {
            if (logging)
            {
                if (logFileWriter == null)
                {
                    try
                    {
                        logFileWriter = new StreamWriter(Path.Combine(logPath, "Microsoft.Build.Tasks.log"), false);
                    }
                    catch (UnauthorizedAccessException)
                    {
                        return;
                    }
                    catch (ArgumentException)
                    {
                        return;
                    }
                    catch (IOException)
                    {
                        return;
                    }
                    catch (SecurityException)
                    {
                        return;
                    }
                }
                logFileWriter.WriteLine(text);
                logFileWriter.Flush();
            }
        }

        public static void WriteLogFile(string filename, Stream s)
        {
            if (logging)
            {
                string path = Path.Combine(logPath, filename);
                string str2 = new StreamReader(s).ReadToEnd();
                try
                {
                    WriteFile(path, str2);
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (ArgumentException)
                {
                }
                catch (IOException)
                {
                }
                catch (SecurityException)
                {
                }
                s.Position = 0L;
            }
        }

        public static void WriteLogFile(string filename, string s)
        {
            if (logging)
            {
                string path = Path.Combine(logPath, filename);
                try
                {
                    WriteFile(path, s);
                }
                catch (UnauthorizedAccessException)
                {
                }
                catch (ArgumentException)
                {
                }
                catch (IOException)
                {
                }
                catch (SecurityException)
                {
                }
            }
        }

        public static void WriteLogFile(string filename, XmlElement element)
        {
            if (logging)
            {
                WriteLogFile(filename, element.OuterXml);
            }
        }

        public static string WriteTempFile(Stream s)
        {
            string temporaryFile = Microsoft.Build.Shared.FileUtilities.GetTemporaryFile();
            WriteFile(temporaryFile, s);
            return temporaryFile;
        }

        public static string WriteTempFile(string s)
        {
            string temporaryFile = Microsoft.Build.Shared.FileUtilities.GetTemporaryFile();
            WriteFile(temporaryFile, s);
            return temporaryFile;
        }

        private class ItemComparer : IComparer
        {
            int IComparer.Compare(object obj1, object obj2)
            {
                if ((obj1 != null) && (obj2 != null))
                {
                    if (!(obj1 is ITaskItem) || !(obj2 is ITaskItem))
                    {
                        return 0;
                    }
                    ITaskItem item = obj1 as ITaskItem;
                    ITaskItem item2 = obj2 as ITaskItem;
                    if ((item.ItemSpec != null) && (item2.ItemSpec != null))
                    {
                        return string.Compare(item.ItemSpec, item2.ItemSpec, StringComparison.Ordinal);
                    }
                }
                return 0;
            }
        }
    }
}

