namespace System.EnterpriseServices.Internal
{
    using Microsoft.Runtime.Hosting;
    using Microsoft.Win32;
    using System;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Security.Permissions;

    [Guid("d8013ff1-730b-45e2-ba24-874b7242c425")]
    public class GenerateMetadata : IComSoapMetadata
    {
        internal bool _nameonly;
        internal bool _signed;

        public string Generate(string strSrcTypeLib, string outPath)
        {
            return this.GenerateMetaData(strSrcTypeLib, outPath, null, null);
        }

        public string GenerateMetaData(string strSrcTypeLib, string outPath, byte[] PublicKey, StrongNameKeyPair KeyPair)
        {
            string str2;
            try
            {
                new SecurityPermission(SecurityPermissionFlag.UnmanagedCode).Demand();
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception.ToString());
                throw;
            }
            string str = "";
            if ((0 >= strSrcTypeLib.Length) || (0 >= outPath.Length))
            {
                return str;
            }
            if (!outPath.EndsWith("/", StringComparison.Ordinal) && !outPath.EndsWith(@"\", StringComparison.Ordinal))
            {
                outPath = outPath + @"\";
            }
            ITypeLib typeLib = null;
            typeLib = CacheInfo.GetTypeLib(strSrcTypeLib);
            if (typeLib == null)
            {
                return str;
            }
            str = CacheInfo.GetMetadataName(strSrcTypeLib, typeLib, out str2);
            if (str.Length == 0)
            {
                return str;
            }
            if (this._nameonly)
            {
                return str;
            }
            string assemblyPath = outPath + str2;
            if (this._signed)
            {
                try
                {
                    AssemblyManager manager = new AssemblyManager();
                    if (manager.CompareToCache(assemblyPath, strSrcTypeLib))
                    {
                        new Publish().GacInstall(assemblyPath);
                        return str;
                    }
                    if (manager.GetFromCache(assemblyPath, strSrcTypeLib))
                    {
                        new Publish().GacInstall(assemblyPath);
                        return str;
                    }
                    goto Label_0133;
                }
                catch (Exception exception2)
                {
                    if ((exception2 is NullReferenceException) || (exception2 is SEHException))
                    {
                        throw;
                    }
                    ComSoapPublishError.Report(exception2.ToString());
                    goto Label_0133;
                }
            }
            if (File.Exists(assemblyPath))
            {
                return str;
            }
        Label_0133:
            try
            {
                ITypeLibConverter converter = new TypeLibConverter();
                ImporterCallback notifySink = new ImporterCallback {
                    OutputDir = outPath
                };
                AssemblyBuilder builder = converter.ConvertTypeLibToAssembly(typeLib, assemblyPath, TypeLibImporterFlags.UnsafeInterfaces, notifySink, PublicKey, KeyPair, null, null);
                FileInfo info = new FileInfo(assemblyPath);
                builder.Save(info.Name);
                if (this._signed)
                {
                    new AssemblyManager().CopyToCache(assemblyPath, strSrcTypeLib);
                    new Publish().GacInstall(assemblyPath);
                }
            }
            catch (ReflectionTypeLoadException exception3)
            {
                Exception[] loaderExceptions = exception3.LoaderExceptions;
                for (int i = 0; i < loaderExceptions.Length; i++)
                {
                    try
                    {
                        ComSoapPublishError.Report(loaderExceptions[i].ToString());
                    }
                    catch (Exception exception4)
                    {
                        if ((exception4 is NullReferenceException) || (exception4 is SEHException))
                        {
                            throw;
                        }
                        ComSoapPublishError.Report(exception3.ToString());
                    }
                }
                return string.Empty;
            }
            catch (Exception exception5)
            {
                if ((exception5 is NullReferenceException) || (exception5 is SEHException))
                {
                    throw;
                }
                ComSoapPublishError.Report(exception5.ToString());
                return string.Empty;
            }
            return str;
        }

        public string GenerateSigned(string strSrcTypeLib, string outPath, bool InstallGac, out string Error)
        {
            string str = "";
            this._signed = true;
            try
            {
                Error = "";
                string pwzKeyContainer = strSrcTypeLib;
                int dwFlags = 0;
                IntPtr zero = IntPtr.Zero;
                int pcbKeyBlob = 0;
                Microsoft.Runtime.Hosting.StrongNameHelpers.StrongNameKeyGen(pwzKeyContainer, dwFlags, out zero, out pcbKeyBlob);
                byte[] destination = new byte[pcbKeyBlob];
                Marshal.Copy(zero, destination, 0, pcbKeyBlob);
                Microsoft.Runtime.Hosting.StrongNameHelpers.StrongNameFreeBuffer(zero);
                StrongNameKeyPair keyPair = new StrongNameKeyPair(destination);
                str = this.GenerateMetaData(strSrcTypeLib, outPath, null, keyPair);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                Error = exception.ToString();
                ComSoapPublishError.Report(Error);
            }
            return str;
        }

        internal string GetAssemblyName(string strSrcTypeLib, string outPath)
        {
            this._nameonly = true;
            return this.Generate(strSrcTypeLib, outPath);
        }

        [DllImport("kernel32.dll", CharSet=CharSet.Unicode, SetLastError=true)]
        public static extern int SearchPath(string path, string fileName, string extension, int numBufferChars, string buffer, int[] filePart);

        internal class ImporterCallback : ITypeLibImporterNotifySink
        {
            private string m_strOutputDir = "";

            internal string GetTlbPath(string guidAttr, string strMajorVer, string strMinorVer)
            {
                string name = @"TypeLib\{" + guidAttr + @"}\" + strMajorVer + "." + strMinorVer + @"\0\win32";
                RegistryKey key = Registry.ClassesRoot.OpenSubKey(name);
                if (key == null)
                {
                    throw new COMException(Resource.FormatString("Soap_ResolutionForTypeLibFailed") + " " + guidAttr, -2147221164);
                }
                return (string) key.GetValue("");
            }

            public void ReportEvent(ImporterEventKind EventKind, int EventCode, string EventMsg)
            {
            }

            public Assembly ResolveRef(object TypeLib)
            {
                Assembly assembly = null;
                IntPtr zero = IntPtr.Zero;
                try
                {
                    ((ITypeLib) TypeLib).GetLibAttr(out zero);
                    System.Runtime.InteropServices.ComTypes.TYPELIBATTR typelibattr = (System.Runtime.InteropServices.ComTypes.TYPELIBATTR) Marshal.PtrToStructure(zero, typeof(System.Runtime.InteropServices.ComTypes.TYPELIBATTR));
                    string guidAttr = typelibattr.guid.ToString(string.Empty, CultureInfo.InvariantCulture);
                    string strMajorVer = typelibattr.wMajorVerNum.ToString(CultureInfo.InvariantCulture);
                    string strSrcTypeLib = this.GetTlbPath(guidAttr, strMajorVer, typelibattr.wMinorVerNum.ToString(CultureInfo.InvariantCulture));
                    if (strSrcTypeLib.Length > 0)
                    {
                        GenerateMetadata metadata = new GenerateMetadata();
                        string error = "";
                        string assemblyString = metadata.GenerateSigned(strSrcTypeLib, this.m_strOutputDir, true, out error);
                        if (assemblyString.Length > 0)
                        {
                            assembly = Assembly.Load(assemblyString);
                        }
                    }
                }
                finally
                {
                    if (zero != IntPtr.Zero)
                    {
                        ((ITypeLib) TypeLib).ReleaseTLibAttr(zero);
                    }
                }
                if (null == assembly)
                {
                    ComSoapPublishError.Report(Resource.FormatString("Soap_ResolutionForTypeLibFailed") + " " + Marshal.GetTypeLibName((ITypeLib) TypeLib));
                }
                return assembly;
            }

            internal string OutputDir
            {
                set
                {
                    this.m_strOutputDir = value;
                }
            }
        }
    }
}

