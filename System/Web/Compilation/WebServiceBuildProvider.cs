namespace System.Web.Compilation
{
    using System.Web.UI;

    internal class WebServiceBuildProvider : SimpleHandlerBuildProvider
    {
        protected override SimpleWebHandlerParser CreateParser()
        {
            return new WebServiceParser(base.VirtualPath);
        }
    }
}

