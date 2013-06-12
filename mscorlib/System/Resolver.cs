namespace System
{
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Threading;

    internal abstract class Resolver
    {
        internal const int COR_ILEXCEPTION_CLAUSE_CACHED_CLASS = 0x10000000;
        internal const int COR_ILEXCEPTION_CLAUSE_MUST_CACHE_CLASS = 0x20000000;
        internal const int FieldToken = 4;
        internal const int MethodToken = 2;
        internal const int TypeToken = 1;

        protected Resolver()
        {
        }

        internal abstract byte[] GetCodeInfo(ref int stackSize, ref int initLocals, ref int EHCount);
        internal abstract MethodInfo GetDynamicMethod();
        [SecurityCritical]
        internal abstract unsafe void GetEHInfo(int EHNumber, void* exception);
        internal abstract RuntimeType GetJitContext(ref int securityControlFlags);
        internal abstract byte[] GetLocalsSignature();
        internal abstract byte[] GetRawEHInfo();
        internal abstract CompressedStack GetSecurityContext();
        internal abstract string GetStringLiteral(int token);
        internal abstract int IsValidToken(int token);
        internal abstract int ParentToken(int token);
        internal abstract byte[] ResolveSignature(int token, int fromMethod);
        [SecurityCritical]
        internal abstract unsafe void* ResolveToken(int token);

        [StructLayout(LayoutKind.Sequential)]
        internal struct CORINFO_EH_CLAUSE
        {
            internal int Flags;
            internal int TryOffset;
            internal int TryLength;
            internal int HandlerOffset;
            internal int HandlerLength;
            internal int ClassTokenOrFilterOffset;
        }
    }
}

