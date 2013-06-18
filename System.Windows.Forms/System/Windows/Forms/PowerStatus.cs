namespace System.Windows.Forms
{
    using System;

    public class PowerStatus
    {
        private NativeMethods.SYSTEM_POWER_STATUS systemPowerStatus;

        internal PowerStatus()
        {
        }

        private void UpdateSystemPowerStatus()
        {
            UnsafeNativeMethods.GetSystemPowerStatus(ref this.systemPowerStatus);
        }

        public System.Windows.Forms.BatteryChargeStatus BatteryChargeStatus
        {
            get
            {
                this.UpdateSystemPowerStatus();
                return (System.Windows.Forms.BatteryChargeStatus) this.systemPowerStatus.BatteryFlag;
            }
        }

        public int BatteryFullLifetime
        {
            get
            {
                this.UpdateSystemPowerStatus();
                return this.systemPowerStatus.BatteryFullLifeTime;
            }
        }

        public float BatteryLifePercent
        {
            get
            {
                this.UpdateSystemPowerStatus();
                float num = ((float) this.systemPowerStatus.BatteryLifePercent) / 100f;
                if (num <= 1f)
                {
                    return num;
                }
                return 1f;
            }
        }

        public int BatteryLifeRemaining
        {
            get
            {
                this.UpdateSystemPowerStatus();
                return this.systemPowerStatus.BatteryLifeTime;
            }
        }

        public System.Windows.Forms.PowerLineStatus PowerLineStatus
        {
            get
            {
                this.UpdateSystemPowerStatus();
                return (System.Windows.Forms.PowerLineStatus) this.systemPowerStatus.ACLineStatus;
            }
        }
    }
}

