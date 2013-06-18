namespace System.ServiceModel.Administration
{
    using System;
    using System.Diagnostics;

    internal sealed class AppDomainInfo
    {
        private string friendlyName;
        private int id;
        private Guid instanceId = Guid.NewGuid();
        private bool isDefaultAppDomain;
        private string machineName;
        private int processId;
        private string processName;
        private static AppDomainInfo singleton;
        private static object syncRoot = new object();

        private AppDomainInfo(AppDomain appDomain)
        {
            this.friendlyName = appDomain.FriendlyName;
            this.isDefaultAppDomain = appDomain.IsDefaultAppDomain();
            Process currentProcess = Process.GetCurrentProcess();
            this.processName = currentProcess.ProcessName;
            this.machineName = Environment.MachineName;
            this.processId = currentProcess.Id;
            this.id = appDomain.Id;
        }

        internal static AppDomainInfo Current
        {
            get
            {
                if (singleton == null)
                {
                    lock (syncRoot)
                    {
                        if (singleton == null)
                        {
                            singleton = new AppDomainInfo(AppDomain.CurrentDomain);
                        }
                    }
                }
                return singleton;
            }
        }

        public int Id
        {
            get
            {
                return this.id;
            }
        }

        public Guid InstanceId
        {
            get
            {
                return this.instanceId;
            }
        }

        public bool IsDefaultAppDomain
        {
            get
            {
                return this.isDefaultAppDomain;
            }
        }

        public string MachineName
        {
            get
            {
                return this.machineName;
            }
        }

        public string Name
        {
            get
            {
                return this.friendlyName;
            }
        }

        public int ProcessId
        {
            get
            {
                return this.processId;
            }
        }

        public string ProcessName
        {
            get
            {
                return this.processName;
            }
        }
    }
}

