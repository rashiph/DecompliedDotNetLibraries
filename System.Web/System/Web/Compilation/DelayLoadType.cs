namespace System.Web.Compilation
{
    using System;
    using System.Globalization;
    using System.Reflection;

    internal class DelayLoadType : System.Type
    {
        private string _assemblyName;
        private System.Type _type;
        private string _typeName;

        public DelayLoadType(string assemblyName, string typeName)
        {
            this._assemblyName = assemblyName;
            this._typeName = typeName;
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return this.Type.Attributes;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, System.Type[] types, ParameterModifier[] modifiers)
        {
            return this.Type.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return this.Type.GetConstructors(bindingAttr);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.Type.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(System.Type attributeType, bool inherit)
        {
            return this.Type.GetCustomAttributes(attributeType, inherit);
        }

        public override System.Type GetElementType()
        {
            return this.Type.GetElementType();
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            return this.Type.GetEvent(name, bindingAttr);
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            return this.Type.GetEvents(bindingAttr);
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return this.Type.GetField(name, bindingAttr);
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return this.Type.GetFields(bindingAttr);
        }

        public override System.Type GetInterface(string name, bool ignoreCase)
        {
            return this.Type.GetInterface(name, ignoreCase);
        }

        public override System.Type[] GetInterfaces()
        {
            return this.Type.GetInterfaces();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return this.Type.GetMembers(bindingAttr);
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, System.Type[] types, ParameterModifier[] modifiers)
        {
            return this.Type.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return this.Type.GetMethods(bindingAttr);
        }

        public override System.Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            return this.Type.GetNestedType(name, bindingAttr);
        }

        public override System.Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            return this.Type.GetNestedTypes(bindingAttr);
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return this.Type.GetProperties(bindingAttr);
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, System.Type returnType, System.Type[] types, ParameterModifier[] modifiers)
        {
            return this.Type.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        }

        protected override bool HasElementTypeImpl()
        {
            return this.Type.HasElementType;
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            return this.Type.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        protected override bool IsArrayImpl()
        {
            return this.Type.IsArray;
        }

        protected override bool IsByRefImpl()
        {
            return this.Type.IsByRef;
        }

        protected override bool IsCOMObjectImpl()
        {
            return this.Type.IsCOMObject;
        }

        public override bool IsDefined(System.Type attributeType, bool inherit)
        {
            return this.Type.IsDefined(attributeType, inherit);
        }

        protected override bool IsPointerImpl()
        {
            return this.Type.IsPointer;
        }

        protected override bool IsPrimitiveImpl()
        {
            return this.Type.IsPrimitive;
        }

        public override System.Reflection.Assembly Assembly
        {
            get
            {
                return this.Type.Assembly;
            }
        }

        public string AssemblyName
        {
            get
            {
                return this._assemblyName;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                return this.Type.AssemblyQualifiedName;
            }
        }

        public override System.Type BaseType
        {
            get
            {
                return this.Type.BaseType;
            }
        }

        internal static bool Enabled
        {
            get
            {
                return BuildManagerHost.InClientBuildManager;
            }
        }

        public override string FullName
        {
            get
            {
                return this.Type.FullName;
            }
        }

        public override Guid GUID
        {
            get
            {
                return this.Type.GUID;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.Type.Module;
            }
        }

        public override string Name
        {
            get
            {
                return this.Type.Name;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.Type.Namespace;
            }
        }

        public System.Type Type
        {
            get
            {
                if (this._type == null)
                {
                    this._type = System.Reflection.Assembly.Load(this._assemblyName).GetType(this._typeName);
                }
                return this._type;
            }
        }

        public string TypeName
        {
            get
            {
                return this._typeName;
            }
        }

        public override System.Type UnderlyingSystemType
        {
            get
            {
                return this.Type.UnderlyingSystemType;
            }
        }
    }
}

