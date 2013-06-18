namespace System.ServiceModel.Channels
{
    using System;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel;
    using System.Web;
    using System.Web.Hosting;

    internal class MsmqHostedTransportManager : TransportManager
    {
        private MsmqUri.IAddressTranslator addressing;
        private List<MsmqBindingMonitor> bindingMonitors;
        private HostedBindingFilter filter;
        private string[] hosts;
        private Action messageReceivedCallback;

        public MsmqHostedTransportManager(string[] hosts, MsmqUri.IAddressTranslator addressing)
        {
            this.hosts = hosts;
            this.bindingMonitors = new List<MsmqBindingMonitor>();
            this.addressing = addressing;
            this.filter = new HostedBindingFilter(HostingEnvironment.ApplicationVirtualPath, addressing);
            foreach (string str in this.hosts)
            {
                MsmqBindingMonitor item = new MsmqBindingMonitor(str);
                item.AddFilter(this.filter);
                this.bindingMonitors.Add(item);
            }
            foreach (MsmqBindingMonitor monitor2 in this.bindingMonitors)
            {
                monitor2.Open();
            }
        }

        public Uri[] GetBaseAddresses(string virtualPath)
        {
            foreach (MsmqBindingMonitor monitor in this.bindingMonitors)
            {
                monitor.WaitForFirstRoundComplete();
            }
            string str = VirtualPathUtility.ToAbsolute(virtualPath, HostingEnvironment.ApplicationVirtualPath);
            List<Uri> list = new List<Uri>(this.hosts.Length);
            string processedVirtualPath = str.Substring(1);
            foreach (string str3 in this.hosts)
            {
                bool isPrivate = this.filter.IsPrivateMatch(processedVirtualPath);
                Uri item = this.addressing.CreateUri(str3, processedVirtualPath, isPrivate);
                list.Add(item);
                MsmqDiagnostics.FoundBaseAddress(item, str);
            }
            return list.ToArray();
        }

        internal override void OnClose(TimeSpan timeout)
        {
        }

        private void OnMessageReceived()
        {
            Action messageReceivedCallback = this.messageReceivedCallback;
            if (messageReceivedCallback != null)
            {
                messageReceivedCallback();
            }
        }

        internal override void OnOpen()
        {
        }

        internal override void Register(TransportChannelListener channelListener)
        {
            channelListener.SetMessageReceivedCallback(new Action(this.OnMessageReceived));
        }

        internal void Start(Action messageReceivedCallback)
        {
            this.messageReceivedCallback = messageReceivedCallback;
        }

        internal override void Unregister(TransportChannelListener channelListener)
        {
        }

        internal override string Scheme
        {
            get
            {
                return this.addressing.Scheme;
            }
        }

        private class HostedBindingFilter : MsmqBindingFilter
        {
            private Dictionary<string, string> privateMatches;

            public HostedBindingFilter(string path, MsmqUri.IAddressTranslator addressing) : base(path, addressing)
            {
                this.privateMatches = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            private bool CheckServiceExists(string serviceFile)
            {
                try
                {
                    return (ServiceHostingEnvironment.IsConfigurationBasedService(serviceFile) || HostingEnvironmentWrapper.ServiceFileExists(serviceFile));
                }
                catch (ArgumentException exception)
                {
                    MsmqDiagnostics.ExpectedException(exception);
                    return false;
                }
            }

            private string CreateBaseQueue(string serviceFile)
            {
                if (serviceFile.StartsWith("~", StringComparison.OrdinalIgnoreCase))
                {
                    serviceFile = serviceFile.Substring(1);
                }
                if (serviceFile.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    serviceFile = serviceFile.Substring(1);
                }
                string applicationVirtualPath = HostingEnvironment.ApplicationVirtualPath;
                if (applicationVirtualPath.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    applicationVirtualPath = applicationVirtualPath.Substring(0, applicationVirtualPath.Length - 1);
                }
                if (applicationVirtualPath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    applicationVirtualPath = applicationVirtualPath.Substring(1);
                }
                return (applicationVirtualPath + "/" + serviceFile);
            }

            private string CreateRelativeVirtualPath(string host, string name, bool isPrivate)
            {
                return ("~/" + name.Substring(base.CanonicalPrefix.Length));
            }

            public bool IsPrivateMatch(string processedVirtualPath)
            {
                lock (this)
                {
                    return this.privateMatches.ContainsKey(processedVirtualPath);
                }
            }

            public override object MatchFound(string host, string name, bool isPrivate)
            {
                string virtualPath = this.CreateRelativeVirtualPath(host, name, isPrivate);
                string serviceFile = ServiceHostingEnvironment.NormalizeVirtualPath(virtualPath);
                lock (this)
                {
                    if (isPrivate)
                    {
                        string str3 = this.CreateBaseQueue(serviceFile);
                        this.privateMatches[str3] = str3;
                    }
                }
                if (this.CheckServiceExists(serviceFile))
                {
                    MsmqDiagnostics.StartingService(host, name, isPrivate, virtualPath);
                    ActionItem.Schedule(new Action<object>(this.StartService), virtualPath);
                }
                return null;
            }

            public override void MatchLost(string host, string name, bool isPrivate, object callbackState)
            {
            }

            private void StartService(object state)
            {
                try
                {
                    string virtualPath = (string) state;
                    ServiceHostingEnvironment.EnsureServiceAvailable(virtualPath);
                }
                catch (ServiceActivationException exception)
                {
                    MsmqDiagnostics.ExpectedException(exception);
                }
                catch (EndpointNotFoundException exception2)
                {
                    MsmqDiagnostics.ExpectedException(exception2);
                }
            }
        }
    }
}

