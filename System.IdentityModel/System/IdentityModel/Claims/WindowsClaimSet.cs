namespace System.IdentityModel.Claims
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IdentityModel;
    using System.IdentityModel.Policy;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.Threading;

    public class WindowsClaimSet : ClaimSet, IIdentityInfo, IDisposable
    {
        private string authenticationType;
        private IList<Claim> claims;
        internal const bool DefaultIncludeWindowsGroups = true;
        private bool disposed;
        private DateTime expirationTime;
        private GroupSidClaimCollection groups;
        private bool includeWindowsGroups;
        private System.Security.Principal.WindowsIdentity windowsIdentity;

        private WindowsClaimSet(WindowsClaimSet from) : this(from.WindowsIdentity, from.authenticationType, from.includeWindowsGroups, from.expirationTime, true)
        {
        }

        public WindowsClaimSet(System.Security.Principal.WindowsIdentity windowsIdentity) : this(windowsIdentity, true)
        {
        }

        public WindowsClaimSet(System.Security.Principal.WindowsIdentity windowsIdentity, bool includeWindowsGroups) : this(windowsIdentity, includeWindowsGroups, DateTime.UtcNow.AddHours(10.0))
        {
        }

        public WindowsClaimSet(System.Security.Principal.WindowsIdentity windowsIdentity, DateTime expirationTime) : this(windowsIdentity, true, expirationTime)
        {
        }

        public WindowsClaimSet(System.Security.Principal.WindowsIdentity windowsIdentity, bool includeWindowsGroups, DateTime expirationTime) : this(windowsIdentity, null, includeWindowsGroups, expirationTime, true)
        {
        }

        internal WindowsClaimSet(System.Security.Principal.WindowsIdentity windowsIdentity, string authenticationType, bool includeWindowsGroups, bool clone) : this(windowsIdentity, authenticationType, includeWindowsGroups, DateTime.UtcNow.AddHours(10.0), clone)
        {
        }

        public WindowsClaimSet(System.Security.Principal.WindowsIdentity windowsIdentity, string authenticationType, bool includeWindowsGroups, DateTime expirationTime) : this(windowsIdentity, authenticationType, includeWindowsGroups, expirationTime, true)
        {
        }

        internal WindowsClaimSet(System.Security.Principal.WindowsIdentity windowsIdentity, string authenticationType, bool includeWindowsGroups, DateTime expirationTime, bool clone)
        {
            if (windowsIdentity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("windowsIdentity");
            }
            this.windowsIdentity = clone ? System.IdentityModel.SecurityUtils.CloneWindowsIdentityIfNecessary(windowsIdentity, authenticationType) : windowsIdentity;
            this.includeWindowsGroups = includeWindowsGroups;
            this.expirationTime = expirationTime;
            this.authenticationType = authenticationType;
        }

        internal WindowsClaimSet Clone()
        {
            this.ThrowIfDisposed();
            return new WindowsClaimSet(this);
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.windowsIdentity.Dispose();
            }
        }

        private void EnsureClaims()
        {
            if (this.claims == null)
            {
                this.claims = this.InitializeClaimsCore();
            }
        }

        public override IEnumerable<Claim> FindClaims(string claimType, string right)
        {
            this.ThrowIfDisposed();
            if (SupportedClaimType(claimType) && ClaimSet.SupportedRight(right))
            {
                if ((this.claims != null) || (!(ClaimTypes.Sid == claimType) && !(ClaimTypes.DenyOnlySid == claimType)))
                {
                    this.EnsureClaims();
                    bool iteratorVariable3 = claimType == null;
                    bool iteratorVariable4 = right == null;
                    for (int i = 0; i < this.claims.Count; i++)
                    {
                        Claim iteratorVariable6 = this.claims[i];
                        if (((iteratorVariable6 != null) && (iteratorVariable3 || (claimType == iteratorVariable6.ClaimType))) && (iteratorVariable4 || (right == iteratorVariable6.Right)))
                        {
                            yield return iteratorVariable6;
                        }
                    }
                }
                else
                {
                    Claim iteratorVariable0;
                    if ((ClaimTypes.Sid == claimType) && ((right == null) || (Rights.Identity == right)))
                    {
                        yield return new Claim(ClaimTypes.Sid, this.windowsIdentity.User, Rights.Identity);
                    }
                    if (((right == null) || (Rights.PossessProperty == right)) && (TryCreateWindowsSidClaim(this.windowsIdentity, out iteratorVariable0) && (claimType == iteratorVariable0.ClaimType)))
                    {
                        yield return iteratorVariable0;
                    }
                    if (this.includeWindowsGroups && ((right == null) || (Rights.PossessProperty == right)))
                    {
                        for (int j = 0; j < this.Groups.Count; j++)
                        {
                            Claim iteratorVariable2 = this.Groups[j];
                            if (claimType == iteratorVariable2.ClaimType)
                            {
                                yield return iteratorVariable2;
                            }
                        }
                    }
                }
            }
        }

        public override IEnumerator<Claim> GetEnumerator()
        {
            this.ThrowIfDisposed();
            this.EnsureClaims();
            return this.claims.GetEnumerator();
        }

        private static SafeHGlobalHandle GetTokenInformation(IntPtr tokenHandle, System.IdentityModel.TokenInformationClass tokenInformationClass, out uint dwLength)
        {
            SafeHGlobalHandle invalidHandle = SafeHGlobalHandle.InvalidHandle;
            dwLength = (uint) Marshal.SizeOf(typeof(uint));
            bool flag = System.IdentityModel.NativeMethods.GetTokenInformation(tokenHandle, (uint) tokenInformationClass, invalidHandle, 0, out dwLength);
            int error = Marshal.GetLastWin32Error();
            switch (error)
            {
                case (0x18 && 0x7a):
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
            }
            invalidHandle = SafeHGlobalHandle.AllocHGlobal(dwLength);
            flag = System.IdentityModel.NativeMethods.GetTokenInformation(tokenHandle, (uint) tokenInformationClass, invalidHandle, dwLength, out dwLength);
            error = Marshal.GetLastWin32Error();
            if (flag)
            {
                return invalidHandle;
            }
            invalidHandle.Close();
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new Win32Exception(error));
        }

        private IList<Claim> InitializeClaimsCore()
        {
            Claim claim;
            if (this.windowsIdentity.Token == IntPtr.Zero)
            {
                return new List<Claim>();
            }
            List<Claim> list = new List<Claim>(3) {
                new Claim(ClaimTypes.Sid, this.windowsIdentity.User, Rights.Identity)
            };
            if (TryCreateWindowsSidClaim(this.windowsIdentity, out claim))
            {
                list.Add(claim);
            }
            list.Add(Claim.CreateNameClaim(this.windowsIdentity.Name));
            if (this.includeWindowsGroups)
            {
                list.AddRange(this.Groups);
            }
            return list;
        }

        private static bool SupportedClaimType(string claimType)
        {
            if (((claimType != null) && !(ClaimTypes.Sid == claimType)) && !(ClaimTypes.DenyOnlySid == claimType))
            {
                return (ClaimTypes.Name == claimType);
            }
            return true;
        }

        private void ThrowIfDisposed()
        {
            if (this.disposed)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ObjectDisposedException(base.GetType().FullName));
            }
        }

        public override string ToString()
        {
            if (!this.disposed)
            {
                return System.IdentityModel.SecurityUtils.ClaimSetToString(this);
            }
            return base.ToString();
        }

        private static bool TryCreateWindowsSidClaim(System.Security.Principal.WindowsIdentity windowsIdentity, out Claim claim)
        {
            SafeHGlobalHandle invalidHandle = SafeHGlobalHandle.InvalidHandle;
            try
            {
                uint num;
                invalidHandle = GetTokenInformation(windowsIdentity.Token, System.IdentityModel.TokenInformationClass.TokenUser, out num);
                SID_AND_ATTRIBUTES sid_and_attributes = (SID_AND_ATTRIBUTES) Marshal.PtrToStructure(invalidHandle.DangerousGetHandle(), typeof(SID_AND_ATTRIBUTES));
                uint num2 = 0x10;
                if (sid_and_attributes.Attributes == 0)
                {
                    claim = Claim.CreateWindowsSidClaim(new SecurityIdentifier(sid_and_attributes.Sid));
                    return true;
                }
                if ((sid_and_attributes.Attributes & num2) == 0x10)
                {
                    claim = Claim.CreateDenyOnlyWindowsSidClaim(new SecurityIdentifier(sid_and_attributes.Sid));
                    return true;
                }
            }
            finally
            {
                invalidHandle.Close();
            }
            claim = null;
            return false;
        }

        public override int Count
        {
            get
            {
                this.ThrowIfDisposed();
                this.EnsureClaims();
                return this.claims.Count;
            }
        }

        public DateTime ExpirationTime
        {
            get
            {
                return this.expirationTime;
            }
        }

        private GroupSidClaimCollection Groups
        {
            get
            {
                if (this.groups == null)
                {
                    this.groups = new GroupSidClaimCollection(this.windowsIdentity);
                }
                return this.groups;
            }
        }

        public override ClaimSet Issuer
        {
            get
            {
                return ClaimSet.Windows;
            }
        }

        public override Claim this[int index]
        {
            get
            {
                this.ThrowIfDisposed();
                this.EnsureClaims();
                return this.claims[index];
            }
        }

        IIdentity IIdentityInfo.Identity
        {
            get
            {
                this.ThrowIfDisposed();
                return this.windowsIdentity;
            }
        }

        public System.Security.Principal.WindowsIdentity WindowsIdentity
        {
            get
            {
                this.ThrowIfDisposed();
                return this.windowsIdentity;
            }
        }


        private class GroupSidClaimCollection : Collection<Claim>
        {
            public GroupSidClaimCollection(WindowsIdentity windowsIdentity)
            {
                if (windowsIdentity.Token != IntPtr.Zero)
                {
                    SafeHGlobalHandle invalidHandle = SafeHGlobalHandle.InvalidHandle;
                    try
                    {
                        uint num;
                        invalidHandle = WindowsClaimSet.GetTokenInformation(windowsIdentity.Token, System.IdentityModel.TokenInformationClass.TokenGroups, out num);
                        int num2 = Marshal.ReadInt32(invalidHandle.DangerousGetHandle());
                        IntPtr ptr = new IntPtr(((long) invalidHandle.DangerousGetHandle()) + ((long) Marshal.OffsetOf(typeof(TOKEN_GROUPS), "Groups")));
                        for (int i = 0; i < num2; i++)
                        {
                            SID_AND_ATTRIBUTES sid_and_attributes = (SID_AND_ATTRIBUTES) Marshal.PtrToStructure(ptr, typeof(SID_AND_ATTRIBUTES));
                            uint num4 = 0xc0000014;
                            if ((sid_and_attributes.Attributes & num4) == 4)
                            {
                                base.Add(Claim.CreateWindowsSidClaim(new SecurityIdentifier(sid_and_attributes.Sid)));
                            }
                            else if ((sid_and_attributes.Attributes & num4) == 0x10)
                            {
                                base.Add(Claim.CreateDenyOnlyWindowsSidClaim(new SecurityIdentifier(sid_and_attributes.Sid)));
                            }
                            ptr = new IntPtr(((long) ptr) + SID_AND_ATTRIBUTES.SizeOf);
                        }
                    }
                    finally
                    {
                        invalidHandle.Close();
                    }
                }
            }
        }
    }
}

