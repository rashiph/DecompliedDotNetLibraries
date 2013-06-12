namespace System.Web.UI
{
    using System;
    using System.Web;
    using System.Web.Util;

    public class PartialCachingControl : BasePartialCachingControl
    {
        private object[] _args;
        private Type _createCachedControlType;
        private IWebObjectFactory _objectFactory;

        internal PartialCachingControl(IWebObjectFactory objectFactory, Type createCachedControlType, PartialCachingAttribute cacheAttrib, string cacheKey, object[] args)
        {
            string providerName = cacheAttrib.ProviderName;
            base._ctrlID = cacheKey;
            base.Duration = new TimeSpan(0, 0, cacheAttrib.Duration);
            base.SetVaryByParamsCollectionFromString(cacheAttrib.VaryByParams);
            if (cacheAttrib.VaryByControls != null)
            {
                base._varyByControlsCollection = cacheAttrib.VaryByControls.Split(new char[] { ';' });
            }
            base._varyByCustom = cacheAttrib.VaryByCustom;
            base._sqlDependency = cacheAttrib.SqlDependency;
            if (providerName == "AspNetInternalProvider")
            {
                providerName = null;
            }
            base._provider = providerName;
            base._guid = cacheKey;
            this._objectFactory = objectFactory;
            this._createCachedControlType = createCachedControlType;
            this._args = args;
        }

        internal override Control CreateCachedControl()
        {
            Control control;
            if (this._objectFactory != null)
            {
                control = (Control) this._objectFactory.CreateInstance();
            }
            else
            {
                control = (Control) HttpRuntime.CreatePublicInstance(this._createCachedControlType, this._args);
            }
            UserControl control2 = control as UserControl;
            if (control2 != null)
            {
                control2.InitializeAsUserControl(this.Page);
            }
            control.ID = base._ctrlID;
            return control;
        }

        public Control CachedControl
        {
            get
            {
                return base._cachedCtrl;
            }
        }
    }
}

