namespace System.Web.UI
{
    using System;
    using System.Web;
    using System.Web.Compilation;

    internal class SimpleHandlerFactory : IHttpHandlerFactory2, IHttpHandlerFactory
    {
        internal SimpleHandlerFactory()
        {
        }

        public virtual IHttpHandler GetHandler(HttpContext context, string requestType, string virtualPath, string path)
        {
            return ((IHttpHandlerFactory2) this).GetHandler(context, requestType, VirtualPath.CreateNonRelative(virtualPath), path);
        }

        public virtual void ReleaseHandler(IHttpHandler handler)
        {
        }

        IHttpHandler IHttpHandlerFactory2.GetHandler(HttpContext context, string requestType, VirtualPath virtualPath, string physicalPath)
        {
            BuildResultCompiledType vPathBuildResult = (BuildResultCompiledType) BuildManager.GetVPathBuildResult(context, virtualPath);
            Util.CheckAssignableType(typeof(IHttpHandler), vPathBuildResult.ResultType);
            return (IHttpHandler) vPathBuildResult.CreateInstance();
        }
    }
}

