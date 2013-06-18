namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.Util;

    internal sealed class MasterPageParser : UserControlParser
    {
        private Type _masterPageType;
        private CaseInsensitiveStringSet _placeHolderList;
        internal const string defaultDirectiveName = "master";

        internal override void ApplyBaseType()
        {
        }

        internal override RootBuilder CreateDefaultFileLevelBuilder()
        {
            return new FileLevelMasterPageControlBuilder();
        }

        internal override void ProcessDirective(string directiveName, IDictionary directive)
        {
            if (StringUtil.EqualsIgnoreCase(directiveName, "masterType"))
            {
                if (this._masterPageType != null)
                {
                    base.ProcessError(System.Web.SR.GetString("Only_one_directive_allowed", new object[] { directiveName }));
                }
                else
                {
                    this._masterPageType = base.GetDirectiveType(directive, directiveName);
                    Util.CheckAssignableType(typeof(MasterPage), this._masterPageType);
                }
            }
            else if (StringUtil.EqualsIgnoreCase(directiveName, "outputcache"))
            {
                base.ProcessError(System.Web.SR.GetString("Directive_not_allowed", new object[] { directiveName }));
            }
            else
            {
                base.ProcessDirective(directiveName, directive);
            }
        }

        internal override bool ProcessMainDirectiveAttribute(string deviceName, string name, string value, IDictionary parseData)
        {
            switch (name)
            {
                case "masterpagefile":
                    if (!base.IsExpressionBuilderValue(value) && (value.Length > 0))
                    {
                        Type referencedType = base.GetReferencedType(value);
                        Util.CheckAssignableType(typeof(MasterPage), referencedType);
                    }
                    return false;

                case "outputcaching":
                    base.ProcessError(System.Web.SR.GetString("Attr_not_supported_in_directive", new object[] { name, this.DefaultDirectiveName }));
                    return false;
            }
            return base.ProcessMainDirectiveAttribute(deviceName, name, value, parseData);
        }

        internal override Type DefaultBaseType
        {
            get
            {
                return typeof(MasterPage);
            }
        }

        internal override string DefaultDirectiveName
        {
            get
            {
                return "master";
            }
        }

        internal override Type DefaultFileLevelBuilderType
        {
            get
            {
                return typeof(FileLevelMasterPageControlBuilder);
            }
        }

        internal Type MasterPageType
        {
            get
            {
                return this._masterPageType;
            }
        }

        internal CaseInsensitiveStringSet PlaceHolderList
        {
            get
            {
                if (this._placeHolderList == null)
                {
                    this._placeHolderList = new CaseInsensitiveStringSet();
                }
                return this._placeHolderList;
            }
        }
    }
}

