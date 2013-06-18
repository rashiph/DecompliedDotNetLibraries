namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    [DefaultProperty("Color"), System.Windows.Forms.SRDescription("DescriptionColorDialog")]
    public class ColorDialog : CommonDialog
    {
        private System.Drawing.Color color;
        private int[] customColors = new int[0x10];
        private int options;

        public ColorDialog()
        {
            this.Reset();
        }

        private bool GetOption(int option)
        {
            return ((this.options & option) != 0);
        }

        public override void Reset()
        {
            this.options = 0;
            this.color = System.Drawing.Color.Black;
            this.CustomColors = null;
        }

        private void ResetColor()
        {
            this.Color = System.Drawing.Color.Black;
        }

        protected override bool RunDialog(IntPtr hwndOwner)
        {
            bool flag;
            System.Windows.Forms.NativeMethods.WndProc proc = new System.Windows.Forms.NativeMethods.WndProc(this.HookProc);
            System.Windows.Forms.NativeMethods.CHOOSECOLOR cc = new System.Windows.Forms.NativeMethods.CHOOSECOLOR();
            IntPtr destination = Marshal.AllocCoTaskMem(0x40);
            try
            {
                Marshal.Copy(this.customColors, 0, destination, 0x10);
                cc.hwndOwner = hwndOwner;
                cc.hInstance = this.Instance;
                cc.rgbResult = ColorTranslator.ToWin32(this.color);
                cc.lpCustColors = destination;
                int num = this.Options | 0x11;
                if (!this.AllowFullOpen)
                {
                    num &= -3;
                }
                cc.Flags = num;
                cc.lpfnHook = proc;
                if (!System.Windows.Forms.SafeNativeMethods.ChooseColor(cc))
                {
                    return false;
                }
                if (cc.rgbResult != ColorTranslator.ToWin32(this.color))
                {
                    this.color = ColorTranslator.FromOle(cc.rgbResult);
                }
                Marshal.Copy(destination, this.customColors, 0, 0x10);
                flag = true;
            }
            finally
            {
                Marshal.FreeCoTaskMem(destination);
            }
            return flag;
        }

        private void SetOption(int option, bool value)
        {
            if (value)
            {
                this.options |= option;
            }
            else
            {
                this.options &= ~option;
            }
        }

        private bool ShouldSerializeColor()
        {
            return !this.Color.Equals(System.Drawing.Color.Black);
        }

        public override string ToString()
        {
            return (base.ToString() + ",  Color: " + this.Color.ToString());
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("CDallowFullOpenDescr"), DefaultValue(true)]
        public virtual bool AllowFullOpen
        {
            get
            {
                return !this.GetOption(4);
            }
            set
            {
                this.SetOption(4, !value);
            }
        }

        [System.Windows.Forms.SRCategory("CatBehavior"), DefaultValue(false), System.Windows.Forms.SRDescription("CDanyColorDescr")]
        public virtual bool AnyColor
        {
            get
            {
                return this.GetOption(0x100);
            }
            set
            {
                this.SetOption(0x100, value);
            }
        }

        [System.Windows.Forms.SRDescription("CDcolorDescr"), System.Windows.Forms.SRCategory("CatData")]
        public System.Drawing.Color Color
        {
            get
            {
                return this.color;
            }
            set
            {
                if (!value.IsEmpty)
                {
                    this.color = value;
                }
                else
                {
                    this.color = System.Drawing.Color.Black;
                }
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden), Browsable(false), System.Windows.Forms.SRDescription("CDcustomColorsDescr")]
        public int[] CustomColors
        {
            get
            {
                return (int[]) this.customColors.Clone();
            }
            set
            {
                int length = (value == null) ? 0 : Math.Min(value.Length, 0x10);
                if (length > 0)
                {
                    Array.Copy(value, 0, this.customColors, 0, length);
                }
                for (int i = length; i < 0x10; i++)
                {
                    this.customColors[i] = 0xffffff;
                }
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatAppearance"), System.Windows.Forms.SRDescription("CDfullOpenDescr")]
        public virtual bool FullOpen
        {
            get
            {
                return this.GetOption(2);
            }
            set
            {
                this.SetOption(2, value);
            }
        }

        protected virtual IntPtr Instance
        {
            [SecurityPermission(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.UnmanagedCode), SecurityPermission(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.UnmanagedCode)]
            get
            {
                return System.Windows.Forms.UnsafeNativeMethods.GetModuleHandle(null);
            }
        }

        protected virtual int Options
        {
            get
            {
                return this.options;
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRCategory("CatBehavior"), System.Windows.Forms.SRDescription("CDshowHelpDescr")]
        public virtual bool ShowHelp
        {
            get
            {
                return this.GetOption(8);
            }
            set
            {
                this.SetOption(8, value);
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("CDsolidColorOnlyDescr"), System.Windows.Forms.SRCategory("CatBehavior")]
        public virtual bool SolidColorOnly
        {
            get
            {
                return this.GetOption(0x80);
            }
            set
            {
                this.SetOption(0x80, value);
            }
        }
    }
}

