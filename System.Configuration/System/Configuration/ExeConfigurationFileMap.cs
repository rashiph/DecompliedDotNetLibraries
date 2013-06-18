namespace System.Configuration
{
    using System;
    using System.Runtime;

    public sealed class ExeConfigurationFileMap : ConfigurationFileMap
    {
        private string _exeConfigFilename;
        private string _localUserConfigFilename;
        private string _roamingUserConfigFilename;

        public ExeConfigurationFileMap()
        {
            this._exeConfigFilename = string.Empty;
            this._roamingUserConfigFilename = string.Empty;
            this._localUserConfigFilename = string.Empty;
        }

        public ExeConfigurationFileMap(string machineConfigFileName) : base(machineConfigFileName)
        {
            this._exeConfigFilename = string.Empty;
            this._roamingUserConfigFilename = string.Empty;
            this._localUserConfigFilename = string.Empty;
        }

        private ExeConfigurationFileMap(string machineConfigFileName, string exeConfigFilename, string roamingUserConfigFilename, string localUserConfigFilename) : base(machineConfigFileName)
        {
            this._exeConfigFilename = exeConfigFilename;
            this._roamingUserConfigFilename = roamingUserConfigFilename;
            this._localUserConfigFilename = localUserConfigFilename;
        }

        public override object Clone()
        {
            return new ExeConfigurationFileMap(base.MachineConfigFilename, this._exeConfigFilename, this._roamingUserConfigFilename, this._localUserConfigFilename);
        }

        public string ExeConfigFilename
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._exeConfigFilename;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._exeConfigFilename = value;
            }
        }

        public string LocalUserConfigFilename
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._localUserConfigFilename;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._localUserConfigFilename = value;
            }
        }

        public string RoamingUserConfigFilename
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this._roamingUserConfigFilename;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this._roamingUserConfigFilename = value;
            }
        }
    }
}

