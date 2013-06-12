namespace System.Web.Util
{
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web;
    using System.Web.Hosting;

    internal sealed class Misc
    {
        private const string APPLICATION_ID = "\r\n\r\nApplication ID: ";
        private const string EXCEPTION = "\r\n\r\nException: ";
        private const string INNER_EXCEPTION = "\r\n\r\nInnerException: ";
        private const string MESSAGE = "\r\n\r\nMessage: ";
        private const string PROCESS_ID = "\r\n\r\nProcess ID: ";
        private static StringComparer s_caseInsensitiveInvariantKeyComparer;
        private const string STACK_TRACE = "\r\n\r\nStackTrace: ";

        internal static void CopyMemory(IntPtr src, int srcOffset, byte[] dest, int destOffset, int size)
        {
            Marshal.Copy(new IntPtr(src.ToInt64() + srcOffset), dest, destOffset, size);
        }

        internal static void CopyMemory(byte[] src, int srcOffset, IntPtr dest, int destOffset, int size)
        {
            Marshal.Copy(src, srcOffset, new IntPtr(dest.ToInt64() + destOffset), size);
        }

        internal static unsafe void CopyMemory(IntPtr src, int srcOffset, IntPtr dest, int destOffset, int size)
        {
            byte* numPtr = (byte*) (((void*) src) + srcOffset);
            byte* numPtr2 = (byte*) (((void*) dest) + destOffset);
            StringUtil.memcpyimpl(numPtr, numPtr2, size);
        }

        internal static IProcessHostSupportFunctions CreateLocalSupportFunctions(IProcessHostSupportFunctions proxyFunctions)
        {
            IProcessHostSupportFunctions objectForIUnknown = null;
            IntPtr iUnknownForObject = Marshal.GetIUnknownForObject(proxyFunctions);
            if (IntPtr.Zero == iUnknownForObject)
            {
                return null;
            }
            IntPtr zero = IntPtr.Zero;
            try
            {
                Guid gUID = typeof(IProcessHostSupportFunctions).GUID;
                int errorCode = Marshal.QueryInterface(iUnknownForObject, ref gUID, out zero);
                if (errorCode < 0)
                {
                    Marshal.ThrowExceptionForHR(errorCode);
                }
                objectForIUnknown = (IProcessHostSupportFunctions) Marshal.GetObjectForIUnknown(zero);
            }
            finally
            {
                if (IntPtr.Zero != zero)
                {
                    Marshal.Release(zero);
                }
                if (IntPtr.Zero != iUnknownForObject)
                {
                    Marshal.Release(iUnknownForObject);
                }
            }
            return objectForIUnknown;
        }

        internal static string FormatExceptionMessage(Exception e, string[] strings)
        {
            StringBuilder builder = new StringBuilder(0x1000);
            for (int i = 0; i < strings.Length; i++)
            {
                builder.Append(strings[i]);
            }
            for (Exception exception = e; exception != null; exception = exception.InnerException)
            {
                if (exception == e)
                {
                    builder.Append("\r\n\r\nException: ");
                }
                else
                {
                    builder.Append("\r\n\r\nInnerException: ");
                }
                builder.Append(exception.GetType().FullName);
                builder.Append("\r\n\r\nMessage: ");
                builder.Append(exception.Message);
                builder.Append("\r\n\r\nStackTrace: ");
                builder.Append(exception.StackTrace);
            }
            return builder.ToString();
        }

        internal static object GetAspNetRegValue(string subKey, string valueName, object defaultValue)
        {
            object obj2;
            try
            {
                using (RegistryKey key = OpenAspNetRegKey(subKey))
                {
                    if (key == null)
                    {
                        return defaultValue;
                    }
                    obj2 = key.GetValue(valueName, defaultValue);
                }
            }
            catch
            {
                obj2 = defaultValue;
            }
            return obj2;
        }

        internal static RegistryKey OpenAspNetRegKey(string subKey)
        {
            string systemWebVersion = VersionInfo.SystemWebVersion;
            if (!string.IsNullOrEmpty(systemWebVersion))
            {
                int num = systemWebVersion.LastIndexOf('.');
                if (num > -1)
                {
                    systemWebVersion = systemWebVersion.Substring(0, num + 1) + "0";
                }
            }
            string name = @"Software\Microsoft\ASP.NET\" + systemWebVersion;
            if (subKey != null)
            {
                name = name + @"\" + subKey;
            }
            return Registry.LocalMachine.OpenSubKey(name);
        }

        internal static void ReportUnhandledException(Exception e, string[] strings)
        {
            System.Web.UnsafeNativeMethods.ReportUnhandledException(FormatExceptionMessage(e, strings));
        }

        internal static void ThrowIfFailedHr(int hresult)
        {
            if (hresult < 0)
            {
                Marshal.ThrowExceptionForHR(hresult);
            }
        }

        internal static void WriteUnhandledExceptionToEventLog(AppDomain appDomain, Exception exception)
        {
            if ((appDomain != null) && (exception != null))
            {
                ProcessImpersonationContext context = null;
                try
                {
                    context = new ProcessImpersonationContext();
                    string data = appDomain.GetData(".appId") as string;
                    if (data == null)
                    {
                        data = appDomain.FriendlyName;
                    }
                    string str2 = System.Web.SafeNativeMethods.GetCurrentProcessId().ToString(CultureInfo.InstalledUICulture);
                    string str3 = System.Web.SR.Resources.GetString("Unhandled_Exception", CultureInfo.InstalledUICulture);
                    ReportUnhandledException(exception, new string[] { str3, "\r\n\r\nApplication ID: ", data, "\r\n\r\nProcess ID: ", str2 });
                }
                catch
                {
                }
                finally
                {
                    if (context != null)
                    {
                        context.Undo();
                    }
                }
            }
        }

        internal static StringComparer CaseInsensitiveInvariantKeyComparer
        {
            get
            {
                if (s_caseInsensitiveInvariantKeyComparer == null)
                {
                    s_caseInsensitiveInvariantKeyComparer = StringComparer.Create(CultureInfo.InvariantCulture, true);
                }
                return s_caseInsensitiveInvariantKeyComparer;
            }
        }
    }
}

