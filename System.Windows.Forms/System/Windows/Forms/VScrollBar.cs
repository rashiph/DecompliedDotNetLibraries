namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [ComVisible(true), System.Windows.Forms.SRDescription("DescriptionVScrollBar"), ClassInterface(ClassInterfaceType.AutoDispatch)]
    public class VScrollBar : ScrollBar
    {
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public event EventHandler RightToLeftChanged
        {
            add
            {
                base.RightToLeftChanged += value;
            }
            remove
            {
                base.RightToLeftChanged -= value;
            }
        }

        protected override System.Windows.Forms.CreateParams CreateParams
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                System.Windows.Forms.CreateParams createParams = base.CreateParams;
                createParams.Style |= 1;
                return createParams;
            }
        }

        protected override Size DefaultSize
        {
            get
            {
                return new Size(SystemInformation.VerticalScrollBarWidth, 80);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never), Browsable(false)]
        public override System.Windows.Forms.RightToLeft RightToLeft
        {
            get
            {
                return System.Windows.Forms.RightToLeft.No;
            }
            set
            {
            }
        }
    }
}

