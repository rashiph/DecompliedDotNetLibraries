namespace System.Diagnostics
{
    using System;
    using System.Security.Permissions;

    [SwitchLevel(typeof(bool))]
    public class BooleanSwitch : Switch
    {
        public BooleanSwitch(string displayName, string description) : base(displayName, description)
        {
        }

        public BooleanSwitch(string displayName, string description, string defaultSwitchValue) : base(displayName, description, defaultSwitchValue)
        {
        }

        protected override void OnValueChanged()
        {
            bool flag;
            if (bool.TryParse(base.Value, out flag))
            {
                base.SwitchSetting = flag ? 1 : 0;
            }
            else
            {
                base.OnValueChanged();
            }
        }

        public bool Enabled
        {
            get
            {
                return (base.SwitchSetting != 0);
            }
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            set
            {
                base.SwitchSetting = value ? 1 : 0;
            }
        }
    }
}

