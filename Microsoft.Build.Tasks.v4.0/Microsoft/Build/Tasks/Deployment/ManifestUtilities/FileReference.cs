namespace Microsoft.Build.Tasks.Deployment.ManifestUtilities
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Xml.Serialization;

    [ComVisible(false)]
    public sealed class FileReference : BaseReference
    {
        private ComClass[] comClasses;
        private ProxyStub[] proxyStubs;
        private TypeLib[] typeLibs;
        private string writeableType;

        public FileReference()
        {
        }

        public FileReference(string path) : base(path)
        {
        }

        internal bool ImportComComponent(string path, OutputMessageCollection outputMessages, string outputDisplayName)
        {
            ComImporter importer = new ComImporter(path, outputMessages, outputDisplayName);
            if (importer.Success)
            {
                ArrayList list = new ArrayList();
                if (this.typeLibs != null)
                {
                    list.AddRange(this.typeLibs);
                }
                if (importer.TypeLib != null)
                {
                    list.Add(importer.TypeLib);
                }
                this.typeLibs = (TypeLib[]) list.ToArray(typeof(TypeLib));
                list.Clear();
                if (this.comClasses != null)
                {
                    list.AddRange(this.comClasses);
                }
                if (importer.ComClasses != null)
                {
                    list.AddRange(importer.ComClasses);
                }
                this.comClasses = (ComClass[]) list.ToArray(typeof(ComClass));
            }
            return importer.Success;
        }

        [XmlIgnore]
        public ComClass[] ComClasses
        {
            get
            {
                return this.comClasses;
            }
        }

        [XmlIgnore]
        public bool IsDataFile
        {
            get
            {
                return (string.Compare(this.writeableType, "applicationData", StringComparison.OrdinalIgnoreCase) == 0);
            }
            set
            {
                this.writeableType = value ? "applicationData" : null;
            }
        }

        [XmlIgnore]
        public ProxyStub[] ProxyStubs
        {
            get
            {
                return this.proxyStubs;
            }
        }

        protected internal override string SortName
        {
            get
            {
                return base.TargetPath;
            }
        }

        [XmlIgnore]
        public TypeLib[] TypeLibs
        {
            get
            {
                return this.typeLibs;
            }
        }

        [XmlArray("ComClasses"), Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public ComClass[] XmlComClasses
        {
            get
            {
                return this.comClasses;
            }
            set
            {
                this.comClasses = value;
            }
        }

        [XmlArray("ProxyStubs"), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public ProxyStub[] XmlProxyStubs
        {
            get
            {
                return this.proxyStubs;
            }
            set
            {
                this.proxyStubs = value;
            }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), XmlArray("TypeLibs")]
        public TypeLib[] XmlTypeLibs
        {
            get
            {
                return this.typeLibs;
            }
            set
            {
                this.typeLibs = value;
            }
        }

        [XmlAttribute("WriteableType"), EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public string XmlWriteableType
        {
            get
            {
                return this.writeableType;
            }
            set
            {
                this.writeableType = value;
            }
        }
    }
}

