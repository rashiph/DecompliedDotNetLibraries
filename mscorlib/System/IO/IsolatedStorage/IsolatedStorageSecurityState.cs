namespace System.IO.IsolatedStorage
{
    using System;
    using System.Security;

    [SecurityCritical]
    public class IsolatedStorageSecurityState : SecurityState
    {
        private IsolatedStorageSecurityOptions m_Options;
        private long m_Quota;
        private long m_UsedSize;

        [SecurityCritical]
        private IsolatedStorageSecurityState()
        {
        }

        internal static IsolatedStorageSecurityState CreateStateToIncreaseQuotaForApplication(long newQuota, long usedSize)
        {
            return new IsolatedStorageSecurityState { m_Options = IsolatedStorageSecurityOptions.IncreaseQuotaForApplication, m_Quota = newQuota, m_UsedSize = usedSize };
        }

        [SecurityCritical]
        public override void EnsureState()
        {
            if (!base.IsStateAvailable())
            {
                throw new IsolatedStorageException(Environment.GetResourceString("IsolatedStorage_Operation"));
            }
        }

        public IsolatedStorageSecurityOptions Options
        {
            get
            {
                return this.m_Options;
            }
        }

        public long Quota
        {
            get
            {
                return this.m_Quota;
            }
            set
            {
                this.m_Quota = value;
            }
        }

        public long UsedSize
        {
            get
            {
                return this.m_UsedSize;
            }
        }
    }
}

