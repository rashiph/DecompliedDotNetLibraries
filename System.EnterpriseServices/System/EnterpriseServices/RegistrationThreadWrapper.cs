namespace System.EnterpriseServices
{
    using System;
    using System.Runtime.InteropServices;

    internal class RegistrationThreadWrapper
    {
        private Exception _exception;
        private RegistrationHelper _helper;
        private RegistrationConfig _regConfig;

        internal RegistrationThreadWrapper(RegistrationHelper helper, RegistrationConfig regConfig)
        {
            this._regConfig = regConfig;
            this._helper = helper;
            this._exception = null;
        }

        internal void InstallThread()
        {
            try
            {
                this._helper.InstallAssemblyFromConfig(ref this._regConfig);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                this._exception = exception;
            }
        }

        internal void PropInstallResult()
        {
            if (this._exception != null)
            {
                throw this._exception;
            }
        }

        internal void PropUninstallResult()
        {
            if (this._exception != null)
            {
                throw this._exception;
            }
        }

        internal void UninstallThread()
        {
            try
            {
                this._helper.UninstallAssemblyFromConfig(ref this._regConfig);
            }
            catch (Exception exception)
            {
                if ((exception is NullReferenceException) || (exception is SEHException))
                {
                    throw;
                }
                this._exception = exception;
            }
        }
    }
}

