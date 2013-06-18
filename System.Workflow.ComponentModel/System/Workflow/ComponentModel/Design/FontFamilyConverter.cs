namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;

    internal sealed class FontFamilyConverter : TypeConverter
    {
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new TypeConverter.StandardValuesCollection(AmbientTheme.SupportedFonts);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

