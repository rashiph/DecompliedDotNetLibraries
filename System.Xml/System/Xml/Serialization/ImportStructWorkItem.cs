namespace System.Xml.Serialization
{
    using System;

    internal class ImportStructWorkItem
    {
        private StructMapping mapping;
        private StructModel model;

        internal ImportStructWorkItem(StructModel model, StructMapping mapping)
        {
            this.model = model;
            this.mapping = mapping;
        }

        internal StructMapping Mapping
        {
            get
            {
                return this.mapping;
            }
        }

        internal StructModel Model
        {
            get
            {
                return this.model;
            }
        }
    }
}

