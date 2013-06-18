namespace System.DirectoryServices
{
    using System;
    using System.ComponentModel;
    using System.DirectoryServices.Interop;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DirectoryServicesPermission(SecurityAction.LinkDemand, Unrestricted=true)]
    public class DirectoryEntryConfiguration
    {
        private DirectoryEntry entry;
        private const int ISC_RET_MUTUAL_AUTH = 2;

        internal DirectoryEntryConfiguration(DirectoryEntry entry)
        {
            this.entry = entry;
        }

        public string GetCurrentServerName()
        {
            return (string) ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions) this.entry.AdsObject).GetOption(0);
        }

        public bool IsMutuallyAuthenticated()
        {
            try
            {
                int option = (int) ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions) this.entry.AdsObject).GetOption(4);
                return ((option & 2) != 0);
            }
            catch (COMException exception)
            {
                if (exception.ErrorCode != -2147463160)
                {
                    throw;
                }
                return false;
            }
        }

        public void SetUserNameQueryQuota(string accountName)
        {
            ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions) this.entry.AdsObject).SetOption(5, accountName);
        }

        public int PageSize
        {
            get
            {
                return (int) ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions) this.entry.AdsObject).GetOption(2);
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException(Res.GetString("DSBadPageSize"));
                }
                ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions) this.entry.AdsObject).SetOption(2, value);
            }
        }

        public PasswordEncodingMethod PasswordEncoding
        {
            get
            {
                return (PasswordEncodingMethod) ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions) this.entry.AdsObject).GetOption(7);
            }
            set
            {
                if ((value < PasswordEncodingMethod.PasswordEncodingSsl) || (value > PasswordEncodingMethod.PasswordEncodingClear))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(PasswordEncodingMethod));
                }
                ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions) this.entry.AdsObject).SetOption(7, value);
            }
        }

        public int PasswordPort
        {
            get
            {
                return (int) ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions) this.entry.AdsObject).GetOption(6);
            }
            set
            {
                ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions) this.entry.AdsObject).SetOption(6, value);
            }
        }

        public ReferralChasingOption Referral
        {
            get
            {
                return (ReferralChasingOption) ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions) this.entry.AdsObject).GetOption(1);
            }
            set
            {
                if (((value != ReferralChasingOption.None) && (value != ReferralChasingOption.Subordinate)) && ((value != ReferralChasingOption.External) && (value != ReferralChasingOption.All)))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ReferralChasingOption));
                }
                ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions) this.entry.AdsObject).SetOption(1, value);
            }
        }

        public System.DirectoryServices.SecurityMasks SecurityMasks
        {
            get
            {
                return (System.DirectoryServices.SecurityMasks) ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions) this.entry.AdsObject).GetOption(3);
            }
            set
            {
                if (value > (System.DirectoryServices.SecurityMasks.Sacl | System.DirectoryServices.SecurityMasks.Dacl | System.DirectoryServices.SecurityMasks.Group | System.DirectoryServices.SecurityMasks.Owner))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(System.DirectoryServices.SecurityMasks));
                }
                ((System.DirectoryServices.Interop.UnsafeNativeMethods.IAdsObjectOptions) this.entry.AdsObject).SetOption(3, value);
            }
        }
    }
}

