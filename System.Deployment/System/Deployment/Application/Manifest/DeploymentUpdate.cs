namespace System.Deployment.Application.Manifest
{
    using System;
    using System.Deployment.Internal.Isolation.Manifest;
    using System.Runtime.InteropServices;

    internal class DeploymentUpdate
    {
        private readonly bool _beforeApplicationStartup;
        private readonly TimeSpan _maximumAgeAllowed;
        private readonly uint _maximumAgeCount;
        private readonly bool _maximumAgeSpecified;
        private readonly timeUnitType _maximumAgeUnit;

        public DeploymentUpdate(System.Deployment.Internal.Isolation.Manifest.DeploymentMetadataEntry entry)
        {
            this._beforeApplicationStartup = (entry.DeploymentFlags & 4) != 0;
            this._maximumAgeAllowed = GetTimeSpanFromItem(entry.MaximumAge, entry.MaximumAge_Unit, out this._maximumAgeCount, out this._maximumAgeUnit, out this._maximumAgeSpecified);
        }

        private static TimeSpan GetTimeSpanFromItem(ushort time, byte elapsedunit, out uint count, out timeUnitType unit, out bool specified)
        {
            TimeSpan zero;
            specified = true;
            switch (elapsedunit)
            {
                case 1:
                    zero = TimeSpan.FromHours((double) time);
                    count = time;
                    unit = timeUnitType.hours;
                    return zero;

                case 2:
                    zero = TimeSpan.FromDays((double) time);
                    count = time;
                    unit = timeUnitType.days;
                    return zero;

                case 3:
                    zero = TimeSpan.FromDays((double) (time * 7));
                    count = time;
                    unit = timeUnitType.weeks;
                    return zero;
            }
            specified = false;
            zero = TimeSpan.Zero;
            count = 0;
            unit = timeUnitType.days;
            return zero;
        }

        public bool BeforeApplicationStartup
        {
            get
            {
                return this._beforeApplicationStartup;
            }
        }

        public TimeSpan MaximumAgeAllowed
        {
            get
            {
                return this._maximumAgeAllowed;
            }
        }

        public bool MaximumAgeSpecified
        {
            get
            {
                return this._maximumAgeSpecified;
            }
        }
    }
}

