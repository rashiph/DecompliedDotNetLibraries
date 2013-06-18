namespace System.Web.Hosting
{
    using System;
    using System.Web;

    public abstract class VirtualFileBase : MarshalByRefObject
    {
        internal System.Web.VirtualPath _virtualPath;

        protected VirtualFileBase()
        {
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        public abstract bool IsDirectory { get; }

        public virtual string Name
        {
            get
            {
                return this._virtualPath.FileName;
            }
        }

        public string VirtualPath
        {
            get
            {
                return this._virtualPath.VirtualPathString;
            }
        }

        internal System.Web.VirtualPath VirtualPathObject
        {
            get
            {
                return this._virtualPath;
            }
        }
    }
}

