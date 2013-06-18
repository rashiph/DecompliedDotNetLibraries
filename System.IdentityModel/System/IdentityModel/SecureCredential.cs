namespace System.IdentityModel
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Cryptography.X509Certificates;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SecureCredential
    {
        public const int CurrentVersion = 4;
        public int version;
        public int cCreds;
        public IntPtr certContextArray;
        private IntPtr rootStore;
        public int cMappers;
        private IntPtr phMappers;
        public int cSupportedAlgs;
        private IntPtr palgSupportedAlgs;
        public SchProtocols grbitEnabledProtocols;
        public int dwMinimumCipherStrength;
        public int dwMaximumCipherStrength;
        public int dwSessionLifespan;
        public Flags dwFlags;
        public int reserved;
        public SecureCredential(int version, X509Certificate2 certificate, Flags flags, SchProtocols protocols)
        {
            this.rootStore = this.phMappers = this.palgSupportedAlgs = this.certContextArray = IntPtr.Zero;
            this.cCreds = this.cMappers = this.cSupportedAlgs = 0;
            this.dwMinimumCipherStrength = this.dwMaximumCipherStrength = 0;
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

