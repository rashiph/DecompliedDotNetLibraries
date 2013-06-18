namespace System.Web.Management
{
    using System;
    using System.Threading;
    using System.Web.Configuration;

    internal class HealthMonitoringManager
    {
        internal bool _enabled;
        internal HealthMonitoringSectionHelper _sectionHelper = HealthMonitoringSectionHelper.GetHelper();
        private static Timer s_heartbeatTimer = null;
        private static bool s_inited = false;
        private static bool s_initing = false;
        private static object s_lockObject = new object();
        private static HealthMonitoringManager s_manager = null;

        private HealthMonitoringManager()
        {
            this._enabled = this._sectionHelper.Enabled;
            bool flag1 = this._enabled;
        }

        internal static void Dispose()
        {
            try
            {
                if (s_heartbeatTimer != null)
                {
                    s_heartbeatTimer.Dispose();
                    s_heartbeatTimer = null;
                }
            }
            catch
            {
            }
        }

        internal void HeartbeatCallback(object state)
        {
            WebBaseEvent.RaiseSystemEvent(null, 0x3ed);
        }

        internal static HealthMonitoringManager Manager()
        {
            if (s_initing)
            {
                return null;
            }
            if (!s_inited)
            {
                lock (s_lockObject)
                {
                    if (s_inited)
                    {
                        return s_manager;
                    }
                    try
                    {
                        s_initing = true;
                        s_manager = new HealthMonitoringManager();
                    }
                    finally
                    {
                        s_initing = false;
                        s_inited = true;
                    }
                }
            }
            return s_manager;
        }

        internal static void Shutdown()
        {
            WebEventManager.Shutdown();
            Dispose();
        }

        internal static void StartHealthMonitoringHeartbeat()
        {
            HealthMonitoringManager manager = Manager();
            if ((manager != null) && manager._enabled)
            {
                manager.StartHeartbeatTimer();
            }
        }

        internal void StartHeartbeatTimer()
        {
            TimeSpan heartbeatInterval = this._sectionHelper.HealthMonitoringSection.HeartbeatInterval;
            if (heartbeatInterval != TimeSpan.Zero)
            {
                s_heartbeatTimer = new Timer(new TimerCallback(this.HeartbeatCallback), null, TimeSpan.Zero, heartbeatInterval);
            }
        }

        internal static bool Enabled
        {
            get
            {
                HealthMonitoringManager manager = Manager();
                if (manager == null)
                {
                    return false;
                }
                return manager._enabled;
            }
        }

        internal static System.Web.Configuration.HealthMonitoringSectionHelper.ProviderInstances ProviderInstances
        {
            get
            {
                HealthMonitoringManager manager = Manager();
                if (manager == null)
                {
                    return null;
                }
                if (!manager._enabled)
                {
                    return null;
                }
                return manager._sectionHelper._providerInstances;
            }
        }
    }
}

