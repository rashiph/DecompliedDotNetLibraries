namespace System.Diagnostics
{
    using System;
    using System.Security.Permissions;

    [SwitchLevel(typeof(TraceLevel))]
    public class TraceSwitch : Switch
    {
        public TraceSwitch(string displayName, string description) : base(displayName, description)
        {
        }

        public TraceSwitch(string displayName, string description, string defaultSwitchValue) : base(displayName, description, defaultSwitchValue)
        {
        }

        protected override void OnSwitchSettingChanged()
        {
            int switchSetting = base.SwitchSetting;
            if (switchSetting < 0)
            {
                Trace.WriteLine(SR.GetString("TraceSwitchLevelTooLow", new object[] { base.DisplayName }));
                base.SwitchSetting = 0;
            }
            else if (switchSetting > 4)
            {
                Trace.WriteLine(SR.GetString("TraceSwitchLevelTooHigh", new object[] { base.DisplayName }));
                base.SwitchSetting = 4;
            }
        }

        protected override void OnValueChanged()
        {
            base.SwitchSetting = (int) Enum.Parse(typeof(TraceLevel), base.Value, true);
        }

        public TraceLevel Level
        {
            get
            {
                return (TraceLevel) base.SwitchSetting;
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            set
            {
                if ((value < TraceLevel.Off) || (value > TraceLevel.Verbose))
                {
                    throw new ArgumentException(SR.GetString("TraceSwitchInvalidLevel"));
                }
                base.SwitchSetting = (int) value;
            }
        }

        public bool TraceError
        {
            get
            {
                return (this.Level >= TraceLevel.Error);
            }
        }

        public bool TraceInfo
        {
            get
            {
                return (this.Level >= TraceLevel.Info);
            }
        }

        public bool TraceVerbose
        {
            get
            {
                return (this.Level == TraceLevel.Verbose);
            }
        }

        public bool TraceWarning
        {
            get
            {
                return (this.Level >= TraceLevel.Warning);
            }
        }
    }
}

