namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Security.Permissions;
    using System.Web;
    using System.Web.Util;

    public sealed class HttpHandlerAction : ConfigurationElement
    {
        private WildcardUrl _path;
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propPath = new ConfigurationProperty("path", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propType = new ConfigurationProperty("type", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsTypeStringTransformationRequired | ConfigurationPropertyOptions.IsRequired);
        private static readonly ConfigurationProperty _propValidate = new ConfigurationProperty("validate", typeof(bool), true, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propVerb = new ConfigurationProperty("verb", typeof(string), null, null, StdValidatorsAndConverters.NonEmptyStringValidator, ConfigurationPropertyOptions.IsKey | ConfigurationPropertyOptions.IsRequired);
        private Wildcard _requestType;
        private System.Type _type;
        private string typeCache;

        static HttpHandlerAction()
        {
            _properties.Add(_propPath);
            _properties.Add(_propVerb);
            _properties.Add(_propType);
            _properties.Add(_propValidate);
        }

        internal HttpHandlerAction()
        {
        }

        public HttpHandlerAction(string path, string type, string verb) : this(path, type, verb, true)
        {
        }

        public HttpHandlerAction(string path, string type, string verb, bool validate)
        {
            this.Path = path;
            this.Type = type;
            this.Verb = verb;
            this.Validate = validate;
        }

        internal object Create()
        {
            if (this._type == null)
            {
                System.Type t = ConfigUtil.GetType(this.Type, "type", this);
                if (!ConfigUtil.IsTypeHandlerOrFactory(t))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Type_not_factory_or_handler", new object[] { this.Type }), base.ElementInformation.Source, base.ElementInformation.LineNumber);
                }
                this._type = t;
            }
            return HttpRuntime.CreateNonPublicInstance(this._type);
        }

        [FileIOPermission(SecurityAction.Assert, AllFiles=FileIOPermissionAccess.PathDiscovery | FileIOPermissionAccess.Read)]
        internal void InitValidateInternal()
        {
            string pattern = this.Verb.Replace(" ", string.Empty);
            this._requestType = new Wildcard(pattern, false);
            this._path = new WildcardUrl(this.Path, true);
            if (!this.Validate)
            {
                this._type = null;
            }
            else
            {
                this._type = ConfigUtil.GetType(this.Type, "type", this);
                if (!ConfigUtil.IsTypeHandlerOrFactory(this._type))
                {
                    throw new ConfigurationErrorsException(System.Web.SR.GetString("Type_not_factory_or_handler", new object[] { this.Type }), base.ElementInformation.Source, base.ElementInformation.LineNumber);
                }
            }
        }

        internal bool IsMatch(string verb, VirtualPath path)
        {
            return (this._path.IsSuffix(path.VirtualPathString) && this._requestType.IsMatch(verb));
        }

        internal string Key
        {
            get
            {
                return ("verb=" + this.Verb + " | path=" + this.Path);
            }
        }

        [ConfigurationProperty("path", IsRequired=true, IsKey=true)]
        public string Path
        {
            get
            {
                return (string) base[_propPath];
            }
            set
            {
                base[_propPath] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("type", IsRequired=true)]
        public string Type
        {
            get
            {
                if (this.typeCache == null)
                {
                    this.typeCache = (string) base[_propType];
                }
                return this.typeCache;
            }
            set
            {
                base[_propType] = value;
                this.typeCache = value;
            }
        }

        internal System.Type TypeInternal
        {
            get
            {
                return this._type;
            }
        }

        [ConfigurationProperty("validate", DefaultValue=true)]
        public bool Validate
        {
            get
            {
                return (bool) base[_propValidate];
            }
            set
            {
                base[_propValidate] = value;
            }
        }

        [ConfigurationProperty("verb", IsRequired=true, IsKey=true)]
        public string Verb
        {
            get
            {
                return (string) base[_propVerb];
            }
            set
            {
                base[_propVerb] = value;
            }
        }
    }
}

