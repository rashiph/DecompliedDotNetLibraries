namespace System.Configuration
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Deployment.Internal;
    using System.Reflection;
    using System.Security.Permissions;

    public abstract class ApplicationSettingsBase : SettingsBase, INotifyPropertyChanged
    {
        private object[] _classAttributes;
        private SettingsContext _context;
        private bool _explicitSerializeOnClass;
        private bool _firstLoad;
        private SettingsProperty _init;
        private bool _initialized;
        private IComponent _owner;
        private SettingsProviderCollection _providers;
        private SettingsPropertyCollection _settings;
        private string _settingsKey;

        public event PropertyChangedEventHandler PropertyChanged;

        public event SettingChangingEventHandler SettingChanging;

        public event SettingsLoadedEventHandler SettingsLoaded;

        public event SettingsSavingEventHandler SettingsSaving;

        protected ApplicationSettingsBase()
        {
            this._settingsKey = string.Empty;
            this._firstLoad = true;
        }

        protected ApplicationSettingsBase(IComponent owner) : this(owner, string.Empty)
        {
        }

        protected ApplicationSettingsBase(string settingsKey)
        {
            this._settingsKey = string.Empty;
            this._firstLoad = true;
            this._settingsKey = settingsKey;
        }

        protected ApplicationSettingsBase(IComponent owner, string settingsKey) : this(settingsKey)
        {
            if (owner == null)
            {
                throw new ArgumentNullException("owner");
            }
            this._owner = owner;
            if (owner.Site != null)
            {
                ISettingsProviderService service = owner.Site.GetService(typeof(ISettingsProviderService)) as ISettingsProviderService;
                if (service != null)
                {
                    foreach (SettingsProperty property in this.Properties)
                    {
                        SettingsProvider settingsProvider = service.GetSettingsProvider(property);
                        if (settingsProvider != null)
                        {
                            property.Provider = settingsProvider;
                        }
                    }
                    this.ResetProviders();
                }
            }
        }

        private SettingsProperty CreateSetting(PropertyInfo propInfo)
        {
            object[] customAttributes = propInfo.GetCustomAttributes(false);
            SettingsProperty property = new SettingsProperty(this.Initializer);
            bool flag = this._explicitSerializeOnClass;
            property.Name = propInfo.Name;
            property.PropertyType = propInfo.PropertyType;
            for (int i = 0; i < customAttributes.Length; i++)
            {
                Attribute attribute = customAttributes[i] as Attribute;
                if (attribute != null)
                {
                    if (attribute is DefaultSettingValueAttribute)
                    {
                        property.DefaultValue = ((DefaultSettingValueAttribute) attribute).Value;
                    }
                    else if (attribute is ReadOnlyAttribute)
                    {
                        property.IsReadOnly = true;
                    }
                    else if (attribute is SettingsProviderAttribute)
                    {
                        string providerTypeName = ((SettingsProviderAttribute) attribute).ProviderTypeName;
                        Type type = Type.GetType(providerTypeName);
                        if (type == null)
                        {
                            throw new ConfigurationErrorsException(System.SR.GetString("ProviderTypeLoadFailed", new object[] { providerTypeName }));
                        }
                        SettingsProvider provider = SecurityUtils.SecureCreateInstance(type) as SettingsProvider;
                        if (provider == null)
                        {
                            throw new ConfigurationErrorsException(System.SR.GetString("ProviderInstantiationFailed", new object[] { providerTypeName }));
                        }
                        provider.Initialize(null, null);
                        provider.ApplicationName = ConfigurationManagerInternalFactory.Instance.ExeProductName;
                        SettingsProvider provider2 = this._providers[provider.Name];
                        if (provider2 != null)
                        {
                            provider = provider2;
                        }
                        property.Provider = provider;
                    }
                    else if (attribute is SettingsSerializeAsAttribute)
                    {
                        property.SerializeAs = ((SettingsSerializeAsAttribute) attribute).SerializeAs;
                        flag = true;
                    }
                    else
                    {
                        property.Attributes.Add(attribute.GetType(), attribute);
                    }
                }
            }
            if (!flag)
            {
                property.SerializeAs = this.GetSerializeAs(propInfo.PropertyType);
            }
            return property;
        }

        private void EnsureInitialized()
        {
            if (!this._initialized)
            {
                this._initialized = true;
                Type type = base.GetType();
                if (this._context == null)
                {
                    this._context = new SettingsContext();
                }
                this._context["GroupName"] = type.FullName;
                this._context["SettingsKey"] = this.SettingsKey;
                this._context["SettingsClassType"] = type;
                PropertyInfo[] infoArray = this.SettingsFilter(type.GetProperties(BindingFlags.Public | BindingFlags.Instance));
                this._classAttributes = type.GetCustomAttributes(false);
                if (this._settings == null)
                {
                    this._settings = new SettingsPropertyCollection();
                }
                if (this._providers == null)
                {
                    this._providers = new SettingsProviderCollection();
                }
                for (int i = 0; i < infoArray.Length; i++)
                {
                    SettingsProperty property = this.CreateSetting(infoArray[i]);
                    if (property != null)
                    {
                        this._settings.Add(property);
                        if ((property.Provider != null) && (this._providers[property.Provider.Name] == null))
                        {
                            this._providers.Add(property.Provider);
                        }
                    }
                }
            }
        }

        public object GetPreviousVersion(string propertyName)
        {
            if (this.Properties.Count == 0)
            {
                throw new SettingsPropertyNotFoundException();
            }
            SettingsProperty property = this.Properties[propertyName];
            SettingsPropertyValue previousVersion = null;
            if (property == null)
            {
                throw new SettingsPropertyNotFoundException();
            }
            IApplicationSettingsProvider provider = property.Provider as IApplicationSettingsProvider;
            if (provider != null)
            {
                previousVersion = provider.GetPreviousVersion(this.Context, property);
            }
            if (previousVersion != null)
            {
                return previousVersion.PropertyValue;
            }
            return null;
        }

        private SettingsPropertyCollection GetPropertiesForProvider(SettingsProvider provider)
        {
            SettingsPropertyCollection propertys = new SettingsPropertyCollection();
            foreach (SettingsProperty property in this.Properties)
            {
                if (property.Provider == provider)
                {
                    propertys.Add(property);
                }
            }
            return propertys;
        }

        private object GetPropertyValue(string propertyName)
        {
            if (this.PropertyValues[propertyName] == null)
            {
                if (this._firstLoad)
                {
                    this._firstLoad = false;
                    if (this.IsFirstRunOfClickOnceApp())
                    {
                        this.Upgrade();
                    }
                }
                object obj1 = base[propertyName];
                SettingsProperty property = this.Properties[propertyName];
                SettingsProvider provider = (property != null) ? property.Provider : null;
                SettingsLoadedEventArgs e = new SettingsLoadedEventArgs(provider);
                this.OnSettingsLoaded(this, e);
            }
            return base[propertyName];
        }

        private SettingsSerializeAs GetSerializeAs(Type type)
        {
            TypeConverter converter = TypeDescriptor.GetConverter(type);
            bool flag = converter.CanConvertTo(typeof(string));
            bool flag2 = converter.CanConvertFrom(typeof(string));
            if (flag && flag2)
            {
                return SettingsSerializeAs.String;
            }
            return SettingsSerializeAs.Xml;
        }

        [SecurityPermission(SecurityAction.Assert, Flags=SecurityPermissionFlag.UnmanagedCode)]
        internal static bool IsClickOnceDeployed(AppDomain appDomain)
        {
            ActivationContext activationContext = appDomain.ActivationContext;
            return (((activationContext != null) && (activationContext.Form == ActivationContext.ContextForm.StoreBounded)) && !string.IsNullOrEmpty(activationContext.Identity.FullName));
        }

        private bool IsFirstRunOfClickOnceApp()
        {
            ActivationContext activationContext = AppDomain.CurrentDomain.ActivationContext;
            return (IsClickOnceDeployed(AppDomain.CurrentDomain) && InternalActivationContextHelper.IsFirstRun(activationContext));
        }

        protected virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (this._onPropertyChanged != null)
            {
                this._onPropertyChanged(this, e);
            }
        }

        protected virtual void OnSettingChanging(object sender, SettingChangingEventArgs e)
        {
            if (this._onSettingChanging != null)
            {
                this._onSettingChanging(this, e);
            }
        }

        protected virtual void OnSettingsLoaded(object sender, SettingsLoadedEventArgs e)
        {
            if (this._onSettingsLoaded != null)
            {
                this._onSettingsLoaded(this, e);
            }
        }

        protected virtual void OnSettingsSaving(object sender, CancelEventArgs e)
        {
            if (this._onSettingsSaving != null)
            {
                this._onSettingsSaving(this, e);
            }
        }

        public void Reload()
        {
            if (this.PropertyValues != null)
            {
                this.PropertyValues.Clear();
            }
            foreach (SettingsProperty property in this.Properties)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(property.Name);
                this.OnPropertyChanged(this, e);
            }
        }

        public void Reset()
        {
            if (this.Properties != null)
            {
                foreach (SettingsProvider provider in this.Providers)
                {
                    IApplicationSettingsProvider provider2 = provider as IApplicationSettingsProvider;
                    if (provider2 != null)
                    {
                        provider2.Reset(this.Context);
                    }
                }
            }
            this.Reload();
        }

        private void ResetProviders()
        {
            this.Providers.Clear();
            foreach (SettingsProperty property in this.Properties)
            {
                if (this.Providers[property.Provider.Name] == null)
                {
                    this.Providers.Add(property.Provider);
                }
            }
        }

        public override void Save()
        {
            CancelEventArgs e = new CancelEventArgs(false);
            this.OnSettingsSaving(this, e);
            if (!e.Cancel)
            {
                base.Save();
            }
        }

        private PropertyInfo[] SettingsFilter(PropertyInfo[] allProps)
        {
            ArrayList list = new ArrayList();
            for (int i = 0; i < allProps.Length; i++)
            {
                object[] customAttributes = allProps[i].GetCustomAttributes(false);
                for (int j = 0; j < customAttributes.Length; j++)
                {
                    Attribute attribute = customAttributes[j] as Attribute;
                    if (attribute is SettingAttribute)
                    {
                        list.Add(allProps[i]);
                        break;
                    }
                }
            }
            return (PropertyInfo[]) list.ToArray(typeof(PropertyInfo));
        }

        public virtual void Upgrade()
        {
            if (this.Properties != null)
            {
                foreach (SettingsProvider provider in this.Providers)
                {
                    IApplicationSettingsProvider provider2 = provider as IApplicationSettingsProvider;
                    if (provider2 != null)
                    {
                        provider2.Upgrade(this.Context, this.GetPropertiesForProvider(provider));
                    }
                }
            }
            this.Reload();
        }

        [Browsable(false)]
        public override SettingsContext Context
        {
            get
            {
                if (this._context == null)
                {
                    if (base.IsSynchronized)
                    {
                        lock (this)
                        {
                            if (this._context == null)
                            {
                                this._context = new SettingsContext();
                                this.EnsureInitialized();
                            }
                            goto Label_0052;
                        }
                    }
                    this._context = new SettingsContext();
                    this.EnsureInitialized();
                }
            Label_0052:
                return this._context;
            }
        }

        private SettingsProperty Initializer
        {
            get
            {
                if (this._init == null)
                {
                    this._init = new SettingsProperty("");
                    this._init.DefaultValue = null;
                    this._init.IsReadOnly = false;
                    this._init.PropertyType = null;
                    SettingsProvider provider = new LocalFileSettingsProvider();
                    if (this._classAttributes != null)
                    {
                        for (int i = 0; i < this._classAttributes.Length; i++)
                        {
                            Attribute attribute = this._classAttributes[i] as Attribute;
                            if (attribute != null)
                            {
                                if (attribute is ReadOnlyAttribute)
                                {
                                    this._init.IsReadOnly = true;
                                }
                                else if (attribute is SettingsGroupNameAttribute)
                                {
                                    if (this._context == null)
                                    {
                                        this._context = new SettingsContext();
                                    }
                                    this._context["GroupName"] = ((SettingsGroupNameAttribute) attribute).GroupName;
                                }
                                else if (attribute is SettingsProviderAttribute)
                                {
                                    string providerTypeName = ((SettingsProviderAttribute) attribute).ProviderTypeName;
                                    Type type = Type.GetType(providerTypeName);
                                    if (type == null)
                                    {
                                        throw new ConfigurationErrorsException(System.SR.GetString("ProviderTypeLoadFailed", new object[] { providerTypeName }));
                                    }
                                    SettingsProvider provider2 = SecurityUtils.SecureCreateInstance(type) as SettingsProvider;
                                    if (provider2 == null)
                                    {
                                        throw new ConfigurationErrorsException(System.SR.GetString("ProviderInstantiationFailed", new object[] { providerTypeName }));
                                    }
                                    provider = provider2;
                                }
                                else if (attribute is SettingsSerializeAsAttribute)
                                {
                                    this._init.SerializeAs = ((SettingsSerializeAsAttribute) attribute).SerializeAs;
                                    this._explicitSerializeOnClass = true;
                                }
                                else
                                {
                                    this._init.Attributes.Add(attribute.GetType(), attribute);
                                }
                            }
                        }
                    }
                    provider.Initialize(null, null);
                    provider.ApplicationName = ConfigurationManagerInternalFactory.Instance.ExeProductName;
                    this._init.Provider = provider;
                }
                return this._init;
            }
        }

        public override object this[string propertyName]
        {
            get
            {
                if (base.IsSynchronized)
                {
                    lock (this)
                    {
                        return this.GetPropertyValue(propertyName);
                    }
                }
                return this.GetPropertyValue(propertyName);
            }
            set
            {
                SettingChangingEventArgs e = new SettingChangingEventArgs(propertyName, base.GetType().FullName, this.SettingsKey, value, false);
                this.OnSettingChanging(this, e);
                if (!e.Cancel)
                {
                    base[propertyName] = value;
                    PropertyChangedEventArgs args2 = new PropertyChangedEventArgs(propertyName);
                    this.OnPropertyChanged(this, args2);
                }
            }
        }

        [Browsable(false)]
        public override SettingsPropertyCollection Properties
        {
            get
            {
                if (this._settings == null)
                {
                    if (base.IsSynchronized)
                    {
                        lock (this)
                        {
                            if (this._settings == null)
                            {
                                this._settings = new SettingsPropertyCollection();
                                this.EnsureInitialized();
                            }
                            goto Label_0052;
                        }
                    }
                    this._settings = new SettingsPropertyCollection();
                    this.EnsureInitialized();
                }
            Label_0052:
                return this._settings;
            }
        }

        [Browsable(false)]
        public override SettingsPropertyValueCollection PropertyValues
        {
            get
            {
                return base.PropertyValues;
            }
        }

        [Browsable(false)]
        public override SettingsProviderCollection Providers
        {
            get
            {
                if (this._providers == null)
                {
                    if (base.IsSynchronized)
                    {
                        lock (this)
                        {
                            if (this._providers == null)
                            {
                                this._providers = new SettingsProviderCollection();
                                this.EnsureInitialized();
                            }
                            goto Label_0052;
                        }
                    }
                    this._providers = new SettingsProviderCollection();
                    this.EnsureInitialized();
                }
            Label_0052:
                return this._providers;
            }
        }

        [Browsable(false)]
        public string SettingsKey
        {
            get
            {
                return this._settingsKey;
            }
            set
            {
                this._settingsKey = value;
                this.Context["SettingsKey"] = this._settingsKey;
            }
        }
    }
}

