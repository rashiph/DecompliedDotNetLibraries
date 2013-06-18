namespace System.Data
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Data.Common;
    using System.Globalization;
    using System.Reflection;

    internal sealed class RelationshipConverter : ExpandableObjectConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(InstanceDescriptor)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            ConstructorInfo member = null;
            object[] arguments = null;
            if (!(destinationType == typeof(InstanceDescriptor)) || !(value is DataRelation))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            DataRelation relation = (DataRelation) value;
            DataTable table2 = relation.ParentKey.Table;
            DataTable table = relation.ChildKey.Table;
            if (ADP.IsEmpty(table2.Namespace) && ADP.IsEmpty(table.Namespace))
            {
                member = typeof(DataRelation).GetConstructor(new Type[] { typeof(string), typeof(string), typeof(string), typeof(string[]), typeof(string[]), typeof(bool) });
                arguments = new object[] { relation.RelationName, relation.ParentKey.Table.TableName, relation.ChildKey.Table.TableName, relation.ParentColumnNames, relation.ChildColumnNames, relation.Nested };
            }
            else
            {
                member = typeof(DataRelation).GetConstructor(new Type[] { typeof(string), typeof(string), typeof(string), typeof(string), typeof(string), typeof(string[]), typeof(string[]), typeof(bool) });
                arguments = new object[] { relation.RelationName, relation.ParentKey.Table.TableName, relation.ParentKey.Table.Namespace, relation.ChildKey.Table.TableName, relation.ChildKey.Table.Namespace, relation.ParentColumnNames, relation.ChildColumnNames, relation.Nested };
            }
            return new InstanceDescriptor(member, arguments);
        }
    }
}

