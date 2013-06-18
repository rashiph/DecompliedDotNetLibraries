namespace System.Workflow.Activities.Rules.Design
{
    using System;
    using System.CodeDom;
    using System.ComponentModel;
    using System.Workflow.Activities.Rules;

    internal class RuleConditionReferenceTypeConverter : TypeConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            PropertyDescriptorCollection descriptors = new PropertyDescriptorCollection(null);
            descriptors.Add(new RuleConditionReferenceNamePropertyDescriptor(context, TypeDescriptor.CreateProperty(typeof(RuleConditionReference), "ConditionName", typeof(string), new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content), DesignOnlyAttribute.Yes })));
            descriptors.Add(new RuleConditionReferencePropertyDescriptor(context, TypeDescriptor.CreateProperty(typeof(RuleConditionReference), "Expression", typeof(CodeExpression), new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content), DesignOnlyAttribute.Yes })));
            return descriptors.Sort(new string[] { "ConditionName", "Expression" });
        }

        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }
    }
}

