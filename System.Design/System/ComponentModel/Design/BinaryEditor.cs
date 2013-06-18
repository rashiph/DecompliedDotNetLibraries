namespace System.ComponentModel.Design
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using System.IO;
    using System.Text;
    using System.Windows.Forms;
    using System.Windows.Forms.Design;

    public sealed class BinaryEditor : UITypeEditor
    {
        private BinaryUI binaryUI;
        private ITypeDescriptorContext context;
        private static readonly string HELP_KEYWORD = "System.ComponentModel.Design.BinaryEditor";

        internal byte[] ConvertToBytes(object value)
        {
            if (value is Stream)
            {
                Stream stream = (Stream) value;
                stream.Position = 0L;
                int count = (int) (stream.Length - stream.Position);
                byte[] buffer = new byte[count];
                stream.Read(buffer, 0, count);
                return buffer;
            }
            if (value is byte[])
            {
                return (byte[]) value;
            }
            if (value is string)
            {
                int num2 = ((string) value).Length * 2;
                byte[] bytes = new byte[num2];
                Encoding.Unicode.GetBytes(((string) value).ToCharArray(), 0, num2 / 2, bytes, 0);
                return bytes;
            }
            return null;
        }

        internal void ConvertToValue(byte[] bytes, ref object value)
        {
            if (value is Stream)
            {
                Stream stream = (Stream) value;
                stream.Position = 0L;
                stream.Write(bytes, 0, bytes.Length);
            }
            else if (value is byte[])
            {
                value = bytes;
            }
            else if (value is string)
            {
                value = BitConverter.ToString(bytes);
            }
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null)
            {
                this.context = context;
                IWindowsFormsEditorService service = (IWindowsFormsEditorService) provider.GetService(typeof(IWindowsFormsEditorService));
                if (service == null)
                {
                    return value;
                }
                if (this.binaryUI == null)
                {
                    this.binaryUI = new BinaryUI(this);
                }
                this.binaryUI.Value = value;
                if (service.ShowDialog(this.binaryUI) == DialogResult.OK)
                {
                    value = this.binaryUI.Value;
                }
                this.binaryUI.Value = null;
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        internal object GetService(System.Type serviceType)
        {
            if (this.context == null)
            {
                return null;
            }
            IDesignerHost service = this.context.GetService(typeof(IDesignerHost)) as IDesignerHost;
            if (service == null)
            {
                return this.context.GetService(serviceType);
            }
            return service.GetService(serviceType);
        }

        internal void ShowHelp()
        {
            IHelpService service = this.GetService(typeof(IHelpService)) as IHelpService;
            if (service != null)
            {
                service.ShowHelpFromKeyword(HELP_KEYWORD);
            }
        }
    }
}

