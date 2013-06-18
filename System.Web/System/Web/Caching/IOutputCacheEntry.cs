namespace System.Web.Caching
{
    using System;
    using System.Collections.Generic;

    public interface IOutputCacheEntry
    {
        List<HeaderElement> HeaderElements { get; set; }

        List<ResponseElement> ResponseElements { get; set; }
    }
}

