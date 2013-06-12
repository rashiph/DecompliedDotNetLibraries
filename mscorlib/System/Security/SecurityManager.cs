namespace System.Security
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Security.Util;
    using System.Threading;

    [ComVisible(true)]
    public static class SecurityManager
    {
        private static SecurityPermission executionSecurityPermission = null;
        private static System.Security.PolicyManager polmgr = new System.Security.PolicyManager();
        private static int[][] s_BuiltInPermissionIndexMap;
        private static CodeAccessPermission[] s_UnrestrictedSpecialPermissionMap;

        static SecurityManager()
        {
            int[][] numArray = new int[6][];
            int[] numArray2 = new int[2];
            numArray2[1] = 10;
            numArray[0] = numArray2;
            numArray[1] = new int[] { 1, 11 };
            numArray[2] = new int[] { 2, 12 };
            numArray[3] = new int[] { 4, 13 };
            numArray[4] = new int[] { 6, 14 };
            numArray[5] = new int[] { 7, 9 };
            s_BuiltInPermissionIndexMap = numArray;
            s_UnrestrictedSpecialPermissionMap = new CodeAccessPermission[] { new EnvironmentPermission(PermissionState.Unrestricted), new FileDialogPermission(PermissionState.Unrestricted), new FileIOPermission(PermissionState.Unrestricted), new ReflectionPermission(PermissionState.Unrestricted), new SecurityPermission(PermissionState.Unrestricted), new UIPermission(PermissionState.Unrestricted) };
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool _SetThreadSecurity(bool bThreadSecurity);
        [SecurityCritical]
        public static bool CurrentThreadRequiresSecurityContextCapture()
        {
            return !CodeAccessSecurityEngine.QuickCheckForAllDemands();
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern void GetGrantedPermissions(ObjectHandleOnStack retGranted, ObjectHandleOnStack retDenied, StackCrawlMarkHandle stackMark);
        internal static int GetSpecialFlags(PermissionSet grantSet, PermissionSet deniedSet)
        {
            if (((grantSet != null) && grantSet.IsUnrestricted()) && ((deniedSet == null) || deniedSet.IsEmpty()))
            {
                return -1;
            }
            SecurityPermission permission = null;
            SecurityPermissionFlag noFlags = SecurityPermissionFlag.NoFlags;
            ReflectionPermission permission2 = null;
            ReflectionPermissionFlag reflectionPermissionFlags = ReflectionPermissionFlag.NoFlags;
            CodeAccessPermission[] permissionArray = new CodeAccessPermission[6];
            if (grantSet != null)
            {
                if (grantSet.IsUnrestricted())
                {
                    noFlags = SecurityPermissionFlag.AllFlags;
                    reflectionPermissionFlags = ReflectionPermissionFlag.RestrictedMemberAccess | ReflectionPermissionFlag.AllFlags;
                    for (int i = 0; i < permissionArray.Length; i++)
                    {
                        permissionArray[i] = s_UnrestrictedSpecialPermissionMap[i];
                    }
                }
                else
                {
                    permission = grantSet.GetPermission(6) as SecurityPermission;
                    if (permission != null)
                    {
                        noFlags = permission.Flags;
                    }
                    permission2 = grantSet.GetPermission(4) as ReflectionPermission;
                    if (permission2 != null)
                    {
                        reflectionPermissionFlags = permission2.Flags;
                    }
                    for (int j = 0; j < permissionArray.Length; j++)
                    {
                        permissionArray[j] = grantSet.GetPermission(s_BuiltInPermissionIndexMap[j][0]) as CodeAccessPermission;
                    }
                }
            }
            if (deniedSet != null)
            {
                if (deniedSet.IsUnrestricted())
                {
                    noFlags = SecurityPermissionFlag.NoFlags;
                    reflectionPermissionFlags = ReflectionPermissionFlag.NoFlags;
                    for (int k = 0; k < s_BuiltInPermissionIndexMap.Length; k++)
                    {
                        permissionArray[k] = null;
                    }
                }
                else
                {
                    permission = deniedSet.GetPermission(6) as SecurityPermission;
                    if (permission != null)
                    {
                        noFlags &= ~permission.Flags;
                    }
                    permission2 = deniedSet.GetPermission(4) as ReflectionPermission;
                    if (permission2 != null)
                    {
                        reflectionPermissionFlags &= ~permission2.Flags;
                    }
                    for (int m = 0; m < s_BuiltInPermissionIndexMap.Length; m++)
                    {
                        CodeAccessPermission permission3 = deniedSet.GetPermission(s_BuiltInPermissionIndexMap[m][0]) as CodeAccessPermission;
                        if ((permission3 != null) && !permission3.IsSubsetOf(null))
                        {
                            permissionArray[m] = null;
                        }
                    }
                }
            }
            int num5 = MapToSpecialFlags(noFlags, reflectionPermissionFlags);
            if (num5 != -1)
            {
                for (int n = 0; n < permissionArray.Length; n++)
                {
                    if ((permissionArray[n] != null) && ((IUnrestrictedPermission) permissionArray[n]).IsUnrestricted())
                    {
                        num5 |= ((int) 1) << s_BuiltInPermissionIndexMap[n][1];
                    }
                }
            }
            return num5;
        }

        public static PermissionSet GetStandardSandbox(Evidence evidence)
        {
            if (evidence == null)
            {
                throw new ArgumentNullException("evidence");
            }
            Zone hostEvidence = evidence.GetHostEvidence<Zone>();
            if (hostEvidence == null)
            {
                return new PermissionSet(PermissionState.None);
            }
            if (hostEvidence.SecurityZone == SecurityZone.MyComputer)
            {
                return new PermissionSet(PermissionState.Unrestricted);
            }
            if (hostEvidence.SecurityZone == SecurityZone.Intranet)
            {
                PermissionSet localIntranet = BuiltInPermissionSets.LocalIntranet;
                PolicyStatement statement = new NetCodeGroup(new AllMembershipCondition()).Resolve(evidence);
                PolicyStatement statement2 = new FileCodeGroup(new AllMembershipCondition(), FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read).Resolve(evidence);
                if (statement != null)
                {
                    localIntranet.InplaceUnion(statement.PermissionSet);
                }
                if (statement2 != null)
                {
                    localIntranet.InplaceUnion(statement2.PermissionSet);
                }
                return localIntranet;
            }
            if ((hostEvidence.SecurityZone != SecurityZone.Internet) && (hostEvidence.SecurityZone != SecurityZone.Trusted))
            {
                return new PermissionSet(PermissionState.None);
            }
            PermissionSet internet = BuiltInPermissionSets.Internet;
            PolicyStatement statement3 = new NetCodeGroup(new AllMembershipCondition()).Resolve(evidence);
            if (statement3 != null)
            {
                internet.InplaceUnion(statement3.PermissionSet);
            }
            return internet;
        }

        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        public static void GetZoneAndOrigin(out ArrayList zone, out ArrayList origin)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            CodeAccessSecurityEngine.GetZoneAndOrigin(ref lookForMyCaller, out zone, out origin);
        }

        [MethodImpl(MethodImplOptions.NoInlining), Obsolete("IsGranted is obsolete and will be removed in a future release of the .NET Framework.  Please use the PermissionSet property of either AppDomain or Assembly instead."), SecuritySafeCritical]
        public static bool IsGranted(IPermission perm)
        {
            if (perm != null)
            {
                PermissionSet o = null;
                PermissionSet set2 = null;
                StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
                GetGrantedPermissions(JitHelpers.GetObjectHandleOnStack<PermissionSet>(ref o), JitHelpers.GetObjectHandleOnStack<PermissionSet>(ref set2), JitHelpers.GetStackCrawlMarkHandle(ref lookForMyCaller));
                if (!o.Contains(perm))
                {
                    return false;
                }
                if (set2 != null)
                {
                    return !set2.Contains(perm);
                }
            }
            return true;
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        internal static extern bool IsSameType(string strLeft, string strRight);
        [SecuritySafeCritical, Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
        public static PolicyLevel LoadPolicyLevelFromFile(string path, PolicyLevelType type)
        {
            PolicyLevel level;
            if (path == null)
            {
                throw new ArgumentNullException("path");
            }
            if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyExplicit"));
            }
            if (!File.InternalExists(path))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_PolicyFileDoesNotExist"));
            }
            string fullPath = Path.GetFullPath(path);
            FileIOPermission permission = new FileIOPermission(PermissionState.None);
            permission.AddPathList(FileIOPermissionAccess.Read, fullPath);
            permission.AddPathList(FileIOPermissionAccess.Write, fullPath);
            permission.Demand();
            using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    level = LoadPolicyLevelFromStringHelper(reader.ReadToEnd(), path, type);
                }
            }
            return level;
        }

        [Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
        public static PolicyLevel LoadPolicyLevelFromString(string str, PolicyLevelType type)
        {
            return LoadPolicyLevelFromStringHelper(str, null, type);
        }

        private static PolicyLevel LoadPolicyLevelFromStringHelper(string str, string path, PolicyLevelType type)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            PolicyLevel level = new PolicyLevel(type, path);
            SecurityElement topElement = new Parser(str).GetTopElement();
            if (topElement == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Policy_BadXml"), new object[] { "configuration" }));
            }
            SecurityElement element2 = topElement.SearchForChildByTag("mscorlib");
            if (element2 == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Policy_BadXml"), new object[] { "mscorlib" }));
            }
            SecurityElement element3 = element2.SearchForChildByTag("security");
            if (element3 == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Policy_BadXml"), new object[] { "security" }));
            }
            SecurityElement element4 = element3.SearchForChildByTag("policy");
            if (element4 == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Policy_BadXml"), new object[] { "policy" }));
            }
            SecurityElement e = element4.SearchForChildByTag("PolicyLevel");
            if (e == null)
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Policy_BadXml"), new object[] { "PolicyLevel" }));
            }
            level.FromXml(e);
            return level;
        }

        private static int MapToSpecialFlags(SecurityPermissionFlag securityPermissionFlags, ReflectionPermissionFlag reflectionPermissionFlags)
        {
            int num = 0;
            if ((securityPermissionFlags & SecurityPermissionFlag.UnmanagedCode) == SecurityPermissionFlag.UnmanagedCode)
            {
                num |= 1;
            }
            if ((securityPermissionFlags & SecurityPermissionFlag.SkipVerification) == SecurityPermissionFlag.SkipVerification)
            {
                num |= 2;
            }
            if ((securityPermissionFlags & SecurityPermissionFlag.Assertion) == SecurityPermissionFlag.Assertion)
            {
                num |= 8;
            }
            if ((securityPermissionFlags & SecurityPermissionFlag.SerializationFormatter) == SecurityPermissionFlag.SerializationFormatter)
            {
                num |= 0x20;
            }
            if ((securityPermissionFlags & SecurityPermissionFlag.BindingRedirects) == SecurityPermissionFlag.BindingRedirects)
            {
                num |= 0x100;
            }
            if ((securityPermissionFlags & SecurityPermissionFlag.ControlEvidence) == SecurityPermissionFlag.ControlEvidence)
            {
                num |= 0x10000;
            }
            if ((securityPermissionFlags & SecurityPermissionFlag.ControlPrincipal) == SecurityPermissionFlag.ControlPrincipal)
            {
                num |= 0x20000;
            }
            if ((reflectionPermissionFlags & ReflectionPermissionFlag.RestrictedMemberAccess) == ReflectionPermissionFlag.RestrictedMemberAccess)
            {
                num |= 0x40;
            }
            if ((reflectionPermissionFlags & ReflectionPermissionFlag.MemberAccess) == ReflectionPermissionFlag.MemberAccess)
            {
                num |= 0x10;
            }
            return num;
        }

        [SecuritySafeCritical, Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public static IEnumerator PolicyHierarchy()
        {
            if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyExplicit"));
            }
            return polmgr.PolicyHierarchy();
        }

        [SecurityCritical]
        private static PermissionSet ResolveCasPolicy(Evidence evidence, PermissionSet reqdPset, PermissionSet optPset, PermissionSet denyPset, out PermissionSet denied, out int securitySpecialFlags, bool checkExecutionPermission)
        {
            CodeAccessPermission.Assert(true);
            PermissionSet grantSet = ResolvePolicy(evidence, reqdPset, optPset, denyPset, out denied, checkExecutionPermission);
            securitySpecialFlags = GetSpecialFlags(grantSet, denied);
            return grantSet;
        }

        [SecuritySafeCritical, Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public static PermissionSet ResolvePolicy(Evidence evidence)
        {
            if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyExplicit"));
            }
            if (evidence == null)
            {
                evidence = new Evidence();
            }
            return polmgr.Resolve(evidence);
        }

        [SecuritySafeCritical, Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public static PermissionSet ResolvePolicy(Evidence[] evidences)
        {
            if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyExplicit"));
            }
            if ((evidences == null) || (evidences.Length == 0))
            {
                evidences = new Evidence[1];
            }
            PermissionSet set = ResolvePolicy(evidences[0]);
            if (set == null)
            {
                return null;
            }
            for (int i = 1; i < evidences.Length; i++)
            {
                set = set.Intersect(ResolvePolicy(evidences[i]));
                if ((set == null) || set.IsEmpty())
                {
                    return set;
                }
            }
            return set;
        }

        [Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
        public static PermissionSet ResolvePolicy(Evidence evidence, PermissionSet reqdPset, PermissionSet optPset, PermissionSet denyPset, out PermissionSet denied)
        {
            if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyExplicit"));
            }
            return ResolvePolicy(evidence, reqdPset, optPset, denyPset, out denied, true);
        }

        [SecurityCritical]
        private static PermissionSet ResolvePolicy(Evidence evidence, PermissionSet reqdPset, PermissionSet optPset, PermissionSet denyPset, out PermissionSet denied, bool checkExecutionPermission)
        {
            if (executionSecurityPermission == null)
            {
                executionSecurityPermission = new SecurityPermission(SecurityPermissionFlag.Execution);
            }
            PermissionSet other = null;
            Exception exception = null;
            PermissionSet set2 = optPset;
            if (reqdPset == null)
            {
                other = set2;
            }
            else
            {
                other = (set2 == null) ? null : reqdPset.Union(set2);
            }
            if ((other != null) && !other.IsUnrestricted())
            {
                other.AddPermission(executionSecurityPermission);
            }
            if (evidence == null)
            {
                evidence = new Evidence();
            }
            PermissionSet target = polmgr.Resolve(evidence);
            if (other != null)
            {
                target.InplaceIntersect(other);
            }
            if (checkExecutionPermission && (!target.Contains(executionSecurityPermission) || ((denyPset != null) && denyPset.Contains(executionSecurityPermission))))
            {
                throw new PolicyException(Environment.GetResourceString("Policy_NoExecutionPermission"), -2146233320, exception);
            }
            if ((reqdPset != null) && !reqdPset.IsSubsetOf(target))
            {
                throw new PolicyException(Environment.GetResourceString("Policy_NoRequiredPermission"), -2146233321, exception);
            }
            if (denyPset != null)
            {
                denied = denyPset.Copy();
                target.MergeDeniedSet(denied);
                if (denied.IsEmpty())
                {
                    denied = null;
                }
            }
            else
            {
                denied = null;
            }
            target.IgnoreTypeLoadFailures = true;
            return target;
        }

        [SecuritySafeCritical, Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information.")]
        public static IEnumerator ResolvePolicyGroups(Evidence evidence)
        {
            if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyExplicit"));
            }
            return polmgr.ResolveCodeGroups(evidence);
        }

        [Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical]
        public static PermissionSet ResolveSystemPolicy(Evidence evidence)
        {
            if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyExplicit"));
            }
            if (System.Security.PolicyManager.IsGacAssembly(evidence))
            {
                return new PermissionSet(PermissionState.Unrestricted);
            }
            return polmgr.CodeGroupResolve(evidence, true);
        }

        [Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecuritySafeCritical, SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
        public static void SavePolicy()
        {
            if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyExplicit"));
            }
            polmgr.Save();
        }

        [SecuritySafeCritical, Obsolete("This method is obsolete and will be removed in a future release of the .NET Framework. See http://go.microsoft.com/fwlink/?LinkID=155570 for more information."), SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.ControlPolicy)]
        public static void SavePolicyLevel(PolicyLevel level)
        {
            if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_RequiresCasPolicyExplicit"));
            }
            System.Security.PolicyManager.EncodeLevel(level);
        }

        [Obsolete("Because execution permission checks can no longer be turned off, the CheckExecutionRights property no longer has any effect.")]
        public static bool CheckExecutionRights
        {
            get
            {
                return true;
            }
            set
            {
            }
        }

        internal static System.Security.PolicyManager PolicyManager
        {
            get
            {
                return polmgr;
            }
        }

        [Obsolete("Because security can no longer be turned off, the SecurityEnabled property no longer has any effect.")]
        public static bool SecurityEnabled
        {
            get
            {
                return true;
            }
            set
            {
            }
        }
    }
}

