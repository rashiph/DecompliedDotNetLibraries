namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Data.Common;
    using System.Data.OleDb;
    using System.Drawing;
    using System.Drawing.Design;
    using System.IO;
    using System.Web;
    using System.Web.Caching;
    using System.Web.UI;
    using System.Web.Util;

    [Designer("System.Web.UI.Design.WebControls.AccessDataSourceDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), WebSysDisplayName("AccessDataSource_DisplayName"), WebSysDescription("AccessDataSource_Description"), ToolboxBitmap(typeof(AccessDataSource))]
    public class AccessDataSource : SqlDataSource
    {
        private System.Web.UI.FileDataSourceCache _cache;
        private string _connectionString;
        private string _dataFile;
        private string _physicalDataFile;
        private const string Access2007FileExtension = ".accdb";
        private const string Access2007Provider = "Microsoft.ACE.OLEDB.12.0";
        private const string JetProvider = "Microsoft.Jet.OLEDB.4.0";
        private const string OleDbProviderName = "System.Data.OleDb";

        public AccessDataSource()
        {
        }

        public AccessDataSource(string dataFile, string selectCommand)
        {
            if (string.IsNullOrEmpty(dataFile))
            {
                throw new ArgumentNullException("dataFile");
            }
            this.DataFile = dataFile;
            base.SelectCommand = selectCommand;
        }

        private void AddCacheFileDependency()
        {
            this.FileDataSourceCache.FileDependencies.Clear();
            string physicalDataFile = this.PhysicalDataFile;
            if (physicalDataFile.Length > 0)
            {
                this.FileDataSourceCache.FileDependencies.Add(physicalDataFile);
            }
        }

        private string CreateConnectionString()
        {
            return ("Provider=" + this.NativeProvider + "; Data Source=" + this.PhysicalDataFile);
        }

        protected override SqlDataSourceView CreateDataSourceView(string viewName)
        {
            return new AccessDataSourceView(this, viewName, this.Context);
        }

        protected override DbProviderFactory GetDbProviderFactory()
        {
            return OleDbFactory.Instance;
        }

        private string GetPhysicalDataFilePath()
        {
            string dataFile = this.DataFile;
            if (dataFile.Length == 0)
            {
                return null;
            }
            if (!UrlPath.IsAbsolutePhysicalPath(dataFile))
            {
                if (base.DesignMode)
                {
                    throw new NotSupportedException(System.Web.SR.GetString("AccessDataSource_DesignTimeRelativePathsNotSupported", new object[] { this.ID }));
                }
                dataFile = this.Context.Request.MapPath(dataFile, base.AppRelativeTemplateSourceDirectory, true);
            }
            HttpRuntime.CheckFilePermission(dataFile, true);
            if (!HttpRuntime.HasPathDiscoveryPermission(dataFile))
            {
                throw new HttpException(System.Web.SR.GetString("AccessDataSource_NoPathDiscoveryPermission", new object[] { HttpRuntime.GetSafePath(dataFile), this.ID }));
            }
            return dataFile;
        }

        internal override void SaveDataToCache(int startRowIndex, int maximumRows, object data, CacheDependency dependency)
        {
            this.AddCacheFileDependency();
            base.SaveDataToCache(startRowIndex, maximumRows, data, dependency);
        }

        internal override DataSourceCache Cache
        {
            get
            {
                if (this._cache == null)
                {
                    this._cache = new System.Web.UI.FileDataSourceCache();
                }
                return this._cache;
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string ConnectionString
        {
            get
            {
                if (this._connectionString == null)
                {
                    this._connectionString = this.CreateConnectionString();
                }
                return this._connectionString;
            }
            set
            {
                throw new InvalidOperationException(System.Web.SR.GetString("AccessDataSource_CannotSetConnectionString"));
            }
        }

        [UrlProperty, Editor("System.Web.UI.Design.MdbDataFileEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor)), DefaultValue(""), WebCategory("Data"), WebSysDescription("AccessDataSource_DataFile")]
        public string DataFile
        {
            get
            {
                if (this._dataFile != null)
                {
                    return this._dataFile;
                }
                return string.Empty;
            }
            set
            {
                if (this.DataFile != value)
                {
                    this._dataFile = value;
                    this._connectionString = null;
                    this._physicalDataFile = null;
                    this.RaiseDataSourceChangedEvent(EventArgs.Empty);
                }
            }
        }

        private System.Web.UI.FileDataSourceCache FileDataSourceCache
        {
            get
            {
                return (this.Cache as System.Web.UI.FileDataSourceCache);
            }
        }

        internal virtual bool IsAccess2007
        {
            get
            {
                return (Path.GetExtension(this.PhysicalDataFile) == ".accdb");
            }
        }

        internal string NativeProvider
        {
            get
            {
                if (this.IsAccess2007)
                {
                    return "Microsoft.ACE.OLEDB.12.0";
                }
                return "Microsoft.Jet.OLEDB.4.0";
            }
        }

        private string PhysicalDataFile
        {
            get
            {
                if (this._physicalDataFile == null)
                {
                    this._physicalDataFile = this.GetPhysicalDataFilePath();
                }
                return this._physicalDataFile;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false)]
        public override string ProviderName
        {
            get
            {
                return "System.Data.OleDb";
            }
            set
            {
                throw new InvalidOperationException(System.Web.SR.GetString("AccessDataSource_CannotSetProvider", new object[] { this.ID }));
            }
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override string SqlCacheDependency
        {
            get
            {
                throw new NotSupportedException(System.Web.SR.GetString("AccessDataSource_SqlCacheDependencyNotSupported", new object[] { this.ID }));
            }
            set
            {
                throw new NotSupportedException(System.Web.SR.GetString("AccessDataSource_SqlCacheDependencyNotSupported", new object[] { this.ID }));
            }
        }
    }
}

