namespace System.Security.Cryptography
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.AccessControl;
    using System.Security.Permissions;

    [ComVisible(true)]
    public sealed class CspKeyContainerInfo
    {
        private CspParameters m_parameters;
        private bool m_randomKeyContainer;

        private CspKeyContainerInfo()
        {
        }

        [SecuritySafeCritical]
        public CspKeyContainerInfo(CspParameters parameters) : this(parameters, false)
        {
        }

        [SecurityCritical]
        internal CspKeyContainerInfo(CspParameters parameters, bool randomKeyContainer)
        {
            KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
            KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(parameters, KeyContainerPermissionFlags.Open);
            permission.AccessEntries.Add(accessEntry);
            permission.Demand();
            this.m_parameters = new CspParameters(parameters);
            if (this.m_parameters.KeyNumber == -1)
            {
                if ((this.m_parameters.ProviderType == 1) || (this.m_parameters.ProviderType == 0x18))
                {
                    this.m_parameters.KeyNumber = 1;
                }
                else if (this.m_parameters.ProviderType == 13)
                {
                    this.m_parameters.KeyNumber = 2;
                }
            }
            this.m_randomKeyContainer = randomKeyContainer;
        }

        public bool Accessible
        {
            [SecuritySafeCritical]
            get
            {
                SafeProvHandle invalidHandle = SafeProvHandle.InvalidHandle;
                if (Utils._OpenCSP(this.m_parameters, 0x40, ref invalidHandle) != 0)
                {
                    return false;
                }
                byte[] buffer = (byte[]) Utils._GetProviderParameter(invalidHandle, this.m_parameters.KeyNumber, 6);
                invalidHandle.Dispose();
                return (buffer[0] == 1);
            }
        }

        public System.Security.AccessControl.CryptoKeySecurity CryptoKeySecurity
        {
            [SecuritySafeCritical]
            get
            {
                KeyContainerPermission permission = new KeyContainerPermission(KeyContainerPermissionFlags.NoFlags);
                KeyContainerPermissionAccessEntry accessEntry = new KeyContainerPermissionAccessEntry(this.m_parameters, KeyContainerPermissionFlags.ChangeAcl | KeyContainerPermissionFlags.ViewAcl);
                permission.AccessEntries.Add(accessEntry);
                permission.Demand();
                SafeProvHandle invalidHandle = SafeProvHandle.InvalidHandle;
                if (Utils._OpenCSP(this.m_parameters, 0x40, ref invalidHandle) != 0)
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NotFound"));
                }
                using (invalidHandle)
                {
                    return Utils.GetKeySetSecurityInfo(invalidHandle, AccessControlSections.All);
                }
            }
        }

        public bool Exportable
        {
            [SecuritySafeCritical]
            get
            {
                if (this.HardwareDevice)
                {
                    return false;
                }
                SafeProvHandle invalidHandle = SafeProvHandle.InvalidHandle;
                if (Utils._OpenCSP(this.m_parameters, 0x40, ref invalidHandle) != 0)
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NotFound"));
                }
                byte[] buffer = (byte[]) Utils._GetProviderParameter(invalidHandle, this.m_parameters.KeyNumber, 3);
                invalidHandle.Dispose();
                return (buffer[0] == 1);
            }
        }

        public bool HardwareDevice
        {
            [SecuritySafeCritical]
            get
            {
                CspParameters parameters;
                SafeProvHandle invalidHandle = SafeProvHandle.InvalidHandle;
                parameters = new CspParameters(this.m_parameters) {
                    KeyContainerName = null,
                    Flags = ((parameters.Flags & CspProviderFlags.UseMachineKeyStore) != CspProviderFlags.NoFlags) ? CspProviderFlags.UseMachineKeyStore : CspProviderFlags.NoFlags
                };
                uint flags = 0xf0000000;
                if (Utils._OpenCSP(parameters, flags, ref invalidHandle) != 0)
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NotFound"));
                }
                byte[] buffer = (byte[]) Utils._GetProviderParameter(invalidHandle, parameters.KeyNumber, 5);
                invalidHandle.Dispose();
                return (buffer[0] == 1);
            }
        }

        public string KeyContainerName
        {
            get
            {
                return this.m_parameters.KeyContainerName;
            }
        }

        public System.Security.Cryptography.KeyNumber KeyNumber
        {
            get
            {
                return (System.Security.Cryptography.KeyNumber) this.m_parameters.KeyNumber;
            }
        }

        public bool MachineKeyStore
        {
            get
            {
                if ((this.m_parameters.Flags & CspProviderFlags.UseMachineKeyStore) != CspProviderFlags.UseMachineKeyStore)
                {
                    return false;
                }
                return true;
            }
        }

        public bool Protected
        {
            [SecuritySafeCritical]
            get
            {
                if (this.HardwareDevice)
                {
                    return true;
                }
                SafeProvHandle invalidHandle = SafeProvHandle.InvalidHandle;
                if (Utils._OpenCSP(this.m_parameters, 0x40, ref invalidHandle) != 0)
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NotFound"));
                }
                byte[] buffer = (byte[]) Utils._GetProviderParameter(invalidHandle, this.m_parameters.KeyNumber, 7);
                invalidHandle.Dispose();
                return (buffer[0] == 1);
            }
        }

        public string ProviderName
        {
            get
            {
                return this.m_parameters.ProviderName;
            }
        }

        public int ProviderType
        {
            get
            {
                return this.m_parameters.ProviderType;
            }
        }

        public bool RandomlyGenerated
        {
            get
            {
                return this.m_randomKeyContainer;
            }
        }

        public bool Removable
        {
            [SecuritySafeCritical]
            get
            {
                CspParameters parameters;
                SafeProvHandle invalidHandle = SafeProvHandle.InvalidHandle;
                parameters = new CspParameters(this.m_parameters) {
                    KeyContainerName = null,
                    Flags = ((parameters.Flags & CspProviderFlags.UseMachineKeyStore) != CspProviderFlags.NoFlags) ? CspProviderFlags.UseMachineKeyStore : CspProviderFlags.NoFlags
                };
                uint flags = 0xf0000000;
                if (Utils._OpenCSP(parameters, flags, ref invalidHandle) != 0)
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NotFound"));
                }
                byte[] buffer = (byte[]) Utils._GetProviderParameter(invalidHandle, parameters.KeyNumber, 4);
                invalidHandle.Dispose();
                return (buffer[0] == 1);
            }
        }

        public string UniqueKeyContainerName
        {
            [SecuritySafeCritical]
            get
            {
                SafeProvHandle invalidHandle = SafeProvHandle.InvalidHandle;
                if (Utils._OpenCSP(this.m_parameters, 0x40, ref invalidHandle) != 0)
                {
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_CSP_NotFound"));
                }
                string str = (string) Utils._GetProviderParameter(invalidHandle, this.m_parameters.KeyNumber, 8);
                invalidHandle.Dispose();
                return str;
            }
        }
    }
}

