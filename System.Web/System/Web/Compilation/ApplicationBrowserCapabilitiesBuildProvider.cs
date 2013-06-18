namespace System.Web.Compilation
{
    using System;
    using System.Web.Hosting;

    internal class ApplicationBrowserCapabilitiesBuildProvider : BuildProvider
    {
        private ApplicationBrowserCapabilitiesCodeGenerator _codeGenerator;

        internal ApplicationBrowserCapabilitiesBuildProvider()
        {
            this._codeGenerator = new ApplicationBrowserCapabilitiesCodeGenerator(this);
        }

        internal void AddFile(string virtualPath)
        {
            string filePath = HostingEnvironment.MapPathInternal(virtualPath);
            this._codeGenerator.AddFile(filePath);
        }

        public override void GenerateCode(AssemblyBuilder assemblyBuilder)
        {
            this._codeGenerator.GenerateCode(assemblyBuilder);
        }
    }
}

