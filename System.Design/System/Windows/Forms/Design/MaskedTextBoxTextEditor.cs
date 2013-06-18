namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.Windows.Forms;

    internal class MaskedTextBoxTextEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            IWindowsFormsEditorService service = null;
            if (((context != null) && (context.Instance != null)) && (provider != null))
            {
                service = provider.GetService(typeof(IWindowsFormsEditorService)) as IWindowsFormsEditorService;
                if ((service == null) || (context.Instance == null))
                {
                    return value;
                }
                MaskedTextBox instance = context.Instance as MaskedTextBox;
                if (instance == null)
                {
                    instance = new MaskedTextBox {
                        Text = value as string
                    };
                }
                MaskedTextBoxTextEditorDropDown control = new MaskedTextBoxTextEditorDropDown(instance);
                service.DropDownControl(control);
                if (control.Value != null)
                {
                    value = control.Value;
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            if ((context != null) && (context.Instance != null))
            {
                return UITypeEditorEditStyle.DropDown;
            }
            return base.GetEditStyle(context);
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            if ((context != null) && (context.Instance != null))
            {
                return false;
            }
            return base.GetPaintValueSupported(context);
        }

        public override bool IsDropDownResizable
        {
            get
            {
                return false;
            }
        }
    }
}

