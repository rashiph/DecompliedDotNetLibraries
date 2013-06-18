namespace System.Web.UI.Design
{
    using System;
    using System.ComponentModel;
    using System.Security.Permissions;

    [SecurityPermission(SecurityAction.Demand, Flags=SecurityPermissionFlag.UnmanagedCode)]
    public class DataSourceBooleanViewSchemaConverter : DataSourceViewSchemaConverter
    {
        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return this.GetStandardValues(context, typeof(bool));
        }
    }
}

