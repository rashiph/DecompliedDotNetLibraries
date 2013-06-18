namespace System.Web.Compilation
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Web.UI;

    internal class BuildResultNoCompileMasterPage : BuildResultNoCompileUserControl
    {
        private ICollection _placeHolderList;

        internal BuildResultNoCompileMasterPage(Type baseType, TemplateParser parser) : base(baseType, parser)
        {
            this._placeHolderList = ((MasterPageParser) parser).PlaceHolderList;
        }

        public override object CreateInstance()
        {
            MasterPage page = (MasterPage) base.CreateInstance();
            foreach (string str in this._placeHolderList)
            {
                page.ContentPlaceHolders.Add(str.ToLower(CultureInfo.InvariantCulture));
            }
            return page;
        }
    }
}

