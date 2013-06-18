namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Web.Caching;

    internal class UserControlParser : TemplateControlParser
    {
        private bool _fSharedPartialCaching;
        private string _provider;
        internal const string defaultDirectiveName = "control";

        internal virtual void ApplyBaseType()
        {
            if (PageParser.DefaultUserControlBaseType != null)
            {
                base.BaseType = PageParser.DefaultUserControlBaseType;
            }
            else if ((base.PagesConfig != null) && (base.PagesConfig.UserControlBaseTypeInternal != null))
            {
                base.BaseType = base.PagesConfig.UserControlBaseTypeInternal;
            }
        }

        internal override RootBuilder CreateDefaultFileLevelBuilder()
        {
            return new FileLevelUserControlBuilder();
        }

        internal override void ProcessConfigSettings()
        {
            base.ProcessConfigSettings();
            this.ApplyBaseType();
        }

        internal override void ProcessOutputCacheDirective(string directiveName, IDictionary directive)
        {
            Util.GetAndRemoveBooleanAttribute(directive, "shared", ref this._fSharedPartialCaching);
            this._provider = Util.GetAndRemoveNonEmptyAttribute(directive, "providerName");
            if (this._provider == "AspNetInternalProvider")
            {
                this._provider = null;
            }
            OutputCache.ThrowIfProviderNotFound(this._provider);
            string andRemoveNonEmptyAttribute = Util.GetAndRemoveNonEmptyAttribute(directive, "sqldependency");
            if (andRemoveNonEmptyAttribute != null)
            {
                SqlCacheDependency.ValidateOutputCacheDependencyString(andRemoveNonEmptyAttribute, false);
                base.OutputCacheParameters.SqlDependency = andRemoveNonEmptyAttribute;
            }
            base.ProcessOutputCacheDirective(directiveName, directive);
        }

        internal override Type DefaultBaseType
        {
            get
            {
                return typeof(UserControl);
            }
        }

        internal override string DefaultDirectiveName
        {
            get
            {
                return "control";
            }
        }

        internal override Type DefaultFileLevelBuilderType
        {
            get
            {
                return typeof(FileLevelUserControlBuilder);
            }
        }

        internal bool FSharedPartialCaching
        {
            get
            {
                return this._fSharedPartialCaching;
            }
        }

        internal override bool FVaryByParamsRequiredOnOutputCache
        {
            get
            {
                return (base.OutputCacheParameters.VaryByControl == null);
            }
        }

        internal string Provider
        {
            get
            {
                return this._provider;
            }
        }

        internal override string UnknownOutputCacheAttributeError
        {
            get
            {
                return "Attr_not_supported_in_ucdirective";
            }
        }
    }
}

