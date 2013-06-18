namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.Workflow.ComponentModel;

    internal sealed class PropertySegmentPropertyInfo : PropertyInfo
    {
        private System.Workflow.ComponentModel.Design.PropertySegment propertySegment;
        private PropertyDescriptor realPropDesc;
        private PropertyInfo realPropInfo;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal PropertySegmentPropertyInfo(System.Workflow.ComponentModel.Design.PropertySegment propertySegment, PropertyDescriptor realPropDesc)
        {
            this.realPropDesc = realPropDesc;
            this.propertySegment = propertySegment;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal PropertySegmentPropertyInfo(System.Workflow.ComponentModel.Design.PropertySegment propertySegment, PropertyInfo realPropInfo)
        {
            this.realPropInfo = realPropInfo;
            this.propertySegment = propertySegment;
        }

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            if (this.realPropInfo != null)
            {
                return this.realPropInfo.GetAccessors(nonPublic);
            }
            return new MethodInfo[0];
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            if (this.realPropInfo != null)
            {
                return this.realPropInfo.GetCustomAttributes(inherit);
            }
            return new AttributeInfoAttribute[0];
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (this.realPropInfo != null)
            {
                return this.realPropInfo.GetCustomAttributes(attributeType, inherit);
            }
            return new AttributeInfoAttribute[0];
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            if (this.realPropInfo != null)
            {
                return this.realPropInfo.GetGetMethod(nonPublic);
            }
            return null;
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            if (this.realPropInfo != null)
            {
                return this.realPropInfo.GetIndexParameters();
            }
            return new ParameterInfo[0];
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            if (this.realPropInfo != null)
            {
                return this.realPropInfo.GetSetMethod(nonPublic);
            }
            return null;
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            DependencyProperty dependencyProperty = null;
            Activity activity = null;
            if (this.propertySegment != null)
            {
                activity = this.propertySegment.Object as Activity;
            }
            if (activity != null)
            {
                string name = this.Name;
                Type declaringType = this.DeclaringType;
                if (!string.IsNullOrEmpty(name) && (declaringType != null))
                {
                    dependencyProperty = DependencyProperty.FromName(name, declaringType);
                }
            }
            object binding = null;
            object obj3 = (this.propertySegment == null) ? obj : this.propertySegment.Object;
            if ((dependencyProperty != null) && !dependencyProperty.DefaultMetadata.IsMetaProperty)
            {
                if (activity.IsBindingSet(dependencyProperty))
                {
                    binding = activity.GetBinding(dependencyProperty);
                }
                else if (!dependencyProperty.IsEvent)
                {
                    binding = activity.GetValue(dependencyProperty);
                }
                else
                {
                    binding = activity.GetHandler(dependencyProperty);
                }
            }
            if (binding == null)
            {
                if (this.realPropInfo != null)
                {
                    return this.realPropInfo.GetValue(obj3, invokeAttr, binder, index, culture);
                }
                if (this.realPropDesc != null)
                {
                    binding = this.realPropDesc.GetValue(obj3);
                }
            }
            return binding;
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            return ((this.realPropInfo != null) && this.realPropInfo.IsDefined(attributeType, inherit));
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }
            string name = this.Name;
            System.Workflow.ComponentModel.Design.PropertySegment segment = obj as System.Workflow.ComponentModel.Design.PropertySegment;
            if (((segment != null) ? (segment.Object as Activity) : (obj as Activity)) != null)
            {
                Type declaringType = this.DeclaringType;
                if (!string.IsNullOrEmpty(name) && (declaringType != null))
                {
                    DependencyProperty.FromName(name, declaringType);
                }
            }
            PropertyDescriptor descriptor = null;
            object component = null;
            if (segment != null)
            {
                PropertyDescriptorCollection properties = null;
                TypeConverter converter = TypeDescriptor.GetConverter(segment.Object);
                if ((converter != null) && converter.GetPropertiesSupported())
                {
                    DummyTypeDescriptorContext context = new DummyTypeDescriptorContext(segment.ServiceProvider, segment.Object, this.realPropDesc);
                    properties = converter.GetProperties(context, segment.Object, new Attribute[0]);
                }
                else
                {
                    properties = TypeDescriptor.GetProperties(segment.Object);
                }
                foreach (PropertyDescriptor descriptor2 in properties)
                {
                    if (descriptor2.Name == name)
                    {
                        descriptor = descriptor2;
                    }
                    else if (descriptor2.Converter != null)
                    {
                        DummyTypeDescriptorContext context2 = new DummyTypeDescriptorContext(segment.ServiceProvider, segment.Object, descriptor2);
                        if ((descriptor2.GetValue(segment.Object) != null) && descriptor2.Converter.GetPropertiesSupported(context2))
                        {
                            foreach (PropertyDescriptor descriptor3 in descriptor2.Converter.GetProperties(context2, descriptor2.GetValue(segment.Object), new Attribute[0]))
                            {
                                if (descriptor3.Name == name)
                                {
                                    descriptor = descriptor3;
                                }
                            }
                        }
                    }
                }
                component = segment.Object;
            }
            else
            {
                descriptor = TypeDescriptor.GetProperties(obj)[name];
                component = obj;
            }
            if ((descriptor != null) && (component != null))
            {
                descriptor.SetValue(component, value);
            }
        }

        public override PropertyAttributes Attributes
        {
            get
            {
                if (this.realPropInfo != null)
                {
                    return this.realPropInfo.Attributes;
                }
                return PropertyAttributes.None;
            }
        }

        public override bool CanRead
        {
            get
            {
                if (this.realPropInfo != null)
                {
                    return this.realPropInfo.CanRead;
                }
                return true;
            }
        }

        public override bool CanWrite
        {
            get
            {
                if (this.realPropInfo != null)
                {
                    return this.realPropInfo.CanWrite;
                }
                return ((this.realPropDesc != null) && !this.realPropDesc.IsReadOnly);
            }
        }

        public override Type DeclaringType
        {
            get
            {
                if (this.realPropInfo != null)
                {
                    return this.realPropInfo.DeclaringType;
                }
                if (this.realPropDesc != null)
                {
                    return this.realPropDesc.ComponentType;
                }
                return null;
            }
        }

        public override string Name
        {
            get
            {
                if (this.realPropInfo != null)
                {
                    return this.realPropInfo.Name;
                }
                if (this.realPropDesc != null)
                {
                    return this.realPropDesc.Name;
                }
                return string.Empty;
            }
        }

        internal System.Workflow.ComponentModel.Design.PropertySegment PropertySegment
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.propertySegment;
            }
        }

        public override Type PropertyType
        {
            get
            {
                if (this.realPropInfo != null)
                {
                    return this.realPropInfo.PropertyType;
                }
                if (this.realPropDesc != null)
                {
                    return this.realPropDesc.PropertyType;
                }
                return null;
            }
        }

        public override Type ReflectedType
        {
            get
            {
                if (this.realPropInfo != null)
                {
                    return this.realPropInfo.ReflectedType;
                }
                return null;
            }
        }
    }
}

