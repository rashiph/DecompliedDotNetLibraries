namespace System.Security
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Threading;

    [Serializable]
    internal sealed class PermissionListSet
    {
        private PermissionSetTriple m_firstPermSetTriple;
        private ArrayList m_originList;
        private ArrayList m_permSetTriples;
        private ArrayList m_zoneList;

        internal PermissionListSet()
        {
        }

        private void AppendZoneOrigin(ZoneIdentityPermission z, UrlIdentityPermission u)
        {
            if (z != null)
            {
                if (this.m_zoneList == null)
                {
                    this.m_zoneList = new ArrayList();
                }
                z.AppendZones(this.m_zoneList);
            }
            if (u != null)
            {
                if (this.m_originList == null)
                {
                    this.m_originList = new ArrayList();
                }
                u.AppendOrigin(this.m_originList);
            }
        }

        [SecurityCritical]
        internal bool CheckDemand(CodeAccessPermission demand, PermissionToken permToken, RuntimeMethodHandleInternal rmh)
        {
            bool flag = true;
            if (this.m_permSetTriples != null)
            {
                for (int i = 0; (i < this.m_permSetTriples.Count) && flag; i++)
                {
                    flag = ((PermissionSetTriple) this.m_permSetTriples[i]).CheckDemand(demand, permToken, rmh);
                }
            }
            else if (this.m_firstPermSetTriple != null)
            {
                flag = this.m_firstPermSetTriple.CheckDemand(demand, permToken, rmh);
            }
            return false;
        }

        [SecurityCritical]
        internal bool CheckDemandNoThrow(CodeAccessPermission demand)
        {
            PermissionToken permToken = null;
            if (demand != null)
            {
                permToken = PermissionToken.GetToken(demand);
            }
            return this.m_firstPermSetTriple.CheckDemandNoThrow(demand, permToken);
        }

        [SecurityCritical]
        private bool CheckFlags(int flags)
        {
            bool flag = true;
            if (this.m_permSetTriples != null)
            {
                for (int i = 0; ((i < this.m_permSetTriples.Count) && flag) && (flags != 0); i++)
                {
                    flag &= ((PermissionSetTriple) this.m_permSetTriples[i]).CheckFlags(ref flags);
                }
                return flag;
            }
            if (this.m_firstPermSetTriple != null)
            {
                flag = this.m_firstPermSetTriple.CheckFlags(ref flags);
            }
            return flag;
        }

        [SecurityCritical]
        internal bool CheckSetDemand(PermissionSet pset, RuntimeMethodHandleInternal rmh)
        {
            PermissionSet set2;
            bool flag = true;
            PermissionSet demandSet = pset;
            if (this.m_permSetTriples != null)
            {
                for (int i = 0; (i < this.m_permSetTriples.Count) && flag; i++)
                {
                    flag = ((PermissionSetTriple) this.m_permSetTriples[i]).CheckSetDemand(demandSet, out set2, rmh);
                    if (set2 != null)
                    {
                        demandSet = set2;
                    }
                }
            }
            else if (this.m_firstPermSetTriple != null)
            {
                flag = this.m_firstPermSetTriple.CheckSetDemand(demandSet, out set2, rmh);
            }
            return false;
        }

        [SecurityCritical]
        internal bool CheckSetDemandNoThrow(PermissionSet pSet)
        {
            return this.m_firstPermSetTriple.CheckSetDemandNoThrow(pSet);
        }

        [SecurityCritical]
        internal static PermissionListSet CreateCompressedState(IntPtr unmanagedDCS, out bool bHaltConstruction)
        {
            PermissionSet set2;
            PermissionSet set3;
            PermissionListSet set = new PermissionListSet();
            PermissionSetTriple currentTriple = new PermissionSetTriple();
            int descCount = DomainCompressedStack.GetDescCount(unmanagedDCS);
            bHaltConstruction = false;
            for (int i = 0; (i < descCount) && !bHaltConstruction; i++)
            {
                FrameSecurityDescriptor descriptor;
                Assembly assembly;
                if (DomainCompressedStack.GetDescriptorInfo(unmanagedDCS, i, out set2, out set3, out assembly, out descriptor))
                {
                    bHaltConstruction = set.Update(currentTriple, descriptor);
                }
                else
                {
                    set.Update(currentTriple, set2, set3);
                }
            }
            if (!bHaltConstruction && !DomainCompressedStack.IgnoreDomain(unmanagedDCS))
            {
                DomainCompressedStack.GetDomainPermissionSets(unmanagedDCS, out set2, out set3);
                set.Update(currentTriple, set2, set3);
            }
            set.Terminate(currentTriple);
            return set;
        }

        [SecurityCritical, ComVisible(true)]
        internal static PermissionListSet CreateCompressedState(CompressedStack cs, CompressedStack innerCS)
        {
            bool constructionHalted = false;
            if (cs.CompressedStackHandle == null)
            {
                return null;
            }
            PermissionListSet set = new PermissionListSet();
            PermissionSetTriple currentTriple = new PermissionSetTriple();
            for (int i = CompressedStack.GetDCSCount(cs.CompressedStackHandle) - 1; (i >= 0) && !constructionHalted; i--)
            {
                DomainCompressedStack domainCompressedStack = CompressedStack.GetDomainCompressedStack(cs.CompressedStackHandle, i);
                if (domainCompressedStack != null)
                {
                    if (domainCompressedStack.PLS == null)
                    {
                        throw new SecurityException(string.Format(CultureInfo.InvariantCulture, Environment.GetResourceString("Security_Generic"), new object[0]));
                    }
                    set.UpdateZoneAndOrigin(domainCompressedStack.PLS);
                    set.Update(currentTriple, domainCompressedStack.PLS);
                    constructionHalted = domainCompressedStack.ConstructionHalted;
                }
            }
            if (!constructionHalted)
            {
                PermissionListSet pls = null;
                if (innerCS != null)
                {
                    innerCS.CompleteConstruction(null);
                    pls = innerCS.PLS;
                }
                set.Terminate(currentTriple, pls);
                return set;
            }
            set.Terminate(currentTriple);
            return set;
        }

        [SecurityCritical]
        internal static PermissionListSet CreateCompressedState_HG()
        {
            PermissionListSet hgPLS = new PermissionListSet();
            CompressedStack.GetHomogeneousPLS(hgPLS);
            return hgPLS;
        }

        [SecurityCritical]
        internal void DemandFlagsOrGrantSet(int flags, PermissionSet grantSet)
        {
            if (!this.CheckFlags(flags))
            {
                this.CheckSetDemand(grantSet, RuntimeMethodHandleInternal.EmptyHandle);
            }
        }

        private void EnsureTriplesListCreated()
        {
            if (this.m_permSetTriples == null)
            {
                this.m_permSetTriples = new ArrayList();
                if (this.m_firstPermSetTriple != null)
                {
                    this.m_permSetTriples.Add(this.m_firstPermSetTriple);
                    this.m_firstPermSetTriple = null;
                }
            }
        }

        internal void GetZoneAndOrigin(ArrayList zoneList, ArrayList originList, PermissionToken zoneToken, PermissionToken originToken)
        {
            if (this.m_zoneList != null)
            {
                zoneList.AddRange(this.m_zoneList);
            }
            if (this.m_originList != null)
            {
                originList.AddRange(this.m_originList);
            }
        }

        private void Terminate(PermissionSetTriple currentTriple)
        {
            this.UpdateTripleListAndCreateNewTriple(currentTriple, null);
        }

        [SecurityCritical]
        private void Terminate(PermissionSetTriple currentTriple, PermissionListSet pls)
        {
            this.UpdateZoneAndOrigin(pls);
            this.UpdatePermissions(currentTriple, pls);
            this.UpdateTripleListAndCreateNewTriple(currentTriple, null);
        }

        [SecurityCritical]
        private void Update(PermissionSet in_g)
        {
            if (this.m_firstPermSetTriple == null)
            {
                this.m_firstPermSetTriple = new PermissionSetTriple();
            }
            this.Update(this.m_firstPermSetTriple, in_g, null);
        }

        [SecurityCritical]
        private bool Update(PermissionSetTriple currentTriple, FrameSecurityDescriptor fsd)
        {
            bool flag = this.Update2(currentTriple, fsd, false);
            if (!flag)
            {
                flag = this.Update2(currentTriple, fsd, true);
            }
            return flag;
        }

        [SecurityCritical]
        private bool Update(PermissionSetTriple currentTriple, PermissionListSet pls)
        {
            this.UpdateZoneAndOrigin(pls);
            return this.UpdatePermissions(currentTriple, pls);
        }

        [SecurityCritical]
        private void Update(PermissionSetTriple currentTriple, PermissionSet in_g, PermissionSet in_r)
        {
            ZoneIdentityPermission permission;
            UrlIdentityPermission permission2;
            currentTriple.UpdateGrant(in_g, out permission, out permission2);
            currentTriple.UpdateRefused(in_r);
            this.AppendZoneOrigin(permission, permission2);
        }

        [SecurityCritical]
        private bool Update2(PermissionSetTriple currentTriple, FrameSecurityDescriptor fsd, bool fDeclarative)
        {
            PermissionSet denials = fsd.GetDenials(fDeclarative);
            if (denials != null)
            {
                currentTriple.UpdateRefused(denials);
            }
            PermissionSet permitOnly = fsd.GetPermitOnly(fDeclarative);
            if (permitOnly != null)
            {
                currentTriple.UpdateGrant(permitOnly);
            }
            if (fsd.GetAssertAllPossible())
            {
                if (currentTriple.GrantSet == null)
                {
                    currentTriple.GrantSet = PermissionSet.s_fullTrust;
                }
                return true;
            }
            PermissionSet assertions = fsd.GetAssertions(fDeclarative);
            if (assertions != null)
            {
                if (assertions.IsUnrestricted())
                {
                    if (currentTriple.GrantSet == null)
                    {
                        currentTriple.GrantSet = PermissionSet.s_fullTrust;
                    }
                    return true;
                }
                PermissionSetTriple triple = currentTriple.UpdateAssert(assertions);
                if (triple != null)
                {
                    this.EnsureTriplesListCreated();
                    this.m_permSetTriples.Add(triple);
                }
            }
            return false;
        }

        private static void UpdateArrayList(ArrayList current, ArrayList newList)
        {
            if (newList != null)
            {
                for (int i = 0; i < newList.Count; i++)
                {
                    if (!current.Contains(newList[i]))
                    {
                        current.Add(newList[i]);
                    }
                }
            }
        }

        [SecurityCritical]
        internal void UpdateDomainPLS(PermissionListSet adPLS)
        {
            if ((adPLS != null) && (adPLS.m_firstPermSetTriple != null))
            {
                this.UpdateDomainPLS(adPLS.m_firstPermSetTriple.GrantSet, adPLS.m_firstPermSetTriple.RefusedSet);
            }
        }

        [SecurityCritical]
        internal void UpdateDomainPLS(PermissionSet grantSet, PermissionSet deniedSet)
        {
            if (this.m_firstPermSetTriple == null)
            {
                this.m_firstPermSetTriple = new PermissionSetTriple();
            }
            this.m_firstPermSetTriple.UpdateGrant(grantSet);
            this.m_firstPermSetTriple.UpdateRefused(deniedSet);
        }

        [SecurityCritical]
        private bool UpdatePermissions(PermissionSetTriple currentTriple, PermissionListSet pls)
        {
            if (pls != null)
            {
                if (pls.m_permSetTriples != null)
                {
                    this.UpdateTripleListAndCreateNewTriple(currentTriple, pls.m_permSetTriples);
                }
                else
                {
                    PermissionSetTriple triple2;
                    PermissionSetTriple firstPermSetTriple = pls.m_firstPermSetTriple;
                    if (currentTriple.Update(firstPermSetTriple, out triple2))
                    {
                        return true;
                    }
                    if (triple2 != null)
                    {
                        this.EnsureTriplesListCreated();
                        this.m_permSetTriples.Add(triple2);
                    }
                }
            }
            else
            {
                this.UpdateTripleListAndCreateNewTriple(currentTriple, null);
            }
            return false;
        }

        private void UpdateTripleListAndCreateNewTriple(PermissionSetTriple currentTriple, ArrayList tripleList)
        {
            if (!currentTriple.IsEmpty())
            {
                if ((this.m_firstPermSetTriple == null) && (this.m_permSetTriples == null))
                {
                    this.m_firstPermSetTriple = new PermissionSetTriple(currentTriple);
                }
                else
                {
                    this.EnsureTriplesListCreated();
                    this.m_permSetTriples.Add(new PermissionSetTriple(currentTriple));
                }
                currentTriple.Reset();
            }
            if (tripleList != null)
            {
                this.EnsureTriplesListCreated();
                this.m_permSetTriples.AddRange(tripleList);
            }
        }

        private void UpdateZoneAndOrigin(PermissionListSet pls)
        {
            if (pls != null)
            {
                if (((this.m_zoneList == null) && (pls.m_zoneList != null)) && (pls.m_zoneList.Count > 0))
                {
                    this.m_zoneList = new ArrayList();
                }
                UpdateArrayList(this.m_zoneList, pls.m_zoneList);
                if (((this.m_originList == null) && (pls.m_originList != null)) && (pls.m_originList.Count > 0))
                {
                    this.m_originList = new ArrayList();
                }
                UpdateArrayList(this.m_originList, pls.m_originList);
            }
        }
    }
}

