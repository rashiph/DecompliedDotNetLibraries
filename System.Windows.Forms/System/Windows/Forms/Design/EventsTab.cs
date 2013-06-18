namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [PermissionSet(SecurityAction.LinkDemand, Name="FullTrust"), PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class EventsTab : PropertyTab
    {
        private IDesignerHost currentHost;
        private IServiceProvider sp;
        private bool sunkEvent;

        public EventsTab(IServiceProvider sp)
        {
            this.sp = sp;
        }

        public override bool CanExtend(object extendee)
        {
            return !Marshal.IsComObject(extendee);
        }

        public override PropertyDescriptor GetDefaultProperty(object obj)
        {
            IEventBindingService eventPropertyService = this.GetEventPropertyService(obj, null);
            if (eventPropertyService != null)
            {
                EventDescriptor defaultEvent = TypeDescriptor.GetDefaultEvent(obj);
                if (defaultEvent != null)
                {
                    return eventPropertyService.GetEventProperty(defaultEvent);
                }
            }
            return null;
        }

        private IEventBindingService GetEventPropertyService(object obj, ITypeDescriptorContext context)
        {
            IEventBindingService service = null;
            if (!this.sunkEvent)
            {
                IDesignerEventService service2 = (IDesignerEventService) this.sp.GetService(typeof(IDesignerEventService));
                if (service2 != null)
                {
                    service2.ActiveDesignerChanged += new ActiveDesignerEventHandler(this.OnActiveDesignerChanged);
                }
                this.sunkEvent = true;
            }
            if ((service == null) && (this.currentHost != null))
            {
                service = (IEventBindingService) this.currentHost.GetService(typeof(IEventBindingService));
            }
            if ((service == null) && (obj is IComponent))
            {
                ISite site = ((IComponent) obj).Site;
                if (site != null)
                {
                    service = (IEventBindingService) site.GetService(typeof(IEventBindingService));
                }
            }
            if ((service == null) && (context != null))
            {
                service = (IEventBindingService) context.GetService(typeof(IEventBindingService));
            }
            return service;
        }

        public override PropertyDescriptorCollection GetProperties(object component, Attribute[] attributes)
        {
            return this.GetProperties(null, component, attributes);
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object component, Attribute[] attributes)
        {
            IEventBindingService eventPropertyService = this.GetEventPropertyService(component, context);
            if (eventPropertyService == null)
            {
                return new PropertyDescriptorCollection(null);
            }
            EventDescriptorCollection events = TypeDescriptor.GetEvents(component, attributes);
            PropertyDescriptorCollection eventProperties = eventPropertyService.GetEventProperties(events);
            Attribute[] destinationArray = new Attribute[attributes.Length + 1];
            Array.Copy(attributes, 0, destinationArray, 0, attributes.Length);
            destinationArray[attributes.Length] = DesignerSerializationVisibilityAttribute.Content;
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(component, destinationArray);
            if (properties.Count > 0)
            {
                ArrayList list = null;
                for (int i = 0; i < properties.Count; i++)
                {
                    PropertyDescriptor oldPropertyDescriptor = properties[i];
                    if (oldPropertyDescriptor.Converter.GetPropertiesSupported() && (TypeDescriptor.GetEvents(oldPropertyDescriptor.GetValue(component), attributes).Count > 0))
                    {
                        if (list == null)
                        {
                            list = new ArrayList();
                        }
                        oldPropertyDescriptor = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { MergablePropertyAttribute.No });
                        list.Add(oldPropertyDescriptor);
                    }
                }
                if (list != null)
                {
                    PropertyDescriptor[] array = new PropertyDescriptor[list.Count];
                    list.CopyTo(array, 0);
                    PropertyDescriptor[] descriptorArray2 = new PropertyDescriptor[eventProperties.Count + array.Length];
                    eventProperties.CopyTo(descriptorArray2, 0);
                    Array.Copy(array, 0, descriptorArray2, eventProperties.Count, array.Length);
                    eventProperties = new PropertyDescriptorCollection(descriptorArray2);
                }
            }
            return eventProperties;
        }

        private void OnActiveDesignerChanged(object sender, ActiveDesignerEventArgs adevent)
        {
            this.currentHost = adevent.NewDesigner;
        }

        public override string HelpKeyword
        {
            get
            {
                return "Events";
            }
        }

        public override string TabName
        {
            get
            {
                return System.Windows.Forms.SR.GetString("PBRSToolTipEvents");
            }
        }
    }
}

