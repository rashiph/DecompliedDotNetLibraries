namespace System.Security.Cryptography.Xml
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [StructLayout(LayoutKind.Sequential), HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public struct X509IssuerSerial
    {
        private string issuerName;
        private string serialNumber;
        internal X509IssuerSerial(string issuerName, string serialNumber)
        {
            if ((issuerName == null) || (issuerName.Length == 0))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Arg_EmptyOrNullString"), "issuerName");
            }
            if ((serialNumber == null) || (serialNumber.Length == 0))
            {
                throw new ArgumentException(SecurityResources.GetResourceString("Arg_EmptyOrNullString"), "serialNumber");
            }
            this.issuerName = issuerName;
            this.serialNumber = serialNumber;
        }

        public string IssuerName
        {
            get
            {
                return this.issuerName;
            }
            set
            {
                this.issuerName = value;
            }
        }
        public string SerialNumber
        {
            get
            {
                return this.serialNumber;
            }
            set
            {
                this.serialNumber = value;
            }
        }
    }
}

