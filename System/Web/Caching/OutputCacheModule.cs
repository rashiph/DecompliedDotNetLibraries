namespace System.Web.Caching
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    internal sealed class OutputCacheModule : IHttpModule
    {
        private string _key;
        private bool _recordedCacheMiss;
        private const string ASTERISK = "*";
        private const string ERROR_VARYBY_VALUE = "+e+";
        private const string IDENTITY = "identity";
        private const int MAX_POST_KEY_LENGTH = 0x3a98;
        private const string NULL_VARYBY_VALUE = "+n+";
        private const string OUTPUTCACHE_KEYPREFIX_GET = "a2";
        private const string OUTPUTCACHE_KEYPREFIX_POST = "a1";
        internal static readonly char[] s_fieldSeparators = new char[] { ',', ' ' };
        internal const string TAG_OUTPUTCACHE = "OutputCache";

        internal OutputCacheModule()
        {
        }

        private string CreateOutputCachedItemKey(HttpContext context, CachedVary cachedVary)
        {
            return CreateOutputCachedItemKey(context.Request.Path, context.Request.HttpVerb, context, cachedVary);
        }

        internal static string CreateOutputCachedItemKey(string path, HttpVerb verb, HttpContext context, CachedVary cachedVary)
        {
            StringBuilder builder;
            if (verb == HttpVerb.POST)
            {
                builder = new StringBuilder("a1", path.Length + "a1".Length);
            }
            else
            {
                builder = new StringBuilder("a2", path.Length + "a2".Length);
            }
            builder.Append(CultureInfo.InvariantCulture.TextInfo.ToLower(path));
            if (cachedVary != null)
            {
                string varyByCustomString;
                HttpRequest request = context.Request;
                for (int i = 0; i <= 2; i++)
                {
                    int num;
                    string[] array = null;
                    NameValueCollection serverVarsWithoutDemand = null;
                    bool flag = false;
                    switch (i)
                    {
                        case 0:
                            builder.Append("H");
                            array = cachedVary._headers;
                            if (array != null)
                            {
                                serverVarsWithoutDemand = request.GetServerVarsWithoutDemand();
                            }
                            break;

                        case 1:
                            builder.Append("Q");
                            array = cachedVary._params;
                            if (request.HasQueryString && ((array != null) || cachedVary._varyByAllParams))
                            {
                                serverVarsWithoutDemand = request.QueryString;
                                flag = cachedVary._varyByAllParams;
                            }
                            break;

                        default:
                            builder.Append("F");
                            if (verb == HttpVerb.POST)
                            {
                                array = cachedVary._params;
                                if (request.HasForm && ((array != null) || cachedVary._varyByAllParams))
                                {
                                    serverVarsWithoutDemand = request.Form;
                                    flag = cachedVary._varyByAllParams;
                                }
                            }
                            break;
                    }
                    if (flag && (serverVarsWithoutDemand.Count > 0))
                    {
                        array = serverVarsWithoutDemand.AllKeys;
                        num = array.Length - 1;
                        while (num >= 0)
                        {
                            if (array[num] != null)
                            {
                                array[num] = CultureInfo.InvariantCulture.TextInfo.ToLower(array[num]);
                            }
                            num--;
                        }
                        Array.Sort(array, System.InvariantComparer.Default);
                    }
                    if (array != null)
                    {
                        num = 0;
                        int length = array.Length;
                        while (num < length)
                        {
                            string str = array[num];
                            if (serverVarsWithoutDemand == null)
                            {
                                varyByCustomString = "+n+";
                            }
                            else
                            {
                                varyByCustomString = serverVarsWithoutDemand[str];
                                if (varyByCustomString == null)
                                {
                                    varyByCustomString = "+n+";
                                }
                            }
                            builder.Append("N");
                            builder.Append(str);
                            builder.Append("V");
                            builder.Append(varyByCustomString);
                            num++;
                        }
                    }
                }
                builder.Append("C");
                if (cachedVary._varyByCustom != null)
                {
                    builder.Append("N");
                    builder.Append(cachedVary._varyByCustom);
                    builder.Append("V");
                    try
                    {
                        varyByCustomString = context.ApplicationInstance.GetVaryByCustomString(context, cachedVary._varyByCustom);
                        if (varyByCustomString == null)
                        {
                            varyByCustomString = "+n+";
                        }
                    }
                    catch (Exception exception)
                    {
                        varyByCustomString = "+e+";
                        HttpApplicationFactory.RaiseError(exception);
                    }
                    builder.Append(varyByCustomString);
                }
                builder.Append("D");
                if (((verb == HttpVerb.POST) && cachedVary._varyByAllParams) && (request.Form.Count == 0))
                {
                    int contentLength = request.ContentLength;
                    if ((contentLength > 0x3a98) || (contentLength < 0))
                    {
                        return null;
                    }
                    if (contentLength > 0)
                    {
                        byte[] asByteArray = ((HttpInputStream) request.InputStream).GetAsByteArray();
                        if (asByteArray == null)
                        {
                            return null;
                        }
                        varyByCustomString = Convert.ToBase64String(MachineKeySection.HashData(asByteArray, null, 0, asByteArray.Length));
                        builder.Append(varyByCustomString);
                    }
                }
                builder.Append("E");
                string[] strArray2 = cachedVary._contentEncodings;
                if (strArray2 != null)
                {
                    string httpHeaderContentEncoding = context.Response.GetHttpHeaderContentEncoding();
                    if (httpHeaderContentEncoding != null)
                    {
                        for (int j = 0; j < strArray2.Length; j++)
                        {
                            if (strArray2[j] == httpHeaderContentEncoding)
                            {
                                builder.Append(httpHeaderContentEncoding);
                                break;
                            }
                        }
                    }
                }
            }
            return builder.ToString();
        }

        private static int GetAcceptableEncoding(string[] contentEncodings, int startIndex, string acceptEncoding)
        {
            if (string.IsNullOrEmpty(acceptEncoding))
            {
                return -1;
            }
            if (acceptEncoding.IndexOf(',') == -1)
            {
                string str = acceptEncoding;
                int index = acceptEncoding.IndexOf(';');
                if (index > -1)
                {
                    int num2 = acceptEncoding.IndexOf(' ');
                    if ((num2 > -1) && (num2 < index))
                    {
                        index = num2;
                    }
                    str = acceptEncoding.Substring(0, index);
                    if (ParseWeight(acceptEncoding, index) == 0.0)
                    {
                        switch (str)
                        {
                            case "identity":
                            case "*":
                                return -2;
                        }
                        return -1;
                    }
                }
                if (str == "*")
                {
                    return 0;
                }
                for (int j = startIndex; j < contentEncodings.Length; j++)
                {
                    if (contentEncodings[j] == str)
                    {
                        return j;
                    }
                }
                return -1;
            }
            int num4 = -1;
            double num5 = 0.0;
            for (int i = startIndex; i < contentEncodings.Length; i++)
            {
                string coding = contentEncodings[i];
                double acceptableEncodingHelper = GetAcceptableEncodingHelper(coding, acceptEncoding);
                if (acceptableEncodingHelper == 1.0)
                {
                    return i;
                }
                if (acceptableEncodingHelper > num5)
                {
                    num4 = i;
                    num5 = acceptableEncodingHelper;
                }
            }
            if ((num4 == -1) && !IsIdentityAcceptable(acceptEncoding))
            {
                num4 = -2;
            }
            return num4;
        }

        private static double GetAcceptableEncodingHelper(string coding, string acceptEncoding)
        {
            double num = -1.0;
            int startIndex = 0;
            int length = coding.Length;
            int num4 = acceptEncoding.Length;
            int num5 = num4 - length;
            while (startIndex < num5)
            {
                int num6 = acceptEncoding.IndexOf(coding, startIndex, StringComparison.Ordinal);
                if (num6 == -1)
                {
                    return num;
                }
                if (num6 != 0)
                {
                    char ch = acceptEncoding[num6 - 1];
                    if ((ch != ' ') && (ch != ','))
                    {
                        startIndex = num6 + 1;
                        continue;
                    }
                }
                int num7 = num6 + length;
                char ch2 = '\0';
                if (num7 < num4)
                {
                    ch2 = acceptEncoding[num7];
                    while ((ch2 == ' ') && (++num7 < num4))
                    {
                        ch2 = acceptEncoding[num7];
                    }
                    if (((ch2 != ' ') && (ch2 != ',')) && (ch2 != ';'))
                    {
                        startIndex = num6 + 1;
                        continue;
                    }
                }
                return ((ch2 == ';') ? ParseWeight(acceptEncoding, num7) : 1.0);
            }
            return num;
        }

        private static bool IsAcceptableEncoding(string contentEncoding, string acceptEncoding)
        {
            if (string.IsNullOrEmpty(contentEncoding))
            {
                contentEncoding = "identity";
            }
            if (string.IsNullOrEmpty(acceptEncoding))
            {
                return (contentEncoding == "identity");
            }
            double acceptableEncodingHelper = GetAcceptableEncodingHelper(contentEncoding, acceptEncoding);
            return ((acceptableEncodingHelper != 0.0) && ((acceptableEncodingHelper > 0.0) || (GetAcceptableEncodingHelper("*", acceptEncoding) != 0.0)));
        }

        private static bool IsIdentityAcceptable(string acceptEncoding)
        {
            bool flag = true;
            double acceptableEncodingHelper = GetAcceptableEncodingHelper("identity", acceptEncoding);
            return (((acceptableEncodingHelper != 0.0) && ((acceptableEncodingHelper > 0.0) || (GetAcceptableEncodingHelper("*", acceptEncoding) != 0.0))) && flag);
        }

        internal void OnEnter(object source, EventArgs eventArgs)
        {
            this._key = null;
            this._recordedCacheMiss = false;
            if (OutputCache.InUse)
            {
                string[] strArray2 = null;
                string[] strArray3 = null;
                HttpApplication application = (HttpApplication) source;
                HttpContext context = application.Context;
                context.GetFilePathData();
                HttpRequest request = context.Request;
                HttpResponse response = context.Response;
                switch (request.HttpVerb)
                {
                    case HttpVerb.GET:
                    case HttpVerb.HEAD:
                    case HttpVerb.POST:
                    {
                        string str;
                        this._key = str = this.CreateOutputCachedItemKey(context, null);
                        object obj2 = OutputCache.Get(str);
                        if (obj2 != null)
                        {
                            int num;
                            int length;
                            CachedVary cachedVary = obj2 as CachedVary;
                            if (cachedVary != null)
                            {
                                str = this.CreateOutputCachedItemKey(context, cachedVary);
                                if (str == null)
                                {
                                    return;
                                }
                                if (cachedVary._contentEncodings == null)
                                {
                                    obj2 = OutputCache.Get(str);
                                }
                                else
                                {
                                    obj2 = null;
                                    bool flag3 = true;
                                    string knownRequestHeader = context.WorkerRequest.GetKnownRequestHeader(0x16);
                                    if (knownRequestHeader != null)
                                    {
                                        string[] contentEncodings = cachedVary._contentEncodings;
                                        int startIndex = 0;
                                        bool flag4 = false;
                                        while (!flag4)
                                        {
                                            flag4 = true;
                                            int index = GetAcceptableEncoding(contentEncodings, startIndex, knownRequestHeader);
                                            if (index > -1)
                                            {
                                                flag3 = false;
                                                obj2 = OutputCache.Get(str + contentEncodings[index]);
                                                if (obj2 == null)
                                                {
                                                    startIndex = index + 1;
                                                    if (startIndex < contentEncodings.Length)
                                                    {
                                                        flag4 = false;
                                                    }
                                                }
                                            }
                                            else if (index == -2)
                                            {
                                                flag3 = false;
                                            }
                                        }
                                    }
                                    if ((obj2 == null) && flag3)
                                    {
                                        obj2 = OutputCache.Get(str);
                                    }
                                }
                                if ((obj2 == null) || (((CachedRawResponse) obj2)._cachedVaryId != cachedVary.CachedVaryId))
                                {
                                    if (obj2 != null)
                                    {
                                        OutputCache.Remove(str, context);
                                    }
                                    return;
                                }
                            }
                            CachedRawResponse response2 = (CachedRawResponse) obj2;
                            HttpCachePolicySettings settings = response2._settings;
                            if ((cachedVary == null) && !settings.IgnoreParams)
                            {
                                if (request.HttpVerb == HttpVerb.POST)
                                {
                                    this.RecordCacheMiss();
                                    return;
                                }
                                if (request.HasQueryString)
                                {
                                    this.RecordCacheMiss();
                                    return;
                                }
                            }
                            if (settings.IgnoreRangeRequests)
                            {
                                string str8 = request.Headers["Range"];
                                if (StringUtil.StringStartsWithIgnoreCase(str8, "bytes"))
                                {
                                    return;
                                }
                            }
                            if (!settings.HasValidationPolicy())
                            {
                                string str4 = request.Headers["Cache-Control"];
                                if (str4 != null)
                                {
                                    strArray2 = str4.Split(s_fieldSeparators);
                                    for (num = 0; num < strArray2.Length; num++)
                                    {
                                        string str6 = strArray2[num];
                                        switch (str6)
                                        {
                                            case "no-cache":
                                            case "no-store":
                                                this.RecordCacheMiss();
                                                return;
                                        }
                                        if (StringUtil.StringStartsWith(str6, "max-age="))
                                        {
                                            int num4;
                                            try
                                            {
                                                num4 = Convert.ToInt32(str6.Substring(8), CultureInfo.InvariantCulture);
                                            }
                                            catch
                                            {
                                                num4 = -1;
                                            }
                                            if (num4 >= 0)
                                            {
                                                int num6 = (int) ((context.UtcTimestamp.Ticks - settings.UtcTimestampCreated.Ticks) / 0x989680L);
                                                if (num6 >= num4)
                                                {
                                                    this.RecordCacheMiss();
                                                    return;
                                                }
                                            }
                                        }
                                        else if (StringUtil.StringStartsWith(str6, "min-fresh="))
                                        {
                                            int num5;
                                            try
                                            {
                                                num5 = Convert.ToInt32(str6.Substring(10), CultureInfo.InvariantCulture);
                                            }
                                            catch
                                            {
                                                num5 = -1;
                                            }
                                            if (((num5 >= 0) && settings.IsExpiresSet) && !settings.SlidingExpiration)
                                            {
                                                int num7 = (int) ((settings.UtcExpires.Ticks - context.UtcTimestamp.Ticks) / 0x989680L);
                                                if (num7 < num5)
                                                {
                                                    this.RecordCacheMiss();
                                                    return;
                                                }
                                            }
                                        }
                                    }
                                }
                                string str5 = request.Headers["Pragma"];
                                if (str5 != null)
                                {
                                    strArray3 = str5.Split(s_fieldSeparators);
                                    for (num = 0; num < strArray3.Length; num++)
                                    {
                                        if (strArray3[num] == "no-cache")
                                        {
                                            this.RecordCacheMiss();
                                            return;
                                        }
                                    }
                                }
                            }
                            else if (settings.ValidationCallbackInfo != null)
                            {
                                HttpValidationStatus valid = HttpValidationStatus.Valid;
                                HttpValidationStatus ignoreThisRequest = valid;
                                num = 0;
                                length = settings.ValidationCallbackInfo.Length;
                                while (num < length)
                                {
                                    ValidationCallbackInfo info = settings.ValidationCallbackInfo[num];
                                    try
                                    {
                                        info.handler(context, info.data, ref valid);
                                    }
                                    catch (Exception exception)
                                    {
                                        valid = HttpValidationStatus.Invalid;
                                        HttpApplicationFactory.RaiseError(exception);
                                    }
                                    switch (valid)
                                    {
                                        case HttpValidationStatus.Invalid:
                                            OutputCache.Remove(str, context);
                                            this.RecordCacheMiss();
                                            return;

                                        case HttpValidationStatus.IgnoreThisRequest:
                                            ignoreThisRequest = HttpValidationStatus.IgnoreThisRequest;
                                            break;

                                        case HttpValidationStatus.Valid:
                                            break;

                                        default:
                                            valid = ignoreThisRequest;
                                            break;
                                    }
                                    num++;
                                }
                                if (ignoreThisRequest == HttpValidationStatus.IgnoreThisRequest)
                                {
                                    this.RecordCacheMiss();
                                    return;
                                }
                            }
                            HttpRawResponse rawResponse = response2._rawResponse;
                            if ((cachedVary == null) || (cachedVary._contentEncodings == null))
                            {
                                string acceptEncoding = request.Headers["Accept-Encoding"];
                                string contentEncoding = null;
                                ArrayList headers = rawResponse.Headers;
                                if (headers != null)
                                {
                                    foreach (HttpResponseHeader header in headers)
                                    {
                                        if (header.Name == "Content-Encoding")
                                        {
                                            contentEncoding = header.Value;
                                            break;
                                        }
                                    }
                                }
                                if (!IsAcceptableEncoding(contentEncoding, acceptEncoding))
                                {
                                    this.RecordCacheMiss();
                                    return;
                                }
                            }
                            int num3 = -1;
                            if (!rawResponse.HasSubstBlocks)
                            {
                                string ifModifiedSince = request.IfModifiedSince;
                                if (ifModifiedSince != null)
                                {
                                    num3 = 0;
                                    try
                                    {
                                        DateTime time = HttpDate.UtcParse(ifModifiedSince);
                                        if ((settings.IsLastModifiedSet && (settings.UtcLastModified <= time)) && (time <= context.UtcTimestamp))
                                        {
                                            num3 = 1;
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                                if (num3 != 0)
                                {
                                    string ifNoneMatch = request.IfNoneMatch;
                                    if (ifNoneMatch != null)
                                    {
                                        num3 = 0;
                                        string[] strArray = ifNoneMatch.Split(s_fieldSeparators);
                                        num = 0;
                                        length = strArray.Length;
                                        while (num < length)
                                        {
                                            if ((num == 0) && strArray[num].Equals("*"))
                                            {
                                                num3 = 1;
                                                break;
                                            }
                                            if (strArray[num].Equals(settings.ETag))
                                            {
                                                num3 = 1;
                                                break;
                                            }
                                            num++;
                                        }
                                    }
                                }
                            }
                            if (num3 == 1)
                            {
                                response.ClearAll();
                                response.StatusCode = 0x130;
                            }
                            else
                            {
                                bool sendBody = request.HttpVerb != HttpVerb.HEAD;
                                response.UseSnapshot(rawResponse, sendBody);
                            }
                            response.Cache.ResetFromHttpCachePolicySettings(settings, context.UtcTimestamp);
                            string originalCacheUrl = response2._kernelCacheUrl;
                            if (originalCacheUrl != null)
                            {
                                response.SetupKernelCaching(originalCacheUrl);
                            }
                            PerfCounters.IncrementCounter(AppPerfCounter.OUTPUT_CACHE_RATIO_BASE);
                            PerfCounters.IncrementCounter(AppPerfCounter.OUTPUT_CACHE_HITS);
                            this._key = null;
                            this._recordedCacheMiss = false;
                            application.CompleteRequest();
                            return;
                        }
                        return;
                    }
                }
            }
        }

        internal void OnLeave(object source, EventArgs eventArgs)
        {
            HttpApplication application = (HttpApplication) source;
            HttpContext context = application.Context;
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            HttpCachePolicy cache = null;
            bool flag = false;
            if (response.HasCachePolicy)
            {
                cache = response.Cache;
                if (((cache.IsModified() && (response.Cookies.Count <= 0)) && (response.StatusCode == 200)) && (((request.HttpVerb == HttpVerb.GET) || (request.HttpVerb == HttpVerb.POST)) && response.IsBuffered()))
                {
                    bool flag3 = false;
                    if ((cache.GetCacheability() == HttpCacheability.Public) && context.RequestRequiresAuthorization())
                    {
                        cache.SetCacheability(HttpCacheability.Private);
                        flag3 = true;
                    }
                    if (((((cache.GetCacheability() == HttpCacheability.Public) || (cache.GetCacheability() == HttpCacheability.ServerAndPrivate)) || ((cache.GetCacheability() == HttpCacheability.Server) || flag3)) && (!cache.GetNoServerCaching() && (cache.HasExpirationPolicy() || cache.HasValidationPolicy()))) && ((!cache.VaryByHeaders.GetVaryByUnspecifiedParameters() && (cache.VaryByParams.AcceptsParams() || ((request.HttpVerb != HttpVerb.POST) && !request.HasQueryString))) && (!cache.VaryByContentEncodings.IsModified() || cache.VaryByContentEncodings.IsCacheableEncoding(context.Response.GetHttpHeaderContentEncoding()))))
                    {
                        flag = true;
                    }
                }
            }
            if (flag)
            {
                CachedVary vary;
                string str;
                string[] varyByParams;
                this.RecordCacheMiss();
                HttpCachePolicySettings currentSettings = cache.GetCurrentSettings(response);
                string[] varyByContentEncodings = currentSettings.VaryByContentEncodings;
                string[] varyByHeaders = currentSettings.VaryByHeaders;
                if (currentSettings.IgnoreParams)
                {
                    varyByParams = null;
                }
                else
                {
                    varyByParams = currentSettings.VaryByParams;
                }
                if (this._key == null)
                {
                    this._key = this.CreateOutputCachedItemKey(context, null);
                }
                if (((varyByContentEncodings == null) && (varyByHeaders == null)) && ((varyByParams == null) && (currentSettings.VaryByCustom == null)))
                {
                    str = this._key;
                    vary = null;
                }
                else
                {
                    int num;
                    int length;
                    if (varyByHeaders != null)
                    {
                        num = 0;
                        length = varyByHeaders.Length;
                        while (num < length)
                        {
                            varyByHeaders[num] = "HTTP_" + CultureInfo.InvariantCulture.TextInfo.ToUpper(varyByHeaders[num].Replace('-', '_'));
                            num++;
                        }
                    }
                    bool varyByAllParams = false;
                    if (varyByParams != null)
                    {
                        varyByAllParams = (varyByParams.Length == 1) && (varyByParams[0] == "*");
                        if (varyByAllParams)
                        {
                            varyByParams = null;
                        }
                        else
                        {
                            num = 0;
                            length = varyByParams.Length;
                            while (num < length)
                            {
                                varyByParams[num] = CultureInfo.InvariantCulture.TextInfo.ToLower(varyByParams[num]);
                                num++;
                            }
                        }
                    }
                    vary = new CachedVary(varyByContentEncodings, varyByHeaders, varyByParams, varyByAllParams, currentSettings.VaryByCustom);
                    str = this.CreateOutputCachedItemKey(context, vary);
                    if (str == null)
                    {
                        return;
                    }
                    if (!response.IsBuffered())
                    {
                        return;
                    }
                }
                DateTime noAbsoluteExpiration = Cache.NoAbsoluteExpiration;
                TimeSpan noSlidingExpiration = Cache.NoSlidingExpiration;
                if (currentSettings.SlidingExpiration)
                {
                    noSlidingExpiration = currentSettings.SlidingDelta;
                }
                else if (currentSettings.IsMaxAgeSet)
                {
                    DateTime time2 = (currentSettings.UtcTimestampCreated != DateTime.MinValue) ? currentSettings.UtcTimestampCreated : context.UtcTimestamp;
                    noAbsoluteExpiration = time2 + currentSettings.MaxAge;
                }
                else if (currentSettings.IsExpiresSet)
                {
                    noAbsoluteExpiration = currentSettings.UtcExpires;
                }
                if (noAbsoluteExpiration > DateTime.UtcNow)
                {
                    HttpRawResponse snapshot = response.GetSnapshot();
                    string kernelCacheUrl = response.SetupKernelCaching(null);
                    Guid cachedVaryId = (vary != null) ? vary.CachedVaryId : Guid.Empty;
                    CachedRawResponse rawResponse = new CachedRawResponse(snapshot, currentSettings, kernelCacheUrl, cachedVaryId);
                    CacheDependency dependencies = response.CreateCacheDependencyForResponse();
                    try
                    {
                        OutputCache.InsertResponse(this._key, vary, str, rawResponse, dependencies, noAbsoluteExpiration, noSlidingExpiration);
                    }
                    catch
                    {
                        if (dependencies != null)
                        {
                            dependencies.Dispose();
                        }
                        throw;
                    }
                }
                this._key = null;
            }
        }

        private static double ParseWeight(string acceptEncoding, int startIndex)
        {
            double num5;
            double num = 1.0;
            int index = acceptEncoding.IndexOf(',', startIndex);
            if (index == -1)
            {
                index = acceptEncoding.Length;
            }
            int num3 = acceptEncoding.IndexOf('q', startIndex);
            if ((num3 <= -1) || (num3 >= index))
            {
                return num;
            }
            int num4 = acceptEncoding.IndexOf('=', num3);
            if (((num4 <= -1) || (num4 >= index)) || !double.TryParse(acceptEncoding.Substring(num4 + 1, index - (num4 + 1)), NumberStyles.AllowDecimalPoint | NumberStyles.AllowTrailingWhite | NumberStyles.AllowLeadingWhite, CultureInfo.InvariantCulture, out num5))
            {
                return num;
            }
            return (((num5 >= 0.0) && (num5 <= 1.0)) ? num5 : 1.0);
        }

        private void RecordCacheMiss()
        {
            if (!this._recordedCacheMiss)
            {
                PerfCounters.IncrementCounter(AppPerfCounter.OUTPUT_CACHE_RATIO_BASE);
                PerfCounters.IncrementCounter(AppPerfCounter.OUTPUT_CACHE_MISSES);
                this._recordedCacheMiss = true;
            }
        }

        void IHttpModule.Dispose()
        {
        }

        void IHttpModule.Init(HttpApplication app)
        {
            if (RuntimeConfig.GetAppConfig().OutputCache.EnableOutputCache)
            {
                app.ResolveRequestCache += new EventHandler(this.OnEnter);
                app.UpdateRequestCache += new EventHandler(this.OnLeave);
            }
        }
    }
}

