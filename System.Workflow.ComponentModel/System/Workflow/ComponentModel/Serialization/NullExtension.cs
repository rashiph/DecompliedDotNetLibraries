namespace System.Workflow.ComponentModel.Serialization
{
    using System;
    using System.ComponentModel.Design.Serialization;

    [DesignerSerializer(typeof(MarkupExtensionSerializer), typeof(WorkflowMarkupSerializer))]
    internal sealed class NullExtension : MarkupExtension
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return null;
        }
    }
}

