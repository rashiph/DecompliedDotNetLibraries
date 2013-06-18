namespace System.Workflow.ComponentModel.Compiler
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;

    internal sealed class DesignTimePropertyInfo : PropertyInfo
    {
        private Attribute[] attributes;
        private DesignTimeType declaringType;
        private MethodInfo getMethod;
        private System.CodeDom.CodeMemberProperty property;
        private MethodInfo setMethod;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal DesignTimePropertyInfo(DesignTimeType declaringType, System.CodeDom.CodeMemberProperty property)
        {
            this.property = property;
            this.declaringType = declaringType;
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            ArrayList list = new ArrayList();
            if (Helper.IncludeAccessor(this.GetGetMethod(nonPublic), nonPublic))
            {
                list.Add(this.getMethod);
            }
            if (Helper.IncludeAccessor(this.GetSetMethod(nonPublic), nonPublic))
            {
                list.Add(this.setMethod);
            }
            return (list.ToArray(typeof(MethodInfo)) as MethodInfo[]);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.GetCustomAttributes(typeof(object), inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (this.attributes == null)
            {
                this.attributes = Helper.LoadCustomAttributes(this.property.CustomAttributes, this.DeclaringType as DesignTimeType);
            }
            return Helper.GetCustomAttributes(attributeType, inherit, this.attributes, this);
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            if (this.CanRead && (this.getMethod == null))
            {
                string name = "get_" + this.Name;
                this.getMethod = new PropertyMethodInfo(true, name, this);
            }
            if (!nonPublic && ((this.getMethod == null) || ((this.getMethod.Attributes & MethodAttributes.Public) != MethodAttributes.Public)))
            {
                return null;
            }
            return this.getMethod;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            int length = 0;
            ParameterInfo[] parameters = null;
            MethodInfo getMethod = this.GetGetMethod(true);
            if (getMethod != null)
            {
                parameters = getMethod.GetParameters();
                length = parameters.Length;
            }
            else
            {
                getMethod = this.GetSetMethod(true);
                if (getMethod != null)
                {
                    parameters = getMethod.GetParameters();
                    length = parameters.Length - 1;
                }
            }
            ParameterInfo[] infoArray2 = new ParameterInfo[length];
            for (int i = 0; i < length; i++)
            {
                infoArray2[i] = parameters[i];
            }
            return infoArray2;
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            if (this.CanWrite && (this.setMethod == null))
            {
                string name = "set_" + this.Name;
                this.setMethod = new PropertyMethodInfo(false, name, this);
            }
            if (!nonPublic && ((this.setMethod == null) || ((this.setMethod.Attributes & MethodAttributes.Public) != MethodAttributes.Public)))
            {
                return null;
            }
            return this.setMethod;
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
            {
                throw new ArgumentNullException("attributeType");
            }
            if (this.attributes == null)
            {
                this.attributes = Helper.LoadCustomAttributes(this.property.CustomAttributes, this.DeclaringType as DesignTimeType);
            }
            return Helper.IsDefined(attributeType, inherit, this.attributes, this);
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
        }

        public override PropertyAttributes Attributes
        {
            get
            {
                return PropertyAttributes.None;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this.property.HasGet;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.property.HasSet;
            }
        }

        internal System.CodeDom.CodeMemberProperty CodeMemberProperty
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.property;
            }
        }

        public override Type DeclaringType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.declaringType;
            }
        }

        public override string Name
        {
            get
            {
                return Helper.EnsureTypeName(this.property.Name);
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this.declaringType.ResolveType(DesignTimeType.GetTypeNameFromCodeTypeReference(this.property.Type, this.declaringType));
            }
        }

        public override Type ReflectedType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.declaringType;
            }
        }

        private sealed class PropertyMethodInfo : MethodInfo
        {
            private bool isGetter;
            private string name = string.Empty;
            private ParameterInfo[] parameters;
            private DesignTimePropertyInfo property;

            internal PropertyMethodInfo(bool isGetter, string name, DesignTimePropertyInfo property)
            {
                this.isGetter = isGetter;
                this.name = name;
                this.property = property;
            }

            public override MethodInfo GetBaseDefinition()
            {
                throw new NotImplementedException();
            }

            public override object[] GetCustomAttributes(bool inherit)
            {
                return this.GetCustomAttributes(typeof(object), inherit);
            }

            public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            {
                return new object[0];
            }

            public override MethodImplAttributes GetMethodImplementationFlags()
            {
                return MethodImplAttributes.IL;
            }

            public override ParameterInfo[] GetParameters()
            {
                if (this.parameters == null)
                {
                    CodeParameterDeclarationExpressionCollection parameters = this.property.CodeMemberProperty.Parameters;
                    ParameterInfo[] infoArray = new ParameterInfo[this.IsGetter ? parameters.Count : (parameters.Count + 1)];
                    for (int i = 0; i < parameters.Count; i++)
                    {
                        infoArray[i] = new DesignTimeParameterInfo(parameters[i], i, this.property);
                    }
                    if (!this.IsGetter)
                    {
                        CodeParameterDeclarationExpression codeParameter = new CodeParameterDeclarationExpression(this.property.CodeMemberProperty.Type.BaseType, "value") {
                            Direction = FieldDirection.In
                        };
                        infoArray[parameters.Count] = new DesignTimeParameterInfo(codeParameter, 0, this.property);
                    }
                    this.parameters = infoArray;
                }
                return this.parameters;
            }

            public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
            {
                throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
            }

            public override bool IsDefined(Type attributeType, bool inherit)
            {
                return false;
            }

            public override MethodAttributes Attributes
            {
                get
                {
                    return (Helper.ConvertToMethodAttributes(this.property.CodeMemberProperty.Attributes) | MethodAttributes.SpecialName);
                }
            }

            public override Type DeclaringType
            {
                get
                {
                    return this.property.declaringType;
                }
            }

            internal bool IsGetter
            {
                get
                {
                    return this.isGetter;
                }
            }

            public override RuntimeMethodHandle MethodHandle
            {
                get
                {
                    throw new NotImplementedException(TypeSystemSR.GetString("Error_RuntimeNotSupported"));
                }
            }

            public override string Name
            {
                get
                {
                    return Helper.EnsureTypeName(this.name);
                }
            }

            public override Type ReflectedType
            {
                get
                {
                    return this.property.declaringType;
                }
            }

            public override ParameterInfo ReturnParameter
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public override Type ReturnType
            {
                get
                {
                    if (this.isGetter)
                    {
                        return ((DesignTimeType) this.DeclaringType).ResolveType(DesignTimeType.GetTypeNameFromCodeTypeReference(this.property.CodeMemberProperty.Type, (DesignTimeType) this.DeclaringType));
                    }
                    return typeof(void);
                }
            }

            public override ICustomAttributeProvider ReturnTypeCustomAttributes
            {
                get
                {
                    throw new NotImplementedException();
                }
            }
        }
    }
}

