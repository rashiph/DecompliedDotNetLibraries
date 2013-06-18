namespace System.EnterpriseServices.Internal
{
    using System;
    using System.EnterpriseServices;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using System.Text;

    internal static class CacheInfo
    {
        internal static string GetCacheName(string AssemblyPath, string srcTypeLib)
        {
            string str = string.Empty;
            try
            {
                FileInfo info = new FileInfo(srcTypeLib);
                string cachePath = GetCachePath(true);
                string str3 = info.Length.ToString(CultureInfo.InvariantCulture);
                string str4 = info.Name.ToString();
                string str5 = ((((info.LastWriteTime.Year.ToString(CultureInfo.InvariantCulture) + "_" + info.LastWriteTime.Month.ToString(CultureInfo.InvariantCulture)) + "_" + info.LastWriteTime.Day.ToString(CultureInfo.InvariantCulture)) + "_" + info.LastWriteTime.Hour.ToString(CultureInfo.InvariantCulture)) + "_" + info.LastWriteTime.Minute.ToString(CultureInfo.InvariantCulture)) + "_" + info.LastWriteTime.Second.ToString(CultureInfo.InvariantCulture);
                string path = str4 + "_" + str3 + "_" + str5;
                path = cachePath + path + @"\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                char[] anyOf = new char[] { '/', '\\' };
                int startIndex = AssemblyPath.LastIndexOfAny(anyOf) + 1;
                if (startIndex <= 0)
                {
                    startIndex = 0;
                }
                string str7 = AssemblyPath.Substring(startIndex, AssemblyPath.Length - startIndex);
                str = path + str7;
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                str = string.Empty;
                ComSoapPublishError.Report(exception.ToString());
            }
            return str;
        }

        internal static string GetCachePath(bool CreateDir)
        {
            StringBuilder lpBuf = new StringBuilder(0x400, 0x400);
            uint uSize = 0x400;
            Publish.GetSystemDirectory(lpBuf, uSize);
            string path = lpBuf.ToString() + @"\com\SOAPCache\";
            if (CreateDir)
            {
                try
                {
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
                catch (Exception exception)
                {
                    if ((exception is NullReferenceException) || (exception is SEHException))
                    {
                        throw;
                    }
                    ComSoapPublishError.Report(exception.ToString());
                }
            }
            return path;
        }

        internal static string GetMetadataName(string strSrcTypeLib, ITypeLib TypeLib, out string strMetaFileRoot)
        {
            string typeLibName = "";
            strMetaFileRoot = "";
            if (TypeLib == null)
            {
                TypeLib = GetTypeLib(strSrcTypeLib);
                if (TypeLib == null)
                {
                    return typeLibName;
                }
            }
            typeLibName = Marshal.GetTypeLibName(TypeLib);
            strMetaFileRoot = typeLibName + ".dll";
            char[] anyOf = new char[] { '/', '\\' };
            int startIndex = strSrcTypeLib.LastIndexOfAny(anyOf) + 1;
            if (startIndex <= 0)
            {
                startIndex = 0;
            }
            if (strSrcTypeLib.Substring(startIndex, strSrcTypeLib.Length - startIndex).ToLower(CultureInfo.InvariantCulture) == strMetaFileRoot.ToLower(CultureInfo.InvariantCulture))
            {
                typeLibName = typeLibName + "SoapLib";
                strMetaFileRoot = typeLibName + ".dll";
            }
            return typeLibName;
        }

        internal static ITypeLib GetTypeLib(string strTypeLibPath)
        {
            ITypeLib typeLib = null;
            try
            {
                LoadTypeLibEx(strTypeLibPath, REGKIND.REGKIND_NONE, out typeLib);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode == -2147312566)
                {
                    ComSoapPublishError.Report(Resource.FormatString("Soap_InputFileNotValidTypeLib") + " " + strTypeLibPath);
                }
                else
                {
                    ComSoapPublishError.Report(exception.ToString());
                }
                return null;
            }
            return typeLib;
        }

        [DllImport("oleaut32.dll", CharSet=CharSet.Unicode)]
        private static extern void LoadTypeLibEx(string strTypeLibName, REGKIND regKind, out ITypeLib TypeLib);
    }
}

