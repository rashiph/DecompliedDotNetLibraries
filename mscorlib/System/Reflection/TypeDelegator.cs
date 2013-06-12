namespace System.Reflection
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;

    [Serializable, ComVisible(true)]
    public class TypeDelegator : Type
    {
        protected Type typeImpl;

        protected TypeDelegator()
        {
        }

        [SecuritySafeCritical]
        public TypeDelegator(Type delegatingType)
        {
            if (delegatingType == null)
            {
                throw new ArgumentNullException("delegatingType");
            }
            this.typeImpl = delegatingType;
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            return this.typeImpl.Attributes;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            return this.typeImpl.GetConstructor(bindingAttr, binder, callConvention, types, modifiers);
        }

        [ComVisible(true)]
        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            return this.typeImpl.GetConstructors(bindingAttr);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.typeImpl.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.typeImpl.GetCustomAttributes(attributeType, inherit);
        }

        public override Type GetElementType()
        {
            return this.typeImpl.GetElementType();
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            return this.typeImpl.GetEvent(name, bindingAttr);
        }

        public override EventInfo[] GetEvents()
        {
            return this.typeImpl.GetEvents();
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            return this.typeImpl.GetEvents(bindingAttr);
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            return this.typeImpl.GetField(name, bindingAttr);
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            return this.typeImpl.GetFields(bindingAttr);
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            return this.typeImpl.GetInterface(name, ignoreCase);
        }

        [ComVisible(true)]
        public override InterfaceMapping GetInterfaceMap(Type interfaceType)
        {
            return this.typeImpl.GetInterfaceMap(interfaceType);
        }

        public override Type[] GetInterfaces()
        {
            return this.typeImpl.GetInterfaces();
        }

        public override MemberInfo[] GetMember(string name, MemberTypes type, BindingFlags bindingAttr)
        {
            return this.typeImpl.GetMember(name, type, bindingAttr);
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            return this.typeImpl.GetMembers(bindingAttr);
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            if (types == null)
            {
                return this.typeImpl.GetMethod(name, bindingAttr);
            }
            return this.typeImpl.GetMethod(name, bindingAttr, binder, callConvention, types, modifiers);
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            return this.typeImpl.GetMethods(bindingAttr);
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            return this.typeImpl.GetNestedType(name, bindingAttr);
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            return this.typeImpl.GetNestedTypes(bindingAttr);
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            return this.typeImpl.GetProperties(bindingAttr);
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            if ((returnType == null) && (types == null))
            {
                return this.typeImpl.GetProperty(name, bindingAttr);
            }
            return this.typeImpl.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        }

        protected override bool HasElementTypeImpl()
        {
            return this.typeImpl.HasElementType;
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            return this.typeImpl.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        protected override bool IsArrayImpl()
        {
            return this.typeImpl.IsArray;
        }

        protected override bool IsByRefImpl()
        {
            return this.typeImpl.IsByRef;
        }

        protected override bool IsCOMObjectImpl()
        {
            return this.typeImpl.IsCOMObject;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.typeImpl.IsDefined(attributeType, inherit);
        }

        protected override bool IsPointerImpl()
        {
            return this.typeImpl.IsPointer;
        }

        protected override bool IsPrimitiveImpl()
        {
            return this.typeImpl.IsPrimitive;
        }

        protected override bool IsValueTypeImpl()
        {
            return this.typeImpl.IsValueType;
        }

        public override System.Reflection.Assembly Assembly
        {
            get
            {
                return this.typeImpl.Assembly;
            }
        }

        public override string AssemblyQualifiedName
        {
            get
            {
                return this.typeImpl.AssemblyQualifiedName;
            }
        }

        public override Type BaseType
        {
            get
            {
                return this.typeImpl.BaseType;
            }
        }

        public override string FullName
        {
            get
            {
                return this.typeImpl.FullName;
            }
        }

        public override Guid GUID
        {
            get
            {
                return this.typeImpl.GUID;
            }
        }

        public override int MetadataToken
        {
            get
            {
                return this.typeImpl.MetadataToken;
            }
        }

        public override System.Reflection.Module Module
        {
            get
            {
                return this.typeImpl.Module;
            }
        }

        public override string Name
        {
            get
            {
                return this.typeImpl.Name;
            }
        }

        public override string Namespace
        {
            get
            {
                return this.typeImpl.Namespace;
            }
        }

        public override RuntimeTypeHandle TypeHandle
        {
            get
            {
                return this.typeImpl.TypeHandle;
            }
        }

        public override Type UnderlyingSystemType
        {
            get
            {
                return this.typeImpl.UnderlyingSystemType;
            }
        }
    }
}

