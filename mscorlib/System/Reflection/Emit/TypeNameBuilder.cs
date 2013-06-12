namespace System.Reflection.Emit
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;

    internal class TypeNameBuilder
    {
        private IntPtr m_typeNameBuilder;

        private TypeNameBuilder(IntPtr typeNameBuilder)
        {
            this.m_typeNameBuilder = typeNameBuilder;
        }

        [SecurityCritical]
        private void AddArray(int rank)
        {
            AddArray(this.m_typeNameBuilder, rank);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void AddArray(IntPtr tnb, int rank);
        [SecurityCritical]
        private void AddAssemblySpec(string assemblySpec)
        {
            AddAssemblySpec(this.m_typeNameBuilder, assemblySpec);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void AddAssemblySpec(IntPtr tnb, string assemblySpec);
        [SecurityCritical]
        private void AddByRef()
        {
            AddByRef(this.m_typeNameBuilder);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void AddByRef(IntPtr tnb);
        [SecurityCritical]
        private void AddElementType(Type elementType)
        {
            if (elementType.HasElementType)
            {
                this.AddElementType(elementType.GetElementType());
            }
            if (elementType.IsPointer)
            {
                this.AddPointer();
            }
            else if (elementType.IsByRef)
            {
                this.AddByRef();
            }
            else if (elementType.IsSzArray)
            {
                this.AddSzArray();
            }
            else if (elementType.IsArray)
            {
                this.AddArray(elementType.GetArrayRank());
            }
        }

        [SecurityCritical]
        private void AddName(string name)
        {
            AddName(this.m_typeNameBuilder, name);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void AddName(IntPtr tnb, string name);
        [SecurityCritical]
        private void AddPointer()
        {
            AddPointer(this.m_typeNameBuilder);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void AddPointer(IntPtr tnb);
        [SecurityCritical]
        private void AddSzArray()
        {
            AddSzArray(this.m_typeNameBuilder);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void AddSzArray(IntPtr tnb);
        [SecurityCritical]
        private void Clear()
        {
            Clear(this.m_typeNameBuilder);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void Clear(IntPtr tnb);
        [SecurityCritical]
        private void CloseGenericArgument()
        {
            CloseGenericArgument(this.m_typeNameBuilder);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void CloseGenericArgument(IntPtr tnb);
        [SecurityCritical]
        private void CloseGenericArguments()
        {
            CloseGenericArguments(this.m_typeNameBuilder);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void CloseGenericArguments(IntPtr tnb);
        [SecurityCritical]
        private void ConstructAssemblyQualifiedNameWorker(Type type, Format format)
        {
            Type elementType = type;
            while (elementType.HasElementType)
            {
                elementType = elementType.GetElementType();
            }
            List<Type> list = new List<Type>();
            for (Type type3 = elementType; type3 != null; type3 = type3.IsGenericParameter ? null : type3.DeclaringType)
            {
                list.Add(type3);
            }
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Type type4 = list[i];
                string name = type4.Name;
                if (((i == (list.Count - 1)) && (type4.Namespace != null)) && (type4.Namespace.Length != 0))
                {
                    name = type4.Namespace + "." + name;
                }
                this.AddName(name);
            }
            if (elementType.IsGenericType && (!elementType.IsGenericTypeDefinition || (format == Format.ToString)))
            {
                Type[] genericArguments = elementType.GetGenericArguments();
                this.OpenGenericArguments();
                for (int j = 0; j < genericArguments.Length; j++)
                {
                    Format format2 = (format == Format.FullName) ? Format.AssemblyQualifiedName : format;
                    this.OpenGenericArgument();
                    this.ConstructAssemblyQualifiedNameWorker(genericArguments[j], format2);
                    this.CloseGenericArgument();
                }
                this.CloseGenericArguments();
            }
            this.AddElementType(type);
            if (format == Format.AssemblyQualifiedName)
            {
                this.AddAssemblySpec(type.Module.Assembly.FullName);
            }
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern IntPtr CreateTypeNameBuilder();
        [SecurityCritical]
        internal void Dispose()
        {
            ReleaseTypeNameBuilder(this.m_typeNameBuilder);
        }

        [SecurityCritical]
        private void OpenGenericArgument()
        {
            OpenGenericArgument(this.m_typeNameBuilder);
        }

        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void OpenGenericArgument(IntPtr tnb);
        [SecurityCritical]
        private void OpenGenericArguments()
        {
            OpenGenericArguments(this.m_typeNameBuilder);
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void OpenGenericArguments(IntPtr tnb);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void ReleaseTypeNameBuilder(IntPtr pAQN);
        [SecuritySafeCritical]
        public override string ToString()
        {
            string s = null;
            ToString(this.m_typeNameBuilder, JitHelpers.GetStringHandleOnStack(ref s));
            return s;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void ToString(IntPtr tnb, StringHandleOnStack retString);
        [SecuritySafeCritical]
        internal static string ToString(Type type, Format format)
        {
            if (((format == Format.FullName) || (format == Format.AssemblyQualifiedName)) && (!type.IsGenericTypeDefinition && type.ContainsGenericParameters))
            {
                return null;
            }
            TypeNameBuilder builder = new TypeNameBuilder(CreateTypeNameBuilder());
            builder.Clear();
            builder.ConstructAssemblyQualifiedNameWorker(type, format);
            string str = builder.ToString();
            builder.Dispose();
            return str;
        }

        internal enum Format
        {
            ToString,
            FullName,
            AssemblyQualifiedName
        }
    }
}

