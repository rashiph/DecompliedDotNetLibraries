namespace System.Deployment.Application
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;

    internal static class PathHelper
    {
        private static object _shortShimDllPath;
        private const int ERROR_FILE_NOT_FOUND = 2;
        private const int ERROR_INVALID_PARAMETER = 0x57;
        private const int MAX_PATH = 260;

        public static string GenerateRandomPath(uint segmentCount)
        {
            if (segmentCount == 0)
            {
                return null;
            }
            uint num = 11 * segmentCount;
            uint num2 = (uint) Math.Ceiling((double) (num * 0.625));
            byte[] data = new byte[num2];
            new RNGCryptoServiceProvider().GetBytes(data);
            string str = Base32String.FromBytes(data);
            if (str.Length < num)
            {
                throw new DeploymentException(Resources.GetString("Ex_TempPathRandomStringTooShort"));
            }
            if (str.IndexOf('\\') >= 0)
            {
                throw new DeploymentException(Resources.GetString("Ex_TempPathRandomStringInvalid"));
            }
            for (int i = ((int) segmentCount) - 1; i > 0; i--)
            {
                int startIndex = i * 11;
                if (startIndex >= str.Length)
                {
                    throw new DeploymentException(Resources.GetString("Ex_TempPathRandomStringInvalid"));
                }
                str = str.Insert(startIndex, @"\");
            }
            string[] strArray = str.Split(new char[] { '\\' });
            if (strArray.Length < segmentCount)
            {
                throw new DeploymentException(Resources.GetString("Ex_TempPathRandomStringInvalid"));
            }
            string str2 = null;
            for (uint j = 0; j < segmentCount; j++)
            {
                if (strArray[j].Length < 11)
                {
                    throw new DeploymentException(Resources.GetString("Ex_TempPathRandomStringInvalid"));
                }
                string str3 = strArray[j].Substring(0, 11).Insert(8, ".");
                if (str2 == null)
                {
                    str2 = str3;
                }
                else
                {
                    str2 = Path.Combine(str2, str3);
                }
            }
            return str2;
        }

        public static string GetRootSegmentPath(string path, uint segmentCount)
        {
            if (segmentCount == 0)
            {
                throw new ArgumentException("segmentCount");
            }
            if (segmentCount == 1)
            {
                return path;
            }
            return GetRootSegmentPath(Path.GetDirectoryName(path), segmentCount - 1);
        }

        public static string GetShortPath(string longPath)
        {
            StringBuilder shortPath = new StringBuilder(260);
            int num = System.Deployment.Application.NativeMethods.GetShortPathName(longPath, shortPath, shortPath.Capacity);
            if (num == 0)
            {
                GetShortPathNameThrowExceptionForLastError(longPath);
            }
            if (num >= shortPath.Capacity)
            {
                shortPath.Capacity = num + 1;
                if (System.Deployment.Application.NativeMethods.GetShortPathName(longPath, shortPath, shortPath.Capacity) == 0)
                {
                    GetShortPathNameThrowExceptionForLastError(longPath);
                }
            }
            return shortPath.ToString();
        }

        private static void GetShortPathNameThrowExceptionForLastError(string path)
        {
            int error = Marshal.GetLastWin32Error();
            switch (error)
            {
                case 2:
                    throw new FileNotFoundException(path);

                case 0x57:
                    throw new InvalidOperationException(Resources.GetString("Ex_ShortFileNameNotSupported"));
            }
            throw new Win32Exception(error);
        }

        public static string ShortShimDllPath
        {
            get
            {
                if (_shortShimDllPath == null)
                {
                    string longPath = Path.Combine(Environment.SystemDirectory, "dfshim.dll");
                    Interlocked.CompareExchange(ref _shortShimDllPath, GetShortPath(longPath), null);
                }
                return (string) _shortShimDllPath;
            }
        }
    }
}

