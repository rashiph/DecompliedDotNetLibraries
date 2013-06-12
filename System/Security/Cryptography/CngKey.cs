namespace System.Security.Cryptography
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.CompilerServices;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public sealed class CngKey : IDisposable
    {
        private SafeNCryptKeyHandle m_keyHandle;
        private SafeNCryptProviderHandle m_kspHandle;

        [SecurityCritical]
        private CngKey(SafeNCryptProviderHandle kspHandle, SafeNCryptKeyHandle keyHandle)
        {
            this.m_keyHandle = keyHandle;
            this.m_kspHandle = kspHandle;
        }

        [SecurityCritical]
        internal KeyContainerPermission BuildKeyContainerPermission(KeyContainerPermissionFlags flags)
        {
            KeyContainerPermission permission = null;
            if (this.IsEphemeral)
            {
                return permission;
            }
            string keyContainerName = null;
            string str2 = null;
            try
            {
                keyContainerName = this.KeyName;
                str2 = NCryptNative.GetPropertyAsString(this.m_kspHandle, "Name", CngPropertyOptions.None);
            }
            catch (CryptographicException)
            {
            }
            if (keyContainerName != null)
            {
                KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(keyContainerName, flags) {
                    ProviderName = str2
                };
                permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                permission.AccessEntries.Add(accessEntry);
                return permission;
            }
            return new KeyContainerPermission(flags);
        }

        public static CngKey Create(CngAlgorithm algorithm)
        {
            return Create(algorithm, null);
        }

        public static CngKey Create(CngAlgorithm algorithm, string keyName)
        {
            return Create(algorithm, keyName, null);
        }

        [SecurityCritical]
        public static CngKey Create(CngAlgorithm algorithm, string keyName, CngKeyCreationParameters creationParameters)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException("algorithm");
            }
            if (creationParameters == null)
            {
                creationParameters = new CngKeyCreationParameters();
            }
            if (!NCryptNative.NCryptSupported)
            {
                throw new PlatformNotSupportedException(System.SR.GetString("Cryptography_PlatformNotSupported"));
            }
            if (keyName != null)
            {
                KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(keyName, KeyContainerPermissionFlags.Create) {
                    ProviderName = creationParameters.Provider.Provider
                };
                KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                permission.AccessEntries.Add(accessEntry);
                permission.Demand();
            }
            SafeNCryptProviderHandle provider = NCryptNative.OpenStorageProvider(creationParameters.Provider.Provider);
            SafeNCryptKeyHandle keyHandle = NCryptNative.CreatePersistedKey(provider, algorithm.Algorithm, keyName, creationParameters.KeyCreationOptions);
            SetKeyProperties(keyHandle, creationParameters);
            NCryptNative.FinalizeKey(keyHandle);
            CngKey key = new CngKey(provider, keyHandle);
            if (keyName == null)
            {
                key.IsEphemeral = true;
            }
            return key;
        }

        [SecurityCritical]
        public void Delete()
        {
            KeyContainerPermission permission = this.BuildKeyContainerPermission(KeyContainerPermissionFlags.Delete);
            if (permission != null)
            {
                permission.Demand();
            }
            NCryptNative.DeleteKey(this.m_keyHandle);
            this.Dispose();
        }

        [SecurityCritical]
        public void Dispose()
        {
            if (this.m_kspHandle != null)
            {
                this.m_kspHandle.Dispose();
            }
            if (this.m_keyHandle != null)
            {
                this.m_keyHandle.Dispose();
            }
        }

        public static bool Exists(string keyName)
        {
            return Exists(keyName, CngProvider.MicrosoftSoftwareKeyStorageProvider);
        }

        public static bool Exists(string keyName, CngProvider provider)
        {
            return Exists(keyName, provider, CngKeyOpenOptions.None);
        }

        [SecurityCritical]
        public static bool Exists(string keyName, CngProvider provider, CngKeyOpenOptions options)
        {
            bool flag2;
            if (keyName == null)
            {
                throw new ArgumentNullException("keyName");
            }
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (!NCryptNative.NCryptSupported)
            {
                throw new PlatformNotSupportedException(System.SR.GetString("Cryptography_PlatformNotSupported"));
            }
            using (SafeNCryptProviderHandle handle = NCryptNative.OpenStorageProvider(provider.Provider))
            {
                using (SafeNCryptKeyHandle handle2 = null)
                {
                    NCryptNative.ErrorCode code = NCryptNative.UnsafeNativeMethods.NCryptOpenKey(handle, out handle2, keyName, 0, options);
                    bool flag = (code == NCryptNative.ErrorCode.KeyDoesNotExist) || (code == NCryptNative.ErrorCode.NotFound);
                    if ((code != NCryptNative.ErrorCode.Success) && !flag)
                    {
                        throw new CryptographicException((int) code);
                    }
                    flag2 = code == NCryptNative.ErrorCode.Success;
                }
            }
            return flag2;
        }

        [SecurityCritical]
        public byte[] Export(CngKeyBlobFormat format)
        {
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            KeyContainerPermission permission = this.BuildKeyContainerPermission(KeyContainerPermissionFlags.Export);
            if (permission != null)
            {
                permission.Demand();
            }
            return NCryptNative.ExportKey(this.m_keyHandle, format.Format);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public CngProperty GetProperty(string name, CngPropertyOptions options)
        {
            bool flag;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            byte[] buffer = NCryptNative.GetProperty(this.m_keyHandle, name, options, out flag);
            if (!flag)
            {
                throw new CryptographicException(-2146893807);
            }
            return new CngProperty(name, buffer, options);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public bool HasProperty(string name, CngPropertyOptions options)
        {
            bool flag;
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            NCryptNative.GetProperty(this.m_keyHandle, name, options, out flag);
            return flag;
        }

        public static CngKey Import(byte[] keyBlob, CngKeyBlobFormat format)
        {
            return Import(keyBlob, format, CngProvider.MicrosoftSoftwareKeyStorageProvider);
        }

        [SecurityCritical]
        public static CngKey Import(byte[] keyBlob, CngKeyBlobFormat format, CngProvider provider)
        {
            if (keyBlob == null)
            {
                throw new ArgumentNullException("keyBlob");
            }
            if (format == null)
            {
                throw new ArgumentNullException("format");
            }
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (!NCryptNative.NCryptSupported)
            {
                throw new PlatformNotSupportedException(System.SR.GetString("Cryptography_PlatformNotSupported"));
            }
            if ((format != CngKeyBlobFormat.EccPublicBlob) && (format != CngKeyBlobFormat.GenericPublicBlob))
            {
                new KeyContainerPermission(KeyContainerPermissionFlags.Import).Demand();
            }
            SafeNCryptProviderHandle kspHandle = NCryptNative.OpenStorageProvider(provider.Provider);
            return new CngKey(kspHandle, NCryptNative.ImportKey(kspHandle, keyBlob, format.Format)) { IsEphemeral = format != CngKeyBlobFormat.OpaqueTransportBlob };
        }

        public static CngKey Open(string keyName)
        {
            return Open(keyName, CngProvider.MicrosoftSoftwareKeyStorageProvider);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public static CngKey Open(SafeNCryptKeyHandle keyHandle, CngKeyHandleOpenOptions keyHandleOpenOptions)
        {
            if (keyHandle == null)
            {
                throw new ArgumentNullException("keyHandle");
            }
            if (keyHandle.IsClosed || keyHandle.IsInvalid)
            {
                throw new ArgumentException(System.SR.GetString("Cryptography_OpenInvalidHandle"), "keyHandle");
            }
            SafeNCryptKeyHandle handle = keyHandle.Duplicate();
            SafeNCryptProviderHandle kspHandle = new SafeNCryptProviderHandle();
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
            }
            finally
            {
                IntPtr newHandleValue = NCryptNative.GetPropertyAsIntPtr(keyHandle, "Provider Handle", CngPropertyOptions.None);
                kspHandle.SetHandleValue(newHandleValue);
            }
            CngKey key = null;
            bool flag = false;
            try
            {
                key = new CngKey(kspHandle, handle);
                bool flag2 = (keyHandleOpenOptions & CngKeyHandleOpenOptions.EphemeralKey) == CngKeyHandleOpenOptions.EphemeralKey;
                if (!key.IsEphemeral && flag2)
                {
                    key.IsEphemeral = true;
                }
                else if (key.IsEphemeral && !flag2)
                {
                    throw new ArgumentException(System.SR.GetString("Cryptography_OpenEphemeralKeyHandleWithoutEphemeralFlag"), "keyHandleOpenOptions");
                }
                flag = true;
            }
            finally
            {
                if (!flag && (key != null))
                {
                    key.Dispose();
                }
            }
            return key;
        }

        public static CngKey Open(string keyName, CngProvider provider)
        {
            return Open(keyName, provider, CngKeyOpenOptions.None);
        }

        [SecurityCritical]
        public static CngKey Open(string keyName, CngProvider provider, CngKeyOpenOptions openOptions)
        {
            if (keyName == null)
            {
                throw new ArgumentNullException("keyName");
            }
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (!NCryptNative.NCryptSupported)
            {
                throw new PlatformNotSupportedException(System.SR.GetString("Cryptography_PlatformNotSupported"));
            }
            KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(keyName, KeyContainerPermissionFlags.Open) {
                ProviderName = provider.Provider
            };
            KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
            permission.AccessEntries.Add(accessEntry);
            permission.Demand();
            SafeNCryptProviderHandle kspHandle = NCryptNative.OpenStorageProvider(provider.Provider);
            return new CngKey(kspHandle, NCryptNative.OpenKey(kspHandle, keyName, openOptions));
        }

        [SecurityCritical]
        private static void SetKeyProperties(SafeNCryptKeyHandle keyHandle, CngKeyCreationParameters creationParameters)
        {
            if (creationParameters.ExportPolicy.HasValue)
            {
                NCryptNative.SetProperty(keyHandle, "Export Policy", creationParameters.ExportPolicy.Value, CngPropertyOptions.None | CngPropertyOptions.Persist);
            }
            if (creationParameters.KeyUsage.HasValue)
            {
                NCryptNative.SetProperty(keyHandle, "Key Usage", creationParameters.KeyUsage.Value, CngPropertyOptions.None | CngPropertyOptions.Persist);
            }
            if (creationParameters.ParentWindowHandle != IntPtr.Zero)
            {
                NCryptNative.SetProperty<IntPtr>(keyHandle, "HWND Handle", creationParameters.ParentWindowHandle, CngPropertyOptions.None);
            }
            if (creationParameters.UIPolicy != null)
            {
                NCryptNative.NCRYPT_UI_POLICY ncrypt_ui_policy = new NCryptNative.NCRYPT_UI_POLICY {
                    dwVersion = 1,
                    dwFlags = creationParameters.UIPolicy.ProtectionLevel,
                    pszCreationTitle = creationParameters.UIPolicy.CreationTitle,
                    pszFriendlyName = creationParameters.UIPolicy.FriendlyName,
                    pszDescription = creationParameters.UIPolicy.Description
                };
                NCryptNative.SetProperty<NCryptNative.NCRYPT_UI_POLICY>(keyHandle, "UI Policy", ncrypt_ui_policy, CngPropertyOptions.None | CngPropertyOptions.Persist);
                if (creationParameters.UIPolicy.UseContext != null)
                {
                    NCryptNative.SetProperty(keyHandle, "Use Context", creationParameters.UIPolicy.UseContext, CngPropertyOptions.None | CngPropertyOptions.Persist);
                }
            }
            foreach (CngProperty property in creationParameters.ParametersNoDemand)
            {
                NCryptNative.SetProperty(keyHandle, property.Name, property.Value, property.Options);
            }
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public void SetProperty(CngProperty property)
        {
            NCryptNative.SetProperty(this.m_keyHandle, property.Name, property.Value, property.Options);
        }

        public CngAlgorithm Algorithm
        {
            [SecurityCritical]
            get
            {
                return new CngAlgorithm(NCryptNative.GetPropertyAsString(this.m_keyHandle, "Algorithm Name", CngPropertyOptions.None));
            }
        }

        public CngAlgorithmGroup AlgorithmGroup
        {
            [SecurityCritical]
            get
            {
                string algorithmGroup = NCryptNative.GetPropertyAsString(this.m_keyHandle, "Algorithm Group", CngPropertyOptions.None);
                if (algorithmGroup == null)
                {
                    return null;
                }
                return new CngAlgorithmGroup(algorithmGroup);
            }
        }

        public CngExportPolicies ExportPolicy
        {
            [SecurityCritical]
            get
            {
                return (CngExportPolicies) NCryptNative.GetPropertyAsDWord(this.m_keyHandle, "Export Policy", CngPropertyOptions.None);
            }
        }

        public SafeNCryptKeyHandle Handle
        {
            [SecurityCritical, SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
            get
            {
                return this.m_keyHandle.Duplicate();
            }
        }

        public bool IsEphemeral
        {
            [SecurityCritical]
            get
            {
                bool flag;
                byte[] buffer = NCryptNative.GetProperty(this.m_keyHandle, "CLR IsEphemeral", CngPropertyOptions.CustomProperty, out flag);
                return (((flag && (buffer != null)) && (buffer.Length == 1)) && (buffer[0] == 1));
            }
            [SecurityCritical]
            private set
            {
                NCryptNative.SetProperty(this.m_keyHandle, "CLR IsEphemeral", new byte[] { value ? ((byte) 1) : ((byte) 0) }, CngPropertyOptions.CustomProperty);
            }
        }

        public bool IsMachineKey
        {
            [SecurityCritical]
            get
            {
                return ((NCryptNative.GetPropertyAsDWord(this.m_keyHandle, "Key Type", CngPropertyOptions.None) & 0x20) == 0x20);
            }
        }

        public string KeyName
        {
            [SecurityCritical]
            get
            {
                if (this.IsEphemeral)
                {
                    return null;
                }
                return NCryptNative.GetPropertyAsString(this.m_keyHandle, "Name", CngPropertyOptions.None);
            }
        }

        public int KeySize
        {
            [SecurityCritical]
            get
            {
                return NCryptNative.GetPropertyAsDWord(this.m_keyHandle, "Length", CngPropertyOptions.None);
            }
        }

        public CngKeyUsages KeyUsage
        {
            [SecurityCritical]
            get
            {
                return (CngKeyUsages) NCryptNative.GetPropertyAsDWord(this.m_keyHandle, "Key Usage", CngPropertyOptions.None);
            }
        }

        public IntPtr ParentWindowHandle
        {
            [SecurityCritical]
            get
            {
                return NCryptNative.GetPropertyAsIntPtr(this.m_keyHandle, "HWND Handle", CngPropertyOptions.None);
            }
            [SecurityCritical, SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
            set
            {
                NCryptNative.SetProperty<IntPtr>(this.m_keyHandle, "HWND Handle", value, CngPropertyOptions.None);
            }
        }

        public CngProvider Provider
        {
            [SecurityCritical]
            get
            {
                string provider = NCryptNative.GetPropertyAsString(this.m_kspHandle, "Name", CngPropertyOptions.None);
                if (provider == null)
                {
                    return null;
                }
                return new CngProvider(provider);
            }
        }

        public SafeNCryptProviderHandle ProviderHandle
        {
            [SecurityCritical, SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
            get
            {
                return this.m_kspHandle.Duplicate();
            }
        }

        public CngUIPolicy UIPolicy
        {
            [SecurityCritical]
            get
            {
                NCryptNative.NCRYPT_UI_POLICY ncrypt_ui_policy = NCryptNative.GetPropertyAsStruct<NCryptNative.NCRYPT_UI_POLICY>(this.m_keyHandle, "UI Policy", CngPropertyOptions.None);
                return new CngUIPolicy(ncrypt_ui_policy.dwFlags, ncrypt_ui_policy.pszFriendlyName, ncrypt_ui_policy.pszDescription, NCryptNative.GetPropertyAsString(this.m_keyHandle, "Use Context", CngPropertyOptions.None), ncrypt_ui_policy.pszCreationTitle);
            }
        }

        public string UniqueName
        {
            [SecurityCritical]
            get
            {
                if (this.IsEphemeral)
                {
                    return null;
                }
                return NCryptNative.GetPropertyAsString(this.m_keyHandle, "Unique Name", CngPropertyOptions.None);
            }
        }
    }
}

