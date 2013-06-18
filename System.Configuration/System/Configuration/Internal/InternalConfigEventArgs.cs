namespace System.Configuration.Internal
{
    using System;
    using System.Runtime;

    public sealed class InternalConfigEventArgs : EventArgs
    {
        private string _configPath;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InternalConfigEventArgs(string configPath)
        {
            this._configPath = configPath;
        }

        public string ConfigPath
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._configPath;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._configPath = value;
            }
        }
    }
}

