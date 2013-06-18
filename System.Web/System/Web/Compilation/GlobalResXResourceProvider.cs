namespace System.Web.Compilation
{
    using System;
    using System.Resources;

    internal class GlobalResXResourceProvider : BaseResXResourceProvider
    {
        private string _classKey;

        internal GlobalResXResourceProvider(string classKey)
        {
            this._classKey = classKey;
        }

        protected override ResourceManager CreateResourceManager()
        {
            string baseName = "Resources." + this._classKey;
            if (BuildManager.AppResourcesAssembly == null)
            {
                return null;
            }
            return new ResourceManager(baseName, BuildManager.AppResourcesAssembly) { IgnoreCase = true };
        }

        public override IResourceReader ResourceReader
        {
            get
            {
                throw new NotSupportedException();
            }
        }
    }
}

