namespace System.Web.Security
{
    using System;
    using System.Runtime.InteropServices;
    using System.Web;

    internal sealed class CookielessHelperClass
    {
        private HttpContext _Context;
        private string _Headers;
        private string _OriginalHeaders;
        internal const string COOKIELESS_SESSION_FILTER_HEADER = "AspFilterSessionId";
        private const string s_AutoDetectName = "AspxAutoDetectCookieSupport";
        private const string s_AutoDetectValue = "1";

        internal CookielessHelperClass(HttpContext context)
        {
            this._Context = context;
        }

        internal bool DoesCookieValueExistInOriginal(char identifier)
        {
            int startPos = 0;
            int endPos = 0;
            this.Init();
            return GetValueStartAndEnd(this._OriginalHeaders, identifier, out startPos, out endPos);
        }

        private void GetCookielessValuesFromHeader()
        {
            this._Headers = this._Context.Request.Headers["AspFilterSessionId"];
            this._OriginalHeaders = this._Headers;
            if (!string.IsNullOrEmpty(this._Headers))
            {
                if ((this._Headers.Length == 0x18) && !this._Headers.Contains("("))
                {
                    this._Headers = null;
                }
                else
                {
                    this._Context.Response.SetAppPathModifier("(" + this._Headers + ")");
                }
            }
        }

        internal string GetCookieValue(char identifier)
        {
            int startPos = 0;
            int endPos = 0;
            this.Init();
            if (!GetValueStartAndEnd(this._Headers, identifier, out startPos, out endPos))
            {
                return null;
            }
            return this._Headers.Substring(startPos, endPos - startPos);
        }

        private static bool GetValueStartAndEnd(string headers, char identifier, out int startPos, out int endPos)
        {
            if (string.IsNullOrEmpty(headers))
            {
                startPos = endPos = -1;
                return false;
            }
            string str = new string(new char[] { identifier, '(' });
            startPos = headers.IndexOf(str, StringComparison.Ordinal);
            if (startPos < 0)
            {
                startPos = endPos = -1;
                return false;
            }
            startPos += 2;
            endPos = headers.IndexOf(')', startPos);
            if (endPos < 0)
            {
                startPos = endPos = -1;
                return false;
            }
            return true;
        }

        private void Init()
        {
            if (this._Headers == null)
            {
                if (this._Headers == null)
                {
                    this.GetCookielessValuesFromHeader();
                }
                if (this._Headers == null)
                {
                    this.RemoveCookielessValuesFromPath();
                }
                if (this._Headers == null)
                {
                    this._Headers = string.Empty;
                }
                this._OriginalHeaders = this._Headers;
            }
        }

