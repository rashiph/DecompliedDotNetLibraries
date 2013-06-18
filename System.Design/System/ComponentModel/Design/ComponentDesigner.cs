namespace System.ComponentModel.Design
{
    using Microsoft.Internal.Performance;
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration;
    using System.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Windows.Forms.Design;

    public class ComponentDesigner : ITreeDesigner, IDesigner, IDisposable, IDesignerFilter, IComponentInitializer
    {
        private DesignerActionListCollection actionLists;
        private static CodeMarkers codemarkers = CodeMarkers.Instance;
        private IComponent component;
        private System.ComponentModel.InheritanceAttribute inheritanceAttribute;
        private Hashtable inheritedProps;
        private bool settingsKeyExplicitlySet;
        private ShadowPropertyCollection shadowProperties;
        private DesignerVerbCollection verbs;

        internal virtual bool CanBeAssociatedWith(IDesigner parentDesigner)
        {
            return true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    service.ComponentRename -= new ComponentRenameEventHandler(this.OnComponentRename);
                }
                this.component = null;
                this.inheritedProps = null;
            }
        }

        public virtual void DoDefaultAction()
        {
            IEventBindingService service = (IEventBindingService) this.GetService(typeof(IEventBindingService));
            if (service != null)
            {
                ISelectionService service2 = (ISelectionService) this.GetService(typeof(ISelectionService));
                if (service2 != null)
                {
                    ICollection selectedComponents = service2.GetSelectedComponents();
                    EventDescriptor e = null;
                    string str = null;
                    IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    DesignerTransaction transaction = null;
                    try
                    {
                        foreach (object obj2 in selectedComponents)
                        {
                            if (!(obj2 is IComponent))
                            {
                                continue;
                            }
                            EventDescriptor defaultEvent = TypeDescriptor.GetDefaultEvent(obj2);
                            PropertyDescriptor eventProperty = null;
                            string str2 = null;
                            bool flag = false;
                            if (defaultEvent != null)
                            {
                                eventProperty = service.GetEventProperty(defaultEvent);
                            }
                            if ((eventProperty != null) && !eventProperty.IsReadOnly)
                            {
                                try
                                {
                                    if ((host != null) && (transaction == null))
                                    {
                                        transaction = host.CreateTransaction(System.Design.SR.GetString("ComponentDesignerAddEvent", new object[] { defaultEvent.Name }));
                                    }
                                }
                                catch (CheckoutException exception)
                                {
                                    if (exception != CheckoutException.Canceled)
                                    {
                                        throw exception;
                                    }
                                    return;
                                }
                                str2 = (string) eventProperty.GetValue(obj2);
                                if (str2 == null)
                                {
                                    flag = true;
                                    str2 = service.CreateUniqueMethodName((IComponent) obj2, defaultEvent);
                                }
                                else
                                {
                                    flag = true;
                                    foreach (string str3 in service.GetCompatibleMethods(defaultEvent))
                                    {
                                        if (str2 == str3)
                                        {
                                            flag = false;
                                            break;
                                        }
                                    }
                                }
                                codemarkers.CodeMarker(CodeMarkerEvent.perfFXBindEventDesignToCode);
                                if (flag && (eventProperty != null))
                                {
                                    eventProperty.SetValue(obj2, str2);
                                }
                                if (this.component == obj2)
                                {
                                    e = defaultEvent;
                                    str = str2;
                                }
                            }
                        }
                    }
                    catch (InvalidOperationException)
                    {
                        if (transaction != null)
                        {
                            transaction.Cancel();
                            transaction = null;
                        }
                    }
                    finally
                    {
                        if (transaction != null)
                        {
                            transaction.Commit();
                        }
                    }
                    if ((str != null) && (e != null))
                    {
                        service.ShowCode(this.component, e);
                    }
                }
            }
        }

        ~ComponentDesigner()
        {
            this.Dispose(false);
        }

        protected virtual object GetService(Type serviceType)
        {
            if (this.component != null)
            {
                ISite site = this.component.Site;
                if (site != null)
                {
                    return site.GetService(serviceType);
                }
            }
            return null;
        }

        public virtual void Initialize(IComponent component)
        {
            this.component = component;
            bool rootComponent = false;
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if ((host != null) && (component == host.RootComponent))
            {
                rootComponent = true;
            }
            IServiceContainer site = component.Site as IServiceContainer;
            if ((site != null) && (this.GetService(typeof(DesignerCommandSet)) == null))
            {
                site.AddService(typeof(DesignerCommandSet), new CDDesignerCommandSet(this));
            }
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.ComponentRename += new ComponentRenameEventHandler(this.OnComponentRename);
            }
            if (rootComponent || !this.InheritanceAttribute.Equals(System.ComponentModel.InheritanceAttribute.NotInherited))
            {
                this.InitializeInheritedProperties(rootComponent);
            }
        }

        public virtual void InitializeExistingComponent(IDictionary defaultValues)
        {
            this.InitializeNonDefault();
        }

        private void InitializeInheritedProperties(bool rootComponent)
        {
            Hashtable hashtable = new Hashtable();
            if (!this.InheritanceAttribute.Equals(System.ComponentModel.InheritanceAttribute.InheritedReadOnly))
            {
                PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(this.Component);
                PropertyDescriptor[] array = new PropertyDescriptor[properties.Count];
                properties.CopyTo(array, 0);
                for (int i = 0; i < array.Length; i++)
                {
                    PropertyDescriptor propertyDescriptor = array[i];
                    if ((!object.Equals(propertyDescriptor.Attributes[typeof(DesignOnlyAttribute)], DesignOnlyAttribute.Yes) && ((propertyDescriptor.SerializationVisibility != DesignerSerializationVisibility.Hidden) || propertyDescriptor.IsBrowsable)) && (((PropertyDescriptor) hashtable[propertyDescriptor.Name]) == null))
                    {
                        hashtable[propertyDescriptor.Name] = new InheritedPropertyDescriptor(propertyDescriptor, this.component, rootComponent);
                    }
                }
            }
            this.inheritedProps = hashtable;
            TypeDescriptor.Refresh(this.Component);
        }

        public virtual void InitializeNewComponent(IDictionary defaultValues)
        {
            DesignerActionUIService service = (DesignerActionUIService) this.GetService(typeof(DesignerActionUIService));
            if ((service != null) && service.ShouldAutoShow(this.Component))
            {
                IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if ((host != null) && host.InTransaction)
                {
                    host.TransactionClosed += new DesignerTransactionCloseEventHandler(this.ShowDesignerActionUI);
                }
                else
                {
                    service.ShowUI(this.Component);
                }
            }
            this.OnSetComponentDefaults();
        }

        [Obsolete("This method has been deprecated. Use InitializeExistingComponent instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public virtual void InitializeNonDefault()
        {
        }

        protected System.ComponentModel.InheritanceAttribute InvokeGetInheritanceAttribute(ComponentDesigner toInvoke)
        {
            return toInvoke.InheritanceAttribute;
        }

        private Attribute[] NonBrowsableAttributes(EventDescriptor e)
        {
            Attribute[] array = new Attribute[e.Attributes.Count];
            e.Attributes.CopyTo(array, 0);
            for (int i = 0; i < array.Length; i++)
            {
                if (((array[i] != null) && typeof(BrowsableAttribute).IsInstanceOfType(array[i])) && ((BrowsableAttribute) array[i]).Browsable)
                {
                    array[i] = BrowsableAttribute.No;
                    return array;
                }
            }
            Attribute[] destinationArray = new Attribute[array.Length + 1];
            Array.Copy(array, 0, destinationArray, 0, array.Length);
            destinationArray[array.Length] = BrowsableAttribute.No;
            return destinationArray;
        }

        private void OnComponentRename(object sender, ComponentRenameEventArgs e)
        {
            if (this.Component is IPersistComponentSettings)
            {
                IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                IComponent component = (service != null) ? service.RootComponent : null;
                if (!this.settingsKeyExplicitlySet && ((e.Component == this.Component) || (e.Component == component)))
                {
                    this.ResetSettingsKey();
                }
            }
        }

        [Obsolete("This method has been deprecated. Use InitializeNewComponent instead.  http://go.microsoft.com/fwlink/?linkid=14202")]
        public virtual void OnSetComponentDefaults()
        {
            ISite site = this.Component.Site;
            if (site != null)
            {
                IComponent component = this.Component;
                PropertyDescriptor defaultProperty = TypeDescriptor.GetDefaultProperty(component);
                if ((defaultProperty != null) && defaultProperty.PropertyType.Equals(typeof(string)))
                {
                    string str = (string) defaultProperty.GetValue(component);
                    if ((str == null) || (str.Length == 0))
                    {
                        defaultProperty.SetValue(component, site.Name);
                    }
                }
            }
        }

        protected virtual void PostFilterAttributes(IDictionary attributes)
        {
            if (attributes.Contains(typeof(System.ComponentModel.InheritanceAttribute)))
            {
                this.inheritanceAttribute = attributes[typeof(System.ComponentModel.InheritanceAttribute)] as System.ComponentModel.InheritanceAttribute;
            }
            else if (!this.InheritanceAttribute.Equals(System.ComponentModel.InheritanceAttribute.NotInherited))
            {
                attributes[typeof(System.ComponentModel.InheritanceAttribute)] = this.InheritanceAttribute;
            }
        }

        protected virtual void PostFilterEvents(IDictionary events)
        {
            if (this.InheritanceAttribute.Equals(System.ComponentModel.InheritanceAttribute.InheritedReadOnly))
            {
                EventDescriptor[] array = new EventDescriptor[events.Values.Count];
                events.Values.CopyTo(array, 0);
                for (int i = 0; i < array.Length; i++)
                {
                    EventDescriptor oldEventDescriptor = array[i];
                    events[oldEventDescriptor.Name] = TypeDescriptor.CreateEvent(oldEventDescriptor.ComponentType, oldEventDescriptor, new Attribute[] { ReadOnlyAttribute.Yes });
                }
            }
        }

        protected virtual void PostFilterProperties(IDictionary properties)
        {
            if (this.inheritedProps != null)
            {
                if (this.InheritanceAttribute.Equals(System.ComponentModel.InheritanceAttribute.InheritedReadOnly))
                {
                    PropertyDescriptor[] array = new PropertyDescriptor[properties.Values.Count];
                    properties.Values.CopyTo(array, 0);
                    for (int i = 0; i < array.Length; i++)
                    {
                        PropertyDescriptor oldPropertyDescriptor = array[i];
                        properties[oldPropertyDescriptor.Name] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { ReadOnlyAttribute.Yes });
                    }
                }
                else
                {
                    foreach (DictionaryEntry entry in this.inheritedProps)
                    {
                        InheritedPropertyDescriptor descriptor2 = entry.Value as InheritedPropertyDescriptor;
                        if (descriptor2 != null)
                        {
                            PropertyDescriptor descriptor3 = (PropertyDescriptor) properties[entry.Key];
                            if (descriptor3 != null)
                            {
                                descriptor2.PropertyDescriptor = descriptor3;
                                properties[entry.Key] = descriptor2;
                            }
                        }
                    }
                }
            }
        }

        protected virtual void PreFilterAttributes(IDictionary attributes)
        {
        }

        protected virtual void PreFilterEvents(IDictionary events)
        {
        }

        protected virtual void PreFilterProperties(IDictionary properties)
        {
            if (this.Component is IPersistComponentSettings)
            {
                PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["SettingsKey"];
                if (oldPropertyDescriptor != null)
                {
                    properties["SettingsKey"] = TypeDescriptor.CreateProperty(typeof(ComponentDesigner), oldPropertyDescriptor, new Attribute[0]);
                }
            }
        }

        protected void RaiseComponentChanged(MemberDescriptor member, object oldValue, object newValue)
        {
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.OnComponentChanged(this.Component, member, oldValue, newValue);
            }
        }

        protected void RaiseComponentChanging(MemberDescriptor member)
        {
            IComponentChangeService service = (IComponentChangeService) this.GetService(typeof(IComponentChangeService));
            if (service != null)
            {
                service.OnComponentChanging(this.Component, member);
            }
        }

        private void ResetSettingsKey()
        {
            if (this.Component is IPersistComponentSettings)
            {
                this.SettingsKey = null;
                this.settingsKeyExplicitlySet = false;
            }
        }

        private bool ShouldSerializeSettingsKey()
        {
            IPersistComponentSettings component = this.Component as IPersistComponentSettings;
            if (component == null)
            {
                return false;
            }
            return (this.settingsKeyExplicitlySet || (component.SaveSettings && !string.IsNullOrEmpty(this.SettingsKey)));
        }

        internal virtual void ShowContextMenu(int x, int y)
        {
            IMenuCommandService service = (IMenuCommandService) this.GetService(typeof(IMenuCommandService));
            if (service != null)
            {
                service.ShowContextMenu(MenuCommands.SelectionMenu, x, y);
            }
        }

        private void ShowDesignerActionUI(object sender, DesignerTransactionCloseEventArgs e)
        {
            DesignerActionUIService service = (DesignerActionUIService) this.GetService(typeof(DesignerActionUIService));
            if (service != null)
            {
                service.ShowUI(this.Component);
            }
            IDesignerHost host = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (host != null)
            {
                host.TransactionClosed -= new DesignerTransactionCloseEventHandler(this.ShowDesignerActionUI);
            }
        }

        void IDesignerFilter.PostFilterAttributes(IDictionary attributes)
        {
            this.PostFilterAttributes(attributes);
        }

        void IDesignerFilter.PostFilterEvents(IDictionary events)
        {
            this.PostFilterEvents(events);
        }

        void IDesignerFilter.PostFilterProperties(IDictionary properties)
        {
            this.PostFilterProperties(properties);
        }

        void IDesignerFilter.PreFilterAttributes(IDictionary attributes)
        {
            this.PreFilterAttributes(attributes);
        }

        void IDesignerFilter.PreFilterEvents(IDictionary events)
        {
            this.PreFilterEvents(events);
        }

        void IDesignerFilter.PreFilterProperties(IDictionary properties)
        {
            this.PreFilterProperties(properties);
        }

        public virtual DesignerActionListCollection ActionLists
        {
            get
            {
                if (this.actionLists == null)
                {
                    this.actionLists = new DesignerActionListCollection();
                }
                return this.actionLists;
            }
        }

        public virtual ICollection AssociatedComponents
        {
            get
            {
                return new IComponent[0];
            }
        }

        public IComponent Component
        {
            get
            {
                return this.component;
            }
        }

        protected virtual System.ComponentModel.InheritanceAttribute InheritanceAttribute
        {
            get
            {
                if (this.inheritanceAttribute == null)
                {
                    IInheritanceService service = (IInheritanceService) this.GetService(typeof(IInheritanceService));
                    if (service != null)
                    {
                        this.inheritanceAttribute = service.GetInheritanceAttribute(this.Component);
                    }
                    else
                    {
                        this.inheritanceAttribute = System.ComponentModel.InheritanceAttribute.Default;
                    }
                }
                return this.inheritanceAttribute;
            }
        }

        protected bool Inherited
        {
            get
            {
                return !this.InheritanceAttribute.Equals(System.ComponentModel.InheritanceAttribute.NotInherited);
            }
        }

        internal bool IsRootDesigner
        {
            get
            {
                bool flag = false;
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if ((service != null) && (this.component == service.RootComponent))
                {
                    flag = true;
                }
                return flag;
            }
        }

        protected virtual IComponent ParentComponent
        {
            get
            {
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                IComponent rootComponent = service.RootComponent;
                if (rootComponent == this.Component)
                {
                    return null;
                }
                return rootComponent;
            }
        }

        private string SettingsKey
        {
            get
            {
                if (string.IsNullOrEmpty((string) this.ShadowProperties["SettingsKey"]))
                {
                    IPersistComponentSettings settings = this.Component as IPersistComponentSettings;
                    IDesignerHost service = this.GetService(typeof(IDesignerHost)) as IDesignerHost;
                    IComponent component = (service != null) ? service.RootComponent : null;
                    if ((settings != null) && (component != null))
                    {
                        if (string.IsNullOrEmpty(settings.SettingsKey))
                        {
                            if ((component != null) && (component != settings))
                            {
                                this.ShadowProperties["SettingsKey"] = string.Format(CultureInfo.CurrentCulture, "{0}.{1}", new object[] { component.Site.Name, this.Component.Site.Name });
                            }
                            else
                            {
                                this.ShadowProperties["SettingsKey"] = this.Component.Site.Name;
                            }
                        }
                        settings.SettingsKey = this.ShadowProperties["SettingsKey"] as string;
                        return settings.SettingsKey;
                    }
                }
                return (this.ShadowProperties["SettingsKey"] as string);
            }
            set
            {
                this.ShadowProperties["SettingsKey"] = value;
                this.settingsKeyExplicitlySet = true;
                IPersistComponentSettings component = this.Component as IPersistComponentSettings;
                if (component != null)
                {
                    component.SettingsKey = value;
                }
            }
        }

        protected ShadowPropertyCollection ShadowProperties
        {
            get
            {
                if (this.shadowProperties == null)
                {
                    this.shadowProperties = new ShadowPropertyCollection(this);
                }
                return this.shadowProperties;
            }
        }

        ICollection ITreeDesigner.Children
        {
            get
            {
                ICollection associatedComponents = this.AssociatedComponents;
                IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                if ((associatedComponents.Count <= 0) || (service == null))
                {
                    return new object[0];
                }
                IDesigner[] sourceArray = new IDesigner[associatedComponents.Count];
                int index = 0;
                foreach (IComponent component in associatedComponents)
                {
                    sourceArray[index] = service.GetDesigner(component);
                    if (sourceArray[index] != null)
                    {
                        index++;
                    }
                }
                if (index < sourceArray.Length)
                {
                    IDesigner[] destinationArray = new IDesigner[index];
                    Array.Copy(sourceArray, 0, destinationArray, 0, index);
                    sourceArray = destinationArray;
                }
                return sourceArray;
            }
        }

        IDesigner ITreeDesigner.Parent
        {
            get
            {
                IComponent parentComponent = this.ParentComponent;
                if (parentComponent != null)
                {
                    IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
                    if (service != null)
                    {
                        return service.GetDesigner(parentComponent);
                    }
                }
                return null;
            }
        }

        public virtual DesignerVerbCollection Verbs
        {
            get
            {
                if (this.verbs == null)
                {
                    this.verbs = new DesignerVerbCollection();
                }
                return this.verbs;
            }
        }

        private class CDDesignerCommandSet : DesignerCommandSet
        {
            private ComponentDesigner componentDesigner;

            public CDDesignerCommandSet(ComponentDesigner componentDesigner)
            {
                this.componentDesigner = componentDesigner;
            }

            public override ICollection GetCommands(string name)
            {
                if (name.Equals("Verbs"))
                {
                    return this.componentDesigner.Verbs;
                }
                if (name.Equals("ActionLists"))
                {
                    return this.componentDesigner.ActionLists;
                }
                return base.GetCommands(name);
            }
        }

        protected sealed class ShadowPropertyCollection
        {
            private Hashtable descriptors;
            private ComponentDesigner designer;
            private Hashtable properties;

            internal ShadowPropertyCollection(ComponentDesigner designer)
            {
                this.designer = designer;
            }

            public bool Contains(string propertyName)
            {
                return ((this.properties != null) && this.properties.ContainsKey(propertyName));
            }

            private PropertyDescriptor GetShadowedPropertyDescriptor(string propertyName)
            {
                if (this.descriptors == null)
                {
                    this.descriptors = new Hashtable();
                }
                PropertyDescriptor descriptor = (PropertyDescriptor) this.descriptors[propertyName];
                if (descriptor == null)
                {
                    descriptor = TypeDescriptor.GetProperties(this.designer.Component.GetType())[propertyName];
                    if (descriptor == null)
                    {
                        throw new ArgumentException(System.Design.SR.GetString("DesignerPropNotFound", new object[] { propertyName, this.designer.Component.GetType().FullName }));
                    }
                    this.descriptors[propertyName] = descriptor;
                }
                return descriptor;
            }

            internal bool ShouldSerializeValue(string propertyName, object defaultValue)
            {
                if (propertyName == null)
                {
                    throw new ArgumentNullException("propertyName");
                }
                if (this.Contains(propertyName))
                {
                    return !object.Equals(this[propertyName], defaultValue);
                }
                return this.GetShadowedPropertyDescriptor(propertyName).ShouldSerializeValue(this.designer.Component);
            }

            public object this[string propertyName]
            {
                get
                {
                    if (propertyName == null)
                    {
                        throw new ArgumentNullException("propertyName");
                    }
                    if ((this.properties != null) && this.properties.ContainsKey(propertyName))
                    {
                        return this.properties[propertyName];
                    }
                    return this.GetShadowedPropertyDescriptor(propertyName).GetValue(this.designer.Component);
                }
                set
                {
                    if (this.properties == null)
                    {
                        this.properties = new Hashtable();
                    }
                    this.properties[propertyName] = value;
                }
            }
        }
    }
}

