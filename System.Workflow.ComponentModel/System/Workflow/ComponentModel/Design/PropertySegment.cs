namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Runtime;

    internal sealed class PropertySegment
    {
        private object obj;
        private PropertyInfo property;
        private System.ComponentModel.PropertyDescriptor propertyDescriptor;
        private IServiceProvider serviceProvider;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public PropertySegment(IServiceProvider serviceProvider, object obj)
        {
            this.serviceProvider = serviceProvider;
            this.obj = obj;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal PropertySegment(IServiceProvider serviceProvider, object obj, System.ComponentModel.PropertyDescriptor propertyDescriptor)
        {
            this.serviceProvider = serviceProvider;
            this.obj = obj;
            this.propertyDescriptor = propertyDescriptor;
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        internal PropertySegment(IServiceProvider serviceProvider, object obj, PropertyInfo property)
        {
            this.serviceProvider = serviceProvider;
            this.obj = obj;
            this.property = property;
        }

        private IComponent GetComponent(object obj, IServiceProvider serviceProvider)
        {
            IComponent component = obj as IComponent;
            if (((component == null) || (component.Site == null)) && (serviceProvider != null))
            {
                IReferenceService service = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
                if (service != null)
                {
                    component = service.GetComponent(obj);
                }
            }
            return component;
        }

        internal PropertyInfo[] GetProperties(IServiceProvider serviceProvider)
        {
            ArrayList list = new ArrayList(base.GetType().GetProperties());
            if (this.property != null)
            {
                list.Add(new PropertySegmentPropertyInfo(this, this.property));
            }
            else if (this.propertyDescriptor != null)
            {
                list.Add(new PropertySegmentPropertyInfo(this, this.propertyDescriptor));
            }
            else if (this.obj != null)
            {
                PropertyDescriptorCollection properties = null;
                TypeConverter converter = TypeDescriptor.GetConverter(this.obj);
                if ((converter != null) && converter.GetPropertiesSupported())
                {
                    DummyTypeDescriptorContext context = new DummyTypeDescriptorContext(this.serviceProvider, this.GetComponent(this.obj, serviceProvider), null);
                    properties = converter.GetProperties(context, this.obj, new Attribute[0]);
                }
                else
                {
                    properties = TypeDescriptor.GetProperties(this.obj);
                }
                foreach (System.ComponentModel.PropertyDescriptor descriptor in properties)
                {
                    PropertyInfo memberInfo = XomlComponentSerializationService.GetProperty(this.obj.GetType(), descriptor.Name, BindingFlags.Public | BindingFlags.Instance);
                    if (memberInfo != null)
                    {
                        if (Helpers.GetSerializationVisibility(memberInfo) != DesignerSerializationVisibility.Hidden)
                        {
                            list.Add(new PropertySegmentPropertyInfo(this, memberInfo));
                        }
                    }
                    else
                    {
                        list.Add(new PropertySegmentPropertyInfo(this, descriptor));
                        if (descriptor.Converter != null)
                        {
                            DummyTypeDescriptorContext context2 = new DummyTypeDescriptorContext(this.serviceProvider, this.GetComponent(this.obj, serviceProvider), descriptor);
                            if (descriptor.Converter.GetPropertiesSupported(context2))
                            {
                                foreach (System.ComponentModel.PropertyDescriptor descriptor2 in descriptor.Converter.GetProperties(context2, this.obj, new Attribute[0]))
                                {
                                    list.Add(new PropertySegmentPropertyInfo(this, descriptor2));
                                }
                            }
                        }
                    }
                }
            }
            return (list.ToArray(typeof(PropertyInfo)) as PropertyInfo[]);
        }

        internal object Object
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.obj;
            }
        }

        internal System.ComponentModel.PropertyDescriptor PropertyDescriptor
        {
            get
            {
                System.ComponentModel.PropertyDescriptor propertyDescriptor = this.propertyDescriptor;
                if (((propertyDescriptor == null) && (this.obj != null)) && (this.property != null))
                {
                    propertyDescriptor = TypeDescriptor.GetProperties(this.obj)[this.property.Name];
                }
                return propertyDescriptor;
            }
        }

        internal IServiceProvider ServiceProvider
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.serviceProvider;
            }
        }
    }
}