        private static bool IsValidHeader(string path, int startPos, int endPos)
        {
            if ((endPos - startPos) >= 3)
            {
                while (startPos <= (endPos - 3))
                {
                    if ((path[startPos] < 'A') || (path[startPos] > 'Z'))
                    {
                        return false;
                    }
                    if (path[startPos + 1] != '(')
                    {
                        return false;
                    }
                    startPos += 2;
                    bool flag = false;
                    while (startPos < endPos)
                    {
                        if (path[startPos] == ')')
                        {
                            startPos++;
                            flag = true;
                            break;
                        }
                        if (path[startPos] == '/')
                        {
                            return false;
                        }
                        startPos++;
                    }
                    if (!flag)
                    {
                        return false;
                    }
                }
                if (startPos < endPos)
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        internal void RedirectWithDetection(string redirectPath)
        {
            this.Init();
            if (string.IsNullOrEmpty(redirectPath))
            {
                redirectPath = this._Context.Request.RawUrl;
            }
            if (redirectPath.IndexOf("?", StringComparison.Ordinal) > 0)
            {
                redirectPath = redirectPath + "&AspxAutoDetectCookieSupport=1";
            }
            else
            {
                redirectPath = redirectPath + "?AspxAutoDetectCookieSupport=1";
            }
            this._Context.Response.Cookies.Add(new HttpCookie("AspxAutoDetectCookieSupport", "1"));
            this._Context.Response.Redirect(redirectPath, true);
        }

        internal void RedirectWithDetectionIfRequired(string redirectPath, HttpCookieMode cookieMode)
        {
            this.Init();
            if ((cookieMode == HttpCookieMode.AutoDetect) && (this._Context.Request.Browser.Cookies && this._Context.Request.Browser.SupportsRedirectWithCookie))
            {
                string cookieValue = this.GetCookieValue('X');
                if ((cookieValue == null) || (cookieValue != "1"))
                {
                    string str2 = this._Context.Request.Headers["Cookie"];
                    if (string.IsNullOrEmpty(str2))
                    {
                        string str3 = this._Context.Request.QueryString["AspxAutoDetectCookieSupport"];
                        if ((str3 != null) && (str3 == "1"))
                        {
                            this.SetCookieValue('X', "1");
                        }
                        else
                        {
                            this.RedirectWithDetection(redirectPath);
                        }
                    }
                }
            }
        }

        internal void RemoveCookielessValuesFromPath()
        {
            string virtualPathString = this._Context.Request.ClientFilePath.VirtualPathString;
            if (virtualPathString.IndexOf('(') != -1)
            {
                int count = virtualPathString.LastIndexOf(")/", StringComparison.Ordinal);
                int length = (count > 2) ? virtualPathString.LastIndexOf("/(", count - 1, count, StringComparison.Ordinal) : -1;
                if (length >= 0)
                {
                    if (this._Headers == null)
                    {
                        this.GetCookielessValuesFromHeader();
                    }
                    if (IsValidHeader(virtualPathString, length + 2, count))
                    {
                        if (this._Headers == null)
                        {
                            this._Headers = virtualPathString.Substring(length + 2, (count - length) - 2);
                        }
                        virtualPathString = virtualPathString.Substring(0, length) + virtualPathString.Substring(count + 1);
                        this._Context.Request.ClientFilePath = VirtualPath.CreateAbsolute(virtualPathString);
                        string rawUrl = this._Context.Request.RawUrl;
                        int index = rawUrl.IndexOf('?');
                        if (index > -1)
                        {
                            virtualPathString = virtualPathString + rawUrl.Substring(index);
                        }
                        this._Context.Request.RawUrl = virtualPathString;
                        if (!string.IsNullOrEmpty(this._Headers))
                        {
                            this._Context.Request.ValidateCookielessHeaderIfRequiredByConfig(this._Headers);
                            this._Context.Response.SetAppPathModifier("(" + this._Headers + ")");
                            string filePath = this._Context.Request.FilePath;
                            string objB = this._Context.Response.RemoveAppPathModifier(filePath);
                            if (!object.ReferenceEquals(filePath, objB))
                            {
                                this._Context.RewritePath(VirtualPath.CreateAbsolute(objB), this._Context.Request.PathInfoObject, null, false);
                            }
                        }
                    }
                }
            }
        }

        internal void SetCookieValue(char identifier, string cookieValue)
        {
            int startPos = 0;
            int endPos = 0;
            this.Init();
            while (GetValueStartAndEnd(this._Headers, identifier, out startPos, out endPos))
            {
                this._Headers = this._Headers.Substring(0, startPos - 2) + this._Headers.Substring(endPos + 1);
            }
            if (!string.IsNullOrEmpty(cookieValue))
            {
                this._Headers = this._Headers + new string(new char[] { identifier, '(' }) + cookieValue + ")";
            }
            if (this._Headers.Length > 0)
            {
                this._Context.Response.SetAppPathModifier("(" + this._Headers + ")");
            }
            else
            {
                this._Context.Response.SetAppPathModifier(null);
            }
        }

        internal static bool UseCookieless(HttpContext context, bool doRedirect, HttpCookieMode cookieMode)
        {
            switch (cookieMode)
            {
                case HttpCookieMode.UseUri:
                    return true;

                case HttpCookieMode.UseCookies:
                    return false;

                case HttpCookieMode.AutoDetect:
                    if (context == null)
                    {
                        context = HttpContext.Current;
                    }
                    if (context == null)
                    {
                        return false;
                    }
                    if (context.Request.Browser.Cookies && context.Request.Browser.SupportsRedirectWithCookie)
                    {
                        string cookieValue = context.CookielessHelper.GetCookieValue('X');
                        if ((cookieValue != null) && (cookieValue == "1"))
                        {
                            return true;
                        }
                        string str2 = context.Request.Headers["Cookie"];
                        if (string.IsNullOrEmpty(str2))
                        {
                            string str3 = context.Request.QueryString["AspxAutoDetectCookieSupport"];
                            if ((str3 != null) && (str3 == "1"))
                            {
                                context.CookielessHelper.SetCookieValue('X', "1");
                                return true;
                            }
                            if (doRedirect)
                            {
                                context.CookielessHelper.RedirectWithDetection(null);
                            }
                        }
                        return false;
                    }
                    return true;

                case HttpCookieMode.UseDeviceProfile:
                    if (context == null)
                    {
                        context = HttpContext.Current;
                    }
                    if (context == null)
                    {
                        return false;
                    }
                    return (!context.Request.Browser.Cookies || !context.Request.Browser.SupportsRedirectWithCookie);
            }
            return false;
        }
    }
}

