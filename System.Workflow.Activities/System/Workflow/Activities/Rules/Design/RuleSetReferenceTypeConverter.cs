namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Workflow.Activities.Common;
    using System.Workflow.Activities.Rules;
    using System.Workflow.ComponentModel;

    internal class RuleSetReferenceTypeConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return ((sourceType == typeof(string)) || base.CanConvertFrom(context, sourceType));
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return ((destinationType == typeof(string)) || base.CanConvertTo(context, destinationType));
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object valueToConvert)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            string key = valueToConvert as string;
            if ((key == null) || (key.TrimEnd(new char[0]).Length == 0))
            {
                key = string.Empty;
            }
            ISite serviceProvider = PropertyDescriptorUtils.GetSite(context, context.Instance);
            if (serviceProvider == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Messages.MissingService, new object[] { typeof(ISite).FullName }));
            }
            RuleSetCollection ruleSets = null;
            RuleDefinitions definitions = ConditionHelper.Load_Rules_DT(serviceProvider, Helpers.GetRootActivity(serviceProvider.Component as Activity));
            if (definitions != null)
            {
                ruleSets = definitions.RuleSets;
            }
            if (((ruleSets != null) && (key.Length != 0)) && !ruleSets.Contains(key))
            {
                RuleSet item = new RuleSet {
                    Name = key
                };
                ruleSets.Add(item);
                ConditionHelper.Flush_Rules_DT(serviceProvider, Helpers.GetRootActivity(serviceProvider.Component as Activity));
            }
            return new RuleSetReference { RuleSetName = key };
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == null)
            {
                throw new ArgumentNullException("destinationType");
            }
            if (destinationType != typeof(string))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
            RuleSetReference reference = value as RuleSetReference;
            if (reference != null)
            {
                return reference.RuleSetName;
            }
            return null;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            ISite serviceProvider = null;
            IComponent component = PropertyDescriptorUtils.GetComponent(context);
            if (component != null)
            {
                serviceProvider = component.Site;
            }
            PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection(null);
            descriptors.Add(new RuleSetPropertyDescriptor(serviceProvider, TypeDescriptor.CreateProperty(typeof(RuleSet), "RuleSet Definition", typeof(RuleSet), new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content), DesignOnlyAttribute.Yes })));
            return descriptors;
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

