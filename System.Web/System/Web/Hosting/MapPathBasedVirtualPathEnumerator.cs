namespace System.Web.Hosting
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.Util;

    internal class MapPathBasedVirtualPathEnumerator : MarshalByRefObject, IEnumerator, IDisposable
    {
        private Hashtable _exclude;
        private IEnumerator _fileEnumerator;
        private RequestedEntryType _requestedEntryType;
        private IServerConfig2 _serverConfig2;
        private bool _useFileEnumerator;
        private IEnumerator _virtualEnumerator;
        private VirtualPath _virtualPath;
        private Hashtable _virtualPaths;

        internal MapPathBasedVirtualPathEnumerator(VirtualPath virtualPath, RequestedEntryType requestedEntryType)
        {
            string str;
            if (virtualPath.IsRelative)
            {
                throw new ArgumentException(System.Web.SR.GetString("Invalid_app_VirtualPath"), "virtualPath");
            }
            this._virtualPath = virtualPath;
            this._requestedEntryType = requestedEntryType;
            if (!ServerConfig.UseServerConfig)
            {
                str = this._virtualPath.MapPathInternal();
            }
            else
            {
                IServerConfig instance = ServerConfig.GetInstance();
                this._serverConfig2 = instance as IServerConfig2;
                str = instance.MapPath(null, this._virtualPath);
                if (this._requestedEntryType != RequestedEntryType.Files)
                {
                    if (this._serverConfig2 == null)
                    {
                        string[] strArray = instance.GetVirtualSubdirs(this._virtualPath, false);
                        if (strArray != null)
                        {
                            this._exclude = new Hashtable(StringComparer.OrdinalIgnoreCase);
                            foreach (string str2 in strArray)
                            {
                                this._exclude[str2] = str2;
                            }
                        }
                    }
                    string[] virtualSubdirs = instance.GetVirtualSubdirs(this._virtualPath, true);
                    if (virtualSubdirs != null)
                    {
                        this._virtualPaths = new Hashtable(StringComparer.OrdinalIgnoreCase);
                        foreach (string str3 in virtualSubdirs)
                        {
                            VirtualPath path = this._virtualPath.SimpleCombineWithDir(str3);
                            if (FileUtil.DirectoryExists(instance.MapPath(null, path)))
                            {
                                this._virtualPaths[str3] = new MapPathBasedVirtualDirectory(path.VirtualPathString);
                            }
                        }
                        this._virtualEnumerator = this._virtualPaths.Values.GetEnumerator();
                    }
                }
            }
            this._fileEnumerator = FileEnumerator.Create(str);
            this._useFileEnumerator = false;
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        bool IEnumerator.MoveNext()
        {
            bool flag = false;
            if (this._virtualEnumerator != null)
            {
                flag = this._virtualEnumerator.MoveNext();
            }
            if (flag)
            {
                return flag;
            }
            this._useFileEnumerator = true;
        Label_0023:
            flag = this._fileEnumerator.MoveNext();
            if (flag)
            {
                FileData current = (FileData) this._fileEnumerator.Current;
                if (current.IsHidden)
                {
                    goto Label_0023;
                }
                if (current.IsDirectory)
                {
                    if (this._requestedEntryType != RequestedEntryType.Files)
                    {
                        string name = current.Name;
                        if ((((this._virtualPaths == null) || !this._virtualPaths.Contains(name)) && ((this._exclude == null) || !this._exclude.Contains(name))) && ((this._serverConfig2 == null) || this._serverConfig2.IsWithinApp(UrlPath.SimpleCombine(this._virtualPath.VirtualPathString, name))))
                        {
                            return flag;
                        }
                    }
                    goto Label_0023;
                }
                if (this._requestedEntryType == RequestedEntryType.Directories)
                {
                    goto Label_0023;
                }
            }
            return flag;
        }

        void IEnumerator.Reset()
        {
            throw new InvalidOperationException();
        }

        void IDisposable.Dispose()
        {
            if (this._fileEnumerator != null)
            {
                ((IDisposable) this._fileEnumerator).Dispose();
                this._fileEnumerator = null;
            }
        }

        internal VirtualFileBase Current
        {
            get
            {
                if (!this._useFileEnumerator)
                {
                    return (VirtualFileBase) this._virtualEnumerator.Current;
                }
                FileData current = (FileData) this._fileEnumerator.Current;
                if (current.IsDirectory)
                {
                    return new MapPathBasedVirtualDirectory(this._virtualPath.SimpleCombineWithDir(current.Name).VirtualPathString);
                }
                VirtualPath path = this._virtualPath.SimpleCombine(current.Name);
                return new MapPathBasedVirtualFile(path.VirtualPathString, current.FullName, current.GetFindFileData());
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return this.Current;
            }
        }
    }
}

