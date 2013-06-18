namespace System.Configuration
{
    using Microsoft.Win32;
    using System;
    using System.Collections.Specialized;
    using System.Runtime;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Text;
    using System.Xml;

    [PermissionSet(SecurityAction.Demand, Name="FullTrust")]
    public sealed class DpapiProtectedConfigurationProvider : ProtectedConfigurationProvider
    {
        private string _KeyEntropy;
        private bool _UseMachineProtection = true;
        private const int CRYPTPROTECT_LOCAL_MACHINE = 4;
        private const int CRYPTPROTECT_UI_FORBIDDEN = 1;

        public override XmlNode Decrypt(XmlNode encryptedNode)
        {
            if ((encryptedNode.NodeType != XmlNodeType.Element) || (encryptedNode.Name != "EncryptedData"))
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("DPAPI_bad_data"));
            }
            XmlNode node = TraverseToChild(encryptedNode, "CipherData", false);
            if (node == null)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("DPAPI_bad_data"));
            }
            XmlNode node2 = TraverseToChild(node, "CipherValue", true);
            if (node2 == null)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("DPAPI_bad_data"));
            }
            string innerText = node2.InnerText;
            if (innerText == null)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("DPAPI_bad_data"));
            }
            string xml = this.DecryptText(innerText);
            XmlDocument document = new XmlDocument {
                PreserveWhitespace = true
            };
            document.LoadXml(xml);
            return document.DocumentElement;
        }

        private string DecryptText(string encText)
        {
            DATA_BLOB data_blob;
            DATA_BLOB data_blob2;
            DATA_BLOB data_blob3;
            string str;
            if ((encText == null) || (encText.Length < 1))
            {
                return encText;
            }
            SafeNativeMemoryHandle handle = new SafeNativeMemoryHandle();
            SafeNativeMemoryHandle handle2 = new SafeNativeMemoryHandle(true);
            SafeNativeMemoryHandle handle3 = new SafeNativeMemoryHandle();
            data_blob.pbData = data_blob2.pbData = data_blob3.pbData = IntPtr.Zero;
            data_blob.cbData = data_blob2.cbData = data_blob3.cbData = 0;
            try
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    data_blob = PrepareDataBlob(Convert.FromBase64String(encText));
                    handle.SetDataHandle(data_blob.pbData);
                    data_blob2 = PrepareDataBlob(this._KeyEntropy);
                    handle3.SetDataHandle(data_blob2.pbData);
                }
                CRYPTPROTECT_PROMPTSTRUCT promptStruct = PreparePromptStructure();
                uint flags = 1;
                if (this.UseMachineProtection)
                {
                    flags |= 4;
                }
                bool flag = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    flag = Microsoft.Win32.UnsafeNativeMethods.CryptUnprotectData(ref data_blob, IntPtr.Zero, ref data_blob2, IntPtr.Zero, ref promptStruct, flags, ref data_blob3);
                    handle2.SetDataHandle(data_blob3.pbData);
                }
                if (!flag || (data_blob3.pbData == IntPtr.Zero))
                {
                    data_blob3.pbData = IntPtr.Zero;
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
                byte[] destination = new byte[data_blob3.cbData];
                Marshal.Copy(data_blob3.pbData, destination, 0, destination.Length);
                str = Encoding.Unicode.GetString(destination);
            }
            finally
            {
                if ((handle2 != null) && !handle2.IsInvalid)
                {
                    handle2.Dispose();
                    data_blob3.pbData = IntPtr.Zero;
                }
                if ((handle3 != null) && !handle3.IsInvalid)
                {
                    handle3.Dispose();
                    data_blob2.pbData = IntPtr.Zero;
                }
                if ((handle != null) && !handle.IsInvalid)
                {
                    handle.Dispose();
                    data_blob.pbData = IntPtr.Zero;
                }
            }
            return str;
        }

        public override XmlNode Encrypt(XmlNode node)
        {
            string outerXml = node.OuterXml;
            string str2 = this.EncryptText(outerXml);
            string str3 = "<EncryptedData><CipherData><CipherValue>";
            string str4 = "</CipherValue></CipherData></EncryptedData>";
            string xml = str3 + str2 + str4;
            XmlDocument document = new XmlDocument {
                PreserveWhitespace = true
            };
            document.LoadXml(xml);
            return document.DocumentElement;
        }

        private string EncryptText(string clearText)
        {
            DATA_BLOB data_blob;
            DATA_BLOB data_blob2;
            DATA_BLOB data_blob3;
            string str;
            if ((clearText == null) || (clearText.Length < 1))
            {
                return clearText;
            }
            SafeNativeMemoryHandle handle = new SafeNativeMemoryHandle();
            SafeNativeMemoryHandle handle2 = new SafeNativeMemoryHandle(true);
            SafeNativeMemoryHandle handle3 = new SafeNativeMemoryHandle();
            data_blob.pbData = data_blob2.pbData = data_blob3.pbData = IntPtr.Zero;
            data_blob.cbData = data_blob2.cbData = data_blob3.cbData = 0;
            try
            {
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    data_blob = PrepareDataBlob(clearText);
                    handle.SetDataHandle(data_blob.pbData);
                    data_blob2 = PrepareDataBlob(this._KeyEntropy);
                    handle3.SetDataHandle(data_blob2.pbData);
                }
                CRYPTPROTECT_PROMPTSTRUCT promptStruct = PreparePromptStructure();
                uint flags = 1;
                if (this.UseMachineProtection)
                {
                    flags |= 4;
                }
                bool flag = false;
                RuntimeHelpers.PrepareConstrainedRegions();
                try
                {
                }
                finally
                {
                    flag = Microsoft.Win32.UnsafeNativeMethods.CryptProtectData(ref data_blob, "", ref data_blob2, IntPtr.Zero, ref promptStruct, flags, ref data_blob3);
                    handle2.SetDataHandle(data_blob3.pbData);
                }
                if (!flag || (data_blob3.pbData == IntPtr.Zero))
                {
                    data_blob3.pbData = IntPtr.Zero;
                    Marshal.ThrowExceptionForHR(Marshal.GetHRForLastWin32Error());
                }
                byte[] destination = new byte[data_blob3.cbData];
                Marshal.Copy(data_blob3.pbData, destination, 0, destination.Length);
                str = Convert.ToBase64String(destination);
            }
            finally
            {
                if ((handle2 != null) && !handle2.IsInvalid)
                {
                    handle2.Dispose();
                    data_blob3.pbData = IntPtr.Zero;
                }
                if ((handle3 != null) && !handle3.IsInvalid)
                {
                    handle3.Dispose();
                    data_blob2.pbData = IntPtr.Zero;
                }
                if ((handle != null) && !handle.IsInvalid)
                {
                    handle.Dispose();
                    data_blob.pbData = IntPtr.Zero;
                }
            }
            return str;
        }

        private static bool GetBooleanValue(NameValueCollection configurationValues, string valueName, bool defaultValue)
        {
            string str = configurationValues[valueName];
            if (str == null)
            {
                return defaultValue;
            }
            configurationValues.Remove(valueName);
            if (str == "true")
            {
                return true;
            }
            if (str != "false")
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Config_invalid_boolean_attribute", new object[] { valueName }));
            }
            return false;
        }

        public override void Initialize(string name, NameValueCollection configurationValues)
        {
            base.Initialize(name, configurationValues);
            this._UseMachineProtection = GetBooleanValue(configurationValues, "useMachineProtection", true);
            this._KeyEntropy = configurationValues["keyEntropy"];
            configurationValues.Remove("keyEntropy");
            if (configurationValues.Count > 0)
            {
                throw new ConfigurationErrorsException(System.Configuration.SR.GetString("Unrecognized_initialization_value", new object[] { configurationValues.GetKey(0) }));
            }
        }

        private static DATA_BLOB PrepareDataBlob(byte[] buf)
        {
            DATA_BLOB data_blob;
            if (buf == null)
            {
                buf = new byte[0];
            }
            data_blob = new DATA_BLOB {
                cbData = buf.Length,
                pbData = Marshal.AllocHGlobal(data_blob.cbData)
            };
            Marshal.Copy(buf, 0, data_blob.pbData, data_blob.cbData);
            return data_blob;
        }

        private static DATA_BLOB PrepareDataBlob(string s)
        {
            return PrepareDataBlob((s != null) ? Encoding.Unicode.GetBytes(s) : new byte[0]);
        }

        private static CRYPTPROTECT_PROMPTSTRUCT PreparePromptStructure()
        {
            return new CRYPTPROTECT_PROMPTSTRUCT { cbSize = Marshal.SizeOf(typeof(CRYPTPROTECT_PROMPTSTRUCT)), dwPromptFlags = 0, hwndApp = IntPtr.Zero, szPrompt = null };
        }

        private static XmlNode TraverseToChild(XmlNode node, string name, bool onlyChild)
        {
            foreach (XmlNode node2 in node.ChildNodes)
            {
                if (node2.NodeType == XmlNodeType.Element)
                {
                    if (node2.Name == name)
                    {
                        return node2;
                    }
                    if (onlyChild)
                    {
                        return null;
                    }
                }
            }
            return null;
        }

        public bool UseMachineProtection
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._UseMachineProtection;
            }
        }
    }
}

