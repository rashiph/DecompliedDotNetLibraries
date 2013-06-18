namespace System.Windows.Markup
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Xaml;
    using System.Xaml.Schema;

    public class PropertyDefinition : MemberDefinition
    {
        private IList<Attribute> attributes;

        public IList<Attribute> Attributes
        {
            get
            {
                if (this.attributes == null)
                {
                    this.attributes = new List<Attribute>();
                }
                return this.attributes;
            }
        }

        [DefaultValue((string) null)]
        public string Modifier { get; set; }

        public override string Name { get; set; }

        [TypeConverter(typeof(XamlTypeTypeConverter))]
        public XamlType Type { get; set; }
    }
}

