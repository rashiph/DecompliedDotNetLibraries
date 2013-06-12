namespace System.Web.UI
{
    using System;

    public class TemplatePropertyEntry : BuilderPropertyEntry
    {
        private bool _bindableTemplate;

        internal TemplatePropertyEntry()
        {
        }

        internal TemplatePropertyEntry(bool bindableTemplate)
        {
            this._bindableTemplate = bindableTemplate;
        }

        public bool BindableTemplate
        {
            get
            {
                return this._bindableTemplate;
            }
        }

        internal bool IsMultiple
        {
            get
            {
                return Util.IsMultiInstanceTemplateProperty(base.PropertyInfo);
            }
        }
    }
}

