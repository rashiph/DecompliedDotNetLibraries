namespace System.Web.SessionState
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Util;

    internal class StateApplication : IHttpHandler
    {
        private CacheItemRemovedCallback _removedHandler;

        internal StateApplication()
        {
            this._removedHandler = new CacheItemRemovedCallback(this.OnCacheItemRemoved);
        }

        private string CreateKey(HttpRequest request)
        {
            return ("k" + HttpUtility.UrlDecode(request.RawUrl));
        }

        private void DecrementStateServiceCounter(StateServicePerfCounter counter)
        {
            if (!HttpRuntime.ShutdownInProgress)
            {
                PerfCounters.DecrementStateServiceCounter(counter);
            }
        }

        internal void DoDelete(HttpContext context)
        {
            string key = this.CreateKey(context.Request);
            CacheInternal cacheInternal = HttpRuntime.CacheInternal;
            CachedContent content = (CachedContent) cacheInternal.Get(key);
            if (content == null)
            {
                this.ReportNotFound(context);
            }
            else
            {
                int num;
                if (this.GetOptionalNonNegativeInt32HeaderValue(context, "Http_LockCookie", out num))
                {
                    content._spinLock.AcquireWriterLock();
                    try
                    {
                        if (content._content == null)
                        {
                            this.ReportNotFound(context);
                            return;
                        }
                        if (content._locked && ((num == -1) || (content._lockCookie != num)))
                        {
                            this.ReportLocked(context, content);
                            return;
                        }
                        content._locked = true;
                        content._lockCookie = 0;
                    }
                    finally
                    {
                        content._spinLock.ReleaseWriterLock();
                    }
                    cacheInternal.Remove(key);
                }
            }
        }

        internal void DoGet(HttpContext context)
        {
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            string key = this.CreateKey(request);
            CacheEntry entry = (CacheEntry) HttpRuntime.CacheInternal.Get(key, CacheGetOptions.ReturnCacheEntry);
            if (entry == null)
            {
                this.ReportNotFound(context);
            }
            else
            {
                string str = request.Headers["Http_Exclusive"];
                CachedContent content = (CachedContent) entry.Value;
                content._spinLock.AcquireWriterLock();
                try
                {
                    if (content._content == null)
                    {
                        this.ReportNotFound(context);
                    }
                    else
                    {
                        int comparand = content._extraFlags;
                        if (((comparand & 1) != 0) && (comparand == Interlocked.CompareExchange(ref content._extraFlags, comparand & -2, comparand)))
                        {
                            this.ReportActionFlags(context, 1);
                        }
                        if (str == "release")
                        {
                            int num;
                            if (this.GetRequiredNonNegativeInt32HeaderValue(context, "Http_LockCookie", out num))
                            {
                                if (content._locked)
                                {
                                    if (num == content._lockCookie)
                                    {
                                        content._locked = false;
                                    }
                                    else
                                    {
                                        this.ReportLocked(context, content);
                                    }
                                }
                                else
                                {
                                    context.Response.StatusCode = 200;
                                }
                            }
                        }
                        else if (content._locked)
                        {
                            this.ReportLocked(context, content);
                        }
                        else
                        {
                            if (str == "acquire")
                            {
                                content._locked = true;
                                content._utcLockDate = DateTime.UtcNow;
                                content._lockCookie++;
                                response.AppendHeader("LockCookie", content._lockCookie.ToString(CultureInfo.InvariantCulture));
                            }
                            response.AppendHeader("Timeout", ((int) (entry.SlidingExpiration.Ticks / 0x23c34600L)).ToString(CultureInfo.InvariantCulture));
                            Stream outputStream = response.OutputStream;
                            byte[] buffer = content._content;
                            outputStream.Write(buffer, 0, buffer.Length);
                            response.Flush();
                        }
                    }
                }
                finally
                {
                    content._spinLock.ReleaseWriterLock();
                }
            }
        }

        internal void DoHead(HttpContext context)
        {
            string key = this.CreateKey(context.Request);
            if (HttpRuntime.CacheInternal.Get(key) == null)
            {
                this.ReportNotFound(context);
            }
        }

        internal void DoPut(HttpContext context)
        {
            IntPtr stateItem = this.FinishPut(context);
            if (stateItem != IntPtr.Zero)
            {
                UnsafeNativeMethods.STWNDDeleteStateItem(stateItem);
            }
        }

        internal void DoUnknown(HttpContext context)
        {
            context.Response.StatusCode = 400;
        }

        private unsafe IntPtr FinishPut(HttpContext context)
        {
            int num;
            int num2;
            IntPtr ptr;
            bool flag;
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            int lockCookie = 1;
            CacheInternal cacheInternal = HttpRuntime.CacheInternal;
            Stream inputStream = request.InputStream;
            int num5 = (int) (inputStream.Length - inputStream.Position);
            byte[] buffer = new byte[num5];
            inputStream.Read(buffer, 0, buffer.Length);
            fixed (byte* numRef = buffer)
            {
                ptr = *((IntPtr*) numRef);
            }
            if (!this.GetOptionalNonNegativeInt32HeaderValue(context, "Http_Timeout", out num))
            {
                return ptr;
            }
            if (num == -1)
            {
                num = 20;
            }
            if (num > 0x80520)
            {
                this.ReportInvalidHeader(context, "Http_Timeout");
                return ptr;
            }
            TimeSpan slidingExpiration = new TimeSpan(0, num, 0);
            if (!this.GetOptionalInt32HeaderValue(context, "Http_ExtraFlags", out num2, out flag))
            {
                return ptr;
            }
            if (!flag)
            {
                num2 = 0;
            }
            string key = this.CreateKey(request);
            CacheEntry entry = (CacheEntry) cacheInternal.Get(key, CacheGetOptions.ReturnCacheEntry);
            if (entry != null)
            {
                int num3;
                if ((1 & num2) == 1)
                {
                    return ptr;
                }
                if (!this.GetOptionalNonNegativeInt32HeaderValue(context, "Http_LockCookie", out num3))
                {
                    return ptr;
                }
                CachedContent content2 = (CachedContent) entry.Value;
                content2._spinLock.AcquireWriterLock();
                try
                {
                    if (content2._content == null)
                    {
                        this.ReportNotFound(context);
                        return ptr;
                    }
                    if (content2._locked && ((num3 == -1) || (num3 != content2._lockCookie)))
                    {
                        this.ReportLocked(context, content2);
                        return ptr;
                    }
                    if ((entry.SlidingExpiration == slidingExpiration) && (content2._content != null))
                    {
                        IntPtr ptr2 = content2._stateItem;
                        content2._content = buffer;
                        content2._stateItem = ptr;
                        content2._locked = false;
                        return ptr2;
                    }
                    content2._extraFlags |= 2;
                    content2._locked = true;
                    content2._lockCookie = 0;
                    lockCookie = num3;
                }
                finally
                {
                    content2._spinLock.ReleaseWriterLock();
                }
            }
            CachedContent content = new CachedContent(buffer, ptr, false, DateTime.MinValue, lockCookie, num2);
            cacheInternal.UtcInsert(key, content, null, Cache.NoAbsoluteExpiration, slidingExpiration, CacheItemPriority.NotRemovable, this._removedHandler);
            if (entry == null)
            {
                this.IncrementStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_TOTAL);
                this.IncrementStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_ACTIVE);
            }
            return IntPtr.Zero;
        }

        private bool GetOptionalInt32HeaderValue(HttpContext context, string header, out int value, out bool found)
        {
            bool flag;
            found = false;
            value = 0;
            string s = context.Request.Headers[header];
            if (s == null)
            {
                flag = true;
            }
            else
            {
                flag = false;
                try
                {
                    value = int.Parse(s, CultureInfo.InvariantCulture);
                    flag = true;
                    found = true;
                }
                catch
                {
                }
            }
            if (!flag)
            {
                this.ReportInvalidHeader(context, header);
            }
            return flag;
        }

        private bool GetOptionalNonNegativeInt32HeaderValue(HttpContext context, string header, out int value)
        {
            bool flag;
            value = -1;
            string s = context.Request.Headers[header];
            if (s == null)
            {
                flag = true;
            }
            else
            {
                flag = false;
                try
                {
                    value = int.Parse(s, CultureInfo.InvariantCulture);
                    if (value >= 0)
                    {
                        flag = true;
                    }
                }
                catch
                {
                }
            }
            if (!flag)
            {
                this.ReportInvalidHeader(context, header);
            }
            return flag;
        }

        private bool GetRequiredNonNegativeInt32HeaderValue(HttpContext context, string header, out int value)
        {
            bool flag = this.GetOptionalNonNegativeInt32HeaderValue(context, header, out value);
            if (flag && (value == -1))
            {
                flag = false;
                this.ReportInvalidHeader(context, header);
            }
            return flag;
        }

        private void IncrementStateServiceCounter(StateServicePerfCounter counter)
        {
            if (!HttpRuntime.ShutdownInProgress)
            {
                PerfCounters.IncrementStateServiceCounter(counter);
            }
        }

        private void OnCacheItemRemoved(string key, object value, CacheItemRemovedReason reason)
        {
            IntPtr ptr;
            CachedContent content = (CachedContent) value;
            content._spinLock.AcquireWriterLock();
            try
            {
                ptr = content._stateItem;
                content._content = null;
                content._stateItem = IntPtr.Zero;
            }
            finally
            {
                content._spinLock.ReleaseWriterLock();
            }
            UnsafeNativeMethods.STWNDDeleteStateItem(ptr);
            if ((content._extraFlags & 2) == 0)
            {
                switch (reason)
                {
                    case CacheItemRemovedReason.Removed:
                        this.IncrementStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_ABANDONED);
                        break;

                    case CacheItemRemovedReason.Expired:
                        this.IncrementStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_TIMED_OUT);
                        break;
                }
                this.DecrementStateServiceCounter(StateServicePerfCounter.STATE_SERVICE_SESSIONS_ACTIVE);
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = null;
            switch (context.Request.HttpVerb)
            {
                case HttpVerb.GET:
                    this.DoGet(context);
                    return;

                case HttpVerb.PUT:
                    this.DoPut(context);
                    return;

                case HttpVerb.HEAD:
                    this.DoHead(context);
                    return;

                case HttpVerb.DELETE:
                    this.DoDelete(context);
                    return;
            }
            this.DoUnknown(context);
        }

        private void ReportActionFlags(HttpContext context, int flags)
        {
            context.Response.AppendHeader("ActionFlags", flags.ToString(CultureInfo.InvariantCulture));
        }

        private void ReportInvalidHeader(HttpContext context, string header)
        {
            HttpResponse response = context.Response;
            response.StatusCode = 400;
            response.Write("<html><head><title>Bad Request</title></head>\r\n");
            response.Write("<body><h1>Http/1.1 400 Bad Request</h1>");
            response.Write("Invalid header <b>" + header + "</b></body></html>");
        }

        private void ReportLocked(HttpContext context, CachedContent content)
        {
            HttpResponse response = context.Response;
            response.StatusCode = 0x1a7;
            DateTime time = DateTimeUtil.ConvertToLocalTime(content._utcLockDate);
            TimeSpan span = (TimeSpan) (DateTime.UtcNow - content._utcLockDate);
            long num = span.Ticks / 0x989680L;
            response.AppendHeader("LockDate", time.Ticks.ToString(CultureInfo.InvariantCulture));
            response.AppendHeader("LockAge", num.ToString(CultureInfo.InvariantCulture));
            response.AppendHeader("LockCookie", content._lockCookie.ToString(CultureInfo.InvariantCulture));
        }

        private void ReportNotFound(HttpContext context)
        {
            context.Response.StatusCode = 0x194;
        }

        public bool IsReusable
        {
            get
            {
                return true;
            }
        }
    }
}

