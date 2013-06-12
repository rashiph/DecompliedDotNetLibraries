namespace System.Net.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Net;
    using System.Threading;

    internal sealed class WebRequestModulesSectionInternal
    {
        private static object classSyncObject;
        private ArrayList webRequestModules;

        internal WebRequestModulesSectionInternal(WebRequestModulesSection section)
        {
            if (section.WebRequestModules.Count > 0)
            {
                this.webRequestModules = new ArrayList(section.WebRequestModules.Count);
                foreach (WebRequestModuleElement element in section.WebRequestModules)
                {
                    try
                    {
                        this.webRequestModules.Add(new WebRequestPrefixElement(element.Prefix, element.Type));
                    }
                    catch (Exception exception)
                    {
                        if (NclUtilities.IsFatal(exception))
                        {
                            throw;
                        }
                        throw new ConfigurationErrorsException(System.SR.GetString("net_config_webrequestmodules"), exception);
                    }
                }
            }
        }

        internal static WebRequestModulesSectionInternal GetSection()
        {
            lock (ClassSyncObject)
            {
                WebRequestModulesSection section = System.Configuration.PrivilegedConfigurationManager.GetSection(ConfigurationStrings.WebRequestModulesSectionPath) as WebRequestModulesSection;
                if (section == null)
                {
                    return null;
                }
                return new WebRequestModulesSectionInternal(section);
            }
        }

        internal static object ClassSyncObject
        {
            get
            {
                if (classSyncObject == null)
                {
                    object obj2 = new object();
                    Interlocked.CompareExchange(ref classSyncObject, obj2, null);
                }
                return classSyncObject;
            }
        }

        internal ArrayList WebRequestModules
        {
            get
            {
                ArrayList webRequestModules = this.webRequestModules;
                if (webRequestModules == null)
                {
                    webRequestModules = new ArrayList(0);
                }
                return webRequestModules;
            }
        }
    }
}

