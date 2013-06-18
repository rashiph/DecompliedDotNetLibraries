namespace System.Web.Management
{
    using System;
    using System.Web;

    public class WebApplicationLifetimeEvent : WebManagementEvent
    {
        internal WebApplicationLifetimeEvent()
        {
        }

        protected internal WebApplicationLifetimeEvent(string message, object eventSource, int eventCode) : base(message, eventSource, eventCode)
        {
        }

        protected internal WebApplicationLifetimeEvent(string message, object eventSource, int eventCode, int eventDetailCode) : base(message, eventSource, eventCode, eventDetailCode)
        {
        }

        internal static int DetailCodeFromShutdownReason(ApplicationShutdownReason reason)
        {
            switch (reason)
            {
                case ApplicationShutdownReason.HostingEnvironment:
                    return 0xc352;

                case ApplicationShutdownReason.ChangeInGlobalAsax:
                    return 0xc353;

                case ApplicationShutdownReason.ConfigurationChange:
                    return 0xc354;

                case ApplicationShutdownReason.UnloadAppDomainCalled:
                    return 0xc355;

                case ApplicationShutdownReason.ChangeInSecurityPolicyFile:
                    return 0xc356;

                case ApplicationShutdownReason.BinDirChangeOrDirectoryRename:
                    return 0xc357;

                case ApplicationShutdownReason.BrowsersDirChangeOrDirectoryRename:
                    return 0xc358;

                case ApplicationShutdownReason.CodeDirChangeOrDirectoryRename:
                    return 0xc359;

                case ApplicationShutdownReason.ResourcesDirChangeOrDirectoryRename:
                    return 0xc35a;

                case ApplicationShutdownReason.IdleTimeout:
                    return 0xc35b;

                case ApplicationShutdownReason.PhysicalApplicationPathChanged:
                    return 0xc35c;

                case ApplicationShutdownReason.HttpRuntimeClose:
                    return 0xc35d;

                case ApplicationShutdownReason.InitializationError:
                    return 0xc35e;

                case ApplicationShutdownReason.MaxRecompilationsReached:
                    return 0xc35f;

                case ApplicationShutdownReason.BuildManagerChange:
                    return 0xc361;
            }
            return 0xc351;
        }

        protected internal override void IncrementPerfCounters()
        {
            base.IncrementPerfCounters();
            PerfCounters.IncrementCounter(AppPerfCounter.EVENTS_APP);
        }
    }
}

