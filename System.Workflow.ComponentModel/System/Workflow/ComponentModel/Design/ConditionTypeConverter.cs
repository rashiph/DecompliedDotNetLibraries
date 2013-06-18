namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Workflow.ComponentModel;

    internal sealed class ConditionTypeConverter : TypeConverter
    {
        internal static readonly Type CodeConditionType = Type.GetType("System.Workflow.Activities.CodeCondition, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        private Hashtable conditionDecls = new Hashtable();
        internal static DependencyProperty DeclarativeConditionDynamicProp = ((DependencyProperty) RuleConditionReferenceType.GetField("RuleDefinitionsProperty").GetValue(null));
        internal static readonly Type RuleConditionReferenceType = Type.GetType("System.Workflow.Activities.Rules.RuleDefinitions, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        internal static readonly Type RuleDefinitionsType = Type.GetType("System.Workflow.Activities.Rules.RuleConditionReference, System.Workflow.Activities, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

        public ConditionTypeConverter()
        {
            string fullName = CodeConditionType.FullName;
            object[] customAttributes = CodeConditionType.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            if (((customAttributes != null) && (customAttributes.Length > 0)) && (customAttributes[0] is DisplayNameAttribute))
            {
                fullName = ((DisplayNameAttribute) customAttributes[0]).DisplayName;
            }
            this.conditionDecls.Add(fullName, CodeConditionType);
            fullName = RuleDefinitionsType.FullName;
            customAttributes = RuleDefinitionsType.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            if (((customAttributes != null) && (customAttributes.Length > 0)) && (customAttributes[0] is DisplayNameAttribute))
            {
                fullName = ((DisplayNameAttribute) customAttributes[0]).DisplayName;
            }
            this.conditionDecls.Add(fullName, RuleDefinitionsType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string))
            {
                return base.ConvertFrom(context, culture, value);
            }
            if ((((string) value).Length != 0) && !(((string) value) == SR.GetString("NullConditionExpression")))
            {
                return Activator.CreateInstance(this.conditionDecls[value] as Type);
            }
            return null;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value == null)
            {
                return SR.GetString("NullConditionExpression");
            }
            object key = null;
            if ((destinationType == typeof(string)) && (value is ActivityCondition))
            {
                foreach (DictionaryEntry entry in this.conditionDecls)
                {
                    if (value.GetType() == entry.Value)
                    {
                        key = entry.Key;
                        break;
                    }
                }
            }
            if (key == null)
            {
                key = base.ConvertTo(context, culture, value, destinationType);
            }
            return key;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection(new PropertyDescriptor[0]);
            TypeConverter converter = TypeDescriptor.GetConverter(value.GetType());
            if (((converter != null) && (converter.GetType() != base.GetType())) && converter.GetPropertiesSupported())
            {
                return converter.GetProperties(context, value, attributes);
            }
            IComponent component = PropertyDescriptorUtils.GetComponent(context);
            if (component != null)
            {
                descriptors = PropertyDescriptorFilter.FilterProperties(component.Site, value, TypeDescriptor.GetProperties(value, new Attribute[] { BrowsableAttribute.Yes }));
            }
            return descriptors;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            ArrayList list = new ArrayList();
            list.Add(null);
            foreach (object obj2 in this.conditionDecls.Keys)
            {
                Type type = this.conditionDecls[obj2] as Type;
                list.Add(Activator.CreateInstance(type));
            }
            return new TypeConverter.StandardValuesCollection((ActivityCondition[]) list.ToArray(typeof(ActivityCondition)));
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

