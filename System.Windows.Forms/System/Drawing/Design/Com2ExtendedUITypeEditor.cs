namespace System.Drawing.Design
{
    using System;
    using System.ComponentModel;

    internal class Com2ExtendedUITypeEditor : UITypeEditor
    {
        private UITypeEditor innerEditor;

        public Com2ExtendedUITypeEditor(UITypeEditor baseTypeEditor)
        {
            this.innerEditor = baseTypeEditor;
        }

        public Com2ExtendedUITypeEditor(Type baseType)
        {
            this.innerEditor = (UITypeEditor) TypeDescriptor.GetEditor(baseType, typeof(UITypeEditor));
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (this.innerEditor != null)
            {
                return this.innerEditor.EditValue(context, provider, value);
            }
            return base.EditValue(context, provider, value);
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if (this.innerEditor != null)
            {
                return this.innerEditor.GetEditStyle(context);
            }
            return base.GetEditStyle(context);
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            if (this.innerEditor != null)
            {
                return this.innerEditor.GetPaintValueSupported(context);
            }
            return base.GetPaintValueSupported(context);
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            if (this.innerEditor != null)
            {
                this.innerEditor.PaintValue(e);
            }
            base.PaintValue(e);
        }

        public UITypeEditor InnerEditor
        {
            get
            {
                return this.innerEditor;
            }
        }
    }
}

