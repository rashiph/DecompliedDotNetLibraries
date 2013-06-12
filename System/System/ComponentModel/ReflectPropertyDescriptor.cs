namespace System.ComponentModel
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Globalization;
    using System.Reflection;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, SharedState=true)]
    internal sealed class ReflectPropertyDescriptor : PropertyDescriptor
    {
        private object ambientValue;
        private static readonly Type[] argsNone = new Type[0];
        private static readonly int BitAmbientValueQueried = BitVector32.CreateMask(BitReadOnlyChecked);
        private static readonly int BitChangedQueried = BitVector32.CreateMask(BitResetQueried);
        private static readonly int BitDefaultValueQueried = BitVector32.CreateMask();
        private static readonly int BitGetQueried = BitVector32.CreateMask(BitDefaultValueQueried);
        private static readonly int BitIPropChangedQueried = BitVector32.CreateMask(BitChangedQueried);
        private static readonly int BitReadOnlyChecked = BitVector32.CreateMask(BitIPropChangedQueried);
        private static readonly int BitResetQueried = BitVector32.CreateMask(BitShouldSerializeQueried);
        private static readonly int BitSetOnDemand = BitVector32.CreateMask(BitAmbientValueQueried);
        private static readonly int BitSetQueried = BitVector32.CreateMask(BitGetQueried);
        private static readonly int BitShouldSerializeQueried = BitVector32.CreateMask(BitSetQueried);
        private Type componentClass;
        private object defaultValue;
        private MethodInfo getMethod;
        private static readonly object noValue = new object();
        private static TraceSwitch PropDescCreateSwitch = new TraceSwitch("PropDescCreate", "ReflectPropertyDescriptor: Dump errors when creating property info");
        private static TraceSwitch PropDescSwitch = new TraceSwitch("PropDesc", "ReflectPropertyDescriptor: Debug property descriptor");
        private static TraceSwitch PropDescUsageSwitch = new TraceSwitch("PropDescUsage", "ReflectPropertyDescriptor: Debug propertydescriptor usage");
        private PropertyInfo propInfo;
        private EventDescriptor realChangedEvent;
        private EventDescriptor realIPropChangedEvent;
        private Type receiverType;
        private MethodInfo resetMethod;
        private MethodInfo setMethod;
        private MethodInfo shouldSerializeMethod;
        private BitVector32 state;
        private Type type;

        public ReflectPropertyDescriptor(Type componentClass, PropertyDescriptor oldReflectPropertyDescriptor, Attribute[] attributes) : base(oldReflectPropertyDescriptor, attributes)
        {
            this.state = new BitVector32();
            this.componentClass = componentClass;
            this.type = oldReflectPropertyDescriptor.PropertyType;
            if (componentClass == null)
            {
                throw new ArgumentException(SR.GetString("InvalidNullArgument", new object[] { "componentClass" }));
            }
            ReflectPropertyDescriptor descriptor = oldReflectPropertyDescriptor as ReflectPropertyDescriptor;
            if (descriptor != null)
            {
                if (descriptor.ComponentType == componentClass)
                {
                    this.propInfo = descriptor.propInfo;
                    this.getMethod = descriptor.getMethod;
                    this.setMethod = descriptor.setMethod;
                    this.shouldSerializeMethod = descriptor.shouldSerializeMethod;
                    this.resetMethod = descriptor.resetMethod;
                    this.defaultValue = descriptor.defaultValue;
                    this.ambientValue = descriptor.ambientValue;
                    this.state = descriptor.state;
                }
                if (attributes != null)
                {
                    foreach (Attribute attribute in attributes)
                    {
                        DefaultValueAttribute attribute2 = attribute as DefaultValueAttribute;
                        if (attribute2 != null)
                        {
                            this.defaultValue = attribute2.Value;
                            this.state[BitDefaultValueQueried] = true;
                        }
                        else
                        {
                            AmbientValueAttribute attribute3 = attribute as AmbientValueAttribute;
                            if (attribute3 != null)
                            {
                                this.ambientValue = attribute3.Value;
                                this.state[BitAmbientValueQueried] = true;
                            }
                        }
                    }
                }
            }
        }

        public ReflectPropertyDescriptor(Type componentClass, string name, Type type, Attribute[] attributes) : base(name, attributes)
        {
            this.state = new BitVector32();
            try
            {
                if (type == null)
                {
                    throw new ArgumentException(SR.GetString("ErrorInvalidPropertyType", new object[] { name }));
                }
                if (componentClass == null)
                {
                    throw new ArgumentException(SR.GetString("InvalidNullArgument", new object[] { "componentClass" }));
                }
                this.type = type;
                this.componentClass = componentClass;
            }
            catch (Exception exception)
            {
                throw exception;
            }
        }

        public ReflectPropertyDescriptor(Type componentClass, string name, Type type, PropertyInfo propInfo, MethodInfo getMethod, MethodInfo setMethod, Attribute[] attrs) : this(componentClass, name, type, attrs)
        {
            this.propInfo = propInfo;
            this.getMethod = getMethod;
            this.setMethod = setMethod;
            if (((getMethod != null) && (propInfo != null)) && (setMethod == null))
            {
                this.state[BitGetQueried | BitSetOnDemand] = true;
            }
            else
            {
                this.state[BitGetQueried | BitSetQueried] = true;
            }
        }

        public ReflectPropertyDescriptor(Type componentClass, string name, Type type, Type receiverType, MethodInfo getMethod, MethodInfo setMethod, Attribute[] attrs) : this(componentClass, name, type, attrs)
        {
            this.receiverType = receiverType;
            this.getMethod = getMethod;
            this.setMethod = setMethod;
            this.state[BitGetQueried | BitSetQueried] = true;
        }

        public override void AddValueChanged(object component, EventHandler handler)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            EventDescriptor changedEventValue = this.ChangedEventValue;
            if ((changedEventValue != null) && changedEventValue.EventType.IsInstanceOfType(handler))
            {
                changedEventValue.AddEventHandler(component, handler);
            }
            else
            {
                if (base.GetValueChangedHandler(component) == null)
                {
                    EventDescriptor iPropChangedEventValue = this.IPropChangedEventValue;
                    if (iPropChangedEventValue != null)
                    {
                        iPropChangedEventValue.AddEventHandler(component, new PropertyChangedEventHandler(this.OnINotifyPropertyChanged));
                    }
                }
                base.AddValueChanged(component, handler);
            }
        }

        public override bool CanResetValue(object component)
        {
            if (this.IsExtender || this.IsReadOnly)
            {
                return false;
            }
            if (this.DefaultValue != noValue)
            {
                return !object.Equals(this.GetValue(component), this.DefaultValue);
            }
            if (this.ResetMethodValue != null)
            {
                if (this.ShouldSerializeMethodValue != null)
                {
                    component = this.GetInvocationTarget(this.componentClass, component);
                    try
                    {
                        return (bool) this.ShouldSerializeMethodValue.Invoke(component, null);
                    }
                    catch
                    {
                    }
                }
                return true;
            }
            return ((this.AmbientValue != noValue) && this.ShouldSerializeValue(component));
        }

        internal bool ExtenderCanResetValue(IExtenderProvider provider, object component)
        {
            if (this.DefaultValue != noValue)
            {
                return !object.Equals(this.ExtenderGetValue(provider, component), this.defaultValue);
            }
            if (this.ResetMethodValue == null)
            {
                return false;
            }
            MethodInfo shouldSerializeMethodValue = this.ShouldSerializeMethodValue;
            if (shouldSerializeMethodValue != null)
            {
                try
                {
                    provider = (IExtenderProvider) this.GetInvocationTarget(this.componentClass, provider);
                    return (bool) shouldSerializeMethodValue.Invoke(provider, new object[] { component });
                }
                catch
                {
                }
            }
            return true;
        }

        internal Type ExtenderGetReceiverType()
        {
            return this.receiverType;
        }

        internal Type ExtenderGetType(IExtenderProvider provider)
        {
            return this.PropertyType;
        }

        internal object ExtenderGetValue(IExtenderProvider provider, object component)
        {
            if (provider != null)
            {
                provider = (IExtenderProvider) this.GetInvocationTarget(this.componentClass, provider);
                return this.GetMethodValue.Invoke(provider, new object[] { component });
            }
            return null;
        }

        internal void ExtenderResetValue(IExtenderProvider provider, object component, PropertyDescriptor notifyDesc)
        {
            if (this.DefaultValue != noValue)
            {
                this.ExtenderSetValue(provider, component, this.DefaultValue, notifyDesc);
            }
            else if (this.AmbientValue != noValue)
            {
                this.ExtenderSetValue(provider, component, this.AmbientValue, notifyDesc);
            }
            else if (this.ResetMethodValue != null)
            {
                ISite site = MemberDescriptor.GetSite(component);
                IComponentChangeService service = null;
                object oldValue = null;
                if (site != null)
                {
                    service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                }
                if (service != null)
                {
                    oldValue = this.ExtenderGetValue(provider, component);
                    try
                    {
                        service.OnComponentChanging(component, notifyDesc);
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
                provider = (IExtenderProvider) this.GetInvocationTarget(this.componentClass, provider);
                if (this.ResetMethodValue != null)
                {
                    this.ResetMethodValue.Invoke(provider, new object[] { component });
                    if (service != null)
                    {
                        object newValue = this.ExtenderGetValue(provider, component);
                        service.OnComponentChanged(component, notifyDesc, oldValue, newValue);
                    }
                }
            }
        }

        internal void ExtenderSetValue(IExtenderProvider provider, object component, object value, PropertyDescriptor notifyDesc)
        {
            if (provider != null)
            {
                ISite site = MemberDescriptor.GetSite(component);
                IComponentChangeService service = null;
                object oldValue = null;
                if (site != null)
                {
                    service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                }
                if (service != null)
                {
                    oldValue = this.ExtenderGetValue(provider, component);
                    try
                    {
                        service.OnComponentChanging(component, notifyDesc);
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
                provider = (IExtenderProvider) this.GetInvocationTarget(this.componentClass, provider);
                if (this.SetMethodValue != null)
                {
                    this.SetMethodValue.Invoke(provider, new object[] { component, value });
                    if (service != null)
                    {
                        service.OnComponentChanged(component, notifyDesc, oldValue, value);
                    }
                }
            }
        }

        internal bool ExtenderShouldSerializeValue(IExtenderProvider provider, object component)
        {
            provider = (IExtenderProvider) this.GetInvocationTarget(this.componentClass, provider);
            if (this.IsReadOnly)
            {
                if (this.ShouldSerializeMethodValue != null)
                {
                    try
                    {
                        return (bool) this.ShouldSerializeMethodValue.Invoke(provider, new object[] { component });
                    }
                    catch
                    {
                    }
                }
                return this.Attributes.Contains(DesignerSerializationVisibilityAttribute.Content);
            }
            if (this.DefaultValue != noValue)
            {
                return !object.Equals(this.DefaultValue, this.ExtenderGetValue(provider, component));
            }
            if (this.ShouldSerializeMethodValue != null)
            {
                try
                {
                    return (bool) this.ShouldSerializeMethodValue.Invoke(provider, new object[] { component });
                }
                catch
                {
                }
            }
            return true;
        }

        protected override void FillAttributes(IList attributes)
        {
            foreach (Attribute attribute in TypeDescriptor.GetAttributes(this.PropertyType))
            {
                attributes.Add(attribute);
            }
            BindingFlags bindingAttr = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            Type componentClass = this.componentClass;
            int num = 0;
            while ((componentClass != null) && (componentClass != typeof(object)))
            {
                num++;
                componentClass = componentClass.BaseType;
            }
            if (num > 0)
            {
                componentClass = this.componentClass;
                Attribute[][] attributeArray = new Attribute[num][];
                while ((componentClass != null) && (componentClass != typeof(object)))
                {
                    MemberInfo member = null;
                    if (this.IsExtender)
                    {
                        member = componentClass.GetMethod("Get" + this.Name, bindingAttr);
                    }
                    else
                    {
                        member = componentClass.GetProperty(this.Name, bindingAttr, null, this.PropertyType, new Type[0], new ParameterModifier[0]);
                    }
                    if (member != null)
                    {
                        attributeArray[--num] = ReflectTypeDescriptionProvider.ReflectGetAttributes(member);
                    }
                    componentClass = componentClass.BaseType;
                }
                foreach (Attribute[] attributeArray2 in attributeArray)
                {
                    if (attributeArray2 != null)
                    {
                        foreach (Attribute attribute2 in attributeArray2)
                        {
                            AttributeProviderAttribute attribute3 = attribute2 as AttributeProviderAttribute;
                            if (attribute3 != null)
                            {
                                Type type = Type.GetType(attribute3.TypeName);
                                if (type != null)
                                {
                                    Attribute[] attributeArray3 = null;
                                    if (!string.IsNullOrEmpty(attribute3.PropertyName))
                                    {
                                        MemberInfo[] infoArray = type.GetMember(attribute3.PropertyName);
                                        if ((infoArray.Length > 0) && (infoArray[0] != null))
                                        {
                                            attributeArray3 = ReflectTypeDescriptionProvider.ReflectGetAttributes(infoArray[0]);
                                        }
                                    }
                                    else
                                    {
                                        attributeArray3 = ReflectTypeDescriptionProvider.ReflectGetAttributes((MemberInfo) type);
                                    }
                                    if (attributeArray3 != null)
                                    {
                                        foreach (Attribute attribute4 in attributeArray3)
                                        {
                                            attributes.Add(attribute4);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                foreach (Attribute[] attributeArray4 in attributeArray)
                {
                    if (attributeArray4 != null)
                    {
                        foreach (Attribute attribute5 in attributeArray4)
                        {
                            attributes.Add(attribute5);
                        }
                    }
                }
            }
            base.FillAttributes(attributes);
            if (this.SetMethodValue == null)
            {
                attributes.Add(ReadOnlyAttribute.Yes);
            }
        }

        public override object GetValue(object component)
        {
            if (!this.IsExtender && (component != null))
            {
                component = this.GetInvocationTarget(this.componentClass, component);
                try
                {
                    return SecurityUtils.MethodInfoInvoke(this.GetMethodValue, component, null);
                }
                catch (Exception innerException)
                {
                    string name = null;
                    IComponent component2 = component as IComponent;
                    if (component2 != null)
                    {
                        ISite site = component2.Site;
                        if ((site != null) && (site.Name != null))
                        {
                            name = site.Name;
                        }
                    }
                    if (name == null)
                    {
                        name = component.GetType().FullName;
                    }
                    if (innerException is TargetInvocationException)
                    {
                        innerException = innerException.InnerException;
                    }
                    string message = innerException.Message;
                    if (message == null)
                    {
                        message = innerException.GetType().Name;
                    }
                    throw new TargetInvocationException(SR.GetString("ErrorPropertyAccessorException", new object[] { this.Name, name, message }), innerException);
                }
            }
            return null;
        }

        internal void OnINotifyPropertyChanged(object component, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || (string.Compare(e.PropertyName, this.Name, true, CultureInfo.InvariantCulture) == 0))
            {
                this.OnValueChanged(component, e);
            }
        }

        protected override void OnValueChanged(object component, EventArgs e)
        {
            if (this.state[BitChangedQueried] && (this.realChangedEvent == null))
            {
                base.OnValueChanged(component, e);
            }
        }

        public override void RemoveValueChanged(object component, EventHandler handler)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (handler == null)
            {
                throw new ArgumentNullException("handler");
            }
            EventDescriptor changedEventValue = this.ChangedEventValue;
            if ((changedEventValue != null) && changedEventValue.EventType.IsInstanceOfType(handler))
            {
                changedEventValue.RemoveEventHandler(component, handler);
            }
            else
            {
                base.RemoveValueChanged(component, handler);
                if (base.GetValueChangedHandler(component) == null)
                {
                    EventDescriptor iPropChangedEventValue = this.IPropChangedEventValue;
                    if (iPropChangedEventValue != null)
                    {
                        iPropChangedEventValue.RemoveEventHandler(component, new PropertyChangedEventHandler(this.OnINotifyPropertyChanged));
                    }
                }
            }
        }

        public override void ResetValue(object component)
        {
            object invocationTarget = this.GetInvocationTarget(this.componentClass, component);
            if (this.DefaultValue != noValue)
            {
                this.SetValue(component, this.DefaultValue);
            }
            else if (this.AmbientValue != noValue)
            {
                this.SetValue(component, this.AmbientValue);
            }
            else if (this.ResetMethodValue != null)
            {
                ISite site = MemberDescriptor.GetSite(component);
                IComponentChangeService service = null;
                object oldValue = null;
                if (site != null)
                {
                    service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                }
                if (service != null)
                {
                    oldValue = SecurityUtils.MethodInfoInvoke(this.GetMethodValue, invocationTarget, null);
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
                if (this.ResetMethodValue != null)
                {
                    SecurityUtils.MethodInfoInvoke(this.ResetMethodValue, invocationTarget, null);
                    if (service != null)
                    {
                        object newValue = SecurityUtils.MethodInfoInvoke(this.GetMethodValue, invocationTarget, null);
                        service.OnComponentChanged(component, this, oldValue, newValue);
                    }
                }
            }
        }

        public override void SetValue(object component, object value)
        {
            if (component != null)
            {
                ISite site = MemberDescriptor.GetSite(component);
                IComponentChangeService service = null;
                object oldValue = null;
                object invocationTarget = this.GetInvocationTarget(this.componentClass, component);
                if (!this.IsReadOnly)
                {
                    if (site != null)
                    {
                        service = (IComponentChangeService) site.GetService(typeof(IComponentChangeService));
                    }
                    if (service != null)
                    {
                        oldValue = SecurityUtils.MethodInfoInvoke(this.GetMethodValue, invocationTarget, null);
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
                    try
                    {
                        SecurityUtils.MethodInfoInvoke(this.SetMethodValue, invocationTarget, new object[] { value });
                        this.OnValueChanged(invocationTarget, EventArgs.Empty);
                    }
                    catch (Exception exception2)
                    {
                        value = oldValue;
                        if ((exception2 is TargetInvocationException) && (exception2.InnerException != null))
                        {
                            throw exception2.InnerException;
                        }
                        throw exception2;
                    }
                    finally
                    {
                        if (service != null)
                        {
                            service.OnComponentChanged(component, this, oldValue, value);
                        }
                    }
                }
            }
        }

        public override bool ShouldSerializeValue(object component)
        {
            component = this.GetInvocationTarget(this.componentClass, component);
            if (this.IsReadOnly)
            {
                if (this.ShouldSerializeMethodValue != null)
                {
                    try
                    {
                        return (bool) this.ShouldSerializeMethodValue.Invoke(component, null);
                    }
                    catch
                    {
                    }
                }
                return this.Attributes.Contains(DesignerSerializationVisibilityAttribute.Content);
            }
            if (this.DefaultValue != noValue)
            {
                return !object.Equals(this.DefaultValue, this.GetValue(component));
            }
            if (this.ShouldSerializeMethodValue != null)
            {
                try
                {
                    return (bool) this.ShouldSerializeMethodValue.Invoke(component, null);
                }
                catch
                {
                }
            }
            return true;
        }

        private object AmbientValue
        {
            get
            {
                if (!this.state[BitAmbientValueQueried])
                {
                    this.state[BitAmbientValueQueried] = true;
                    Attribute attribute = this.Attributes[typeof(AmbientValueAttribute)];
                    if (attribute != null)
                    {
                        this.ambientValue = ((AmbientValueAttribute) attribute).Value;
                    }
                    else
                    {
                        this.ambientValue = noValue;
                    }
                }
                return this.ambientValue;
            }
        }

        private EventDescriptor ChangedEventValue
        {
            get
            {
                if (!this.state[BitChangedQueried])
                {
                    this.state[BitChangedQueried] = true;
                    this.realChangedEvent = TypeDescriptor.GetEvents(this.ComponentType)[string.Format(CultureInfo.InvariantCulture, "{0}Changed", new object[] { this.Name })];
                }
                return this.realChangedEvent;
            }
        }

        public override Type ComponentType
        {
            get
            {
                return this.componentClass;
            }
        }

        private object DefaultValue
        {
            get
            {
                if (!this.state[BitDefaultValueQueried])
                {
                    this.state[BitDefaultValueQueried] = true;
                    Attribute attribute = this.Attributes[typeof(DefaultValueAttribute)];
                    if (attribute != null)
                    {
                        this.defaultValue = ((DefaultValueAttribute) attribute).Value;
                    }
                    else
                    {
                        this.defaultValue = noValue;
                    }
                }
                return this.defaultValue;
            }
        }

        private MethodInfo GetMethodValue
        {
            get
            {
                if (!this.state[BitGetQueried])
                {
                    this.state[BitGetQueried] = true;
                    if (this.receiverType == null)
                    {
                        if (this.propInfo == null)
                        {
                            BindingFlags bindingAttr = BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                            this.propInfo = this.componentClass.GetProperty(this.Name, bindingAttr, null, this.PropertyType, new Type[0], new ParameterModifier[0]);
                        }
                        if (this.propInfo != null)
                        {
                            this.getMethod = this.propInfo.GetGetMethod(true);
                        }
                        if (this.getMethod == null)
                        {
                            throw new InvalidOperationException(SR.GetString("ErrorMissingPropertyAccessors", new object[] { this.componentClass.FullName + "." + this.Name }));
                        }
                    }
                    else
                    {
                        this.getMethod = MemberDescriptor.FindMethod(this.componentClass, "Get" + this.Name, new Type[] { this.receiverType }, this.type);
                        if (this.getMethod == null)
                        {
                            throw new ArgumentException(SR.GetString("ErrorMissingPropertyAccessors", new object[] { this.Name }));
                        }
                    }
                }
                return this.getMethod;
            }
        }

        private EventDescriptor IPropChangedEventValue
        {
            get
            {
                if (!this.state[BitIPropChangedQueried])
                {
                    this.state[BitIPropChangedQueried] = true;
                    if (typeof(INotifyPropertyChanged).IsAssignableFrom(this.ComponentType))
                    {
                        this.realIPropChangedEvent = TypeDescriptor.GetEvents(typeof(INotifyPropertyChanged))["PropertyChanged"];
                    }
                }
                return this.realIPropChangedEvent;
            }
            set
            {
                this.realIPropChangedEvent = value;
                this.state[BitIPropChangedQueried] = true;
            }
        }

        private bool IsExtender
        {
            get
            {
                return (this.receiverType != null);
            }
        }

        public override bool IsReadOnly
        {
            get
            {
                if (this.SetMethodValue != null)
                {
                    return ((ReadOnlyAttribute) this.Attributes[typeof(ReadOnlyAttribute)]).IsReadOnly;
                }
                return true;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this.type;
            }
        }

        private MethodInfo ResetMethodValue
        {
            get
            {
                if (!this.state[BitResetQueried])
                {
                    Type[] argsNone;
                    this.state[BitResetQueried] = true;
                    if (this.receiverType == null)
                    {
                        argsNone = ReflectPropertyDescriptor.argsNone;
                    }
                    else
                    {
                        argsNone = new Type[] { this.receiverType };
                    }
                    IntSecurity.FullReflection.Assert();
                    try
                    {
                        this.resetMethod = MemberDescriptor.FindMethod(this.componentClass, "Reset" + this.Name, argsNone, typeof(void), false);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
                return this.resetMethod;
            }
        }

        private MethodInfo SetMethodValue
        {
            get
            {
                if (!this.state[BitSetQueried] && this.state[BitSetOnDemand])
                {
                    this.state[BitSetQueried] = true;
                    BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
                    string name = this.propInfo.Name;
                    if (this.setMethod == null)
                    {
                        for (Type type = this.ComponentType.BaseType; (type != null) && (type != typeof(object)); type = type.BaseType)
                        {
                            if (type == null)
                            {
                                break;
                            }
                            PropertyInfo info = type.GetProperty(name, bindingAttr, null, this.PropertyType, new Type[0], null);
                            if (info != null)
                            {
                                this.setMethod = info.GetSetMethod();
                                if (this.setMethod != null)
                                {
                                    break;
                                }
                            }
                        }
                    }
                }
                if (!this.state[BitSetQueried])
                {
                    this.state[BitSetQueried] = true;
                    if (this.receiverType == null)
                    {
                        if (this.propInfo == null)
                        {
                            BindingFlags flags2 = BindingFlags.GetProperty | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
                            this.propInfo = this.componentClass.GetProperty(this.Name, flags2, null, this.PropertyType, new Type[0], new ParameterModifier[0]);
                        }
                        if (this.propInfo != null)
                        {
                            this.setMethod = this.propInfo.GetSetMethod(true);
                        }
                    }
                    else
                    {
                        this.setMethod = MemberDescriptor.FindMethod(this.componentClass, "Set" + this.Name, new Type[] { this.receiverType, this.type }, typeof(void));
                    }
                }
                return this.setMethod;
            }
        }

        private MethodInfo ShouldSerializeMethodValue
        {
            get
            {
                if (!this.state[BitShouldSerializeQueried])
                {
                    Type[] argsNone;
                    this.state[BitShouldSerializeQueried] = true;
                    if (this.receiverType == null)
                    {
                        argsNone = ReflectPropertyDescriptor.argsNone;
                    }
                    else
                    {
                        argsNone = new Type[] { this.receiverType };
                    }
                    IntSecurity.FullReflection.Assert();
                    try
                    {
                        this.shouldSerializeMethod = MemberDescriptor.FindMethod(this.componentClass, "ShouldSerialize" + this.Name, argsNone, typeof(bool), false);
                    }
                    finally
                    {
                        CodeAccessPermission.RevertAssert();
                    }
                }
                return this.shouldSerializeMethod;
            }
        }

        public override bool SupportsChangeEvents
        {
            get
            {
                if (this.IPropChangedEventValue == null)
                {
                    return (this.ChangedEventValue != null);
                }
                return true;
            }
        }
    }
}

