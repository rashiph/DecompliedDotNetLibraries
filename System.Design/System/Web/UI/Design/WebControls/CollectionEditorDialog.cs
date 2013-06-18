namespace System.Web.UI.Design.WebControls
{
    using System;
    using System.Web.UI.Design.Util;
    using System.Windows.Forms;

    internal abstract class CollectionEditorDialog : DesignerForm
    {
        protected CollectionEditorDialog(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        protected ToolStripButton CreatePushButton(string toolTipText, int imageIndex)
        {
            return new ToolStripButton { Text = toolTipText, AutoToolTip = true, DisplayStyle = ToolStripItemDisplayStyle.Image, ImageIndex = imageIndex, ImageScaling = ToolStripItemImageScaling.SizeToFit };
        }
    }
}

