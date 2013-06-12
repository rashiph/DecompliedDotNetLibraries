namespace System
{
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Text;
    using System.Threading;

    internal sealed class TypeNameParser : IDisposable
    {
        private SafeTypeNameParserHandle m_NativeParser;
        private static readonly char[] SPECIAL_CHARS = new char[] { ',', '[', ']', '&', '*', '+', '\\' };

        [SecuritySafeCritical]
        private TypeNameParser(SafeTypeNameParserHandle handle)
        {
            this.m_NativeParser = handle;
        }

        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void _CreateTypeNameParser(string typeName, ObjectHandleOnStack retHandle, bool throwOnError);
        [SecurityCritical, SuppressUnmanagedCodeSecurity, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void _GetAssemblyName(SafeTypeNameParserHandle pTypeNameParser, StringHandleOnStack retString);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void _GetModifiers(SafeTypeNameParserHandle pTypeNameParser, ObjectHandleOnStack retArray);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void _GetNames(SafeTypeNameParserHandle pTypeNameParser, ObjectHandleOnStack retArray);
        [SuppressUnmanagedCodeSecurity, SecurityCritical, DllImport("QCall", CharSet=CharSet.Unicode)]
        private static extern void _GetTypeArguments(SafeTypeNameParserHandle pTypeNameParser, ObjectHandleOnStack retArray);
        [SecuritySafeCritical]
        private unsafe Type ConstructType(Func<AssemblyName, Assembly> assemblyResolver, Func<Assembly, string, bool, Type> typeResolver, bool throwOnError, bool ignoreCase, ref StackCrawlMark stackMark)
        {
            Assembly assembly = null;
            int[] numArray2;
            string assemblyName = this.GetAssemblyName();
            if (assemblyName.Length > 0)
            {
                assembly = ResolveAssembly(assemblyName, assemblyResolver, throwOnError, ref stackMark);
                if (assembly == null)
                {
                    return null;
                }
            }
            string[] names = this.GetNames();
            if (names == null)
            {
                if (throwOnError)
                {
                    throw new TypeLoadException(Environment.GetResourceString("Arg_TypeLoadNullStr"));
                }
                return null;
            }
            Type typeStart = ResolveType(assembly, names, typeResolver, throwOnError, ignoreCase, ref stackMark);
            if (typeStart == null)
            {
                return null;
            }
            SafeTypeNameParserHandle[] typeArguments = this.GetTypeArguments();
            Type[] genericArgs = null;
            if (typeArguments != null)
            {
                genericArgs = new Type[typeArguments.Length];
                for (int i = 0; i < typeArguments.Length; i++)
                {
                    using (TypeNameParser parser = new TypeNameParser(typeArguments[i]))
                    {
                        genericArgs[i] = parser.ConstructType(assemblyResolver, typeResolver, throwOnError, ignoreCase, ref stackMark);
                    }
                    if (genericArgs[i] == null)
                    {
                        return null;
                    }
                }
            }
            int[] modifiers = this.GetModifiers();
            if (((numArray2 = modifiers) == null) || (numArray2.Length == 0))
            {
                numRef = null;
                goto Label_00EE;
            }
            fixed (int* numRef = numArray2)
            {
                IntPtr ptr;
            Label_00EE:
                ptr = new IntPtr((void*) numRef);
                return RuntimeTypeHandle.GetTypeHelper(typeStart, genericArgs, ptr, (modifiers == null) ? 0 : modifiers.Length);
            }
        }

        [SecuritySafeCritical]
        private static SafeTypeNameParserHandle CreateTypeNameParser(string typeName, bool throwOnError)
        {
            SafeTypeNameParserHandle o = null;
            _CreateTypeNameParser(typeName, JitHelpers.GetObjectHandleOnStack<SafeTypeNameParserHandle>(ref o), throwOnError);
            return o;
        }

        [SecuritySafeCritical]
        public void Dispose()
        {
            this.m_NativeParser.Dispose();
        }

        private static string EscapeTypeName(string name)
        {
            if (name.IndexOfAny(SPECIAL_CHARS) < 0)
            {
                return name;
            }
            StringBuilder builder = new StringBuilder();
            foreach (char ch in name)
            {
                if (Array.IndexOf<char>(SPECIAL_CHARS, ch) >= 0)
                {
                    builder.Append('\\');
                }
                builder.Append(ch);
            }
            return builder.ToString();
        }

        [SecuritySafeCritical]
        private string GetAssemblyName()
        {
            string s = null;
            _GetAssemblyName(this.m_NativeParser, JitHelpers.GetStringHandleOnStack(ref s));
            return s;
        }

        [SecuritySafeCritical]
        private int[] GetModifiers()
        {
            int[] o = null;
            _GetModifiers(this.m_NativeParser, JitHelpers.GetObjectHandleOnStack<int[]>(ref o));
            return o;
        }

        [SecuritySafeCritical]
        private string[] GetNames()
        {
            string[] o = null;
            _GetNames(this.m_NativeParser, JitHelpers.GetObjectHandleOnStack<string[]>(ref o));
            return o;
        }

        [SecuritySafeCritical]
        internal static Type GetType(string typeName, Func<AssemblyName, Assembly> assemblyResolver, Func<Assembly, string, bool, Type> typeResolver, bool throwOnError, bool ignoreCase, ref StackCrawlMark stackMark)
        {
            if (typeName == null)
            {
                throw new ArgumentNullException("typeName");
            }
            if ((typeName.Length > 0) && (typeName[0] == '\0'))
            {
                throw new ArgumentException(Environment.GetResourceString("Format_StringZeroLength"));
            }
            Type type = null;
            SafeTypeNameParserHandle handle = CreateTypeNameParser(typeName, throwOnError);
            if (handle == null)
            {
                return type;
            }
            using (TypeNameParser parser = new TypeNameParser(handle))
            {
                return parser.ConstructType(assemblyResolver, typeResolver, throwOnError, ignoreCase, ref stackMark);
            }
        }

        [SecuritySafeCritical]
        private SafeTypeNameParserHandle[] GetTypeArguments()
        {
            SafeTypeNameParserHandle[] o = null;
            _GetTypeArguments(this.m_NativeParser, JitHelpers.GetObjectHandleOnStack<SafeTypeNameParserHandle[]>(ref o));
            return o;
        }

        [SecuritySafeCritical]
        private static Assembly ResolveAssembly(string asmName, Func<AssemblyName, Assembly> assemblyResolver, bool throwOnError, ref StackCrawlMark stackMark)
        {
            Assembly assembly = null;
            if (assemblyResolver == null)
            {
                if (throwOnError)
                {
                    return RuntimeAssembly.InternalLoad(asmName, null, ref stackMark, false);
                }
                try
                {
                    return RuntimeAssembly.InternalLoad(asmName, null, ref stackMark, false);
                }
                catch (FileNotFoundException)
                {
                    return null;
                }
            }
            assembly = assemblyResolver(new AssemblyName(asmName));
            if ((assembly == null) && throwOnError)
            {
                throw new FileNotFoundException(Environment.GetResourceString("FileNotFound_ResolveAssembly", new object[] { asmName }));
            }
            return assembly;
        }

        private static Type ResolveType(Assembly assembly, string[] names, Func<Assembly, string, bool, Type> typeResolver, bool throwOnError, bool ignoreCase, ref StackCrawlMark stackMark)
        {
            Type nestedType = null;
            string str = EscapeTypeName(names[0]);
            if (typeResolver != null)
            {
                nestedType = typeResolver(assembly, str, ignoreCase);
                if ((nestedType == null) && throwOnError)
                {
                    string message = (assembly == null) ? Environment.GetResourceString("TypeLoad_ResolveType", new object[] { str }) : Environment.GetResourceString("TypeLoad_ResolveTypeFromAssembly", new object[] { str, assembly.FullName });
                    throw new TypeLoadException(message);
                }
            }
            else if (assembly == null)
            {
                nestedType = RuntimeType.GetType(str, throwOnError, ignoreCase, false, ref stackMark);
            }
            else
            {
                nestedType = assembly.GetType(str, throwOnError, ignoreCase);
            }
            if (nestedType != null)
            {
                BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public;
                if (ignoreCase)
                {
                    bindingAttr |= BindingFlags.IgnoreCase;
                }
                for (int i = 1; i < names.Length; i++)
                {
                    nestedType = nestedType.GetNestedType(names[i], bindingAttr);
                    if (nestedType == null)
                    {
                        if (throwOnError)
                        {
                            throw new TypeLoadException(Environment.GetResourceString("TypeLoad_ResolveNestedType", new object[] { names[i], names[i - 1] }));
                        }
                        return nestedType;
                    }
                }
            }
            return nestedType;
        }
    }
}

