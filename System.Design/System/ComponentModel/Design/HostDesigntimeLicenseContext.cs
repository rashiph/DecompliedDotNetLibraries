namespace System.ComponentModel.Design
{
    using System;

    internal class HostDesigntimeLicenseContext : DesigntimeLicenseContext
    {
        private IServiceProvider provider;

        public HostDesigntimeLicenseContext(IServiceProvider provider)
        {
            this.provider = provider;
        }

        public override object GetService(Type serviceClass)
        {
            return this.provider.GetService(serviceClass);
        }
    }
}

