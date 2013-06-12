namespace System.Security
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    internal class SecurityRuntime
    {
        internal const bool StackContinue = true;
        internal const bool StackHalt = false;

        private SecurityRuntime()
        {
        }

        [SecurityCritical]
        internal static void Assert(PermissionSet permSet, ref StackCrawlMark stackMark)
        {
            FrameSecurityDescriptor descriptor = CodeAccessSecurityEngine.CheckNReturnSO(CodeAccessSecurityEngine.AssertPermissionToken, CodeAccessSecurityEngine.AssertPermission, ref stackMark, 1);
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
                descriptor.SetAssert(permSet);
            }
        }

        [SecurityCritical]
        internal static void AssertAllPossible(ref StackCrawlMark stackMark)
        {
            FrameSecurityDescriptor securityObjectForFrame = GetSecurityObjectForFrame(ref stackMark, true);
            if (securityObjectForFrame == null)
            {
                Environment.FailFast(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
            }
            else
            {
                if (securityObjectForFrame.GetAssertAllPossible())
                {
                    throw new SecurityException(Environment.GetResourceString("Security_MustRevertOverride"));
                }
                securityObjectForFrame.SetAssertAllPossible();
            }
        }

        [SecurityCritical]
        internal static void Deny(PermissionSet permSet, ref StackCrawlMark stackMark)
        {
            if (!AppDomain.CurrentDomain.IsLegacyCasPolicyEnabled)
            {
                throw new NotSupportedException(Environment.GetResourceString("NotSupported_CasDeny"));
            }
            FrameSecurityDescriptor securityObjectForFrame = GetSecurityObjectForFrame(ref stackMark, true);
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
                securityObjectForFrame.SetDeny(permSet);
            }
        }

        [SecurityCritical]
        private static bool FrameDescHelper(FrameSecurityDescriptor secDesc, IPermission demandIn, PermissionToken permToken, RuntimeMethodHandleInternal rmh)
        {
            return secDesc.CheckDemand((CodeAccessPermission) demandIn, permToken, rmh);
        }

        [SecurityCritical]
        private static bool FrameDescSetHelper(FrameSecurityDescriptor secDesc, PermissionSet demandSet, out PermissionSet alteredDemandSet, RuntimeMethodHandleInternal rmh)
        {
            return secDesc.CheckSetDemand(demandSet, out alteredDemandSet, rmh);
        }

        [SecurityCritical]
        internal static MethodInfo GetMethodInfo(RuntimeMethodHandleInternal rmh)
        {
            if (rmh.IsNullHandle())
            {
                return null;
            }
            PermissionSet.s_fullTrust.Assert();
            return (RuntimeType.GetMethodBase(RuntimeMethodHandle.GetDeclaringType(rmh), rmh) as MethodInfo);
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        internal static extern FrameSecurityDescriptor GetSecurityObjectForFrame(ref StackCrawlMark stackMark, bool create);
        private static int OverridesHelper(FrameSecurityDescriptor secDesc)
        {
            return (OverridesHelper2(secDesc, false) + OverridesHelper2(secDesc, true));
        }

        private static int OverridesHelper2(FrameSecurityDescriptor secDesc, bool fDeclarative)
        {
            int num = 0;
            if (secDesc.GetPermitOnly(fDeclarative) != null)
            {
                num++;
            }
            if (secDesc.GetDenials(fDeclarative) != null)
            {
                num++;
            }
            return num;
        }

        [SecurityCritical]
        internal static void PermitOnly(PermissionSet permSet, ref StackCrawlMark stackMark)
        {
            FrameSecurityDescriptor securityObjectForFrame = GetSecurityObjectForFrame(ref stackMark, true);
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
                securityObjectForFrame.SetPermitOnly(permSet);
            }
        }

        [SecurityCritical]
        internal static void RevertAll(ref StackCrawlMark stackMark)
        {
            FrameSecurityDescriptor securityObjectForFrame = GetSecurityObjectForFrame(ref stackMark, false);
            if (securityObjectForFrame == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
            }
            securityObjectForFrame.RevertAll();
        }

        [SecurityCritical]
        internal static void RevertAssert(ref StackCrawlMark stackMark)
        {
            FrameSecurityDescriptor securityObjectForFrame = GetSecurityObjectForFrame(ref stackMark, false);
            if (securityObjectForFrame == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
            }
            securityObjectForFrame.RevertAssert();
        }

        [SecurityCritical]
        internal static void RevertDeny(ref StackCrawlMark stackMark)
        {
            FrameSecurityDescriptor securityObjectForFrame = GetSecurityObjectForFrame(ref stackMark, false);
            if (securityObjectForFrame == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
            }
            securityObjectForFrame.RevertDeny();
        }

        [SecurityCritical]
        internal static void RevertPermitOnly(ref StackCrawlMark stackMark)
        {
            FrameSecurityDescriptor securityObjectForFrame = GetSecurityObjectForFrame(ref stackMark, false);
            if (securityObjectForFrame == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("ExecutionEngine_MissingSecurityDescriptor"));
            }
            securityObjectForFrame.RevertPermitOnly();
        }
    }
}

