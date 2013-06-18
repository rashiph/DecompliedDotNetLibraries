namespace System.ServiceModel.Security
{
    using System;
    using System.Collections.Generic;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;

    public sealed class X509CertificateRecipientClientCredential
    {
        private X509ServiceCertificateAuthentication authentication;
        private X509Certificate2 defaultCertificate;
        internal const X509FindType DefaultFindType = X509FindType.FindBySubjectDistinguishedName;
        internal const StoreLocation DefaultStoreLocation = StoreLocation.CurrentUser;
        internal const StoreName DefaultStoreName = StoreName.My;
        private bool isReadOnly;
        private Dictionary<Uri, X509Certificate2> scopedCertificates;

        internal X509CertificateRecipientClientCredential()
        {
            this.authentication = new X509ServiceCertificateAuthentication();
            this.scopedCertificates = new Dictionary<Uri, X509Certificate2>();
        }

        internal X509CertificateRecipientClientCredential(X509CertificateRecipientClientCredential other)
        {
            this.authentication = new X509ServiceCertificateAuthentication(other.authentication);
            this.defaultCertificate = other.defaultCertificate;
            this.scopedCertificates = new Dictionary<Uri, X509Certificate2>();
            foreach (Uri uri in other.ScopedCertificates.Keys)
            {
                this.scopedCertificates.Add(uri, other.ScopedCertificates[uri]);
            }
            this.isReadOnly = other.isReadOnly;
        }

        internal void MakeReadOnly()
        {
            this.isReadOnly = true;
            this.Authentication.MakeReadOnly();
        }

        public void SetDefaultCertificate(string subjectName, StoreLocation storeLocation, StoreName storeName)
        {
            if (subjectName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subjectName");
            }
            this.SetDefaultCertificate(storeLocation, storeName, X509FindType.FindBySubjectDistinguishedName, subjectName);
        }

        public void SetDefaultCertificate(StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue)
        {
            if (findValue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("findValue");
            }
            this.ThrowIfImmutable();
            this.defaultCertificate = System.ServiceModel.Security.SecurityUtils.GetCertificateFromStore(storeName, storeLocation, findType, findValue, null);
        }

        public void SetScopedCertificate(string subjectName, StoreLocation storeLocation, StoreName storeName, Uri targetService)
        {
            if (subjectName == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("subjectName");
            }
            this.SetScopedCertificate(StoreLocation.CurrentUser, StoreName.My, X509FindType.FindBySubjectDistinguishedName, subjectName, targetService);
        }

        public void SetScopedCertificate(StoreLocation storeLocation, StoreName storeName, X509FindType findType, object findValue, Uri targetService)
        {
            if (findValue == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("findValue");
            }
            if (targetService == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("targetService");
            }
            this.ThrowIfImmutable();
            X509Certificate2 certificate = System.ServiceModel.Security.SecurityUtils.GetCertificateFromStore(storeName, storeLocation, findType, findValue, null);
            this.ScopedCertificates[targetService] = certificate;
        }

        private void ThrowIfImmutable()
        {
            if (this.isReadOnly)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(System.ServiceModel.SR.GetString("ObjectIsReadOnly")));
            }
        }

        public X509ServiceCertificateAuthentication Authentication
        {
            get
            {
                return this.authentication;
            }
        }

        public X509Certificate2 DefaultCertificate
        {
            get
            {
                return this.defaultCertificate;
            }
            set
            {
                this.ThrowIfImmutable();
                this.defaultCertificate = value;
            }
        }

        public Dictionary<Uri, X509Certificate2> ScopedCertificates
        {
            get
            {
                return this.scopedCertificates;
            }
        }
    }
}

