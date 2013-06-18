namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;

    public class DataProviderNameConverter : StringConverter
    {
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            DataRowCollection rows = DbProviderFactories.GetFactoryClasses().Rows;
            string[] values = new string[rows.Count];
            for (int i = 0; i < rows.Count; i++)
            {
                values[i] = (string) rows[i]["InvariantName"];
            }
            return new TypeConverter.StandardValuesCollection(values);
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

