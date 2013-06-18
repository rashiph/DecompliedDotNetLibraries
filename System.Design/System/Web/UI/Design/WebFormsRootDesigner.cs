namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Resources;
    using System.Runtime.InteropServices;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.UI;

    public abstract class WebFormsRootDesigner : IRootDesigner, IDesigner, IDisposable, IDesignerFilter
    {
        private IComponent _component;
        private DesignerActionService _designerActionService;
        private DesignerActionUIService _designerActionUIService;
        private IImplicitResourceProvider _implicitResourceProvider;
        private IUrlResolutionService _urlResolutionService;
        private const char appRelativeCharacter = '~';
        private const string dummyProtocolAndServer = "file://foo";

        public event EventHandler LoadComplete;

        protected WebFormsRootDesigner()
        {
        }

        public abstract void AddClientScriptToDocument(ClientScriptItem scriptItem);
        public abstract string AddControlToDocument(Control newControl, Control referenceControl, ControlLocation location);
        protected virtual DesignerActionService CreateDesignerActionService(IServiceProvider serviceProvider)
        {
            return new WebFormsDesignerActionService(serviceProvider);
        }

        protected virtual IUrlResolutionService CreateUrlResolutionService()
        {
            return new UrlResolutionService(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                IPropertyValueUIService service = (IPropertyValueUIService) this.GetService(typeof(IPropertyValueUIService));
                if (service != null)
                {
                    service.RemovePropertyValueUIHandler(new PropertyValueUIHandler(this.OnGetUIValueItem));
                }
                IServiceContainer container = (IServiceContainer) this.GetService(typeof(IServiceContainer));
                if (container != null)
                {
                    if (this._urlResolutionService != null)
                    {
                        container.RemoveService(typeof(IUrlResolutionService));
                    }
                    container.RemoveService(typeof(IImplicitResourceProvider));
                    if (this._designerActionService != null)
                    {
                        this._designerActionService.Dispose();
                    }
                    this._designerActionUIService.Dispose();
                }
                this._urlResolutionService = null;
                this._component = null;
            }
        }

        ~WebFormsRootDesigner()
        {
            this.Dispose(false);
        }

        public virtual string GenerateEmptyDesignTimeHtml(Control control)
        {
            return this.GenerateErrorDesignTimeHtml(control, null, string.Empty);
        }

        public virtual string GenerateErrorDesignTimeHtml(Control control, Exception e, string errorMessage)
        {
            string name = control.Site.Name;
            if (errorMessage == null)
            {
                errorMessage = string.Empty;
            }
            else
            {
                errorMessage = HttpUtility.HtmlEncode(errorMessage);
            }
            if (e != null)
            {
                errorMessage = errorMessage + "<br />" + HttpUtility.HtmlEncode(e.Message);
            }
            return string.Format(CultureInfo.InvariantCulture, ControlDesigner.ErrorDesignTimeHtmlTemplate, new object[] { System.Design.SR.GetString("ControlDesigner_DesignTimeHtmlError"), HttpUtility.HtmlEncode(name), errorMessage });
        }

        public abstract ClientScriptItemCollection GetClientScriptsInDocument();
        protected internal abstract void GetControlViewAndTag(Control control, out IControlDesignerView view, out IControlDesignerTag tag);
        protected internal virtual object GetService(Type serviceType)
        {
            if (this._component != null)
            {
                ISite site = this._component.Site;
                if (site != null)
                {
                    return site.GetService(serviceType);
                }
            }
            return null;
        }

        protected object GetView(ViewTechnology viewTechnology)
        {
            return null;
        }

        public virtual void Initialize(IComponent component)
        {
            ControlDesigner.VerifyInitializeArgument(component, typeof(TemplateControl));
            this._component = component;
            IServiceContainer container = (IServiceContainer) this.GetService(typeof(IServiceContainer));
            if (container != null)
            {
                this._urlResolutionService = this.CreateUrlResolutionService();
                if (this._urlResolutionService != null)
                {
                    container.AddService(typeof(IUrlResolutionService), this._urlResolutionService);
                }
                this._designerActionService = this.CreateDesignerActionService(this._component.Site);
                this._designerActionUIService = new DesignerActionUIService(this._component.Site);
                ServiceCreatorCallback callback = new ServiceCreatorCallback(this.OnCreateService);
                container.AddService(typeof(IImplicitResourceProvider), callback);
            }
            IPropertyValueUIService service = (IPropertyValueUIService) this.GetService(typeof(IPropertyValueUIService));
            if (service != null)
            {
                service.AddPropertyValueUIHandler(new PropertyValueUIHandler(this.OnGetUIValueItem));
            }
        }

        private static bool IsAppRelativePath(string path)
        {
            if ((path.Length < 2) || (path[0] != '~'))
            {
                return false;
            }
            if (path[1] != '/')
            {
                return (path[1] == '\\');
            }
            return true;
        }

        private static bool IsRooted(string basepath)
        {
            if (((basepath != null) && (basepath.Length != 0)) && (basepath[0] != '/'))
            {
                return (basepath[0] == '\\');
            }
            return true;
        }

        private object OnCreateService(IServiceContainer container, Type serviceType)
        {
            if (!(serviceType == typeof(IImplicitResourceProvider)))
            {
                return null;
            }
            if (this._implicitResourceProvider == null)
            {
                IResourceProvider provider = ControlDesigner.GetDesignTimeResourceProviderFactory(this.Component.Site).CreateDesignTimeLocalResourceProvider(this.Component.Site);
                this._implicitResourceProvider = provider as IImplicitResourceProvider;
                if (this._implicitResourceProvider == null)
                {
                    this._implicitResourceProvider = new ImplicitResourceProvider(this);
                }
            }
            return this._implicitResourceProvider;
        }

        private void OnGetUIValueItem(ITypeDescriptorContext context, PropertyDescriptor propDesc, ArrayList valueUIItemList)
        {
            Control instance = context.Instance as Control;
            if (instance != null)
            {
                IDataBindingsAccessor accessor = instance;
                if (accessor.HasDataBindings && (accessor.DataBindings[propDesc.Name] != null))
                {
                    valueUIItemList.Add(new DataBindingUIItem());
                }
                IExpressionsAccessor accessor2 = instance;
                if (accessor2.HasExpressions)
                {
                    ExpressionBinding binding2 = accessor2.Expressions[propDesc.Name];
                    if (binding2 != null)
                    {
                        if (binding2.Generated)
                        {
                            valueUIItemList.Add(new ImplicitExpressionUIItem());
                        }
                        else
                        {
                            valueUIItemList.Add(new ExpressionBindingUIItem());
                        }
                    }
                }
            }
        }

        protected virtual void OnLoadComplete(EventArgs e)
        {
            if (this._loadCompleteHandler != null)
            {
                this._loadCompleteHandler(this, e);
            }
        }

        protected virtual void PostFilterAttributes(IDictionary attributes)
        {
        }

        protected virtual void PostFilterEvents(IDictionary events)
        {
        }

        protected virtual void PostFilterProperties(IDictionary properties)
        {
        }

        protected virtual void PreFilterAttributes(IDictionary attributes)
        {
        }

        protected virtual void PreFilterEvents(IDictionary events)
        {
        }

        protected virtual void PreFilterProperties(IDictionary properties)
        {
        }

        public abstract void RemoveClientScriptFromDocument(string clientScriptId);
        public abstract void RemoveControlFromDocument(Control control);
        public string ResolveUrl(string relativeUrl)
        {
            if (relativeUrl == null)
            {
                throw new ArgumentNullException("relativeUrl");
            }
            string documentUrl = this.DocumentUrl;
            if (((documentUrl == null) || (documentUrl.Length == 0)) || ((IsAppRelativePath(relativeUrl) || IsRooted(relativeUrl)) || !IsAppRelativePath(documentUrl)))
            {
                return relativeUrl;
            }
            Uri baseUri = new Uri(documentUrl.Replace("~", "file://foo"), true);
            Uri uri2 = new Uri(baseUri, relativeUrl);
            return uri2.ToString().Replace("file://foo", "~");
        }

        public virtual void SetControlID(Control control, string id)
        {
            control.Site.Name = id;
            control.ID = id.Trim();
        }

        void IDesigner.DoDefaultAction()
        {
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

        object IRootDesigner.GetView(ViewTechnology viewTechnology)
        {
            return this.GetView(viewTechnology);
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        public virtual IComponent Component
        {
            get
            {
                return this._component;
            }
            set
            {
                this._component = value;
            }
        }

        public CultureInfo CurrentCulture
        {
            get
            {
                return CultureInfo.CurrentCulture;
            }
        }

        public abstract string DocumentUrl { get; }

        public abstract bool IsDesignerViewLocked { get; }

        public abstract bool IsLoading { get; }

        public abstract WebFormsReferenceManager ReferenceManager { get; }

        protected ViewTechnology[] SupportedTechnologies
        {
            get
            {
                return new ViewTechnology[] { ViewTechnology.Default };
            }
        }

        DesignerVerbCollection IDesigner.Verbs
        {
            get
            {
                return this.Verbs;
            }
        }

        ViewTechnology[] IRootDesigner.SupportedTechnologies
        {
            get
            {
                return this.SupportedTechnologies;
            }
        }

        protected DesignerVerbCollection Verbs
        {
            get
            {
                return new DesignerVerbCollection();
            }
        }

        private sealed class DataBindingUIItem : PropertyValueUIItem
        {
            private static Bitmap _dataBindingBitmap;
            private static string _dataBindingToolTip;

            public DataBindingUIItem() : base(DataBindingBitmap, new PropertyValueUIItemInvokeHandler(WebFormsRootDesigner.DataBindingUIItem.OnValueUIItemInvoke), DataBindingToolTip)
            {
            }

            private static void OnValueUIItemInvoke(ITypeDescriptorContext context, PropertyDescriptor propDesc, PropertyValueUIItem invokedItem)
            {
            }

            private static Bitmap DataBindingBitmap
            {
                get
                {
                    if (_dataBindingBitmap == null)
                    {
                        _dataBindingBitmap = new Bitmap(typeof(WebFormsRootDesigner), "DataBindingGlyph.bmp");
                        _dataBindingBitmap.MakeTransparent(Color.Fuchsia);
                    }
                    return _dataBindingBitmap;
                }
            }

            private static string DataBindingToolTip
            {
                get
                {
                    if (_dataBindingToolTip == null)
                    {
                        _dataBindingToolTip = System.Design.SR.GetString("DataBindingGlyph_ToolTip");
                    }
                    return _dataBindingToolTip;
                }
            }
        }

        private sealed class ExpressionBindingUIItem : PropertyValueUIItem
        {
            private static Bitmap _expressionBindingBitmap;
            private static string _expressionBindingToolTip;

            public ExpressionBindingUIItem() : base(ExpressionBindingBitmap, new PropertyValueUIItemInvokeHandler(WebFormsRootDesigner.ExpressionBindingUIItem.OnValueUIItemInvoke), ExpressionBindingToolTip)
            {
            }

            private static void OnValueUIItemInvoke(ITypeDescriptorContext context, PropertyDescriptor propDesc, PropertyValueUIItem invokedItem)
            {
            }

            private static Bitmap ExpressionBindingBitmap
            {
                get
                {
                    if (_expressionBindingBitmap == null)
                    {
                        _expressionBindingBitmap = new Bitmap(typeof(WebFormsRootDesigner), "ExpressionBindingGlyph.bmp");
                        _expressionBindingBitmap.MakeTransparent(Color.Fuchsia);
                    }
                    return _expressionBindingBitmap;
                }
            }

            private static string ExpressionBindingToolTip
            {
                get
                {
                    if (_expressionBindingToolTip == null)
                    {
                        _expressionBindingToolTip = System.Design.SR.GetString("ExpressionBindingGlyph_ToolTip");
                    }
                    return _expressionBindingToolTip;
                }
            }
        }

        private sealed class ImplicitExpressionUIItem : PropertyValueUIItem
        {
            private static Bitmap _expressionBindingBitmap;
            private static string _expressionBindingToolTip;

            public ImplicitExpressionUIItem() : base(ImplicitExpressionBindingBitmap, new PropertyValueUIItemInvokeHandler(WebFormsRootDesigner.ImplicitExpressionUIItem.OnValueUIItemInvoke), ImplicitExpressionBindingToolTip)
            {
            }

            private static void OnValueUIItemInvoke(ITypeDescriptorContext context, PropertyDescriptor propDesc, PropertyValueUIItem invokedItem)
            {
            }

            private static Bitmap ImplicitExpressionBindingBitmap
            {
                get
                {
                    if (_expressionBindingBitmap == null)
                    {
                        _expressionBindingBitmap = new Bitmap(typeof(WebFormsRootDesigner), "ImplicitExpressionBindingGlyph.bmp");
                        _expressionBindingBitmap.MakeTransparent(Color.Fuchsia);
                    }
                    return _expressionBindingBitmap;
                }
            }

            private static string ImplicitExpressionBindingToolTip
            {
                get
                {
                    if (_expressionBindingToolTip == null)
                    {
                        _expressionBindingToolTip = System.Design.SR.GetString("ImplicitExpressionBindingGlyph_ToolTip");
                    }
                    return _expressionBindingToolTip;
                }
            }
        }

        private sealed class ImplicitResourceProvider : IImplicitResourceProvider
        {
            private WebFormsRootDesigner _owner;

            public ImplicitResourceProvider(WebFormsRootDesigner owner)
            {
                this._owner = owner;
            }

            private IDictionary GetPageResources()
            {
                if (this._owner.Component == null)
                {
                    return null;
                }
                IServiceProvider site = this._owner.Component.Site;
                if (site == null)
                {
                    return null;
                }
                DesignTimeResourceProviderFactory designTimeResourceProviderFactory = ControlDesigner.GetDesignTimeResourceProviderFactory(site);
                if (designTimeResourceProviderFactory == null)
                {
                    return null;
                }
                IResourceProvider provider2 = designTimeResourceProviderFactory.CreateDesignTimeLocalResourceProvider(site);
                if (provider2 == null)
                {
                    return null;
                }
                IResourceReader resourceReader = provider2.ResourceReader;
                if (resourceReader == null)
                {
                    return null;
                }
                IDictionary dictionary = new HybridDictionary(true);
                if (resourceReader != null)
                {
                    foreach (DictionaryEntry entry in resourceReader)
                    {
                        string str = (string) entry.Key;
                        string str2 = string.Empty;
                        if (str.IndexOf(':') > 0)
                        {
                            string[] strArray = str.Split(new char[] { ':' });
                            if (strArray.Length > 2)
                            {
                                continue;
                            }
                            str2 = strArray[0];
                            str = strArray[1];
                        }
                        int index = str.IndexOf('.');
                        if (index > 0)
                        {
                            string str3 = str.Substring(0, index);
                            string str4 = str.Substring(index + 1);
                            ArrayList list = (ArrayList) dictionary[str3];
                            if (list == null)
                            {
                                list = new ArrayList();
                                dictionary[str3] = list;
                            }
                            ImplicitResourceKey key = new ImplicitResourceKey {
                                Filter = str2,
                                Property = str4,
                                KeyPrefix = str3
                            };
                            list.Add(key);
                        }
                    }
                }
                return dictionary;
            }

            ICollection IImplicitResourceProvider.GetImplicitResourceKeys(string keyPrefix)
            {
                return (this.GetPageResources()[keyPrefix] as ICollection);
            }

            object IImplicitResourceProvider.GetObject(ImplicitResourceKey key, CultureInfo culture)
            {
                throw new NotSupportedException();
            }
        }

        private sealed class UrlResolutionService : IUrlResolutionService
        {
            private WebFormsRootDesigner _owner;

            public UrlResolutionService(WebFormsRootDesigner owner)
            {
                this._owner = owner;
            }

            string IUrlResolutionService.ResolveClientUrl(string relativeUrl)
            {
                if (relativeUrl == null)
                {
                    throw new ArgumentNullException("relativeUrl");
                }
                if (!WebFormsRootDesigner.IsAppRelativePath(relativeUrl))
                {
                    return relativeUrl;
                }
                string documentUrl = this._owner.DocumentUrl;
                if (((documentUrl == null) || (documentUrl.Length == 0)) || !WebFormsRootDesigner.IsAppRelativePath(documentUrl))
                {
                    return relativeUrl.Substring(2);
                }
                Uri uri = new Uri(documentUrl.Replace("~", "file://foo"), true);
                relativeUrl = relativeUrl.Replace("~", "file://foo");
                Uri uri2 = new Uri(relativeUrl, true);
                return uri.MakeRelativeUri(uri2).ToString().Replace("file://foo", string.Empty);
            }
        }
    }
}

