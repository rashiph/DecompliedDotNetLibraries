namespace System.Security.Principal
{
    using Microsoft.Win32;
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [ComVisible(false)]
    public sealed class NTAccount : IdentityReference
    {
        private readonly string _Name;
        internal const int MaximumAccountNameLength = 0x100;
        internal const int MaximumDomainNameLength = 0xff;

        public NTAccount(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (name.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_StringZeroLength"), "name");
            }
            if (name.Length > 0x200)
            {
                throw new ArgumentException(Environment.GetResourceString("IdentityReference_AccountNameTooLong"), "name");
            }
            this._Name = name;
        }

        public NTAccount(string domainName, string accountName)
        {
            if (accountName == null)
            {
                throw new ArgumentNullException("accountName");
            }
            if (accountName.Length == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_StringZeroLength"), "accountName");
            }
            if (accountName.Length > 0x100)
            {
                throw new ArgumentException(Environment.GetResourceString("IdentityReference_AccountNameTooLong"), "accountName");
            }
            if ((domainName != null) && (domainName.Length > 0xff))
            {
                throw new ArgumentException(Environment.GetResourceString("IdentityReference_DomainNameTooLong"), "domainName");
            }
            if ((domainName == null) || (domainName.Length == 0))
            {
                this._Name = accountName;
            }
            else
            {
                this._Name = domainName + @"\" + accountName;
            }
        }

        public override bool Equals(object o)
        {
            if (o == null)
            {
                return false;
            }
            NTAccount account = o as NTAccount;
            if (account == null)
            {
                return false;
            }
            return (this == account);
        }

        public override int GetHashCode()
        {
            return StringComparer.InvariantCultureIgnoreCase.GetHashCode(this._Name);
        }

        public override bool IsValidTargetType(Type targetType)
        {
            return ((targetType == typeof(SecurityIdentifier)) || (targetType == typeof(NTAccount)));
        }

        public static bool operator ==(NTAccount left, NTAccount right)
        {
            object obj2 = left;
            object obj3 = right;
            return (((obj2 == null) && (obj3 == null)) || (((obj2 != null) && (obj3 != null)) && left.ToString().Equals(right.ToString(), StringComparison.OrdinalIgnoreCase)));
        }

        public static bool operator !=(NTAccount left, NTAccount right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return this._Name;
        }

        [SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, ControlPrincipal=true)]
        public override IdentityReference Translate(Type targetType)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }
            if (targetType == typeof(NTAccount))
            {
                return this;
            }
            if (targetType != typeof(SecurityIdentifier))
            {
                throw new ArgumentException(Environment.GetResourceString("IdentityReference_MustBeIdentityReference"), "targetType");
            }
            return Translate(new IdentityReferenceCollection(1) { this }, targetType, 1)[0];
        }

        [SecurityCritical]
        internal static IdentityReferenceCollection Translate(IdentityReferenceCollection sourceAccounts, Type targetType, bool forceSuccess)
        {
            bool someFailed = false;
            IdentityReferenceCollection references = Translate(sourceAccounts, targetType, out someFailed);
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
        internal static IdentityReferenceCollection Translate(IdentityReferenceCollection sourceAccounts, Type targetType, out bool someFailed)
        {
            if (sourceAccounts == null)
            {
                throw new ArgumentNullException("sourceAccounts");
            }
            if (targetType != typeof(SecurityIdentifier))
            {
                throw new ArgumentException(Environment.GetResourceString("IdentityReference_MustBeIdentityReference"), "targetType");
            }
            return TranslateToSids(sourceAccounts, out someFailed);
        }

        [SecurityCritical]
        private static IdentityReferenceCollection TranslateToSids(IdentityReferenceCollection sourceAccounts, out bool someFailed)
        {
            IdentityReferenceCollection references2;
            if (sourceAccounts == null)
            {
                throw new ArgumentNullException("sourceAccounts");
            }
            if (sourceAccounts.Count == 0)
            {
                throw new ArgumentException(Environment.GetResourceString("Arg_EmptyCollection"), "sourceAccounts");
            }
            SafeLsaPolicyHandle invalidHandle = SafeLsaPolicyHandle.InvalidHandle;
            SafeLsaMemoryHandle referencedDomains = SafeLsaMemoryHandle.InvalidHandle;
            SafeLsaMemoryHandle sids = SafeLsaMemoryHandle.InvalidHandle;
            try
            {
                uint num2;
                Win32Native.UNICODE_STRING[] names = new Win32Native.UNICODE_STRING[sourceAccounts.Count];
                int index = 0;
                foreach (IdentityReference reference in sourceAccounts)
                {
                    NTAccount account = reference as NTAccount;
                    if (account == null)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_ImproperType"), "sourceAccounts");
                    }
                    names[index].Buffer = account.ToString();
                    if (((names[index].Buffer.Length * 2) + 2) > 0xffff)
                    {
                        throw new SystemException();
                    }
                    names[index].Length = (ushort) (names[index].Buffer.Length * 2);
                    names[index].MaximumLength = (ushort) (names[index].Length + 2);
                    index++;
                }
                invalidHandle = Win32.LsaOpenPolicy(null, PolicyRights.POLICY_LOOKUP_NAMES);
                someFailed = false;
                if (Win32.LsaLookupNames2Supported)
                {
                    num2 = Win32Native.LsaLookupNames2(invalidHandle, 0, sourceAccounts.Count, names, ref referencedDomains, ref sids);
                }
                else
                {
                    num2 = Win32Native.LsaLookupNames(invalidHandle, sourceAccounts.Count, names, ref referencedDomains, ref sids);
                }
                if ((num2 == 0xc0000017) || (num2 == 0xc000009a))
                {
                    throw new OutOfMemoryException();
                }
                if (num2 == 0xc0000022)
                {
                    throw new UnauthorizedAccessException();
                }
                if ((num2 == 0xc0000073) || (num2 == 0x107))
                {
                    someFailed = true;
                }
                else if (num2 != 0)
                {
                    int errorCode = Win32Native.LsaNtStatusToWinError((int) num2);
                    throw new SystemException(Win32Native.GetMessage(errorCode));
                }
                IdentityReferenceCollection references = new IdentityReferenceCollection(sourceAccounts.Count);
                switch (num2)
                {
                    case 0:
                    case 0x107:
                        if (Win32.LsaLookupNames2Supported)
                        {
                            sids.Initialize((uint) sourceAccounts.Count, (uint) Marshal.SizeOf(typeof(Win32Native.LSA_TRANSLATED_SID2)));
                            Win32.InitializeReferencedDomainsPointer(referencedDomains);
                            Win32Native.LSA_TRANSLATED_SID2[] array = new Win32Native.LSA_TRANSLATED_SID2[sourceAccounts.Count];
                            sids.ReadArray<Win32Native.LSA_TRANSLATED_SID2>(0L, array, 0, array.Length);
                            for (int i = 0; i < sourceAccounts.Count; i++)
                            {
                                Win32Native.LSA_TRANSLATED_SID2 lsa_translated_sid = array[i];
                                switch (lsa_translated_sid.Use)
                                {
                                    case 1:
                                    case 2:
                                    case 4:
                                    case 5:
                                    case 9:
                                    {
                                        references.Add(new SecurityIdentifier(lsa_translated_sid.Sid, true));
                                        continue;
                                    }
                                }
                                someFailed = true;
                                references.Add(sourceAccounts[i]);
                            }
                        }
                        else
                        {
                            sids.Initialize((uint) sourceAccounts.Count, (uint) Marshal.SizeOf(typeof(Win32Native.LSA_TRANSLATED_SID)));
                            Win32.InitializeReferencedDomainsPointer(referencedDomains);
                            Win32Native.LSA_REFERENCED_DOMAIN_LIST lsa_referenced_domain_list = referencedDomains.Read<Win32Native.LSA_REFERENCED_DOMAIN_LIST>(0L);
                            SecurityIdentifier[] identifierArray = new SecurityIdentifier[lsa_referenced_domain_list.Entries];
                            for (int j = 0; j < lsa_referenced_domain_list.Entries; j++)
                            {
                                Win32Native.LSA_TRUST_INFORMATION lsa_trust_information = (Win32Native.LSA_TRUST_INFORMATION) Marshal.PtrToStructure(new IntPtr(((long) lsa_referenced_domain_list.Domains) + (j * Marshal.SizeOf(typeof(Win32Native.LSA_TRUST_INFORMATION)))), typeof(Win32Native.LSA_TRUST_INFORMATION));
                                identifierArray[j] = new SecurityIdentifier(lsa_trust_information.Sid, true);
                            }
                            Win32Native.LSA_TRANSLATED_SID[] lsa_translated_sidArray2 = new Win32Native.LSA_TRANSLATED_SID[sourceAccounts.Count];
                            sids.ReadArray<Win32Native.LSA_TRANSLATED_SID>(0L, lsa_translated_sidArray2, 0, lsa_translated_sidArray2.Length);
                            for (int k = 0; k < sourceAccounts.Count; k++)
                            {
                                Win32Native.LSA_TRANSLATED_SID lsa_translated_sid2 = lsa_translated_sidArray2[k];
                                switch (lsa_translated_sid2.Use)
                                {
                                    case 1:
                                    case 2:
                                    case 4:
                                    case 5:
                                    case 9:
                                    {
                                        references.Add(new SecurityIdentifier(identifierArray[lsa_translated_sid2.DomainIndex], lsa_translated_sid2.Rid));
                                        continue;
                                    }
                                }
                                someFailed = true;
                                references.Add(sourceAccounts[k]);
                            }
                        }
                        break;

                    default:
                        for (int m = 0; m < sourceAccounts.Count; m++)
                        {
                            references.Add(sourceAccounts[m]);
                        }
                        break;
                }
                references2 = references;
            }
            finally
            {
                invalidHandle.Dispose();
                referencedDomains.Dispose();
                sids.Dispose();
            }
            return references2;
        }

        public override string Value
        {
            get
            {
                return this.ToString();
            }
        }
    }
}

