namespace System.Diagnostics
{
    using System;
    using System.Security.Permissions;

    public class SourceSwitch : Switch
    {
        public SourceSwitch(string name) : base(name, string.Empty)
        {
        }

        public SourceSwitch(string displayName, string defaultSwitchValue) : base(displayName, string.Empty, defaultSwitchValue)
        {
        }

        protected override void OnValueChanged()
        {
            base.SwitchSetting = (int) Enum.Parse(typeof(SourceLevels), base.Value, true);
        }

        public bool ShouldTrace(TraceEventType eventType)
        {
            return ((base.SwitchSetting & eventType) != 0);
        }

        public SourceLevels Level
        {
            get
            {
                return (SourceLevels) base.SwitchSetting;
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            set
            {
                base.SwitchSetting = (int) value;
            }
        }
    }
}

