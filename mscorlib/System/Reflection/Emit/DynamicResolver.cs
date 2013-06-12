namespace System.Reflection.Emit
{
    using System;
    using System.Reflection;
    using System.Security;
    using System.Threading;

    internal class DynamicResolver : Resolver
    {
        private byte[] m_code;
        private byte[] m_exceptionHeader;
        private __ExceptionInfo[] m_exceptions;
        private byte[] m_localSignature;
        private DynamicMethod m_method;
        private int m_methodToken;
        private DynamicScope m_scope;
        private int m_stackSize;
        internal const int TOKENFORDYNAMICMETHOD = 1;

        internal DynamicResolver(DynamicILGenerator ilGenerator)
        {
            this.m_stackSize = ilGenerator.GetMaxStackSize();
            this.m_exceptions = ilGenerator.GetExceptions();
            this.m_code = ilGenerator.BakeByteArray();
            this.m_localSignature = ilGenerator.m_localSignature.InternalGetSignatureArray();
            this.m_scope = ilGenerator.m_scope;
            this.m_method = (DynamicMethod) ilGenerator.m_methodBuilder;
            this.m_method.m_resolver = this;
        }

        internal DynamicResolver(DynamicILInfo dynamicILInfo)
        {
            this.m_stackSize = dynamicILInfo.MaxStackSize;
            this.m_code = dynamicILInfo.Code;
            this.m_localSignature = dynamicILInfo.LocalSignature;
            this.m_exceptionHeader = dynamicILInfo.Exceptions;
            this.m_scope = dynamicILInfo.DynamicScope;
            this.m_method = dynamicILInfo.DynamicMethod;
            this.m_method.m_resolver = this;
        }

        private static int CalculateNumberOfExceptions(__ExceptionInfo[] excp)
        {
            int num = 0;
            if (excp == null)
            {
                return 0;
            }
            for (int i = 0; i < excp.Length; i++)
            {
                num += excp[i].GetNumberOfCatches();
            }
            return num;
        }

        [SecuritySafeCritical]
        ~DynamicResolver()
        {
            DynamicMethod method = this.m_method;
            if ((method != null) && (method.m_methodHandle != null))
            {
                DestroyScout scout = null;
                try
                {
                    scout = new DestroyScout();
                }
                catch
                {
                    if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
                    {
                        GC.ReRegisterForFinalize(this);
                    }
                    return;
                }
                scout.m_methodHandle = method.m_methodHandle.Value;
            }
        }

        internal override byte[] GetCodeInfo(ref int stackSize, ref int initLocals, ref int EHCount)
        {
            stackSize = this.m_stackSize;
            if ((this.m_exceptionHeader != null) && (this.m_exceptionHeader.Length != 0))
            {
                if (this.m_exceptionHeader.Length < 4)
                {
                    throw new FormatException();
                }
                byte num = this.m_exceptionHeader[0];
                if ((num & 0x40) != 0)
                {
                    byte[] buffer = new byte[4];
                    for (int i = 0; i < 3; i++)
                    {
                        buffer[i] = this.m_exceptionHeader[i + 1];
                    }
                    EHCount = (BitConverter.ToInt32(buffer, 0) - 4) / 0x18;
                }
                else
                {
                    EHCount = (this.m_exceptionHeader[1] - 2) / 12;
                }
            }
            else
            {
                EHCount = CalculateNumberOfExceptions(this.m_exceptions);
            }
            initLocals = this.m_method.InitLocals ? 1 : 0;
            return this.m_code;
        }

        internal override MethodInfo GetDynamicMethod()
        {
            return this.m_method.GetMethodInfo();
        }

        [SecurityCritical]
        internal override unsafe void GetEHInfo(int excNumber, void* exc)
        {
            Resolver.CORINFO_EH_CLAUSE* corinfo_eh_clausePtr = (Resolver.CORINFO_EH_CLAUSE*) exc;
            for (int i = 0; i < this.m_exceptions.Length; i++)
            {
                int numberOfCatches = this.m_exceptions[i].GetNumberOfCatches();
                if (excNumber < numberOfCatches)
                {
                    corinfo_eh_clausePtr->Flags = this.m_exceptions[i].GetExceptionTypes()[excNumber];
                    corinfo_eh_clausePtr->Flags |= 0x20000000;
                    corinfo_eh_clausePtr->TryOffset = this.m_exceptions[i].GetStartAddress();
                    if ((corinfo_eh_clausePtr->Flags & 2) != 2)
                    {
                        corinfo_eh_clausePtr->TryLength = this.m_exceptions[i].GetEndAddress() - corinfo_eh_clausePtr->TryOffset;
                    }
                    else
                    {
                        corinfo_eh_clausePtr->TryLength = this.m_exceptions[i].GetFinallyEndAddress() - corinfo_eh_clausePtr->TryOffset;
                    }
                    corinfo_eh_clausePtr->HandlerOffset = this.m_exceptions[i].GetCatchAddresses()[excNumber];
                    corinfo_eh_clausePtr->HandlerLength = this.m_exceptions[i].GetCatchEndAddresses()[excNumber] - corinfo_eh_clausePtr->HandlerOffset;
                    corinfo_eh_clausePtr->ClassTokenOrFilterOffset = this.m_exceptions[i].GetFilterAddresses()[excNumber];
                    return;
                }
                excNumber -= numberOfCatches;
            }
        }

        internal override RuntimeType GetJitContext(ref int securityControlFlags)
        {
            SecurityControlFlags flags = SecurityControlFlags.Default;
            if (this.m_method.m_restrictedSkipVisibility)
            {
                flags |= SecurityControlFlags.RestrictedSkipVisibilityChecks;
            }
            else if (this.m_method.m_skipVisibility)
            {
                flags |= SecurityControlFlags.SkipVisibilityChecks;
            }
            RuntimeType typeOwner = this.m_method.m_typeOwner;
            if (this.m_method.m_creationContext != null)
            {
                flags |= SecurityControlFlags.HasCreationContext;
            }
            securityControlFlags = (int) flags;
            return typeOwner;
        }

        internal override byte[] GetLocalsSignature()
        {
            return this.m_localSignature;
        }

        [SecurityCritical]
        private int GetMethodToken()
        {
            if (this.IsValidToken(this.m_methodToken) == 0)
            {
                int tokenFor = this.m_scope.GetTokenFor(this.m_method.GetMethodDescriptor());
                Interlocked.CompareExchange(ref this.m_methodToken, tokenFor, 0);
            }
            return this.m_methodToken;
        }

        internal override byte[] GetRawEHInfo()
        {
            return this.m_exceptionHeader;
        }

        internal override CompressedStack GetSecurityContext()
        {
            return this.m_method.m_creationContext;
        }

        internal override string GetStringLiteral(int token)
        {
            return this.m_scope.GetString(token);
        }

        internal override int IsValidToken(int token)
        {
            if (this.m_scope[token] == null)
            {
                return 0;
            }
            return 1;
        }

        [SecuritySafeCritical]
        internal override int ParentToken(int token)
        {
            RuntimeType declaringType = null;
            object obj2 = this.m_scope[token];
            if (obj2 is RuntimeMethodHandle)
            {
                declaringType = RuntimeMethodHandle.GetDeclaringType(((RuntimeMethodHandle) obj2).GetMethodInfo());
            }
            else if (obj2 is RuntimeFieldHandle)
            {
                declaringType = RuntimeFieldHandle.GetApproxDeclaringType(((RuntimeFieldHandle) obj2).GetRuntimeFieldInfo());
            }
            else if (obj2 is DynamicMethod)
            {
                DynamicMethod method = (DynamicMethod) obj2;
                declaringType = RuntimeMethodHandle.GetDeclaringType(method.m_methodHandle);
            }
            else if (obj2 is GenericMethodInfo)
            {
                GenericMethodInfo info = (GenericMethodInfo) obj2;
                declaringType = info.m_context.GetRuntimeType();
            }
            else if (obj2 is GenericFieldInfo)
            {
                GenericFieldInfo info2 = (GenericFieldInfo) obj2;
                declaringType = info2.m_context.GetRuntimeType();
            }
            else if (obj2 is VarArgMethod)
            {
                VarArgMethod method2 = (VarArgMethod) obj2;
                DynamicMethod dynamicMethod = method2.m_dynamicMethod;
                if (dynamicMethod != null)
                {
                    dynamicMethod.GetMethodDescriptor();
                    declaringType = RuntimeMethodHandle.GetDeclaringType(dynamicMethod.m_methodHandle);
                }
                else
                {
                    RuntimeMethodInfo info3 = method2.m_method;
                    if (info3.DeclaringType == null)
                    {
                        declaringType = RuntimeMethodHandle.GetDeclaringType(info3);
                    }
                    else
                    {
                        declaringType = info3.DeclaringType.TypeHandle.GetRuntimeType();
                    }
                }
            }
            if (declaringType == null)
            {
                return -1;
            }
            return this.m_scope.GetTokenFor(declaringType.GetTypeHandleInternal());
        }

        internal override byte[] ResolveSignature(int token, int fromMethod)
        {
            return this.m_scope.ResolveSignature(token, fromMethod);
        }

        [SecurityCritical]
        internal override unsafe void* ResolveToken(int token)
        {
            object obj2 = this.m_scope[token];
            if (obj2 is RuntimeTypeHandle)
            {
                RuntimeTypeHandle handle = (RuntimeTypeHandle) obj2;
                return (void*) handle.Value;
            }
            if (obj2 is RuntimeMethodHandle)
            {
                RuntimeMethodHandle handle2 = (RuntimeMethodHandle) obj2;
                return (void*) handle2.Value;
            }
            if (obj2 is RuntimeFieldHandle)
            {
                RuntimeFieldHandle handle3 = (RuntimeFieldHandle) obj2;
                return (void*) handle3.Value;
            }
            if (obj2 is DynamicMethod)
            {
                DynamicMethod method = (DynamicMethod) obj2;
                return (void*) method.GetMethodDescriptor().Value;
            }
            if (obj2 is GenericMethodInfo)
            {
                GenericMethodInfo info = (GenericMethodInfo) obj2;
                return (void*) info.m_methodHandle.Value;
            }
            if (obj2 is GenericFieldInfo)
            {
                GenericFieldInfo info2 = (GenericFieldInfo) obj2;
                return (void*) info2.m_fieldHandle.Value;
            }
            if (!(obj2 is VarArgMethod))
            {
                return null;
            }
            VarArgMethod method2 = (VarArgMethod) obj2;
            if (method2.m_dynamicMethod == null)
            {
                return (void*) method2.m_method.MethodHandle.Value;
            }
            return (void*) method2.m_dynamicMethod.GetMethodDescriptor().Value;
        }

        private class DestroyScout
        {
            internal RuntimeMethodHandleInternal m_methodHandle;

            [SecuritySafeCritical]
            ~DestroyScout()
            {
                if (!this.m_methodHandle.IsNullHandle())
                {
                    if (RuntimeMethodHandle.GetResolver(this.m_methodHandle) != null)
                    {
                        if (!Environment.HasShutdownStarted && !AppDomain.CurrentDomain.IsFinalizingForUnload())
                        {
                            GC.ReRegisterForFinalize(this);
                        }
                    }
                    else
                    {
                        RuntimeMethodHandle.Destroy(this.m_methodHandle);
                    }
                }
            }
        }

        [Flags]
        internal enum SecurityControlFlags
        {
            Default = 0,
            HasCreationContext = 4,
            RestrictedSkipVisibilityChecks = 2,
            SkipVisibilityChecks = 1
        }
    }
}

