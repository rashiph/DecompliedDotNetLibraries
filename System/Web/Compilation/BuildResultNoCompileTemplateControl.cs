namespace System.Web.Compilation
{
    using System;
    using System.Web;
    using System.Web.UI;
    using System.Web.Util;

    internal abstract class BuildResultNoCompileTemplateControl : BuildResult, ITypedWebObjectFactory, IWebObjectFactory
    {
        protected Type _baseType;
        protected bool _initialized;
        protected RootBuilder _rootBuilder;

        internal BuildResultNoCompileTemplateControl(Type baseType, TemplateParser parser)
        {
            this._baseType = baseType;
            this._rootBuilder = parser.RootBuilder;
            this._rootBuilder.PrepareNoCompilePageSupport();
        }

        public virtual object CreateInstance()
        {
            TemplateControl control = (TemplateControl) HttpRuntime.FastCreatePublicInstance(this._baseType);
            control.TemplateControlVirtualPath = base.VirtualPath;
            control.TemplateControlVirtualDirectory = base.VirtualPath.Parent;
            control.SetNoCompileBuildResult(this);
            return control;
        }

        internal virtual void FrameworkInitialize(TemplateControl templateControl)
        {
            HttpContext current = HttpContext.Current;
            TemplateControl control = current.TemplateControl;
            current.TemplateControl = templateControl;
            try
            {
                if (!this._initialized)
                {
                    lock (this)
                    {
                        this._rootBuilder.InitObject(templateControl);
                    }
                    this._initialized = true;
                }
                else
                {
                    this._rootBuilder.InitObject(templateControl);
                }
            }
            finally
            {
                if (control != null)
                {
                    current.TemplateControl = control;
                }
            }
        }

        internal override BuildResultTypeCode GetCode()
        {
            return BuildResultTypeCode.Invalid;
        }

        internal Type BaseType
        {
            get
            {
                return this._baseType;
            }
        }

        internal override bool CacheToDisk
        {
            get
            {
                return false;
            }
        }

        public virtual Type InstantiatedType
        {
            get
            {
                return this._baseType;
            }
        }

        internal override TimeSpan MemoryCacheSlidingExpiration
        {
            get
            {
                return TimeSpan.FromMinutes(5.0);
            }
        }
    }
}

