namespace System.Data.Design
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    internal sealed class DataSourceXmlClassAttribute : Attribute
    {
        private string name;

        internal DataSourceXmlClassAttribute(string elementName)
        {
            this.name = elementName;
        }

        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.name = value;
            }
        }
    }
}

