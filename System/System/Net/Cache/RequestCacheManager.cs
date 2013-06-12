namespace System.Net.Cache
{
    using System;
    using System.Net.Configuration;

    internal sealed class RequestCacheManager
    {
        private static readonly RequestCacheBinding s_BypassCacheBinding = new RequestCacheBinding(null, null, new RequestCachePolicy(RequestCacheLevel.BypassCache));
        private static RequestCachingSectionInternal s_CacheConfigSettings;
        private static RequestCacheBinding s_DefaultFtpBinding;
        private static RequestCacheBinding s_DefaultGlobalBinding;
        private static RequestCacheBinding s_DefaultHttpBinding;

        private RequestCacheManager()
        {
        }

        internal static RequestCacheBinding GetBinding(string internedScheme)
        {
            if (internedScheme == null)
            {
                throw new ArgumentNullException("uriScheme");
            }
            if (s_CacheConfigSettings == null)
            {
                LoadConfigSettings();
            }
            if (!s_CacheConfigSettings.DisableAllCaching)
            {
                if (internedScheme.Length == 0)
                {
                    return s_DefaultGlobalBinding;
                }
                if ((internedScheme == Uri.UriSchemeHttp) || (internedScheme == Uri.UriSchemeHttps))
                {
                    return s_DefaultHttpBinding;
                }
                if (internedScheme == Uri.UriSchemeFtp)
                {
                    return s_DefaultFtpBinding;
                }
            }
            return s_BypassCacheBinding;
        }

        private static void LoadConfigSettings()
        {
            lock (s_BypassCacheBinding)
            {
                if (s_CacheConfigSettings == null)
                {
                    RequestCachingSectionInternal section = RequestCachingSectionInternal.GetSection();
                    s_DefaultGlobalBinding = new RequestCacheBinding(section.DefaultCache, section.DefaultHttpValidator, section.DefaultCachePolicy);
                    s_DefaultHttpBinding = new RequestCacheBinding(section.DefaultCache, section.DefaultHttpValidator, section.DefaultHttpCachePolicy);
                    s_DefaultFtpBinding = new RequestCacheBinding(section.DefaultCache, section.DefaultFtpValidator, section.DefaultFtpCachePolicy);
                    s_CacheConfigSettings = section;
                }
            }
        }

        internal static void SetBinding(string uriScheme, RequestCacheBinding binding)
        {
            if (uriScheme == null)
            {
                throw new ArgumentNullException("uriScheme");
            }
            if (s_CacheConfigSettings == null)
            {
                LoadConfigSettings();
            }
            if (!s_CacheConfigSettings.DisableAllCaching)
            {
                if (uriScheme.Length == 0)
                {
                    s_DefaultGlobalBinding = binding;
                }
                else if ((uriScheme == Uri.UriSchemeHttp) || (uriScheme == Uri.UriSchemeHttps))
                {
                    s_DefaultHttpBinding = binding;
                }
                else if (uriScheme == Uri.UriSchemeFtp)
                {
                    s_DefaultFtpBinding = binding;
                }
            }
        }

        internal static bool IsCachingEnabled
        {
            get
            {
                if (s_CacheConfigSettings == null)
                {
                    LoadConfigSettings();
                }
                return !s_CacheConfigSettings.DisableAllCaching;
            }
        }
    }
}

