namespace System.Reflection.Emit
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true), ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(_LocalBuilder))]
    public sealed class LocalBuilder : LocalVariableInfo, _LocalBuilder
    {
        private bool m_isPinned;
        private int m_localIndex;
        private Type m_localType;
        private MethodInfo m_methodBuilder;

        private LocalBuilder()
        {
        }

        internal LocalBuilder(int localIndex, Type localType, MethodInfo methodBuilder) : this(localIndex, localType, methodBuilder, false)
        {
        }

        internal LocalBuilder(int localIndex, Type localType, MethodInfo methodBuilder, bool isPinned)
        {
            this.m_isPinned = isPinned;
            this.m_localIndex = localIndex;
            this.m_localType = localType;
            this.m_methodBuilder = methodBuilder;
        }

        internal int GetLocalIndex()
        {
            return this.m_localIndex;
        }

        internal MethodInfo GetMethodBuilder()
        {
            return this.m_methodBuilder;
        }

        public void SetLocalSymInfo(string name)
        {
            this.SetLocalSymInfo(name, 0, 0);
        }

        [SecuritySafeCritical]
        public void SetLocalSymInfo(string name, int startOffset, int endOffset)
        {
            int num;
            MethodBuilder methodBuilder = this.m_methodBuilder as MethodBuilder;
            if (methodBuilder == null)
            {
                throw new NotSupportedException();
            }
            ModuleBuilder module = (ModuleBuilder) methodBuilder.Module;
            if (methodBuilder.IsTypeCreated())
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_TypeHasBeenCreated"));
            }
            if (module.GetSymWriter() == null)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_NotADebugModule"));
            }
            SignatureHelper fieldSigHelper = SignatureHelper.GetFieldSigHelper(module);
            fieldSigHelper.AddArgument(this.m_localType);
            byte[] signature = fieldSigHelper.InternalGetSignature(out num);
            byte[] destinationArray = new byte[num - 1];
            Array.Copy(signature, 1, destinationArray, 0, num - 1);
            if (methodBuilder.GetILGenerator().m_ScopeTree.GetCurrentActiveScopeIndex() == -1)
            {
                methodBuilder.m_localSymInfo.AddLocalSymInfo(name, destinationArray, this.m_localIndex, startOffset, endOffset);
            }
            else
            {
                methodBuilder.GetILGenerator().m_ScopeTree.AddLocalSymInfoToCurrentScope(name, destinationArray, this.m_localIndex, startOffset, endOffset);
            }
        }

        void _LocalBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _LocalBuilder.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _LocalBuilder.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _LocalBuilder.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public override bool IsPinned
        {
            get
            {
                return this.m_isPinned;
            }
        }

        public override int LocalIndex
        {
            get
            {
                return this.m_localIndex;
            }
        }

        public override Type LocalType
        {
            get
            {
                return this.m_localType;
            }
        }
    }
}

