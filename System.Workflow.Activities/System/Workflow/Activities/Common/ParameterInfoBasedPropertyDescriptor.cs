namespace System.Workflow.Activities.Common
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;

    internal class ParameterInfoBasedPropertyDescriptor : PropertyDescriptor
    {
        private bool avoidDuplication;
        private Type componentType;
        private string desc;
        private object parameter;
        private const string parameterPrefix = "(Parameter) ";
        private Type parameterType;

        internal ParameterInfoBasedPropertyDescriptor(Type componentType, ParameterInfo paramInfo, bool avoidDuplication, params Attribute[] attributes) : base((paramInfo.Position == -1) ? "(ReturnValue)" : paramInfo.Name, attributes)
        {
            this.desc = string.Empty;
            if (componentType == null)
            {
                throw new ArgumentNullException("componentType");
            }
            if (paramInfo == null)
            {
                throw new ArgumentNullException("paramInfo");
            }
            if (paramInfo.ParameterType == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ParameterTypeResolution", new object[] { paramInfo.Name }));
            }
            this.componentType = componentType;
            this.parameter = paramInfo;
            this.avoidDuplication = avoidDuplication;
            this.parameterType = paramInfo.ParameterType;
            if ((paramInfo.ParameterType != null) && (paramInfo.ParameterType.IsByRef || (paramInfo.IsIn && paramInfo.IsOut)))
            {
                SR.GetString("Ref");
            }
            else if (paramInfo.IsOut || (paramInfo.Name == null))
            {
                SR.GetString("Out");
            }
            else
            {
                SR.GetString("In");
            }
            this.desc = SR.GetString("ParameterDescription", new object[] { paramInfo.ParameterType.FullName });
        }

        internal ParameterInfoBasedPropertyDescriptor(Type componentType, string propertyName, Type propertyType, bool avoidDuplication, params Attribute[] attributes) : base(propertyName, attributes)
        {
            this.desc = string.Empty;
            if (componentType == null)
            {
                throw new ArgumentNullException("componentType");
            }
            if (propertyType == null)
            {
                throw new InvalidOperationException(SR.GetString("Error_ParameterTypeResolution", new object[] { propertyName }));
            }
            this.componentType = componentType;
            this.parameterType = propertyType;
            this.avoidDuplication = avoidDuplication;
            this.desc = SR.GetString("InvokeParameterDescription", new object[] { propertyType.FullName.ToString() });
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        internal static MemberInfo FindMatchingMember(string name, Type ownerType, bool ignoreCase)
        {
            foreach (MemberInfo info2 in ownerType.GetMembers(BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance))
            {
                if (info2.Name.Equals(name, ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.CurrentCulture))
                {
                    return info2;
                }
            }
            return null;
        }

        public override object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this.PropertyType, editorBaseType);
        }

        private WorkflowParameterBindingCollection GetParameterBindings(object component)
        {
            WorkflowParameterBindingCollection bindings = null;
            if (component.GetType().GetProperty("ParameterBindings", BindingFlags.ExactBinding | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance, null, typeof(WorkflowParameterBindingCollection), new Type[0], new ParameterModifier[0]) != null)
            {
                bindings = component.GetType().InvokeMember("ParameterBindings", BindingFlags.ExactBinding | BindingFlags.GetProperty | BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance, null, component, new object[0], CultureInfo.InvariantCulture) as WorkflowParameterBindingCollection;
            }
            return bindings;
        }

        public static string GetParameterPropertyName(Type componentType, string paramName)
        {
            string str = paramName;
            if (FindMatchingMember(paramName, componentType, false) != null)
            {
                str = "(Parameter) " + paramName;
            }
            return str;
        }

        public override object GetValue(object component)
        {
            WorkflowParameterBindingCollection parameterBindings = this.GetParameterBindings(component);
            string name = this.Name;
            string key = name.StartsWith("(Parameter) ", StringComparison.Ordinal) ? name.Substring("(Parameter) ".Length) : name;
            if ((parameterBindings == null) || !parameterBindings.Contains(key))
            {
                return null;
            }
            if (parameterBindings[key].IsBindingSet(WorkflowParameterBinding.ValueProperty))
            {
                return parameterBindings[key].GetBinding(WorkflowParameterBinding.ValueProperty);
            }
            return parameterBindings[key].GetValue(WorkflowParameterBinding.ValueProperty);
        }

        public override void ResetValue(object component)
        {
            if ((this.PropertyType != null) && !this.PropertyType.IsValueType)
            {
                this.SetValue(component, null);
            }
        }

        public override void SetValue(object component, object value)
        {
            if (component != null)
            {
                IServiceProvider site = MemberDescriptor.GetSite(component);
                System.Workflow.Activities.Common.ComponentChangeDispatcher dispatcher = (site != null) ? new System.Workflow.Activities.Common.ComponentChangeDispatcher(site, component, this) : null;
                try
                {
                    WorkflowParameterBindingCollection parameterBindings = this.GetParameterBindings(component);
                    if (parameterBindings != null)
                    {
                        string key = string.Empty;
                        if (this.Name.StartsWith("(Parameter) ", StringComparison.Ordinal))
                        {
                            key = this.Name.Substring("(Parameter) ".Length);
                        }
                        else
                        {
                            key = this.Name;
                        }
                        WorkflowParameterBinding item = null;
                        if (parameterBindings.Contains(key))
                        {
                            item = parameterBindings[key];
                        }
                        else
                        {
                            item = new WorkflowParameterBinding(key);
                            parameterBindings.Add(item);
                        }
                        if (value is ActivityBind)
                        {
                            item.SetBinding(WorkflowParameterBinding.ValueProperty, value as ActivityBind);
                        }
                        else
                        {
                            item.SetValue(WorkflowParameterBinding.ValueProperty, value);
                        }
                        this.OnValueChanged(component, EventArgs.Empty);
                    }
                }
                catch (Exception exception)
                {
                    if ((exception is TargetInvocationException) && (exception.InnerException != null))
                    {
                        throw exception.InnerException;
                    }
                    throw exception;
                }
                finally
                {
                    if (dispatcher != null)
                    {
                        dispatcher.Dispose();
                    }
                }
            }
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }

        public override AttributeCollection Attributes
        {
            get
            {
                ArrayList list = new ArrayList();
                list.AddRange(base.Attributes);
                list.AddRange(TypeDescriptor.GetAttributes(this.PropertyType));
                return new AttributeCollection((Attribute[]) list.ToArray(typeof(Attribute)));
            }
        }

        public override string Category
        {
            get
            {
                return SR.GetString("Parameters");
            }
        }

        public override Type ComponentType
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.componentType;
            }
        }

        public override TypeConverter Converter
        {
            get
            {
                return new ActivityBindTypeConverter();
            }
        }

        public override string Description
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.desc;
            }
        }

        public override string DisplayName
        {
            get
            {
                return this.Name;
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public override string Name
        {
            get
            {
                if (this.avoidDuplication)
                {
                    return GetParameterPropertyName(this.componentType, base.Name);
                }
                return base.Name;
            }
        }

        internal Type ParameterType
        {
            get
            {
                Type parameterType = this.parameterType;
                if (parameterType.IsByRef)
                {
                    parameterType = parameterType.GetElementType();
                }
                return parameterType;
            }
        }

        public override Type PropertyType
        {
            get
            {
                Type parameterType = this.ParameterType;
                if (parameterType == null)
                {
                    parameterType = typeof(ActivityBind);
                }
                return parameterType;
            }
        }
    }
}

