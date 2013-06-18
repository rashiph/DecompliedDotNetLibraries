namespace System.Configuration
{
    using System;

    public sealed class ContextInformation
    {
        private BaseConfigurationRecord _configRecord;
        private object _hostingContext = null;
        private bool _hostingContextEvaluated = false;

        internal ContextInformation(BaseConfigurationRecord configRecord)
        {
            this._configRecord = configRecord;
        }

        public object GetSection(string sectionName)
        {
            return this._configRecord.GetSection(sectionName);
        }

        public object HostingContext
        {
            get
            {
                if (!this._hostingContextEvaluated)
                {
                    this._hostingContext = this._configRecord.ConfigContext;
                    this._hostingContextEvaluated = true;
                }
                return this._hostingContext;
            }
        }

        public bool IsMachineLevel
        {
            get
            {
                return this._configRecord.IsMachineConfig;
            }
        }
    }
}

