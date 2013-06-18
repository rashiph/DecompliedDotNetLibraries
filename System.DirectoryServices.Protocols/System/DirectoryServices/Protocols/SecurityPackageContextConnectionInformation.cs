namespace System.DirectoryServices.Protocols
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Authentication;

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public class SecurityPackageContextConnectionInformation
    {
        private SecurityProtocol securityProtocol;
        private CipherAlgorithmType identifier;
        private int strength;
        private HashAlgorithmType hashAlgorithm;
        private int hashStrength;
        private int keyExchangeAlgorithm;
        private int exchangeStrength;
        internal SecurityPackageContextConnectionInformation()
        {
        }

        public SecurityProtocol Protocol
        {
            get
            {
                return this.securityProtocol;
            }
        }
        public CipherAlgorithmType AlgorithmIdentifier
        {
            get
            {
                return this.identifier;
            }
        }
        public int CipherStrength
        {
            get
            {
                return this.strength;
            }
        }
        public HashAlgorithmType Hash
        {
            get
            {
                return this.hashAlgorithm;
            }
        }
        public int HashStrength
        {
            get
            {
                return this.hashStrength;
            }
        }
        public int KeyExchangeAlgorithm
        {
            get
            {
                return this.keyExchangeAlgorithm;
            }
        }
        public int ExchangeStrength
        {
            get
            {
                return this.exchangeStrength;
            }
        }
    }
}

