namespace Microsoft.Transactions.Wsat.StateMachines
{
    using System;
    using System.Runtime;

    internal class TimerInstance
    {
        public static readonly TimerInstance Committing = new TimerInstance(TimerProfile.Committing);
        public static readonly TimerInstance Prepared = new TimerInstance(TimerProfile.Prepared);
        public static readonly TimerInstance Preparing = new TimerInstance(TimerProfile.Preparing);
        private TimerProfile profile;
        public static readonly TimerInstance Replaying = new TimerInstance(TimerProfile.Replaying);
        public static readonly TimerInstance VolatileOutcomeAssurance = new TimerInstance(TimerProfile.VolatileOutcomeAssurance);

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public TimerInstance(TimerProfile profile)
        {
            this.profile = profile;
        }

        public TimerProfile Profile
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.profile;
            }
        }
    }
}

