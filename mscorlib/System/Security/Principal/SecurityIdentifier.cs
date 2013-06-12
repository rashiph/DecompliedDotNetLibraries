namespace System.Security.Principal
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Text;

    [ComVisible(false)]
    public sealed class SecurityIdentifier : IdentityReference, IComparable<SecurityIdentifier>
    {
        private SecurityIdentifier _AccountDomainSid;
        private bool _AccountDomainSidInitialized;
        private byte[] _BinaryForm;
        private System.Security.Principal.IdentifierAuthority _IdentifierAuthority;
        private string _SddlForm;
        private int[] _SubAuthorities;
        public static readonly int MaxBinaryLength = (8 + (MaxSubAuthorities * 4));
        internal static readonly long MaxIdentifierAuthority = 0xffffffffffffL;
        internal static readonly byte MaxSubAuthorities = 15;
        public static readonly int MinBinaryLength = 8;

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public SecurityIdentifier(IntPtr binaryForm) : this(binaryForm, true)
        {
        }

        [SecuritySafeCritical]
        public SecurityIdentifier(string sddlForm)
        {
            byte[] buffer;
            if (sddlForm == null)
            {
                throw new ArgumentNullException("sddlForm");
            }
            int errorCode = Win32.CreateSidFromString(sddlForm, out buffer);
            switch (errorCode)
            {
                case 0x539:
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"), "sddlForm");

                case 8:
                    throw new OutOfMemoryException();
            }
            if (errorCode != 0)
            {
                throw new SystemException(Win32Native.GetMessage(errorCode));
            }
            this.CreateFromBinaryForm(buffer, 0);
        }

        [SecurityCritical]
        internal SecurityIdentifier(IntPtr binaryForm, bool noDemand) : this(Win32.ConvertIntPtrSidToByteArraySid(binaryForm), 0)
        {
        }

        public SecurityIdentifier(byte[] binaryForm, int offset)
        {
            this.CreateFromBinaryForm(binaryForm, offset);
        }

        internal SecurityIdentifier(System.Security.Principal.IdentifierAuthority identifierAuthority, int[] subAuthorities)
        {
            this.CreateFromParts(identifierAuthority, subAuthorities);
        }

        internal SecurityIdentifier(SecurityIdentifier domainSid, uint rid)
        {
            int[] subAuthorities = new int[domainSid.SubAuthorityCount + 1];
            int index = 0;
            while (index < domainSid.SubAuthorityCount)
            {
                subAuthorities[index] = domainSid.GetSubAuthority(index);
                index++;
            }
            subAuthorities[index] = (int) rid;
            this.CreateFromParts(domainSid.IdentifierAuthority, subAuthorities);
        }

        [SecuritySafeCritical]
        public SecurityIdentifier(WellKnownSidType sidType, SecurityIdentifier domainSid)
        {
            byte[] buffer;
            if (sidType == WellKnownSidType.LogonIdsSid)
            {
                throw new ArgumentException(Environment.GetResourceString("IdentityReference_CannotCreateLogonIdsSid"), "sidType");
            }
            if (!Win32.WellKnownSidApisSupported)
            {
                throw new PlatformNotSupportedException(Environment.GetResourceString("PlatformNotSupported_RequiresW2kSP3"));
            }
            if ((sidType < WellKnownSidType.NullSid) || (sidType > WellKnownSidType.WinBuiltinTerminalServerLicenseServersSid))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidValue"), "sidType");
            }
            if ((sidType >= WellKnownSidType.AccountAdministratorSid) && (sidType <= WellKnownSidType.AccountRasAndIasServersSid))
            {
                SecurityIdentifier identifier;
                if (domainSid == null)
                {
                    throw new ArgumentNullException("domainSid", Environment.GetResourceString("IdentityReference_DomainSidRequired", new object[] { sidType }));
                }
                int windowsAccountDomainSid = Win32.GetWindowsAccountDomainSid(domainSid, out identifier);
                switch (windowsAccountDomainSid)
                {
                    case 0x7a:
                        throw new OutOfMemoryException();

                    case 0x4e9:
                        throw new ArgumentException(Environment.GetResourceString("IdentityReference_NotAWindowsDomain"), "domainSid");
                }
                if (windowsAccountDomainSid != 0)
                {
                    throw new SystemException(Win32Native.GetMessage(windowsAccountDomainSid));
                }
                if (identifier != domainSid)
                {
                    throw new ArgumentException(Environment.GetResourceString("IdentityReference_NotAWindowsDomain"), "domainSid");
                }
            }
            int errorCode = Win32.CreateWellKnownSid(sidType, domainSid, out buffer);
            if (errorCode == 0x57)
            {
                throw new ArgumentException(Win32Native.GetMessage(errorCode), "sidType/domainSid");
            }
            if (errorCode != 0)
            {
                throw new SystemException(Win32Native.GetMessage(errorCode));
            }
            this.CreateFromBinaryForm(buffer, 0);
        }

        public int CompareTo(SecurityIdentifier sid)
        {
            if (sid == null)
            {
                throw new ArgumentNullException("sid");
            }
            if (this.IdentifierAuthority < sid.IdentifierAuthority)
            {
                return -1;
            }
            if (this.IdentifierAuthority > sid.IdentifierAuthority)
            {
                return 1;
            }
            if (this.SubAuthorityCount < sid.SubAuthorityCount)
            {
                return -1;
            }
            if (this.SubAuthorityCount > sid.SubAuthorityCount)
            {
                return 1;
            }
            for (int i = 0; i < this.SubAuthorityCount; i++)
            {
                int num2 = this.GetSubAuthority(i) - sid.GetSubAuthority(i);
                if (num2 != 0)
                {
                    return num2;
                }
            }
            return 0;
        }

        private void CreateFromBinaryForm(byte[] binaryForm, int offset)
        {
            if (binaryForm == null)
            {
                throw new ArgumentNullException("binaryForm");
            }
            if (offset < 0)
            {
                throw new ArgumentOutOfRangeException("offset", offset, Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"));
            }
            if ((binaryForm.Length - offset) < MinBinaryLength)
            {
                throw new ArgumentOutOfRangeException("binaryForm", Environment.GetResourceString("ArgumentOutOfRange_ArrayTooSmall"));
            }
            if (binaryForm[offset] != Revision)
            {
                throw new ArgumentException(Environment.GetResourceString("IdentityReference_InvalidSidRevision"), "binaryForm");
            }
            if (binaryForm[offset + 1] > MaxSubAuthorities)
            {
                throw new ArgumentException(Environment.GetResourceString("IdentityReference_InvalidNumberOfSubauthorities", new object[] { MaxSubAuthorities }), "binaryForm");
            }
            int num = 8 + (4 * binaryForm[offset + 1]);
            if ((binaryForm.Length - offset) < num)
            {
                throw new ArgumentException(Environment.GetResourceString("ArgumentOutOfRange_ArrayTooSmall"), "binaryForm");
            }
            System.Security.Principal.IdentifierAuthority identifierAuthority = (System.Security.Principal.IdentifierAuthority) ((((((binaryForm[offset + 2] << 40) + (binaryForm[offset + 3] << 0x20)) + (binaryForm[offset + 4] << 0x18)) + (binaryForm[offset + 5] << 0x10)) + (binaryForm[offset + 6] << 8)) + binaryForm[offset + 7]);
            int[] subAuthorities = new int[binaryForm[offset + 1]];
            for (byte i = 0; i < binaryForm[offset + 1]; i = (byte) (i + 1))
            {
                subAuthorities[i] = ((binaryForm[(offset + 8) + (4 * i)] + (binaryForm[((offset + 8) + (4 * i)) + 1] << 8)) + (binaryForm[((offset + 8) + (4 * i)) + 2] << 0x10)) + (binaryForm[((offset + 8) + (4 * i)) + 3] << 0x18);
            }
            this.CreateFromParts(identifierAuthority, subAuthorities);
        }

        private void CreateFromParts(System.Security.Principal.IdentifierAuthority identifierAuthority, int[] subAuthorities)
        {
            byte num;
            if (subAuthorities == null)
            {
                throw new ArgumentNullException("subAuthorities");
            }
            if (subAuthorities.Length > MaxSubAuthorities)
            {
                throw new ArgumentOutOfRangeException("subAuthorities.Length", subAuthorities.Length, Environment.GetResourceString("IdentityReference_InvalidNumberOfSubauthorities", new object[] { MaxSubAuthorities }));
            }
            if ((identifierAuthority < System.Security.Principal.IdentifierAuthority.NullAuthority) || (identifierAuthority > MaxIdentifierAuthority))
            {
                throw new ArgumentOutOfRangeException("identifierAuthority", identifierAuthority, Environment.GetResourceString("IdentityReference_IdentifierAuthorityTooLarge"));
            }
            this._IdentifierAuthority = identifierAuthority;
            this._SubAuthorities = new int[subAuthorities.Length];
            subAuthorities.CopyTo(this._SubAuthorities, 0);
            this._BinaryForm = new byte[8 + (4 * this.SubAuthorityCount)];
            this._BinaryForm[0] = Revision;
            this._BinaryForm[1] = (byte) this.SubAuthorityCount;
            for (num = 0; num < 6; num = (byte) (num + 1))
            {
                this._BinaryForm[2 + num] = (byte) ((((long) this._IdentifierAuthority) >> ((5 - num) * 8)) & 0xffL);
            }
            for (num = 0; num < this.SubAuthorityCount; num = (byte) (num + 1))
            {
                for (byte i = 0; i < 4; i = (byte) (i + 1))
                {
                    this._BinaryForm[(8 + (4 * num)) + i] = (byte) (this._SubAuthorities[num] >> (i * 8));
                }
            }
        }

        public override bool Equals(object o)
        {
            if (o == null)
            {
                return false;
            }
            SecurityIdentifier identifier = o as SecurityIdentifier;
            if (identifier == null)
            {
                return false;
            }
            return (this == identifier);
        }

        public bool Equals(SecurityIdentifier sid)
        {
            if (sid == null)
            {
                return false;
            }
            return (this == sid);
        }

        [SecurityCritical]
        internal SecurityIdentifier GetAccountDomainSid()
        {
            SecurityIdentifier identifier;
            int windowsAccountDomainSid = Win32.GetWindowsAccountDomainSid(this, out identifier);
            switch (windowsAccountDomainSid)
            {
                case 0x7a:
                    throw new OutOfMemoryException();

                case 0x4e9:
                    return null;

                case 0:
                    return identifier;
            }
            throw new SystemException(Win32Native.GetMessage(windowsAccountDomainSid));
        }

        public void GetBinaryForm(byte[] binaryForm, int offset)
        {
            this._BinaryForm.CopyTo(binaryForm, offset);
        }

        public override int GetHashCode()
        {
            int hashCode = ((long) this.IdentifierAuthority).GetHashCode();
            for (int i = 0; i < this.SubAuthorityCount; i++)
            {
                hashCode ^= this.GetSubAuthority(i);
            }
            return hashCode;
        }

        internal int GetSubAuthority(int index)
        {
            return this._SubAuthorities[index];
        }

        [SecuritySafeCritical]
        public bool IsAccountSid()
        {
            if (!this._AccountDomainSidInitialized)
            {
                this._AccountDomainSid = this.GetAccountDomainSid();
                this._AccountDomainSidInitialized = true;
            }
            if (this._AccountDomainSid == null)
            {
                return false;
            }
            return true;
        }

        [SecuritySafeCritical]
        public bool IsEqualDomainSid(SecurityIdentifier sid)
        {
            return Win32.IsEqualDomainSid(this, sid);
        }

        public override bool IsValidTargetType(Type targetType)
        {
            return IsValidTargetTypeStatic(targetType);
        }

        internal static bool IsValidTargetTypeStatic(Type targetType)
        {
            return ((targetType == typeof(NTAccount)) || (targetType == typeof(SecurityIdentifier)));
        }

        [SecuritySafeCritical]
        public bool IsWellKnown(WellKnownSidType type)
        {
            return Win32.IsWellKnownSid(this, type);
        }

        public static bool operator ==(SecurityIdentifier left, SecurityIdentifier right)
        {
            object obj2 = left;
            object obj3 = right;
            return (((obj2 == null) && (obj3 == null)) || (((obj2 != null) && (obj3 != null)) && (left.CompareTo(right) == 0)));
        }

        public static bool operator !=(SecurityIdentifier left, SecurityIdentifier right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            if (this._SddlForm == null)
            {
                StringBuilder builder = new StringBuilder();
                builder.AppendFormat("S-1-{0}", (long) this._IdentifierAuthority);
                for (int i = 0; i < this.SubAuthorityCount; i++)
                {
                    builder.AppendFormat("-{0}", (uint) this._SubAuthorities[i]);
                }
                this._SddlForm = builder.ToString();
            }
            return this._SddlForm;
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlPrincipal=true)]
        public override IdentityReference Translate(Type targetType)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            if (targetType == typeof(SecurityIdentifier))
            {
                return this;
            }
            if (targetType != typeof(NTAccount))
            {
                throw new ArgumentException(Environment.GetResourceString("IdentityReference_MustBeIdentityReference"), "targetType");
            }
            return Translate(new IdentityReferenceCollection(1) { this }, targetType, 1)[0];
        }

        [SecurityCritical]
        internal static IdentityReferenceCollection Translate(IdentityReferenceCollection sourceSids, Type targetType, bool forceSuccess)
        {
            bool someFailed = false;
            IdentityReferenceCollection references = Translate(sourceSids, targetType, out someFailed);
            if (!forceSuccess || !someFailed)
            {
                return references;
            }
            IdentityReferenceCollection unmappedIdentities = new IdentityReferenceCollection();
            foreach (IdentityReference reference in references)
            {
                if (reference.GetType() != targetType)
                {
                    unmappedIdentities.Add(reference);
                }
            }
            throw new IdentityNotMappedException(Environment.GetResourceString("IdentityReference_IdentityNotMapped"), unmappedIdentities);
        }

        [SecurityCritical]
        internal static IdentityReferenceCollection Translate(IdentityReferenceCollection sourceSids, Type targetType, out bool someFailed)
        {
            if (sourceSids == null)
            {
                throw new ArgumentNullException("sourceSids");
            }
            if (targetType != typeof(NTAccount))
            {
                throw new ArgumentException(Environment.GetResourceString("IdentityReference_MustBeIdentityReference"), "targetType");
            }
            return TranslateToNTAccounts(sourceSids, out someFailed);
        }

        [SecurityCritical]
        private static IdentityReferenceCollection TranslateToNTAccounts(IdentityReferenceCollection sourceSids, out bool someFailed)
        {
            IdentityReferenceCollection references2;
            if (sourceSids == null)
            {
                throw new ArgumentNullException("sourceSids");
            }
            if (sourceSids.Count == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EmptyCollection"), "sourceSids");
            }
            IntPtr[] sids = new IntPtr[sourceSids.Count];
            GCHandle[] handleArray = new GCHandle[sourceSids.Count];
            SafeLsaPolicyHandle invalidHandle = SafeLsaPolicyHandle.InvalidHandle;
            SafeLsaMemoryHandle referencedDomains = SafeLsaMemoryHandle.InvalidHandle;
            SafeLsaMemoryHandle names = SafeLsaMemoryHandle.InvalidHandle;
            try
            {
                int index = 0;
                foreach (IdentityReference reference in sourceSids)
                {
                    SecurityIdentifier identifier = reference as SecurityIdentifier;
                    if (identifier == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_ImproperType"), "sourceSids");
                    }
                    handleArray[index] = GCHandle.Alloc(identifier.BinaryForm, GCHandleType.Pinned);
                    sids[index] = handleArray[index].AddrOfPinnedObject();
                    index++;
                }
                invalidHandle = Win32.LsaOpenPolicy(null, PolicyRights.POLICY_LOOKUP_NAMES);
                someFailed = false;
                uint num2 = Win32Native.LsaLookupSids(invalidHandle, sourceSids.Count, sids, ref referencedDomains, ref names);
                switch (num2)
                {
                    case 0xc0000017:
                    case 0xc000009a:
                        throw new OutOfMemoryException();

                    case 0xc0000022:
                        throw new UnauthorizedAccessException();
                }
                if ((num2 == 0xc0000073) || (num2 == 0x107))
                {
                    someFailed = true;
                }
                else if (num2 != 0)
                {
                    throw new SystemException(Win32Native.GetMessage(Win32Native.LsaNtStatusToWinError((int) num2)));
                }
                names.Initialize((uint) sourceSids.Count, (uint) Marshal.SizeOf(typeof(Win32Native.LSA_TRANSLATED_NAME)));
                Win32.InitializeReferencedDomainsPointer(referencedDomains);
                IdentityReferenceCollection references = new IdentityReferenceCollection(sourceSids.Count);
                if ((num2 == 0) || (num2 == 0x107))
                {
                    Win32Native.LSA_REFERENCED_DOMAIN_LIST lsa_referenced_domain_list = referencedDomains.Read<Win32Native.LSA_REFERENCED_DOMAIN_LIST>(0L);
                    string[] strArray = new string[lsa_referenced_domain_list.Entries];
                    for (int i = 0; i < lsa_referenced_domain_list.Entries; i++)
                    {
                        Win32Native.LSA_TRUST_INFORMATION lsa_trust_information = (Win32Native.LSA_TRUST_INFORMATION) Marshal.PtrToStructure(new IntPtr(((long) lsa_referenced_domain_list.Domains) + (i * Marshal.SizeOf(typeof(Win32Native.LSA_TRUST_INFORMATION)))), typeof(Win32Native.LSA_TRUST_INFORMATION));
                        strArray[i] = Marshal.PtrToStringUni(lsa_trust_information.Name.Buffer, lsa_trust_information.Name.Length / 2);
                    }
                    Win32Native.LSA_TRANSLATED_NAME[] array = new Win32Native.LSA_TRANSLATED_NAME[sourceSids.Count];
                    names.ReadArray<Win32Native.LSA_TRANSLATED_NAME>(0L, array, 0, array.Length);
                    for (int j = 0; j < sourceSids.Count; j++)
                    {
                        Win32Native.LSA_TRANSLATED_NAME lsa_translated_name = array[j];
                        switch (lsa_translated_name.Use)
                        {
                            case 1:
                            case 2:
                            case 4:
                            case 5:
                            case 9:
                            {
                                string accountName = Marshal.PtrToStringUni(lsa_translated_name.Name.Buffer, lsa_translated_name.Name.Length / 2);
                                string domainName = strArray[lsa_translated_name.DomainIndex];
                                references.Add(new NTAccount(domainName, accountName));
                                continue;
                            }
                        }
                        someFailed = true;
                        references.Add(sourceSids[j]);
                    }
                }
                else
                {
                    for (int k = 0; k < sourceSids.Count; k++)
                    {
                        references.Add(sourceSids[k]);
                    }
                }
                references2 = references;
            }
            finally
            {
                for (int m = 0; m < sourceSids.Count; m++)
                {
                    if (handleArray[m].IsAllocated)
                    {
                        handleArray[m].Free();
                    }
                }
                invalidHandle.Dispose();
                referencedDomains.Dispose();
                names.Dispose();
            }
            return references2;
        }

        public SecurityIdentifier AccountDomainSid
        {
            [SecuritySafeCritical]
            get
            {
                if (!this._AccountDomainSidInitialized)
                {
                    this._AccountDomainSid = this.GetAccountDomainSid();
                    this._AccountDomainSidInitialized = true;
                }
                return this._AccountDomainSid;
            }
        }

        internal byte[] BinaryForm
        {
            get
            {
                return this._BinaryForm;
            }
        }

        public int BinaryLength
        {
            get
            {
                return this._BinaryForm.Length;
            }
        }

        internal System.Security.Principal.IdentifierAuthority IdentifierAuthority
        {
            get
            {
                return this._IdentifierAuthority;
            }
        }

        internal static byte Revision
        {
            get
            {
                return 1;
            }
        }

        internal int SubAuthorityCount
        {
            get
            {
                return this._SubAuthorities.Length;
            }
        }

        public override string Value
        {
            get
            {
                return this.ToString().ToUpper(CultureInfo.InvariantCulture);
            }
        }
    }
}

