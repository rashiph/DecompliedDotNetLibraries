namespace System.Management
{
    using System;

    internal class SecuredConnectHandler
    {
        private ManagementScope scope;

        internal SecuredConnectHandler(ManagementScope theScope)
        {
            this.scope = theScope;
        }

        internal int ConnectNSecureIWbemServices(string path, ref IWbemServices pServices)
        {
            int num = -2147217407;
            if (this.scope != null)
            {
                num = WmiNetUtilsHelper.ConnectServerWmi_f(path, this.scope.Options.Username, this.scope.Options.GetPassword(), this.scope.Options.Locale, this.scope.Options.Flags, this.scope.Options.Authority, this.scope.Options.GetContext(), out pServices, (int) this.scope.Options.Impersonation, (int) this.scope.Options.Authentication);
            }
            return num;
        }
    }
}

