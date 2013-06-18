namespace System.ServiceModel.Channels
{
    using System;
    using System.Runtime.CompilerServices;

    internal class MsmqReceiveContextSettings : IReceiveContextSettings
    {
        public MsmqReceiveContextSettings()
        {
            this.ValidityDuration = MsmqDefaults.ValidityDuration;
        }

        public MsmqReceiveContextSettings(IReceiveContextSettings toBeCloned)
        {
            this.Enabled = toBeCloned.Enabled;
            this.ValidityDuration = toBeCloned.ValidityDuration;
        }

        internal void SetValidityDuration(TimeSpan validityDuration)
        {
            this.ValidityDuration = validityDuration;
        }

        public bool Enabled { get; set; }

        public TimeSpan ValidityDuration { get; private set; }
    }
}

