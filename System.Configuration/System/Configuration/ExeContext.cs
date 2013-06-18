namespace System.Configuration
{
    using System;
    using System.Runtime;

    public sealed class ExeContext
    {
        private string _exePath;
        private ConfigurationUserLevel _userContext;

        internal ExeContext(ConfigurationUserLevel userContext, string exePath)
        {
            this._userContext = userContext;
            this._exePath = exePath;
        }

        public string ExePath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._exePath;
            }
        }

        public ConfigurationUserLevel UserLevel
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._userContext;
            }
        }
    }
}

