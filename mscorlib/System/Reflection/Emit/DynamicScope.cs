namespace System.Reflection.Emit
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Security;

    internal class DynamicScope
    {
        internal List<object> m_tokens = new List<object>();

        internal DynamicScope()
        {
            this.m_tokens.Add(null);
        }

        internal string GetString(int token)
        {
            return (this[token] as string);
        }

        public int GetTokenFor(DynamicMethod method)
        {
            this.m_tokens.Add(method);
            return ((this.m_tokens.Count - 1) | 0x6000000);
        }

        internal int GetTokenFor(VarArgMethod varArgMethod)
        {
            this.m_tokens.Add(varArgMethod);
            return ((this.m_tokens.Count - 1) | 0xa000000);
        }

        public int GetTokenFor(RuntimeFieldHandle field)
        {
            this.m_tokens.Add(field);
            return ((this.m_tokens.Count - 1) | 0x4000000);
        }

        [SecurityCritical]
        public int GetTokenFor(RuntimeMethodHandle method)
        {
            IRuntimeMethodInfo methodInfo = method.GetMethodInfo();
            RuntimeMethodHandleInternal internal2 = methodInfo.Value;
            if ((methodInfo != null) && !RuntimeMethodHandle.IsDynamicMethod(internal2))
            {
                RuntimeType declaringType = RuntimeMethodHandle.GetDeclaringType(internal2);
                if ((declaringType != null) && RuntimeTypeHandle.IsGenericType(declaringType))
                {
                    MethodBase methodBase = RuntimeType.GetMethodBase(methodInfo);
                    Type genericTypeDefinition = methodBase.DeclaringType.GetGenericTypeDefinition();
                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Environment.GetResourceString("Argument_MethodDeclaringTypeGenericLcg"), new object[] { methodBase, genericTypeDefinition }));
                }
            }
            this.m_tokens.Add(method);
            return ((this.m_tokens.Count - 1) | 0x6000000);
        }

        public int GetTokenFor(RuntimeTypeHandle type)
        {
            this.m_tokens.Add(type);
            return ((this.m_tokens.Count - 1) | 0x2000000);
        }

        public int GetTokenFor(string literal)
        {
            this.m_tokens.Add(literal);
            return ((this.m_tokens.Count - 1) | 0x70000000);
        }

        public int GetTokenFor(byte[] signature)
        {
            this.m_tokens.Add(signature);
            return ((this.m_tokens.Count - 1) | 0x11000000);
        }

        public int GetTokenFor(RuntimeFieldHandle field, RuntimeTypeHandle typeContext)
        {
            this.m_tokens.Add(new GenericFieldInfo(field, typeContext));
            return ((this.m_tokens.Count - 1) | 0x4000000);
        }

        public int GetTokenFor(RuntimeMethodHandle method, RuntimeTypeHandle typeContext)
        {
            this.m_tokens.Add(new GenericMethodInfo(method, typeContext));
            return ((this.m_tokens.Count - 1) | 0x6000000);
        }

        internal byte[] ResolveSignature(int token, int fromMethod)
        {
            if (fromMethod == 0)
            {
                return (byte[]) this[token];
            }
            VarArgMethod method = this[token] as VarArgMethod;
            if (method == null)
            {
                return null;
            }
            return method.m_signature.GetSignature(true);
        }

        internal object this[int token]
        {
            get
            {
                token &= 0xffffff;
                if ((token >= 0) && (token <= this.m_tokens.Count))
                {
                    return this.m_tokens[token];
                }
                return null;
            }
        }
    }
}

