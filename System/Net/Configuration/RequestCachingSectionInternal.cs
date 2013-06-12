namespace System.Net.Configuration
{
    using Microsoft.Win32;
    using System;
    using System.Configuration;
    using System.Net;
    using System.Net.Cache;
    using System.Threading;

    internal sealed class RequestCachingSectionInternal
    {
        private static object classSyncObject;
        private RequestCache defaultCache;
        private RequestCachePolicy defaultCachePolicy;
        private RequestCachePolicy defaultFtpCachePolicy;
        private HttpRequestCachePolicy defaultHttpCachePolicy;
        private bool disableAllCaching;
        private FtpRequestCacheValidator ftpRequestCacheValidator;
        private HttpRequestCacheValidator httpRequestCacheValidator;
        private bool isPrivateCache;
        private TimeSpan unspecifiedMaximumAge;

        private RequestCachingSectionInternal()
        {
        }

        internal RequestCachingSectionInternal(RequestCachingSection section)
        {
            if (!section.DisableAllCaching)
            {
                this.defaultCachePolicy = new RequestCachePolicy(section.DefaultPolicyLevel);
                this.isPrivateCache = section.IsPrivateCache;
                this.unspecifiedMaximumAge = section.UnspecifiedMaximumAge;
            }
            else
            {
                this.disableAllCaching = true;
            }
            this.httpRequestCacheValidator = new HttpRequestCacheValidator(false, this.UnspecifiedMaximumAge);
            this.ftpRequestCacheValidator = new FtpRequestCacheValidator(false, this.UnspecifiedMaximumAge);
            this.defaultCache = new WinInetCache(this.IsPrivateCache, true, true);
            if (!section.DisableAllCaching)
            {
                HttpCachePolicyElement defaultHttpCachePolicy = section.DefaultHttpCachePolicy;
                if (defaultHttpCachePolicy.WasReadFromConfig)
                {
                    if (defaultHttpCachePolicy.PolicyLevel == HttpRequestCacheLevel.Default)
                    {
                        HttpCacheAgeControl cacheAgeControl = (defaultHttpCachePolicy.MinimumFresh != TimeSpan.MinValue) ? HttpCacheAgeControl.MaxAgeAndMinFresh : HttpCacheAgeControl.MaxAgeAndMaxStale;
                        this.defaultHttpCachePolicy = new HttpRequestCachePolicy(cacheAgeControl, defaultHttpCachePolicy.MaximumAge, (defaultHttpCachePolicy.MinimumFresh != TimeSpan.MinValue) ? defaultHttpCachePolicy.MinimumFresh : defaultHttpCachePolicy.MaximumStale);
                    }
                    else
                    {
                        this.defaultHttpCachePolicy = new HttpRequestCachePolicy(defaultHttpCachePolicy.PolicyLevel);
                    }
                }
                FtpCachePolicyElement defaultFtpCachePolicy = section.DefaultFtpCachePolicy;
                if (defaultFtpCachePolicy.WasReadFromConfig)
                {
                    this.defaultFtpCachePolicy = new RequestCachePolicy(defaultFtpCachePolicy.PolicyLevel);
                }
            }
        }

        internal static RequestCachingSectionInternal GetSection()
        {
            RequestCachingSectionInternal internal2;
            lock (ClassSyncObject)
            {
                RequestCachingSection section = System.Configuration.PrivilegedConfigurationManager.GetSection(ConfigurationStrings.RequestCachingSectionPath) as RequestCachingSection;
                if (section == null)
                {
                    internal2 = null;
                }
                else
                {
                    try
                    {
                        internal2 = new RequestCachingSectionInternal(section);
                    }
                    catch (Exception exception)
                    {
                        if (NclUtilities.IsFatal(exception))
                        {
                            throw;
                        }
                        throw new ConfigurationErrorsException(System.SR.GetString("net_config_requestcaching"), exception);
                    }
                }
            }
            return internal2;
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

        internal RequestCache DefaultCache
        {
            get
            {
                return this.defaultCache;
            }
        }

        internal RequestCachePolicy DefaultCachePolicy
        {
            get
            {
                return this.defaultCachePolicy;
            }
        }

        internal RequestCachePolicy DefaultFtpCachePolicy
        {
            get
            {
                return this.defaultFtpCachePolicy;
            }
        }

        internal FtpRequestCacheValidator DefaultFtpValidator
        {
            get
            {
                return this.ftpRequestCacheValidator;
            }
        }

        internal HttpRequestCachePolicy DefaultHttpCachePolicy
        {
            get
            {
                return this.defaultHttpCachePolicy;
            }
        }

        internal HttpRequestCacheValidator DefaultHttpValidator
        {
            get
            {
                return this.httpRequestCacheValidator;
            }
        }

        internal bool DisableAllCaching
        {
            get
            {
                return this.disableAllCaching;
            }
        }

        internal bool IsPrivateCache
        {
            get
            {
                return this.isPrivateCache;
            }
        }

        internal TimeSpan UnspecifiedMaximumAge
        {
            get
            {
                return this.unspecifiedMaximumAge;
            }
        }
    }
}

