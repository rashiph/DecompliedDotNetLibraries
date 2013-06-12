namespace System.Security
{
    using System;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [Serializable]
    internal sealed class PermissionSetTriple
    {
        internal PermissionSet AssertSet;
        internal PermissionSet GrantSet;
        internal PermissionSet RefusedSet;
        private static PermissionToken s_urlToken;
        private static PermissionToken s_zoneToken;

        internal PermissionSetTriple()
        {
            this.Reset();
        }

        internal PermissionSetTriple(PermissionSetTriple triple)
        {
            this.AssertSet = triple.AssertSet;
            this.GrantSet = triple.GrantSet;
            this.RefusedSet = triple.RefusedSet;
        }

        [SecurityCritical]
        private static bool CheckAssert(PermissionSet pSet, CodeAccessPermission demand, PermissionToken permToken)
        {
            if (pSet != null)
            {
                pSet.CheckDecoded(demand, permToken);
                CodeAccessPermission asserted = (CodeAccessPermission) pSet.GetPermission(demand);
                try
                {
                    if (pSet.IsUnrestricted() || demand.CheckAssert(asserted))
                    {
                        return false;
                    }
                }
                catch (ArgumentException)
                {
                }
            }
            return true;
        }

        [SecurityCritical]
        private static bool CheckAssert(PermissionSet assertPset, PermissionSet demandSet, out PermissionSet newDemandSet)
        {
            newDemandSet = null;
            if (assertPset != null)
            {
                assertPset.CheckDecoded(demandSet);
                if (demandSet.CheckAssertion(assertPset))
                {
                    return false;
                }
                PermissionSet.RemoveAssertedPermissionSet(demandSet, assertPset, out newDemandSet);
            }
            return true;
        }

        [SecurityCritical]
        internal bool CheckDemand(CodeAccessPermission demand, PermissionToken permToken, RuntimeMethodHandleInternal rmh)
        {
            if (!CheckAssert(this.AssertSet, demand, permToken))
            {
                return false;
            }
            CodeAccessSecurityEngine.CheckHelper(this.GrantSet, this.RefusedSet, demand, permToken, rmh, null, SecurityAction.Demand, true);
            return true;
        }

        [SecurityCritical]
        internal bool CheckDemandNoThrow(CodeAccessPermission demand, PermissionToken permToken)
        {
            return CodeAccessSecurityEngine.CheckHelper(this.GrantSet, this.RefusedSet, demand, permToken, RuntimeMethodHandleInternal.EmptyHandle, null, SecurityAction.Demand, false);
        }

        [SecurityCritical]
        internal bool CheckFlags(ref int flags)
        {
            if (this.AssertSet != null)
            {
                int specialFlags = SecurityManager.GetSpecialFlags(this.AssertSet, null);
                if ((flags & specialFlags) != 0)
                {
                    flags &= ~specialFlags;
                }
            }
            return ((SecurityManager.GetSpecialFlags(this.GrantSet, this.RefusedSet) & flags) == flags);
        }

        [SecurityCritical]
        internal bool CheckSetDemand(PermissionSet demandSet, out PermissionSet alteredDemandset, RuntimeMethodHandleInternal rmh)
        {
            alteredDemandset = null;
            if (!CheckAssert(this.AssertSet, demandSet, out alteredDemandset))
            {
                return false;
            }
            if (alteredDemandset != null)
            {
                demandSet = alteredDemandset;
            }
            CodeAccessSecurityEngine.CheckSetHelper(this.GrantSet, this.RefusedSet, demandSet, rmh, null, SecurityAction.Demand, true);
            return true;
        }

        [SecurityCritical]
        internal bool CheckSetDemandNoThrow(PermissionSet demandSet)
        {
            return CodeAccessSecurityEngine.CheckSetHelper(this.GrantSet, this.RefusedSet, demandSet, RuntimeMethodHandleInternal.EmptyHandle, null, SecurityAction.Demand, false);
        }

        internal bool IsEmpty()
        {
            return (((this.AssertSet == null) && (this.GrantSet == null)) && (this.RefusedSet == null));
        }

        internal void Reset()
        {
            this.AssertSet = null;
            this.GrantSet = null;
            this.RefusedSet = null;
        }

        [SecurityCritical]
        internal bool Update(PermissionSetTriple psTriple, out PermissionSetTriple retTriple)
        {
            retTriple = null;
            if ((psTriple.AssertSet != null) && psTriple.AssertSet.IsUnrestricted())
            {
                return true;
            }
            retTriple = this.UpdateAssert(psTriple.AssertSet);
            this.UpdateGrant(psTriple.GrantSet);
            this.UpdateRefused(psTriple.RefusedSet);
            return false;
        }

        [SecurityCritical]
        internal PermissionSetTriple UpdateAssert(PermissionSet in_a)
        {
            PermissionSetTriple triple = null;
            if (in_a != null)
            {
                PermissionSet set;
                bool flag;
                if (in_a.IsSubsetOf(this.AssertSet))
                {
                    return null;
                }
                if (this.GrantSet != null)
                {
                    set = in_a.Intersect(this.GrantSet);
                }
                else
                {
                    this.GrantSet = new PermissionSet(true);
                    set = in_a.Copy();
                }
                set = PermissionSet.RemoveRefusedPermissionSet(set, this.RefusedSet, out flag);
                if (!flag)
                {
                    flag = PermissionSet.IsIntersectingAssertedPermissions(set, this.AssertSet);
                }
                if (flag)
                {
                    triple = new PermissionSetTriple(this);
                    this.Reset();
                    this.GrantSet = triple.GrantSet.Copy();
                }
                if (this.AssertSet == null)
                {
                    this.AssertSet = set;
                    return triple;
                }
                this.AssertSet.InplaceUnion(set);
            }
            return triple;
        }

        [SecurityCritical]
        internal void UpdateGrant(PermissionSet in_g)
        {
            if (in_g != null)
            {
                if (this.GrantSet == null)
                {
                    this.GrantSet = in_g.Copy();
                }
                else
                {
                    this.GrantSet.InplaceIntersect(in_g);
                }
            }
        }

        [SecurityCritical]
        internal void UpdateGrant(PermissionSet in_g, out ZoneIdentityPermission z, out UrlIdentityPermission u)
        {
            z = null;
            u = null;
            if (in_g != null)
            {
                if (this.GrantSet == null)
                {
                    this.GrantSet = in_g.Copy();
                }
                else
                {
                    this.GrantSet.InplaceIntersect(in_g);
                }
                z = (ZoneIdentityPermission) in_g.GetPermission(this.ZoneToken);
                u = (UrlIdentityPermission) in_g.GetPermission(this.UrlToken);
            }
        }

        internal void UpdateRefused(PermissionSet in_r)
        {
            if (in_r != null)
            {
                if (this.RefusedSet == null)
                {
                    this.RefusedSet = in_r.Copy();
                }
                else
                {
                    this.RefusedSet.InplaceUnion(in_r);
                }
            }
        }

        private PermissionToken UrlToken
        {
            [SecurityCritical]
            get
            {
                if (s_urlToken == null)
                {
                    s_urlToken = PermissionToken.GetToken(typeof(UrlIdentityPermission));
                }
                return s_urlToken;
            }
        }

        private PermissionToken ZoneToken
        {
            [SecurityCritical]
            get
            {
                if (s_zoneToken == null)
                {
                    s_zoneToken = PermissionToken.GetToken(typeof(ZoneIdentityPermission));
                }
                return s_zoneToken;
            }
        }
    }
}

