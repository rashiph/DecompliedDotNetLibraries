namespace System.Security
{
    using Microsoft.Win32.SafeHandles;
    using System;
    using System.Globalization;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    [Serializable]
    internal sealed class FrameSecurityDescriptor
    {
        private bool m_assertAllPossible;
        private bool m_AssertFT;
        private PermissionSet m_assertions;
        [NonSerialized, SecurityCritical]
        private SafeTokenHandle m_callerToken;
        private PermissionSet m_DeclarativeAssertions;
        private PermissionSet m_DeclarativeDenials;
        private PermissionSet m_DeclarativeRestrictions;
        private bool m_declSecComputed;
        private PermissionSet m_denials;
        [NonSerialized, SecurityCritical]
        private SafeTokenHandle m_impToken;
        private PermissionSet m_restriction;

        internal FrameSecurityDescriptor()
        {
        }

        [SecurityCritical]
        internal bool CheckDemand(CodeAccessPermission demand, PermissionToken permToken, RuntimeMethodHandleInternal rmh)
        {
            bool flag = this.CheckDemand2(demand, permToken, rmh, false);
            if (flag)
            {
                flag = this.CheckDemand2(demand, permToken, rmh, true);
            }
            return flag;
        }

        [SecurityCritical]
        internal bool CheckDemand2(CodeAccessPermission demand, PermissionToken permToken, RuntimeMethodHandleInternal rmh, bool fDeclarative)
        {
            if (this.GetPermitOnly(fDeclarative) != null)
            {
                this.GetPermitOnly(fDeclarative).CheckDecoded(demand, permToken);
            }
            if (this.GetDenials(fDeclarative) != null)
            {
                this.GetDenials(fDeclarative).CheckDecoded(demand, permToken);
            }
            if (this.GetAssertions(fDeclarative) != null)
            {
                this.GetAssertions(fDeclarative).CheckDecoded(demand, permToken);
            }
            bool flag = SecurityManager._SetThreadSecurity(false);
            try
            {
                PermissionSet permitOnly = this.GetPermitOnly(fDeclarative);
                if (permitOnly != null)
                {
                    CodeAccessPermission permitted = (CodeAccessPermission) permitOnly.GetPermission(demand);
                    if (permitted == null)
                    {
                        if (!permitOnly.IsUnrestricted())
                        {
                            throw new SecurityException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Security_Generic"), new object[] { demand.GetType().AssemblyQualifiedName }), null, permitOnly, SecurityRuntime.GetMethodInfo(rmh), demand, demand);
                        }
                    }
                    else
                    {
                        bool flag2 = true;
                        try
                        {
                            flag2 = !demand.CheckPermitOnly(permitted);
                        }
                        catch (ArgumentException)
                        {
                        }
                        if (flag2)
                        {
                            throw new SecurityException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Security_Generic"), new object[] { demand.GetType().AssemblyQualifiedName }), null, permitOnly, SecurityRuntime.GetMethodInfo(rmh), demand, demand);
                        }
                    }
                }
                permitOnly = this.GetDenials(fDeclarative);
                if (permitOnly != null)
                {
                    CodeAccessPermission permission = (CodeAccessPermission) permitOnly.GetPermission(demand);
                    if (permitOnly.IsUnrestricted())
                    {
                        throw new SecurityException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Security_Generic"), new object[] { demand.GetType().AssemblyQualifiedName }), permitOnly, null, SecurityRuntime.GetMethodInfo(rmh), demand, demand);
                    }
                    bool flag3 = true;
                    try
                    {
                        flag3 = !demand.CheckDeny(permission);
                    }
                    catch (ArgumentException)
                    {
                    }
                    if (flag3)
                    {
                        throw new SecurityException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Security_Generic"), new object[] { demand.GetType().AssemblyQualifiedName }), permitOnly, null, SecurityRuntime.GetMethodInfo(rmh), demand, demand);
                    }
                }
                if (this.GetAssertAllPossible())
                {
                    return false;
                }
                permitOnly = this.GetAssertions(fDeclarative);
                if (permitOnly != null)
                {
                    CodeAccessPermission asserted = (CodeAccessPermission) permitOnly.GetPermission(demand);
                    try
                    {
                        if (permitOnly.IsUnrestricted() || demand.CheckAssert(asserted))
                        {
                            return false;
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    SecurityManager._SetThreadSecurity(true);
                }
            }
            return true;
        }

        [SecurityCritical]
        internal bool CheckSetDemand(PermissionSet demandSet, out PermissionSet alteredDemandSet, RuntimeMethodHandleInternal rmh)
        {
            PermissionSet set = null;
            PermissionSet set2 = null;
            bool flag = this.CheckSetDemand2(demandSet, out set, rmh, false);
            if (set != null)
            {
                demandSet = set;
            }
            if (flag)
            {
                flag = this.CheckSetDemand2(demandSet, out set2, rmh, true);
            }
            if (set2 != null)
            {
                alteredDemandSet = set2;
                return flag;
            }
            if (set != null)
            {
                alteredDemandSet = set;
                return flag;
            }
            alteredDemandSet = null;
            return flag;
        }

        [SecurityCritical]
        internal bool CheckSetDemand2(PermissionSet demandSet, out PermissionSet alteredDemandSet, RuntimeMethodHandleInternal rmh, bool fDeclarative)
        {
            alteredDemandSet = null;
            if ((demandSet == null) || demandSet.IsEmpty())
            {
                return false;
            }
            if (this.GetPermitOnly(fDeclarative) != null)
            {
                this.GetPermitOnly(fDeclarative).CheckDecoded(demandSet);
            }
            if (this.GetDenials(fDeclarative) != null)
            {
                this.GetDenials(fDeclarative).CheckDecoded(demandSet);
            }
            if (this.GetAssertions(fDeclarative) != null)
            {
                this.GetAssertions(fDeclarative).CheckDecoded(demandSet);
            }
            bool flag = SecurityManager._SetThreadSecurity(false);
            try
            {
                PermissionSet permitOnly = this.GetPermitOnly(fDeclarative);
                if (permitOnly != null)
                {
                    IPermission firstPermThatFailed = null;
                    bool flag2 = true;
                    try
                    {
                        flag2 = !demandSet.CheckPermitOnly(permitOnly, out firstPermThatFailed);
                    }
                    catch (ArgumentException)
                    {
                    }
                    if (flag2)
                    {
                        throw new SecurityException(Environment.GetResourceString("Security_GenericNoType"), null, permitOnly, SecurityRuntime.GetMethodInfo(rmh), demandSet, firstPermThatFailed);
                    }
                }
                permitOnly = this.GetDenials(fDeclarative);
                if (permitOnly != null)
                {
                    IPermission permission2 = null;
                    bool flag3 = true;
                    try
                    {
                        flag3 = !demandSet.CheckDeny(permitOnly, out permission2);
                    }
                    catch (ArgumentException)
                    {
                    }
                    if (flag3)
                    {
                        throw new SecurityException(Environment.GetResourceString("Security_GenericNoType"), permitOnly, null, SecurityRuntime.GetMethodInfo(rmh), demandSet, permission2);
                    }
                }
                if (this.GetAssertAllPossible())
                {
                    return false;
                }
                permitOnly = this.GetAssertions(fDeclarative);
                if (permitOnly != null)
                {
                    if (demandSet.CheckAssertion(permitOnly))
                    {
                        return false;
                    }
                    if (!permitOnly.IsUnrestricted())
                    {
                        PermissionSet.RemoveAssertedPermissionSet(demandSet, permitOnly, out alteredDemandSet);
                    }
                }
            }
            finally
            {
                if (flag)
                {
                    SecurityManager._SetThreadSecurity(true);
                }
            }
            return true;
        }

        private PermissionSet CreateSingletonSet(IPermission perm)
        {
            PermissionSet set = new PermissionSet(false);
            set.AddPermission(perm.Copy());
            return set;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void DecrementAssertCount();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void DecrementOverridesCount();
        internal bool GetAssertAllPossible()
        {
            return this.m_assertAllPossible;
        }

        internal PermissionSet GetAssertions(bool fDeclarative)
        {
            if (!fDeclarative)
            {
                return this.m_assertions;
            }
            return this.m_DeclarativeAssertions;
        }

        internal PermissionSet GetDenials(bool fDeclarative)
        {
            if (!fDeclarative)
            {
                return this.m_denials;
            }
            return this.m_DeclarativeDenials;
        }

        internal PermissionSet GetPermitOnly(bool fDeclarative)
        {
            if (!fDeclarative)
            {
                return this.m_restriction;
            }
            return this.m_DeclarativeRestrictions;
        }

        internal bool HasImperativeAsserts()
        {
            return (this.m_assertions != null);
        }

        internal bool HasImperativeDenials()
        {
            return (this.m_denials != null);
        }

        internal bool HasImperativeRestrictions()
        {
            return (this.m_restriction != null);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void IncrementAssertCount();
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void IncrementOverridesCount();
        [SecurityCritical]
        internal void RevertAll()
        {
            this.RevertAssert();
            this.RevertAssertAllPossible();
            this.RevertDeny();
            this.RevertPermitOnly();
        }

        [SecurityCritical]
        internal void RevertAssert()
        {
            if (this.m_assertions != null)
            {
                this.m_assertions = null;
                DecrementAssertCount();
            }
            if (this.m_DeclarativeAssertions != null)
            {
                this.m_AssertFT = this.m_DeclarativeAssertions.IsUnrestricted();
            }
            else
            {
                this.m_AssertFT = false;
            }
        }

        [SecurityCritical]
        internal void RevertAssertAllPossible()
        {
            if (this.m_assertAllPossible)
            {
                this.m_assertAllPossible = false;
                DecrementAssertCount();
            }
        }

        [SecurityCritical]
        internal void RevertDeny()
        {
            if (this.HasImperativeDenials())
            {
                DecrementOverridesCount();
                this.m_denials = null;
            }
        }

        [SecurityCritical]
        internal void RevertPermitOnly()
        {
            if (this.HasImperativeRestrictions())
            {
                DecrementOverridesCount();
                this.m_restriction = null;
            }
        }

        [SecurityCritical]
        internal void SetAssert(IPermission perm)
        {
            this.m_assertions = this.CreateSingletonSet(perm);
            IncrementAssertCount();
        }

        [SecurityCritical]
        internal void SetAssert(PermissionSet permSet)
        {
            this.m_assertions = permSet.Copy();
            this.m_AssertFT = this.m_AssertFT || this.m_assertions.IsUnrestricted();
            IncrementAssertCount();
        }

        [SecurityCritical]
        internal void SetAssertAllPossible()
        {
            this.m_assertAllPossible = true;
            IncrementAssertCount();
        }

        [SecurityCritical]
        internal void SetDeny(IPermission perm)
        {
            this.m_denials = this.CreateSingletonSet(perm);
            IncrementOverridesCount();
        }

        [SecurityCritical]
        internal void SetDeny(PermissionSet permSet)
        {
            this.m_denials = permSet.Copy();
            IncrementOverridesCount();
        }

        [SecurityCritical]
        internal void SetPermitOnly(IPermission perm)
        {
            this.m_restriction = this.CreateSingletonSet(perm);
            IncrementOverridesCount();
        }

        [SecurityCritical]
        internal void SetPermitOnly(PermissionSet permSet)
        {
            this.m_restriction = permSet.Copy();
            IncrementOverridesCount();
        }

        [SecurityCritical]
        internal void SetTokenHandles(SafeTokenHandle callerToken, SafeTokenHandle impToken)
        {
            this.m_callerToken = callerToken;
            this.m_impToken = impToken;
        }
    }
}

