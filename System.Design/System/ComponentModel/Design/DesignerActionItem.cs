namespace System.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Text.RegularExpressions;

    public abstract class DesignerActionItem
    {
        private bool allowAssociate;
        private string category;
        private string description;
        private string displayName;
        private IDictionary properties;

        internal DesignerActionItem()
        {
        }

        public DesignerActionItem(string displayName, string category, string description)
        {
            this.category = category;
            this.description = description;
            this.displayName = (displayName == null) ? null : Regex.Replace(displayName, @"\(\&.\)", "");
        }

        public bool AllowAssociate
        {
            get
            {
                return this.allowAssociate;
            }
            set
            {
                this.allowAssociate = value;
            }
        }

        public virtual string Category
        {
            get
            {
                return this.category;
            }
        }

        public virtual string Description
        {
            get
            {
                return this.description;
            }
        }

        public virtual string DisplayName
        {
            get
            {
                return this.displayName;
            }
        }

        public IDictionary Properties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new HybridDictionary();
                }
                return this.properties;
            }
        }
    }
}

