namespace System.Configuration
{
    using Microsoft.Win32;
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Text;

    internal class ClientConfigPaths
    {
        private string _applicationConfigUri;
        private string _applicationUri;
        private string _companyName;
        private bool _hasEntryAssembly;
        private bool _includesUserConfig;
        private string _localConfigDirectory;
        private string _localConfigFilename;
        private string _productName;
        private string _productVersion;
        private string _roamingConfigDirectory;
        private string _roamingConfigFilename;
        private const string ClickOnceDataDirectory = "DataDirectory";
        private const string ConfigExtension = ".config";
        private const string FILE_URI = "file:";
        private const string FILE_URI_LOCAL = "file:///";
        private const string FILE_URI_UNC = "file://";
        private const string HTTP_URI = "http://";
        private const int MAX_LENGTH_TO_USE = 0x19;
        private const int MAX_PATH = 260;
        private const string PathDesc = "Path";
        private static char[] s_Base32Char = new char[] { 
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 
            'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5'
         };
        private static SecurityPermission s_controlEvidencePerm;
        private static volatile ClientConfigPaths s_current;
        private static volatile bool s_currentIncludesUserConfig;
        private static SecurityPermission s_serializationPerm;
        private const string StrongNameDesc = "StrongName";
        private const string UrlDesc = "Url";
        internal const string UserConfigFilename = "user.config";

        [SecurityPermission(SecurityAction.Assert, UnmanagedCode=true), FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read)]
        private ClientConfigPaths(string exePath, bool includeUserConfig)
        {
            this._includesUserConfig = includeUserConfig;
            Assembly exeAssembly = null;
            string codeBase = null;
            string applicationFilename = null;
            if (exePath == null)
            {
                AppDomainSetup setupInformation = AppDomain.CurrentDomain.SetupInformation;
                this._applicationConfigUri = setupInformation.ConfigurationFile;
                exeAssembly = Assembly.GetEntryAssembly();
                if (exeAssembly != null)
                {
                    this._hasEntryAssembly = true;
                    codeBase = exeAssembly.CodeBase;
                    bool flag = false;
                    if (StringUtil.StartsWithIgnoreCase(codeBase, "file:///"))
                    {
                        flag = true;
                        codeBase = codeBase.Substring("file:///".Length);
                    }
                    else if (StringUtil.StartsWithIgnoreCase(codeBase, "file://"))
                    {
                        flag = true;
                        codeBase = codeBase.Substring("file:".Length);
                    }
                    if (flag)
                    {
                        codeBase = codeBase.Replace('/', '\\');
                        applicationFilename = codeBase;
                    }
                    else
                    {
                        codeBase = exeAssembly.EscapedCodeBase;
                    }
                }
                else
                {
                    StringBuilder buffer = new StringBuilder(260);
                    Microsoft.Win32.UnsafeNativeMethods.GetModuleFileName(new HandleRef(null, IntPtr.Zero), buffer, buffer.Capacity);
                    codeBase = Path.GetFullPath(buffer.ToString());
                    applicationFilename = codeBase;
                }
            }
            else
            {
                codeBase = Path.GetFullPath(exePath);
                if (!FileUtil.FileExists(codeBase, false))
                {
                    throw ExceptionUtil.ParameterInvalid("exePath");
                }
                applicationFilename = codeBase;
            }
            if (this._applicationConfigUri == null)
            {
                this._applicationConfigUri = codeBase + ".config";
            }
            this._applicationUri = codeBase;
            if ((exePath == null) && this._includesUserConfig)
            {
                bool isHttp = StringUtil.StartsWithIgnoreCase(this._applicationConfigUri, "http://");
                this.SetNamesAndVersion(applicationFilename, exeAssembly, isHttp);
                if (this.IsClickOnceDeployed(AppDomain.CurrentDomain))
                {
                    string data = AppDomain.CurrentDomain.GetData("DataDirectory") as string;
                    string str4 = this.Validate(this._productVersion, false);
                    if (Path.IsPathRooted(data))
                    {
                        this._localConfigDirectory = this.CombineIfValid(data, str4);
                        this._localConfigFilename = this.CombineIfValid(this._localConfigDirectory, "user.config");
                    }
                }
                else if (!isHttp)
                {
                    string str5 = this.Validate(this._companyName, true);
                    string str6 = this.Validate(AppDomain.CurrentDomain.FriendlyName, true);
                    string str7 = !string.IsNullOrEmpty(this._applicationUri) ? this._applicationUri.ToLower(CultureInfo.InvariantCulture) : null;
                    string str8 = !string.IsNullOrEmpty(str6) ? str6 : this.Validate(this._productName, true);
                    string typeAndHashSuffix = this.GetTypeAndHashSuffix(AppDomain.CurrentDomain, str7);
                    string str10 = (!string.IsNullOrEmpty(str8) && !string.IsNullOrEmpty(typeAndHashSuffix)) ? (str8 + typeAndHashSuffix) : null;
                    string str11 = this.Validate(this._productVersion, false);
                    string str12 = this.CombineIfValid(this.CombineIfValid(str5, str10), str11);
                    string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                    if (Path.IsPathRooted(folderPath))
                    {
                        this._roamingConfigDirectory = this.CombineIfValid(folderPath, str12);
                        this._roamingConfigFilename = this.CombineIfValid(this._roamingConfigDirectory, "user.config");
                    }
                    string path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                    if (Path.IsPathRooted(path))
                    {
                        this._localConfigDirectory = this.CombineIfValid(path, str12);
                        this._localConfigFilename = this.CombineIfValid(this._localConfigDirectory, "user.config");
                    }
                }
            }
        }

