namespace System.Web.UI
{
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web;

    internal sealed class CollectionBuilder : ControlBuilder
    {
        private bool _ignoreUnknownContent;
        private Type _itemType;

        internal CollectionBuilder(bool ignoreUnknownContent)
        {
            this._ignoreUnknownContent = ignoreUnknownContent;
        }

        public override void AppendLiteralString(string s)
        {
            if (!this._ignoreUnknownContent && !Util.IsWhiteSpaceString(s))
            {
                throw new HttpException(System.Web.SR.GetString("Literal_content_not_allowed", new object[] { base.ControlType.FullName, s.Trim() }));
            }
        }

        public override object BuildObject()
        {
            return this;
        }

        public override Type GetChildControlType(string tagName, IDictionary attribs)
        {
            Type c = base.Parser.MapStringToType(tagName, attribs);
            if ((this._itemType == null) || this._itemType.IsAssignableFrom(c))
            {
                return c;
            }
            if (this._ignoreUnknownContent)
            {
                return null;
            }
            string fullName = string.Empty;
            if (base.ControlType != null)
            {
                fullName = base.ControlType.FullName;
            }
            else
            {
                fullName = base.TagName;
            }
            throw new HttpException(System.Web.SR.GetString("Invalid_collection_item_type", new string[] { fullName, this._itemType.FullName, tagName, c.FullName }));
        }

        public override void Init(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string ID, IDictionary attribs)
        {
            base.Init(parser, parentBuilder, type, tagName, ID, attribs);
            PropertyInfo info = TargetFrameworkUtil.GetProperty(parentBuilder.ControlType, tagName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.IgnoreCase);
            base.SetControlType(info.PropertyType);
            BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance;
            info = TargetFrameworkUtil.GetProperty(base.ControlType, "Item", bindingAttr, null, null, new Type[] { typeof(int) }, null);
            if (info == null)
            {
                info = TargetFrameworkUtil.GetProperty(base.ControlType, "Item", bindingAttr);
            }
            if (info != null)
            {
                this._itemType = info.PropertyType;
            }
        }
    }
}

