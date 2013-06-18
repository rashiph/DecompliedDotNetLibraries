namespace System.Windows.Forms.Design
{
    using System;
    using System.Windows.Forms;

    internal class VsPropertyGrid : PropertyGrid
    {
        public VsPropertyGrid(IServiceProvider serviceProvider)
        {
            if (serviceProvider != null)
            {
                IUIService service = serviceProvider.GetService(typeof(IUIService)) as IUIService;
                if (service != null)
                {
                    base.ToolStripRenderer = (ToolStripProfessionalRenderer) service.Styles["VsToolWindowRenderer"];
                }
            }
        }
    }
}

