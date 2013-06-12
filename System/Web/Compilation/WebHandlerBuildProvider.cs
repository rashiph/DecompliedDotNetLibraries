namespace System.Web.Compilation
{
    using System.Web.UI;

    internal class WebHandlerBuildProvider : SimpleHandlerBuildProvider
    {
        protected override SimpleWebHandlerParser CreateParser()
        {
            return new WebHandlerParser(base.VirtualPath);
        }
    }
}

