namespace System.Workflow.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Windows.Forms;

    internal sealed class ColorPickerEditor : UITypeEditor
    {
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            ColorDialog dialog = new ColorDialog {
                AllowFullOpen = true,
                FullOpen = true,
                AnyColor = true,
                Color = (value is Color) ? ((Color) value) : Color.White
            };
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                return dialog.Color;
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return ((context != null) && (context.PropertyDescriptor != null));
        }

        public override void PaintValue(PaintValueEventArgs e)
        {
            base.PaintValue(e);
            if (e.Value is Color)
            {
                Color color = (Color) e.Value;
                if (color != Color.Empty)
                {
                    using (Brush brush = new SolidBrush(color))
                    {
                        e.Graphics.FillRectangle(brush, e.Bounds);
                    }
                }
            }
        }
    }
}

