namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    internal sealed class ReflectEventDescriptor : EventDescriptor
    {
        private MethodInfo addMethod;
        private static readonly Type[] argsNone = new Type[0];
        private readonly Type componentClass;
        private bool filledMethods;
        private static readonly object noDefault = new object();
        private EventInfo realEvent;
        private MethodInfo removeMethod;
        private Type type;

        public ReflectEventDescriptor(Type componentClass, EventInfo eventInfo) : base(eventInfo.Name, new Attribute[0])
        {
            if (componentClass == null)
            {
                throw new ArgumentException(SR.GetString("InvalidNullArgument", new object[] { "componentClass" }));
            }
            this.componentClass = componentClass;
            this.realEvent = eventInfo;
        }

        public ReflectEventDescriptor(Type componentType, EventDescriptor oldReflectEventDescriptor, Attribute[] attributes) : base(oldReflectEventDescriptor, attributes)
        {
            this.componentClass = componentType;
            this.type = oldReflectEventDescriptor.EventType;
            ReflectEventDescriptor descriptor = oldReflectEventDescriptor as ReflectEventDescriptor;
            if (descriptor != null)
            {
                this.addMethod = descriptor.addMethod;
                this.removeMethod = descriptor.removeMethod;
                this.filledMethods = true;
            }
        }

        public ReflectEventDescriptor(Type componentClass, string name, Type type, Attribute[] attributes) : base(name, attributes)
        {
            if (componentClass == null)
            {
                throw new ArgumentException(SR.GetString("InvalidNullArgument", new object[] { "componentClass" }));
            }
            if ((type == null) || !typeof(Delegate).IsAssignableFrom(type))
            {
                throw new ArgumentException(SR.GetString("ErrorInvalidEventType", new object[] { name }));
            }
            this.componentClass = componentClass;
            this.type = type;
        }

        public override void AddEventHandler(object component, Delegate value)
        {
            this.FillMethods();
            if (component != null)
            {
                ISite site = MemberDescriptor.GetSite(component);
                IComponentChangeService service = null;
                if (site != null)
                {
                    service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                }
                if (service != null)
                {
                    try
                    {
                        service.OnComponentChanging(component, this);
                    }
                    catch (CheckoutException exception)
                    {
                        if (exception != CheckoutException.Canceled)
                        {
                            throw exception;
                        }
                        return;
                    }
                }
                bool flag = false;
                if ((site != null) && site.DesignMode)
                {
                    if (this.EventType != value.GetType())
                    {
                        throw new ArgumentException(SR.GetString("ErrorInvalidEventHandler", new object[] { this.Name }));
                    }
                    IDictionaryService service2 = (IDictionaryService) site.GetService(typeof(IDictionaryService));
                    if (service2 != null)
                    {
                        Delegate a = (Delegate) service2.GetValue(this);
                        a = Delegate.Combine(a, value);
                        service2.SetValue(this, a);
                        flag = true;
                    }
                }
                if (!flag)
                {
                    SecurityUtils.MethodInfoInvoke(this.addMethod, component, new object[] { value });
                }
                if (service != null)
                {
                    service.OnComponentChanged(component, this, null, value);
                }
            }
        }

        protected override void FillAttributes(IList attributes)
        {
            this.FillMethods();
            if (this.realEvent != null)
            {
                this.FillEventInfoAttribute(this.realEvent, attributes);
            }
            else
            {
                this.FillSingleMethodAttribute(this.removeMethod, attributes);
                this.FillSingleMethodAttribute(this.addMethod, attributes);
            }
            base.FillAttributes(attributes);
        }

        private void FillEventInfoAttribute(EventInfo realEventInfo, IList attributes)
        {
            string name = realEventInfo.Name;
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            Type reflectedType = realEventInfo.ReflectedType;
            int num = 0;
            while (reflectedType != typeof(object))
            {
                num++;
                reflectedType = reflectedType.BaseType;
            }
            if (num > 0)
            {
                reflectedType = realEventInfo.ReflectedType;
                Attribute[][] attributeArray = new Attribute[num][];
                while (reflectedType != typeof(object))
                {
                    MemberInfo member = reflectedType.GetEvent(name, bindingAttr);
                    if (member != null)
                    {
                        attributeArray[--num] = ReflectTypeDescriptionProvider.ReflectGetAttributes(member);
                    }
                    reflectedType = reflectedType.BaseType;
                }
                foreach (Attribute[] attributeArray2 in attributeArray)
                {
                    if (attributeArray2 != null)
                    {
                        foreach (Attribute attribute in attributeArray2)
                        {
                            attributes.Add(attribute);
                        }
                    }
                }
            }
        }

        private void FillMethods()
        {
            if (!this.filledMethods)
            {
                if (this.realEvent == null)
                {
                    this.realEvent = this.componentClass.GetEvent(this.Name);
                    if (this.realEvent != null)
                    {
                        this.FillMethods();
                        return;
                    }
                    Type[] args = new Type[] { this.type };
                    this.addMethod = MemberDescriptor.FindMethod(this.componentClass, "AddOn" + this.Name, args, typeof(void));
                    this.removeMethod = MemberDescriptor.FindMethod(this.componentClass, "RemoveOn" + this.Name, args, typeof(void));
                    if ((this.addMethod == null) || (this.removeMethod == null))
                    {
                        throw new ArgumentException(SR.GetString("ErrorMissingEventAccessors", new object[] { this.Name }));
                    }
                }
                else
                {
                    this.addMethod = this.realEvent.GetAddMethod();
                    this.removeMethod = this.realEvent.GetRemoveMethod();
                    EventInfo info = null;
                    if ((this.addMethod == null) || (this.removeMethod == null))
                    {
                        Type baseType = this.componentClass.BaseType;
                        while ((baseType != null) && (baseType != typeof(object)))
                        {
                            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                            EventInfo info2 = baseType.GetEvent(this.realEvent.Name, bindingAttr);
                            if (info2.GetAddMethod() != null)
                            {
                                info = info2;
                                break;
                            }
                        }
                    }
                    if (info != null)
                    {
                        this.addMethod = info.GetAddMethod();
                        this.removeMethod = info.GetRemoveMethod();
                        this.type = info.EventHandlerType;
                    }
                    else
                    {
                        this.type = this.realEvent.EventHandlerType;
                    }
                }
                this.filledMethods = true;
            }
        }

        private void FillSingleMethodAttribute(MethodInfo realMethodInfo, IList attributes)
        {
            string name = realMethodInfo.Name;
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            Type reflectedType = realMethodInfo.ReflectedType;
            int num = 0;
            while ((reflectedType != null) && (reflectedType != typeof(object)))
            {
                num++;
                reflectedType = reflectedType.BaseType;
            }
            if (num > 0)
            {
                reflectedType = realMethodInfo.ReflectedType;
                Attribute[][] attributeArray = new Attribute[num][];
                while ((reflectedType != null) && (reflectedType != typeof(object)))
                {
                    MemberInfo method = reflectedType.GetMethod(name, bindingAttr);
                    if (method != null)
                    {
                        attributeArray[--num] = ReflectTypeDescriptionProvider.ReflectGetAttributes(method);
                    }
                    reflectedType = reflectedType.BaseType;
                }
                foreach (Attribute[] attributeArray2 in attributeArray)
                {
                    if (attributeArray2 != null)
                    {
                        foreach (Attribute attribute in attributeArray2)
                        {
                            attributes.Add(attribute);
                        }
                    }
                }
            }
        }

        public override void RemoveEventHandler(object component, Delegate value)
        {
            this.FillMethods();
            if (component != null)
            {
                ISite site = MemberDescriptor.GetSite(component);
                IComponentChangeService service = null;
                if (site != null)
                {
                    service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                }
                if (service != null)
                {
                    try
                    {
                        service.OnComponentChanging(component, this);
                    }
                    catch (CheckoutException exception)
                    {
                        if (exception != CheckoutException.Canceled)
                        {
                            throw exception;
                        }
                        return;
                    }
                }
                bool flag = false;
                if ((site != null) && site.DesignMode)
                {
                    IDictionaryService service2 = (IDictionaryService) site.GetService(typeof(IDictionaryService));
                    if (service2 != null)
                    {
                        Delegate source = (Delegate) service2.GetValue(this);
                        source = Delegate.Remove(source, value);
                        service2.SetValue(this, source);
                        flag = true;
                    }
                }
                if (!flag)
                {
                    SecurityUtils.MethodInfoInvoke(this.removeMethod, component, new object[] { value });
                }
                if (service != null)
                {
                    service.OnComponentChanged(component, this, null, value);
                }
            }
        }

        public override Type ComponentType
        {
            get
            {
                return this.componentClass;
            }
        }

        public override Type EventType
        {
            get
            {
                this.FillMethods();
                return this.type;
            }
        }

        public override bool IsMulticast
        {
            get
            {
                return typeof(MulticastDelegate).IsAssignableFrom(this.EventType);
            }
        }
    }
}

