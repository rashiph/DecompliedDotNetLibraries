namespace System.Windows.Forms
{
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ClassInterface(ClassInterfaceType.AutoDispatch), System.Windows.Forms.SRDescription("DescriptionHScrollBar"), ComVisible(true)]
    public class HScrollBar : ScrollBar
    {
        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.Style = createParams.Style;
                return createParams;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(80, SystemInformation.HorizontalScrollBarHeight);
            }
        }
    }
}

