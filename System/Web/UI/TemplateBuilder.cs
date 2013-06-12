namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Web;

    public class TemplateBuilder : ControlBuilder, ITemplate
    {
        private bool _allowMultipleInstances = true;
        private IDesignerHost _designerHost;
        internal string _tagInnerText;

        public override object BuildObject()
        {
            return this;
        }

        public override void CloseControl()
        {
            base.CloseControl();
            if ((base.InPageTheme && (base.ParentBuilder != null)) && base.ParentBuilder.IsControlSkin)
            {
                ((PageThemeParser) base.Parser).CurrentSkinBuilder = null;
            }
        }

        public override void Init(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string ID, IDictionary attribs)
        {
            base.Init(parser, parentBuilder, type, tagName, ID, attribs);
            if ((base.InPageTheme && (base.ParentBuilder != null)) && base.ParentBuilder.IsControlSkin)
            {
                ((PageThemeParser) base.Parser).CurrentSkinBuilder = parentBuilder;
            }
        }

        public virtual void InstantiateIn(Control container)
        {
            IServiceProvider serviceProvider = null;
            if (this._designerHost != null)
            {
                serviceProvider = this._designerHost;
            }
            else if (!base.IsNoCompile)
            {
                ServiceContainer container2 = new ServiceContainer();
                if (container is IThemeResolutionService)
                {
                    container2.AddService(typeof(IThemeResolutionService), (IThemeResolutionService) container);
                }
                if (container is IFilterResolutionService)
                {
                    container2.AddService(typeof(IFilterResolutionService), (IFilterResolutionService) container);
                }
                serviceProvider = container2;
            }
            HttpContext current = null;
            TemplateControl templateControl = null;
            TemplateControl control2 = container as TemplateControl;
            if (control2 != null)
            {
                current = HttpContext.Current;
                if (current != null)
                {
                    templateControl = current.TemplateControl;
                }
            }
            try
            {
                if (!base.IsNoCompile)
                {
                    base.SetServiceProvider(serviceProvider);
                }
                if (current != null)
                {
                    current.TemplateControl = control2;
                }
                this.BuildChildren(container);
            }
            finally
            {
                if (!base.IsNoCompile)
                {
                    base.SetServiceProvider(null);
                }
                if (current != null)
                {
                    current.TemplateControl = templateControl;
                }
            }
        }

        public override bool NeedsTagInnerText()
        {
            return base.InDesigner;
        }

        internal void SetDesignerHost(IDesignerHost designerHost)
        {
            this._designerHost = designerHost;
        }

        public override void SetTagInnerText(string text)
        {
            this._tagInnerText = text;
        }

        internal bool AllowMultipleInstances
        {
            get
            {
                return this._allowMultipleInstances;
            }
            set
            {
                this._allowMultipleInstances = value;
            }
        }

        public virtual string Text
        {
            get
            {
                return this._tagInnerText;
            }
            set
            {
                this._tagInnerText = value;
            }
        }
    }
}

