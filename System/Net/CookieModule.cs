namespace System.Net
{
    using System;

    internal static class CookieModule
    {
        internal static void OnReceivedHeaders(HttpWebRequest httpWebRequest)
        {
            try
            {
                if (httpWebRequest.CookieContainer != null)
                {
                    HttpWebResponse response = httpWebRequest._HttpResponse;
                    if (response != null)
                    {
                        CookieCollection cookies = null;
                        try
                        {
                            string setCookie = response.Headers.SetCookie;
                            if ((setCookie != null) && (setCookie.Length > 0))
                            {
                                cookies = httpWebRequest.CookieContainer.CookieCutter(response.ResponseUri, "Set-Cookie", setCookie, false);
                            }
                        }
                        catch
                        {
                        }
                        try
                        {
                            string setCookieHeader = response.Headers.SetCookie2;
                            if ((setCookieHeader != null) && (setCookieHeader.Length > 0))
                            {
                                CookieCollection cookies2 = httpWebRequest.CookieContainer.CookieCutter(response.ResponseUri, "Set-Cookie2", setCookieHeader, false);
                                if ((cookies != null) && (cookies.Count != 0))
                                {
                                    cookies.Add(cookies2);
                                }
                                else
                                {
                                    cookies = cookies2;
                                }
                            }
                        }
                        catch
                        {
                        }
                        if (cookies != null)
                        {
                            response.Cookies = cookies;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        internal static void OnSendingHeaders(HttpWebRequest httpWebRequest)
        {
            try
            {
                if (httpWebRequest.CookieContainer != null)
                {
                    string str;
                    httpWebRequest.Headers.RemoveInternal("Cookie");
                    string cookieHeader = httpWebRequest.CookieContainer.GetCookieHeader(httpWebRequest.GetRemoteResourceUri(), out str);
                    if (cookieHeader.Length > 0)
                    {
                        httpWebRequest.Headers["Cookie"] = cookieHeader;
                    }
                }
            }
            catch
            {
            }
        }
    }
}

