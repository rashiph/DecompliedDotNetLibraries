namespace System.Workflow.ComponentModel
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;

    internal sealed class SynchronizationHandlesEditor : UITypeEditor
    {
        private MultilineStringEditor stringEditor = new MultilineStringEditor();

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            string str = SynchronizationHandlesTypeConverter.Stringify(value as ICollection<string>);
            str = this.stringEditor.EditValue(context, provider, str) as string;
            value = SynchronizationHandlesTypeConverter.UnStringify(str);
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return this.stringEditor.GetEditStyle(context);
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return this.stringEditor.GetPaintValueSupported(context);
        }
    }
}

