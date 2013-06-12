namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.Util;

    internal class ContentBuilderInternalFactory : IWebObjectFactory
    {
        object IWebObjectFactory.CreateInstance()
        {
            return new ContentBuilderInternal();
        }
    }
}

