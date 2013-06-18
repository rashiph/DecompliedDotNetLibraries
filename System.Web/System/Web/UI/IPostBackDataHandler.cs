namespace System.Web.UI
{
    using System;
    using System.Collections.Specialized;

    public interface IPostBackDataHandler
    {
        bool LoadPostData(string postDataKey, NameValueCollection postCollection);
        void RaisePostDataChangedEvent();
    }
}

