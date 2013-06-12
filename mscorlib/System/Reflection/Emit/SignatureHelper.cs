namespace System.Reflection.Emit
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;

    [ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(_SignatureHelper)), ComVisible(true)]
    public sealed class SignatureHelper : _SignatureHelper
    {
        private int m_argCount;
        private int m_currSig;
        private ModuleBuilder m_module;
        private bool m_sigDone;
        private byte[] m_signature;
        private int m_sizeLoc;
        private const int NO_SIZE_IN_SIG = -1;

        private SignatureHelper(Module mod, System.Reflection.MdSigCallingConvention callingConvention)
        {
            this.Init(mod, callingConvention);
        }

        [SecurityCritical]
        private SignatureHelper(Module mod, Type type)
        {
            this.Init(mod);
            this.AddOneArgTypeHelper(type);
        }

        [SecurityCritical]
        private SignatureHelper(Module mod, System.Reflection.MdSigCallingConvention callingConvention, Type returnType, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers) : this(mod, callingConvention, 0, returnType, requiredCustomModifiers, optionalCustomModifiers)
        {
        }

        [SecurityCritical]
        private SignatureHelper(Module mod, System.Reflection.MdSigCallingConvention callingConvention, int cGenericParameters, Type returnType, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
        {
            this.Init(mod, callingConvention, cGenericParameters);
            if (callingConvention == (System.Reflection.MdSigCallingConvention.Default | System.Reflection.MdSigCallingConvention.FastCall | System.Reflection.MdSigCallingConvention.StdCall))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_BadFieldSig"));
            }
            this.AddOneArgTypeHelper(returnType, requiredCustomModifiers, optionalCustomModifiers);
        }

        public void AddArgument(Type clsArgument)
        {
            this.AddArgument(clsArgument, null, null);
        }

        [SecuritySafeCritical]
        public void AddArgument(Type argument, bool pinned)
        {
            if (argument == null)
            {
                throw new ArgumentNullException("argument");
            }
            this.IncrementArgCounts();
            this.AddOneArgTypeHelper(argument, pinned);
        }

        [SecuritySafeCritical]
        public void AddArgument(Type argument, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
        {
            if (this.m_sigDone)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_SigIsFinalized"));
            }
            if (argument == null)
            {
                throw new ArgumentNullException("argument");
            }
            this.IncrementArgCounts();
            this.AddOneArgTypeHelper(argument, requiredCustomModifiers, optionalCustomModifiers);
        }

        public void AddArguments(Type[] arguments, Type[][] requiredCustomModifiers, Type[][] optionalCustomModifiers)
        {
            if ((requiredCustomModifiers != null) && ((arguments == null) || (requiredCustomModifiers.Length != arguments.Length)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MismatchedArrays", new object[] { "requiredCustomModifiers", "arguments" }));
            }
            if ((optionalCustomModifiers != null) && ((arguments == null) || (optionalCustomModifiers.Length != arguments.Length)))
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_MismatchedArrays", new object[] { "optionalCustomModifiers", "arguments" }));
            }
            if (arguments != null)
            {
                for (int i = 0; i < arguments.Length; i++)
                {
                    this.AddArgument(arguments[i], (requiredCustomModifiers == null) ? null : requiredCustomModifiers[i], (optionalCustomModifiers == null) ? null : optionalCustomModifiers[i]);
                }
            }
        }

        private void AddData(int data)
        {
            if ((this.m_currSig + 4) > this.m_signature.Length)
            {
                this.m_signature = this.ExpandArray(this.m_signature);
            }
            if (data <= 0x7f)
            {
                this.m_signature[this.m_currSig++] = (byte) (data & 0xff);
            }
            else if (data <= 0x3fff)
            {
                this.m_signature[this.m_currSig++] = (byte) ((data >> 8) | 0x80);
                this.m_signature[this.m_currSig++] = (byte) (data & 0xff);
            }
            else
            {
                if (data > 0x1fffffff)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_LargeInteger"));
                }
                this.m_signature[this.m_currSig++] = (byte) ((data >> 0x18) | 0xc0);
                this.m_signature[this.m_currSig++] = (byte) ((data >> 0x10) & 0xff);
                this.m_signature[this.m_currSig++] = (byte) ((data >> 8) & 0xff);
                this.m_signature[this.m_currSig++] = (byte) (data & 0xff);
            }
        }

        private void AddData(uint data)
        {
            if ((this.m_currSig + 4) > this.m_signature.Length)
            {
                this.m_signature = this.ExpandArray(this.m_signature);
            }
            this.m_signature[this.m_currSig++] = (byte) (data & 0xff);
            this.m_signature[this.m_currSig++] = (byte) ((data >> 8) & 0xff);
            this.m_signature[this.m_currSig++] = (byte) ((data >> 0x10) & 0xff);
            this.m_signature[this.m_currSig++] = (byte) ((data >> 0x18) & 0xff);
        }

        private void AddData(ulong data)
        {
            if ((this.m_currSig + 8) > this.m_signature.Length)
            {
                this.m_signature = this.ExpandArray(this.m_signature);
            }
            this.m_signature[this.m_currSig++] = (byte) (data & ((ulong) 0xffL));
            this.m_signature[this.m_currSig++] = (byte) ((data >> 8) & ((ulong) 0xffL));
            this.m_signature[this.m_currSig++] = (byte) ((data >> 0x10) & ((ulong) 0xffL));
            this.m_signature[this.m_currSig++] = (byte) ((data >> 0x18) & ((ulong) 0xffL));
            this.m_signature[this.m_currSig++] = (byte) ((data >> 0x20) & ((ulong) 0xffL));
            this.m_signature[this.m_currSig++] = (byte) ((data >> 40) & ((ulong) 0xffL));
            this.m_signature[this.m_currSig++] = (byte) ((data >> 0x30) & ((ulong) 0xffL));
            this.m_signature[this.m_currSig++] = (byte) ((data >> 0x38) & ((ulong) 0xffL));
        }

        private void AddElementType(CorElementType cvt)
        {
            if ((this.m_currSig + 1) > this.m_signature.Length)
            {
                this.m_signature = this.ExpandArray(this.m_signature);
            }
            this.m_signature[this.m_currSig++] = (byte) cvt;
        }

        [SecurityCritical]
        private void AddOneArgTypeHelper(Type clsArgument)
        {
            this.AddOneArgTypeHelperWorker(clsArgument, false);
        }

        [SecurityCritical]
        private void AddOneArgTypeHelper(Type argument, bool pinned)
        {
            if (pinned)
            {
                this.AddElementType(CorElementType.Pinned);
            }
            this.AddOneArgTypeHelper(argument);
        }

        [SecurityCritical]
        private void AddOneArgTypeHelper(Type clsArgument, Type[] requiredCustomModifiers, Type[] optionalCustomModifiers)
        {
            if (optionalCustomModifiers != null)
            {
                for (int i = 0; i < optionalCustomModifiers.Length; i++)
                {
                    Type type = optionalCustomModifiers[i];
                    if (type == null)
                    {
                        throw new ArgumentNullException("optionalCustomModifiers");
                    }
                    if (type.HasElementType)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_ArraysInvalid"), "optionalCustomModifiers");
                    }
                    if (type.ContainsGenericParameters)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_GenericsInvalid"), "optionalCustomModifiers");
                    }
                    this.AddElementType(CorElementType.CModOpt);
                    int token = this.m_module.GetTypeToken(type).Token;
                    this.AddToken(token);
                }
            }
            if (requiredCustomModifiers != null)
            {
                for (int j = 0; j < requiredCustomModifiers.Length; j++)
                {
                    Type type2 = requiredCustomModifiers[j];
                    if (type2 == null)
                    {
                        throw new ArgumentNullException("requiredCustomModifiers");
                    }
                    if (type2.HasElementType)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_ArraysInvalid"), "requiredCustomModifiers");
                    }
                    if (type2.ContainsGenericParameters)
                    {
                        throw new ArgumentException(Environment.GetResourceString("Argument_GenericsInvalid"), "requiredCustomModifiers");
                    }
                    this.AddElementType(CorElementType.CModReqd);
                    int num4 = this.m_module.GetTypeToken(type2).Token;
                    this.AddToken(num4);
                }
            }
            this.AddOneArgTypeHelper(clsArgument);
        }

        [SecurityCritical]
        private void AddOneArgTypeHelperWorker(Type clsArgument, bool lastWasGenericInst)
        {
            if (clsArgument.IsGenericParameter)
            {
                if (clsArgument.DeclaringMethod != null)
                {
                    this.AddElementType(CorElementType.MVar);
                }
                else
                {
                    this.AddElementType(CorElementType.Var);
                }
                this.AddData(clsArgument.GenericParameterPosition);
            }
            else if (clsArgument.IsGenericType && (!clsArgument.IsGenericTypeDefinition || !lastWasGenericInst))
            {
                this.AddElementType(CorElementType.GenericInst);
                this.AddOneArgTypeHelperWorker(clsArgument.GetGenericTypeDefinition(), true);
                Type[] genericArguments = clsArgument.GetGenericArguments();
                this.AddData(genericArguments.Length);
                foreach (Type type in genericArguments)
                {
                    this.AddOneArgTypeHelper(type);
                }
            }
            else if (clsArgument is TypeBuilder)
            {
                TypeToken typeToken;
                TypeBuilder builder = (TypeBuilder) clsArgument;
                if (builder.Module.Equals(this.m_module))
                {
                    typeToken = builder.TypeToken;
                }
                else
                {
                    typeToken = this.m_module.GetTypeToken(clsArgument);
                }
                if (clsArgument.IsValueType)
                {
                    this.InternalAddTypeToken(typeToken, CorElementType.ValueType);
                }
                else
                {
                    this.InternalAddTypeToken(typeToken, CorElementType.Class);
                }
            }
            else if (clsArgument is EnumBuilder)
            {
                TypeToken token2;
                TypeBuilder typeBuilder = ((EnumBuilder) clsArgument).m_typeBuilder;
                if (typeBuilder.Module.Equals(this.m_module))
                {
                    token2 = typeBuilder.TypeToken;
                }
                else
                {
                    token2 = this.m_module.GetTypeToken(clsArgument);
                }
                if (clsArgument.IsValueType)
                {
                    this.InternalAddTypeToken(token2, CorElementType.ValueType);
                }
                else
                {
                    this.InternalAddTypeToken(token2, CorElementType.Class);
                }
            }
            else if (clsArgument.IsByRef)
            {
                this.AddElementType(CorElementType.ByRef);
                clsArgument = clsArgument.GetElementType();
                this.AddOneArgTypeHelper(clsArgument);
            }
            else if (clsArgument.IsPointer)
            {
                this.AddElementType(CorElementType.Ptr);
                this.AddOneArgTypeHelper(clsArgument.GetElementType());
            }
            else if (clsArgument.IsArray)
            {
                if (clsArgument.IsSzArray)
                {
                    this.AddElementType(CorElementType.SzArray);
                    this.AddOneArgTypeHelper(clsArgument.GetElementType());
                }
                else
                {
                    this.AddElementType(CorElementType.Array);
                    this.AddOneArgTypeHelper(clsArgument.GetElementType());
                    int arrayRank = clsArgument.GetArrayRank();
                    this.AddData(arrayRank);
                    this.AddData(0);
                    this.AddData(arrayRank);
                    for (int i = 0; i < arrayRank; i++)
                    {
                        this.AddData(0);
                    }
                }
            }
            else
            {
                CorElementType max = CorElementType.Max;
                if (clsArgument.IsRuntimeType)
                {
                    max = RuntimeTypeHandle.GetCorElementType((RuntimeType) clsArgument);
                    if (max == CorElementType.Class)
                    {
                        if (clsArgument == typeof(object))
                        {
                            max = CorElementType.Object;
                        }
                        else if (clsArgument == typeof(string))
                        {
                            max = CorElementType.String;
                        }
                    }
                }
                if (IsSimpleType(max))
                {
                    this.AddElementType(max);
                }
                else if (this.m_module == null)
                {
                    this.InternalAddRuntimeType(clsArgument);
                }
                else if (clsArgument.IsValueType)
                {
                    this.InternalAddTypeToken(this.m_module.GetTypeToken(clsArgument), CorElementType.ValueType);
                }
                else
                {
                    this.InternalAddTypeToken(this.m_module.GetTypeToken(clsArgument), CorElementType.Class);
                }
            }
        }

        public void AddSentinel()
        {
            this.AddElementType(CorElementType.Sentinel);
        }

        private void AddToken(int token)
        {
            int data = token & 0xffffff;
            MetadataTokenType type = ((MetadataTokenType) token) & ((MetadataTokenType) (-16777216));
            if (data > 0x3ffffff)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_LargeInteger"));
            }
            data = data << 2;
            switch (type)
            {
                case MetadataTokenType.TypeRef:
                    data |= 1;
                    break;

                case MetadataTokenType.TypeSpec:
                    data |= 2;
                    break;
            }
            this.AddData(data);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is SignatureHelper))
            {
                return false;
            }
            SignatureHelper helper = (SignatureHelper) obj;
            if ((!helper.m_module.Equals(this.m_module) || (helper.m_currSig != this.m_currSig)) || ((helper.m_sizeLoc != this.m_sizeLoc) || (helper.m_sigDone != this.m_sigDone)))
            {
                return false;
            }
            for (int i = 0; i < this.m_currSig; i++)
            {
                if (this.m_signature[i] != helper.m_signature[i])
                {
                    return false;
                }
            }
            return true;
        }

        private byte[] ExpandArray(byte[] inArray)
        {
            return this.ExpandArray(inArray, inArray.Length * 2);
        }

        private byte[] ExpandArray(byte[] inArray, int requiredLength)
        {
            if (requiredLength < inArray.Length)
            {
                requiredLength = inArray.Length * 2;
            }
            byte[] destinationArray = new byte[requiredLength];
            Array.Copy(inArray, destinationArray, inArray.Length);
            return destinationArray;
        }

        public static SignatureHelper GetFieldSigHelper(Module mod)
        {
            return new SignatureHelper(mod, System.Reflection.MdSigCallingConvention.Default | System.Reflection.MdSigCallingConvention.FastCall | System.Reflection.MdSigCallingConvention.StdCall);
        }

        public override int GetHashCode()
        {
            int num = (this.m_module.GetHashCode() + this.m_currSig) + this.m_sizeLoc;
            if (this.m_sigDone)
            {
                num++;
            }
            for (int i = 0; i < this.m_currSig; i++)
            {
                num += this.m_signature[i].GetHashCode();
            }
            return num;
        }

        public static SignatureHelper GetLocalVarSigHelper()
        {
            return GetLocalVarSigHelper(null);
        }

        public static SignatureHelper GetLocalVarSigHelper(Module mod)
        {
            return new SignatureHelper(mod, System.Reflection.MdSigCallingConvention.C | System.Reflection.MdSigCallingConvention.FastCall | System.Reflection.MdSigCallingConvention.StdCall);
        }

        public static SignatureHelper GetMethodSigHelper(CallingConventions callingConvention, Type returnType)
        {
            return GetMethodSigHelper(null, callingConvention, returnType);
        }

        public static SignatureHelper GetMethodSigHelper(CallingConvention unmanagedCallingConvention, Type returnType)
        {
            return GetMethodSigHelper(null, unmanagedCallingConvention, returnType);
        }

        [SecuritySafeCritical]
        public static SignatureHelper GetMethodSigHelper(Module mod, CallingConventions callingConvention, Type returnType)
        {
            return GetMethodSigHelper(mod, callingConvention, returnType, null, null, null, null, null);
        }

        [SecuritySafeCritical]
        public static SignatureHelper GetMethodSigHelper(Module mod, CallingConvention unmanagedCallConv, Type returnType)
        {
            System.Reflection.MdSigCallingConvention c;
            if (returnType == null)
            {
                returnType = typeof(void);
            }
            if (unmanagedCallConv == CallingConvention.Cdecl)
            {
                c = System.Reflection.MdSigCallingConvention.C;
            }
            else if ((unmanagedCallConv == CallingConvention.StdCall) || (unmanagedCallConv == CallingConvention.Winapi))
            {
                c = System.Reflection.MdSigCallingConvention.Default | System.Reflection.MdSigCallingConvention.StdCall;
            }
            else if (unmanagedCallConv == CallingConvention.ThisCall)
            {
                c = System.Reflection.MdSigCallingConvention.C | System.Reflection.MdSigCallingConvention.StdCall;
            }
            else
            {
                if (unmanagedCallConv != CallingConvention.FastCall)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_UnknownUnmanagedCallConv"), "unmanagedCallConv");
                }
                c = System.Reflection.MdSigCallingConvention.Default | System.Reflection.MdSigCallingConvention.FastCall;
            }
            return new SignatureHelper(mod, c, returnType, null, null);
        }

        [SecuritySafeCritical]
        public static SignatureHelper GetMethodSigHelper(Module mod, Type returnType, Type[] parameterTypes)
        {
            return GetMethodSigHelper(mod, CallingConventions.Standard, returnType, null, null, parameterTypes, null, null);
        }

        [SecurityCritical]
        internal static SignatureHelper GetMethodSigHelper(Module mod, CallingConventions callingConvention, Type returnType, int cGenericParam)
        {
            return GetMethodSigHelper(mod, callingConvention, cGenericParam, returnType, null, null, null, null, null);
        }

        [SecurityCritical]
        internal static SignatureHelper GetMethodSigHelper(Module scope, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
        {
            return GetMethodSigHelper(scope, callingConvention, 0, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
        }

        [SecurityCritical]
        internal static SignatureHelper GetMethodSigHelper(Module scope, CallingConventions callingConvention, int cGenericParam, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
        {
            if (returnType == null)
            {
                returnType = typeof(void);
            }
            System.Reflection.MdSigCallingConvention convention = System.Reflection.MdSigCallingConvention.Default;
            if ((callingConvention & CallingConventions.VarArgs) == CallingConventions.VarArgs)
            {
                convention = System.Reflection.MdSigCallingConvention.C | System.Reflection.MdSigCallingConvention.FastCall;
            }
            if (cGenericParam > 0)
            {
                convention = (System.Reflection.MdSigCallingConvention) ((byte) (convention | (System.Reflection.MdSigCallingConvention.Default | System.Reflection.MdSigCallingConvention.Generic)));
            }
            if ((callingConvention & CallingConventions.HasThis) == CallingConventions.HasThis)
            {
                convention = (System.Reflection.MdSigCallingConvention) ((byte) (convention | (System.Reflection.MdSigCallingConvention.Default | System.Reflection.MdSigCallingConvention.HasThis)));
            }
            SignatureHelper helper = new SignatureHelper(scope, convention, cGenericParam, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers);
            helper.AddArguments(parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
            return helper;
        }

        internal static SignatureHelper GetMethodSpecSigHelper(Module scope, Type[] inst)
        {
            SignatureHelper helper = new SignatureHelper(scope, System.Reflection.MdSigCallingConvention.Default | System.Reflection.MdSigCallingConvention.GenericInst);
            helper.AddData(inst.Length);
            foreach (Type type in inst)
            {
                helper.AddArgument(type);
            }
            return helper;
        }

        public static SignatureHelper GetPropertySigHelper(Module mod, Type returnType, Type[] parameterTypes)
        {
            return GetPropertySigHelper(mod, returnType, null, null, parameterTypes, null, null);
        }

        [SecuritySafeCritical]
        public static SignatureHelper GetPropertySigHelper(Module mod, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
        {
            return GetPropertySigHelper(mod, 0, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers, parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
        }

        [SecuritySafeCritical]
        public static SignatureHelper GetPropertySigHelper(Module mod, CallingConventions callingConvention, Type returnType, Type[] requiredReturnTypeCustomModifiers, Type[] optionalReturnTypeCustomModifiers, Type[] parameterTypes, Type[][] requiredParameterTypeCustomModifiers, Type[][] optionalParameterTypeCustomModifiers)
        {
            if (returnType == null)
            {
                returnType = typeof(void);
            }
            System.Reflection.MdSigCallingConvention convention = System.Reflection.MdSigCallingConvention.Default | System.Reflection.MdSigCallingConvention.Property;
            if ((callingConvention & CallingConventions.HasThis) == CallingConventions.HasThis)
            {
                convention = (System.Reflection.MdSigCallingConvention) ((byte) (convention | (System.Reflection.MdSigCallingConvention.Default | System.Reflection.MdSigCallingConvention.HasThis)));
            }
            SignatureHelper helper = new SignatureHelper(mod, convention, returnType, requiredReturnTypeCustomModifiers, optionalReturnTypeCustomModifiers);
            helper.AddArguments(parameterTypes, requiredParameterTypeCustomModifiers, optionalParameterTypeCustomModifiers);
            return helper;
        }

        public byte[] GetSignature()
        {
            return this.GetSignature(false);
        }

        internal byte[] GetSignature(bool appendEndOfSig)
        {
            if (!this.m_sigDone)
            {
                if (appendEndOfSig)
                {
                    this.AddElementType(CorElementType.End);
                }
                this.SetNumberOfSignatureElements(true);
                this.m_sigDone = true;
            }
            if (this.m_signature.Length > this.m_currSig)
            {
                byte[] destinationArray = new byte[this.m_currSig];
                Array.Copy(this.m_signature, destinationArray, this.m_currSig);
                this.m_signature = destinationArray;
            }
            return this.m_signature;
        }

        [SecurityCritical]
        internal static SignatureHelper GetTypeSigToken(Module mod, Type type)
        {
            if (mod == null)
            {
                throw new ArgumentNullException("module");
            }
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            return new SignatureHelper(mod, type);
        }

        private void IncrementArgCounts()
        {
            if (this.m_sizeLoc != -1)
            {
                this.m_argCount++;
            }
        }

        private void Init(Module mod)
        {
            this.m_signature = new byte[0x20];
            this.m_currSig = 0;
            this.m_module = mod as ModuleBuilder;
            this.m_argCount = 0;
            this.m_sigDone = false;
            this.m_sizeLoc = -1;
            if ((this.m_module == null) && (mod != null))
            {
                throw new ArgumentException(Environment.GetResourceString("NotSupported_MustBeModuleBuilder"));
            }
        }

        private void Init(Module mod, System.Reflection.MdSigCallingConvention callingConvention)
        {
            this.Init(mod, callingConvention, 0);
        }

        private void Init(Module mod, System.Reflection.MdSigCallingConvention callingConvention, int cGenericParam)
        {
            this.Init(mod);
            this.AddData((int) callingConvention);
            if ((callingConvention == (System.Reflection.MdSigCallingConvention.Default | System.Reflection.MdSigCallingConvention.FastCall | System.Reflection.MdSigCallingConvention.StdCall)) || (callingConvention == (System.Reflection.MdSigCallingConvention.Default | System.Reflection.MdSigCallingConvention.GenericInst)))
            {
                this.m_sizeLoc = -1;
            }
            else
            {
                if (cGenericParam > 0)
                {
                    this.AddData(cGenericParam);
                }
                this.m_sizeLoc = this.m_currSig++;
            }
        }

        [SecurityCritical]
        private unsafe void InternalAddRuntimeType(Type type)
        {
            this.AddElementType(CorElementType.Internal);
            void* voidPtr = (void*) type.GetTypeHandleInternal().Value;
            if ((this.m_currSig + sizeof(void*)) > this.m_signature.Length)
            {
                this.m_signature = this.ExpandArray(this.m_signature);
            }
            byte* numPtr = (byte*) &voidPtr;
            for (int i = 0; i < sizeof(void*); i++)
            {
                this.m_signature[this.m_currSig++] = numPtr[i];
            }
        }

        private void InternalAddTypeToken(TypeToken clsToken, CorElementType CorType)
        {
            this.AddElementType(CorType);
            this.AddToken(clsToken.Token);
        }

        internal byte[] InternalGetSignature(out int length)
        {
            if (!this.m_sigDone)
            {
                this.m_sigDone = true;
                this.SetNumberOfSignatureElements(false);
            }
            length = this.m_currSig;
            return this.m_signature;
        }

        internal byte[] InternalGetSignatureArray()
        {
            int argCount = this.m_argCount;
            int currSig = this.m_currSig;
            int num3 = currSig;
            if (argCount < 0x7f)
            {
                num3++;
            }
            else if (argCount < 0x3fff)
            {
                num3 += 2;
            }
            else
            {
                num3 += 4;
            }
            byte[] destinationArray = new byte[num3];
            int destinationIndex = 0;
            destinationArray[destinationIndex++] = this.m_signature[0];
            if (argCount <= 0x7f)
            {
                destinationArray[destinationIndex++] = (byte) (argCount & 0xff);
            }
            else if (argCount <= 0x3fff)
            {
                destinationArray[destinationIndex++] = (byte) ((argCount >> 8) | 0x80);
                destinationArray[destinationIndex++] = (byte) (argCount & 0xff);
            }
            else
            {
                if (argCount > 0x1fffffff)
                {
                    throw new ArgumentException(Environment.GetResourceString("Argument_LargeInteger"));
                }
                destinationArray[destinationIndex++] = (byte) ((argCount >> 0x18) | 0xc0);
                destinationArray[destinationIndex++] = (byte) ((argCount >> 0x10) & 0xff);
                destinationArray[destinationIndex++] = (byte) ((argCount >> 8) & 0xff);
                destinationArray[destinationIndex++] = (byte) (argCount & 0xff);
            }
            Array.Copy(this.m_signature, 2, destinationArray, destinationIndex, currSig - 2);
            destinationArray[num3 - 1] = 0;
            return destinationArray;
        }

        internal static bool IsSimpleType(CorElementType type)
        {
            if ((type > CorElementType.String) && (((type != CorElementType.TypedByRef) && (type != CorElementType.I)) && ((type != CorElementType.U) && (type != CorElementType.Object))))
            {
                return false;
            }
            return true;
        }

        private void SetNumberOfSignatureElements(bool forceCopy)
        {
            int currSig = this.m_currSig;
            if (this.m_sizeLoc != -1)
            {
                if ((this.m_argCount < 0x80) && !forceCopy)
                {
                    this.m_signature[this.m_sizeLoc] = (byte) this.m_argCount;
                }
                else
                {
                    int num;
                    if (this.m_argCount < 0x80)
                    {
                        num = 1;
                    }
                    else if (this.m_argCount < 0x4000)
                    {
                        num = 2;
                    }
                    else
                    {
                        num = 4;
                    }
                    byte[] destinationArray = new byte[(this.m_currSig + num) - 1];
                    destinationArray[0] = this.m_signature[0];
                    Array.Copy(this.m_signature, (int) (this.m_sizeLoc + 1), destinationArray, (int) (this.m_sizeLoc + num), (int) (currSig - (this.m_sizeLoc + 1)));
                    this.m_signature = destinationArray;
                    this.m_currSig = this.m_sizeLoc;
                    this.AddData(this.m_argCount);
                    this.m_currSig = currSig + (num - 1);
                }
            }
        }

        void _SignatureHelper.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _SignatureHelper.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _SignatureHelper.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _SignatureHelper.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("Length: " + this.m_currSig + Environment.NewLine);
            if (this.m_sizeLoc != -1)
            {
                builder.Append("Arguments: " + this.m_signature[this.m_sizeLoc] + Environment.NewLine);
            }
            else
            {
                builder.Append("Field Signature" + Environment.NewLine);
            }
            builder.Append("Signature: " + Environment.NewLine);
            for (int i = 0; i <= this.m_currSig; i++)
            {
                builder.Append(this.m_signature[i] + "  ");
            }
            builder.Append(Environment.NewLine);
            return builder.ToString();
        }

        internal int ArgumentCount
        {
            get
            {
                return this.m_argCount;
            }
        }
    }
}

