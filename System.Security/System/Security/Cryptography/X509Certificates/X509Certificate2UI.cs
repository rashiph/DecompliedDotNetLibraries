namespace System.Security.Cryptography.X509Certificates
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Cryptography;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, MayLeakOnAbort=true)]
    public static class X509Certificate2UI
    {
        [SecuritySafeCritical]
        public static void DisplayCertificate(X509Certificate2 certificate)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
            DisplayX509Certificate(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(certificate), IntPtr.Zero);
        }

        [SecurityCritical]
        public static void DisplayCertificate(X509Certificate2 certificate, IntPtr hwndParent)
        {
            if (certificate == null)
            {
                throw new ArgumentNullException("certificate");
            }
            DisplayX509Certificate(System.Security.Cryptography.X509Certificates.X509Utils.GetCertContext(certificate), hwndParent);
        }

        [SecurityCritical]
        private static void DisplayX509Certificate(System.Security.Cryptography.SafeCertContextHandle safeCertContext, IntPtr hwndParent)
        {
            System.Security.Cryptography.CAPI.CRYPTUI_VIEWCERTIFICATE_STRUCTW cryptui_viewcertificate_structw;
            if (safeCertContext.IsInvalid)
            {
                throw new CryptographicException(SecurityResources.GetResourceString("Cryptography_InvalidHandle"), "safeCertContext");
            }
            int num = 0;
            cryptui_viewcertificate_structw = new System.Security.Cryptography.CAPI.CRYPTUI_VIEWCERTIFICATE_STRUCTW {
                dwSize = Marshal.SizeOf(cryptui_viewcertificate_structw),
                hwndParent = hwndParent,
                dwFlags = 0,
                szTitle = null,
                pCertContext = safeCertContext.DangerousGetHandle(),
                rgszPurposes = IntPtr.Zero,
                cPurposes = 0,
                pCryptProviderData = IntPtr.Zero,
                fpCryptProviderDataTrustedUsage = false,
                idxSigner = 0,
                idxCert = 0,
                fCounterSigner = false,
                idxCounterSigner = 0,
                cStores = 0,
                rghStores = IntPtr.Zero,
                cPropSheetPages = 0,
                rgPropSheetPages = IntPtr.Zero,
                nStartPage = 0
            };
            if (!System.Security.Cryptography.CAPI.CryptUIDlgViewCertificateW(cryptui_viewcertificate_structw, IntPtr.Zero))
            {
                num = Marshal.GetLastWin32Error();
            }
            if ((num != 0) && (num != 0x4c7))
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
        }

        public static X509Certificate2Collection SelectFromCollection(X509Certificate2Collection certificates, string title, string message, X509SelectionFlag selectionFlag)
        {
            return SelectFromCollectionHelper(certificates, title, message, selectionFlag, IntPtr.Zero);
        }

        [SecurityCritical]
        public static X509Certificate2Collection SelectFromCollection(X509Certificate2Collection certificates, string title, string message, X509SelectionFlag selectionFlag, IntPtr hwndParent)
        {
            return SelectFromCollectionHelper(certificates, title, message, selectionFlag, hwndParent);
        }

        [SecuritySafeCritical]
        private static X509Certificate2Collection SelectFromCollectionHelper(X509Certificate2Collection certificates, string title, string message, X509SelectionFlag selectionFlag, IntPtr hwndParent)
        {
            X509Certificate2Collection certificates2;
            if (certificates == null)
            {
                throw new ArgumentNullException("certificates");
            }
            if ((selectionFlag < X509SelectionFlag.SingleSelection) || (selectionFlag > X509SelectionFlag.MultiSelection))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, SecurityResources.GetResourceString("Arg_EnumIllegalVal"), new object[] { "selectionFlag" }));
            }
            new StorePermission(StorePermissionFlags.AllFlags).Assert();
            using (System.Security.Cryptography.SafeCertStoreHandle handle = System.Security.Cryptography.X509Certificates.X509Utils.ExportToMemoryStore(certificates))
            {
                using (System.Security.Cryptography.SafeCertStoreHandle handle2 = SelectFromStore(handle, title, message, selectionFlag, hwndParent))
                {
                    certificates2 = System.Security.Cryptography.X509Certificates.X509Utils.GetCertificates(handle2);
                }
            }
            return certificates2;
        }

        [SecurityCritical]
        private static unsafe System.Security.Cryptography.SafeCertStoreHandle SelectFromStore(System.Security.Cryptography.SafeCertStoreHandle safeSourceStoreHandle, string title, string message, X509SelectionFlag selectionFlags, IntPtr hwndParent)
        {
            int num = 0;
            System.Security.Cryptography.SafeCertStoreHandle hCertStore = System.Security.Cryptography.CAPI.CertOpenStore((IntPtr) 2L, 0x10001, IntPtr.Zero, 0, null);
            if ((hCertStore == null) || hCertStore.IsInvalid)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            System.Security.Cryptography.CAPI.CRYPTUI_SELECTCERTIFICATE_STRUCTW csc = new System.Security.Cryptography.CAPI.CRYPTUI_SELECTCERTIFICATE_STRUCTW {
                dwSize = (uint) ((int) Marshal.OffsetOf(typeof(System.Security.Cryptography.CAPI.CRYPTUI_SELECTCERTIFICATE_STRUCTW), "hSelectedCertStore")),
                hwndParent = hwndParent,
                dwFlags = (uint) selectionFlags,
                szTitle = title,
                dwDontUseColumn = 0,
                szDisplayString = message,
                pFilterCallback = IntPtr.Zero,
                pDisplayCallback = IntPtr.Zero,
                pvCallbackData = IntPtr.Zero,
                cDisplayStores = 1
            };
            IntPtr handle = safeSourceStoreHandle.DangerousGetHandle();
            csc.rghDisplayStores = new IntPtr((void*) &handle);
            csc.cStores = 0;
            csc.rghStores = IntPtr.Zero;
            csc.cPropSheetPages = 0;
            csc.rgPropSheetPages = IntPtr.Zero;
            csc.hSelectedCertStore = hCertStore.DangerousGetHandle();
            System.Security.Cryptography.SafeCertContextHandle pCertContext = System.Security.Cryptography.CAPI.CryptUIDlgSelectCertificateW(csc);
            if ((pCertContext != null) && !pCertContext.IsInvalid)
            {
                System.Security.Cryptography.SafeCertContextHandle invalidHandle = System.Security.Cryptography.SafeCertContextHandle.InvalidHandle;
                if (!System.Security.Cryptography.CAPI.CertAddCertificateContextToStore(hCertStore, pCertContext, 7, invalidHandle))
                {
                    num = Marshal.GetLastWin32Error();
                }
            }
            if (num != 0)
            {
                throw new CryptographicException(Marshal.GetLastWin32Error());
            }
            return hCertStore;
        }
    }
}

