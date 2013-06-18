namespace System.ComponentModel.Design.Serialization
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Globalization;
    using System.Resources;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    public sealed class CodeDomLocalizationProvider : IDisposable, IDesignerSerializationProvider
    {
        private LanguageExtenders _extender;
        private Hashtable _memberSerializers;
        private CodeDomLocalizationModel _model;
        private Hashtable _nopMemberSerializers;
        private IExtenderProviderService _providerService;
        private CultureInfo[] _supportedCultures;

        public CodeDomLocalizationProvider(IServiceProvider provider, CodeDomLocalizationModel model)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            this._model = model;
            this.Initialize(provider);
        }

        public CodeDomLocalizationProvider(IServiceProvider provider, CodeDomLocalizationModel model, CultureInfo[] supportedCultures)
        {
            if (provider == null)
            {
                throw new ArgumentNullException("provider");
            }
            if (supportedCultures == null)
            {
                throw new ArgumentNullException("supportedCultures");
            }
            this._model = model;
            this._supportedCultures = (CultureInfo[]) supportedCultures.Clone();
            this.Initialize(provider);
        }

        public void Dispose()
        {
            if ((this._providerService != null) && (this._extender != null))
            {
                this._providerService.RemoveExtenderProvider(this._extender);
                this._providerService = null;
                this._extender = null;
            }
        }

        private object GetCodeDomSerializer(IDesignerSerializationManager manager, object currentSerializer, System.Type objectType, System.Type serializerType)
        {
            if (currentSerializer != null)
            {
                if (typeof(ResourceManager).IsAssignableFrom(objectType))
                {
                    return null;
                }
                CodeDomLocalizationModel none = CodeDomLocalizationModel.None;
                object obj2 = manager.Context[typeof(CodeDomLocalizationModel)];
                if (obj2 != null)
                {
                    none = (CodeDomLocalizationModel) obj2;
                }
                if (none != CodeDomLocalizationModel.None)
                {
                    return new LocalizationCodeDomSerializer(none, currentSerializer);
                }
            }
            return null;
        }

        private object GetMemberCodeDomSerializer(IDesignerSerializationManager manager, object currentSerializer, System.Type objectType, System.Type serializerType)
        {
            CodeDomLocalizationModel none = this._model;
            if (!typeof(PropertyDescriptor).IsAssignableFrom(objectType))
            {
                return null;
            }
            if (currentSerializer == null)
            {
                return null;
            }
            if (currentSerializer is ResourcePropertyMemberCodeDomSerializer)
            {
                return null;
            }
            if ((this._extender == null) || !this._extender.GetLocalizable(null))
            {
                return null;
            }
            PropertyDescriptor descriptor = manager.Context[typeof(PropertyDescriptor)] as PropertyDescriptor;
            if ((descriptor == null) || !descriptor.IsLocalizable)
            {
                none = CodeDomLocalizationModel.None;
            }
            if (this._memberSerializers == null)
            {
                this._memberSerializers = new Hashtable();
            }
            if (this._nopMemberSerializers == null)
            {
                this._nopMemberSerializers = new Hashtable();
            }
            object obj2 = null;
            if (none == CodeDomLocalizationModel.None)
            {
                obj2 = this._nopMemberSerializers[currentSerializer];
            }
            else
            {
                obj2 = this._memberSerializers[currentSerializer];
            }
            if (obj2 == null)
            {
                obj2 = new ResourcePropertyMemberCodeDomSerializer((MemberCodeDomSerializer) currentSerializer, this._extender, none);
                if (none == CodeDomLocalizationModel.None)
                {
                    this._nopMemberSerializers[currentSerializer] = obj2;
                    return obj2;
                }
                this._memberSerializers[currentSerializer] = obj2;
            }
            return obj2;
        }

        private void Initialize(IServiceProvider provider)
        {
            this._providerService = provider.GetService(typeof(IExtenderProviderService)) as IExtenderProviderService;
            if (this._providerService == null)
            {
                throw new NotSupportedException(System.Design.SR.GetString("LocalizationProviderMissingService", new object[] { typeof(IExtenderProviderService).Name }));
            }
            this._extender = new LanguageExtenders(provider, this._supportedCultures);
            this._providerService.AddExtenderProvider(this._extender);
        }

        object IDesignerSerializationProvider.GetSerializer(IDesignerSerializationManager manager, object currentSerializer, System.Type objectType, System.Type serializerType)
        {
            if (serializerType == typeof(CodeDomSerializer))
            {
                return this.GetCodeDomSerializer(manager, currentSerializer, objectType, serializerType);
            }
            if (serializerType == typeof(MemberCodeDomSerializer))
            {
                return this.GetMemberCodeDomSerializer(manager, currentSerializer, objectType, serializerType);
            }
            return null;
        }

        internal sealed class LanguageCultureInfoConverter : CultureInfoConverter
        {
            protected override string GetCultureName(CultureInfo culture)
            {
                return culture.DisplayName;
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                TypeConverter.StandardValuesCollection supportedCultures = null;
                if (context.PropertyDescriptor != null)
                {
                    ExtenderProvidedPropertyAttribute attribute = context.PropertyDescriptor.Attributes[typeof(ExtenderProvidedPropertyAttribute)] as ExtenderProvidedPropertyAttribute;
                    if (attribute != null)
                    {
                        CodeDomLocalizationProvider.LanguageExtenders provider = attribute.Provider as CodeDomLocalizationProvider.LanguageExtenders;
                        if (provider != null)
                        {
                            supportedCultures = provider.SupportedCultures;
                        }
                    }
                }
                if (supportedCultures == null)
                {
                    supportedCultures = base.GetStandardValues(context);
                }
                return supportedCultures;
            }
        }

        [ProvideProperty("Localizable", typeof(IComponent)), ProvideProperty("Language", typeof(IComponent)), ProvideProperty("LoadLanguage", typeof(IComponent))]
        internal class LanguageExtenders : IExtenderProvider
        {
            private CultureInfo _defaultLanguage;
            private IDesignerHost _host;
            private CultureInfo _language;
            private IComponent _lastRoot;
            private CultureInfo _loadLanguage;
            private bool _localizable;
            private IServiceProvider _serviceProvider;
            private TypeConverter.StandardValuesCollection _supportedCultures;

            public LanguageExtenders(IServiceProvider serviceProvider, CultureInfo[] supportedCultures)
            {
                this._serviceProvider = serviceProvider;
                this._host = serviceProvider.GetService(typeof(IDesignerHost)) as IDesignerHost;
                this._language = CultureInfo.InvariantCulture;
                if (supportedCultures != null)
                {
                    this._supportedCultures = new TypeConverter.StandardValuesCollection(supportedCultures);
                }
            }

            private void BroadcastGlobalChange(IComponent comp)
            {
                ISite site = comp.Site;
                if (site != null)
                {
                    IComponentChangeService service = site.GetService(typeof(IComponentChangeService)) as IComponentChangeService;
                    IContainer container = site.GetService(typeof(IContainer)) as IContainer;
                    if ((service != null) && (container != null))
                    {
                        foreach (IComponent component in container.Components)
                        {
                            service.OnComponentChanging(component, null);
                            service.OnComponentChanged(component, null, null, null);
                        }
                    }
                }
            }

            public bool CanExtend(object o)
            {
                this.CheckRoot();
                return ((this._host != null) && (o == this._host.RootComponent));
            }

            private void CheckRoot()
            {
                if ((this._host != null) && (this._host.RootComponent != this._lastRoot))
                {
                    this._lastRoot = this._host.RootComponent;
                    this._language = CultureInfo.InvariantCulture;
                    this._loadLanguage = null;
                    this._localizable = false;
                }
            }

            [DesignOnly(true), System.Design.SRDescription("LocalizationProviderLanguageDescr"), TypeConverter(typeof(CodeDomLocalizationProvider.LanguageCultureInfoConverter)), Category("Design")]
            public CultureInfo GetLanguage(IComponent o)
            {
                this.CheckRoot();
                return this._language;
            }

            [DesignOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
            public CultureInfo GetLoadLanguage(IComponent o)
            {
                this.CheckRoot();
                if (this._loadLanguage == null)
                {
                    this._loadLanguage = CultureInfo.InvariantCulture;
                }
                return this._loadLanguage;
            }

            [System.Design.SRDescription("LocalizationProviderLocalizableDescr"), DesignOnly(true), Category("Design")]
            public bool GetLocalizable(IComponent o)
            {
                this.CheckRoot();
                return this._localizable;
            }

            private void ResetLanguage(IComponent o)
            {
                this.SetLanguage(o, CultureInfo.InvariantCulture);
            }

            private void ResetLocalizable(IComponent o)
            {
                this.SetLocalizable(o, false);
            }

            public void SetLanguage(IComponent o, CultureInfo language)
            {
                this.CheckRoot();
                if (language == null)
                {
                    language = CultureInfo.InvariantCulture;
                }
                bool flag = language.Equals(CultureInfo.InvariantCulture);
                if (!this._language.Equals(language))
                {
                    this._language = language;
                    if (!flag)
                    {
                        this.SetLocalizable(o, true);
                    }
                    if ((this._serviceProvider != null) && (this._host != null))
                    {
                        IDesignerLoaderService service = this._serviceProvider.GetService(typeof(IDesignerLoaderService)) as IDesignerLoaderService;
                        if (this._host.Loading)
                        {
                            this._loadLanguage = language;
                        }
                        else
                        {
                            bool flag2 = false;
                            if (service != null)
                            {
                                flag2 = service.Reload();
                            }
                            if (!flag2)
                            {
                                IUIService service2 = (IUIService) this._serviceProvider.GetService(typeof(IUIService));
                                if (service2 != null)
                                {
                                    service2.ShowMessage(System.Design.SR.GetString("LocalizationProviderManualReload"));
                                }
                            }
                        }
                    }
                }
            }

            public void SetLocalizable(IComponent o, bool localizable)
            {
                this.CheckRoot();
                if (localizable != this._localizable)
                {
                    this._localizable = localizable;
                    if (!localizable)
                    {
                        this.SetLanguage(o, CultureInfo.InvariantCulture);
                    }
                    if ((this._host != null) && !this._host.Loading)
                    {
                        this.BroadcastGlobalChange(o);
                    }
                }
            }

            private bool ShouldSerializeLanguage(IComponent o)
            {
                return ((this._language != null) && (this._language != CultureInfo.InvariantCulture));
            }

            private bool ShouldSerializeLocalizable(IComponent o)
            {
                return this._localizable;
            }

            internal TypeConverter.StandardValuesCollection SupportedCultures
            {
                get
                {
                    return this._supportedCultures;
                }
            }

            private CultureInfo ThreadDefaultLanguage
            {
                get
                {
                    if (this._defaultLanguage == null)
                    {
                        this._defaultLanguage = Application.CurrentCulture;
                    }
                    return this._defaultLanguage;
                }
            }
        }
    }
}

