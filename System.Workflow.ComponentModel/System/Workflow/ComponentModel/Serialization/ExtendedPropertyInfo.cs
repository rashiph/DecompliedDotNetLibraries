namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Xml;

    internal sealed class ExtendedPropertyInfo : PropertyInfo
    {
        private WorkflowMarkupSerializationManager manager;
        private GetValueHandler OnGetValue;
        private GetQualifiedNameHandler OnGetXmlQualifiedName;
        private SetValueHandler OnSetValue;
        private PropertyInfo realPropertyInfo;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ExtendedPropertyInfo(PropertyInfo propertyInfo, GetValueHandler getValueHandler, SetValueHandler setValueHandler, GetQualifiedNameHandler qualifiedNameHandler)
        {
            this.realPropertyInfo = propertyInfo;
            this.OnGetValue = getValueHandler;
            this.OnSetValue = setValueHandler;
            this.OnGetXmlQualifiedName = qualifiedNameHandler;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal ExtendedPropertyInfo(PropertyInfo propertyInfo, GetValueHandler getValueHandler, SetValueHandler setValueHandler, GetQualifiedNameHandler qualifiedNameHandler, WorkflowMarkupSerializationManager manager) : this(propertyInfo, getValueHandler, setValueHandler, qualifiedNameHandler)
        {
            this.manager = manager;
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            return this.realPropertyInfo.GetAccessors(nonPublic);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            return this.realPropertyInfo.GetCustomAttributes(inherit);
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            return this.realPropertyInfo.GetCustomAttributes(attributeType, inherit);
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return this.realPropertyInfo.GetGetMethod(nonPublic);
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            return this.realPropertyInfo.GetIndexParameters();
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return this.realPropertyInfo.GetSetMethod(nonPublic);
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (this.OnGetValue != null)
            {
                return this.OnGetValue(this, obj);
            }
            return null;
        }

        public XmlQualifiedName GetXmlQualifiedName(WorkflowMarkupSerializationManager manager, out string prefix)
        {
            prefix = string.Empty;
            if (this.OnGetXmlQualifiedName != null)
            {
                return this.OnGetXmlQualifiedName(this, manager, out prefix);
            }
            return null;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return this.realPropertyInfo.IsDefined(attributeType, inherit);
        }

        internal static bool IsExtendedProperty(WorkflowMarkupSerializationManager manager, XmlQualifiedName xmlQualifiedName)
        {
            object current = manager.Context.Current;
            if (current != null)
            {
                foreach (ExtendedPropertyInfo info in manager.GetExtendedProperties(current))
                {
                    string prefix = string.Empty;
                    XmlQualifiedName name = info.GetXmlQualifiedName(manager, out prefix);
                    if (name.Name.Equals(xmlQualifiedName.Name, StringComparison.Ordinal) && name.Namespace.Equals(xmlQualifiedName.Namespace, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static bool IsExtendedProperty(WorkflowMarkupSerializationManager manager, IList<PropertyInfo> propInfos, XmlQualifiedName xmlQualifiedName)
        {
            foreach (PropertyInfo info in propInfos)
            {
                ExtendedPropertyInfo info2 = info as ExtendedPropertyInfo;
                if (info2 != null)
                {
                    string prefix = string.Empty;
                    XmlQualifiedName name = info2.GetXmlQualifiedName(manager, out prefix);
                    if (name.Name.Equals(xmlQualifiedName.Name, StringComparison.Ordinal) && name.Namespace.Equals(xmlQualifiedName.Namespace, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (this.OnSetValue != null)
            {
                this.OnSetValue(this, obj, value);
            }
        }

        public override PropertyAttributes Attributes
        {
            get
            {
                return this.realPropertyInfo.Attributes;
            }
        }

        public override bool CanRead
        {
            get
            {
                return this.realPropertyInfo.CanRead;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return this.realPropertyInfo.CanWrite;
            }
        }

        public override Type DeclaringType
        {
            get
            {
                return this.realPropertyInfo.DeclaringType;
            }
        }

        public override string Name
        {
            get
            {
                return this.realPropertyInfo.Name;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this.realPropertyInfo.PropertyType;
            }
        }

        internal PropertyInfo RealPropertyInfo
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.realPropertyInfo;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                return this.realPropertyInfo.ReflectedType;
            }
        }

        internal WorkflowMarkupSerializationManager SerializationManager
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.manager;
            }
        }
    }
}

