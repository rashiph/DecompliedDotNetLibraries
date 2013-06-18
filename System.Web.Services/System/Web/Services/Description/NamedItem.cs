namespace System.Web.Services.Description
{
    using System;
    using System.Runtime;
    using System.Xml.Serialization;

    public abstract class NamedItem : DocumentableItem
    {
        private string name;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        protected NamedItem()
        {
        }

        [XmlAttribute("name")]
        public string Name
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.name;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.name = value;
            }
        }
    }
}

