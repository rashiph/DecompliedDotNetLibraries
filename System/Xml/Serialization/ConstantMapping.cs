namespace System.Xml.Serialization
{
    using System;

    internal class ConstantMapping : Mapping
    {
        private string name;
        private long value;
        private string xmlName;

        internal string Name
        {
            get
            {
                if (this.name != null)
                {
                    return this.name;
                }
                return string.Empty;
            }
            set
            {
                this.name = value;
            }
        }

        internal long Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }

        internal string XmlName
        {
            get
            {
                if (this.xmlName != null)
                {
                    return this.xmlName;
                }
                return string.Empty;
            }
            set
            {
                this.xmlName = value;
            }
        }
    }
}

