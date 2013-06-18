namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Design;
    using System.Globalization;
    using System.Resources;
    using System.Threading;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    [ProvideProperty("LoadLanguage", typeof(object)), ProvideProperty("Localizable", typeof(object)), Obsolete("This class has been deprecated. Use CodeDomLocalizationProvider instead.  http://go.microsoft.com/fwlink/?linkid=14202"), ProvideProperty("Language", typeof(object))]
    public class LocalizationExtenderProvider : IExtenderProvider, IDisposable
    {
        private IComponent baseComponent;
        private CultureInfo defaultLanguage;
        private bool defaultLocalizable;
        private const string KeyThreadDefaultLanguage = "_Thread_Default_Language";
        private CultureInfo language;
        private CultureInfo loadLanguage;
        private bool localizable;
        private static object localizationLock = new object();
        private IServiceProvider serviceProvider;

        public LocalizationExtenderProvider(ISite serviceProvider, IComponent baseComponent)
        {
            this.serviceProvider = serviceProvider;
            this.baseComponent = baseComponent;
            if (serviceProvider != null)
            {
                IExtenderProviderService service = (IExtenderProviderService) serviceProvider.GetService(typeof(IExtenderProviderService));
                if (service != null)
                {
                    service.AddExtenderProvider(this);
                }
            }
            this.language = CultureInfo.InvariantCulture;
            ResourceManager manager = new ResourceManager(baseComponent.GetType());
            if (manager != null)
            {
                ResourceSet set = manager.GetResourceSet(this.language, true, false);
                if (set != null)
                {
                    object obj2 = set.GetObject("$this.Localizable");
                    if (obj2 is bool)
                    {
                        this.defaultLocalizable = (bool) obj2;
                        this.localizable = this.defaultLocalizable;
                    }
                }
            }
        }

        public bool CanExtend(object o)
        {
            return o.Equals(this.baseComponent);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && (this.serviceProvider != null))
            {
                IExtenderProviderService service = (IExtenderProviderService) this.serviceProvider.GetService(typeof(IExtenderProviderService));
                if (service != null)
                {
                    service.RemoveExtenderProvider(this);
                }
            }
        }

        [DesignOnly(true), Localizable(true), System.Design.SRDescription("ParentControlDesignerLanguageDescr")]
        public CultureInfo GetLanguage(object o)
        {
            return this.language;
        }

        [DesignOnly(true), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public CultureInfo GetLoadLanguage(object o)
        {
            if (this.loadLanguage == null)
            {
                this.loadLanguage = CultureInfo.InvariantCulture;
            }
            return this.loadLanguage;
        }

        [DesignOnly(true), Localizable(true), System.Design.SRDescription("ParentControlDesignerLocalizableDescr")]
        public bool GetLocalizable(object o)
        {
            return this.localizable;
        }

        public void ResetLanguage(object o)
        {
            this.SetLanguage(null, CultureInfo.InvariantCulture);
        }

        private void ResetLocalizable(object o)
        {
            this.SetLocalizable(null, this.defaultLocalizable);
        }

        public void SetLanguage(object o, CultureInfo language)
        {
            if (language == null)
            {
                language = CultureInfo.InvariantCulture;
            }
            if (!this.language.Equals(language))
            {
                bool flag = language.Equals(CultureInfo.InvariantCulture);
                CultureInfo threadDefaultLanguage = this.ThreadDefaultLanguage;
                this.language = language;
                if (!flag)
                {
                    this.SetLocalizable(null, true);
                }
                if (this.serviceProvider != null)
                {
                    IDesignerLoaderService service = (IDesignerLoaderService) this.serviceProvider.GetService(typeof(IDesignerLoaderService));
                    IDesignerHost host = (IDesignerHost) this.serviceProvider.GetService(typeof(IDesignerHost));
                    if (host != null)
                    {
                        if (host.Loading)
                        {
                            this.loadLanguage = language;
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
                                IUIService service2 = (IUIService) this.serviceProvider.GetService(typeof(IUIService));
                                if (service2 != null)
                                {
                                    service2.ShowMessage(System.Design.SR.GetString("LocalizerManualReload"));
                                }
                            }
                        }
                    }
                }
            }
        }

        public void SetLocalizable(object o, bool localizable)
        {
            this.localizable = localizable;
            if (!localizable)
            {
                this.SetLanguage(null, CultureInfo.InvariantCulture);
            }
        }

        public bool ShouldSerializeLanguage(object o)
        {
            return ((this.language != null) && (this.language != CultureInfo.InvariantCulture));
        }

        private bool ShouldSerializeLocalizable(object o)
        {
            return (this.localizable != this.defaultLocalizable);
        }

        private CultureInfo ThreadDefaultLanguage
        {
            get
            {
                lock (localizationLock)
                {
                    if (this.defaultLanguage != null)
                    {
                        return this.defaultLanguage;
                    }
                    LocalDataStoreSlot namedDataSlot = Thread.GetNamedDataSlot("_Thread_Default_Language");
                    if (namedDataSlot == null)
                    {
                        return null;
                    }
                    this.defaultLanguage = (CultureInfo) Thread.GetData(namedDataSlot);
                    if (this.defaultLanguage == null)
                    {
                        this.defaultLanguage = Application.CurrentCulture;
                        Thread.SetData(namedDataSlot, this.defaultLanguage);
                    }
                }
                return this.defaultLanguage;
            }
        }
    }
}

