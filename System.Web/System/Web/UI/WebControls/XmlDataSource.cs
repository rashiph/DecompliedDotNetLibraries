namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Web;
    using System.Web.Caching;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.Web.Util;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    [Designer("System.Web.UI.Design.WebControls.XmlDataSourceDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), PersistChildren(false), ParseChildren(true), WebSysDisplayName("XmlDataSource_DisplayName"), DefaultProperty("DataFile"), WebSysDescription("XmlDataSource_Description"), DefaultEvent("Transforming"), ToolboxBitmap(typeof(XmlDataSource))]
    public class XmlDataSource : HierarchicalDataSourceControl, IDataSource, IListSource
    {
        private DataSourceCache _cache;
        private bool _cacheLookupDone;
        private string _data;
        private string _dataFile;
        private bool _disallowChanges;
        private string _transform;
        private XsltArgumentList _transformArgumentList;
        private string _transformFile;
        private ICollection _viewNames;
        private string _writeableDataFile;
        private XmlDocument _xmlDocument;
        private string _xPath;
        private const string DefaultViewName = "DefaultView";
        private static readonly object EventTransforming = new object();

        event EventHandler IDataSource.DataSourceChanged
        {
            add
            {
                this.DataSourceChanged += value;
            }
            remove
            {
                this.DataSourceChanged -= value;
            }
        }

        [WebCategory("Data"), WebSysDescription("XmlDataSource_Transforming")]
        public event EventHandler Transforming
        {
            add
            {
                base.Events.AddHandler(EventTransforming, value);
            }
            remove
            {
                base.Events.RemoveHandler(EventTransforming, value);
            }
        }

        internal string CreateCacheKey()
        {
            StringBuilder builder = new StringBuilder("u", 0x400);
            builder.Append(base.GetType().GetHashCode().ToString(CultureInfo.InvariantCulture));
            builder.Append(this.CacheDuration.ToString(CultureInfo.InvariantCulture));
            builder.Append(':');
            builder.Append(((int) this.CacheExpirationPolicy).ToString(CultureInfo.InvariantCulture));
            bool flag = false;
            if (!string.IsNullOrEmpty(this.CacheKeyContext))
            {
                builder.Append(':');
                builder.Append(this.CacheKeyContext);
            }
            if (this.DataFile.Length > 0)
            {
                builder.Append(':');
                builder.Append(this.DataFile);
            }
            else if (this.Data.Length > 0)
            {
                flag = true;
            }
            if (this.TransformFile.Length > 0)
            {
                builder.Append(':');
                builder.Append(this.TransformFile);
            }
            else if (this.Transform.Length > 0)
            {
                flag = true;
            }
            if (flag)
            {
                if (this.Page != null)
                {
                    builder.Append(':');
                    builder.Append(this.Page.GetType().AssemblyQualifiedName);
                }
                builder.Append(':');
                string uniqueID = this.UniqueID;
                if (string.IsNullOrEmpty(uniqueID))
                {
                    throw new InvalidOperationException(System.Web.SR.GetString("XmlDataSource_NeedUniqueIDForCache"));
                }
                builder.Append(uniqueID);
            }
            return builder.ToString();
        }

        protected override HierarchicalDataSourceView GetHierarchicalView(string viewPath)
        {
            return new XmlHierarchicalDataSourceView(this, viewPath);
        }

        private XmlReader GetReader(string path, string content, out CacheDependency cacheDependency)
        {
            if (path.Length != 0)
            {
                Uri uri;
                VirtualPath path2;
                string str;
                if (Uri.TryCreate(path, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp))
                {
                    if (!HttpRuntime.HasWebPermission(uri))
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("XmlDataSource_NoWebPermission", new object[] { uri.PathAndQuery, this.ID }));
                    }
                    cacheDependency = null;
                    if (AppSettings.RestrictXmlControls)
                    {
                        return new NoEntitiesXmlReader(path);
                    }
                    return new XmlTextReader(path);
                }
                base.ResolvePhysicalOrVirtualPath(path, out path2, out str);
                if ((path2 != null) && base.DesignMode)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("XmlDataSource_DesignTimeRelativePathsNotSupported", new object[] { this.ID }));
                }
                Stream datastream = base.OpenFileAndGetDependency(path2, str, out cacheDependency);
                if (AppSettings.RestrictXmlControls)
                {
                    return new NoEntitiesXmlReader(datastream);
                }
                return new XmlTextReader(datastream);
            }
            cacheDependency = null;
            content = content.Trim();
            if (content.Length == 0)
            {
                return null;
            }
            if (AppSettings.RestrictXmlControls)
            {
                return new NoEntitiesXmlReader(new StringReader(content));
            }
            return new XmlTextReader(new StringReader(content));
        }

        private string GetWriteableDataFile()
        {
            Uri uri;
            VirtualPath path;
            string str;
            if (this.DataFile.Length == 0)
            {
                return null;
            }
            if (Uri.TryCreate(this.DataFile, UriKind.Absolute, out uri) && (uri.Scheme == Uri.UriSchemeHttp))
            {
                return null;
            }
            if (!HostingEnvironment.UsingMapPathBasedVirtualPathProvider)
            {
                return null;
            }
            base.ResolvePhysicalOrVirtualPath(this.DataFile, out path, out str);
            if (str == null)
            {
                str = path.MapPathInternal(base.TemplateControlVirtualDirectory, true);
            }
            return str;
        }

        public XmlDocument GetXmlDocument()
        {
            string key = null;
            if (!this._cacheLookupDone && this.Cache.Enabled)
            {
                key = this.CreateCacheKey();
                this._xmlDocument = this.Cache.LoadDataFromCache(key) as XmlDocument;
                this._cacheLookupDone = true;
            }
            if (this._xmlDocument == null)
            {
                CacheDependency dependency;
                CacheDependency dependency2;
                this._xmlDocument = new XmlDocument();
                this.PopulateXmlDocument(this._xmlDocument, out dependency2, out dependency);
                if (key != null)
                {
                    CacheDependency dependency3;
                    if (dependency2 != null)
                    {
                        if (dependency != null)
                        {
                            AggregateCacheDependency dependency4 = new AggregateCacheDependency();
                            dependency4.Add(new CacheDependency[] { dependency2, dependency });
                            dependency3 = dependency4;
                        }
                        else
                        {
                            dependency3 = dependency2;
                        }
                    }
                    else
                    {
                        dependency3 = dependency;
                    }
                    this.Cache.SaveDataToCache(key, this._xmlDocument, dependency3);
                }
            }
            return this._xmlDocument;
        }

        protected virtual void OnTransforming(EventArgs e)
        {
            EventHandler handler = (EventHandler) base.Events[EventTransforming];
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void PopulateXmlDocument(XmlDocument document, out CacheDependency dataCacheDependency, out CacheDependency transformCacheDependency)
        {
            XmlReader stylesheet = null;
            XmlReader reader = null;
            XmlReader reader3 = null;
            try
            {
                this._disallowChanges = true;
                stylesheet = this.GetReader(this.TransformFile, this.Transform, out transformCacheDependency);
                if (stylesheet != null)
                {
                    XmlDocument document2 = new XmlDocument();
                    reader3 = this.GetReader(this.DataFile, this.Data, out dataCacheDependency);
                    document2.Load(reader3);
                    if (AppSettings.RestrictXmlControls)
                    {
                        ((XmlTextReader) stylesheet).DtdProcessing = DtdProcessing.Ignore;
                        XslCompiledTransform transform = new XslCompiledTransform();
                        transform.Load(stylesheet, null, null);
                        this.OnTransforming(EventArgs.Empty);
                        using (MemoryStream stream = new MemoryStream())
                        {
                            transform.Transform((IXPathNavigable) document2, this._transformArgumentList, (Stream) stream);
                            document.Load(stream);
                            return;
                        }
                    }
                    XslTransform transform2 = new XslTransform();
                    transform2.Load(stylesheet, null, null);
                    this.OnTransforming(EventArgs.Empty);
                    reader = transform2.Transform((IXPathNavigable) document2, this._transformArgumentList, (XmlResolver) null);
                    document.Load(reader);
                }
                else
                {
                    reader = this.GetReader(this.DataFile, this.Data, out dataCacheDependency);
                    document.Load(reader);
                }
            }
            finally
            {
                this._disallowChanges = false;
                if (reader != null)
                {
                    reader.Close();
                }
                if (reader3 != null)
                {
                    reader3.Close();
                }
                if (stylesheet != null)
                {
                    stylesheet.Close();
                }
            }
        }

        public void Save()
        {
            if (!this.IsModifiable)
            {
                throw new InvalidOperationException(System.Web.SR.GetString("XmlDataSource_SaveNotAllowed", new object[] { this.ID }));
            }
            string writeableDataFile = this.WriteableDataFile;
            HttpRuntime.CheckFilePermission(writeableDataFile, true);
            this.GetXmlDocument().Save(writeableDataFile);
        }

        IList IListSource.GetList()
        {
            if (base.DesignMode)
            {
                return null;
            }
            return ListSourceHelper.GetList(this);
        }

        DataSourceView IDataSource.GetView(string viewName)
        {
            if (viewName.Length == 0)
            {
                viewName = "DefaultView";
            }
            return new XmlDataSourceView(this, viewName);
        }

        ICollection IDataSource.GetViewNames()
        {
            if (this._viewNames == null)
            {
                this._viewNames = new string[] { "DefaultView" };
            }
            return this._viewNames;
        }

        private DataSourceCache Cache
        {
            get
            {
                if (this._cache == null)
                {
                    this._cache = new DataSourceCache();
                    this._cache.Enabled = true;
                }
                return this._cache;
            }
        }

        [DefaultValue(0), WebSysDescription("DataSourceCache_Duration"), TypeConverter(typeof(DataSourceCacheDurationConverter)), WebCategory("Cache")]
        public virtual int CacheDuration
        {
            get
            {
                return this.Cache.Duration;
            }
            set
            {
                this.Cache.Duration = value;
            }
        }

        [DefaultValue(0), WebSysDescription("DataSourceCache_ExpirationPolicy"), WebCategory("Cache")]
        public virtual DataSourceCacheExpiry CacheExpirationPolicy
        {
            get
            {
                return this.Cache.ExpirationPolicy;
            }
            set
            {
                this.Cache.ExpirationPolicy = value;
            }
        }

        [WebCategory("Cache"), DefaultValue(""), WebSysDescription("XmlDataSource_CacheKeyContext")]
        public virtual string CacheKeyContext
        {
            get
            {
                return (((string) this.ViewState["CacheKeyContext "]) ?? string.Empty);
            }
            set
            {
                this.ViewState["CacheKeyContext "] = value;
            }
        }

        [WebSysDescription("DataSourceCache_KeyDependency"), DefaultValue(""), WebCategory("Cache")]
        public virtual string CacheKeyDependency
        {
            get
            {
                return this.Cache.KeyDependency;
            }
            set
            {
                this.Cache.KeyDependency = value;
            }
        }

        [Editor("System.ComponentModel.Design.MultilineStringEditor,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), PersistenceMode(PersistenceMode.InnerProperty), TypeConverter("System.ComponentModel.MultilineStringConverter,System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"), WebCategory("Data"), WebSysDescription("XmlDataSource_Data")]
        public virtual string Data
        {
            get
            {
                if (this._data == null)
                {
                    return string.Empty;
                }
                return this._data;
            }
            set
            {
                if (value != null)
                {
                    value = value.Trim();
                }
                if (this.Data != value)
                {
                    if (this._disallowChanges)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("XmlDataSource_CannotChangeWhileLoading", new object[] { "Data", this.ID }));
                    }
                    this._data = value;
                    this._xmlDocument = null;
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        [WebCategory("Data"), Editor("System.Web.UI.Design.XmlDataFileEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), WebSysDescription("XmlDataSource_DataFile")]
        public virtual string DataFile
        {
            get
            {
                if (this._dataFile == null)
                {
                    return string.Empty;
                }
                return this._dataFile;
            }
            set
            {
                if (this.DataFile != value)
                {
                    if (this._disallowChanges)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("XmlDataSource_CannotChangeWhileLoading", new object[] { "DataFile", this.ID }));
                    }
                    this._dataFile = value;
                    this._xmlDocument = null;
                    this._writeableDataFile = null;
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        [DefaultValue(true), WebCategory("Cache"), WebSysDescription("DataSourceCache_Enabled")]
        public virtual bool EnableCaching
        {
            get
            {
                return this.Cache.Enabled;
            }
            set
            {
                this.Cache.Enabled = value;
            }
        }

        internal bool IsModifiable
        {
            get
            {
                return ((string.IsNullOrEmpty(this.TransformFile) && string.IsNullOrEmpty(this.Transform)) && !string.IsNullOrEmpty(this.WriteableDataFile));
            }
        }

        bool IListSource.ContainsListCollection
        {
            get
            {
                if (base.DesignMode)
                {
                    return false;
                }
                return ListSourceHelper.ContainsListCollection(this);
            }
        }

        [DefaultValue(""), Editor("System.ComponentModel.Design.MultilineStringEditor,System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), PersistenceMode(PersistenceMode.InnerProperty), TypeConverter("System.ComponentModel.MultilineStringConverter,System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"), WebCategory("Data"), WebSysDescription("XmlDataSource_Transform")]
        public virtual string Transform
        {
            get
            {
                if (this._transform == null)
                {
                    return string.Empty;
                }
                return this._transform;
            }
            set
            {
                if (value != null)
                {
                    value = value.Trim();
                }
                if (this.Transform != value)
                {
                    if (this._disallowChanges)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("XmlDataSource_CannotChangeWhileLoading", new object[] { "Transform", this.ID }));
                    }
                    this._transform = value;
                    this._xmlDocument = null;
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        [Browsable(false)]
        public virtual XsltArgumentList TransformArgumentList
        {
            get
            {
                return this._transformArgumentList;
            }
            set
            {
                this._transformArgumentList = value;
            }
        }

        [Editor("System.Web.UI.Design.XslTransformFileEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), WebCategory("Data"), WebSysDescription("XmlDataSource_TransformFile")]
        public virtual string TransformFile
        {
            get
            {
                if (this._transformFile == null)
                {
                    return string.Empty;
                }
                return this._transformFile;
            }
            set
            {
                if (this.TransformFile != value)
                {
                    if (this._disallowChanges)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("XmlDataSource_CannotChangeWhileLoading", new object[] { "TransformFile", this.ID }));
                    }
                    this._transformFile = value;
                    this._xmlDocument = null;
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }

        private string WriteableDataFile
        {
            get
            {
                if (this._writeableDataFile == null)
                {
                    this._writeableDataFile = this.GetWriteableDataFile();
                }
                return this._writeableDataFile;
            }
        }

        [WebCategory("Data"), DefaultValue(""), WebSysDescription("XmlDataSource_XPath")]
        public virtual string XPath
        {
            get
            {
                if (this._xPath == null)
                {
                    return string.Empty;
                }
                return this._xPath;
            }
            set
            {
                if (this.XPath != value)
                {
                    if (this._disallowChanges)
                    {
                        throw new InvalidOperationException(System.Web.SR.GetString("XmlDataSource_CannotChangeWhileLoading", new object[] { "XPath", this.ID }));
                    }
                    this._xPath = value;
                    this.OnDataSourceChanged(EventArgs.Empty);
                }
            }
        }
    }
}

