namespace System.Data
{
    using System;
    using System.ComponentModel;

    internal sealed class DataTableTypeConverter : ReferenceConverter
    {
        public DataTableTypeConverter() : base(typeof(DataTable))
        {
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
}

