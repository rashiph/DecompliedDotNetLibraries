namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;

    public sealed class DesignerActionPropertyItem : DesignerActionItem
    {
        private string memberName;
        private IComponent relatedComponent;

        public DesignerActionPropertyItem(string memberName, string displayName) : this(memberName, displayName, null, null)
        {
        }

        public DesignerActionPropertyItem(string memberName, string displayName, string category) : this(memberName, displayName, category, null)
        {
        }

        public DesignerActionPropertyItem(string memberName, string displayName, string category, string description) : base(displayName, category, description)
        {
            this.memberName = memberName;
        }

        public string MemberName
        {
            get
            {
                return this.memberName;
            }
        }

        public IComponent RelatedComponent
        {
            get
            {
                return this.relatedComponent;
            }
            set
            {
                this.relatedComponent = value;
            }
        }
    }
}

