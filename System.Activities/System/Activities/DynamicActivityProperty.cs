namespace System.Activities
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;

    public class DynamicActivityProperty
    {
        private Collection<Attribute> attributes;

        public override string ToString()
        {
            if ((this.Type != null) && (this.Name != null))
            {
                return ("Property: " + this.Type.ToString() + " " + this.Name);
            }
            return string.Empty;
        }

        public Collection<Attribute> Attributes
        {
            get
            {
                if (this.attributes == null)
                {
                    this.attributes = new Collection<Attribute>();
                }
                return this.attributes;
            }
        }

        [DefaultValue((string) null)]
        public string Name { get; set; }

        [DefaultValue((string) null)]
        public System.Type Type { get; set; }

        [DefaultValue((string) null)]
        public object Value { get; set; }
    }
}