        private string CombineIfValid(string path1, string path2)
        {
            string str = null;
            if ((path1 != null) && (path2 != null))
            {
                try
                {
                    string str2 = Path.Combine(path1, path2);
                    if (str2.Length < 260)
                    {
                        str = str2;
                    }
                }
                catch
                {
                }
            }
            return str;
        }

        private static object GetEvidenceInfo(AppDomain appDomain, string exePath, out string typeName)
        {
            ControlEvidencePermission.Assert();
            Evidence evidence = appDomain.Evidence;
            StrongName sn = null;
            Url url = null;
            if (evidence != null)
            {
                IEnumerator hostEnumerator = evidence.GetHostEnumerator();
                object current = null;
                while (hostEnumerator.MoveNext())
                {
                    current = hostEnumerator.Current;
                    if (current is StrongName)
                    {
                        sn = (StrongName) current;
                        break;
                    }
                    if (current is Url)
                    {
                        url = (Url) current;
                    }
                }
            }
            object obj3 = null;
            if (sn != null)
            {
                obj3 = MakeVersionIndependent(sn);
                typeName = "StrongName";
                return obj3;
            }
            if (url != null)
            {
                obj3 = url.Value.ToUpperInvariant();
                typeName = "Url";
                return obj3;
            }
            if (exePath != null)
            {
                obj3 = exePath;
                typeName = "Path";
                return obj3;
            }
            typeName = null;
            return obj3;
        }

        private static string GetHash(Stream s)
        {
            byte[] buffer;
            using (SHA1 sha = new SHA1CryptoServiceProvider())
            {
                buffer = sha.ComputeHash(s);
            }
            return ToBase32StringSuitableForDirName(buffer);
        }

        internal static ClientConfigPaths GetPaths(string exePath, bool includeUserConfig)
        {
            if (exePath == null)
            {
                if ((s_current == null) || (includeUserConfig && !s_currentIncludesUserConfig))
                {
                    s_current = new ClientConfigPaths(null, includeUserConfig);
                    s_currentIncludesUserConfig = includeUserConfig;
                }
                return s_current;
            }
            return new ClientConfigPaths(exePath, includeUserConfig);
        }

        private string GetTypeAndHashSuffix(AppDomain appDomain, string exePath)
        {
            string str = null;
            string typeName = null;
            object graph = null;
            graph = GetEvidenceInfo(appDomain, exePath, out typeName);
            if ((graph != null) && !string.IsNullOrEmpty(typeName))
            {
                MemoryStream serializationStream = new MemoryStream();
                BinaryFormatter formatter = new BinaryFormatter();
                SerializationFormatterPermission.Assert();
                formatter.Serialize(serializationStream, graph);
                serializationStream.Position = 0L;
                string hash = GetHash(serializationStream);
                if (!string.IsNullOrEmpty(hash))
                {
                    str = "_" + typeName + "_" + hash;
                }
            }
            return str;
        }

