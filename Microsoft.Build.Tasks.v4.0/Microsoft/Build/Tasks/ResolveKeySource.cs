namespace Microsoft.Build.Tasks
{
    using Microsoft.Build.Framework;
    using Microsoft.Build.Shared;
    using Microsoft.Runtime.Hosting;
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Security.Cryptography;
    using System.Security.Cryptography.X509Certificates;
    using System.Text;

    public class ResolveKeySource : TaskExtension
    {
        private int autoClosePasswordPromptShow = 15;
        private int autoClosePasswordPromptTimeout = 20;
        private string certificateFile;
        private string certificateThumbprint;
        private string keyFile;
        private const string pfxFileContainerPrefix = "VS_KEY_";
        private const string pfxFileExtension = ".pfx";
        private static Hashtable pfxKeysToIgnore = new Hashtable(StringComparer.OrdinalIgnoreCase);
        private string resolvedKeyContainer = string.Empty;
        private string resolvedKeyFile = string.Empty;
        private string resolvedThumbprint = string.Empty;
        private bool showImportDialogDespitePreviousFailures;
        private bool suppressAutoClosePasswordPrompt;

        public override bool Execute()
        {
            return (this.ResolveAssemblyKey() && this.ResolveManifestKey());
        }

        private static ulong HashFromBlob(byte[] data)
        {
            uint num = 0x1089355;
            uint num2 = 0x12b5e65;
            uint num3 = 0xa4d92f;
            foreach (byte num4 in data)
            {
                uint num5 = num4 ^ num3;
                num3 *= 0xa4d92f;
                num += ((num5 ^ num2) * 0xf158ef) + 0x1090501;
                num2 ^= ((num5 + num) * 0xe4a565) ^ 0xb33cc3;
            }
            ulong num6 = num;
            num6 = num6 << 0x20;
            return (num6 | num2);
        }

        private bool ResolveAssemblyKey()
        {
            bool flag = true;
            if ((this.KeyFile != null) && (this.KeyFile.Length > 0))
            {
                string strA = string.Empty;
                try
                {
                    strA = Path.GetExtension(this.KeyFile);
                }
                catch (ArgumentException exception)
                {
                    base.Log.LogErrorWithCodeFromResources("ResolveKeySource.InvalidKeyName", new object[] { this.KeyFile, exception.Message });
                    flag = false;
                }
                if (!flag)
                {
                    return flag;
                }
                if (string.Compare(strA, ".pfx", StringComparison.OrdinalIgnoreCase) != 0)
                {
                    this.ResolvedKeyFile = this.KeyFile;
                    return flag;
                }
                flag = false;
                FileStream stream = null;
                try
                {
                    string pwzKeyContainer = string.Empty;
                    string str3 = Environment.UserDomainName + @"\" + Environment.UserName;
                    byte[] bytes = Encoding.Unicode.GetBytes(str3.ToLower(CultureInfo.InvariantCulture));
                    stream = File.OpenRead(this.KeyFile);
                    int length = (int) stream.Length;
                    byte[] buffer = new byte[length];
                    stream.Read(buffer, 0, length);
                    pwzKeyContainer = "VS_KEY_" + ((HashFromBlob(buffer) ^ HashFromBlob(bytes))).ToString("X016", CultureInfo.InvariantCulture);
                    IntPtr zero = IntPtr.Zero;
                    int pcbPublicKeyBlob = 0;
                    if (Microsoft.Runtime.Hosting.StrongNameHelpers.StrongNameGetPublicKey(pwzKeyContainer, IntPtr.Zero, 0, out zero, out pcbPublicKeyBlob) && (zero != IntPtr.Zero))
                    {
                        Microsoft.Runtime.Hosting.StrongNameHelpers.StrongNameFreeBuffer(zero);
                        flag = true;
                    }
                    else
                    {
                        if (this.ShowImportDialogDespitePreviousFailures || !pfxKeysToIgnore.Contains(pwzKeyContainer))
                        {
                            base.Log.LogErrorWithCodeFromResources("ResolveKeySource.KeyFileForSignAssemblyNotImported", new object[] { this.KeyFile, pwzKeyContainer });
                        }
                        if (!flag)
                        {
                            base.Log.LogErrorWithCodeFromResources("ResolveKeySource.KeyImportError", new object[] { this.KeyFile });
                        }
                    }
                    if (flag)
                    {
                        this.ResolvedKeyContainer = pwzKeyContainer;
                    }
                }
                catch (Exception exception2)
                {
                    if (Microsoft.Build.Shared.ExceptionHandling.NotExpectedException(exception2))
                    {
                        throw;
                    }
                    base.Log.LogErrorWithCodeFromResources("ResolveKeySource.KeyMD5SumError", new object[] { this.KeyFile, exception2.Message });
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
            }
            return flag;
        }

        private bool ResolveManifestKey()
        {
            bool flag = false;
            bool flag2 = false;
            if (!string.IsNullOrEmpty(this.CertificateThumbprint))
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                try
                {
                    store.Open(OpenFlags.ReadWrite);
                    if (store.Certificates.Find(X509FindType.FindByThumbprint, this.CertificateThumbprint, false).Count == 1)
                    {
                        flag2 = true;
                        this.ResolvedThumbprint = this.CertificateThumbprint;
                        flag = true;
                    }
                }
                finally
                {
                    store.Close();
                }
            }
            if (!string.IsNullOrEmpty(this.CertificateFile) && !flag2)
            {
                if (!File.Exists(this.CertificateFile))
                {
                    base.Log.LogErrorWithCodeFromResources("ResolveKeySource.CertificateNotInStore", new object[0]);
                    return flag;
                }
                if (X509Certificate2.GetCertContentType(this.CertificateFile) == X509ContentType.Pfx)
                {
                    bool flag3 = false;
                    X509Certificate2 certificate = new X509Certificate2();
                    X509Store store2 = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                    try
                    {
                        store2.Open(OpenFlags.ReadWrite);
                        certificate.Import(this.CertificateFile, (string) null, X509KeyStorageFlags.PersistKeySet);
                        store2.Add(certificate);
                        this.ResolvedThumbprint = certificate.Thumbprint;
                        flag3 = true;
                        flag = true;
                    }
                    catch (CryptographicException)
                    {
                    }
                    finally
                    {
                        store2.Close();
                    }
                    if (!flag3 && this.ShowImportDialogDespitePreviousFailures)
                    {
                        base.Log.LogErrorWithCodeFromResources("ResolveKeySource.KeyFileForManifestNotImported", new object[] { this.KeyFile });
                    }
                    if (!flag)
                    {
                        base.Log.LogErrorWithCodeFromResources("ResolveKeySource.KeyImportError", new object[] { this.CertificateFile });
                    }
                    return flag;
                }
                X509Store store3 = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                try
                {
                    try
                    {
                        X509Certificate2 certificate2 = new X509Certificate2(this.CertificateFile);
                        store3.Open(OpenFlags.ReadWrite);
                        store3.Add(certificate2);
                        this.ResolvedThumbprint = certificate2.Thumbprint;
                        flag = true;
                    }
                    catch (CryptographicException)
                    {
                        base.Log.LogErrorWithCodeFromResources("ResolveKeySource.KeyImportError", new object[] { this.CertificateFile });
                    }
                    return flag;
                }
                finally
                {
                    store3.Close();
                }
            }
            if ((!flag2 && !string.IsNullOrEmpty(this.CertificateFile)) && !string.IsNullOrEmpty(this.CertificateThumbprint))
            {
                base.Log.LogErrorWithCodeFromResources("ResolveKeySource.CertificateNotInStore", new object[0]);
                return false;
            }
            return true;
        }

        public int AutoClosePasswordPromptShow
        {
            get
            {
                return this.autoClosePasswordPromptShow;
            }
            set
            {
                this.autoClosePasswordPromptShow = value;
            }
        }

        public int AutoClosePasswordPromptTimeout
        {
            get
            {
                return this.autoClosePasswordPromptTimeout;
            }
            set
            {
                this.autoClosePasswordPromptTimeout = value;
            }
        }

        public string CertificateFile
        {
            get
            {
                return this.certificateFile;
            }
            set
            {
                this.certificateFile = value;
            }
        }

        public string CertificateThumbprint
        {
            get
            {
                return this.certificateThumbprint;
            }
            set
            {
                this.certificateThumbprint = value;
            }
        }

        public string KeyFile
        {
            get
            {
                return this.keyFile;
            }
            set
            {
                this.keyFile = value;
            }
        }

        [Output]
        public string ResolvedKeyContainer
        {
            get
            {
                return this.resolvedKeyContainer;
            }
            set
            {
                this.resolvedKeyContainer = value;
            }
        }

        [Output]
        public string ResolvedKeyFile
        {
            get
            {
                return this.resolvedKeyFile;
            }
            set
            {
                this.resolvedKeyFile = value;
            }
        }

        [Output]
        public string ResolvedThumbprint
        {
            get
            {
                return this.resolvedThumbprint;
            }
            set
            {
                this.resolvedThumbprint = value;
            }
        }

        public bool ShowImportDialogDespitePreviousFailures
        {
            get
            {
                return this.showImportDialogDespitePreviousFailures;
            }
            set
            {
                this.showImportDialogDespitePreviousFailures = value;
            }
        }

        public bool SuppressAutoClosePasswordPrompt
        {
            get
            {
                return this.suppressAutoClosePasswordPrompt;
            }
            set
            {
                this.suppressAutoClosePasswordPrompt = value;
            }
        }
    }
}

