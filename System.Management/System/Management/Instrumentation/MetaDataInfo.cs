namespace System.Management.Instrumentation
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text;

    internal class MetaDataInfo : IDisposable
    {
        private IMetaDataImportInternalOnly importInterface;
        private Guid mvid;
        private string name;

        public MetaDataInfo(Assembly assembly) : this(assembly.Location)
        {
        }

        public MetaDataInfo(string assemblyName)
        {
            Guid riid = new Guid(((GuidAttribute) Attribute.GetCustomAttribute(typeof(IMetaDataImportInternalOnly), typeof(GuidAttribute), false)).Value);
            IMetaDataDispenser o = (IMetaDataDispenser) new CorMetaDataDispenser();
            this.importInterface = (IMetaDataImportInternalOnly) o.OpenScope(assemblyName, 0, ref riid);
            Marshal.ReleaseComObject(o);
        }

        public void Dispose()
        {
            if (this.importInterface == null)
            {
                Marshal.ReleaseComObject(this.importInterface);
            }
            this.importInterface = null;
            GC.SuppressFinalize(this);
        }

        ~MetaDataInfo()
        {
            this.Dispose();
        }

        public static Guid GetMvid(Assembly assembly)
        {
            using (MetaDataInfo info = new MetaDataInfo(assembly))
            {
                return info.Mvid;
            }
        }

        private void InitNameAndMvid()
        {
            if (this.name == null)
            {
                uint num;
                StringBuilder szName = new StringBuilder {
                    Capacity = 0
                };
                this.importInterface.GetScopeProps(szName, (uint) szName.Capacity, out num, out this.mvid);
                szName.Capacity = (int) num;
                this.importInterface.GetScopeProps(szName, (uint) szName.Capacity, out num, out this.mvid);
                this.name = szName.ToString();
            }
        }

        public Guid Mvid
        {
            get
            {
                this.InitNameAndMvid();
                return this.mvid;
            }
        }

        public string Name
        {
            get
            {
                this.InitNameAndMvid();
                return this.name;
            }
        }
    }
}

