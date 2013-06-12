namespace System.Xml.Serialization
{
    using System;

    [AttributeUsage(AttributeTargets.Field)]
    public class SoapEnumAttribute : Attribute
    {
        private string name;

        public SoapEnumAttribute()
        {
        }

        public SoapEnumAttribute(string name)
        {
            this.name = name;
        }

        public string Name
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
    }
}

