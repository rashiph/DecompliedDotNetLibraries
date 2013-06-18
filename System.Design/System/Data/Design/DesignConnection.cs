namespace System.Data.Design
{
    using System;
    using System.CodeDom;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Design;
    using System.Xml;

    [DataSourceXmlClass("Connection")]
    internal class DesignConnection : DataSourceComponent, IDesignConnection, IDataSourceNamedObject, INamedObject, ICloneable, IDataSourceInitAfterLoading, IDataSourceXmlSpecialOwner, IDataSourceCollectionMember
    {
        private string appSettingsObjectName;
        private System.Data.Design.ConnectionString connectionStringObject;
        private string connectionStringValue;
        private bool isAppSettingsProperty;
        private MemberAttributes modifier;
        private string name;
        private string parameterPrefix;
        private HybridDictionary properties;
        private CodePropertyReferenceExpression propertyReference;
        private string provider;
        private static readonly string regexAlphaCharacter = @"[\p{L}\p{Nl}]";
        private static readonly string regexIdentifier = (regexIdentifierStart + regexIdentifierCharacter + "*");
        private static readonly string regexIdentifierCharacter = @"[\p{L}\p{Nl}\p{Nd}\p{Mn}\p{Mc}\p{Cf}]";
        private static readonly string regexIdentifierStart = ("(" + regexAlphaCharacter + "|(" + regexUnderscoreCharacter + regexIdentifierCharacter + "))");
        private static readonly string regexUnderscoreCharacter = @"\p{Pc}";

        public DesignConnection()
        {
            this.properties = new HybridDictionary();
            this.modifier = MemberAttributes.Assembly;
        }

        public DesignConnection(string connectionName, IDbConnection conn)
        {
            this.properties = new HybridDictionary();
            this.modifier = MemberAttributes.Assembly;
            if (conn == null)
            {
                throw new ArgumentNullException("conn");
            }
            this.name = connectionName;
            DbProviderFactory factoryFromType = ProviderManager.GetFactoryFromType(conn.GetType(), ProviderManager.ProviderSupportedClasses.DbConnection);
            this.provider = ProviderManager.GetInvariantProviderName(factoryFromType);
            this.connectionStringObject = new System.Data.Design.ConnectionString(this.provider, conn.ConnectionString);
        }

        public DesignConnection(string connectionName, System.Data.Design.ConnectionString cs, string provider)
        {
            this.properties = new HybridDictionary();
            this.modifier = MemberAttributes.Assembly;
            this.name = connectionName;
            this.connectionStringObject = cs;
            this.provider = provider;
        }

        public object Clone()
        {
            DesignConnection connection = new DesignConnection {
                Name = this.name
            };
            if (this.ConnectionStringObject != null)
            {
                connection.ConnectionStringObject = (System.Data.Design.ConnectionString) ((ICloneable) this.ConnectionStringObject).Clone();
            }
            connection.provider = this.provider;
            connection.isAppSettingsProperty = this.isAppSettingsProperty;
            connection.propertyReference = this.propertyReference;
            connection.properties = (HybridDictionary) DesignUtil.CloneDictionary(this.properties);
            return connection;
        }

        public IDbConnection CreateEmptyDbConnection()
        {
            return ProviderManager.GetFactory(this.provider).CreateConnection();
        }

        void IDataSourceInitAfterLoading.InitializeAfterLoading()
        {
            if ((this.name == null) || (this.name.Length == 0))
            {
                throw new DataSourceSerializationException(System.Design.SR.GetString("DTDS_NameIsRequired", new object[] { "Connection" }));
            }
            if (StringUtil.EmptyOrSpace(this.provider))
            {
                throw new DataSourceSerializationException(System.Design.SR.GetString("DTDS_CouldNotDeserializeConnection"));
            }
            if (this.connectionStringValue != null)
            {
                this.ConnectionStringObject = new System.Data.Design.ConnectionString(this.provider, this.connectionStringValue);
            }
            this.properties.Clear();
        }

        void IDataSourceXmlSpecialOwner.ReadSpecialItem(string propertyName, XmlNode xmlNode, DataSourceXmlSerializer serializer)
        {
            if (propertyName == "ConnectionStringObject")
            {
                this.connectionStringValue = xmlNode.InnerText;
            }
            else if (propertyName == "PropertyReference")
            {
                this.propertyReference = PropertyReferenceSerializer.Deserialize(xmlNode.InnerText);
            }
        }

        void IDataSourceXmlSpecialOwner.WriteSpecialItem(string propertyName, XmlWriter writer, DataSourceXmlSerializer serializer)
        {
            if (propertyName == "ConnectionStringObject")
            {
                writer.WriteString(this.ConnectionStringObject.ToFullString());
            }
            else if (propertyName == "PropertyReference")
            {
                writer.WriteString(PropertyReferenceSerializer.Serialize(this.PropertyReference));
            }
        }

        [Browsable(false), DataSourceXmlAttribute]
        public string AppSettingsObjectName
        {
            get
            {
                return this.appSettingsObjectName;
            }
            set
            {
                this.appSettingsObjectName = value;
            }
        }

        internal static string ConnectionNameRegex
        {
            get
            {
                return regexIdentifier;
            }
        }

        public string ConnectionString
        {
            get
            {
                if (this.ConnectionStringObject != null)
                {
                    return this.ConnectionStringObject.ToString();
                }
                return string.Empty;
            }
            set
            {
                if (this.ConnectionStringObject != null)
                {
                    this.ConnectionStringObject = new System.Data.Design.ConnectionString(this.provider, value);
                }
            }
        }

        [Browsable(false), DataSourceXmlAttribute(SpecialWay=true)]
        public System.Data.Design.ConnectionString ConnectionStringObject
        {
            get
            {
                return this.connectionStringObject;
            }
            set
            {
                this.connectionStringObject = value;
            }
        }

        [Browsable(false), DataSourceXmlAttribute]
        public bool IsAppSettingsProperty
        {
            get
            {
                return this.isAppSettingsProperty;
            }
            set
            {
                this.isAppSettingsProperty = value;
            }
        }

        [DataSourceXmlAttribute, DefaultValue(0x1000)]
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

        [DataSourceXmlAttribute]
        public string Name
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

        [DataSourceXmlAttribute, Browsable(false)]
        public string ParameterPrefix
        {
            get
            {
                return this.parameterPrefix;
            }
            set
            {
                this.parameterPrefix = value;
            }
        }

        [Browsable(false)]
        public IDictionary Properties
        {
            get
            {
                return this.properties;
            }
        }

        [Browsable(false), DataSourceXmlAttribute(SpecialWay=true)]
        public CodePropertyReferenceExpression PropertyReference
        {
            get
            {
                return this.propertyReference;
            }
            set
            {
                this.propertyReference = value;
            }
        }

        [Browsable(false), DataSourceXmlAttribute]
        public string Provider
        {
            get
            {
                return this.provider;
            }
            set
            {
                this.provider = value;
            }
        }

        [Browsable(false)]
        public string PublicTypeName
        {
            get
            {
                return "Connection";
            }
        }
    }
}

