namespace System.Web.UI.WebControls
{
    using System;
    using System.Collections;
    using System.Web;
    using System.Web.UI;

    internal class ContentPlaceHolderBuilder : ControlBuilder
    {
        private string _contentPlaceHolderID;
        private string _templateName;

        internal override void BuildChildren(object parentObj)
        {
            MasterPage templateControl = base.TemplateControl as MasterPage;
            if (!this.PageProvidesMatchingContent(templateControl))
            {
                base.BuildChildren(parentObj);
            }
        }

        public override object BuildObject()
        {
            MasterPage templateControl = base.TemplateControl as MasterPage;
            ContentPlaceHolder contentPlaceHolder = (ContentPlaceHolder) base.BuildObject();
            if (this.PageProvidesMatchingContent(templateControl))
            {
                ITemplate template = (ITemplate) templateControl.ContentTemplates[this._contentPlaceHolderID];
                templateControl.InstantiateInContentPlaceHolder(contentPlaceHolder, template);
            }
            return contentPlaceHolder;
        }

        public override void Init(TemplateParser parser, ControlBuilder parentBuilder, Type type, string tagName, string ID, IDictionary attribs)
        {
            this._contentPlaceHolderID = ID;
            if (parser.FInDesigner)
            {
                base.Init(parser, parentBuilder, type, tagName, ID, attribs);
            }
            else
            {
                if (string.IsNullOrEmpty(ID))
                {
                    throw new HttpException(System.Web.SR.GetString("Control_Missing_Attribute", new object[] { "ID", type.Name }));
                }
                this._templateName = ID;
                MasterPageParser parser2 = parser as MasterPageParser;
                if (parser2 == null)
                {
                    throw new HttpException(System.Web.SR.GetString("ContentPlaceHolder_only_in_master"));
                }
                base.Init(parser, parentBuilder, type, tagName, ID, attribs);
                if (parser2.PlaceHolderList.Contains(this.Name))
                {
                    throw new HttpException(System.Web.SR.GetString("ContentPlaceHolder_duplicate_contentPlaceHolderID", new object[] { this.Name }));
                }
                parser2.PlaceHolderList.Add(this.Name);
            }
        }

        private bool PageProvidesMatchingContent(MasterPage masterPage)
        {
            return (((masterPage != null) && (masterPage.ContentTemplates != null)) && masterPage.ContentTemplates.Contains(this._contentPlaceHolderID));
        }

        internal string Name
        {
            get
            {
                return this._templateName;
            }
        }
    }
}

