namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;

    internal abstract class Source : DataSourceComponent, IDataSourceNamedObject, INamedObject, ICloneable
    {
        private string generatorGetMethodName;
        private string generatorGetMethodNameForPaging;
        private string generatorSourceName;
        private string generatorSourceNameForPaging;
        private MemberAttributes modifier = MemberAttributes.Public;
        protected string name;
        protected DataSourceComponent owner;
        private string userSourceName;
        private bool webMethod;
        private string webMethodDescription;

        internal Source()
        {
        }

        public abstract object Clone();
        internal virtual bool NameExist(string nameToCheck)
        {
            return StringUtil.EqualValue(this.Name, nameToCheck, true);
        }

        public override void SetCollection(DataSourceCollectionBase collection)
        {
            base.SetCollection(collection);
            if (collection != null)
            {
                this.Owner = collection.CollectionHost;
            }
            else
            {
                this.Owner = null;
            }
        }

        public override string ToString()
        {
            return (this.PublicTypeName + " " + this.DisplayName);
        }

        internal virtual string DisplayName
        {
            get
            {
                return this.Name;
            }
            set
            {
            }
        }

        [DataSourceXmlAttribute, DefaultValue(false)]
        public bool EnableWebMethods
        {
            get
            {
                return this.webMethod;
            }
            set
            {
                this.webMethod = value;
            }
        }

        [Browsable(false), DefaultValue((string) null), DataSourceXmlAttribute]
        public string GeneratorGetMethodName
        {
            get
            {
                return this.generatorGetMethodName;
            }
            set
            {
                this.generatorGetMethodName = value;
            }
        }

        [Browsable(false), DefaultValue((string) null), DataSourceXmlAttribute]
        public string GeneratorGetMethodNameForPaging
        {
            get
            {
                return this.generatorGetMethodNameForPaging;
            }
            set
            {
                this.generatorGetMethodNameForPaging = value;
            }
        }

        [Browsable(false)]
        public override string GeneratorName
        {
            get
            {
                return this.GeneratorSourceName;
            }
        }

        [DefaultValue((string) null), DataSourceXmlAttribute, Browsable(false)]
        public string GeneratorSourceName
        {
            get
            {
                return this.generatorSourceName;
            }
            set
            {
                this.generatorSourceName = value;
            }
        }

        [DefaultValue((string) null), DataSourceXmlAttribute, Browsable(false)]
        public string GeneratorSourceNameForPaging
        {
            get
            {
                return this.generatorSourceNameForPaging;
            }
            set
            {
                this.generatorSourceNameForPaging = value;
            }
        }

        internal bool IsMainSource
        {
            get
            {
                DesignTable owner = this.Owner as DesignTable;
                return ((owner != null) && (owner.MainSource == this));
            }
        }

        [DefaultValue(0x6000), DataSourceXmlAttribute]
        public MemberAttributes Modifier
        {
            get
            {
                return this.modifier;
            }
            set
            {
                this.modifier = value;
            }
        }

        [MergableProperty(false), DataSourceXmlAttribute, DefaultValue("")]
        public virtual string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                if (this.name != value)
                {
                    if (this.CollectionParent != null)
                    {
                        this.CollectionParent.ValidateUniqueName(this, value);
                    }
                    this.name = value;
                }
            }
        }

        [Browsable(false)]
        internal DataSourceComponent Owner
        {
            get
            {
                if ((this.owner == null) && (this.CollectionParent != null))
                {
                    SourceCollection collectionParent = this.CollectionParent as SourceCollection;
                    if (collectionParent != null)
                    {
                        this.owner = collectionParent.CollectionHost;
                    }
                }
                return this.owner;
            }
            set
            {
                this.owner = value;
            }
        }

        [Browsable(false)]
        public virtual string PublicTypeName
        {
            get
            {
                return "Function";
            }
        }

        [DefaultValue((string) null), Browsable(false), DataSourceXmlAttribute]
        public string UserSourceName
        {
            get
            {
                return this.userSourceName;
            }
            set
            {
                this.userSourceName = value;
            }
        }

        [DefaultValue(""), DataSourceXmlAttribute]
        public string WebMethodDescription
        {
            get
            {
                return this.webMethodDescription;
            }
            set
            {
                this.webMethodDescription = value;
            }
        }
    }
}

