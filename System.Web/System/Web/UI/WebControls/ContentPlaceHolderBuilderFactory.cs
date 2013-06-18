namespace System.Web.UI.WebControls
{
    using System;
    using System.Web.Util;

    internal class ContentPlaceHolderBuilderFactory : IWebObjectFactory
    {
        object IWebObjectFactory.CreateInstance()
        {
            return new ContentPlaceHolderBuilder();
        }
    }
}

