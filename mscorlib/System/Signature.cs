namespace System
{
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class Signature
    {
        internal SignatureStruct m_signature;

        [SecurityCritical]
        public Signature(IRuntimeFieldInfo fieldHandle, RuntimeType declaringType)
        {
            SignatureStruct signature = new SignatureStruct();
            GetSignature(ref signature, null, 0, fieldHandle.Value, null, declaringType);
            GC.KeepAlive(fieldHandle);
            this.m_signature = signature;
        }

        [SecuritySafeCritical]
        public Signature(IRuntimeMethodInfo methodHandle, RuntimeType declaringType)
        {
            SignatureStruct signature = new SignatureStruct();
            GetSignature(ref signature, null, 0, new RuntimeFieldHandleInternal(), methodHandle, declaringType);
            this.m_signature = signature;
        }

        [SecurityCritical]
        public unsafe Signature(void* pCorSig, int cCorSig, RuntimeType declaringType)
        {
            SignatureStruct signature = new SignatureStruct();
            GetSignature(ref signature, pCorSig, cCorSig, new RuntimeFieldHandleInternal(), null, declaringType);
            this.m_signature = signature;
        }

        public Signature(IRuntimeMethodInfo method, RuntimeType[] arguments, RuntimeType returnType, CallingConventions callingConvention)
        {
            SignatureStruct signature = new SignatureStruct(method.Value, arguments, returnType, callingConvention);
            GetSignatureForDynamicMethod(ref signature, method);
            this.m_signature = signature;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern bool CompareSig(ref SignatureStruct left, ref SignatureStruct right);
        [SecuritySafeCritical]
        internal static bool DiffSigs(Signature sig1, Signature sig2)
        {
            SignatureStruct left = (SignatureStruct) sig1;
            SignatureStruct right = (SignatureStruct) sig2;
            return CompareSig(ref left, ref right);
        }

        [SecuritySafeCritical]
        public Type[] GetCustomModifiers(int position, bool required)
        {
            Type[] typeArray = null;
            Type[] optional = null;
            SignatureStruct signature = (SignatureStruct) this;
            GetCustomModifiers(ref signature, position, out typeArray, out optional);
            if (!required)
            {
                return optional;
            }
            return typeArray;
        }

        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern void GetCustomModifiers(ref SignatureStruct signature, int parameter, out Type[] required, out Type[] optional);
        [MethodImpl(MethodImplOptions.InternalCall), SecurityCritical]
        private static extern unsafe void GetSignature(ref SignatureStruct signature, void* pCorSig, int cCorSig, RuntimeFieldHandleInternal fieldHandle, IRuntimeMethodInfo methodHandle, RuntimeType declaringType);
        [SecuritySafeCritical]
        internal static void GetSignatureForDynamicMethod(ref SignatureStruct signature, IRuntimeMethodInfo methodHandle)
        {
            GetSignature(ref signature, null, 0, new RuntimeFieldHandleInternal(), methodHandle, null);
        }

        public static implicit operator SignatureStruct(Signature pThis)
        {
            return pThis.m_signature;
        }

        internal RuntimeType[] Arguments
        {
            get
            {
                return this.m_signature.m_arguments;
            }
        }

        internal CallingConventions CallingConvention
        {
            get
            {
                return (this.m_signature.m_managedCallingConvention & 0xff);
            }
        }

        internal RuntimeType FieldType
        {
            get
            {
                return this.m_signature.m_returnTypeORfieldType;
            }
        }

        internal RuntimeType ReturnType
        {
            get
            {
                return this.m_signature.m_returnTypeORfieldType;
            }
        }

        internal enum MdSigCallingConvention : byte
        {
            C = 1,
            CallConvMask = 15,
            Default = 0,
            ExplicitThis = 0x40,
            FastCall = 4,
            Field = 6,
            GenericInst = 10,
            Generics = 0x10,
            HasThis = 0x20,
            LocalSig = 7,
            Max = 11,
            Property = 8,
            StdCall = 2,
            ThisCall = 3,
            Unmgd = 9,
            Vararg = 5
        }
    }
}

