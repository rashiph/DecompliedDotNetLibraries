namespace System.Web.Security
{
    using System;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Text;
    using System.Web;
    using System.Web.Util;

    [Obsolete("This type is obsolete. The Passport authentication product is no longer supported and has been superseded by Live ID.")]
    public sealed class PassportIdentity : IIdentity, IDisposable
    {
        private bool _Authenticated;
        private IntPtr _iPassport;
        private static int _iPassportVer;
        private string _Name;
        private bool _WWWAuthHeaderSet;

        [SecurityPermission(SecurityAction.Demand, UnmanagedCode=true)]
        public PassportIdentity()
        {
            HttpContext current = HttpContext.Current;
            if (_iPassportVer == 0)
            {
                _iPassportVer = System.Web.UnsafeNativeMethods.PassportVersion();
            }
            if (_iPassportVer < 3)
            {
                string szQueryStrT = current.Request.QueryString["t"];
                string szQueryStrP = current.Request.QueryString["p"];
                HttpCookie cookie = current.Request.Cookies["MSPAuth"];
                HttpCookie cookie2 = current.Request.Cookies["MSPProf"];
                HttpCookie cookie3 = current.Request.Cookies["MSPProfC"];
                string str = ((cookie != null) && (cookie.Value != null)) ? cookie.Value : string.Empty;
                string str4 = ((cookie2 != null) && (cookie2.Value != null)) ? cookie2.Value : string.Empty;
                string str5 = ((cookie3 != null) && (cookie3.Value != null)) ? cookie3.Value : string.Empty;
                StringBuilder szAuthCookieRet = new StringBuilder(0x404);
                StringBuilder szProfCookieRet = new StringBuilder(0x404);
                str = HttpUtility.UrlDecode(str);
                str4 = HttpUtility.UrlDecode(str4);
                str5 = HttpUtility.UrlDecode(str5);
                int errorCode = System.Web.UnsafeNativeMethods.PassportCreate(szQueryStrT, szQueryStrP, str, str4, str5, szAuthCookieRet, szProfCookieRet, 0x400, ref this._iPassport);
                if (this._iPassport == IntPtr.Zero)
                {
                    throw new COMException(System.Web.SR.GetString("Could_not_create_passport_identity"), errorCode);
                }
                string str6 = UrlEncodeCookie(szAuthCookieRet.ToString());
                string str7 = UrlEncodeCookie(szProfCookieRet.ToString());
                if (str6.Length > 1)
                {
                    current.Response.AppendHeader("Set-Cookie", str6);
                }
                if (str7.Length > 1)
                {
                    current.Response.AppendHeader("Set-Cookie", str7);
                }
            }
            else
            {
                string szRequestLine = current.Request.HttpMethod + " " + current.Request.RawUrl + " " + current.Request.ServerVariables["SERVER_PROTOCOL"] + "\r\n";
                StringBuilder szBufOut = new StringBuilder(0xffc);
                int num2 = System.Web.UnsafeNativeMethods.PassportCreateHttpRaw(szRequestLine, current.Request.ServerVariables["ALL_RAW"], current.Request.IsSecureConnection ? 1 : 0, szBufOut, 0xffa, ref this._iPassport);
                if (this._iPassport == IntPtr.Zero)
                {
                    throw new COMException(System.Web.SR.GetString("Could_not_create_passport_identity"), num2);
                }
                string strResponseHeaders = szBufOut.ToString();
                this.SetHeaders(current, strResponseHeaders);
            }
            this._Authenticated = this.GetIsAuthenticated(-1, -1, -1);
            if (!this._Authenticated)
            {
                this._Name = string.Empty;
            }
        }

        public string AuthUrl()
        {
            return this.AuthUrl(null, -1, -1, null, -1, null, -1, -1);
        }

        public string AuthUrl(string strReturnUrl)
        {
            return this.AuthUrl(strReturnUrl, -1, -1, null, -1, null, -1, -1);
        }

        public string AuthUrl(string strReturnUrl, int iTimeWindow, bool fForceLogin, string strCoBrandedArgs, int iLangID, string strNameSpace, int iKPP, bool bUseSecureAuth)
        {
            StringBuilder szAuthVal = new StringBuilder(0xffc);
            int errorCode = System.Web.UnsafeNativeMethods.PassportAuthURL(this._iPassport, strReturnUrl, iTimeWindow, fForceLogin ? 1 : 0, strCoBrandedArgs, iLangID, strNameSpace, iKPP, bUseSecureAuth ? 10 : 0, szAuthVal, 0xffa);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return szAuthVal.ToString();
        }

        public string AuthUrl(string strReturnUrl, int iTimeWindow, int iForceLogin, string strCoBrandedArgs, int iLangID, string strNameSpace, int iKPP, int iUseSecureAuth)
        {
            StringBuilder szAuthVal = new StringBuilder(0xffc);
            int errorCode = System.Web.UnsafeNativeMethods.PassportAuthURL(this._iPassport, strReturnUrl, iTimeWindow, iForceLogin, strCoBrandedArgs, iLangID, strNameSpace, iKPP, iUseSecureAuth, szAuthVal, 0xffa);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return szAuthVal.ToString();
        }

        public string AuthUrl2()
        {
            return this.AuthUrl2(null, -1, -1, null, -1, null, -1, -1);
        }

        public string AuthUrl2(string strReturnUrl)
        {
            return this.AuthUrl2(strReturnUrl, -1, -1, null, -1, null, -1, -1);
        }

        public string AuthUrl2(string strReturnUrl, int iTimeWindow, bool fForceLogin, string strCoBrandedArgs, int iLangID, string strNameSpace, int iKPP, bool bUseSecureAuth)
        {
            StringBuilder szAuthVal = new StringBuilder(0xffc);
            int errorCode = System.Web.UnsafeNativeMethods.PassportAuthURL2(this._iPassport, strReturnUrl, iTimeWindow, fForceLogin ? 1 : 0, strCoBrandedArgs, iLangID, strNameSpace, iKPP, bUseSecureAuth ? 10 : 0, szAuthVal, 0xffa);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return szAuthVal.ToString();
        }

        public string AuthUrl2(string strReturnUrl, int iTimeWindow, int iForceLogin, string strCoBrandedArgs, int iLangID, string strNameSpace, int iKPP, int iUseSecureAuth)
        {
            StringBuilder szAuthVal = new StringBuilder(0xffc);
            int errorCode = System.Web.UnsafeNativeMethods.PassportAuthURL2(this._iPassport, strReturnUrl, iTimeWindow, iForceLogin, strCoBrandedArgs, iLangID, strNameSpace, iKPP, iUseSecureAuth, szAuthVal, 0xffa);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return szAuthVal.ToString();
        }

        private static string CallPassportCryptFunction(int iFunctionID, string strData)
        {
            int errorCode = 0;
            int capacity = ((strData == null) || (strData.Length < 0x200)) ? 0x200 : strData.Length;
            do
            {
                capacity *= 2;
                StringBuilder szDest = new StringBuilder(capacity);
                errorCode = System.Web.UnsafeNativeMethods.PassportCrypt(iFunctionID, strData, szDest, capacity);
                if (errorCode == 0)
                {
                    return szDest.ToString();
                }
                if ((errorCode != -2147024774) && (errorCode < 0))
                {
                    throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
                }
            }
            while ((errorCode == -2147024774) && (capacity < 0xa00000));
            return null;
        }

        public static string Compress(string strData)
        {
            return CallPassportCryptFunction(2, strData);
        }

        public static bool CryptIsValid()
        {
            int errorCode = System.Web.UnsafeNativeMethods.PassportCryptIsValid();
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return (errorCode == 0);
        }

        public static int CryptPutHost(string strHost)
        {
            int errorCode = System.Web.UnsafeNativeMethods.PassportCryptPut(0, strHost);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return errorCode;
        }

        public static int CryptPutSite(string strSite)
        {
            int errorCode = System.Web.UnsafeNativeMethods.PassportCryptPut(1, strSite);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return errorCode;
        }

        public static string Decompress(string strData)
        {
            return CallPassportCryptFunction(3, strData);
        }

        public static string Decrypt(string strData)
        {
            return CallPassportCryptFunction(1, strData);
        }

        public static string Encrypt(string strData)
        {
            return CallPassportCryptFunction(0, strData);
        }

        ~PassportIdentity()
        {
            System.Web.UnsafeNativeMethods.PassportDestroy(this._iPassport);
            this._iPassport = IntPtr.Zero;
        }

        public object GetCurrentConfig(string strAttribute)
        {
            object pReturn = new object();
            int errorCode = System.Web.UnsafeNativeMethods.PassportGetCurrentConfig(this._iPassport, strAttribute, out pReturn);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return pReturn;
        }

        public string GetDomainAttribute(string strAttribute, int iLCID, string strDomain)
        {
            StringBuilder szValue = new StringBuilder(0x404);
            int errorCode = System.Web.UnsafeNativeMethods.PassportGetDomainAttribute(this._iPassport, strAttribute, iLCID, strDomain, szValue, 0x400);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return szValue.ToString();
        }

        public string GetDomainFromMemberName(string strMemberName)
        {
            StringBuilder szMember = new StringBuilder(0x404);
            int errorCode = System.Web.UnsafeNativeMethods.PassportDomainFromMemberName(this._iPassport, strMemberName, szMember, 0x400);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return szMember.ToString();
        }

        public bool GetIsAuthenticated(int iTimeWindow, bool bForceLogin, bool bCheckSecure)
        {
            return this.GetIsAuthenticated(iTimeWindow, bForceLogin ? 1 : 0, bCheckSecure ? 10 : 0);
        }

        public bool GetIsAuthenticated(int iTimeWindow, int iForceLogin, int iCheckSecure)
        {
            int errorCode = System.Web.UnsafeNativeMethods.PassportIsAuthenticated(this._iPassport, iTimeWindow, iForceLogin, iCheckSecure);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return (errorCode == 0);
        }

        public string GetLoginChallenge()
        {
            return this.GetLoginChallenge(null, -1, -1, null, -1, null, -1, -1, null);
        }

        public string GetLoginChallenge(string strReturnUrl)
        {
            return this.GetLoginChallenge(strReturnUrl, -1, -1, null, -1, null, -1, -1, null);
        }

        public string GetLoginChallenge(string szRetURL, int iTimeWindow, int fForceLogin, string szCOBrandArgs, int iLangID, string strNameSpace, int iKPP, int iUseSecureAuth, object oExtraParams)
        {
            StringBuilder szOut = new StringBuilder(0xffc);
            int errorCode = System.Web.UnsafeNativeMethods.PassportGetLoginChallenge(this._iPassport, szRetURL, iTimeWindow, fForceLogin, szCOBrandArgs, iLangID, strNameSpace, iKPP, iUseSecureAuth, oExtraParams, szOut, 0xffa);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            string str = szOut.ToString();
            if ((str != null) && !StringUtil.StringStartsWith(str, "WWW-Authenticate"))
            {
                str = "WWW-Authenticate: " + str;
            }
            return str;
        }

        public object GetOption(string strOpt)
        {
            object vOut = new object();
            int errorCode = System.Web.UnsafeNativeMethods.PassportGetOption(this._iPassport, strOpt, out vOut);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return vOut;
        }

        public object GetProfileObject(string strProfileName)
        {
            object rOut = new object();
            int errorCode = System.Web.UnsafeNativeMethods.PassportGetProfile(this._iPassport, strProfileName, out rOut);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return rOut;
        }

        public bool HasFlag(int iFlagMask)
        {
            int errorCode = System.Web.UnsafeNativeMethods.PassportHasFlag(this._iPassport, iFlagMask);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return (errorCode == 0);
        }

        public bool HasProfile(string strProfile)
        {
            int errorCode = System.Web.UnsafeNativeMethods.PassportHasProfile(this._iPassport, strProfile);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return (errorCode == 0);
        }

        public bool HaveConsent(bool bNeedFullConsent, bool bNeedBirthdate)
        {
            int errorCode = System.Web.UnsafeNativeMethods.PassportHasConsent(this._iPassport, bNeedFullConsent ? 1 : 0, bNeedBirthdate ? 1 : 0);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return (errorCode == 0);
        }

        public int LoginUser()
        {
            return this.LoginUser(null, -1, -1, null, -1, null, -1, -1, null);
        }

        public int LoginUser(string strReturnUrl)
        {
            return this.LoginUser(strReturnUrl, -1, -1, null, -1, null, -1, -1, null);
        }

        public int LoginUser(string szRetURL, int iTimeWindow, bool fForceLogin, string szCOBrandArgs, int iLangID, string strNameSpace, int iKPP, bool fUseSecureAuth, object oExtraParams)
        {
            return this.LoginUser(szRetURL, iTimeWindow, fForceLogin ? 1 : 0, szCOBrandArgs, iLangID, strNameSpace, iKPP, fUseSecureAuth ? 10 : 0, oExtraParams);
        }

        public int LoginUser(string szRetURL, int iTimeWindow, int fForceLogin, string szCOBrandArgs, int iLangID, string strNameSpace, int iKPP, int iUseSecureAuth, object oExtraParams)
        {
            string strResponseHeaders = this.GetLoginChallenge(szRetURL, iTimeWindow, fForceLogin, szCOBrandArgs, iLangID, strNameSpace, iKPP, iUseSecureAuth, oExtraParams);
            if ((strResponseHeaders != null) && (strResponseHeaders.Length >= 1))
            {
                HttpContext current = HttpContext.Current;
                this.SetHeaders(current, strResponseHeaders);
                this._WWWAuthHeaderSet = true;
                strResponseHeaders = current.Request.Headers["Accept-Auth"];
                if (((strResponseHeaders != null) && (strResponseHeaders.Length > 0)) && (strResponseHeaders.IndexOf("Passport", StringComparison.Ordinal) >= 0))
                {
                    current.Response.StatusCode = 0x191;
                    current.Response.End();
                    return 0;
                }
                strResponseHeaders = this.AuthUrl(szRetURL, iTimeWindow, fForceLogin, szCOBrandArgs, iLangID, strNameSpace, iKPP, iUseSecureAuth);
                if (!string.IsNullOrEmpty(strResponseHeaders))
                {
                    current.Response.Redirect(strResponseHeaders, false);
                    return 0;
                }
            }
            return -1;
        }

        public string LogoTag()
        {
            return this.LogoTag(null, -1, -1, null, -1, -1, null, -1, -1);
        }

        public string LogoTag(string strReturnUrl)
        {
            return this.LogoTag(strReturnUrl, -1, -1, null, -1, -1, null, -1, -1);
        }

        public string LogoTag(string strReturnUrl, int iTimeWindow, bool fForceLogin, string strCoBrandedArgs, int iLangID, bool fSecure, string strNameSpace, int iKPP, bool bUseSecureAuth)
        {
            return this.LogoTag(strReturnUrl, iTimeWindow, fForceLogin ? 1 : 0, strCoBrandedArgs, iLangID, fSecure ? 1 : 0, strNameSpace, iKPP, bUseSecureAuth ? 10 : 0);
        }

        public string LogoTag(string strReturnUrl, int iTimeWindow, int iForceLogin, string strCoBrandedArgs, int iLangID, int iSecure, string strNameSpace, int iKPP, int iUseSecureAuth)
        {
            StringBuilder szValue = new StringBuilder(0xffc);
            int errorCode = System.Web.UnsafeNativeMethods.PassportLogoTag(this._iPassport, strReturnUrl, iTimeWindow, iForceLogin, strCoBrandedArgs, iLangID, iSecure, strNameSpace, iKPP, iUseSecureAuth, szValue, 0xffa);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return szValue.ToString();
        }

        public string LogoTag2()
        {
            return this.LogoTag2(null, -1, -1, null, -1, -1, null, -1, -1);
        }

        public string LogoTag2(string strReturnUrl)
        {
            return this.LogoTag2(strReturnUrl, -1, -1, null, -1, -1, null, -1, -1);
        }

        public string LogoTag2(string strReturnUrl, int iTimeWindow, bool fForceLogin, string strCoBrandedArgs, int iLangID, bool fSecure, string strNameSpace, int iKPP, bool bUseSecureAuth)
        {
            return this.LogoTag2(strReturnUrl, iTimeWindow, fForceLogin ? 1 : 0, strCoBrandedArgs, iLangID, fSecure ? 1 : 0, strNameSpace, iKPP, bUseSecureAuth ? 10 : 0);
        }

        public string LogoTag2(string strReturnUrl, int iTimeWindow, int iForceLogin, string strCoBrandedArgs, int iLangID, int iSecure, string strNameSpace, int iKPP, int iUseSecureAuth)
        {
            StringBuilder szValue = new StringBuilder(0xffc);
            int errorCode = System.Web.UnsafeNativeMethods.PassportLogoTag2(this._iPassport, strReturnUrl, iTimeWindow, iForceLogin, strCoBrandedArgs, iLangID, iSecure, strNameSpace, iKPP, iUseSecureAuth, szValue, 0xffa);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return szValue.ToString();
        }

        public string LogoutURL()
        {
            return this.LogoutURL(null, null, -1, null, -1);
        }

        public string LogoutURL(string szReturnURL, string szCOBrandArgs, int iLangID, string strDomain, int iUseSecureAuth)
        {
            StringBuilder szAuthVal = new StringBuilder(0x1000);
            int errorCode = System.Web.UnsafeNativeMethods.PassportLogoutURL(this._iPassport, szReturnURL, szCOBrandArgs, iLangID, strDomain, iUseSecureAuth, szAuthVal, 0x1000);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return szAuthVal.ToString();
        }

        private void SetHeaders(HttpContext context, string strResponseHeaders)
        {
            int index;
            for (int i = 0; i < strResponseHeaders.Length; i = index + 2)
            {
                index = strResponseHeaders.IndexOf('\r', i);
                if (index < 0)
                {
                    index = strResponseHeaders.Length;
                }
                string str = strResponseHeaders.Substring(i, index - i);
                int length = str.IndexOf(':');
                if (length > 0)
                {
                    string name = str.Substring(0, length);
                    string str3 = str.Substring(length + 1);
                    context.Response.AppendHeader(name, str3);
                }
            }
        }

        public void SetOption(string strOpt, object vOpt)
        {
            int errorCode = System.Web.UnsafeNativeMethods.PassportSetOption(this._iPassport, strOpt, vOpt);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
        }

        public static void SignOut(string strSignOutDotGifFileName)
        {
            HttpContext current = HttpContext.Current;
            string[] strArray = new string[] { "MSPAuth", "MSPProf", "MSPConsent", "MSPSecAuth", "MSPProfC" };
            string[] strArray2 = new string[] { "TicketDomain", "TicketDomain", "ProfileDomain", "SecureDomain", "TicketDomain" };
            string[] strArray3 = new string[] { "TicketPath", "TicketPath", "ProfilePath", "SecurePath", "TicketPath" };
            string[] strArray4 = new string[5];
            string[] strArray5 = new string[5];
            PassportIdentity identity = null;
            int index = 0;
            current.Response.ClearHeaders();
            try
            {
                if (current.User.Identity is PassportIdentity)
                {
                    identity = (PassportIdentity) current.User.Identity;
                }
                else
                {
                    identity = new PassportIdentity();
                }
                if ((identity != null) && (_iPassportVer >= 3))
                {
                    for (index = 0; index < 5; index++)
                    {
                        object currentConfig = identity.GetCurrentConfig(strArray2[index]);
                        if ((currentConfig != null) && (currentConfig is string))
                        {
                            strArray4[index] = (string) currentConfig;
                        }
                    }
                    for (index = 0; index < 5; index++)
                    {
                        object obj3 = identity.GetCurrentConfig(strArray3[index]);
                        if ((obj3 != null) && (obj3 is string))
                        {
                            strArray5[index] = (string) obj3;
                        }
                    }
                }
            }
            catch
            {
            }
            for (index = 0; index < 5; index++)
            {
                HttpCookie cookie = new HttpCookie(strArray[index], string.Empty) {
                    Expires = new DateTime(0x7ce, 1, 1)
                };
                if ((strArray4[index] != null) && (strArray4[index].Length > 0))
                {
                    cookie.Domain = strArray4[index];
                }
                if ((strArray5[index] != null) && (strArray5[index].Length > 0))
                {
                    cookie.Path = strArray5[index];
                }
                else
                {
                    cookie.Path = "/";
                }
                current.Response.Cookies.Add(cookie);
            }
            current.Response.Expires = -1;
            current.Response.Cache.SetCacheability(HttpCacheability.NoCache);
            current.Response.AppendHeader("Pragma", "no-cache");
            current.Response.ContentType = "image/gif";
            current.Response.WriteFile(strSignOutDotGifFileName);
            string url = current.Request.QueryString["ru"];
            if ((url != null) && (url.Length > 1))
            {
                current.Response.Redirect(url, false);
            }
        }

        void IDisposable.Dispose()
        {
            if (this._iPassport != IntPtr.Zero)
            {
                System.Web.UnsafeNativeMethods.PassportDestroy(this._iPassport);
            }
            this._iPassport = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        public object Ticket(string strAttribute)
        {
            object pReturn = new object();
            int errorCode = System.Web.UnsafeNativeMethods.PassportTicket(this._iPassport, strAttribute, out pReturn);
            if (errorCode < 0)
            {
                throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
            }
            return pReturn;
        }

        private static string UrlEncodeCookie(string strIn)
        {
            if ((strIn == null) || (strIn.Length < 1))
            {
                return string.Empty;
            }
            int index = strIn.IndexOf('=');
            if (index < 0)
            {
                return HttpUtility.AspCompatUrlEncode(strIn);
            }
            index++;
            int startIndex = strIn.IndexOf(';', index);
            if (startIndex < 0)
            {
                return HttpUtility.AspCompatUrlEncode(strIn);
            }
            string str = strIn.Substring(0, index);
            string s = strIn.Substring(index, startIndex - index);
            string str3 = strIn.Substring(startIndex, strIn.Length - startIndex);
            return (str + HttpUtility.AspCompatUrlEncode(s) + str3);
        }

        public string AuthenticationType
        {
            get
            {
                return "Passport";
            }
        }

        public int Error
        {
            get
            {
                return System.Web.UnsafeNativeMethods.PassportGetError(this._iPassport);
            }
        }

        public bool GetFromNetworkServer
        {
            get
            {
                int errorCode = System.Web.UnsafeNativeMethods.PassportGetFromNetworkServer(this._iPassport);
                if (errorCode < 0)
                {
                    throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
                }
                return (errorCode == 0);
            }
        }

        public bool HasSavedPassword
        {
            get
            {
                int errorCode = System.Web.UnsafeNativeMethods.PassportGetHasSavedPassword(this._iPassport);
                if (errorCode < 0)
                {
                    throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
                }
                return (errorCode == 0);
            }
        }

        public bool HasTicket
        {
            get
            {
                int errorCode = System.Web.UnsafeNativeMethods.PassportHasTicket(this._iPassport);
                if (errorCode < 0)
                {
                    throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
                }
                return (errorCode == 0);
            }
        }

        public string HexPUID
        {
            get
            {
                StringBuilder szOut = new StringBuilder(0x400);
                int errorCode = System.Web.UnsafeNativeMethods.PassportHexPUID(this._iPassport, szOut, 0x400);
                if (errorCode < 0)
                {
                    throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
                }
                return szOut.ToString();
            }
        }

        public bool IsAuthenticated
        {
            get
            {
                return this._Authenticated;
            }
        }

        public string this[string strProfileName]
        {
            get
            {
                object profileObject = this.GetProfileObject(strProfileName);
                if (profileObject == null)
                {
                    return string.Empty;
                }
                if (profileObject is string)
                {
                    return (string) profileObject;
                }
                return profileObject.ToString();
            }
        }

        public string Name
        {
            get
            {
                if (this._Name == null)
                {
                    if (_iPassportVer >= 3)
                    {
                        this._Name = this.HexPUID;
                    }
                    else if (this.HasProfile("core"))
                    {
                        this._Name = int.Parse(this["MemberIDHigh"], CultureInfo.InvariantCulture).ToString("X8", CultureInfo.InvariantCulture) + int.Parse(this["MemberIDLow"], CultureInfo.InvariantCulture).ToString("X8", CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        this._Name = string.Empty;
                    }
                }
                return this._Name;
            }
        }

        public int TicketAge
        {
            get
            {
                int errorCode = System.Web.UnsafeNativeMethods.PassportGetTicketAge(this._iPassport);
                if (errorCode < 0)
                {
                    throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
                }
                return errorCode;
            }
        }

        public int TimeSinceSignIn
        {
            get
            {
                int errorCode = System.Web.UnsafeNativeMethods.PassportGetTimeSinceSignIn(this._iPassport);
                if (errorCode < 0)
                {
                    throw new COMException(System.Web.SR.GetString("Passport_method_failed"), errorCode);
                }
                return errorCode;
            }
        }

        internal bool WWWAuthHeaderSet
        {
            get
            {
                return this._WWWAuthHeaderSet;
            }
        }
    }
}

