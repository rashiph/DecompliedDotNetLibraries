namespace System.Web.UI.Design
{
    using System;
    using System.Collections;

    public sealed class ClientScriptItemCollection : ReadOnlyCollectionBase
    {
        public ClientScriptItemCollection(ClientScriptItem[] clientScriptItems)
        {
            if (clientScriptItems != null)
            {
                foreach (ClientScriptItem item in clientScriptItems)
                {
                    base.InnerList.Add(item);
                }
            }
        }
    }
}

