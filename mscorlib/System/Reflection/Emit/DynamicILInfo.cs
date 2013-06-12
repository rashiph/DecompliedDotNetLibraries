namespace System.Reflection.Emit
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security;

    [ComVisible(true)]
    public class DynamicILInfo
    {
        private byte[] m_code;
        private byte[] m_exceptions;
        private byte[] m_localSignature;
        private int m_maxStackSize;
        private System.Reflection.Emit.DynamicMethod m_method;
        private int m_methodSignature;
        private System.Reflection.Emit.DynamicScope m_scope;

        internal DynamicILInfo(System.Reflection.Emit.DynamicScope scope, System.Reflection.Emit.DynamicMethod method, byte[] methodSignature)
        {
            this.m_method = method;
            this.m_scope = scope;
            this.m_methodSignature = this.m_scope.GetTokenFor(methodSignature);
            this.m_exceptions = new byte[0];
            this.m_code = new byte[0];
            this.m_localSignature = new byte[0];
        }

        [SecurityCritical]
        internal void GetCallableMethod(RuntimeModule module, System.Reflection.Emit.DynamicMethod dm)
        {
            dm.m_methodHandle = ModuleHandle.GetDynamicMethod(dm, module, this.m_method.Name, (byte[]) this.m_scope[this.m_methodSignature], new DynamicResolver(this));
        }

        public int GetTokenFor(System.Reflection.Emit.DynamicMethod method)
        {
            return this.DynamicScope.GetTokenFor(method);
        }

        public int GetTokenFor(RuntimeFieldHandle field)
        {
            return this.DynamicScope.GetTokenFor(field);
        }

        [SecuritySafeCritical]
        public int GetTokenFor(RuntimeMethodHandle method)
        {
            return this.DynamicScope.GetTokenFor(method);
        }

        public int GetTokenFor(RuntimeTypeHandle type)
        {
            return this.DynamicScope.GetTokenFor(type);
        }

        public int GetTokenFor(string literal)
        {
            return this.DynamicScope.GetTokenFor(literal);
        }

        public int GetTokenFor(byte[] signature)
        {
            return this.DynamicScope.GetTokenFor(signature);
        }

        public int GetTokenFor(RuntimeFieldHandle field, RuntimeTypeHandle contextType)
        {
            return this.DynamicScope.GetTokenFor(field, contextType);
        }

        public int GetTokenFor(RuntimeMethodHandle method, RuntimeTypeHandle contextType)
        {
            return this.DynamicScope.GetTokenFor(method, contextType);
        }

        public void SetCode(byte[] code, int maxStackSize)
        {
            if (code == null)
            {
                code = new byte[0];
            }
            this.m_code = (byte[]) code.Clone();
            this.m_maxStackSize = maxStackSize;
        }

        [SecurityCritical, CLSCompliant(false)]
        public unsafe void SetCode(byte* code, int codeSize, int maxStackSize)
        {
            if (codeSize < 0)
            {
                throw new ArgumentOutOfRangeException("codeSize", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            if ((codeSize > 0) && (code == null))
            {
                throw new ArgumentNullException("code");
            }
            this.m_code = new byte[codeSize];
            for (int i = 0; i < codeSize; i++)
            {
                this.m_code[i] = code[0];
                code++;
            }
            this.m_maxStackSize = maxStackSize;
        }

        public void SetExceptions(byte[] exceptions)
        {
            if (exceptions == null)
            {
                exceptions = new byte[0];
            }
            this.m_exceptions = (byte[]) exceptions.Clone();
        }

        [CLSCompliant(false), SecurityCritical]
        public unsafe void SetExceptions(byte* exceptions, int exceptionsSize)
        {
            if (exceptionsSize < 0)
            {
                throw new ArgumentOutOfRangeException("exceptionsSize", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            if ((exceptionsSize > 0) && (exceptions == null))
            {
                throw new ArgumentNullException("exceptions");
            }
            this.m_exceptions = new byte[exceptionsSize];
            for (int i = 0; i < exceptionsSize; i++)
            {
                this.m_exceptions[i] = exceptions[0];
                exceptions++;
            }
        }

        public void SetLocalSignature(byte[] localSignature)
        {
            if (localSignature == null)
            {
                localSignature = new byte[0];
            }
            this.m_localSignature = (byte[]) localSignature.Clone();
        }

        [SecurityCritical, CLSCompliant(false)]
        public unsafe void SetLocalSignature(byte* localSignature, int signatureSize)
        {
            if (signatureSize < 0)
            {
                throw new ArgumentOutOfRangeException("signatureSize", Environment.GetResourceString("ArgumentOutOfRange_GenericPositive"));
            }
            if ((signatureSize > 0) && (localSignature == null))
            {
                throw new ArgumentNullException("localSignature");
            }
            this.m_localSignature = new byte[signatureSize];
            for (int i = 0; i < signatureSize; i++)
            {
                this.m_localSignature[i] = localSignature[0];
                localSignature++;
            }
        }

        internal byte[] Code
        {
            get
            {
                return this.m_code;
            }
        }

        public System.Reflection.Emit.DynamicMethod DynamicMethod
        {
            get
            {
                return this.m_method;
            }
        }

        internal System.Reflection.Emit.DynamicScope DynamicScope
        {
            get
            {
                return this.m_scope;
            }
        }

        internal byte[] Exceptions
        {
            get
            {
                return this.m_exceptions;
            }
        }

        internal byte[] LocalSignature
        {
            get
            {
                if (this.m_localSignature == null)
                {
                    this.m_localSignature = SignatureHelper.GetLocalVarSigHelper().InternalGetSignatureArray();
                }
                return this.m_localSignature;
            }
        }

        internal int MaxStackSize
        {
            get
            {
                return this.m_maxStackSize;
            }
        }
    }
}

