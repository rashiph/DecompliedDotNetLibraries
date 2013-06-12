namespace System.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Security.Permissions;

    [Serializable, ComVisible(true), ClassInterface(ClassInterfaceType.None), ComDefaultInterface(typeof(_Module)), PermissionSet(SecurityAction.InheritanceDemand, Unrestricted=true)]
    public abstract class Module : _Module, ISerializable, ICustomAttributeProvider
    {
        private const BindingFlags DefaultLookup = (BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        public static readonly TypeFilter FilterTypeName;
        public static readonly TypeFilter FilterTypeNameIgnoreCase;

        static Module()
        {
            System.Reflection.__Filters filters = new System.Reflection.__Filters();
            FilterTypeName = new TypeFilter(filters.FilterTypeName);
            FilterTypeNameIgnoreCase = new TypeFilter(filters.FilterTypeNameIgnoreCase);
        }

        protected Module()
        {
        }

        public override bool Equals(object o)
        {
            return base.Equals(o);
        }

        public virtual Type[] FindTypes(TypeFilter filter, object filterCriteria)
        {
            Type[] types = this.GetTypes();
            int num = 0;
            for (int i = 0; i < types.Length; i++)
            {
                if ((filter != null) && !filter(types[i], filterCriteria))
                {
                    types[i] = null;
                }
                else
                {
                    num++;
                }
            }
            if (num == types.Length)
            {
                return types;
            }
            Type[] typeArray2 = new Type[num];
            num = 0;
            for (int j = 0; j < types.Length; j++)
            {
                if (types[j] != null)
                {
                    typeArray2[num++] = types[j];
                }
            }
            return typeArray2;
        }

        public virtual object[] GetCustomAttributes(bool inherit)
        {
            throw new NotImplementedException();
        }

        public virtual object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public virtual IList<CustomAttributeData> GetCustomAttributesData()
        {
            throw new NotImplementedException();
        }

        public FieldInfo GetField(string name)
        {
            return this.GetField(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public virtual FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            RuntimeModule module = this as RuntimeModule;
            if (module == null)
            {
                throw new NotImplementedException();
            }
            return module.GetField(name, bindingAttr);
        }

        public FieldInfo[] GetFields()
        {
            return this.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public virtual FieldInfo[] GetFields(BindingFlags bindingFlags)
        {
            RuntimeModule module = this as RuntimeModule;
            if (module == null)
            {
                throw new NotImplementedException();
            }
            return module.GetFields(bindingFlags);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public MethodInfo GetMethod(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            return this.GetMethodImpl(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, CallingConventions.Any, null, null);
        }

        public MethodInfo GetMethod(string name, Type[] types)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == null)
                {
                    throw new ArgumentNullException("types");
                }
            }
            return this.GetMethodImpl(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance, null, CallingConventions.Any, types, null);
        }

        public MethodInfo GetMethod(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (types == null)
            {
                throw new ArgumentNullException("types");
            }
            for (int i = 0; i < types.Length; i++)
            {
                if (types[i] == null)
                {
                    throw new ArgumentNullException("types");
                }
            }
            return this.GetMethodImpl(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        protected virtual MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        public MethodInfo[] GetMethods()
        {
            return this.GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
        }

        public virtual MethodInfo[] GetMethods(BindingFlags bindingFlags)
        {
            RuntimeModule module = this as RuntimeModule;
            if (module == null)
            {
                throw new NotImplementedException();
            }
            return module.GetMethods(bindingFlags);
        }

        internal virtual System.ModuleHandle GetModuleHandle()
        {
            return System.ModuleHandle.EmptyHandle;
        }

        [SecurityCritical]
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public virtual void GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine machine)
        {
            RuntimeModule module = this as RuntimeModule;
            if (module != null)
            {
                module.GetPEKind(out peKind, out machine);
            }
            throw new NotImplementedException();
        }

        public virtual X509Certificate GetSignerCertificate()
        {
            throw new NotImplementedException();
        }

        [ComVisible(true)]
        public virtual Type GetType(string className)
        {
            return this.GetType(className, false, false);
        }

        [ComVisible(true)]
        public virtual Type GetType(string className, bool ignoreCase)
        {
            return this.GetType(className, false, ignoreCase);
        }

        [ComVisible(true)]
        public virtual Type GetType(string className, bool throwOnError, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        public virtual Type[] GetTypes()
        {
            throw new NotImplementedException();
        }

        public virtual bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public virtual bool IsResource()
        {
            RuntimeModule module = this as RuntimeModule;
            if (module == null)
            {
                throw new NotImplementedException();
            }
            return module.IsResource();
        }

        public static bool operator ==(Module left, Module right)
        {
            return (object.ReferenceEquals(left, right) || ((((left != null) && (right != null)) && (!(left is RuntimeModule) && !(right is RuntimeModule))) && left.Equals(right)));
        }

        public static bool operator !=(Module left, Module right)
        {
            return !(left == right);
        }

        public FieldInfo ResolveField(int metadataToken)
        {
            return this.ResolveField(metadataToken, null, null);
        }

        public virtual FieldInfo ResolveField(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            RuntimeModule module = this as RuntimeModule;
            if (module == null)
            {
                throw new NotImplementedException();
            }
            return module.ResolveField(metadataToken, genericTypeArguments, genericMethodArguments);
        }

        public MemberInfo ResolveMember(int metadataToken)
        {
            return this.ResolveMember(metadataToken, null, null);
        }

        public virtual MemberInfo ResolveMember(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            RuntimeModule module = this as RuntimeModule;
            if (module == null)
            {
                throw new NotImplementedException();
            }
            return module.ResolveMember(metadataToken, genericTypeArguments, genericMethodArguments);
        }

        public MethodBase ResolveMethod(int metadataToken)
        {
            return this.ResolveMethod(metadataToken, null, null);
        }

        public virtual MethodBase ResolveMethod(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            RuntimeModule module = this as RuntimeModule;
            if (module == null)
            {
                throw new NotImplementedException();
            }
            return module.ResolveMethod(metadataToken, genericTypeArguments, genericMethodArguments);
        }

        public virtual byte[] ResolveSignature(int metadataToken)
        {
            RuntimeModule module = this as RuntimeModule;
            if (module == null)
            {
                throw new NotImplementedException();
            }
            return module.ResolveSignature(metadataToken);
        }

        public virtual string ResolveString(int metadataToken)
        {
            RuntimeModule module = this as RuntimeModule;
            if (module == null)
            {
                throw new NotImplementedException();
            }
            return module.ResolveString(metadataToken);
        }

        public Type ResolveType(int metadataToken)
        {
            return this.ResolveType(metadataToken, null, null);
        }

        public virtual Type ResolveType(int metadataToken, Type[] genericTypeArguments, Type[] genericMethodArguments)
        {
            RuntimeModule module = this as RuntimeModule;
            if (module == null)
            {
                throw new NotImplementedException();
            }
            return module.ResolveType(metadataToken, genericTypeArguments, genericMethodArguments);
        }

        void _Module.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
        {
            throw new NotImplementedException();
        }

        void _Module.GetTypeInfo(uint iTInfo, uint lcid, IntPtr ppTInfo)
        {
            throw new NotImplementedException();
        }

        void _Module.GetTypeInfoCount(out uint pcTInfo)
        {
            throw new NotImplementedException();
        }

        void _Module.Invoke(uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return this.ScopeName;
        }

        public virtual System.Reflection.Assembly Assembly
        {
            get
            {
                RuntimeModule module = this as RuntimeModule;
                if (module == null)
                {
                    throw new NotImplementedException();
                }
                return module.Assembly;
            }
        }

        public virtual string FullyQualifiedName
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public virtual int MDStreamVersion
        {
            get
            {
                RuntimeModule module = this as RuntimeModule;
                if (module == null)
                {
                    throw new NotImplementedException();
                }
                return module.MDStreamVersion;
            }
        }

        public virtual int MetadataToken
        {
            get
            {
                RuntimeModule module = this as RuntimeModule;
                if (module == null)
                {
                    throw new NotImplementedException();
                }
                return module.MetadataToken;
            }
        }

        public System.ModuleHandle ModuleHandle
        {
            get
            {
                return this.GetModuleHandle();
            }
        }

        public virtual Guid ModuleVersionId
        {
            get
            {
                RuntimeModule module = this as RuntimeModule;
                if (module == null)
                {
                    throw new NotImplementedException();
                }
                return module.ModuleVersionId;
            }
        }

        public virtual string Name
        {
            get
            {
                RuntimeModule module = this as RuntimeModule;
                if (module == null)
                {
                    throw new NotImplementedException();
                }
                return module.Name;
            }
        }

        public virtual string ScopeName
        {
            get
            {
                RuntimeModule module = this as RuntimeModule;
                if (module == null)
                {
                    throw new NotImplementedException();
                }
                return module.ScopeName;
            }
        }
    }
}

