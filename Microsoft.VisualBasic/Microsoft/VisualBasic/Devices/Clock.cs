namespace Microsoft.VisualBasic.Devices
{
    using System;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class Clock
    {
        public DateTime GmtTime
        {
            get
            {
                return DateTime.UtcNow;
            }
        }

        public DateTime LocalTime
        {
            get
            {
                return DateTime.Now;
            }
        }

        public int TickCount
        {
            get
            {
                return Environment.TickCount;
            }
        }
    }
}

