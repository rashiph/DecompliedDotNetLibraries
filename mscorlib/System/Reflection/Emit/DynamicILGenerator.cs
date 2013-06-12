namespace System.Reflection.Emit
{
    using System;
    using System.Diagnostics.SymbolStore;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class DynamicILGenerator : ILGenerator
    {
        private int m_methodSigToken;
        internal DynamicScope m_scope;

        internal DynamicILGenerator(DynamicMethod method, byte[] methodSignature, int size) : base(method, size)
        {
            this.m_scope = new DynamicScope();
            this.m_scope.GetTokenFor(method);
            this.m_methodSigToken = this.m_scope.GetTokenFor(methodSignature);
        }

        private int AddSignature(byte[] sig)
        {
            return (this.m_scope.GetTokenFor(sig) | 0x11000000);
        }

        private int AddStringLiteral(string s)
        {
            return (this.m_scope.GetTokenFor(s) | 0x70000000);
        }

        public override void BeginCatchBlock(Type exceptionType)
        {
            if (base.CurrExcStackCount == 0)
            {
                throw new NotSupportedException(Environment.GetResourceString("Argument_NotInExceptionBlock"));
            }
            __ExceptionInfo info = base.CurrExcStack[base.CurrExcStackCount - 1];
            if (info.GetCurrentState() == 1)
            {
                if (exceptionType != null)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_ShouldNotSpecifyExceptionType"));
                }
                this.Emit(OpCodes.Endfilter);
            }
            else
            {
                if (exceptionType == null)
                {
                    throw new ArgumentNullException("exceptionType");
                }
                if (!exceptionType.GetType().IsRuntimeType)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
                }
                Label endLabel = info.GetEndLabel();
                this.Emit(OpCodes.Leave, endLabel);
                base.UpdateStackSize(OpCodes.Nop, 1);
            }
            info.MarkCatchAddr(this.ILOffset, exceptionType);
            info.m_filterAddr[info.m_currentCatch - 1] = this.m_scope.GetTokenFor(exceptionType.TypeHandle);
        }

        public override void BeginExceptFilterBlock()
        {
            throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
        }

        public override Label BeginExceptionBlock()
        {
            return base.BeginExceptionBlock();
        }

        public override void BeginFaultBlock()
        {
            throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
        }

        public override void BeginFinallyBlock()
        {
            base.BeginFinallyBlock();
        }

        public override void BeginScope()
        {
            throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
        }

        public override LocalBuilder DeclareLocal(Type localType, bool pinned)
        {
            if (localType == null)
            {
                throw new ArgumentNullException("localType");
            }
            if (!localType.IsRuntimeType)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));
            }
            LocalBuilder builder = new LocalBuilder(base.m_localCount, localType, base.m_methodBuilder);
            base.m_localSignature.AddArgument(localType, pinned);
            base.m_localCount++;
            return builder;
        }

        [SecuritySafeCritical, ComVisible(true)]
        public override void Emit(OpCode opcode, ConstructorInfo con)
        {
            if ((con == null) || !(con is RuntimeConstructorInfo))
            {
                throw new ArgumentNullException("con");
            }
            if ((con.DeclaringType != null) && (con.DeclaringType.IsGenericType || con.DeclaringType.IsArray))
            {
                this.Emit(opcode, con.MethodHandle, con.DeclaringType.TypeHandle);
            }
            else
            {
                this.Emit(opcode, con.MethodHandle);
            }
        }

        public override void Emit(OpCode opcode, SignatureHelper signature)
        {
            if (signature == null)
            {
                throw new ArgumentNullException("signature");
            }
            int stackchange = 0;
            base.EnsureCapacity(7);
            base.InternalEmit(opcode);
            if (opcode.m_pop == StackBehaviour.Varpop)
            {
                stackchange -= signature.ArgumentCount;
                stackchange--;
                base.UpdateStackSize(opcode, stackchange);
            }
            int num2 = this.AddSignature(signature.GetSignature(true));
            base.PutInteger4(num2);
        }

        public override void Emit(OpCode opcode, FieldInfo field)
        {
            if (field == null)
            {
                throw new ArgumentNullException("field");
            }
            if (!(field is RuntimeFieldInfo))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeFieldInfo"), "field");
            }
            if (field.DeclaringType == null)
            {
                this.Emit(opcode, field.FieldHandle);
            }
            else
            {
                this.Emit(opcode, field.FieldHandle, field.DeclaringType.GetTypeHandleInternal());
            }
        }

        [SecuritySafeCritical]
        public override void Emit(OpCode opcode, MethodInfo meth)
        {
            if (meth == null)
            {
                throw new ArgumentNullException("meth");
            }
            int stackchange = 0;
            int tokenFor = 0;
            DynamicMethod method = meth as DynamicMethod;
            if (method == null)
            {
                if (!(meth is RuntimeMethodInfo))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"), "meth");
                }
                if ((meth.DeclaringType != null) && (meth.DeclaringType.IsGenericType || meth.DeclaringType.IsArray))
                {
                    tokenFor = this.m_scope.GetTokenFor(meth.MethodHandle, meth.DeclaringType.TypeHandle);
                }
                else
                {
                    tokenFor = this.m_scope.GetTokenFor(meth.MethodHandle);
                }
            }
            else
            {
                if ((opcode.Equals(OpCodes.Ldtoken) || opcode.Equals(OpCodes.Ldftn)) || opcode.Equals(OpCodes.Ldvirtftn))
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_InvalidOpCodeOnDynamicMethod"));
                }
                tokenFor = this.m_scope.GetTokenFor(method);
            }
            base.EnsureCapacity(7);
            base.InternalEmit(opcode);
            if ((opcode.m_push == StackBehaviour.Varpush) && (meth.ReturnType != typeof(void)))
            {
                stackchange++;
            }
            if (opcode.m_pop == StackBehaviour.Varpop)
            {
                stackchange -= meth.GetParametersNoCopy().Length;
            }
            if ((!meth.IsStatic && !opcode.Equals(OpCodes.Newobj)) && (!opcode.Equals(OpCodes.Ldtoken) && !opcode.Equals(OpCodes.Ldftn)))
            {
                stackchange--;
            }
            base.UpdateStackSize(opcode, stackchange);
            base.PutInteger4(tokenFor);
        }

        [SecuritySafeCritical]
        public void Emit(OpCode opcode, RuntimeFieldHandle fieldHandle)
        {
            if (fieldHandle.IsNullHandle())
            {
                throw new ArgumentNullException("fieldHandle");
            }
            int tokenFor = this.m_scope.GetTokenFor(fieldHandle);
            base.EnsureCapacity(7);
            base.InternalEmit(opcode);
            base.PutInteger4(tokenFor);
        }

        [SecuritySafeCritical]
        public void Emit(OpCode opcode, RuntimeMethodHandle meth)
        {
            if (meth.IsNullHandle())
            {
                throw new ArgumentNullException("meth");
            }
            int tokenFor = this.m_scope.GetTokenFor(meth);
            base.EnsureCapacity(7);
            base.InternalEmit(opcode);
            base.UpdateStackSize(opcode, 1);
            base.PutInteger4(tokenFor);
        }

        public void Emit(OpCode opcode, RuntimeTypeHandle typeHandle)
        {
            if (typeHandle.IsNullHandle())
            {
                throw new ArgumentNullException("typeHandle");
            }
            int tokenFor = this.m_scope.GetTokenFor(typeHandle);
            base.EnsureCapacity(7);
            base.InternalEmit(opcode);
            base.PutInteger4(tokenFor);
        }

        public override void Emit(OpCode opcode, string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException("str");
            }
            int num = this.AddStringLiteral(str) | 0x70000000;
            base.EnsureCapacity(7);
            base.InternalEmit(opcode);
            base.PutInteger4(num);
        }

        [SecuritySafeCritical]
        public override void Emit(OpCode opcode, Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            this.Emit(opcode, type.TypeHandle);
        }

        [SecuritySafeCritical]
        public void Emit(OpCode opcode, RuntimeFieldHandle fieldHandle, RuntimeTypeHandle typeContext)
        {
            if (fieldHandle.IsNullHandle())
            {
                throw new ArgumentNullException("fieldHandle");
            }
            int tokenFor = this.m_scope.GetTokenFor(fieldHandle, typeContext);
            base.EnsureCapacity(7);
            base.InternalEmit(opcode);
            base.PutInteger4(tokenFor);
        }

        [SecuritySafeCritical]
        public void Emit(OpCode opcode, RuntimeMethodHandle meth, RuntimeTypeHandle typeContext)
        {
            if (meth.IsNullHandle())
            {
                throw new ArgumentNullException("meth");
            }
            if (typeContext.IsNullHandle())
            {
                throw new ArgumentNullException("typeContext");
            }
            int tokenFor = this.m_scope.GetTokenFor(meth, typeContext);
            base.EnsureCapacity(7);
            base.InternalEmit(opcode);
            base.UpdateStackSize(opcode, 1);
            base.PutInteger4(tokenFor);
        }

        [SecuritySafeCritical]
        public override void EmitCall(OpCode opcode, MethodInfo methodInfo, Type[] optionalParameterTypes)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException("methodInfo");
            }
            if ((!opcode.Equals(OpCodes.Call) && !opcode.Equals(OpCodes.Callvirt)) && !opcode.Equals(OpCodes.Newobj))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NotMethodCallOpcode"), "opcode");
            }
            if (methodInfo.ContainsGenericParameters)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_GenericsInvalid"), "methodInfo");
            }
            if ((methodInfo.DeclaringType != null) && methodInfo.DeclaringType.ContainsGenericParameters)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_GenericsInvalid"), "methodInfo");
            }
            int stackchange = 0;
            int memberRefToken = this.GetMemberRefToken(methodInfo, optionalParameterTypes);
            base.EnsureCapacity(7);
            base.InternalEmit(opcode);
            if (methodInfo.ReturnType != typeof(void))
            {
                stackchange++;
            }
            stackchange -= methodInfo.GetParameterTypes().Length;
            if ((!(methodInfo is SymbolMethod) && !methodInfo.IsStatic) && !opcode.Equals(OpCodes.Newobj))
            {
                stackchange--;
            }
            if (optionalParameterTypes != null)
            {
                stackchange -= optionalParameterTypes.Length;
            }
            base.UpdateStackSize(opcode, stackchange);
            base.PutInteger4(memberRefToken);
        }

        public override void EmitCalli(OpCode opcode, CallingConvention unmanagedCallConv, Type returnType, Type[] parameterTypes)
        {
            int stackchange = 0;
            int length = 0;
            if (parameterTypes != null)
            {
                length = parameterTypes.Length;
            }
            SignatureHelper methodSigHelper = SignatureHelper.GetMethodSigHelper(unmanagedCallConv, returnType);
            if (parameterTypes != null)
            {
                for (int i = 0; i < length; i++)
                {
                    methodSigHelper.AddArgument(parameterTypes[i]);
                }
            }
            if (returnType != typeof(void))
            {
                stackchange++;
            }
            if (parameterTypes != null)
            {
                stackchange -= length;
            }
            stackchange--;
            base.UpdateStackSize(OpCodes.Calli, stackchange);
            base.EnsureCapacity(7);
            this.Emit(OpCodes.Calli);
            int num4 = this.AddSignature(methodSigHelper.GetSignature(true));
            base.PutInteger4(num4);
        }

        [SecuritySafeCritical]
        public override void EmitCalli(OpCode opcode, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes)
        {
            int stackchange = 0;
            if ((optionalParameterTypes != null) && ((callingConvention & CallingConventions.VarArgs) == 0))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAVarArgCallingConvention"));
            }
            SignatureHelper helper = this.GetMemberRefSignature(callingConvention, returnType, parameterTypes, optionalParameterTypes);
            base.EnsureCapacity(7);
            this.Emit(OpCodes.Calli);
            if (returnType != typeof(void))
            {
                stackchange++;
            }
            if (parameterTypes != null)
            {
                stackchange -= parameterTypes.Length;
            }
            if (optionalParameterTypes != null)
            {
                stackchange -= optionalParameterTypes.Length;
            }
            if ((callingConvention & CallingConventions.HasThis) == CallingConventions.HasThis)
            {
                stackchange--;
            }
            stackchange--;
            base.UpdateStackSize(OpCodes.Calli, stackchange);
            int num2 = this.AddSignature(helper.GetSignature(true));
            base.PutInteger4(num2);
        }

        public override void EndExceptionBlock()
        {
            base.EndExceptionBlock();
        }

        public override void EndScope()
        {
            throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
        }

        [SecurityCritical]
        internal void GetCallableMethod(RuntimeModule module, DynamicMethod dm)
        {
            dm.m_methodHandle = ModuleHandle.GetDynamicMethod(dm, module, base.m_methodBuilder.Name, (byte[]) this.m_scope[this.m_methodSigToken], new DynamicResolver(this));
        }

        [SecurityCritical]
        internal override SignatureHelper GetMemberRefSignature(CallingConventions call, Type returnType, Type[] parameterTypes, Type[] optionalParameterTypes)
        {
            int length;
            int num2;
            if (parameterTypes == null)
            {
                length = 0;
            }
            else
            {
                length = parameterTypes.Length;
            }
            SignatureHelper methodSigHelper = SignatureHelper.GetMethodSigHelper(call, returnType);
            for (num2 = 0; num2 < length; num2++)
            {
                methodSigHelper.AddArgument(parameterTypes[num2]);
            }
            if ((optionalParameterTypes != null) && (optionalParameterTypes.Length != 0))
            {
                methodSigHelper.AddSentinel();
                for (num2 = 0; num2 < optionalParameterTypes.Length; num2++)
                {
                    methodSigHelper.AddArgument(optionalParameterTypes[num2]);
                }
            }
            return methodSigHelper;
        }

        [SecurityCritical]
        internal override int GetMemberRefToken(MethodBase methodInfo, Type[] optionalParameterTypes)
        {
            Type[] typeArray;
            if ((optionalParameterTypes != null) && ((methodInfo.CallingConvention & CallingConventions.VarArgs) == 0))
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotAVarArgCallingConvention"));
            }
            if (!((methodInfo is RuntimeMethodInfo) || (methodInfo is DynamicMethod)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeMethodInfo"), "methodInfo");
            }
            ParameterInfo[] parametersNoCopy = methodInfo.GetParametersNoCopy();
            if ((parametersNoCopy != null) && (parametersNoCopy.Length != 0))
            {
                typeArray = new Type[parametersNoCopy.Length];
                for (int i = 0; i < parametersNoCopy.Length; i++)
                {
                    typeArray[i] = parametersNoCopy[i].ParameterType;
                }
            }
            else
            {
                typeArray = null;
            }
            SignatureHelper signature = this.GetMemberRefSignature(methodInfo.CallingConvention, MethodBuilder.GetMethodBaseReturnType(methodInfo), typeArray, optionalParameterTypes);
            return this.m_scope.GetTokenFor(new VarArgMethod(methodInfo as RuntimeMethodInfo, methodInfo as DynamicMethod, signature));
        }

        public override void MarkSequencePoint(ISymbolDocumentWriter document, int startLine, int startColumn, int endLine, int endColumn)
        {
            throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
        }

        public override void UsingNamespace(string ns)
        {
            throw new NotSupportedException(Environment.GetResourceString("InvalidOperation_NotAllowedInDynamicMethod"));
        }
    }
}

