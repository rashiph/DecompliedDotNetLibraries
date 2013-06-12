namespace System.Web.UI
{
    using System;

    public class FileLevelMasterPageControlBuilder : FileLevelPageControlBuilder
    {
        internal override void AddContentTemplate(object obj, string templateName, ITemplate template)
        {
            ((MasterPage) obj).AddContentTemplate(templateName, template);
        }
    }
}