        private bool IsClickOnceDeployed(AppDomain appDomain)
        {
            ActivationContext activationContext = appDomain.ActivationContext;
            return (((activationContext != null) && (activationContext.Form == ActivationContext.ContextForm.StoreBounded)) && !string.IsNullOrEmpty(activationContext.Identity.FullName));
        }

        private static StrongName MakeVersionIndependent(StrongName sn)
        {
            return new StrongName(sn.PublicKey, sn.Name, new Version(0, 0, 0, 0));
        }

        internal static void RefreshCurrent()
        {
            s_currentIncludesUserConfig = false;
            s_current = null;
        }

        private void SetNamesAndVersion(string applicationFilename, Assembly exeAssembly, bool isHttp)
        {
            Type reflectedType = null;
            if (exeAssembly != null)
            {
                object[] customAttributes = exeAssembly.GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    this._companyName = ((AssemblyCompanyAttribute) customAttributes[0]).Company;
                    if (this._companyName != null)
                    {
                        this._companyName = this._companyName.Trim();
                    }
                }
                customAttributes = exeAssembly.GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if ((customAttributes != null) && (customAttributes.Length > 0))
                {
                    this._productName = ((AssemblyProductAttribute) customAttributes[0]).Product;
                    if (this._productName != null)
                    {
                        this._productName = this._productName.Trim();
                    }
                }
                this._productVersion = exeAssembly.GetName().Version.ToString();
                if (this._productVersion != null)
                {
                    this._productVersion = this._productVersion.Trim();
                }
            }
            if (!isHttp && ((string.IsNullOrEmpty(this._companyName) || string.IsNullOrEmpty(this._productName)) || string.IsNullOrEmpty(this._productVersion)))
            {
                string fileName = null;
                if (exeAssembly != null)
                {
                    MethodInfo entryPoint = exeAssembly.EntryPoint;
                    if (entryPoint != null)
                    {
                        reflectedType = entryPoint.ReflectedType;
                        if (reflectedType != null)
                        {
                            fileName = reflectedType.Module.FullyQualifiedName;
                        }
                    }
                }
                if (fileName == null)
                {
                    fileName = applicationFilename;
                }
                if (fileName != null)
                {
                    FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(fileName);
                    if (versionInfo != null)
                    {
                        if (string.IsNullOrEmpty(this._companyName))
                        {
                            this._companyName = versionInfo.CompanyName;
                            if (this._companyName != null)
                            {
                                this._companyName = this._companyName.Trim();
                            }
                        }
                        if (string.IsNullOrEmpty(this._productName))
                        {
                            this._productName = versionInfo.ProductName;
                            if (this._productName != null)
                            {
                                this._productName = this._productName.Trim();
                            }
                        }
                        if (string.IsNullOrEmpty(this._productVersion))
                        {
                            this._productVersion = versionInfo.ProductVersion;
                            if (this._productVersion != null)
                            {
                                this._productVersion = this._productVersion.Trim();
                            }
                        }
                    }
                }
            }
            if (string.IsNullOrEmpty(this._companyName) || string.IsNullOrEmpty(this._productName))
            {
                string str2 = null;
                if (reflectedType != null)
                {
                    str2 = reflectedType.Namespace;
                }
                if (string.IsNullOrEmpty(this._productName))
                {
                    if (str2 != null)
                    {
                        int num = str2.LastIndexOf(".", StringComparison.Ordinal);
                        if ((num != -1) && (num < (str2.Length - 1)))
                        {
                            this._productName = str2.Substring(num + 1);
                        }
                        else
                        {
                            this._productName = str2;
                        }
                        this._productName = this._productName.Trim();
                    }
                    if (string.IsNullOrEmpty(this._productName) && (reflectedType != null))
                    {
                        this._productName = reflectedType.Name.Trim();
                    }
                    if (this._productName == null)
                    {
                        this._productName = string.Empty;
                    }
                }
                if (string.IsNullOrEmpty(this._companyName))
                {
                    if (str2 != null)
                    {
                        int index = str2.IndexOf(".", StringComparison.Ordinal);
                        if (index != -1)
                        {
                            this._companyName = str2.Substring(0, index);
                        }
                        else
                        {
                            this._companyName = str2;
                        }
                        this._companyName = this._companyName.Trim();
                    }
                    if (string.IsNullOrEmpty(this._companyName))
                    {
                        this._companyName = this._productName;
                    }
                }
            }
            if (string.IsNullOrEmpty(this._productVersion))
            {
                this._productVersion = "1.0.0.0";
            }
        }

        private static string ToBase32StringSuitableForDirName(byte[] buff)
        {
            StringBuilder builder = new StringBuilder();
            int length = buff.Length;
            int num7 = 0;
            do
            {
                byte num = (num7 < length) ? buff[num7++] : ((byte) 0);
                byte num2 = (num7 < length) ? buff[num7++] : ((byte) 0);
                byte index = (num7 < length) ? buff[num7++] : ((byte) 0);
                byte num4 = (num7 < length) ? buff[num7++] : ((byte) 0);
                byte num5 = (num7 < length) ? buff[num7++] : ((byte) 0);
                builder.Append(s_Base32Char[num & 0x1f]);
                builder.Append(s_Base32Char[num2 & 0x1f]);
                builder.Append(s_Base32Char[index & 0x1f]);
                builder.Append(s_Base32Char[num4 & 0x1f]);
                builder.Append(s_Base32Char[num5 & 0x1f]);
                builder.Append(s_Base32Char[((num & 0xe0) >> 5) | ((num4 & 0x60) >> 2)]);
                builder.Append(s_Base32Char[((num2 & 0xe0) >> 5) | ((num5 & 0x60) >> 2)]);
                index = (byte) (index >> 5);
                if ((num4 & 0x80) != 0)
                {
                    index = (byte) (index | 8);
                }
                if ((num5 & 0x80) != 0)
                {
                    index = (byte) (index | 0x10);
                }
                builder.Append(s_Base32Char[index]);
            }
            while (num7 < length);
            return builder.ToString();
        }

        private string Validate(string str, bool limitSize)
        {
            string str2 = str;
            if (!string.IsNullOrEmpty(str2))
            {
                foreach (char ch in Path.GetInvalidFileNameChars())
                {
                    str2 = str2.Replace(ch, '_');
                }
                str2 = str2.Replace(' ', '_');
                if (limitSize)
                {
                    str2 = (str2.Length > 0x19) ? str2.Substring(0, 0x19) : str2;
                }
            }
            return str2;
        }

        internal string ApplicationConfigUri
        {
            get
            {
                return this._applicationConfigUri;
            }
        }

        internal string ApplicationUri
        {
            get
            {
                return this._applicationUri;
            }
        }

        private static SecurityPermission ControlEvidencePermission
        {
            get
            {
                if (s_controlEvidencePerm == null)
                {
                    s_controlEvidencePerm = new SecurityPermission(SecurityPermissionFlag.ControlEvidence);
                }
                return s_controlEvidencePerm;
            }
        }

        internal static ClientConfigPaths Current
        {
            get
            {
                return GetPaths(null, true);
            }
        }

        internal bool HasEntryAssembly
        {
            get
            {
                return this._hasEntryAssembly;
            }
        }

        internal bool HasLocalConfig
        {
            get
            {
                if (this.LocalConfigFilename == null)
                {
                    return !this._includesUserConfig;
                }
                return true;
            }
        }

        internal bool HasRoamingConfig
        {
            get
            {
                if (this.RoamingConfigFilename == null)
                {
                    return !this._includesUserConfig;
                }
                return true;
            }
        }

        internal string LocalConfigDirectory
        {
            get
            {
                return this._localConfigDirectory;
            }
        }

        internal string LocalConfigFilename
        {
            get
            {
                return this._localConfigFilename;
            }
        }

        internal string ProductName
        {
            get
            {
                return this._productName;
            }
        }

        internal string ProductVersion
        {
            get
            {
                return this._productVersion;
            }
        }

        internal string RoamingConfigDirectory
        {
            get
            {
                return this._roamingConfigDirectory;
            }
        }

        internal string RoamingConfigFilename
        {
            get
            {
                return this._roamingConfigFilename;
            }
        }

        private static SecurityPermission SerializationFormatterPermission
        {
            get
            {
                if (s_serializationPerm == null)
                {
                    s_serializationPerm = new SecurityPermission(SecurityPermissionFlag.SerializationFormatter);
                }
                return s_serializationPerm;
            }
        }
    }
}

