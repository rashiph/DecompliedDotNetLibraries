namespace System.Web.UI.WebControls
{
    using System;
    using System.ComponentModel;

    public class TargetConverter : StringConverter
    {
        private static string[] targetValues = new string[] { "_blank", "_parent", "_search", "_self", "_top" };
        private TypeConverter.StandardValuesCollection values;

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (this.values == null)
            {
                this.values = new TypeConverter.StandardValuesCollection(targetValues);
            }
            return this.values;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

