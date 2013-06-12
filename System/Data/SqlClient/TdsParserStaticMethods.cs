namespace System.Data.SqlClient
{
    using System;
    using System.Data.Common;
    using System.Globalization;
    using System.Security.Permissions;

    internal sealed class TdsParserStaticMethods
    {
        private TdsParserStaticMethods()
        {
        }

        internal static void AliasRegistryLookup(ref string host, ref string protocol)
        {
            if (!ADP.IsEmpty(host))
            {
                string str3 = (string) ADP.LocalMachineRegistryValue(@"SOFTWARE\Microsoft\MSSQLServer\Client\ConnectTo", host);
                if (!ADP.IsEmpty(str3))
                {
                    int index = str3.IndexOf(',');
                    if (-1 != index)
                    {
                        string protocal = str3.Substring(0, index).ToLower(CultureInfo.InvariantCulture);
                        if ((index + 1) < str3.Length)
                        {
                            string str = str3.Substring(index + 1);
                            if ("dbnetlib" == protocal)
                            {
                                index = str.IndexOf(':');
                                if ((-1 != index) && ((index + 1) < str.Length))
                                {
                                    protocal = str.Substring(0, index);
                                    if (SqlConnectionString.ValidProtocal(protocal))
                                    {
                                        protocol = protocal;
                                        host = str.Substring(index + 1);
                                    }
                                }
                            }
                            else
                            {
                                protocol = (string) SqlConnectionString.NetlibMapping()[protocal];
                                if (protocol != null)
                                {
                                    host = str;
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static byte[] EncryptPassword(string password)
        {
            byte[] buffer = new byte[password.Length << 1];
            for (int i = 0; i < password.Length; i++)
            {
                int num4 = password[i];
                byte num3 = (byte) (num4 & 0xff);
                byte num2 = (byte) ((num4 >> 8) & 0xff);
                buffer[i << 1] = (byte) ((((num3 & 15) << 4) | (num3 >> 4)) ^ 0xa5);
                buffer[(i << 1) + 1] = (byte) ((((num2 & 15) << 4) | (num2 >> 4)) ^ 0xa5);
            }
            return buffer;
        }

        internal static int GetCurrentProcessIdForTdsLoginOnly()
        {
            return SafeNativeMethods.GetCurrentProcessId();
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
        internal static int GetCurrentThreadIdForTdsLoginOnly()
        {
            return AppDomain.GetCurrentThreadId();
        }

        internal static byte[] GetNetworkPhysicalAddressForTdsLoginOnly()
        {
            int num = 0;
            byte[] buffer = null;
            object obj2 = ADP.LocalMachineRegistryValue(@"SOFTWARE\Description\Microsoft\Rpc\UuidTemporaryData", "NetworkAddressLocal");
            if (obj2 is int)
            {
                num = (int) obj2;
            }
            if (num <= 0)
            {
                obj2 = ADP.LocalMachineRegistryValue(@"SOFTWARE\Description\Microsoft\Rpc\UuidTemporaryData", "NetworkAddress");
                if (obj2 is byte[])
                {
                    buffer = (byte[]) obj2;
                }
            }
            if (buffer == null)
            {
                buffer = new byte[6];
                new Random().NextBytes(buffer);
            }
            return buffer;
        }

        internal static int GetTimeoutMilliseconds(long timeoutTime)
        {
            if (0x7fffffffffffffffL == timeoutTime)
            {
                return -1;
            }
            long num = ADP.TimerRemainingMilliseconds(timeoutTime);
            if (num < 0L)
            {
                return 0;
            }
            if (num > 0x7fffffffL)
            {
                return 0x7fffffff;
            }
            return (int) num;
        }

        internal static long GetTimeoutSeconds(int timeoutSeconds)
        {
            if (timeoutSeconds == 0)
            {
                return 0x7fffffffffffffffL;
            }
            return (ADP.TimerCurrent() + ADP.TimerFromSeconds(timeoutSeconds));
        }

        internal static bool TimeoutHasExpired(long timeoutTime)
        {
            bool flag = false;
            if ((0L != timeoutTime) && (0x7fffffffffffffffL != timeoutTime))
            {
                flag = ADP.TimerHasExpired(timeoutTime);
            }
            return flag;
        }
    }
}

