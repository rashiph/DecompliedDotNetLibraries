namespace System.Data.Design
{
    using System;

    internal abstract class DataSourceXmlSerializationAttribute : Attribute
    {
        private Type itemType;
        private string name;
        private bool specialWay = false;

        internal DataSourceXmlSerializationAttribute()
        {
        }

        public Type ItemType
        {
            get
            {
                return this.itemType;
            }
            set
            {
                this.itemType = value;
            }
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

        public bool SpecialWay
        {
            get
            {
                return this.specialWay;
            }
            set
            {
                this.specialWay = value;
            }
        }
    }
}

