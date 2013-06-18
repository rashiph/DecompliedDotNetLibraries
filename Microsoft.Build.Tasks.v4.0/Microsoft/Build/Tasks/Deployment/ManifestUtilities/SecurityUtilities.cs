namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Utilities;
    using Microsoft.Win32;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Deployment.Internal.CodeSigning;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Resources;
    using System.Runtime.Hosting;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;
    using System.Threading;
    using System.Xml;

    [ComVisible(false)]
    public static class SecurityUtilities
    {
        private const string Custom = "Custom";
        private static readonly Version dotNet40Version = new Version("4.0");
        private const string Everything = "Everything";
        private const int Fx2MajorVersion = 2;
        private const int Fx3MajorVersion = 3;
        private const string Internet = "Internet";
        private const string InternetPermissionSetWithWPFXml = "<PermissionSet class=\"System.Security.PermissionSet\" version=\"1\" ID=\"Custom\" SameSite=\"site\">\n<IPermission class=\"System.Security.Permissions.FileDialogPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Access=\"Open\" />\n<IPermission class=\"System.Security.Permissions.IsolatedStorageFilePermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Allowed=\"ApplicationIsolationByUser\" UserQuota=\"512000\" />\n<IPermission class=\"System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Flags=\"Execution\" />\n<IPermission class=\"System.Security.Permissions.UIPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Window=\"SafeTopLevelWindows\" Clipboard=\"OwnClipboard\" />\n<IPermission class=\"System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" version=\"1\" Level=\"SafePrinting\" />\n<IPermission class=\"System.Security.Permissions.MediaPermission, WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" version=\"1\" Audio=\"SafeAudio\" Video=\"SafeVideo\" Image=\"SafeImage\" />\n<IPermission class=\"System.Security.Permissions.WebBrowserPermission, WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" version=\"1\" Level=\"Safe\" />\n</PermissionSet>";
        private const string InternetPermissionSetXml = "<PermissionSet class=\"System.Security.PermissionSet\" version=\"1\" ID=\"Custom\" SameSite=\"site\">\n<IPermission class=\"System.Security.Permissions.FileDialogPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Access=\"Open\" />\n<IPermission class=\"System.Security.Permissions.IsolatedStorageFilePermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Allowed=\"ApplicationIsolationByUser\" UserQuota=\"512000\" />\n<IPermission class=\"System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Flags=\"Execution\" />\n<IPermission class=\"System.Security.Permissions.UIPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Window=\"SafeTopLevelWindows\" Clipboard=\"OwnClipboard\" />\n<IPermission class=\"System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" version=\"1\" Level=\"SafePrinting\" />\n</PermissionSet>";
        private const string LocalIntranet = "LocalIntranet";
        private const string LocalIntranetPermissionSetWithWPFXml = "<PermissionSet class=\"System.Security.PermissionSet\" version=\"1\" ID=\"Custom\" SameSite=\"site\">\n<IPermission class=\"System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Read=\"USERNAME\" />\n<IPermission class=\"System.Security.Permissions.FileDialogPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Unrestricted=\"true\" />\n<IPermission class=\"System.Security.Permissions.IsolatedStorageFilePermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Allowed=\"AssemblyIsolationByUser\" UserQuota=\"9223372036854775807\" Expiry=\"9223372036854775807\" Permanent=\"True\" />\n<IPermission class=\"System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Flags=\"ReflectionEmit\" />\n<IPermission class=\"System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Flags=\"Assertion, Execution, BindingRedirects\" />\n<IPermission class=\"System.Security.Permissions.UIPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Unrestricted=\"true\" />\n<IPermission class=\"System.Net.DnsPermission, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Unrestricted=\"true\" />\n<IPermission class=\"System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" version=\"1\" Level=\"DefaultPrinting\" />\n<IPermission class=\"System.Security.Permissions.MediaPermission, WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" version=\"1\" Audio=\"SafeAudio\" Video=\"SafeVideo\" Image=\"SafeImage\" />\n<IPermission class=\"System.Security.Permissions.WebBrowserPermission, WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" version=\"1\" Level=\"Safe\" />\n</PermissionSet>";
        private const string LocalIntranetPermissionSetXml = "<PermissionSet class=\"System.Security.PermissionSet\" version=\"1\" ID=\"Custom\" SameSite=\"site\">\n<IPermission class=\"System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Read=\"USERNAME\" />\n<IPermission class=\"System.Security.Permissions.FileDialogPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Unrestricted=\"true\" />\n<IPermission class=\"System.Security.Permissions.IsolatedStorageFilePermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Allowed=\"AssemblyIsolationByUser\" UserQuota=\"9223372036854775807\" Expiry=\"9223372036854775807\" Permanent=\"True\" />\n<IPermission class=\"System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Flags=\"ReflectionEmit\" />\n<IPermission class=\"System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Flags=\"Assertion, Execution, BindingRedirects\" />\n<IPermission class=\"System.Security.Permissions.UIPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Unrestricted=\"true\" />\n<IPermission class=\"System.Net.DnsPermission, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Unrestricted=\"true\" />\n<IPermission class=\"System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" version=\"1\" Level=\"DefaultPrinting\" />\n</PermissionSet>";
        private const string PermissionSetsFolder = "PermissionSets";

        public static PermissionSet ComputeZonePermissionSet(string targetZone, PermissionSet includedPermissionSet, string[] excludedPermissions)
        {
            return ComputeZonePermissionSetHelper(targetZone, includedPermissionSet, null, string.Empty);
        }

        internal static PermissionSet ComputeZonePermissionSetHelper(string targetZone, PermissionSet includedPermissionSet, ITaskItem[] dependencies, string targetFrameworkMoniker)
        {
            if (!string.IsNullOrEmpty(targetZone) && !string.Equals(targetZone, "Custom", StringComparison.OrdinalIgnoreCase))
            {
                return GetNamedPermissionSetFromZone(targetZone, dependencies, targetFrameworkMoniker);
            }
            return includedPermissionSet.Copy();
        }

        private static XmlDocument CreateXmlDocV2(string targetZone)
        {
            XmlDocument document = new XmlDocument();
            switch (targetZone)
            {
                case "LocalIntranet":
                    document.LoadXml("<PermissionSet class=\"System.Security.PermissionSet\" version=\"1\" ID=\"Custom\" SameSite=\"site\">\n<IPermission class=\"System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Read=\"USERNAME\" />\n<IPermission class=\"System.Security.Permissions.FileDialogPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Unrestricted=\"true\" />\n<IPermission class=\"System.Security.Permissions.IsolatedStorageFilePermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Allowed=\"AssemblyIsolationByUser\" UserQuota=\"9223372036854775807\" Expiry=\"9223372036854775807\" Permanent=\"True\" />\n<IPermission class=\"System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Flags=\"ReflectionEmit\" />\n<IPermission class=\"System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Flags=\"Assertion, Execution, BindingRedirects\" />\n<IPermission class=\"System.Security.Permissions.UIPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Unrestricted=\"true\" />\n<IPermission class=\"System.Net.DnsPermission, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Unrestricted=\"true\" />\n<IPermission class=\"System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" version=\"1\" Level=\"DefaultPrinting\" />\n</PermissionSet>");
                    return document;

                case "Internet":
                    document.LoadXml("<PermissionSet class=\"System.Security.PermissionSet\" version=\"1\" ID=\"Custom\" SameSite=\"site\">\n<IPermission class=\"System.Security.Permissions.FileDialogPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Access=\"Open\" />\n<IPermission class=\"System.Security.Permissions.IsolatedStorageFilePermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Allowed=\"ApplicationIsolationByUser\" UserQuota=\"512000\" />\n<IPermission class=\"System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Flags=\"Execution\" />\n<IPermission class=\"System.Security.Permissions.UIPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Window=\"SafeTopLevelWindows\" Clipboard=\"OwnClipboard\" />\n<IPermission class=\"System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" version=\"1\" Level=\"SafePrinting\" />\n</PermissionSet>");
                    return document;
            }
            throw new ArgumentException(string.Empty, "targetZone");
        }

        private static XmlDocument CreateXmlDocV3(string targetZone)
        {
            XmlDocument document = new XmlDocument();
            switch (targetZone)
            {
                case "LocalIntranet":
                    document.LoadXml("<PermissionSet class=\"System.Security.PermissionSet\" version=\"1\" ID=\"Custom\" SameSite=\"site\">\n<IPermission class=\"System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Read=\"USERNAME\" />\n<IPermission class=\"System.Security.Permissions.FileDialogPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Unrestricted=\"true\" />\n<IPermission class=\"System.Security.Permissions.IsolatedStorageFilePermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Allowed=\"AssemblyIsolationByUser\" UserQuota=\"9223372036854775807\" Expiry=\"9223372036854775807\" Permanent=\"True\" />\n<IPermission class=\"System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Flags=\"ReflectionEmit\" />\n<IPermission class=\"System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Flags=\"Assertion, Execution, BindingRedirects\" />\n<IPermission class=\"System.Security.Permissions.UIPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Unrestricted=\"true\" />\n<IPermission class=\"System.Net.DnsPermission, System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Unrestricted=\"true\" />\n<IPermission class=\"System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" version=\"1\" Level=\"DefaultPrinting\" />\n<IPermission class=\"System.Security.Permissions.MediaPermission, WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" version=\"1\" Audio=\"SafeAudio\" Video=\"SafeVideo\" Image=\"SafeImage\" />\n<IPermission class=\"System.Security.Permissions.WebBrowserPermission, WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" version=\"1\" Level=\"Safe\" />\n</PermissionSet>");
                    return document;

                case "Internet":
                    document.LoadXml("<PermissionSet class=\"System.Security.PermissionSet\" version=\"1\" ID=\"Custom\" SameSite=\"site\">\n<IPermission class=\"System.Security.Permissions.FileDialogPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Access=\"Open\" />\n<IPermission class=\"System.Security.Permissions.IsolatedStorageFilePermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Allowed=\"ApplicationIsolationByUser\" UserQuota=\"512000\" />\n<IPermission class=\"System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Flags=\"Execution\" />\n<IPermission class=\"System.Security.Permissions.UIPermission, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089\" version=\"1\" Window=\"SafeTopLevelWindows\" Clipboard=\"OwnClipboard\" />\n<IPermission class=\"System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a\" version=\"1\" Level=\"SafePrinting\" />\n<IPermission class=\"System.Security.Permissions.MediaPermission, WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" version=\"1\" Audio=\"SafeAudio\" Video=\"SafeVideo\" Image=\"SafeImage\" />\n<IPermission class=\"System.Security.Permissions.WebBrowserPermission, WindowsBase, Version=3.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\" version=\"1\" Level=\"Safe\" />\n</PermissionSet>");
                    return document;
            }
            throw new ArgumentException(string.Empty, "targetZone");
        }

        internal static X509Certificate2 GetCert(string thumbprint)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                X509Certificate2Collection certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (certificates.Count == 1)
                {
                    return certificates[0];
                }
            }
            finally
            {
                store.Close();
            }
            return null;
        }

        internal static string GetCommandLineParameters(string certThumbprint, Uri timestampUrl, string path)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(string.Format(CultureInfo.InvariantCulture, "sign /sha1 {0} ", new object[] { certThumbprint }));
            if (timestampUrl != null)
            {
                builder.Append(string.Format(CultureInfo.InvariantCulture, "/t {0} ", new object[] { timestampUrl.ToString() }));
            }
            builder.Append(string.Format(CultureInfo.InvariantCulture, "\"{0}\"", new object[] { path }));
            return builder.ToString();
        }

        private static XmlElement GetCurrentCLRPermissions(string targetZone)
        {
            string str = string.Empty;
            SecurityZone noZone = SecurityZone.NoZone;
            string str2 = targetZone;
            if (str2 != null)
            {
                if (!(str2 == "LocalIntranet"))
                {
                    if (str2 == "Internet")
                    {
                        noZone = SecurityZone.Internet;
                        goto Label_0044;
                    }
                }
                else
                {
                    noZone = SecurityZone.Intranet;
                    goto Label_0044;
                }
            }
            throw new ArgumentException(string.Empty, "targetZone");
        Label_0044:;
            Evidence evidence = new Evidence(new EvidenceBase[] { new Zone(noZone), new ActivationArguments(new System.ApplicationIdentity("")) }, null);
            str = SecurityManager.GetStandardSandbox(evidence).ToString();
            if (!string.IsNullOrEmpty(str))
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(str);
                return document.DocumentElement;
            }
            return null;
        }

        private static PermissionSet GetNamedPermissionSet(string targetZone, ITaskItem[] dependencies, string targetFrameworkMoniker)
        {
            FrameworkName fn = null;
            if (!string.IsNullOrEmpty(targetFrameworkMoniker))
            {
                fn = new FrameworkName(targetFrameworkMoniker);
            }
            else
            {
                fn = new FrameworkName(".NETFramework", dotNet40Version);
            }
            int major = fn.Version.Major;
            switch (major)
            {
                case 2:
                    return XmlToPermissionSet(GetXmlElement(targetZone, major));

                case 3:
                    return XmlToPermissionSet(GetXmlElement(targetZone, major));
            }
            return XmlToPermissionSet(GetXmlElement(targetZone, fn));
        }

        private static PermissionSet GetNamedPermissionSetFromZone(string targetZone, ITaskItem[] dependencies, string targetFrameworkMoniker)
        {
            switch (targetZone)
            {
                case "LocalIntranet":
                    return GetNamedPermissionSet("LocalIntranet", dependencies, targetFrameworkMoniker);

                case "Internet":
                    return GetNamedPermissionSet("Internet", dependencies, targetFrameworkMoniker);
            }
            throw new ArgumentException(string.Empty, "targetZone");
        }

        internal static string GetPathToTool()
        {
            string pathToDotNetFrameworkSdkFile = ToolLocationHelper.GetPathToDotNetFrameworkSdkFile(ToolName(), TargetDotNetFrameworkVersion.Version35);
            if (pathToDotNetFrameworkSdkFile == null)
            {
                pathToDotNetFrameworkSdkFile = Path.Combine(Environment.CurrentDirectory, ToolName());
            }
            if (!File.Exists(pathToDotNetFrameworkSdkFile))
            {
                pathToDotNetFrameworkSdkFile = null;
            }
            return pathToDotNetFrameworkSdkFile;
        }

        private static string[] GetRegistryPermissionSetByName(string name)
        {
            string[] strArray = null;
            using (RegistryKey key2 = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\.NETFramework", false))
            {
                if (key2 == null)
                {
                    return strArray;
                }
                using (RegistryKey key3 = key2.OpenSubKey(@"Security\Policy\Extensions\NamedPermissionSets", false))
                {
                    if (key3 != null)
                    {
                        using (RegistryKey key4 = key3.OpenSubKey(name, false))
                        {
                            if (key4 != null)
                            {
                                string[] subKeyNames = key4.GetSubKeyNames();
                                strArray = new string[subKeyNames.Length];
                                for (int i = 0; i < subKeyNames.Length; i++)
                                {
                                    using (RegistryKey key5 = key4.OpenSubKey(subKeyNames[i], false))
                                    {
                                        strArray[i] = key5.GetValue("Xml") as string;
                                    }
                                }
                            }
                            return strArray;
                        }
                    }
                    return strArray;
                }
            }
        }

        private static XmlElement GetXmlElement(string targetZone, int majorVersion)
        {
            XmlDocument document = null;
            switch (majorVersion)
            {
                case 2:
                    document = CreateXmlDocV2(targetZone);
                    break;

                case 3:
                    document = CreateXmlDocV3(targetZone);
                    break;

                default:
                    throw new ArgumentException(string.Empty, "majorVersion");
            }
            XmlElement documentElement = document.DocumentElement;
            if (documentElement == null)
            {
                return null;
            }
            return documentElement;
        }

        private static XmlElement GetXmlElement(string targetZone, FrameworkName fn)
        {
            IList<string> pathToReferenceAssemblies = ToolLocationHelper.GetPathToReferenceAssemblies(fn);
            if (pathToReferenceAssemblies.Count > 0)
            {
                string path = Path.Combine(pathToReferenceAssemblies[0], "PermissionSets");
                if (Directory.Exists(path))
                {
                    string[] files = Directory.GetFiles(path, "*.xml");
                    FileInfo[] infoArray = new FileInfo[files.Length];
                    int index = -1;
                    for (int i = 0; i < files.Length; i++)
                    {
                        infoArray[i] = new FileInfo(files[i]);
                        if (string.Equals(Path.GetFileNameWithoutExtension(files[i]), targetZone, StringComparison.OrdinalIgnoreCase))
                        {
                            index = i;
                            break;
                        }
                    }
                    if (index != -1)
                    {
                        string str3 = string.Empty;
                        FileStream stream = infoArray[index].OpenRead();
                        try
                        {
                            str3 = new StreamReader(stream).ReadToEnd();
                        }
                        catch (ArgumentException)
                        {
                        }
                        finally
                        {
                            if (stream != null)
                            {
                                stream.Dispose();
                            }
                        }
                        if (!string.IsNullOrEmpty(str3))
                        {
                            XmlDocument document = new XmlDocument();
                            document.LoadXml(str3);
                            return document.DocumentElement;
                        }
                    }
                }
            }
            return GetCurrentCLRPermissions(targetZone);
        }

        public static PermissionSet IdentityListToPermissionSet(string[] ids)
        {
            XmlDocument document = new XmlDocument();
            XmlElement newChild = document.CreateElement("PermissionSet");
            document.AppendChild(newChild);
            foreach (string str in ids)
            {
                XmlElement element2 = document.CreateElement("IPermission");
                XmlAttribute node = document.CreateAttribute("class");
                node.Value = str;
                element2.Attributes.Append(node);
                newChild.AppendChild(element2);
            }
            return XmlToPermissionSet(newChild);
        }

        private static bool IsCertInStore(X509Certificate2 cert)
        {
            X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                if (store.Certificates.Find(X509FindType.FindByThumbprint, cert.Thumbprint, false).Count == 1)
                {
                    return true;
                }
            }
            finally
            {
                store.Close();
            }
            return false;
        }

        internal static bool ParseElementForAssemblyIdentification(SecurityElement el, out string className, out string assemblyName, out string assemblyVersion)
        {
            className = null;
            assemblyName = null;
            assemblyVersion = null;
            string str = el.Attribute("class");
            if (str == null)
            {
                return false;
            }
            if (str.IndexOf('\'') >= 0)
            {
                str = str.Replace('\'', '"');
            }
            int index = str.IndexOf(',');
            if (index == -1)
            {
                return false;
            }
            int length = index;
            className = str.Substring(0, length);
            AssemblyName name = new AssemblyName(str.Substring(index + 1));
            assemblyName = name.Name;
            assemblyVersion = name.Version.ToString();
            return true;
        }

        public static string[] PermissionSetToIdentityList(PermissionSet permissionSet)
        {
            string xml = (permissionSet != null) ? permissionSet.ToString() : "<PermissionSet/>";
            XmlDocument document = new XmlDocument();
            document.LoadXml(xml);
            return XmlToIdentityList(document.DocumentElement);
        }

        internal static XmlDocument PermissionSetToXml(PermissionSet ps)
        {
            XmlDocument document = new XmlDocument();
            string xml = (ps != null) ? ps.ToString() : "<PermissionSet/>";
            document.LoadXml(xml);
            XmlDocument document2 = new XmlDocument();
            XmlElement newChild = XmlUtil.CloneElementToDocument(document.DocumentElement, document2, "urn:schemas-microsoft-com:asm.v2");
            document2.AppendChild(newChild);
            return document2;
        }

        private static PermissionSet RemoveNonReferencedPermissions(string[] setToFilter, ITaskItem[] dependencies)
        {
            PermissionSet set = new PermissionSet(PermissionState.None);
            if (((dependencies == null) || (setToFilter == null)) || (setToFilter.Length == 0))
            {
                return set;
            }
            List<string> list = new List<string>();
            foreach (ITaskItem item in dependencies)
            {
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(item.ItemSpec);
                list.Add(assemblyName.Name + ", " + assemblyName.Version.ToString());
            }
            SecurityElement permissionSetXml = set.ToXml();
            foreach (string str in setToFilter)
            {
                string str2;
                string str3;
                string str4;
                if ((!string.IsNullOrEmpty(str) && ParseElementForAssemblyIdentification(SecurityElement.FromString(str), out str3, out str2, out str4)) && list.Contains(str2 + ", " + str4))
                {
                    permissionSetXml.AddChild(SecurityElement.FromString(str));
                }
            }
            return new ReadOnlyPermissionSet(permissionSetXml);
        }

        public static void SignFile(X509Certificate2 cert, Uri timestampUrl, string path)
        {
            ResourceManager resources = new ResourceManager("Microsoft.Build.Tasks.Deployment.ManifestUtilities.Strings", typeof(SecurityUtilities).Module.Assembly);
            if (cert == null)
            {
                throw new ArgumentNullException("cert");
            }
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("path");
            }
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(string.Format(CultureInfo.InvariantCulture, resources.GetString("SecurityUtil.SignTargetNotFound"), new object[] { path }), path);
            }
            if (PathUtil.IsPEFile(path))
            {
                if (!IsCertInStore(cert))
                {
                    throw new InvalidOperationException(resources.GetString("SignFile.CertNotInStore"));
                }
                SignPEFile(cert, timestampUrl, path, resources);
            }
            else
            {
                if (cert.PrivateKey.GetType() != typeof(RSACryptoServiceProvider))
                {
                    throw new ApplicationException(resources.GetString("SecurityUtil.OnlyRSACertsAreAllowed"));
                }
                try
                {
                    XmlDocument manifestDom = new XmlDocument {
                        PreserveWhitespace = true
                    };
                    manifestDom.Load(path);
                    System.Deployment.Internal.CodeSigning.SignedCmiManifest manifest = new System.Deployment.Internal.CodeSigning.SignedCmiManifest(manifestDom);
                    System.Deployment.Internal.CodeSigning.CmiManifestSigner signer = new System.Deployment.Internal.CodeSigning.CmiManifestSigner(cert.PrivateKey, cert);
                    if (timestampUrl == null)
                    {
                        manifest.Sign(signer);
                    }
                    else
                    {
                        manifest.Sign(signer, timestampUrl.ToString());
                    }
                    manifestDom.Save(path);
                }
                catch (Exception exception)
                {
                    int hRForException = Marshal.GetHRForException(exception);
                    if ((hRForException != -2147012889) && (hRForException != -2147012867))
                    {
                        throw new ApplicationException(exception.Message, exception);
                    }
                    throw new ApplicationException(resources.GetString("SecurityUtil.TimestampUrlNotFound"), exception);
                }
            }
        }

        public static void SignFile(string certThumbprint, Uri timestampUrl, string path)
        {
            ResourceManager manager = new ResourceManager("Microsoft.Build.Tasks.Deployment.ManifestUtilities.Strings", typeof(SecurityUtilities).Module.Assembly);
            if (string.IsNullOrEmpty(certThumbprint))
            {
                throw new ArgumentNullException("certThumbprint");
            }
            X509Certificate2 cert = GetCert(certThumbprint);
            if (cert == null)
            {
                throw new ArgumentException(manager.GetString("CertNotInStore"), "certThumbprint");
            }
            SignFile(cert, timestampUrl, path);
        }

        public static void SignFile(string certPath, SecureString certPassword, Uri timestampUrl, string path)
        {
            X509Certificate2 cert = new X509Certificate2(certPath, certPassword, X509KeyStorageFlags.PersistKeySet);
            SignFile(cert, timestampUrl, path);
        }

        private static void SignPEFile(X509Certificate2 cert, Uri timestampUrl, string path, ResourceManager resources)
        {
            if (GetPathToTool() == null)
            {
                throw new ApplicationException(resources.GetString("SecurityUtil.SigntoolNotFound"));
            }
            ProcessStartInfo startInfo = new ProcessStartInfo(GetPathToTool(), GetCommandLineParameters(cert.Thumbprint, timestampUrl, path)) {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };
            Process process = null;
            try
            {
                process = Process.Start(startInfo);
                process.WaitForExit();
                while (!process.HasExited)
                {
                    Thread.Sleep(50);
                }
                switch (process.ExitCode)
                {
                    case 0:
                        return;

                    case 1:
                        throw new ApplicationException(string.Format(CultureInfo.InvariantCulture, resources.GetString("SecurityUtil.SigntoolFail"), new object[] { path, process.StandardError.ReadToEnd() }));

                    case 2:
                        throw new WarningException(string.Format(CultureInfo.InvariantCulture, resources.GetString("SecurityUtil.SigntoolWarning"), new object[] { path, process.StandardError.ReadToEnd() }));
                }
                throw new ApplicationException(string.Format(CultureInfo.InvariantCulture, resources.GetString("SecurityUtil.SigntoolFail"), new object[] { path, process.StandardError.ReadToEnd() }));
            }
            finally
            {
                if (process != null)
                {
                    process.Close();
                }
            }
        }

        private static string ToolName()
        {
            return "signtool.exe";
        }

        private static SecurityElement XmlElementToSecurityElement(XmlElement xe)
        {
            SecurityElement element = new SecurityElement(xe.Name);
            foreach (XmlAttribute attribute in xe.Attributes)
            {
                element.AddAttribute(attribute.Name, attribute.Value);
            }
            foreach (XmlNode node in xe.ChildNodes)
            {
                if (node.NodeType == XmlNodeType.Element)
                {
                    element.AddChild(XmlElementToSecurityElement((XmlElement) node));
                }
            }
            return element;
        }

        private static string[] XmlToIdentityList(XmlElement psElement)
        {
            XmlNamespaceManager namespaceManager = XmlNamespaces.GetNamespaceManager(psElement.OwnerDocument.NameTable);
            XmlNodeList list = psElement.SelectNodes("asmv2:IPermission/@class", namespaceManager);
            if ((list == null) || (list.Count == 0))
            {
                list = psElement.SelectNodes(XmlUtil.TrimPrefix("asmv2:IPermission/@class"));
            }
            if (list != null)
            {
                string[] strArray = new string[list.Count];
                int num = 0;
                foreach (XmlNode node in list)
                {
                    strArray[num++] = node.Value;
                }
                return strArray;
            }
            return new string[0];
        }

        public static PermissionSet XmlToPermissionSet(XmlElement element)
        {
            if (element == null)
            {
                return null;
            }
            SecurityElement permissionSetXml = XmlElementToSecurityElement(element);
            if (permissionSetXml == null)
            {
                return null;
            }
            PermissionSet set = new PermissionSet(PermissionState.None);
            try
            {
                set = new ReadOnlyPermissionSet(permissionSetXml);
            }
            catch (ArgumentException)
            {
                return null;
            }
            return set;
        }
    }
}

