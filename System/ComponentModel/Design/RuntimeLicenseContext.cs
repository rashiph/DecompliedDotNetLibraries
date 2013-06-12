namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    internal class RuntimeLicenseContext : LicenseContext
    {
        private const int ReadBlock = 400;
        private static TraceSwitch RuntimeLicenseContextSwitch = new TraceSwitch("RuntimeLicenseContextTrace", "RuntimeLicenseContext tracing");
        internal Hashtable savedLicenseKeys;

        private Stream CaseInsensitiveManifestResourceStreamLookup(Assembly satellite, string name)
        {
            CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            string str = satellite.GetName().Name;
            foreach (string str2 in satellite.GetManifestResourceNames())
            {
                if (((compareInfo.Compare(str2, name, CompareOptions.IgnoreCase) == 0) || (compareInfo.Compare(str2, str + ".exe.licenses") == 0)) || (compareInfo.Compare(str2, str + ".dll.licenses") == 0))
                {
                    name = str2;
                    break;
                }
            }
            return satellite.GetManifestResourceStream(name);
        }

        private string GetLocalPath(string fileName)
        {
            Uri uri = new Uri(fileName);
            return (uri.LocalPath + uri.Fragment);
        }

        public override string GetSavedLicenseKey(Type type, Assembly resourceAssembly)
        {
            if ((this.savedLicenseKeys == null) || (this.savedLicenseKeys[type.AssemblyQualifiedName] == null))
            {
                if (this.savedLicenseKeys == null)
                {
                    this.savedLicenseKeys = new Hashtable();
                }
                Uri resourceUri = null;
                if (resourceAssembly == null)
                {
                    string applicationBase;
                    string licenseFile = AppDomain.CurrentDomain.SetupInformation.LicenseFile;
                    new FileIOPermission(PermissionState.Unrestricted).Assert();
                    try
                    {
                        applicationBase = AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                    if ((licenseFile != null) && (applicationBase != null))
                    {
                        resourceUri = new Uri(new Uri(applicationBase), licenseFile);
                    }
                }
                if (resourceUri == null)
                {
                    if (resourceAssembly == null)
                    {
                        resourceAssembly = Assembly.GetEntryAssembly();
                    }
                    if (resourceAssembly == null)
                    {
                        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            if (!assembly.IsDynamic)
                            {
                                string localPath;
                                new FileIOPermission(PermissionState.Unrestricted).Assert();
                                try
                                {
                                    localPath = this.GetLocalPath(assembly.EscapedCodeBase);
                                    localPath = new FileInfo(localPath).Name;
                                }
                                finally
                                {
                                    CodeAccessPermission.RevertAssert();
                                }
                                Stream manifestResourceStream = assembly.GetManifestResourceStream(localPath + ".licenses");
                                if (manifestResourceStream == null)
                                {
                                    manifestResourceStream = this.CaseInsensitiveManifestResourceStreamLookup(assembly, localPath + ".licenses");
                                }
                                if (manifestResourceStream != null)
                                {
                                    DesigntimeLicenseContextSerializer.Deserialize(manifestResourceStream, localPath.ToUpper(CultureInfo.InvariantCulture), this);
                                    break;
                                }
                            }
                        }
                    }
                    else if (!resourceAssembly.IsDynamic)
                    {
                        string fileName;
                        new FileIOPermission(PermissionState.Unrestricted).Assert();
                        try
                        {
                            fileName = this.GetLocalPath(resourceAssembly.EscapedCodeBase);
                        }
                        finally
                        {
                            CodeAccessPermission.RevertAssert();
                        }
                        fileName = Path.GetFileName(fileName);
                        string name = fileName + ".licenses";
                        Stream o = resourceAssembly.GetManifestResourceStream(name);
                        if (o == null)
                        {
                            string str6 = null;
                            CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
                            string str7 = resourceAssembly.GetName().Name;
                            foreach (string str8 in resourceAssembly.GetManifestResourceNames())
                            {
                                if (((compareInfo.Compare(str8, name, CompareOptions.IgnoreCase) == 0) || (compareInfo.Compare(str8, str7 + ".exe.licenses", CompareOptions.IgnoreCase) == 0)) || (compareInfo.Compare(str8, str7 + ".dll.licenses", CompareOptions.IgnoreCase) == 0))
                                {
                                    str6 = str8;
                                    break;
                                }
                            }
                            if (str6 != null)
                            {
                                o = resourceAssembly.GetManifestResourceStream(str6);
                            }
                        }
                        if (o != null)
                        {
                            DesigntimeLicenseContextSerializer.Deserialize(o, fileName.ToUpper(CultureInfo.InvariantCulture), this);
                        }
                    }
                }
                if (resourceUri != null)
                {
                    Stream stream3 = OpenRead(resourceUri);
                    if (stream3 != null)
                    {
                        string[] segments = resourceUri.Segments;
                        string str9 = segments[segments.Length - 1];
                        string str10 = str9.Substring(0, str9.LastIndexOf("."));
                        DesigntimeLicenseContextSerializer.Deserialize(stream3, str10.ToUpper(CultureInfo.InvariantCulture), this);
                    }
                }
            }
            return (string) this.savedLicenseKeys[type.AssemblyQualifiedName];
        }

        private static Stream OpenRead(Uri resourceUri)
        {
            Stream stream = null;
            new PermissionSet(PermissionState.Unrestricted).Assert();
            try
            {
                stream = new WebClient { Credentials = CredentialCache.DefaultCredentials }.OpenRead(resourceUri.ToString());
            }
            catch (Exception)
            {
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
            return stream;
        }
    }
}

