namespace System.Net
{
    using Microsoft.Win32;
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;

    internal class RegBlobWebProxyDataBuilder : WebProxyDataBuilder
    {
        private const string DefaultConnectionSettings = "DefaultConnectionSettings";
        private const int IE50StrucSize = 60;
        private int m_ByteOffset;
        private string m_Connectoid;
        private SafeRegistryHandle m_Registry;
        private byte[] m_RegistryBytes;
        internal const string PolicyKey = @"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\Internet Settings";
        internal const string ProxyKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Connections";
        private const string ProxySettingsPerUser = "ProxySettingsPerUser";

        public RegBlobWebProxyDataBuilder(string connectoid, SafeRegistryHandle registry)
        {
            this.m_Registry = registry;
            this.m_Connectoid = connectoid;
        }

        protected override void BuildInternal()
        {
            bool flag = this.ReadRegSettings();
            if (flag)
            {
                flag = this.ReadInt32() >= 60;
            }
            if (!flag)
            {
                base.SetAutoDetectSettings(true);
            }
            else
            {
                this.ReadInt32();
                ProxyTypeFlags flags = (ProxyTypeFlags) this.ReadInt32();
                string addressString = this.ReadString();
                string bypassListString = this.ReadString();
                if ((flags & ProxyTypeFlags.PROXY_TYPE_PROXY) != 0)
                {
                    base.SetProxyAndBypassList(addressString, bypassListString);
                }
                base.SetAutoDetectSettings((flags & ProxyTypeFlags.PROXY_TYPE_AUTO_DETECT) != 0);
                string autoConfigUrl = this.ReadString();
                if ((flags & ProxyTypeFlags.PROXY_TYPE_AUTO_PROXY_URL) != 0)
                {
                    base.SetAutoProxyUrl(autoConfigUrl);
                }
            }
        }

        internal unsafe int ReadInt32()
        {
            int num = 0;
            int num2 = this.m_RegistryBytes.Length - this.m_ByteOffset;
            if (num2 >= 4)
            {
                fixed (byte* numRef = this.m_RegistryBytes)
                {
                    if (sizeof(IntPtr) == 4)
                    {
                        num = numRef[this.m_ByteOffset];
                    }
                    else
                    {
                        num = Marshal.ReadInt32((IntPtr) numRef, this.m_ByteOffset);
                    }
                }
                this.m_ByteOffset += 4;
            }
            return num;
        }

        [RegistryPermission(SecurityAction.Assert, Read=@"HKEY_LOCAL_MACHINE\SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\Internet Settings")]
        private bool ReadRegSettings()
        {
            SafeRegistryHandle resultSubKey = null;
            RegistryKey key = null;
            try
            {
                uint num;
                object obj3;
                bool flag = true;
                key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\CurrentVersion\Internet Settings");
                if (key != null)
                {
                    object obj2 = key.GetValue("ProxySettingsPerUser");
                    if (((obj2 != null) && (obj2.GetType() == typeof(int))) && (((int) obj2) == 0))
                    {
                        flag = false;
                    }
                }
                if (flag)
                {
                    if (this.m_Registry != null)
                    {
                        num = this.m_Registry.RegOpenKeyEx(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", 0, 0x20019, out resultSubKey);
                    }
                    else
                    {
                        num = 0x490;
                    }
                }
                else
                {
                    num = SafeRegistryHandle.RegOpenKeyEx(UnsafeNclNativeMethods.RegistryHelper.HKEY_LOCAL_MACHINE, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Internet Settings\Connections", 0, 0x20019, out resultSubKey);
                }
                if (num != 0)
                {
                    resultSubKey = null;
                }
                if ((resultSubKey != null) && (resultSubKey.QueryValue((this.m_Connectoid != null) ? this.m_Connectoid : "DefaultConnectionSettings", out obj3) == 0))
                {
                    this.m_RegistryBytes = (byte[]) obj3;
                }
            }
            catch (Exception exception)
            {
                if (NclUtilities.IsFatal(exception))
                {
                    throw;
                }
            }
            finally
            {
                if (key != null)
                {
                    key.Close();
                }
                if (resultSubKey != null)
                {
                    resultSubKey.RegCloseKey();
                }
            }
            return (this.m_RegistryBytes != null);
        }

        public string ReadString()
        {
            string str = null;
            int count = this.ReadInt32();
            if (count > 0)
            {
                int num2 = this.m_RegistryBytes.Length - this.m_ByteOffset;
                if (count >= num2)
                {
                    count = num2;
                }
                str = Encoding.UTF8.GetString(this.m_RegistryBytes, this.m_ByteOffset, count);
                this.m_ByteOffset += count;
            }
            return str;
        }

        [Flags]
        private enum ProxyTypeFlags
        {
            PROXY_TYPE_AUTO_DETECT = 8,
            PROXY_TYPE_AUTO_PROXY_URL = 4,
            PROXY_TYPE_DIRECT = 1,
            PROXY_TYPE_PROXY = 2
        }
    }
}

