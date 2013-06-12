namespace System.Security
{
    using System;
    using System.Collections;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Policy;
    using System.Threading;

    internal static class CodeAccessSecurityEngine
    {
        internal static SecurityPermission AssertPermission = new SecurityPermission(SecurityPermissionFlag.Assertion);
        internal static PermissionToken AssertPermissionToken = PermissionToken.GetToken(AssertPermission);

        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern void _GetGrantedPermissionSet(IntPtr secDesc, out PermissionSet grants, out PermissionSet refused);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool AllDomainsHomogeneousWithNoStackModifiers();
        [SecurityCritical]
        internal static void Assert(CodeAccessPermission cap, ref StackCrawlMark stackMark)
        {
            FrameSecurityDescriptor descriptor = CheckNReturnSO(AssertPermissionToken, AssertPermission, ref stackMark, 1);
            if (descriptor == null)
            {
                Environment.FailFast(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
            }
            else
            {
                if (descriptor.HasImperativeAsserts())
                {
                    throw new SecurityException(Environment.GetResourceString("Security_MustRevertOverride"));
                }
                descriptor.SetAssert(cap);
            }
        }

        [SecurityCritical]
        internal static void Check(CodeAccessPermission cap, ref StackCrawlMark stackMark)
        {
            Check(cap, ref stackMark, false);
        }

        [SecurityCritical]
        internal static void Check(PermissionSet permSet, ref StackCrawlMark stackMark)
        {
            Check(permSet, ref stackMark, true);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void Check(object demand, ref StackCrawlMark stackMark, bool isPermSet);
        [SecurityCritical]
        internal static void CheckAssembly(RuntimeAssembly asm, CodeAccessPermission demand)
        {
            PermissionSet set;
            PermissionSet set2;
            asm.GetGrantSet(out set, out set2);
            CheckHelper(set, set2, demand, PermissionToken.GetToken(demand), RuntimeMethodHandleInternal.EmptyHandle, asm, SecurityAction.Demand, true);
        }

        [SecurityCritical]
        private static void CheckGrantSetHelper(PermissionSet grantSet)
        {
            grantSet.CopyWithNoIdentityPermissions().Demand();
        }

        [SecurityCritical]
        internal static bool CheckHelper(PermissionSet grantedSet, PermissionSet refusedSet, CodeAccessPermission demand, PermissionToken permToken, RuntimeMethodHandleInternal rmh, object assemblyOrString, SecurityAction action, bool throwException)
        {
            if (permToken == null)
            {
                permToken = PermissionToken.GetToken(demand);
            }
            if (grantedSet != null)
            {
                grantedSet.CheckDecoded(permToken.m_index);
            }
            if (refusedSet != null)
            {
                refusedSet.CheckDecoded(permToken.m_index);
            }
            bool flag = SecurityManager._SetThreadSecurity(false);
            try
            {
                if (grantedSet == null)
                {
                    if (!throwException)
                    {
                        return false;
                    }
                    ThrowSecurityException(assemblyOrString, grantedSet, refusedSet, rmh, action, demand, demand);
                }
                else if (!grantedSet.IsUnrestricted())
                {
                    CodeAccessPermission grant = (CodeAccessPermission) grantedSet.GetPermission(permToken);
                    if (!demand.CheckDemand(grant))
                    {
                        if (throwException)
                        {
                            ThrowSecurityException(assemblyOrString, grantedSet, refusedSet, rmh, action, demand, demand);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                if (refusedSet != null)
                {
                    CodeAccessPermission permission = (CodeAccessPermission) refusedSet.GetPermission(permToken);
                    if ((permission != null) && !permission.CheckDeny(demand))
                    {
                        if (!throwException)
                        {
                            return false;
                        }
                        ThrowSecurityException(assemblyOrString, grantedSet, refusedSet, rmh, action, demand, demand);
                    }
                    if (refusedSet.IsUnrestricted())
                    {
                        if (throwException)
                        {
                            ThrowSecurityException(assemblyOrString, grantedSet, refusedSet, rmh, action, demand, demand);
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (Exception)
            {
                if (throwException)
                {
                    ThrowSecurityException(assemblyOrString, grantedSet, refusedSet, rmh, action, demand, demand);
                }
                else
                {
                    return false;
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
        internal static void CheckHelper(CompressedStack cs, PermissionSet grantedSet, PermissionSet refusedSet, CodeAccessPermission demand, PermissionToken permToken, RuntimeMethodHandleInternal rmh, RuntimeAssembly asm, SecurityAction action)
        {
            if (cs != null)
            {
                cs.CheckDemand(demand, permToken, rmh);
            }
            else
            {
                CheckHelper(grantedSet, refusedSet, demand, permToken, rmh, asm, action, true);
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern FrameSecurityDescriptor CheckNReturnSO(PermissionToken permToken, CodeAccessPermission demand, ref StackCrawlMark stackMark, int create);
        [SecurityCritical]
        internal static bool CheckSetHelper(PermissionSet grants, PermissionSet refused, PermissionSet demands, RuntimeMethodHandleInternal rmh, object assemblyOrString, SecurityAction action, bool throwException)
        {
            IPermission firstPermThatFailed = null;
            if (grants != null)
            {
                grants.CheckDecoded(demands);
            }
            if (refused != null)
            {
                refused.CheckDecoded(demands);
            }
            bool flag = SecurityManager._SetThreadSecurity(false);
            try
            {
                if (!demands.CheckDemand(grants, out firstPermThatFailed))
                {
                    if (!throwException)
                    {
                        return false;
                    }
                    ThrowSecurityException(assemblyOrString, grants, refused, rmh, action, demands, firstPermThatFailed);
                }
                if (!demands.CheckDeny(refused, out firstPermThatFailed))
                {
                    if (throwException)
                    {
                        ThrowSecurityException(assemblyOrString, grants, refused, rmh, action, demands, firstPermThatFailed);
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            catch (SecurityException)
            {
                throw;
            }
            catch (Exception)
            {
                if (throwException)
                {
                    ThrowSecurityException(assemblyOrString, grants, refused, rmh, action, demands, firstPermThatFailed);
                }
                else
                {
                    return false;
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
        internal static void CheckSetHelper(CompressedStack cs, PermissionSet grants, PermissionSet refused, PermissionSet demands, RuntimeMethodHandleInternal rmh, RuntimeAssembly asm, SecurityAction action)
        {
            if (cs != null)
            {
                cs.CheckSetDemand(demands, rmh);
            }
            else
            {
                CheckSetHelper(grants, refused, demands, rmh, asm, action, true);
            }
        }

        [Conditional("_DEBUG"), SecurityCritical]
        private static void DEBUG_OUT(string str)
        {
        }

        [SecurityCritical]
        internal static void Deny(CodeAccessPermission cap, ref StackCrawlMark stackMark)
        {
            if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_CasDeny"));
            }
            FrameSecurityDescriptor securityObjectForFrame = SecurityRuntime.GetSecurityObjectForFrame(ref stackMark, true);
            if (securityObjectForFrame == null)
            {
                Environment.FailFast(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
            }
            else
            {
                if (securityObjectForFrame.HasImperativeDenials())
                {
                    throw new SecurityException(Environment.GetResourceString("Security_MustRevertOverride"));
                }
                securityObjectForFrame.SetDeny(cap);
            }
        }

        [SecurityCritical]
        internal static void GetZoneAndOrigin(ref StackCrawlMark mark, out ArrayList zone, out ArrayList origin)
        {
            zone = new ArrayList();
            origin = new ArrayList();
            GetZoneAndOriginInternal(zone, origin, ref mark);
        }

        [SecurityCritical]
        internal static void GetZoneAndOriginHelper(CompressedStack cs, PermissionSet grantSet, PermissionSet refusedSet, ArrayList zoneList, ArrayList originList)
        {
            if (cs != null)
            {
                cs.GetZoneAndOrigin(zoneList, originList, PermissionToken.GetToken(typeof(ZoneIdentityPermission)), PermissionToken.GetToken(typeof(UrlIdentityPermission)));
            }
            else
            {
                ZoneIdentityPermission permission = (ZoneIdentityPermission) grantSet.GetPermission(typeof(ZoneIdentityPermission));
                UrlIdentityPermission permission2 = (UrlIdentityPermission) grantSet.GetPermission(typeof(UrlIdentityPermission));
                if (permission != null)
                {
                    zoneList.Add(permission.SecurityZone);
                }
                if (permission2 != null)
                {
                    originList.Add(permission2.Url);
                }
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void GetZoneAndOriginInternal(ArrayList zoneList, ArrayList originList, ref StackCrawlMark stackMark);
        [SecurityCritical]
        internal static void PermitOnly(CodeAccessPermission cap, ref StackCrawlMark stackMark)
        {
            FrameSecurityDescriptor securityObjectForFrame = SecurityRuntime.GetSecurityObjectForFrame(ref stackMark, true);
            if (securityObjectForFrame == null)
            {
                Environment.FailFast(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
            }
            else
            {
                if (securityObjectForFrame.HasImperativeRestrictions())
                {
                    throw new SecurityException(Environment.GetResourceString("Security_MustRevertOverride"));
                }
                securityObjectForFrame.SetPermitOnly(cap);
            }
        }

        private static void PreResolve(out bool isFullyTrusted, out bool isHomogeneous)
        {
            ApplicationTrust applicationTrust = AppDomain.CurrentDomain.SetupInformation.ApplicationTrust;
            if (applicationTrust != null)
            {
                isFullyTrusted = applicationTrust.DefaultGrantSet.PermissionSet.IsUnrestricted();
                isHomogeneous = true;
            }
            else
            {
                bool? nullable = AppDomain.CurrentDomain.IsCompatibilitySwitchSet("NetFx40_LegacySecurityPolicy");
                if ((nullable.HasValue && nullable.Value) || AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
                {
                    isFullyTrusted = false;
                    isHomogeneous = false;
                }
                else
                {
                    isFullyTrusted = true;
                    isHomogeneous = true;
                }
            }
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern bool QuickCheckForAllDemands();
        [MethodImpl(MethodImplOptions.NoInlining), SecurityCritical]
        private static void ReflectionTargetDemandHelper(int permission, PermissionSet targetGrant)
        {
            StackCrawlMark lookForMyCaller = StackCrawlMark.LookForMyCaller;
            CompressedStack compressedStack = CompressedStack.GetCompressedStack(ref lookForMyCaller);
            ReflectionTargetDemandHelper(permission, targetGrant, compressedStack);
        }

        [SecurityCritical]
        internal static void ReflectionTargetDemandHelper(PermissionType permission, PermissionSet targetGrant)
        {
            ReflectionTargetDemandHelper((int) permission, targetGrant);
        }

        [SecurityCritical]
        private static void ReflectionTargetDemandHelper(int permission, PermissionSet targetGrant, Resolver accessContext)
        {
            ReflectionTargetDemandHelper(permission, targetGrant, accessContext.GetSecurityContext());
        }

        [SecurityCritical]
        private static void ReflectionTargetDemandHelper(int permission, PermissionSet targetGrant, CompressedStack securityContext)
        {
            PermissionSet grantSet = null;
            if (targetGrant == null)
            {
                grantSet = new PermissionSet(PermissionState.Unrestricted);
            }
            else
            {
                grantSet = targetGrant.CopyWithNoIdentityPermissions();
                grantSet.AddPermission(new ReflectionPermission(ReflectionPermissionFlag.RestrictedMemberAccess));
            }
            securityContext.DemandFlagsOrGrantSet(((int) 1) << permission, grantSet);
        }

        [SecuritySafeCritical]
        private static PermissionSet ResolveGrantSet(Evidence evidence, out int specialFlags, bool checkExecutionPermission)
        {
            PermissionSet grantSet = null;
            if (!TryResolveGrantSet(evidence, out grantSet))
            {
                grantSet = new PermissionSet(PermissionState.Unrestricted);
            }
            if (checkExecutionPermission)
            {
                SecurityPermission perm = new SecurityPermission(SecurityPermissionFlag.Execution);
                if (!grantSet.Contains(perm))
                {
                    throw new PolicyException(Environment.GetResourceString("Policy_NoExecutionPermission"), -2146233320);
                }
            }
            specialFlags = SecurityManager.GetSpecialFlags(grantSet, null);
            return grantSet;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern void SpecialDemand(PermissionType whatPermission, ref StackCrawlMark stackMark);
        [SecurityCritical]
        private static void ThrowSecurityException(object assemblyOrString, PermissionSet granted, PermissionSet refused, RuntimeMethodHandleInternal rmh, SecurityAction action, object demand, IPermission permThatFailed)
        {
            if ((assemblyOrString == null) || (assemblyOrString is RuntimeAssembly))
            {
                ThrowSecurityException((RuntimeAssembly) assemblyOrString, granted, refused, rmh, action, demand, permThatFailed);
            }
            else
            {
                AssemblyName asmName = new AssemblyName((string) assemblyOrString);
                throw SecurityException.MakeSecurityException(asmName, null, granted, refused, rmh, action, demand, permThatFailed);
            }
        }

        [SecurityCritical]
        private static void ThrowSecurityException(RuntimeAssembly asm, PermissionSet granted, PermissionSet refused, RuntimeMethodHandleInternal rmh, SecurityAction action, object demand, IPermission permThatFailed)
        {
            AssemblyName asmName = null;
            Evidence asmEvidence = null;
            if (asm != null)
            {
                PermissionSet.s_fullTrust.Assert();
                asmName = asm.GetName();
                if (asm != Assembly.GetExecutingAssembly())
                {
                    asmEvidence = asm.Evidence;
                }
            }
            throw SecurityException.MakeSecurityException(asmName, asmEvidence, granted, refused, rmh, action, demand, permThatFailed);
        }

        [SecuritySafeCritical]
        internal static bool TryResolveGrantSet(Evidence evidence, out PermissionSet grantSet)
        {
            HostSecurityManager hostSecurityManager = AppDomain.CurrentDomain.HostSecurityManager;
            if (evidence.GetHostEvidence<GacInstalled>() != null)
            {
                grantSet = new PermissionSet(PermissionState.Unrestricted);
                return true;
            }
            if ((hostSecurityManager.Flags & HostSecurityManagerOptions.HostResolvePolicy) == HostSecurityManagerOptions.HostResolvePolicy)
            {
                PermissionSet target = hostSecurityManager.ResolvePolicy(evidence);
                if (target == null)
                {
                    throw new PolicyException(Environment.GetResourceString("Policy_NullHostGrantSet", new object[] { hostSecurityManager.GetType().FullName }));
                }
                if (AppDomain.CurrentDomain.IsHomogenous)
                {
                    if (target.IsEmpty())
                    {
                        throw new PolicyException(Environment.GetResourceString("Policy_NoExecutionPermission"));
                    }
                    PermissionSet permissionSet = AppDomain.CurrentDomain.ApplicationTrust.DefaultGrantSet.PermissionSet;
                    if (!(target.IsUnrestricted() || (target.IsSubsetOf(permissionSet) && permissionSet.IsSubsetOf(target))))
                    {
                        throw new PolicyException(Environment.GetResourceString("Policy_GrantSetDoesNotMatchDomain", new object[] { hostSecurityManager.GetType().FullName }));
                    }
                }
                grantSet = target;
                return true;
            }
            if (AppDomain.CurrentDomain.IsHomogenous)
            {
                grantSet = AppDomain.CurrentDomain.GetHomogenousGrantSet(evidence);
                return true;
            }
            grantSet = null;
            return false;
        }

        [SecurityCritical]
        private static PermissionListSet UpdateAppDomainPLS(PermissionListSet adPLS, PermissionSet grantedPerms, PermissionSet refusedPerms)
        {
            if (adPLS == null)
            {
                adPLS = new PermissionListSet();
                adPLS.UpdateDomainPLS(grantedPerms, refusedPerms);
                return adPLS;
            }
            PermissionListSet set = new PermissionListSet();
            set.UpdateDomainPLS(adPLS);
            set.UpdateDomainPLS(grantedPerms, refusedPerms);
            return set;
        }
    }
}

