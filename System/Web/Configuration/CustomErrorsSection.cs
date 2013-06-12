namespace System.Web.Configuration
{
    using System;
    using System.Configuration;
    using System.Globalization;
    using System.Web;
    using System.Web.Util;
    using System.Xml;

    public sealed class CustomErrorsSection : ConfigurationSection
    {
        private static CustomErrorsSection _default = null;
        private string _DefaultAbsolutePath;
        private static readonly ConfigurationProperty _propDefaultRedirect = new ConfigurationProperty("defaultRedirect", typeof(string), null, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propErrors = new ConfigurationProperty(null, typeof(CustomErrorCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);
        private static ConfigurationPropertyCollection _properties = new ConfigurationPropertyCollection();
        private static readonly ConfigurationProperty _propMode = new ConfigurationProperty("mode", typeof(CustomErrorsMode), CustomErrorsMode.RemoteOnly, ConfigurationPropertyOptions.None);
        private static readonly ConfigurationProperty _propRedirectMode = new ConfigurationProperty("redirectMode", typeof(CustomErrorsRedirectMode), CustomErrorsRedirectMode.ResponseRedirect, ConfigurationPropertyOptions.None);
        private string basepath;

        static CustomErrorsSection()
        {
            _properties.Add(_propDefaultRedirect);
            _properties.Add(_propRedirectMode);
            _properties.Add(_propMode);
            _properties.Add(_propErrors);
        }

        internal bool CustomErrorsEnabled(HttpRequest request)
        {
            try
            {
                if (DeploymentSection.RetailInternal)
                {
                    return true;
                }
            }
            catch
            {
            }
            switch (this.Mode)
            {
                case CustomErrorsMode.RemoteOnly:
                    return !request.IsLocal;

                case CustomErrorsMode.On:
                    return true;

                case CustomErrorsMode.Off:
                    return false;
            }
            return false;
        }

        protected override void DeserializeSection(XmlReader reader)
        {
            base.DeserializeSection(reader);
            WebContext hostingContext = base.EvaluationContext.HostingContext as WebContext;
            if (hostingContext != null)
            {
                this.basepath = System.Web.Util.UrlPath.AppendSlashToPathIfNeeded(hostingContext.Path);
            }
        }

        internal static string GetAbsoluteRedirect(string path, string basePath)
        {
            if ((path != null) && System.Web.Util.UrlPath.IsRelativeUrl(path))
            {
                if (string.IsNullOrEmpty(basePath))
                {
                    basePath = "/";
                }
                path = System.Web.Util.UrlPath.Combine(basePath, path);
            }
            return path;
        }

        internal string GetRedirectString(int code)
        {
            string absoluteRedirect = null;
            if (this.Errors != null)
            {
                CustomError error = this.Errors[code.ToString(CultureInfo.InvariantCulture)];
                if (error != null)
                {
                    absoluteRedirect = GetAbsoluteRedirect(error.Redirect, this.basepath);
                }
            }
            if (absoluteRedirect == null)
            {
                absoluteRedirect = this.DefaultAbsolutePath;
            }
            return absoluteRedirect;
        }

        internal static CustomErrorsSection GetSettings(HttpContext context)
        {
            return GetSettings(context, false);
        }

        internal static CustomErrorsSection GetSettings(HttpContext context, bool canThrow)
        {
            CustomErrorsSection customErrors = null;
            RuntimeConfig lKGConfig = null;
            if (canThrow)
            {
                lKGConfig = RuntimeConfig.GetConfig(context);
                if (lKGConfig != null)
                {
                    customErrors = lKGConfig.CustomErrors;
                }
                return customErrors;
            }
            lKGConfig = RuntimeConfig.GetLKGConfig(context);
            if (lKGConfig != null)
            {
                customErrors = lKGConfig.CustomErrors;
            }
            if (customErrors != null)
            {
                return customErrors;
            }
            if (_default == null)
            {
                _default = new CustomErrorsSection();
            }
            return _default;
        }

        protected override void Reset(ConfigurationElement parentElement)
        {
            base.Reset(parentElement);
            CustomErrorsSection section = parentElement as CustomErrorsSection;
            if (section != null)
            {
                this.basepath = section.basepath;
            }
        }

        internal string DefaultAbsolutePath
        {
            get
            {
                if (this._DefaultAbsolutePath == null)
                {
                    this._DefaultAbsolutePath = GetAbsoluteRedirect(this.DefaultRedirect, this.basepath);
                }
                return this._DefaultAbsolutePath;
            }
        }

        [ConfigurationProperty("defaultRedirect")]
        public string DefaultRedirect
        {
            get
            {
                return (string) base[_propDefaultRedirect];
            }
            set
            {
                base[_propDefaultRedirect] = value;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public CustomErrorCollection Errors
        {
            get
            {
                return (CustomErrorCollection) base[_propErrors];
            }
        }

        [ConfigurationProperty("mode", DefaultValue=0)]
        public CustomErrorsMode Mode
        {
            get
            {
                return (CustomErrorsMode) base[_propMode];
            }
            set
            {
                base[_propMode] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        [ConfigurationProperty("redirectMode", DefaultValue=0)]
        public CustomErrorsRedirectMode RedirectMode
        {
            get
            {
                return (CustomErrorsRedirectMode) base[_propRedirectMode];
            }
            set
            {
                base[_propRedirectMode] = value;
            }
        }
    }
}

