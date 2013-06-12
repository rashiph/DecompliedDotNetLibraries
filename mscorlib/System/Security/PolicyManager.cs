namespace System.Security
{
    using System;
    using System.Collections;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Security.Util;
    using System.Text;
    using System.Threading;

    internal class PolicyManager
    {
        private static QuickCacheEntryType[] FullTrustMap;
        private object m_policyLevels;

        internal PolicyManager()
        {
        }

        [SecurityCritical]
        internal void AddLevel(PolicyLevel level)
        {
            this.PolicyLevels.Add(level);
        }

        internal static bool CanUseQuickCache(CodeGroup group)
        {
            ArrayList list = new ArrayList();
            list.Add(group);
            for (int i = 0; i < list.Count; i++)
            {
                group = (CodeGroup) list[i];
                if (group is IUnionSemanticCodeGroup)
                {
                    if (!TestPolicyStatement(group.PolicyStatement))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
                IMembershipCondition membershipCondition = group.MembershipCondition;
                if ((membershipCondition != null) && !(membershipCondition is IConstantMembershipCondition))
                {
                    return false;
                }
                IList children = group.Children;
                if ((children != null) && (children.Count > 0))
                {
                    IEnumerator enumerator = children.GetEnumerator();
                    while (enumerator.MoveNext())
                    {
                        list.Add(enumerator.Current);
                    }
                }
            }
            return true;
        }

        internal static bool CheckMembershipCondition(IMembershipCondition membershipCondition, Evidence evidence, out object usedEvidence)
        {
            IReportMatchMembershipCondition condition = membershipCondition as IReportMatchMembershipCondition;
            if (condition != null)
            {
                return condition.Check(evidence, out usedEvidence);
            }
            usedEvidence = null;
            evidence.MarkAllEvidenceAsUsed();
            return membershipCondition.Check(evidence);
        }

        [SecurityCritical]
        internal PermissionSet CodeGroupResolve(Evidence evidence, bool systemPolicy)
        {
            PermissionSet permissionSet = null;
            PolicyLevel current = null;
            IEnumerator enumerator = this.PolicyLevels.GetEnumerator();
            evidence.GetHostEvidence<Zone>();
            evidence.GetHostEvidence<StrongName>();
            evidence.GetHostEvidence<Url>();
            byte[] serializedEvidence = evidence.RawSerialize();
            int rawCount = evidence.RawCount;
            bool flag = AppDomain.CurrentDomain.GetData("IgnoreSystemPolicy") != null;
            bool flag2 = false;
            while (enumerator.MoveNext())
            {
                PolicyStatement statement;
                current = (PolicyLevel) enumerator.Current;
                if (systemPolicy)
                {
                    if (current.Type != PolicyLevelType.AppDomain)
                    {
                        goto Label_0078;
                    }
                    continue;
                }
                if (flag && (current.Type != PolicyLevelType.AppDomain))
                {
                    continue;
                }
            Label_0078:
                statement = current.Resolve(evidence, rawCount, serializedEvidence);
                if (permissionSet == null)
                {
                    permissionSet = statement.PermissionSet;
                }
                else
                {
                    permissionSet.InplaceIntersect(statement.GetPermissionSetNoCopy());
                }
                if ((permissionSet == null) || permissionSet.FastIsEmpty())
                {
                    break;
                }
                if ((statement.Attributes & PolicyStatementAttribute.LevelFinal) == PolicyStatementAttribute.LevelFinal)
                {
                    if (current.Type != PolicyLevelType.AppDomain)
                    {
                        flag2 = true;
                    }
                    break;
                }
            }
            if ((permissionSet != null) && flag2)
            {
                PolicyLevel level2 = null;
                for (int i = this.PolicyLevels.Count - 1; i >= 0; i--)
                {
                    current = (PolicyLevel) this.PolicyLevels[i];
                    if (current.Type == PolicyLevelType.AppDomain)
                    {
                        level2 = current;
                        break;
                    }
                }
                if (level2 != null)
                {
                    permissionSet.InplaceIntersect(level2.Resolve(evidence, rawCount, serializedEvidence).GetPermissionSetNoCopy());
                }
            }
            if (permissionSet == null)
            {
                permissionSet = new PermissionSet(PermissionState.None);
            }
            if (!permissionSet.IsUnrestricted())
            {
                IEnumerator hostEnumerator = evidence.GetHostEnumerator();
                while (hostEnumerator.MoveNext())
                {
                    object obj2 = hostEnumerator.Current;
                    IIdentityPermissionFactory factory = obj2 as IIdentityPermissionFactory;
                    if (factory != null)
                    {
                        IPermission perm = factory.CreateIdentityPermission(evidence);
                        if (perm != null)
                        {
                            permissionSet.AddPermission(perm);
                        }
                    }
                }
            }
            permissionSet.IgnoreTypeLoadFailures = true;
            return permissionSet;
        }

        [SecurityCritical]
        internal static void EncodeLevel(PolicyLevel level)
        {
            if (level.Path == null)
            {
                throw new PolicyException(Environment.GetResourceString("Policy_UnableToSave", new object[] { level.Label, Environment.GetResourceString("Policy_SaveNotFileBased") }));
            }
            SecurityElement element = new SecurityElement("configuration");
            SecurityElement child = new SecurityElement("mscorlib");
            SecurityElement element3 = new SecurityElement("security");
            SecurityElement element4 = new SecurityElement("policy");
            element.AddChild(child);
            child.AddChild(element3);
            element3.AddChild(element4);
            element4.AddChild(level.ToXml());
            try
            {
                StringBuilder builder = new StringBuilder();
                Encoding encoding = Encoding.UTF8;
                SecurityElement element5 = new SecurityElement("xml") {
                    m_type = SecurityElementType.Format
                };
                element5.AddAttribute("version", "1.0");
                element5.AddAttribute("encoding", encoding.WebName);
                builder.Append(element5.ToString());
                builder.Append(element.ToString());
                byte[] bytes = encoding.GetBytes(builder.ToString());
                Exception exceptionForHR = Marshal.GetExceptionForHR(Config.SaveDataByte(level.Path, bytes, bytes.Length));
                if (exceptionForHR != null)
                {
                    string str2 = (exceptionForHR != null) ? exceptionForHR.Message : string.Empty;
                    throw new PolicyException(Environment.GetResourceString("Policy_UnableToSave", new object[] { level.Label, str2 }), exceptionForHR);
                }
            }
            catch (Exception exception2)
            {
                if (exception2 is PolicyException)
                {
                    throw exception2;
                }
                throw new PolicyException(Environment.GetResourceString("Policy_UnableToSave", new object[] { level.Label, exception2.Message }), exception2);
            }
            Config.ResetCacheData(level.ConfigId);
            if (CanUseQuickCache(level.RootCodeGroup))
            {
                Config.SetQuickCache(level.ConfigId, GenerateQuickCache(level));
            }
        }

        [SecurityCritical]
        private void EncodeLevel(string label)
        {
            for (int i = 0; i < this.PolicyLevels.Count; i++)
            {
                PolicyLevel level = (PolicyLevel) this.PolicyLevels[i];
                if (level.Label.Equals(label))
                {
                    EncodeLevel(level);
                    return;
                }
            }
        }

        private static QuickCacheEntryType GenerateQuickCache(PolicyLevel level)
        {
            if (FullTrustMap == null)
            {
                FullTrustMap = new QuickCacheEntryType[] { QuickCacheEntryType.FullTrustZoneMyComputer, QuickCacheEntryType.FullTrustZoneIntranet, QuickCacheEntryType.FullTrustZoneTrusted, QuickCacheEntryType.FullTrustZoneInternet, QuickCacheEntryType.FullTrustZoneUntrusted };
            }
            QuickCacheEntryType type = 0;
            Evidence evidence = new Evidence();
            try
            {
                if (level.Resolve(evidence).PermissionSet.IsUnrestricted())
                {
                    type |= QuickCacheEntryType.FullTrustAll;
                }
            }
            catch (PolicyException)
            {
            }
            foreach (SecurityZone zone in Enum.GetValues(typeof(SecurityZone)))
            {
                if (zone != SecurityZone.NoZone)
                {
                    Evidence evidence2 = new Evidence();
                    evidence2.AddHostEvidence<Zone>(new Zone(zone));
                    try
                    {
                        if (level.Resolve(evidence2).PermissionSet.IsUnrestricted())
                        {
                            type |= FullTrustMap[(int) zone];
                        }
                    }
                    catch (PolicyException)
                    {
                    }
                }
            }
            return type;
        }

        internal static bool IsGacAssembly(Evidence evidence)
        {
            return new GacMembershipCondition().Check(evidence);
        }

        [SecurityCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
        internal IEnumerator PolicyHierarchy()
        {
            return this.PolicyLevels.GetEnumerator();
        }

        [SecurityCritical]
        internal PermissionSet Resolve(Evidence evidence)
        {
            PermissionSet grantSet = null;
            if (CodeAccessSecurityEngine.TryResolveGrantSet(evidence, out grantSet))
            {
                return grantSet;
            }
            return this.CodeGroupResolve(evidence, false);
        }

        internal static PolicyStatement ResolveCodeGroup(CodeGroup codeGroup, Evidence evidence)
        {
            if (codeGroup.GetType().Assembly != typeof(UnionCodeGroup).Assembly)
            {
                evidence.MarkAllEvidenceAsUsed();
            }
            return codeGroup.Resolve(evidence);
        }

        [SecurityCritical]
        internal IEnumerator ResolveCodeGroups(Evidence evidence)
        {
            ArrayList list = new ArrayList();
            IEnumerator enumerator = this.PolicyLevels.GetEnumerator();
            while (enumerator.MoveNext())
            {
                CodeGroup group = ((PolicyLevel) enumerator.Current).ResolveMatchingCodeGroups(evidence);
                if (group != null)
                {
                    list.Add(group);
                }
            }
            return list.GetEnumerator(0, list.Count);
        }

        [SecurityCritical]
        internal void Save()
        {
            this.EncodeLevel(Environment.GetResourceString("Policy_PL_Enterprise"));
            this.EncodeLevel(Environment.GetResourceString("Policy_PL_Machine"));
            this.EncodeLevel(Environment.GetResourceString("Policy_PL_User"));
        }

        private static bool TestPolicyStatement(PolicyStatement policy)
        {
            return ((policy == null) || ((policy.Attributes & PolicyStatementAttribute.Exclusive) == PolicyStatementAttribute.Nothing));
        }

        private IList PolicyLevels
        {
            [SecurityCritical]
            get
            {
                if (this.m_policyLevels == null)
                {
                    ArrayList list = new ArrayList();
                    string locationFromType = PolicyLevel.GetLocationFromType(PolicyLevelType.Enterprise);
                    list.Add(new PolicyLevel(PolicyLevelType.Enterprise, locationFromType, ConfigId.EnterprisePolicyLevel));
                    string path = PolicyLevel.GetLocationFromType(PolicyLevelType.Machine);
                    list.Add(new PolicyLevel(PolicyLevelType.Machine, path, ConfigId.MachinePolicyLevel));
                    if (Config.UserDirectory != null)
                    {
                        string str3 = PolicyLevel.GetLocationFromType(PolicyLevelType.User);
                        list.Add(new PolicyLevel(PolicyLevelType.User, str3, ConfigId.UserPolicyLevel));
                    }
                    Interlocked.CompareExchange(ref this.m_policyLevels, list, null);
                }
                return (this.m_policyLevels as ArrayList);
            }
        }
    }
}

