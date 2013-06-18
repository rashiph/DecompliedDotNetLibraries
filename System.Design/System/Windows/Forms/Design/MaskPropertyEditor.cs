namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing.Design;
    using System.Windows.Forms;

    internal class MaskPropertyEditor : UITypeEditor
    {
        internal static string EditMask(ITypeDiscoveryService discoverySvc, IUIService uiSvc, MaskedTextBox instance, IHelpService helpService)
        {
            string mask = null;
            using (MaskDesignerDialog dialog = new MaskDesignerDialog(instance, helpService))
            {
                dialog.DiscoverMaskDescriptors(discoverySvc);
                DialogResult result = (uiSvc != null) ? uiSvc.ShowDialog(dialog) : dialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    mask = dialog.Mask;
                    if (dialog.ValidatingType != instance.ValidatingType)
                    {
                        instance.ValidatingType = dialog.ValidatingType;
                    }
                }
            }
            return mask;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if ((context != null) && (provider != null))
            {
                ITypeDiscoveryService discoverySvc = (ITypeDiscoveryService) provider.GetService(typeof(ITypeDiscoveryService));
                IUIService service = (IUIService) provider.GetService(typeof(IUIService));
                IHelpService helpService = (IHelpService) provider.GetService(typeof(IHelpService));
                string str = EditMask(discoverySvc, service, context.Instance as MaskedTextBox, helpService);
                if (str != null)
                {
                    return str;
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override bool GetPaintValueSupported(ITypeDescriptorContext context)
        {
            return false;
        }
    }
}

