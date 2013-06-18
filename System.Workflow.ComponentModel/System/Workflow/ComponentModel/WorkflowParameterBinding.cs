namespace System.Workflow.ComponentModel
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Runtime;
    using System.Workflow.ComponentModel.Serialization;

    [Browsable(true), DesignerSerializer(typeof(DependencyObjectCodeDomSerializer), typeof(CodeDomSerializer))]
    public sealed class WorkflowParameterBinding : DependencyObject
    {
        public static readonly DependencyProperty ParameterNameProperty = DependencyProperty.Register("ParameterName", typeof(string), typeof(WorkflowParameterBinding), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(object), typeof(WorkflowParameterBinding));

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public WorkflowParameterBinding()
        {
        }

        public WorkflowParameterBinding(string parameterName)
        {
            base.SetValue(ParameterNameProperty, parameterName);
        }

        public string ParameterName
        {
            get
            {
                return (string) base.GetValue(ParameterNameProperty);
            }
            set
            {
                base.SetValue(ParameterNameProperty, value);
            }
        }

        [DefaultValue((string) null)]
        public object Value
        {
            get
            {
                return base.GetValue(ValueProperty);
            }
            set
            {
                base.SetValue(ValueProperty, value);
            }
        }
    }
}

