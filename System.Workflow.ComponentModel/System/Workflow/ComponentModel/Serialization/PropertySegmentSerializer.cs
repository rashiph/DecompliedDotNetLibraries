namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Reflection;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Workflow.ComponentModel.Design;

    internal sealed class PropertySegmentSerializer : WorkflowMarkupSerializer
    {
        private WorkflowMarkupSerializer containedSerializer;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PropertySegmentSerializer(WorkflowMarkupSerializer containedSerializer)
        {
            this.containedSerializer = containedSerializer;
        }

        protected internal override void AddChild(WorkflowMarkupSerializationManager serializationManager, object obj, object childObj)
        {
            if (this.containedSerializer != null)
            {
                this.containedSerializer.AddChild(serializationManager, obj, childObj);
            }
        }

        protected internal override bool CanSerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            bool flag = false;
            if (value != null)
            {
                ITypeDescriptorContext context = null;
                TypeConverter converter = this.GetTypeConversionInfoForPropertySegment(serializationManager, value.GetType(), out context);
                if (converter != null)
                {
                    flag = converter.CanConvertTo(context, typeof(string));
                }
                if (flag)
                {
                    return flag;
                }
                if (this.containedSerializer != null)
                {
                    return this.containedSerializer.CanSerializeToString(serializationManager, value);
                }
                return base.CanSerializeToString(serializationManager, value);
            }
            return true;
        }

        protected internal override void ClearChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (this.containedSerializer != null)
            {
                this.containedSerializer.ClearChildren(serializationManager, obj);
            }
        }

        protected override object CreateInstance(WorkflowMarkupSerializationManager serializationManager, Type type)
        {
            if (typeof(PropertySegment) == type)
            {
                return Activator.CreateInstance(type, new object[] { serializationManager, serializationManager.Context.Current });
            }
            return base.CreateInstance(serializationManager, type);
        }

        protected internal override object DeserializeFromString(WorkflowMarkupSerializationManager serializationManager, Type propertyType, string value)
        {
            if (serializationManager == null)
            {
                throw new ArgumentNullException("serializationManager");
            }
            if (propertyType == null)
            {
                throw new ArgumentNullException("propertyType");
            }
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            if (string.Equals(value, "*null", StringComparison.Ordinal))
            {
                return null;
            }
            ITypeDescriptorContext context = null;
            TypeConverter converter = this.GetTypeConversionInfoForPropertySegment(serializationManager, propertyType, out context);
            if ((converter != null) && converter.CanConvertFrom(context, typeof(string)))
            {
                return converter.ConvertFromString(context, value);
            }
            if (this.containedSerializer != null)
            {
                return this.containedSerializer.DeserializeFromString(serializationManager, propertyType, value);
            }
            return base.DeserializeFromString(serializationManager, propertyType, value);
        }

        protected internal override IList GetChildren(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (this.containedSerializer != null)
            {
                return this.containedSerializer.GetChildren(serializationManager, obj);
            }
            return null;
        }

        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            if (this.containedSerializer != null)
            {
                return this.containedSerializer.GetProperties(serializationManager, obj);
            }
            if ((obj != null) && (obj.GetType() == typeof(PropertySegment)))
            {
                return (obj as PropertySegment).GetProperties(serializationManager);
            }
            return base.GetProperties(serializationManager, obj);
        }

        private TypeConverter GetTypeConversionInfoForPropertySegment(WorkflowMarkupSerializationManager serializationManager, Type propertyType, out ITypeDescriptorContext context)
        {
            TypeConverter converter = null;
            context = null;
            PropertySegmentPropertyInfo info = serializationManager.Context[typeof(PropertySegmentPropertyInfo)] as PropertySegmentPropertyInfo;
            if (info.PropertySegment != null)
            {
                if (info.PropertySegment.PropertyDescriptor != null)
                {
                    context = new TypeDescriptorContext(info.PropertySegment.ServiceProvider, info.PropertySegment.PropertyDescriptor, info.PropertySegment.Object);
                    converter = info.PropertySegment.PropertyDescriptor.Converter;
                }
                else if (info.PropertySegment.Object != null)
                {
                    PropertyDescriptor propDesc = TypeDescriptor.GetProperties(info.PropertySegment.Object)[info.Name];
                    if (propDesc != null)
                    {
                        context = new TypeDescriptorContext(info.PropertySegment.ServiceProvider, propDesc, info.PropertySegment.Object);
                        converter = propDesc.Converter;
                    }
                }
            }
            if ((propertyType != null) && (converter == null))
            {
                converter = TypeDescriptor.GetConverter(propertyType);
            }
            return converter;
        }

        protected internal override string SerializeToString(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            if (value == null)
            {
                return "*null";
            }
            ITypeDescriptorContext context = null;
            TypeConverter converter = this.GetTypeConversionInfoForPropertySegment(serializationManager, value.GetType(), out context);
            if ((converter != null) && converter.CanConvertTo(context, typeof(string)))
            {
                return converter.ConvertToString(context, value);
            }
            if (this.containedSerializer != null)
            {
                return this.containedSerializer.SerializeToString(serializationManager, value);
            }
            return base.SerializeToString(serializationManager, value);
        }

        protected internal override bool ShouldSerializeValue(WorkflowMarkupSerializationManager serializationManager, object value)
        {
            return true;
        }
    }
}

