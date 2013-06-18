namespace System.Web.UI.Design
{
    using System;
    using System.Collections;
    using System.Web.UI.WebControls;

    public class TemplateGroup
    {
        private string _groupName;
        private Style _groupStyle;
        private ArrayList _templates;
        private static TemplateDefinition[] emptyTemplateDefinitionArray = new TemplateDefinition[0];

        public TemplateGroup(string groupName) : this(groupName, null)
        {
        }

        public TemplateGroup(string groupName, Style groupStyle)
        {
            this._groupName = groupName;
            this._groupStyle = groupStyle;
        }

        public void AddTemplateDefinition(TemplateDefinition templateDefinition)
        {
            if (this._templates == null)
            {
                this._templates = new ArrayList();
            }
            this._templates.Add(templateDefinition);
        }

        public string GroupName
        {
            get
            {
                return this._groupName;
            }
        }

        public Style GroupStyle
        {
            get
            {
                return this._groupStyle;
            }
        }

        public bool IsEmpty
        {
            get
            {
                if (this._templates != null)
                {
                    return (this._templates.Count == 0);
                }
                return true;
            }
        }

        public TemplateDefinition[] Templates
        {
            get
            {
                if (this._templates == null)
                {
                    return emptyTemplateDefinitionArray;
                }
                return (TemplateDefinition[]) this._templates.ToArray(typeof(TemplateDefinition));
            }
        }
    }
}

