namespace System.ComponentModel.Design
{
    using Microsoft.Internal.Performance;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;
    using System.Text;
    using System.Windows.Forms;

    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust"), PermissionSet(SecurityAction.LinkDemand, Name="FullTrust")]
    public abstract class EventBindingService : IEventBindingService
    {
        private Hashtable _eventProperties;
        private IServiceProvider _provider;
        private static CodeMarkers codemarkers = CodeMarkers.Instance;
        private IComponent showCodeComponent;
        private EventDescriptor showCodeEventDescriptor;
        private string showCodeMethodName;

        protected EventBindingService(IServiceProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            this._provider = provider;
        }

        protected abstract string CreateUniqueMethodName(IComponent component, EventDescriptor e);
        protected virtual void FreeMethod(IComponent component, EventDescriptor e, string methodName)
        {
        }

        protected abstract ICollection GetCompatibleMethods(EventDescriptor e);
        private string GetEventDescriptorHashCode(EventDescriptor eventDesc)
        {
            StringBuilder builder = new StringBuilder(eventDesc.Name);
            builder.Append(eventDesc.EventType.GetHashCode().ToString(CultureInfo.InvariantCulture));
            foreach (Attribute attribute in eventDesc.Attributes)
            {
                builder.Append(attribute.GetHashCode().ToString(CultureInfo.InvariantCulture));
            }
            return builder.ToString();
        }

        protected object GetService(System.Type serviceType)
        {
            if (this._provider != null)
            {
                return this._provider.GetService(serviceType);
            }
            return null;
        }

