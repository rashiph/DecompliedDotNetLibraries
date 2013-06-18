namespace System.EnterpriseServices
{
    using System;

    internal class BooleanSwitch : BaseSwitch
    {
        internal BooleanSwitch(string name) : base(name)
        {
        }

        internal bool Enabled
        {
            get
            {
                return (base.Value != 0);
            }
        }
    }
}

