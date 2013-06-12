namespace System.Security.Cryptography.X509Certificates
{
    using Microsoft.Win32;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;
    using System.Security.Util;
    using System.Text;

    [Serializable, ComVisible(true)]
    public class X509Certificate : IDeserializationCallback, ISerializable
    {
        private const string m_format = "X509";
        private string m_issuerName;
        private DateTime m_notAfter;
        private DateTime m_notBefore;
        private string m_publicKeyOid;
        private byte[] m_publicKeyParameters;
        private byte[] m_publicKeyValue;
        private byte[] m_rawData;
        [SecurityCritical]
        private SafeCertContextHandle m_safeCertContext;
        private byte[] m_serialNumber;
        private string m_subjectName;
        private byte[] m_thumbprint;

        public X509Certificate()
        {
            this.Init();
        }

        public X509Certificate(byte[] data) : this()
        {
            if ((data != null) && (data.Length != 0))
            {
                this.LoadCertificateFromBlob(data, null, X509KeyStorageFlags.DefaultKeySet);
            }
        }

        [SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
        public X509Certificate(IntPtr handle) : this()
        {
            if (handle == IntPtr.Zero)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_InvalidHandle"), "handle");
            }
            X509Utils._DuplicateCertContext(handle, ref this.m_safeCertContext);
        }

        [SecuritySafeCritical]
        public X509Certificate(X509Certificate cert) : this()
        {
            if (cert == null)
            {
                throw new ArgumentNullException("cert");
            }
            if (cert.m_safeCertContext.pCertContext != IntPtr.Zero)
            {
                X509Utils._DuplicateCertContext(cert.m_safeCertContext.pCertContext, ref this.m_safeCertContext);
            }
            GC.KeepAlive(cert.m_safeCertContext);
        }

        [SecuritySafeCritical]
        public X509Certificate(string fileName) : this()
        {
            this.LoadCertificateFromFile(fileName, null, X509KeyStorageFlags.DefaultKeySet);
        }

        public X509Certificate(SerializationInfo info, StreamingContext context) : this()
        {
            byte[] rawData = (byte[]) info.GetValue("RawData", typeof(byte[]));
            if (rawData != null)
            {
                this.LoadCertificateFromBlob(rawData, null, X509KeyStorageFlags.DefaultKeySet);
            }
        }

        public X509Certificate(byte[] rawData, SecureString password) : this()
        {
            this.LoadCertificateFromBlob(rawData, password, X509KeyStorageFlags.DefaultKeySet);
        }

        [SecuritySafeCritical]
        public X509Certificate(string fileName, SecureString password) : this()
        {
            this.LoadCertificateFromFile(fileName, password, X509KeyStorageFlags.DefaultKeySet);
        }

        public X509Certificate(byte[] rawData, string password) : this()
        {
            this.LoadCertificateFromBlob(rawData, password, X509KeyStorageFlags.DefaultKeySet);
        }

        [SecuritySafeCritical]
        public X509Certificate(string fileName, string password) : this()
        {
            this.LoadCertificateFromFile(fileName, password, X509KeyStorageFlags.DefaultKeySet);
        }

        public X509Certificate(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags) : this()
        {
            this.LoadCertificateFromBlob(rawData, password, keyStorageFlags);
        }

        [SecuritySafeCritical]
        public X509Certificate(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags) : this()
        {
            this.LoadCertificateFromFile(fileName, password, keyStorageFlags);
        }

        public X509Certificate(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags) : this()
        {
            this.LoadCertificateFromBlob(rawData, password, keyStorageFlags);
        }

        [SecuritySafeCritical]
        public X509Certificate(string fileName, string password, X509KeyStorageFlags keyStorageFlags) : this()
        {
            this.LoadCertificateFromFile(fileName, password, keyStorageFlags);
        }

        [SecuritySafeCritical]
        public static X509Certificate CreateFromCertFile(string filename)
        {
            return new X509Certificate(filename);
        }

        [SecuritySafeCritical]
        public static X509Certificate CreateFromSignedFile(string filename)
        {
            return new X509Certificate(filename);
        }

        [ComVisible(false)]
        public override bool Equals(object obj)
        {
            if (!(obj is X509Certificate))
            {
                return false;
            }
            X509Certificate other = (X509Certificate) obj;
            return this.Equals(other);
        }

        [SecuritySafeCritical]
        public virtual bool Equals(X509Certificate other)
        {
            if (other == null)
            {
                return false;
            }
            if (this.m_safeCertContext.IsInvalid)
            {
                return other.m_safeCertContext.IsInvalid;
            }
            if (!this.Issuer.Equals(other.Issuer))
            {
                return false;
            }
            if (!this.SerialNumber.Equals(other.SerialNumber))
            {
                return false;
            }
            return true;
        }

        [SecuritySafeCritical, ComVisible(false)]
        public virtual byte[] Export(X509ContentType contentType)
        {
            return this.ExportHelper(contentType, null);
        }

        [SecuritySafeCritical]
        public virtual byte[] Export(X509ContentType contentType, SecureString password)
        {
            return this.ExportHelper(contentType, password);
        }

        [SecuritySafeCritical, ComVisible(false)]
        public virtual byte[] Export(X509ContentType contentType, string password)
        {
            return this.ExportHelper(contentType, password);
        }

        [SecurityCritical]
        private byte[] ExportHelper(X509ContentType contentType, object password)
        {
            switch (contentType)
            {
                case X509ContentType.Cert:
                case X509ContentType.SerializedCert:
                    break;

                case X509ContentType.Pfx:
                    new KeyContainerPermission(KeyContainerPermissionFlags.Export | KeyContainerPermissionFlags.Open).Demand();
                    break;

                default:
                    throw new CryptographicException(Environment.GetResourceString("Cryptography_X509_InvalidContentType"));
            }
            IntPtr zero = IntPtr.Zero;
            byte[] buffer = null;
            SafeCertStoreHandle safeCertStoreHandle = X509Utils.ExportCertToMemoryStore(this);
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                zero = X509Utils.PasswordToHGlobalUni(password);
                buffer = X509Utils._ExportCertificatesToBlob(safeCertStoreHandle, contentType, zero);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(zero);
                }
                safeCertStoreHandle.Dispose();
            }
            if (buffer == null)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_X509_ExportFailed"));
            }
            return buffer;
        }

        protected static string FormatDate(DateTime date)
        {
            CultureInfo currentCulture = CultureInfo.CurrentCulture;
            if (!currentCulture.DateTimeFormat.Calendar.IsValidDay(date.Year, date.Month, date.Day, 0))
            {
                if (currentCulture.DateTimeFormat.Calendar is UmAlQuraCalendar)
                {
                    currentCulture = currentCulture.Clone() as CultureInfo;
                    currentCulture.DateTimeFormat.Calendar = new HijriCalendar();
                }
                else
                {
                    currentCulture = CultureInfo.InvariantCulture;
                }
            }
            return date.ToString(currentCulture);
        }

        public virtual byte[] GetCertHash()
        {
            this.SetThumbprint();
            return (byte[]) this.m_thumbprint.Clone();
        }

        public virtual string GetCertHashString()
        {
            this.SetThumbprint();
            return Hex.EncodeHexString(this.m_thumbprint);
        }

        public virtual string GetEffectiveDateString()
        {
            return this.NotBefore.ToString();
        }

        public virtual string GetExpirationDateString()
        {
            return this.NotAfter.ToString();
        }

        public virtual string GetFormat()
        {
            return "X509";
        }

        [SecuritySafeCritical]
        public override int GetHashCode()
        {
            if (this.m_safeCertContext.IsInvalid)
            {
                return 0;
            }
            this.SetThumbprint();
            int num = 0;
            for (int i = 0; (i < this.m_thumbprint.Length) && (i < 4); i++)
            {
                num = (num << 8) | this.m_thumbprint[i];
            }
            return num;
        }

        [SecuritySafeCritical, Obsolete("This method has been deprecated.  Please use the Issuer property instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public virtual string GetIssuerName()
        {
            this.ThrowIfContextInvalid();
            return X509Utils._GetIssuerName(this.m_safeCertContext, true);
        }

        [SecuritySafeCritical]
        public virtual string GetKeyAlgorithm()
        {
            this.ThrowIfContextInvalid();
            if (this.m_publicKeyOid == null)
            {
                this.m_publicKeyOid = X509Utils._GetPublicKeyOid(this.m_safeCertContext);
            }
            return this.m_publicKeyOid;
        }

        [SecuritySafeCritical]
        public virtual byte[] GetKeyAlgorithmParameters()
        {
            this.ThrowIfContextInvalid();
            if (this.m_publicKeyParameters == null)
            {
                this.m_publicKeyParameters = X509Utils._GetPublicKeyParameters(this.m_safeCertContext);
            }
            return (byte[]) this.m_publicKeyParameters.Clone();
        }

        [SecuritySafeCritical]
        public virtual string GetKeyAlgorithmParametersString()
        {
            this.ThrowIfContextInvalid();
            return Hex.EncodeHexString(this.GetKeyAlgorithmParameters());
        }

        [Obsolete("This method has been deprecated.  Please use the Subject property instead.  http://go.microsoft.com/fwlink/?linkid=14202"), SecuritySafeCritical]
        public virtual string GetName()
        {
            this.ThrowIfContextInvalid();
            return X509Utils._GetSubjectInfo(this.m_safeCertContext, 2, true);
        }

        [SecuritySafeCritical]
        public virtual byte[] GetPublicKey()
        {
            this.ThrowIfContextInvalid();
            if (this.m_publicKeyValue == null)
            {
                this.m_publicKeyValue = X509Utils._GetPublicKeyValue(this.m_safeCertContext);
            }
            return (byte[]) this.m_publicKeyValue.Clone();
        }

        public virtual string GetPublicKeyString()
        {
            return Hex.EncodeHexString(this.GetPublicKey());
        }

        [SecuritySafeCritical]
        public virtual byte[] GetRawCertData()
        {
            return this.RawData;
        }

        public virtual string GetRawCertDataString()
        {
            return Hex.EncodeHexString(this.GetRawCertData());
        }

        [SecuritySafeCritical]
        public virtual byte[] GetSerialNumber()
        {
            this.ThrowIfContextInvalid();
            if (this.m_serialNumber == null)
            {
                this.m_serialNumber = X509Utils._GetSerialNumber(this.m_safeCertContext);
            }
            return (byte[]) this.m_serialNumber.Clone();
        }

        public virtual string GetSerialNumberString()
        {
            return this.SerialNumber;
        }

        [ComVisible(false), SecurityCritical, PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public virtual void Import(byte[] rawData)
        {
            this.Reset();
            this.LoadCertificateFromBlob(rawData, null, X509KeyStorageFlags.DefaultKeySet);
        }

        [ComVisible(false), SecurityCritical, PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public virtual void Import(string fileName)
        {
            this.Reset();
            this.LoadCertificateFromFile(fileName, null, X509KeyStorageFlags.DefaultKeySet);
        }

        [SecurityCritical, PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public virtual void Import(byte[] rawData, SecureString password, X509KeyStorageFlags keyStorageFlags)
        {
            this.Reset();
            this.LoadCertificateFromBlob(rawData, password, keyStorageFlags);
        }

        [SecurityCritical, PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public virtual void Import(string fileName, SecureString password, X509KeyStorageFlags keyStorageFlags)
        {
            this.Reset();
            this.LoadCertificateFromFile(fileName, password, keyStorageFlags);
        }

        [ComVisible(false), SecurityCritical, PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public virtual void Import(byte[] rawData, string password, X509KeyStorageFlags keyStorageFlags)
        {
            this.Reset();
            this.LoadCertificateFromBlob(rawData, password, keyStorageFlags);
        }

        [SecurityCritical, ComVisible(false), PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public virtual void Import(string fileName, string password, X509KeyStorageFlags keyStorageFlags)
        {
            this.Reset();
            this.LoadCertificateFromFile(fileName, password, keyStorageFlags);
        }

        [SecuritySafeCritical]
        private void Init()
        {
            this.m_safeCertContext = SafeCertContextHandle.InvalidHandle;
        }

        [SecuritySafeCritical]
        private void LoadCertificateFromBlob(byte[] rawData, object password, X509KeyStorageFlags keyStorageFlags)
        {
            if ((rawData == null) || (rawData.Length == 0))
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EmptyOrNullArray"), "rawData");
            }
            if ((X509Utils.MapContentType(X509Utils._QueryCertBlobType(rawData)) == X509ContentType.Pfx) && ((keyStorageFlags & X509KeyStorageFlags.PersistKeySet) == X509KeyStorageFlags.PersistKeySet))
            {
                new KeyContainerPermission(KeyContainerPermissionFlags.Create).Demand();
            }
            uint dwFlags = X509Utils.MapKeyStorageFlags(keyStorageFlags);
            IntPtr zero = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                zero = X509Utils.PasswordToHGlobalUni(password);
                X509Utils._LoadCertFromBlob(rawData, zero, dwFlags, (keyStorageFlags & X509KeyStorageFlags.PersistKeySet) != X509KeyStorageFlags.DefaultKeySet, ref this.m_safeCertContext);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(zero);
                }
            }
        }

        [SecurityCritical]
        private void LoadCertificateFromFile(string fileName, object password, X509KeyStorageFlags keyStorageFlags)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }
            string fullPathInternal = Path.GetFullPathInternal(fileName);
            new FileIOPermission(FileIOPermissionAccess.Read, fullPathInternal).Demand();
            if ((X509Utils.MapContentType(X509Utils._QueryCertFileType(fileName)) == X509ContentType.Pfx) && ((keyStorageFlags & X509KeyStorageFlags.PersistKeySet) == X509KeyStorageFlags.PersistKeySet))
            {
                new KeyContainerPermission(KeyContainerPermissionFlags.Create).Demand();
            }
            uint dwFlags = X509Utils.MapKeyStorageFlags(keyStorageFlags);
            IntPtr zero = IntPtr.Zero;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                zero = X509Utils.PasswordToHGlobalUni(password);
                X509Utils._LoadCertFromFile(fileName, zero, dwFlags, (keyStorageFlags & X509KeyStorageFlags.PersistKeySet) != X509KeyStorageFlags.DefaultKeySet, ref this.m_safeCertContext);
            }
            finally
            {
                if (zero != IntPtr.Zero)
                {
                    Marshal.ZeroFreeGlobalAllocUnicode(zero);
                }
            }
        }

        [SecurityCritical, ComVisible(false), PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
        public virtual void Reset()
        {
            this.m_subjectName = null;
            this.m_issuerName = null;
            this.m_serialNumber = null;
            this.m_publicKeyParameters = null;
            this.m_publicKeyValue = null;
            this.m_publicKeyOid = null;
            this.m_rawData = null;
            this.m_thumbprint = null;
            this.m_notBefore = DateTime.MinValue;
            this.m_notAfter = DateTime.MinValue;
            if (!this.m_safeCertContext.IsInvalid)
            {
                this.m_safeCertContext.Dispose();
                this.m_safeCertContext = SafeCertContextHandle.InvalidHandle;
            }
        }

        [SecuritySafeCritical]
        private void SetThumbprint()
        {
            this.ThrowIfContextInvalid();
            if (this.m_thumbprint == null)
            {
                this.m_thumbprint = X509Utils._GetThumbprint(this.m_safeCertContext);
            }
        }

        void IDeserializationCallback.OnDeserialization(object sender)
        {
        }

        [SecurityCritical]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (this.m_safeCertContext.IsInvalid)
            {
                info.AddValue("RawData", null);
            }
            else
            {
                info.AddValue("RawData", this.RawData);
            }
        }

        [SecurityCritical]
        private void ThrowIfContextInvalid()
        {
            if (this.m_safeCertContext.IsInvalid)
            {
                throw new CryptographicException(Environment.GetResourceString("Cryptography_InvalidHandle"), "m_safeCertContext");
            }
        }

        public override string ToString()
        {
            return this.ToString(false);
        }

        [SecuritySafeCritical]
        public virtual string ToString(bool fVerbose)
        {
            if (!fVerbose || this.m_safeCertContext.IsInvalid)
            {
                return base.GetType().FullName;
            }
            StringBuilder builder = new StringBuilder();
            builder.Append("[Subject]" + Environment.NewLine + "  ");
            builder.Append(this.Subject);
            builder.Append(Environment.NewLine + Environment.NewLine + "[Issuer]" + Environment.NewLine + "  ");
            builder.Append(this.Issuer);
            builder.Append(Environment.NewLine + Environment.NewLine + "[Serial Number]" + Environment.NewLine + "  ");
            builder.Append(this.SerialNumber);
            builder.Append(Environment.NewLine + Environment.NewLine + "[Not Before]" + Environment.NewLine + "  ");
            builder.Append(FormatDate(this.NotBefore));
            builder.Append(Environment.NewLine + Environment.NewLine + "[Not After]" + Environment.NewLine + "  ");
            builder.Append(FormatDate(this.NotAfter));
            builder.Append(Environment.NewLine + Environment.NewLine + "[Thumbprint]" + Environment.NewLine + "  ");
            builder.Append(this.GetCertHashString());
            builder.Append(Environment.NewLine);
            return builder.ToString();
        }

        internal SafeCertContextHandle CertContext
        {
            [SecurityCritical]
            get
            {
                return this.m_safeCertContext;
            }
        }

        [ComVisible(false)]
        public IntPtr Handle
        {
            [SecurityCritical, SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                return this.m_safeCertContext.pCertContext;
            }
        }

        public string Issuer
        {
            [SecuritySafeCritical]
            get
            {
                this.ThrowIfContextInvalid();
                if (this.m_issuerName == null)
                {
                    this.m_issuerName = X509Utils._GetIssuerName(this.m_safeCertContext, false);
                }
                return this.m_issuerName;
            }
        }

        private DateTime NotAfter
        {
            [SecuritySafeCritical]
            get
            {
                this.ThrowIfContextInvalid();
                if (this.m_notAfter == DateTime.MinValue)
                {
                    Win32Native.FILE_TIME fileTime = new Win32Native.FILE_TIME();
                    X509Utils._GetDateNotAfter(this.m_safeCertContext, ref fileTime);
                    this.m_notAfter = DateTime.FromFileTime(fileTime.ToTicks());
                }
                return this.m_notAfter;
            }
        }

        private DateTime NotBefore
        {
            [SecuritySafeCritical]
            get
            {
                this.ThrowIfContextInvalid();
                if (this.m_notBefore == DateTime.MinValue)
                {
                    Win32Native.FILE_TIME fileTime = new Win32Native.FILE_TIME();
                    X509Utils._GetDateNotBefore(this.m_safeCertContext, ref fileTime);
                    this.m_notBefore = DateTime.FromFileTime(fileTime.ToTicks());
                }
                return this.m_notBefore;
            }
        }

        private byte[] RawData
        {
            [SecurityCritical]
            get
            {
                this.ThrowIfContextInvalid();
                if (this.m_rawData == null)
                {
                    this.m_rawData = X509Utils._GetCertRawData(this.m_safeCertContext);
                }
                return (byte[]) this.m_rawData.Clone();
            }
        }

        private string SerialNumber
        {
            [SecuritySafeCritical]
            get
            {
                this.ThrowIfContextInvalid();
                if (this.m_serialNumber == null)
                {
                    this.m_serialNumber = X509Utils._GetSerialNumber(this.m_safeCertContext);
                }
                return Hex.EncodeHexStringFromInt(this.m_serialNumber);
            }
        }

        public string Subject
        {
            [SecuritySafeCritical]
            get
            {
                this.ThrowIfContextInvalid();
                if (this.m_subjectName == null)
                {
                    this.m_subjectName = X509Utils._GetSubjectInfo(this.m_safeCertContext, 2, false);
                }
                return this.m_subjectName;
            }
        }
    }
}

