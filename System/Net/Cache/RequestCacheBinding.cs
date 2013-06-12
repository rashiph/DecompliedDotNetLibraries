namespace System.Net.Cache
{
    using System;

    internal class RequestCacheBinding
    {
        private RequestCacheValidator m_CacheValidator;
        private RequestCachePolicy m_Policy;
        private RequestCache m_RequestCache;

        internal RequestCacheBinding(RequestCache requestCache, RequestCacheValidator cacheValidator, RequestCachePolicy policy)
        {
            this.m_RequestCache = requestCache;
            this.m_CacheValidator = cacheValidator;
            this.m_Policy = policy;
        }

        internal RequestCache Cache
        {
            get
            {
                return this.m_RequestCache;
            }
        }

        internal RequestCachePolicy Policy
        {
            get
            {
                return this.m_Policy;
            }
        }

        internal RequestCacheValidator Validator
        {
            get
            {
                return this.m_CacheValidator;
            }
        }
    }
}

