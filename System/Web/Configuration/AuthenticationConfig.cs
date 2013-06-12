namespace System.Web.Configuration
{
    using System;
    using System.Web;
    using System.Web.Util;

    internal static class AuthenticationConfig
    {
        private static AuthenticationMode? s_explicitMode;

        internal static bool AccessingLoginPage(HttpContext context, string loginUrl)
        {
            if (!string.IsNullOrEmpty(loginUrl))
            {
                loginUrl = GetCompleteLoginUrl(context, loginUrl);
                if (string.IsNullOrEmpty(loginUrl))
                {
                    return false;
                }
                int index = loginUrl.IndexOf('?');
                if (index >= 0)
                {
                    loginUrl = loginUrl.Substring(0, index);
                }
                string path = context.Request.Path;
                if (StringUtil.EqualsIgnoreCase(path, loginUrl))
                {
                    return true;
                }
                if (loginUrl.IndexOf('%') >= 0)
                {
                    string str2 = HttpUtility.UrlDecode(loginUrl);
                    if (StringUtil.EqualsIgnoreCase(path, str2))
                    {
                        return true;
                    }
                    str2 = HttpUtility.UrlDecode(loginUrl, context.Request.ContentEncoding);
                    if (StringUtil.EqualsIgnoreCase(path, str2))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        internal static string GetCompleteLoginUrl(HttpContext context, string loginUrl)
        {
            if (string.IsNullOrEmpty(loginUrl))
            {
                return string.Empty;
            }
            if (UrlPath.IsRelativeUrl(loginUrl))
            {
                loginUrl = UrlPath.Combine(HttpRuntime.AppDomainAppVirtualPathString, loginUrl);
            }
            return loginUrl;
        }

        internal static AuthenticationMode Mode
        {
            get
            {
                if (s_explicitMode.HasValue)
                {
                    return s_explicitMode.Value;
                }
                AuthenticationSection authentication = RuntimeConfig.GetAppConfig().Authentication;
                authentication.ValidateAuthenticationMode();
                return authentication.Mode;
            }
            set
            {
                s_explicitMode = new AuthenticationMode?(value);
            }
        }
    }
}

