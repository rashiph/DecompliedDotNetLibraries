namespace System.Web
{
    using System;
    using System.Runtime.CompilerServices;

    [TypeForwardedFrom("System.Web.Abstractions, Version=3.5.0.0, Culture=Neutral, PublicKeyToken=31bf3856ad364e35")]
    public class HttpCachePolicyWrapper : HttpCachePolicyBase
    {
        private HttpCachePolicy _httpCachePolicy;

        public HttpCachePolicyWrapper(HttpCachePolicy httpCachePolicy)
        {
            if (httpCachePolicy == null)
            {
                throw new ArgumentNullException("httpCachePolicy");
            }
            this._httpCachePolicy = httpCachePolicy;
        }

        public override void AddValidationCallback(HttpCacheValidateHandler handler, object data)
        {
            this._httpCachePolicy.AddValidationCallback(handler, data);
        }

        public override void AppendCacheExtension(string extension)
        {
            this._httpCachePolicy.AppendCacheExtension(extension);
        }

        public override void SetAllowResponseInBrowserHistory(bool allow)
        {
            this._httpCachePolicy.SetAllowResponseInBrowserHistory(allow);
        }

        public override void SetCacheability(HttpCacheability cacheability)
        {
            this._httpCachePolicy.SetCacheability(cacheability);
        }

        public override void SetCacheability(HttpCacheability cacheability, string field)
        {
            this._httpCachePolicy.SetCacheability(cacheability, field);
        }

        public override void SetETag(string etag)
        {
            this._httpCachePolicy.SetETag(etag);
        }

        public override void SetETagFromFileDependencies()
        {
            this._httpCachePolicy.SetETagFromFileDependencies();
        }

        public override void SetExpires(DateTime date)
        {
            this._httpCachePolicy.SetExpires(date);
        }

        public override void SetLastModified(DateTime date)
        {
            this._httpCachePolicy.SetLastModified(date);
        }

        public override void SetLastModifiedFromFileDependencies()
        {
            this._httpCachePolicy.SetLastModifiedFromFileDependencies();
        }

        public override void SetMaxAge(TimeSpan delta)
        {
            this._httpCachePolicy.SetMaxAge(delta);
        }

        public override void SetNoServerCaching()
        {
            this._httpCachePolicy.SetNoServerCaching();
        }

        public override void SetNoStore()
        {
            this._httpCachePolicy.SetNoStore();
        }

        public override void SetNoTransforms()
        {
            this._httpCachePolicy.SetNoTransforms();
        }

        public override void SetOmitVaryStar(bool omit)
        {
            this._httpCachePolicy.SetOmitVaryStar(omit);
        }

        public override void SetProxyMaxAge(TimeSpan delta)
        {
            this._httpCachePolicy.SetProxyMaxAge(delta);
        }

        public override void SetRevalidation(HttpCacheRevalidation revalidation)
        {
            this._httpCachePolicy.SetRevalidation(revalidation);
        }

        public override void SetSlidingExpiration(bool slide)
        {
            this._httpCachePolicy.SetSlidingExpiration(slide);
        }

        public override void SetValidUntilExpires(bool validUntilExpires)
        {
            this._httpCachePolicy.SetValidUntilExpires(validUntilExpires);
        }

        public override void SetVaryByCustom(string custom)
        {
            this._httpCachePolicy.SetVaryByCustom(custom);
        }

        public override HttpCacheVaryByContentEncodings VaryByContentEncodings
        {
            get
            {
                return this._httpCachePolicy.VaryByContentEncodings;
            }
        }

        public override HttpCacheVaryByHeaders VaryByHeaders
        {
            get
            {
                return this._httpCachePolicy.VaryByHeaders;
            }
        }

        public override HttpCacheVaryByParams VaryByParams
        {
            get
            {
                return this._httpCachePolicy.VaryByParams;
            }
        }
    }
}