        private bool HasGenericArgument(EventDescriptor ed)
        {
            if ((ed != null) && (ed.ComponentType != null))
            {
                EventInfo info = ed.ComponentType.GetEvent(ed.Name);
                if ((info == null) || !info.EventHandlerType.IsGenericType)
                {
                    return false;
                }
                System.Type[] genericArguments = info.EventHandlerType.GetGenericArguments();
                if ((genericArguments != null) && (genericArguments.Length > 0))
                {
                    for (int i = 0; i < genericArguments.Length; i++)
                    {
                        if (genericArguments[i].IsGenericType)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        protected abstract bool ShowCode();
        protected abstract bool ShowCode(int lineNumber);
        protected abstract bool ShowCode(IComponent component, EventDescriptor e, string methodName);
        private void ShowCodeIdle(object sender, EventArgs e)
        {
            Application.Idle -= new EventHandler(this.ShowCodeIdle);
            try
            {
                this.ShowCode(this.showCodeComponent, this.showCodeEventDescriptor, this.showCodeMethodName);
            }
            finally
            {
                this.showCodeComponent = null;
                this.showCodeEventDescriptor = null;
                this.showCodeMethodName = null;
                codemarkers.CodeMarker(CodeMarkerEvent.perfFXDesignShowCode);
            }
        }

        string IEventBindingService.CreateUniqueMethodName(IComponent component, EventDescriptor e)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            return this.CreateUniqueMethodName(component, e);
        }

        ICollection IEventBindingService.GetCompatibleMethods(EventDescriptor e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            return this.GetCompatibleMethods(e);
        }

        EventDescriptor IEventBindingService.GetEvent(PropertyDescriptor property)
        {
            if (property is EventPropertyDescriptor)
            {
                return ((EventPropertyDescriptor) property).Event;
            }
            return null;
        }

        PropertyDescriptorCollection IEventBindingService.GetEventProperties(EventDescriptorCollection events)
        {
            if (events == null)
            {
                throw new ArgumentNullException("events");
            }
            List<PropertyDescriptor> list = new List<PropertyDescriptor>(events.Count);
            if (this._eventProperties == null)
            {
                this._eventProperties = new Hashtable();
            }
            for (int i = 0; i < events.Count; i++)
            {
                if (!this.HasGenericArgument(events[i]))
                {
                    object eventDescriptorHashCode = this.GetEventDescriptorHashCode(events[i]);
                    PropertyDescriptor item = (PropertyDescriptor) this._eventProperties[eventDescriptorHashCode];
                    if (item == null)
                    {
                        item = new EventPropertyDescriptor(events[i], this);
                        this._eventProperties[eventDescriptorHashCode] = item;
                    }
                    list.Add(item);
                }
            }
            return new PropertyDescriptorCollection(list.ToArray());
        }

        PropertyDescriptor IEventBindingService.GetEventProperty(EventDescriptor e)
        {
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            if (this._eventProperties == null)
            {
                this._eventProperties = new Hashtable();
            }
            object eventDescriptorHashCode = this.GetEventDescriptorHashCode(e);
            PropertyDescriptor descriptor = (PropertyDescriptor) this._eventProperties[eventDescriptorHashCode];
            if (descriptor == null)
            {
                descriptor = new EventPropertyDescriptor(e, this);
                this._eventProperties[eventDescriptorHashCode] = descriptor;
            }
            return descriptor;
        }

        bool IEventBindingService.ShowCode()
        {
            return this.ShowCode();
        }

        bool IEventBindingService.ShowCode(int lineNumber)
        {
            return this.ShowCode(lineNumber);
        }

        bool IEventBindingService.ShowCode(IComponent component, EventDescriptor e)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (e == null)
            {
                throw new ArgumentNullException("e");
            }
            string str = (string) ((IEventBindingService) this).GetEventProperty(e).GetValue(component);
            if (str == null)
            {
                return false;
            }
            this.showCodeComponent = component;
            this.showCodeEventDescriptor = e;
            this.showCodeMethodName = str;
            Application.Idle += new EventHandler(this.ShowCodeIdle);
            return true;
        }

        protected virtual void UseMethod(IComponent component, EventDescriptor e, string methodName)
        {
        }

        protected virtual void ValidateMethodName(string methodName)
        {
        }

        private class EventPropertyDescriptor : PropertyDescriptor
        {
            private TypeConverter _converter;
            private EventDescriptor _eventDesc;
            private EventBindingService _eventSvc;

            internal EventPropertyDescriptor(EventDescriptor eventDesc, EventBindingService eventSvc) : base(eventDesc, null)
            {
                this._eventDesc = eventDesc;
                this._eventSvc = eventSvc;
            }

            public override bool CanResetValue(object component)
            {
                return (this.GetValue(component) != null);
            }

            public override object GetValue(object component)
            {
                if (component == null)
                {
                    throw new ArgumentNullException("component");
                }
                ISite site = null;
                if (component is IComponent)
                {
                    site = ((IComponent) component).Site;
                }
                if (site == null)
                {
                    IReferenceService service = this._eventSvc._provider.GetService(typeof(IReferenceService)) as IReferenceService;
                    if (service != null)
                    {
                        IComponent component2 = service.GetComponent(component);
                        if (component2 != null)
                        {
                            site = component2.Site;
                        }
                    }
                }
                if (site == null)
                {
                    return null;
                }
                IDictionaryService service2 = (IDictionaryService) site.GetService(typeof(IDictionaryService));
                if (service2 == null)
                {
                    return null;
                }
                return (string) service2.GetValue(new ReferenceEventClosure(component, this));
            }

            public override void ResetValue(object component)
            {
                this.SetValue(component, null);
            }

            public override void SetValue(object component, object value)
            {
                if (this.IsReadOnly)
                {
                    Exception exception = new InvalidOperationException(System.Design.SR.GetString("EventBindingServiceEventReadOnly", new object[] { this.Name })) {
                        HelpLink = "EventBindingServiceEventReadOnly"
                    };
                    throw exception;
                }
                if ((value != null) && !(value is string))
                {
                    Exception exception2 = new ArgumentException(System.Design.SR.GetString("EventBindingServiceBadArgType", new object[] { this.Name, typeof(string).Name })) {
                        HelpLink = "EventBindingServiceBadArgType"
                    };
                    throw exception2;
                }
                string objB = (string) value;
                if ((objB != null) && (objB.Length == 0))
                {
                    objB = null;
                }
                ISite site = null;
                if (component is IComponent)
                {
                    site = ((IComponent) component).Site;
                }
                if (site == null)
                {
                    IReferenceService service = this._eventSvc._provider.GetService(typeof(IReferenceService)) as IReferenceService;
                    if (service != null)
                    {
                        IComponent component2 = service.GetComponent(component);
                        if (component2 != null)
                        {
                            site = component2.Site;
                        }
                    }
                }
                if (site == null)
                {
                    Exception exception3 = new InvalidOperationException(System.Design.SR.GetString("EventBindingServiceNoSite")) {
                        HelpLink = "EventBindingServiceNoSite"
                    };
                    throw exception3;
                }
                IDictionaryService service2 = site.GetService(typeof(IDictionaryService)) as IDictionaryService;
                if (service2 == null)
                {
                    Exception exception4 = new InvalidOperationException(System.Design.SR.GetString("EventBindingServiceMissingService", new object[] { typeof(IDictionaryService).Name })) {
                        HelpLink = "EventBindingServiceMissingService"
                    };
                    throw exception4;
                }
                ReferenceEventClosure key = new ReferenceEventClosure(component, this);
                string objA = (string) service2.GetValue(key);
                if (!object.ReferenceEquals(objA, objB) && (((objA == null) || (objB == null)) || !objA.Equals(objB)))
                {
                    if (objB != null)
                    {
                        this._eventSvc.ValidateMethodName(objB);
                    }
                    IDesignerHost host = site.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    DesignerTransaction transaction = null;
                    if (host != null)
                    {
                        transaction = host.CreateTransaction(System.Design.SR.GetString("EventBindingServiceSetValue", new object[] { site.Name, objB }));
                    }
                    try
                    {
                        IComponentChangeService service3 = site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                        if (service3 != null)
                        {
                            try
                            {
                                service3.OnComponentChanging(component, this);
                                service3.OnComponentChanging(component, this.Event);
                            }
                            catch (CheckoutException exception5)
                            {
                                if (exception5 != CheckoutException.Canceled)
                                {
                                    throw;
                                }
                                return;
                            }
                        }
                        if (objB != null)
                        {
                            this._eventSvc.UseMethod((IComponent) component, this._eventDesc, objB);
                        }
                        if (objA != null)
                        {
                            this._eventSvc.FreeMethod((IComponent) component, this._eventDesc, objA);
                        }
                        service2.SetValue(key, objB);
                        if (service3 != null)
                        {
                            service3.OnComponentChanged(component, this.Event, null, null);
                            service3.OnComponentChanged(component, this, objA, objB);
                        }
                        this.OnValueChanged(component, EventArgs.Empty);
                        if (transaction != null)
                        {
                            transaction.Commit();
                        }
                    }
                    finally
                    {
                        if (transaction != null)
                        {
                            ((IDisposable) transaction).Dispose();
                        }
                    }
                }
            }

            public override bool ShouldSerializeValue(object component)
            {
                return this.CanResetValue(component);
            }

            public override System.Type ComponentType
            {
                get
                {
                    return this._eventDesc.ComponentType;
                }
            }

            public override TypeConverter Converter
            {
                get
                {
                    if (this._converter == null)
                    {
                        this._converter = new EventConverter(this._eventDesc);
                    }
                    return this._converter;
                }
            }

            internal EventDescriptor Event
            {
                get
                {
                    return this._eventDesc;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return this.Attributes[typeof(ReadOnlyAttribute)].Equals(ReadOnlyAttribute.Yes);
                }
            }

            public override System.Type PropertyType
            {
                get
                {
                    return this._eventDesc.EventType;
                }
            }

            private class EventConverter : TypeConverter
            {
                private EventDescriptor _evt;

                internal EventConverter(EventDescriptor evt)
                {
                    this._evt = evt;
                }

                public override bool CanConvertFrom(ITypeDescriptorContext context, System.Type sourceType)
                {
                    return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
                }

                public override bool CanConvertTo(ITypeDescriptorContext context, System.Type destinationType)
                {
                    return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
                }

                public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
                {
                    if (value != null)
                    {
                        if (!(value is string))
                        {
                            return base.ConvertFrom(context, culture, value);
                        }
                        if (((string) value).Length == 0)
                        {
                            return null;
                        }
                    }
                    return value;
                }

                public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, System.Type destinationType)
                {
                    if (!(destinationType == typeof(string)))
                    {
                        return base.ConvertTo(context, culture, value, destinationType);
                    }
                    if (value != null)
                    {
                        return value;
                    }
                    return string.Empty;
                }

                public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
                {
                    string[] values = null;
                    if (context != null)
                    {
                        IEventBindingService service = (IEventBindingService) context.GetService(typeof(IEventBindingService));
                        if (service != null)
                        {
                            ICollection compatibleMethods = service.GetCompatibleMethods(this._evt);
                            values = new string[compatibleMethods.Count];
                            int num = 0;
                            foreach (string str in compatibleMethods)
                            {
                                values[num++] = str;
                            }
                        }
                    }
                    return new TypeConverter.StandardValuesCollection(values);
                }

                public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
                {
                    return false;
                }

                public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
                {
                    return true;
                }
            }

            private class ReferenceEventClosure
            {
                private EventBindingService.EventPropertyDescriptor propertyDescriptor;
                private object reference;

                public ReferenceEventClosure(object reference, EventBindingService.EventPropertyDescriptor prop)
                {
                    this.reference = reference;
                    this.propertyDescriptor = prop;
                }

                public override bool Equals(object otherClosure)
                {
                    if (!(otherClosure is EventBindingService.EventPropertyDescriptor.ReferenceEventClosure))
                    {
                        return false;
                    }
                    EventBindingService.EventPropertyDescriptor.ReferenceEventClosure closure = (EventBindingService.EventPropertyDescriptor.ReferenceEventClosure) otherClosure;
                    return ((closure.reference == this.reference) && closure.propertyDescriptor.Equals(this.propertyDescriptor));
                }

                public override int GetHashCode()
                {
                    return (this.reference.GetHashCode() * this.propertyDescriptor.GetHashCode());
                }
            }
        }
    }
}

