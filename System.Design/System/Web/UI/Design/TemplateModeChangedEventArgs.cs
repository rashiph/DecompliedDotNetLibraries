namespace System.Web.UI.Design
{
    using System;

    public class TemplateModeChangedEventArgs : EventArgs
    {
        private TemplateGroup _newTemplateGroup;

        public TemplateModeChangedEventArgs(TemplateGroup newTemplateGroup)
        {
            this._newTemplateGroup = newTemplateGroup;
        }

        public TemplateGroup NewTemplateGroup
        {
            get
            {
                return this._newTemplateGroup;
            }
        }
    }
}

