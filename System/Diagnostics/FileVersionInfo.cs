namespace System.Diagnostics
{
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public sealed class FileVersionInfo
    {
        private string comments;
        private string companyName;
        private int fileBuild;
        private string fileDescription;
        private int fileFlags;
        private int fileMajor;
        private int fileMinor;
        private string fileName;
        private int filePrivate;
        private string fileVersion;
        private string internalName;
        private string language;
        private string legalCopyright;
        private string legalTrademarks;
        private string originalFilename;
        private string privateBuild;
        private int productBuild;
        private int productMajor;
        private int productMinor;
        private string productName;
        private int productPrivate;
        private string productVersion;
        private string specialBuild;

        private FileVersionInfo(string fileName)
        {
            this.fileName = fileName;
        }

        private static string ConvertTo8DigitHex(int value)
        {
            string str = Convert.ToString(value, 0x10).ToUpper(CultureInfo.InvariantCulture);
            if (str.Length == 8)
            {
                return str;
            }
            StringBuilder builder = new StringBuilder(8);
            for (int i = str.Length; i < 8; i++)
            {
                builder.Append("0");
            }
            builder.Append(str);
            return builder.ToString();
        }

        private static string GetFileVersionLanguage(IntPtr memPtr)
        {
            int langID = GetVarEntry(memPtr) >> 0x10;
            StringBuilder lpBuffer = new StringBuilder(0x100);
            Microsoft.Win32.UnsafeNativeMethods.VerLanguageName(langID, lpBuffer, lpBuffer.Capacity);
            return lpBuffer.ToString();
        }

        private static string GetFileVersionString(IntPtr memPtr, string name)
        {
            int num;
            string str = "";
            IntPtr zero = IntPtr.Zero;
            if (Microsoft.Win32.UnsafeNativeMethods.VerQueryValue(new HandleRef(null, memPtr), name, ref zero, out num) && (zero != IntPtr.Zero))
            {
                str = Marshal.PtrToStringAuto(zero);
            }
            return str;
        }

        private static Microsoft.Win32.NativeMethods.VS_FIXEDFILEINFO GetFixedFileInfo(IntPtr memPtr)
        {
            int num;
            IntPtr zero = IntPtr.Zero;
            if (Microsoft.Win32.UnsafeNativeMethods.VerQueryValue(new HandleRef(null, memPtr), @"\", ref zero, out num))
            {
                Microsoft.Win32.NativeMethods.VS_FIXEDFILEINFO structure = new Microsoft.Win32.NativeMethods.VS_FIXEDFILEINFO();
                Marshal.PtrToStructure(zero, structure);
                return structure;
            }
            return new Microsoft.Win32.NativeMethods.VS_FIXEDFILEINFO();
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery)]
        private static string GetFullPathWithAssert(string fileName)
        {
            return Path.GetFullPath(fileName);
        }

        private static int GetVarEntry(IntPtr memPtr)
        {
            int num;
            IntPtr zero = IntPtr.Zero;
            if (Microsoft.Win32.UnsafeNativeMethods.VerQueryValue(new HandleRef(null, memPtr), @"\VarFileInfo\Translation", ref zero, out num))
            {
                return ((Marshal.ReadInt16(zero) << 0x10) + Marshal.ReadInt16((IntPtr) (((long) zero) + 2L)));
            }
            return 0x40904e4;
        }

        public static unsafe FileVersionInfo GetVersionInfo(string fileName)
        {
            int num;
            if (!File.Exists(fileName))
            {
                string fullPathWithAssert = GetFullPathWithAssert(fileName);
                new FileIOPermission(FileIOPermissionAccess.Read, fullPathWithAssert).Demand();
                throw new FileNotFoundException(fileName);
            }
            int fileVersionInfoSize = Microsoft.Win32.UnsafeNativeMethods.GetFileVersionInfoSize(fileName, out num);
            FileVersionInfo info = new FileVersionInfo(fileName);
            if (fileVersionInfoSize != 0)
            {
                byte[] buffer = new byte[fileVersionInfoSize];
                fixed (byte* numRef = buffer)
                {
                    IntPtr handle = new IntPtr((void*) numRef);
                    if (!Microsoft.Win32.UnsafeNativeMethods.GetFileVersionInfo(fileName, 0, fileVersionInfoSize, new HandleRef(null, handle)))
                    {
                        return info;
                    }
                    int varEntry = GetVarEntry(handle);
                    if (!info.GetVersionInfoForCodePage(handle, ConvertTo8DigitHex(varEntry)))
                    {
                        int[] numArray = new int[] { 0x40904b0, 0x40904e4, 0x4090000 };
                        foreach (int num4 in numArray)
                        {
                            if ((num4 != varEntry) && info.GetVersionInfoForCodePage(handle, ConvertTo8DigitHex(num4)))
                            {
                                return info;
                            }
                        }
                    }
                }
            }
            return info;
        }

        private bool GetVersionInfoForCodePage(IntPtr memIntPtr, string codepage)
        {
            string format = @"\\StringFileInfo\\{0}\\{1}";
            this.companyName = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, format, new object[] { codepage, "CompanyName" }));
            this.fileDescription = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, format, new object[] { codepage, "FileDescription" }));
            this.fileVersion = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, format, new object[] { codepage, "FileVersion" }));
            this.internalName = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, format, new object[] { codepage, "InternalName" }));
            this.legalCopyright = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, format, new object[] { codepage, "LegalCopyright" }));
            this.originalFilename = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, format, new object[] { codepage, "OriginalFilename" }));
            this.productName = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, format, new object[] { codepage, "ProductName" }));
            this.productVersion = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, format, new object[] { codepage, "ProductVersion" }));
            this.comments = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, format, new object[] { codepage, "Comments" }));
            this.legalTrademarks = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, format, new object[] { codepage, "LegalTrademarks" }));
            this.privateBuild = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, format, new object[] { codepage, "PrivateBuild" }));
            this.specialBuild = GetFileVersionString(memIntPtr, string.Format(CultureInfo.InvariantCulture, format, new object[] { codepage, "SpecialBuild" }));
            this.language = GetFileVersionLanguage(memIntPtr);
            Microsoft.Win32.NativeMethods.VS_FIXEDFILEINFO fixedFileInfo = GetFixedFileInfo(memIntPtr);
            this.fileMajor = HIWORD(fixedFileInfo.dwFileVersionMS);
            this.fileMinor = LOWORD(fixedFileInfo.dwFileVersionMS);
            this.fileBuild = HIWORD(fixedFileInfo.dwFileVersionLS);
            this.filePrivate = LOWORD(fixedFileInfo.dwFileVersionLS);
            this.productMajor = HIWORD(fixedFileInfo.dwProductVersionMS);
            this.productMinor = LOWORD(fixedFileInfo.dwProductVersionMS);
            this.productBuild = HIWORD(fixedFileInfo.dwProductVersionLS);
            this.productPrivate = LOWORD(fixedFileInfo.dwProductVersionLS);
            this.fileFlags = fixedFileInfo.dwFileFlags;
            return (this.fileVersion != string.Empty);
        }

        private static int HIWORD(int dword)
        {
            return Microsoft.Win32.NativeMethods.Util.HIWORD(dword);
        }

        private static int LOWORD(int dword)
        {
            return Microsoft.Win32.NativeMethods.Util.LOWORD(dword);
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder(0x80);
            string str = "\r\n";
            builder.Append("File:             ");
            builder.Append(this.FileName);
            builder.Append(str);
            builder.Append("InternalName:     ");
            builder.Append(this.InternalName);
            builder.Append(str);
            builder.Append("OriginalFilename: ");
            builder.Append(this.OriginalFilename);
            builder.Append(str);
            builder.Append("FileVersion:      ");
            builder.Append(this.FileVersion);
            builder.Append(str);
            builder.Append("FileDescription:  ");
            builder.Append(this.FileDescription);
            builder.Append(str);
            builder.Append("Product:          ");
            builder.Append(this.ProductName);
            builder.Append(str);
            builder.Append("ProductVersion:   ");
            builder.Append(this.ProductVersion);
            builder.Append(str);
            builder.Append("Debug:            ");
            builder.Append(this.IsDebug.ToString());
            builder.Append(str);
            builder.Append("Patched:          ");
            builder.Append(this.IsPatched.ToString());
            builder.Append(str);
            builder.Append("PreRelease:       ");
            builder.Append(this.IsPreRelease.ToString());
            builder.Append(str);
            builder.Append("PrivateBuild:     ");
            builder.Append(this.IsPrivateBuild.ToString());
            builder.Append(str);
            builder.Append("SpecialBuild:     ");
            builder.Append(this.IsSpecialBuild.ToString());
            builder.Append(str);
            builder.Append("Language:         ");
            builder.Append(this.Language);
            builder.Append(str);
            return builder.ToString();
        }

        public string Comments
        {
            get
            {
                return this.comments;
            }
        }

        public string CompanyName
        {
            get
            {
                return this.companyName;
            }
        }

        public int FileBuildPart
        {
            get
            {
                return this.fileBuild;
            }
        }

        public string FileDescription
        {
            get
            {
                return this.fileDescription;
            }
        }

        public int FileMajorPart
        {
            get
            {
                return this.fileMajor;
            }
        }

        public int FileMinorPart
        {
            get
            {
                return this.fileMinor;
            }
        }

        public string FileName
        {
            get
            {
                new FileIOPermission(FileIOPermissionAccess.PathDiscovery, this.fileName).Demand();
                return this.fileName;
            }
        }

        public int FilePrivatePart
        {
            get
            {
                return this.filePrivate;
            }
        }

        public string FileVersion
        {
            get
            {
                return this.fileVersion;
            }
        }

        public string InternalName
        {
            get
            {
                return this.internalName;
            }
        }

        public bool IsDebug
        {
            get
            {
                return ((this.fileFlags & 1) != 0);
            }
        }

        public bool IsPatched
        {
            get
            {
                return ((this.fileFlags & 4) != 0);
            }
        }

        public bool IsPreRelease
        {
            get
            {
                return ((this.fileFlags & 2) != 0);
            }
        }

        public bool IsPrivateBuild
        {
            get
            {
                return ((this.fileFlags & 8) != 0);
            }
        }

        public bool IsSpecialBuild
        {
            get
            {
                return ((this.fileFlags & 0x20) != 0);
            }
        }

        public string Language
        {
            get
            {
                return this.language;
            }
        }

        public string LegalCopyright
        {
            get
            {
                return this.legalCopyright;
            }
        }

        public string LegalTrademarks
        {
            get
            {
                return this.legalTrademarks;
            }
        }

        public string OriginalFilename
        {
            get
            {
                return this.originalFilename;
            }
        }

        public string PrivateBuild
        {
            get
            {
                return this.privateBuild;
            }
        }

        public int ProductBuildPart
        {
            get
            {
                return this.productBuild;
            }
        }

        public int ProductMajorPart
        {
            get
            {
                return this.productMajor;
            }
        }

        public int ProductMinorPart
        {
            get
            {
                return this.productMinor;
            }
        }

        public string ProductName
        {
            get
            {
                return this.productName;
            }
        }

        public int ProductPrivatePart
        {
            get
            {
                return this.productPrivate;
            }
        }

        public string ProductVersion
        {
            get
            {
                return this.productVersion;
            }
        }

        public string SpecialBuild
        {
            get
            {
                return this.specialBuild;
            }
        }
    }
}

