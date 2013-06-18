namespace System.Web.Services.Protocols
{
    using System;
    using System.Web;

    public class UrlParameterReader : ValueCollectionParameterReader
    {
        public override object[] Read(HttpRequest request)
        {
            return base.Read(request.QueryString);
        }
    }
}

