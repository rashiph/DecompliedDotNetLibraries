namespace System.Web.Compilation
{
    using System;
    using System.Globalization;
    using System.Resources;

    internal abstract class BaseResXResourceProvider : IResourceProvider
    {
        private ResourceManager _resourceManager;

        protected BaseResXResourceProvider()
        {
        }

        protected abstract ResourceManager CreateResourceManager();
        private void EnsureResourceManager()
        {
            if (this._resourceManager == null)
            {
                this._resourceManager = this.CreateResourceManager();
            }
        }

        public virtual object GetObject(string resourceKey, CultureInfo culture)
        {
            this.EnsureResourceManager();
            if (this._resourceManager == null)
            {
                return null;
            }
            if (culture == null)
            {
                culture = CultureInfo.CurrentUICulture;
            }
            return this._resourceManager.GetObject(resourceKey, culture);
        }

        public virtual IResourceReader ResourceReader
        {
            get
            {
                return null;
            }
        }
    }
}

