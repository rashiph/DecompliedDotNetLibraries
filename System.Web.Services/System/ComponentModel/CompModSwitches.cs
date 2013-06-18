namespace System.ComponentModel
{
    using System;
    using System.Diagnostics;

    internal sealed class CompModSwitches
    {
        private static BooleanSwitch disableRemoteDebugging;
        private static TraceSwitch dynamicDiscoSearcher;
        private static BooleanSwitch dynamicDiscoVirtualSearch;
        private static TraceSwitch remote;

        public static BooleanSwitch DisableRemoteDebugging
        {
            get
            {
                if (disableRemoteDebugging == null)
                {
                    disableRemoteDebugging = new BooleanSwitch("Remote.Disable", "Disable remote debugging for web methods.");
                }
                return disableRemoteDebugging;
            }
        }

        public static TraceSwitch DynamicDiscoverySearcher
        {
            get
            {
                if (dynamicDiscoSearcher == null)
                {
                    dynamicDiscoSearcher = new TraceSwitch("DynamicDiscoverySearcher", "Enable tracing for the DynamicDiscoverySearcher class.");
                }
                return dynamicDiscoSearcher;
            }
        }

        public static BooleanSwitch DynamicDiscoveryVirtualSearch
        {
            get
            {
                if (dynamicDiscoVirtualSearch == null)
                {
                    dynamicDiscoVirtualSearch = new BooleanSwitch("DynamicDiscoveryVirtualSearch", "Force virtual search for DiscoveryRequestHandler class.");
                }
                return dynamicDiscoVirtualSearch;
            }
        }

        public static TraceSwitch Remote
        {
            get
            {
                if (remote == null)
                {
                    remote = new TraceSwitch("Microsoft.WFC.Remote", "Enable tracing for remote method calls.");
                }
                return remote;
            }
        }
    }
}

