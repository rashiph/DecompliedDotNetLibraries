namespace System.Net
{
    using System;
    using System.Diagnostics;
    using System.Net.Security;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SecureCredential
    {
        public const int CurrentVersion = 4;
        public int version;
        public int cCreds;
        public IntPtr certContextArray;
        private readonly IntPtr rootStore;
        public int cMappers;
        private readonly IntPtr phMappers;
        public int cSupportedAlgs;
        private readonly IntPtr palgSupportedAlgs;
        public SchProtocols grbitEnabledProtocols;
        public int dwMinimumCipherStrength;
        public int dwMaximumCipherStrength;
        public int dwSessionLifespan;
        public Flags dwFlags;
        public int reserved;
        public SecureCredential(int version, X509Certificate certificate, Flags flags, SchProtocols protocols, EncryptionPolicy policy)
        {
            this.rootStore = this.phMappers = this.palgSupportedAlgs = this.certContextArray = IntPtr.Zero;
            this.cCreds = this.cMappers = this.cSupportedAlgs = 0;
            if (policy == EncryptionPolicy.RequireEncryption)
            {
                this.dwMinimumCipherStrength = 0;
                this.dwMaximumCipherStrength = 0;
            }
            else if (policy == EncryptionPolicy.AllowNoEncryption)
            {
                this.dwMinimumCipherStrength = -1;
                this.dwMaximumCipherStrength = 0;
            }
            else
            {
                if (policy != EncryptionPolicy.NoEncryption)
                {
                    throw new ArgumentException(SR.GetString("net_invalid_enum", new object[] { "EncryptionPolicy" }), "policy");
                }
                this.dwMinimumCipherStrength = -1;
                this.dwMaximumCipherStrength = -1;
            }
            this.dwSessionLifespan = this.reserved = 0;
            this.version = version;
            this.dwFlags = flags;
            this.grbitEnabledProtocols = protocols;
            if (certificate != null)
            {
                this.certContextArray = certificate.Handle;
                this.cCreds = 1;
            }
        }

        [Conditional("TRAVE")]
        internal void DebugDump()
        {
        }
        [Flags]
        public enum Flags
        {
            NoDefaultCred = 0x10,
            NoNameCheck = 4,
            NoSystemMapper = 2,
            ValidateAuto = 0x20,
            ValidateManual = 8,
            Zero = 0
        }
    }
}

