namespace System.Data
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    internal sealed class ConstraintConverter : ExpandableObjectConverter
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
            if ((destinationType == typeof(InstanceDescriptor)) && (value is Constraint))
            {
                if (value is UniqueConstraint)
                {
                    UniqueConstraint constraint2 = (UniqueConstraint) value;
                    ConstructorInfo constructor = typeof(UniqueConstraint).GetConstructor(new Type[] { typeof(string), typeof(string[]), typeof(bool) });
                    if (constructor != null)
                    {
                        return new InstanceDescriptor(constructor, new object[] { constraint2.ConstraintName, constraint2.ColumnNames, constraint2.IsPrimaryKey });
                    }
                }
                else
                {
                    ForeignKeyConstraint constraint = (ForeignKeyConstraint) value;
                    ConstructorInfo member = typeof(ForeignKeyConstraint).GetConstructor(new Type[] { typeof(string), typeof(string), typeof(string[]), typeof(string[]), typeof(AcceptRejectRule), typeof(Rule), typeof(Rule) });
                    if (member != null)
                    {
                        return new InstanceDescriptor(member, new object[] { constraint.ConstraintName, constraint.ParentKey.Table.TableName, constraint.ParentColumnNames, constraint.ChildColumnNames, constraint.AcceptRejectRule, constraint.DeleteRule, constraint.UpdateRule });
                    }
                }
            }
            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}

