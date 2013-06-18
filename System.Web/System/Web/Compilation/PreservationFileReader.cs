namespace System.Web.Compilation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;

    internal class PreservationFileReader
    {
        private DiskBuildResultCache _diskCache;
        private bool _precompilationMode;
        private XmlNode _root;
        private ArrayList _sourceDependencies;

        internal PreservationFileReader(DiskBuildResultCache diskCache, bool precompilationMode)
        {
            this._diskCache = diskCache;
            this._precompilationMode = precompilationMode;
        }

        internal string GetAttribute(string name)
        {
            return HandlerBase.RemoveAttribute(this._root, name);
        }

        internal BuildResult ReadBuildResultFromFile(VirtualPath virtualPath, string preservationFile, long hashCode, bool ensureIsUpToDate)
        {
            if (!FileUtil.FileExists(preservationFile))
            {
                return null;
            }
            BuildResult result = null;
            try
            {
                result = this.ReadFileInternal(virtualPath, preservationFile, hashCode, ensureIsUpToDate);
            }
            catch (SecurityException)
            {
                throw;
            }
            catch
            {
                if (!this._precompilationMode)
                {
                    Util.RemoveOrRenameFile(preservationFile);
                }
            }
            return result;
        }

        private void ReadDependencies()
        {
            IEnumerator enumerator = this._root.ChildNodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                string str;
                XmlNode current = (XmlNode) enumerator.Current;
                if (((current.NodeType == XmlNodeType.Element) && ((str = current.Name) != null)) && (str == "filedeps"))
                {
                    this._sourceDependencies = this.ReadDependencies(current, "filedep");
                }
            }
        }

        private ArrayList ReadDependencies(XmlNode parent, string tagName)
        {
            ArrayList list = new ArrayList();
            IEnumerator enumerator = parent.ChildNodes.GetEnumerator();
            while (enumerator.MoveNext())
            {
                XmlNode current = (XmlNode) enumerator.Current;
                if (current.NodeType == XmlNodeType.Element)
                {
                    if (!current.Name.Equals(tagName))
                    {
                        return list;
                    }
                    string str = HandlerBase.RemoveAttribute(current, "name");
                    if (str == null)
                    {
                        return null;
                    }
                    list.Add(str);
                }
            }
            return list;
        }

        private BuildResult ReadFileInternal(VirtualPath virtualPath, string preservationFile, long hashCode, bool ensureIsUpToDate)
        {
            XmlDocument document = new XmlDocument();
            document.Load(preservationFile);
            this._root = document.DocumentElement;
            if ((this._root == null) || (this._root.Name != "preserve"))
            {
                return null;
            }
            BuildResultTypeCode code = (BuildResultTypeCode) int.Parse(this.GetAttribute("resultType"), CultureInfo.InvariantCulture);
            if (virtualPath == null)
            {
                virtualPath = VirtualPath.Create(this.GetAttribute("virtualPath"));
            }
            long num = 0L;
            string str2 = null;
            if (!this._precompilationMode)
            {
                string attribute = this.GetAttribute("hash");
                if (attribute == null)
                {
                    return null;
                }
                num = long.Parse(attribute, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture);
                str2 = this.GetAttribute("filehash");
            }
            BuildResult result = BuildResult.CreateBuildResultFromCode(code, virtualPath);
            if (!this._precompilationMode)
            {
                this.ReadDependencies();
                if (this._sourceDependencies != null)
                {
                    result.SetVirtualPathDependencies(this._sourceDependencies);
                }
                result.VirtualPathDependenciesHash = str2;
                bool flag = false;
                if (!result.IsUpToDate(virtualPath, ensureIsUpToDate))
                {
                    flag = true;
                }
                else
                {
                    long num2 = result.ComputeHashCode(hashCode);
                    if ((num2 == 0L) || (num2 != num))
                    {
                        flag = true;
                    }
                }
                if (flag)
                {
                    bool gotLock = false;
                    try
                    {
                        CompilationLock.GetLock(ref gotLock);
                        result.RemoveOutOfDateResources(this);
                        File.Delete(preservationFile);
                    }
                    finally
                    {
                        if (gotLock)
                        {
                            CompilationLock.ReleaseLock();
                        }
                    }
                    return null;
                }
            }
            result.GetPreservedAttributes(this);
            return result;
        }

        internal DiskBuildResultCache DiskCache
        {
            get
            {
                return this._diskCache;
            }
        }
    }
}

