namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Web.Hosting;

    internal class ProtocolsConfigurationEntry
    {
        private Type _appDomainHandlerType;
        private string _appDomainHandlerTypeName;
        private int _configFileLine;
        private string _configFileName;
        private string _id;
        private Type _processHandlerType;
        private string _processHandlerTypeName;
        private bool _typesValidated;

        internal ProtocolsConfigurationEntry(string id, string processHandlerType, string appDomainHandlerType, bool validate, string configFileName, int configFileLine)
        {
            this._id = id;
            this._processHandlerTypeName = processHandlerType;
            this._appDomainHandlerTypeName = appDomainHandlerType;
            this._configFileName = configFileName;
            this._configFileLine = configFileLine;
            if (validate)
            {
                this.ValidateTypes();
            }
        }

        private void ValidateTypes()
        {
            if (!this._typesValidated)
            {
                Type type;
                Type type2;
                try
                {
                    type = Type.GetType(this._processHandlerTypeName, true);
                }
                catch (Exception exception)
                {
                    throw new ConfigurationErrorsException(exception.Message, exception, this._configFileName, this._configFileLine);
                }
                System.Web.Configuration.HandlerBase.CheckAssignableType(this._configFileName, this._configFileLine, typeof(ProcessProtocolHandler), type);
                try
                {
                    type2 = Type.GetType(this._appDomainHandlerTypeName, true);
                }
                catch (Exception exception2)
                {
                    throw new ConfigurationErrorsException(exception2.Message, exception2, this._configFileName, this._configFileLine);
                }
                System.Web.Configuration.HandlerBase.CheckAssignableType(this._configFileName, this._configFileLine, typeof(AppDomainProtocolHandler), type2);
                this._processHandlerType = type;
                this._appDomainHandlerType = type2;
                this._typesValidated = true;
            }
        }
    }
}

