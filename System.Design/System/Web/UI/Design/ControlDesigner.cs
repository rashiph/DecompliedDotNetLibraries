namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Configuration;
    using System.Data;
    using System.Design;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.UI.Design.Util;
    using System.Web.UI.WebControls;
    using System.Windows.Forms;
    using System.Xml;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class ControlDesigner : HtmlControlDesigner
    {
        private ControlDesignerState _designerState;
        private bool _expressionsChanged;
        private int _ignoreComponentChangesCount;
        private bool _inTemplateMode;
        private string _localizedInnerContent;
        private IControlDesignerTag _tag;
        private IControlDesignerView _view;
        private System.Web.UI.Control _viewControl;
        private bool _viewControlCreated;
        private readonly string[] DefaultEnabledPropertyInGrid = new string[] { "ID" };
        private static readonly Attribute[] emptyAttrs = new Attribute[0];
        internal static readonly string ErrorDesignTimeHtmlTemplate = "<table cellpadding=\"4\" cellspacing=\"0\" style=\"font: messagebox; color: buttontext; background-color: buttonface; border: solid 1px; border-top-color: buttonhighlight; border-left-color: buttonhighlight; border-bottom-color: buttonshadow; border-right-color: buttonshadow\">\r\n                <tr><td nowrap><span style=\"font-weight: bold; color: red\">{0}</span> - {1}</td></tr>\r\n                <tr><td>{2}</td></tr>\r\n              </table>";
        private bool fDirty;
        private bool isWebControl;
        private static readonly Attribute[] nonBrowsableAttrs = new Attribute[] { BrowsableAttribute.No };
        private static readonly string PlaceHolderDesignTimeHtmlTemplate = "<table cellpadding=4 cellspacing=0 style=\"font:messagebox;color:buttontext;background-color:buttonface;border: solid 1px;border-top-color:buttonhighlight;border-left-color:buttonhighlight;border-bottom-color:buttonshadow;border-right-color:buttonshadow\">\r\n              <tr><td nowrap><span style=\"font-weight:bold\">{0}</span> - {1}</td></tr>\r\n              <tr><td>{2}</td></tr>\r\n            </table>";
        private bool readOnly = true;

        internal static DesignerAutoFormatCollection CreateAutoFormats(string[] schemeNames, Func<string, DesignerAutoFormat> creationDelegate)
        {
            DesignerAutoFormatCollection formats = new DesignerAutoFormatCollection();
            foreach (string str in schemeNames)
            {
                formats.Add(creationDelegate(str));
            }
            return formats;
        }

        internal System.Web.UI.Control CreateClonedControl(IDesignerHost parseTimeDesignerHost, bool applyTheme)
        {
            string outerContent = null;
            if (this.Tag != null)
            {
                outerContent = this.Tag.GetOuterContent();
            }
            if (string.IsNullOrEmpty(outerContent))
            {
                outerContent = ControlPersister.PersistControl((System.Web.UI.Control) base.Component);
            }
            return ControlParser.ParseControl(parseTimeDesignerHost, outerContent, applyTheme);
        }

        protected string CreateErrorDesignTimeHtml(string errorMessage)
        {
            return this.CreateErrorDesignTimeHtml(errorMessage, null);
        }

        protected string CreateErrorDesignTimeHtml(string errorMessage, Exception e)
        {
            return CreateErrorDesignTimeHtml(errorMessage, e, base.Component);
        }

        internal static string CreateErrorDesignTimeHtml(string errorMessage, Exception e, IComponent component)
        {
            string name = component.Site.Name;
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
            return string.Format(CultureInfo.InvariantCulture, ErrorDesignTimeHtmlTemplate, new object[] { System.Design.SR.GetString("ControlDesigner_DesignTimeHtmlError"), HttpUtility.HtmlEncode(name), errorMessage });
        }

        internal string CreateInvalidParentDesignTimeHtml(System.Type controlType, System.Type requiredParentType)
        {
            return this.CreateErrorDesignTimeHtml(System.Design.SR.GetString("Control_CanOnlyBePlacedInside", new object[] { controlType.Name, requiredParentType.Name }));
        }

        protected string CreatePlaceHolderDesignTimeHtml()
        {
            return this.CreatePlaceHolderDesignTimeHtml(null);
        }

        protected string CreatePlaceHolderDesignTimeHtml(string instruction)
        {
            string name = base.Component.GetType().Name;
            string str2 = base.Component.Site.Name;
            if (instruction == null)
            {
                instruction = string.Empty;
            }
            return string.Format(CultureInfo.InvariantCulture, PlaceHolderDesignTimeHtmlTemplate, new object[] { name, str2, instruction });
        }

        protected virtual System.Web.UI.Control CreateViewControl()
        {
            return this.CreateClonedControl((IDesignerHost) this.GetService(typeof(IDesignerHost)), true);
        }

        private System.Web.UI.Control CreateViewControlInternal()
        {
            System.Web.UI.Control component = (System.Web.UI.Control) base.Component;
            System.Web.UI.Control target = this.CreateViewControl();
            target.RenderingCompatibility = component.RenderingCompatibility;
            ((IControlDesignerAccessor) target).SetOwnerControl(component);
            this.UpdateExpressionValues(target);
            return target;
        }

        private object EnsureParsedExpression(TemplateControl templateControl, ExpressionBinding eb, object parsedData)
        {
            if ((parsedData == null) && (templateControl != null))
            {
                string str;
                System.Type type = ExpressionEditor.GetExpressionBuilderType(eb.ExpressionPrefix, base.Component.Site, out str);
                if (type == null)
                {
                    return parsedData;
                }
                try
                {
                    System.Web.Compilation.ExpressionBuilder builder = (System.Web.Compilation.ExpressionBuilder) Activator.CreateInstance(type);
                    ExpressionBuilderContext context = new ExpressionBuilderContext(templateControl);
                    parsedData = builder.ParseExpression(eb.Expression, eb.PropertyType, context);
                }
                catch (Exception exception)
                {
                    IComponentDesignerDebugService service = (IComponentDesignerDebugService) this.GetService(typeof(IComponentDesignerDebugService));
                    if (service != null)
                    {
                        service.Fail(System.Design.SR.GetString("ControlDesigner_CouldNotGetExpressionBuilder", new object[] { eb.ExpressionPrefix, exception.Message }));
                    }
                }
            }
            return parsedData;
        }

        public Rectangle GetBounds()
        {
            if (this.View != null)
            {
                return this.View.GetBounds(null);
            }
            return Rectangle.Empty;
        }

        internal static PropertyDescriptor GetComplexProperty(object target, string propName, out object realTarget)
        {
            realTarget = null;
            string[] strArray = propName.Split(new char[] { '.' });
            PropertyDescriptor descriptor = null;
            foreach (string str in strArray)
            {
                if (string.IsNullOrEmpty(str))
                {
                    return null;
                }
                descriptor = TypeDescriptor.GetProperties(target)[str];
                if (descriptor == null)
                {
                    return null;
                }
                realTarget = target;
                target = descriptor.GetValue(target);
            }
            return descriptor;
        }

        public virtual string GetDesignTimeHtml()
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            DesignTimeHtmlTextWriter writer2 = new DesignTimeHtmlTextWriter(writer);
            string errorDesignTimeHtml = null;
            bool flag = false;
            bool visible = true;
            System.Web.UI.Control viewControl = null;
            try
            {
                viewControl = this.ViewControl;
                visible = viewControl.Visible;
                if (!visible)
                {
                    viewControl.Visible = true;
                    flag = !this.UsePreviewControl;
                }
                viewControl.RenderControl(writer2);
                errorDesignTimeHtml = writer.ToString();
            }
            catch (Exception exception)
            {
                errorDesignTimeHtml = this.GetErrorDesignTimeHtml(exception);
            }
            finally
            {
                if (flag)
                {
                    viewControl.Visible = visible;
                }
            }
            if ((errorDesignTimeHtml != null) && (errorDesignTimeHtml.Length != 0))
            {
                return errorDesignTimeHtml;
            }
            return this.GetEmptyDesignTimeHtml();
        }

        public virtual string GetDesignTimeHtml(DesignerRegionCollection regions)
        {
            return this.GetDesignTimeHtml();
        }

        public static DesignTimeResourceProviderFactory GetDesignTimeResourceProviderFactory(IServiceProvider serviceProvider)
        {
            DesignTimeResourceProviderFactory factory = null;
            IWebApplication application = (IWebApplication) serviceProvider.GetService(typeof(IWebApplication));
            System.Configuration.Configuration configuration = null;
            if (application != null)
            {
                configuration = application.OpenWebConfiguration(true);
                if (configuration != null)
                {
                    GlobalizationSection section = configuration.GetSection("system.web/globalization") as GlobalizationSection;
                    if (section != null)
                    {
                        string resourceProviderFactoryType = section.ResourceProviderFactoryType;
                        if (!string.IsNullOrEmpty(resourceProviderFactoryType))
                        {
                            ITypeResolutionService service = (ITypeResolutionService) serviceProvider.GetService(typeof(ITypeResolutionService));
                            if (service != null)
                            {
                                System.Type type = service.GetType(resourceProviderFactoryType, true, true);
                                if (type != null)
                                {
                                    object[] customAttributes = type.GetCustomAttributes(typeof(DesignTimeResourceProviderFactoryAttribute), true);
                                    if ((customAttributes != null) && (customAttributes.Length > 0))
                                    {
                                        DesignTimeResourceProviderFactoryAttribute attribute = customAttributes[0] as DesignTimeResourceProviderFactoryAttribute;
                                        string factoryTypeName = attribute.FactoryTypeName;
                                        if (!string.IsNullOrEmpty(factoryTypeName))
                                        {
                                            System.Type c = service.GetType(factoryTypeName, true, true);
                                            if ((c != null) && typeof(DesignTimeResourceProviderFactory).IsAssignableFrom(c))
                                            {
                                                try
                                                {
                                                    factory = (DesignTimeResourceProviderFactory) Activator.CreateInstance(c);
                                                }
                                                catch (Exception exception)
                                                {
                                                    if (serviceProvider != null)
                                                    {
                                                        IComponentDesignerDebugService service2 = (IComponentDesignerDebugService) serviceProvider.GetService(typeof(IComponentDesignerDebugService));
                                                        if (service2 != null)
                                                        {
                                                            service2.Fail(System.Design.SR.GetString("ControlDesigner_CouldNotGetDesignTimeResourceProviderFactory", new object[] { factoryTypeName, exception.Message }));
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            if (factory == null)
            {
                IDesignTimeResourceProviderFactoryService service3 = (IDesignTimeResourceProviderFactoryService) serviceProvider.GetService(typeof(IDesignTimeResourceProviderFactoryService));
                if (service3 != null)
                {
                    factory = service3.GetFactory();
                }
            }
            return factory;
        }

        public virtual string GetEditableDesignerRegionContent(EditableDesignerRegion region)
        {
            return string.Empty;
        }

        protected virtual string GetEmptyDesignTimeHtml()
        {
            string name = base.Component.GetType().Name;
            string str2 = base.Component.Site.Name;
            if ((str2 != null) && (str2.Length > 0))
            {
                return ("[ " + name + " \"" + str2 + "\" ]");
            }
            return ("[ " + name + " ]");
        }

        protected virtual string GetErrorDesignTimeHtml(Exception e)
        {
            return this.CreateErrorDesignTimeHtml(System.Design.SR.GetString("ControlDesigner_UnhandledException"), e);
        }

        public virtual string GetPersistenceContent()
        {
            return this.GetPersistInnerHtml();
        }

        [Obsolete("The recommended alternative is GetPersistenceContent(). http://go.microsoft.com/fwlink/?linkid=14202")]
        public virtual string GetPersistInnerHtml()
        {
            return this.GetPersistInnerHtmlInternal();
        }

        internal virtual string GetPersistInnerHtmlInternal()
        {
            if (this._localizedInnerContent != null)
            {
                return this._localizedInnerContent;
            }
            if (!this.IsDirtyInternal)
            {
                return null;
            }
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            this.IsDirtyInternal = false;
            return ControlSerializer.SerializeInnerContents((System.Web.UI.Control) base.Component, service);
        }

        internal static DataRow GetSchemeDataRow(string schemeName, string schemes)
        {
            return GetSchemesTable(schemes).Rows.Find(schemeName);
        }

        private static DataTable GetSchemesTable(string schemes)
        {
            DataSet set = new DataSet {
                Locale = CultureInfo.InvariantCulture
            };
            set.ReadXml(new XmlTextReader(new StringReader(schemes)));
            DataTable table = set.Tables[0];
            table.PrimaryKey = new DataColumn[] { table.Columns["SchemeName"] };
            return table;
        }

        public ViewRendering GetViewRendering()
        {
            EditableDesignerRegion containingRegion = null;
            if (this.View != null)
            {
                containingRegion = this.View.ContainingRegion as EditableDesignerRegion;
            }
            if (containingRegion != null)
            {
                return containingRegion.GetChildViewRendering((System.Web.UI.Control) base.Component);
            }
            return GetViewRendering(this);
        }

        public static ViewRendering GetViewRendering(System.Web.UI.Control control)
        {
            ControlDesigner designer = null;
            ISite site = control.Site;
            if (site != null)
            {
                IDesignerHost service = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                designer = service.GetDesigner(control) as ControlDesigner;
            }
            return GetViewRendering(designer);
        }

        public static ViewRendering GetViewRendering(ControlDesigner designer)
        {
            string content = string.Empty;
            DesignerRegionCollection regions = new DesignerRegionCollection();
            bool visible = true;
            if (designer != null)
            {
                bool supportsRegions = false;
                if (designer.View != null)
                {
                    supportsRegions = designer.View.SupportsRegions;
                }
                try
                {
                    designer.ViewControlCreated = false;
                    if (supportsRegions)
                    {
                        content = designer.GetDesignTimeHtml(regions);
                    }
                    else
                    {
                        content = designer.GetDesignTimeHtml();
                    }
                    visible = designer.Visible;
                }
                catch (Exception exception)
                {
                    regions.Clear();
                    try
                    {
                        content = designer.GetErrorDesignTimeHtml(exception);
                    }
                    catch (Exception exception2)
                    {
                        content = designer.CreateErrorDesignTimeHtml(exception2.Message);
                    }
                    visible = true;
                }
            }
            return new ViewRendering(content, regions, visible);
        }

        internal void HideAllPropertiesUnlessExcluded(IDictionary properties, string[] propertiesToExclude)
        {
            ICollection values = properties.Values;
            if (values != null)
            {
                object[] array = new object[values.Count];
                values.CopyTo(array, 0);
                for (int i = 0; i < array.Length; i++)
                {
                    Predicate<string> match = null;
                    PropertyDescriptor prop = (PropertyDescriptor) array[i];
                    if (prop != null)
                    {
                        if (match == null)
                        {
                            match = s => prop.Name.Equals(s, StringComparison.OrdinalIgnoreCase);
                        }
                        if (!Array.Exists<string>(propertiesToExclude, match))
                        {
                            properties[prop.Name] = TypeDescriptor.CreateProperty(prop.ComponentType, prop, new Attribute[] { BrowsableAttribute.No });
                        }
                    }
                }
            }
        }

        private void IgnoreComponentChanges(bool ignore)
        {
            this._ignoreComponentChangesCount += ignore ? 1 : -1;
        }

        public override void Initialize(IComponent component)
        {
            VerifyInitializeArgument(component, typeof(System.Web.UI.Control));
            base.Initialize(component);
            if (this.RootDesigner != null)
            {
                this.RootDesigner.GetControlViewAndTag((System.Web.UI.Control) base.Component, out this._view, out this._tag);
                if (this._view != null)
                {
                    this._view.ViewEvent += new ViewEventHandler(this.OnViewEvent);
                }
            }
            base.Expressions.Changed += new EventHandler(this.OnExpressionsChanged);
            this.isWebControl = component is WebControl;
            this.UpdateExpressionValues(component);
            this.SetDesignTimeRenderingCompatibility((System.Web.UI.Control) base.Component);
        }

        public void Invalidate()
        {
            if (this.View != null)
            {
                this.Invalidate(this.View.GetBounds(null));
            }
        }

        public void Invalidate(Rectangle rectangle)
        {
            if (this.View != null)
            {
                this.View.Invalidate(rectangle);
            }
        }

        public static void InvokeTransactedChange(IComponent component, TransactedChangeCallback callback, object context, string description)
        {
            InvokeTransactedChange(component, callback, context, description, null);
        }

        public static void InvokeTransactedChange(IComponent component, TransactedChangeCallback callback, object context, string description, MemberDescriptor member)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            InvokeTransactedChange(component.Site, component, callback, context, description, member);
        }

        public static void InvokeTransactedChange(IServiceProvider serviceProvider, IComponent component, TransactedChangeCallback callback, object context, string description, MemberDescriptor member)
        {
            if (component == null)
            {
                throw new ArgumentNullException("component");
            }
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }
            if (serviceProvider == null)
            {
                throw new ArgumentException(System.Design.SR.GetString("ControlDesigner_TransactedChangeRequiresServiceProvider"), "serviceProvider");
            }
            IDesignerHost host = (IDesignerHost) serviceProvider.GetService(typeof(IDesignerHost));
            using (DesignerTransaction transaction = host.CreateTransaction(description))
            {
                IComponentChangeService service = (IComponentChangeService) serviceProvider.GetService(typeof(IComponentChangeService));
                if (service != null)
                {
                    try
                    {
                        service.OnComponentChanging(component, member);
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
                ControlDesigner designer = host.GetDesigner(component) as ControlDesigner;
                bool flag = false;
                try
                {
                    if (designer != null)
                    {
                        designer.IgnoreComponentChanges(true);
                    }
                    if (callback(context))
                    {
                        if (designer != null)
                        {
                            flag = true;
                            designer.IgnoreComponentChanges(false);
                        }
                        if (service != null)
                        {
                            service.OnComponentChanged(component, member, null, null);
                        }
                        TypeDescriptor.Refresh(component);
                        transaction.Commit();
                    }
                }
                finally
                {
                    if ((designer != null) && !flag)
                    {
                        designer.IgnoreComponentChanges(false);
                    }
                }
            }
        }

        [Obsolete("The recommended alternative is DataBindings.Contains(string). The DataBindings collection allows more control of the databindings associated with the control. http://go.microsoft.com/fwlink/?linkid=14202")]
        public bool IsPropertyBound(string propName)
        {
            return (base.DataBindings[propName] != null);
        }

        public void Localize(IDesignTimeResourceWriter resourceWriter)
        {
            string str;
            this.OnComponentChanging(base.Component, new ComponentChangingEventArgs(base.Component, null));
            string str2 = ControlLocalizer.LocalizeControl((System.Web.UI.Control) base.Component, resourceWriter, out str);
            if (!string.IsNullOrEmpty(str2))
            {
                this.SetTagAttribute("meta:resourcekey", str2, true);
            }
            if (!string.IsNullOrEmpty(str))
            {
                this._localizedInnerContent = str;
            }
            this.OnComponentChanged(base.Component, new ComponentChangedEventArgs(base.Component, null, null, null));
        }

        public virtual void OnAutoFormatApplied(DesignerAutoFormat appliedAutoFormat)
        {
        }

        [Obsolete("The recommended alternative is to handle the Changed event on the DataBindings collection. The DataBindings collection allows more control of the databindings associated with the control. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected override void OnBindingsCollectionChanged(string propName)
        {
            if (this.Tag != null)
            {
                DataBindingCollection dataBindings = base.DataBindings;
                if (propName != null)
                {
                    DataBinding binding = dataBindings[propName];
                    string name = propName.Replace('.', '-');
                    if (binding == null)
                    {
                        this.Tag.RemoveAttribute(name);
                    }
                    else
                    {
                        string str2 = "<%# " + binding.Expression + " %>";
                        this.Tag.SetAttribute(name, str2);
                        if (name.IndexOf('-') < 0)
                        {
                            this.ResetPropertyValue(name, false);
                        }
                    }
                }
                else
                {
                    foreach (string str3 in dataBindings.RemovedBindings)
                    {
                        string str4 = str3.Replace('.', '-');
                        this.Tag.RemoveAttribute(str4);
                    }
                    foreach (DataBinding binding2 in dataBindings)
                    {
                        string str5 = "<%# " + binding2.Expression + " %>";
                        string str6 = binding2.PropertyName.Replace('.', '-');
                        this.Tag.SetAttribute(str6, str5);
                        if (str6.IndexOf('-') < 0)
                        {
                            this.ResetPropertyValue(str6, false);
                        }
                    }
                }
            }
        }

        protected virtual void OnClick(DesignerRegionMouseEventArgs e)
        {
        }

        public virtual void OnComponentChanged(object sender, ComponentChangedEventArgs ce)
        {
            if (!this.IsIgnoringComponentChanges)
            {
                IComponent component = base.Component;
                if (base.DesignTimeElementInternal != null)
                {
                    MemberDescriptor member = ce.Member;
                    if (member != null)
                    {
                        PropertyDescriptor propDesc = member as PropertyDescriptor;
                        BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase;
                        if (((propDesc == null) || (component.GetType().GetProperty(propDesc.Name, bindingAttr) == null)) || ((ce.NewValue != null) && (ce.NewValue == ce.OldValue)))
                        {
                            return;
                        }
                        if (propDesc.SerializationVisibility != DesignerSerializationVisibility.Hidden)
                        {
                            this.IsDirtyInternal = true;
                            PersistenceModeAttribute attribute = (PersistenceModeAttribute) member.Attributes[typeof(PersistenceModeAttribute)];
                            switch (attribute.Mode)
                            {
                                case PersistenceMode.Attribute:
                                case PersistenceMode.InnerDefaultProperty:
                                case PersistenceMode.EncodedInnerDefaultProperty:
                                {
                                    string name = member.Name;
                                    if (ce.Component == base.Component)
                                    {
                                        if (base.DataBindings.Contains(name))
                                        {
                                            base.DataBindings.Remove(name, false);
                                            this.RemoveTagAttribute(name, true);
                                        }
                                        if (base.Expressions.Contains(name))
                                        {
                                            ExpressionBinding binding = base.Expressions[name];
                                            if (!binding.Generated)
                                            {
                                                base.Expressions.Remove(name, false);
                                                this.RemoveTagAttribute(name, true);
                                            }
                                            this._expressionsChanged = true;
                                        }
                                    }
                                    System.Web.UI.Control control = (System.Web.UI.Control) ce.Component;
                                    IDesignerHost service = null;
                                    if (control.Site != null)
                                    {
                                        service = (IDesignerHost) control.Site.GetService(typeof(IDesignerHost));
                                    }
                                    if (service != null)
                                    {
                                        ArrayList attributes = ControlSerializer.GetControlPersistedAttribute(control, propDesc, service);
                                        this.PersistAttributes(attributes);
                                    }
                                    break;
                                }
                            }
                        }
                    }
                    else
                    {
                        this.IsDirtyInternal = true;
                        System.Web.UI.Control control2 = (System.Web.UI.Control) ce.Component;
                        IDesignerHost host = null;
                        if (control2.Site != null)
                        {
                            host = (IDesignerHost) control2.Site.GetService(typeof(IDesignerHost));
                        }
                        foreach (string str2 in base.Expressions.RemovedBindings)
                        {
                            object obj2;
                            PropertyDescriptor descriptor3 = GetComplexProperty(base.Component, str2, out obj2);
                            if (descriptor3 != null)
                            {
                                this.IgnoreComponentChanges(true);
                                try
                                {
                                    descriptor3.ResetValue(obj2);
                                }
                                finally
                                {
                                    this.IgnoreComponentChanges(false);
                                }
                            }
                        }
                        if (host != null)
                        {
                            ArrayList controlPersistedAttributes = ControlSerializer.GetControlPersistedAttributes(control2, host);
                            this.PersistAttributes(controlPersistedAttributes);
                        }
                        foreach (DataBinding binding2 in base.DataBindings)
                        {
                            if (binding2.PropertyName.IndexOf('.') < 0)
                            {
                                this.ResetPropertyValue(binding2.PropertyName, false);
                            }
                        }
                        base.OnBindingsCollectionChangedInternal(null);
                        this._expressionsChanged = true;
                    }
                    if (this._expressionsChanged)
                    {
                        this.UpdateExpressionValues(base.Component);
                    }
                    this.UpdateDesignTimeHtml();
                }
            }
        }

        public virtual void OnComponentChanging(object sender, ComponentChangingEventArgs ce)
        {
        }

        [Obsolete("The recommended alternative is OnComponentChanged(). OnComponentChanged is called when any property of the control is changed. http://go.microsoft.com/fwlink/?linkid=14202")]
        protected virtual void OnControlResize()
        {
        }

        private void OnExpressionsChanged(object sender, EventArgs e)
        {
            this._expressionsChanged = true;
        }

        protected virtual void OnPaint(PaintEventArgs e)
        {
        }

        private void OnViewEvent(object sender, ViewEventArgs e)
        {
            if (e.EventType == ViewEvent.Click)
            {
                this.OnClick((DesignerRegionMouseEventArgs) e.EventArgs);
            }
            else if (e.EventType == ViewEvent.Paint)
            {
                this.OnPaint((PaintEventArgs) e.EventArgs);
            }
            else if (e.EventType == ViewEvent.TemplateModeChanged)
            {
                TemplateModeChangedEventArgs eventArgs = (TemplateModeChangedEventArgs) e.EventArgs;
                this._inTemplateMode = eventArgs.NewTemplateGroup != null;
                TypeDescriptor.Refresh(base.Component);
            }
        }

        private void PersistAttributes(ArrayList attributes)
        {
            foreach (Triplet triplet in attributes)
            {
                string name = Convert.ToString(triplet.Second, CultureInfo.InvariantCulture);
                string str2 = triplet.First.ToString();
                if ((str2 == null) || (str2.Length > 0))
                {
                    name = str2 + ':' + name;
                }
                if (triplet.Third == null)
                {
                    this.RemoveTagAttribute(name, true);
                }
                else
                {
                    string str3 = Convert.ToString(triplet.Third, CultureInfo.InvariantCulture);
                    this.SetTagAttribute(name, str3, true);
                }
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);
            PropertyDescriptor oldPropertyDescriptor = (PropertyDescriptor) properties["ID"];
            if (oldPropertyDescriptor != null)
            {
                properties["ID"] = TypeDescriptor.CreateProperty(base.GetType(), oldPropertyDescriptor, emptyAttrs);
            }
            oldPropertyDescriptor = (PropertyDescriptor) properties["SkinID"];
            if (oldPropertyDescriptor != null)
            {
                properties["SkinID"] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { new TypeConverterAttribute(typeof(SkinIDTypeConverter)) });
            }
            if (this.InTemplateMode)
            {
                if (this.HidePropertiesInTemplateMode)
                {
                    this.HideAllPropertiesUnlessExcluded(properties, this.DefaultEnabledPropertyInGrid);
                }
                oldPropertyDescriptor = (PropertyDescriptor) properties["ID"];
                if (oldPropertyDescriptor != null)
                {
                    properties[oldPropertyDescriptor.Name] = TypeDescriptor.CreateProperty(oldPropertyDescriptor.ComponentType, oldPropertyDescriptor, new Attribute[] { ReadOnlyAttribute.Yes });
                }
            }
        }

        [Obsolete("Use of this method is not recommended because resizing is handled by the OnComponentChanged() method. http://go.microsoft.com/fwlink/?linkid=14202")]
        public void RaiseResizeEvent()
        {
            this.OnControlResize();
        }

        public void RegisterClone(object original, object clone)
        {
            if (original == null)
            {
                throw new ArgumentNullException("original");
            }
            if (clone == null)
            {
                throw new ArgumentNullException("clone");
            }
            ControlBuilder controlBuilder = ((IControlBuilderAccessor) base.Component).ControlBuilder;
            if (controlBuilder != null)
            {
                ObjectPersistData objectPersistData = controlBuilder.GetObjectPersistData();
                objectPersistData.BuiltObjects[clone] = objectPersistData.BuiltObjects[original];
            }
        }

        private void RemoveTagAttribute(string name, bool ignoreCase)
        {
            if (this.Tag != null)
            {
                this.Tag.RemoveAttribute(name);
            }
            else
            {
                this.BehaviorInternal.RemoveAttribute(name, ignoreCase);
            }
        }

        private void ResetPropertyValue(string property, bool useInstance)
        {
            PropertyDescriptor descriptor = null;
            if (useInstance)
            {
                descriptor = TypeDescriptor.GetProperties(base.Component)[property];
            }
            else
            {
                descriptor = TypeDescriptor.GetProperties(base.Component.GetType())[property];
            }
            if (descriptor != null)
            {
                this.IgnoreComponentChanges(true);
                try
                {
                    descriptor.ResetValue(base.Component);
                }
                finally
                {
                    this.IgnoreComponentChanges(false);
                }
            }
        }

        private void SetDesignTimeRenderingCompatibility(System.Web.UI.Control control)
        {
            if (TypeDescriptor.GetProvider(control).GetTypeDescriptor(control).GetProperties().Find("RenderingCompatibility", false) == null)
            {
                control.RenderingCompatibility = new Version(3, 5);
            }
            else if (control.Site != null)
            {
                IWebApplication service = (IWebApplication) control.Site.GetService(typeof(IWebApplication));
                if (service != null)
                {
                    PagesSection section = (PagesSection) service.OpenWebConfiguration(true).GetSection("system.web/pages");
                    if (section != null)
                    {
                        control.RenderingCompatibility = section.ControlRenderingCompatibilityVersion;
                    }
                }
            }
        }

        public virtual void SetEditableDesignerRegionContent(EditableDesignerRegion region, string content)
        {
        }

        protected void SetRegionContent(EditableDesignerRegion region, string content)
        {
            if (this.View != null)
            {
                this.View.SetRegionContent(region, content);
            }
        }

        private void SetTagAttribute(string name, object value, bool ignoreCase)
        {
            if (this.Tag != null)
            {
                this.Tag.SetAttribute(name, value.ToString());
            }
            else
            {
                this.BehaviorInternal.SetAttribute(name, value, ignoreCase);
            }
        }

        protected void SetViewFlags(ViewFlags viewFlags, bool setFlag)
        {
            if (this.View != null)
            {
                this.View.SetFlags(viewFlags, setFlag);
            }
        }

        public virtual void UpdateDesignTimeHtml()
        {
            if (this.View != null)
            {
                this.View.Update();
            }
            else if (this.ReadOnlyInternal)
            {
                IHtmlControlDesignerBehavior behaviorInternal = this.BehaviorInternal;
                if (behaviorInternal != null)
                {
                    ((IControlDesignerBehavior) behaviorInternal).DesignTimeHtml = this.GetDesignTimeHtml();
                }
            }
        }

        private void UpdateExpressionValues(IComponent target)
        {
            IExpressionsAccessor accessor = target as IExpressionsAccessor;
            TemplateControl templateControl = null;
            IDesignerHost service = (IDesignerHost) this.GetService(typeof(IDesignerHost));
            if (service != null)
            {
                templateControl = service.RootComponent as TemplateControl;
            }
            foreach (ExpressionBinding binding in accessor.Expressions)
            {
                if (!binding.Generated)
                {
                    object obj2;
                    string propertyName = binding.PropertyName;
                    PropertyDescriptor descriptor = GetComplexProperty(target, propertyName, out obj2);
                    if (descriptor != null)
                    {
                        this.IgnoreComponentChanges(true);
                        try
                        {
                            ExpressionEditor expressionEditor = ExpressionEditor.GetExpressionEditor(binding.ExpressionPrefix, target.Site);
                            if (expressionEditor != null)
                            {
                                object parseTimeData = this.EnsureParsedExpression(templateControl, binding, binding.ParsedExpressionData);
                                object obj4 = expressionEditor.EvaluateExpression(binding.Expression, parseTimeData, descriptor.PropertyType, target.Site);
                                if (obj4 != null)
                                {
                                    if (obj4 is string)
                                    {
                                        TypeConverter converter = descriptor.Converter;
                                        if ((converter != null) && converter.CanConvertFrom(typeof(string)))
                                        {
                                            obj4 = converter.ConvertFromInvariantString((string) obj4);
                                        }
                                    }
                                    descriptor.SetValue(obj2, obj4);
                                }
                                else
                                {
                                    descriptor.SetValue(obj2, System.Design.SR.GetString("ExpressionEditor_ExpressionBound", new object[] { binding.Expression }));
                                }
                            }
                            else
                            {
                                descriptor.SetValue(obj2, System.Design.SR.GetString("ExpressionEditor_ExpressionBound", new object[] { binding.Expression }));
                            }
                        }
                        catch
                        {
                        }
                        finally
                        {
                            this.IgnoreComponentChanges(false);
                        }
                    }
                }
            }
            this._expressionsChanged = false;
        }

        internal bool UseRegions(DesignerRegionCollection regions, ITemplate componentTemplate)
        {
            return (this.UseRegionsCore(regions) && (componentTemplate != null));
        }

        internal bool UseRegions(DesignerRegionCollection regions, ITemplate componentTemplate, ITemplate viewControlTemplate)
        {
            bool flag = (componentTemplate == null) && (viewControlTemplate != null);
            return (this.UseRegionsCore(regions) && !flag);
        }

        private bool UseRegionsCore(DesignerRegionCollection regions)
        {
            return (((regions != null) && (this.View != null)) && this.View.SupportsRegions);
        }

        internal static void VerifyInitializeArgument(IComponent component, System.Type expectedType)
        {
            if (!expectedType.IsInstanceOfType(component))
            {
                throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, System.Design.SR.GetString("ControlDesigner_ArgumentMustBeOfType"), new object[] { expectedType.FullName }), "component");
            }
        }

        public override DesignerActionListCollection ActionLists
        {
            get
            {
                DesignerActionListCollection lists = new DesignerActionListCollection();
                lists.AddRange(base.ActionLists);
                lists.Add(new ControlDesignerActionList(this));
                return lists;
            }
        }

        public virtual bool AllowResize
        {
            get
            {
                return this.IsWebControl;
            }
        }

        public virtual DesignerAutoFormatCollection AutoFormats
        {
            get
            {
                return new DesignerAutoFormatCollection();
            }
        }

        protected virtual bool DataBindingsEnabled
        {
            get
            {
                IControlDesignerView view = this.View;
                while (view != null)
                {
                    EditableDesignerRegion containingRegion = (EditableDesignerRegion) view.ContainingRegion;
                    if (containingRegion != null)
                    {
                        if (containingRegion.SupportsDataBinding)
                        {
                            return true;
                        }
                        ControlDesigner designer = containingRegion.Designer;
                        if (designer != null)
                        {
                            view = designer.View;
                            continue;
                        }
                    }
                    return false;
                }
                return false;
            }
        }

        protected ControlDesignerState DesignerState
        {
            get
            {
                if (this._designerState == null)
                {
                    this._designerState = new ControlDesignerState(base.Component);
                }
                return this._designerState;
            }
        }

        [Obsolete("Error: This property can no longer be referenced, and is included to support existing compiled applications. The design-time element view architecture is no longer used. http://go.microsoft.com/fwlink/?linkid=14202", true)]
        protected object DesignTimeElementView
        {
            get
            {
                IHtmlControlDesignerBehavior behaviorInternal = this.BehaviorInternal;
                if (behaviorInternal != null)
                {
                    return ((IControlDesignerBehavior) behaviorInternal).DesignTimeElementView;
                }
                return null;
            }
        }

        [Obsolete("The recommended alternative is SetViewFlags(ViewFlags.DesignTimeHtmlRequiresLoadComplete, true). http://go.microsoft.com/fwlink/?linkid=14202")]
        public virtual bool DesignTimeHtmlRequiresLoadComplete
        {
            get
            {
                return false;
            }
        }

        protected internal virtual bool HidePropertiesInTemplateMode
        {
            get
            {
                return true;
            }
        }

        public virtual string ID
        {
            get
            {
                return ((System.Web.UI.Control) base.Component).ID;
            }
            set
            {
                if (this.RootDesigner != null)
                {
                    this.RootDesigner.SetControlID((System.Web.UI.Control) base.Component, value);
                }
            }
        }

        protected bool InTemplateMode
        {
            get
            {
                return this._inTemplateMode;
            }
        }

        [Obsolete("The recommended alternative is to use Tag.SetDirty() and Tag.IsDirty. http://go.microsoft.com/fwlink/?linkid=14202")]
        public bool IsDirty
        {
            get
            {
                return this.IsDirtyInternal;
            }
            set
            {
                this.IsDirtyInternal = value;
            }
        }

        internal bool IsDirtyInternal
        {
            get
            {
                if (this.Tag != null)
                {
                    return this.Tag.IsDirty;
                }
                return this.fDirty;
            }
            set
            {
                if (this.Tag != null)
                {
                    this.Tag.SetDirty(value);
                }
                else
                {
                    this.fDirty = value;
                }
            }
        }

        internal bool IsIgnoringComponentChanges
        {
            get
            {
                return (this._ignoreComponentChangesCount > 0);
            }
        }

        internal bool IsWebControl
        {
            get
            {
                return this.isWebControl;
            }
        }

        internal string LocalizedInnerContent
        {
            get
            {
                return this._localizedInnerContent;
            }
        }

        [Obsolete("The recommended alternative is to inherit from ContainerControlDesigner instead and to use an EditableDesignerRegion. Regions allow for better control of the content in the designer. http://go.microsoft.com/fwlink/?linkid=14202")]
        public bool ReadOnly
        {
            get
            {
                return this.ReadOnlyInternal;
            }
            set
            {
                this.ReadOnlyInternal = value;
            }
        }

        internal bool ReadOnlyInternal
        {
            get
            {
                return this.readOnly;
            }
            set
            {
                this.readOnly = value;
            }
        }

        protected WebFormsRootDesigner RootDesigner
        {
            get
            {
                WebFormsRootDesigner designer = null;
                ISite site = base.Component.Site;
                if (site != null)
                {
                    IDesignerHost service = (IDesignerHost) site.GetService(typeof(IDesignerHost));
                    if ((service != null) && (service.RootComponent != null))
                    {
                        designer = service.GetDesigner(service.RootComponent) as WebFormsRootDesigner;
                    }
                }
                return designer;
            }
        }

        private bool SupportsDataBindings
        {
            get
            {
                BindableAttribute attribute = (BindableAttribute) TypeDescriptor.GetAttributes(base.Component)[typeof(BindableAttribute)];
                return ((attribute != null) && attribute.Bindable);
            }
        }

        protected IControlDesignerTag Tag
        {
            get
            {
                return this._tag;
            }
        }

        public virtual TemplateGroupCollection TemplateGroups
        {
            get
            {
                return new TemplateGroupCollection();
            }
        }

        protected virtual bool UsePreviewControl
        {
            get
            {
                object[] customAttributes = base.GetType().GetCustomAttributes(typeof(SupportsPreviewControlAttribute), false);
                if (customAttributes.Length > 0)
                {
                    SupportsPreviewControlAttribute attribute = (SupportsPreviewControlAttribute) customAttributes[0];
                    return attribute.SupportsPreviewControl;
                }
                return false;
            }
        }

        internal IControlDesignerView View
        {
            get
            {
                return this._view;
            }
        }

        public System.Web.UI.Control ViewControl
        {
            get
            {
                if (!this.ViewControlCreated)
                {
                    this._viewControl = this.UsePreviewControl ? this.CreateViewControlInternal() : ((System.Web.UI.Control) base.Component);
                    this.ViewControlCreated = true;
                }
                return this._viewControl;
            }
            set
            {
                this._viewControl = value;
                this.ViewControlCreated = true;
            }
        }

        public virtual bool ViewControlCreated
        {
            get
            {
                return this._viewControlCreated;
            }
            set
            {
                this._viewControlCreated = value;
            }
        }

        protected virtual bool Visible
        {
            get
            {
                return true;
            }
        }

        internal class ControlDesignerActionList : DesignerActionList
        {
            private ControlDesigner _parent;

            public ControlDesignerActionList(ControlDesigner parent) : base(parent.Component)
            {
                this._parent = parent;
            }

            private bool DataBindingsCallback(object context)
            {
                System.Web.UI.Control component = (System.Web.UI.Control) this._parent.Component;
                ISite serviceProvider = component.Site;
                DataBindingsDialog form = new DataBindingsDialog(serviceProvider, component);
                return (UIServiceHelper.ShowDialog(serviceProvider, form) == DialogResult.OK);
            }

            public void EditDataBindings()
            {
                System.Web.UI.Control component = (System.Web.UI.Control) this._parent.Component;
                if (string.IsNullOrEmpty(component.ID))
                {
                    UIServiceHelper.ShowMessage(component.Site, System.Design.SR.GetString("ControlDesigner_EditDataBindingsRequiresID"));
                }
                else
                {
                    ControlDesigner.InvokeTransactedChange(component, new TransactedChangeCallback(this.DataBindingsCallback), null, System.Design.SR.GetString("Designer_DataBindingsVerb"));
                    this._parent.UpdateDesignTimeHtml();
                }
            }

            public override DesignerActionItemCollection GetSortedActionItems()
            {
                DesignerActionItemCollection items = new DesignerActionItemCollection();
                if (this._parent.SupportsDataBindings && this._parent.DataBindingsEnabled)
                {
                    items.Add(new DesignerActionMethodItem(this, "EditDataBindings", System.Design.SR.GetString("Designer_DataBindingsVerb"), string.Empty, System.Design.SR.GetString("Designer_DataBindingsVerbDesc"), true));
                }
                return items;
            }

            public override bool AutoShow
            {
                get
                {
                    return true;
                }
                set
                {
                }
            }
        }
    }
}

