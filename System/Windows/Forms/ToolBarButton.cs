namespace System.Windows.Forms
{
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Text;

    [Designer("System.Windows.Forms.Design.ToolBarButtonDesigner, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"), DefaultProperty("Text"), ToolboxItem(false), DesignTimeVisible(false)]
    public class ToolBarButton : Component
    {
        private int commandId;
        internal Menu dropDownMenu;
        private bool enabled;
        private ToolBarButtonImageIndexer imageIndexer;
        private string name;
        internal ToolBar parent;
        private bool partialPush;
        private bool pushed;
        internal IntPtr stringIndex;
        private ToolBarButtonStyle style;
        private string text;
        private string tooltipText;
        private object userData;
        private bool visible;

        public ToolBarButton()
        {
            this.enabled = true;
            this.visible = true;
            this.commandId = -1;
            this.style = ToolBarButtonStyle.PushButton;
            this.stringIndex = (IntPtr) (-1);
        }

        public ToolBarButton(string text)
        {
            this.enabled = true;
            this.visible = true;
            this.commandId = -1;
            this.style = ToolBarButtonStyle.PushButton;
            this.stringIndex = (IntPtr) (-1);
            this.Text = text;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.parent != null))
            {
                int index = this.FindButtonIndex();
                if (index != -1)
                {
                    this.parent.Buttons.RemoveAt(index);
                }
            }
            base.Dispose(disposing);
        }

        private int FindButtonIndex()
        {
            for (int i = 0; i < this.parent.Buttons.Count; i++)
            {
                if (this.parent.Buttons[i] == this)
                {
                    return i;
                }
            }
            return -1;
        }

        internal int GetButtonWidth()
        {
            int width = this.Parent.ButtonSize.Width;
            System.Windows.Forms.NativeMethods.TBBUTTONINFO lParam = new System.Windows.Forms.NativeMethods.TBBUTTONINFO {
                cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TBBUTTONINFO)),
                dwMask = 0x40
            };
            int num2 = (int) System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.Parent, this.Parent.Handle), System.Windows.Forms.NativeMethods.TB_GETBUTTONINFO, this.commandId, ref lParam);
            if (num2 != -1)
            {
                width = lParam.cx;
            }
            return width;
        }

        private bool GetPushedState()
        {
            if (((int) this.parent.SendMessage(0x40a, this.FindButtonIndex(), 0)) != 0)
            {
                this.pushed = true;
            }
            else
            {
                this.pushed = false;
            }
            return this.pushed;
        }

        internal System.Windows.Forms.NativeMethods.TBBUTTON GetTBBUTTON(int commandId)
        {
            System.Windows.Forms.NativeMethods.TBBUTTON tbbutton = new System.Windows.Forms.NativeMethods.TBBUTTON {
                iBitmap = this.ImageIndexer.ActualIndex,
                fsState = 0
            };
            if (this.enabled)
            {
                tbbutton.fsState = (byte) (tbbutton.fsState | 4);
            }
            if (this.partialPush && (this.style == ToolBarButtonStyle.ToggleButton))
            {
                tbbutton.fsState = (byte) (tbbutton.fsState | 0x10);
            }
            if (this.pushed)
            {
                tbbutton.fsState = (byte) (tbbutton.fsState | 1);
            }
            if (!this.visible)
            {
                tbbutton.fsState = (byte) (tbbutton.fsState | 8);
            }
            switch (this.style)
            {
                case ToolBarButtonStyle.PushButton:
                    tbbutton.fsStyle = 0;
                    break;

                case ToolBarButtonStyle.ToggleButton:
                    tbbutton.fsStyle = 2;
                    break;

                case ToolBarButtonStyle.Separator:
                    tbbutton.fsStyle = 1;
                    break;

                case ToolBarButtonStyle.DropDownButton:
                    tbbutton.fsStyle = 8;
                    break;
            }
            tbbutton.dwData = IntPtr.Zero;
            tbbutton.iString = this.stringIndex;
            this.commandId = commandId;
            tbbutton.idCommand = commandId;
            return tbbutton;
        }

        internal System.Windows.Forms.NativeMethods.TBBUTTONINFO GetTBBUTTONINFO(bool updateText, int newCommandId)
        {
            System.Windows.Forms.NativeMethods.TBBUTTONINFO tbbuttoninfo = new System.Windows.Forms.NativeMethods.TBBUTTONINFO {
                cbSize = Marshal.SizeOf(typeof(System.Windows.Forms.NativeMethods.TBBUTTONINFO)),
                dwMask = 13
            };
            if (updateText)
            {
                tbbuttoninfo.dwMask |= 2;
            }
            tbbuttoninfo.iImage = this.ImageIndexer.ActualIndex;
            if (newCommandId != this.commandId)
            {
                this.commandId = newCommandId;
                tbbuttoninfo.idCommand = newCommandId;
                tbbuttoninfo.dwMask |= 0x20;
            }
            tbbuttoninfo.fsState = 0;
            if (this.enabled)
            {
                tbbuttoninfo.fsState = (byte) (tbbuttoninfo.fsState | 4);
            }
            if (this.partialPush && (this.style == ToolBarButtonStyle.ToggleButton))
            {
                tbbuttoninfo.fsState = (byte) (tbbuttoninfo.fsState | 0x10);
            }
            if (this.pushed)
            {
                tbbuttoninfo.fsState = (byte) (tbbuttoninfo.fsState | 1);
            }
            if (!this.visible)
            {
                tbbuttoninfo.fsState = (byte) (tbbuttoninfo.fsState | 8);
            }
            switch (this.style)
            {
                case ToolBarButtonStyle.PushButton:
                    tbbuttoninfo.fsStyle = 0;
                    break;

                case ToolBarButtonStyle.ToggleButton:
                    tbbuttoninfo.fsStyle = 2;
                    break;

                case ToolBarButtonStyle.Separator:
                    tbbuttoninfo.fsStyle = 1;
                    break;
            }
            if (this.text == null)
            {
                tbbuttoninfo.pszText = Marshal.StringToHGlobalAuto("\0\0");
                return tbbuttoninfo;
            }
            string text = this.text;
            this.PrefixAmpersands(ref text);
            tbbuttoninfo.pszText = Marshal.StringToHGlobalAuto(text);
            return tbbuttoninfo;
        }

        private void PrefixAmpersands(ref string value)
        {
            if (((value != null) && (value.Length != 0)) && (value.IndexOf('&') >= 0))
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < value.Length; i++)
                {
                    if (value[i] == '&')
                    {
                        if ((i < (value.Length - 1)) && (value[i + 1] == '&'))
                        {
                            i++;
                        }
                        builder.Append("&&");
                    }
                    else
                    {
                        builder.Append(value[i]);
                    }
                }
                value = builder.ToString();
            }
        }

        public override string ToString()
        {
            return ("ToolBarButton: " + this.Text + ", Style: " + this.Style.ToString("G"));
        }

        internal void UpdateButton(bool recreate)
        {
            this.UpdateButton(recreate, false, true);
        }

        private void UpdateButton(bool recreate, bool updateText, bool updatePushedState)
        {
            if (((this.style == ToolBarButtonStyle.DropDownButton) && (this.parent != null)) && this.parent.DropDownArrows)
            {
                recreate = true;
            }
            if ((updatePushedState && (this.parent != null)) && this.parent.IsHandleCreated)
            {
                this.GetPushedState();
            }
            if (this.parent != null)
            {
                int index = this.FindButtonIndex();
                if (index != -1)
                {
                    this.parent.InternalSetButton(index, this, recreate, updateText);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolBarButtonMenuDescr"), DefaultValue((string) null), TypeConverter(typeof(ReferenceConverter))]
        public Menu DropDownMenu
        {
            get
            {
                return this.dropDownMenu;
            }
            set
            {
                if ((value != null) && !(value is ContextMenu))
                {
                    throw new ArgumentException(System.Windows.Forms.SR.GetString("ToolBarButtonInvalidDropDownMenuType"));
                }
                this.dropDownMenu = value;
            }
        }

        [Localizable(true), DefaultValue(true), System.Windows.Forms.SRDescription("ToolBarButtonEnabledDescr")]
        public bool Enabled
        {
            get
            {
                return this.enabled;
            }
            set
            {
                if (this.enabled != value)
                {
                    this.enabled = value;
                    if ((this.parent != null) && this.parent.IsHandleCreated)
                    {
                        this.parent.SendMessage(0x401, this.FindButtonIndex(), this.enabled ? 1 : 0);
                    }
                }
            }
        }

        [RefreshProperties(RefreshProperties.Repaint), Localizable(true), DefaultValue(-1), System.Windows.Forms.SRDescription("ToolBarButtonImageIndexDescr"), TypeConverter(typeof(ImageIndexConverter)), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public int ImageIndex
        {
            get
            {
                return this.ImageIndexer.Index;
            }
            set
            {
                if (this.ImageIndexer.Index != value)
                {
                    if (value < -1)
                    {
                        throw new ArgumentOutOfRangeException("ImageIndex", System.Windows.Forms.SR.GetString("InvalidLowBoundArgumentEx", new object[] { "ImageIndex", value.ToString(CultureInfo.CurrentCulture), -1 }));
                    }
                    this.ImageIndexer.Index = value;
                    this.UpdateButton(false);
                }
            }
        }

        internal ToolBarButtonImageIndexer ImageIndexer
        {
            get
            {
                if (this.imageIndexer == null)
                {
                    this.imageIndexer = new ToolBarButtonImageIndexer(this);
                }
                return this.imageIndexer;
            }
        }

        [DefaultValue(""), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("ToolBarButtonImageIndexDescr"), Localizable(true), TypeConverter(typeof(ImageKeyConverter)), Editor("System.Windows.Forms.Design.ImageIndexEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string ImageKey
        {
            get
            {
                return this.ImageIndexer.Key;
            }
            set
            {
                if (this.ImageIndexer.Key != value)
                {
                    this.ImageIndexer.Key = value;
                    this.UpdateButton(false);
                }
            }
        }

        [Browsable(false)]
        public string Name
        {
            get
            {
                return WindowsFormsUtils.GetComponentName(this, this.name);
            }
            set
            {
                if ((value == null) || (value.Length == 0))
                {
                    this.name = null;
                }
                else
                {
                    this.name = value;
                }
                if (this.Site != null)
                {
                    this.Site.Name = this.name;
                }
            }
        }

        [Browsable(false)]
        public ToolBar Parent
        {
            get
            {
                return this.parent;
            }
        }

        [DefaultValue(false), System.Windows.Forms.SRDescription("ToolBarButtonPartialPushDescr")]
        public bool PartialPush
        {
            get
            {
                if ((this.parent != null) && this.parent.IsHandleCreated)
                {
                    if (((int) this.parent.SendMessage(0x40d, this.FindButtonIndex(), 0)) != 0)
                    {
                        this.partialPush = true;
                    }
                    else
                    {
                        this.partialPush = false;
                    }
                }
                return this.partialPush;
            }
            set
            {
                if (this.partialPush != value)
                {
                    this.partialPush = value;
                    this.UpdateButton(false);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolBarButtonPushedDescr"), DefaultValue(false)]
        public bool Pushed
        {
            get
            {
                if ((this.parent != null) && this.parent.IsHandleCreated)
                {
                    return this.GetPushedState();
                }
                return this.pushed;
            }
            set
            {
                if (value != this.Pushed)
                {
                    this.pushed = value;
                    this.UpdateButton(false, false, false);
                }
            }
        }

        public System.Drawing.Rectangle Rectangle
        {
            get
            {
                if (this.parent != null)
                {
                    System.Windows.Forms.NativeMethods.RECT lParam = new System.Windows.Forms.NativeMethods.RECT();
                    System.Windows.Forms.UnsafeNativeMethods.SendMessage(new HandleRef(this.parent, this.parent.Handle), 0x433, this.FindButtonIndex(), ref lParam);
                    return System.Drawing.Rectangle.FromLTRB(lParam.left, lParam.top, lParam.right, lParam.bottom);
                }
                return System.Drawing.Rectangle.Empty;
            }
        }

        [DefaultValue(1), RefreshProperties(RefreshProperties.Repaint), System.Windows.Forms.SRDescription("ToolBarButtonStyleDescr")]
        public ToolBarButtonStyle Style
        {
            get
            {
                return this.style;
            }
            set
            {
                if (!System.Windows.Forms.ClientUtils.IsEnumValid(value, (int) value, 1, 4))
                {
                    throw new InvalidEnumArgumentException("value", (int) value, typeof(ToolBarButtonStyle));
                }
                if (this.style != value)
                {
                    this.style = value;
                    this.UpdateButton(true);
                }
            }
        }

        [TypeConverter(typeof(StringConverter)), System.Windows.Forms.SRCategory("CatData"), Localizable(false), Bindable(true), System.Windows.Forms.SRDescription("ControlTagDescr"), DefaultValue((string) null)]
        public object Tag
        {
            get
            {
                return this.userData;
            }
            set
            {
                this.userData = value;
            }
        }

        [Localizable(true), System.Windows.Forms.SRDescription("ToolBarButtonTextDescr"), DefaultValue("")]
        public string Text
        {
            get
            {
                if (this.text != null)
                {
                    return this.text;
                }
                return "";
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    value = null;
                }
                if (((value == null) && (this.text != null)) || ((value != null) && ((this.text == null) || !this.text.Equals(value))))
                {
                    this.text = value;
                    this.UpdateButton(WindowsFormsUtils.ContainsMnemonic(this.text), true, true);
                }
            }
        }

        [System.Windows.Forms.SRDescription("ToolBarButtonToolTipTextDescr"), DefaultValue(""), Localizable(true)]
        public string ToolTipText
        {
            get
            {
                if (this.tooltipText != null)
                {
                    return this.tooltipText;
                }
                return "";
            }
            set
            {
                this.tooltipText = value;
            }
        }

        [DefaultValue(true), Localizable(true), System.Windows.Forms.SRDescription("ToolBarButtonVisibleDescr")]
        public bool Visible
        {
            get
            {
                return this.visible;
            }
            set
            {
                if (this.visible != value)
                {
                    this.visible = value;
                    this.UpdateButton(false);
                }
            }
        }

        internal short Width
        {
            get
            {
                int width = 0;
                ToolBarButtonStyle style = this.Style;
                Size size = SystemInformation.Border3DSize;
                if (style != ToolBarButtonStyle.Separator)
                {
                    using (Graphics graphics = this.parent.CreateGraphicsInternal())
                    {
                        Size buttonSize = this.parent.buttonSize;
                        if (!buttonSize.IsEmpty)
                        {
                            width = buttonSize.Width;
                        }
                        else if ((this.parent.ImageList != null) || !string.IsNullOrEmpty(this.Text))
                        {
                            Size imageSize = this.parent.ImageSize;
                            Size size4 = Size.Ceiling(graphics.MeasureString(this.Text, this.parent.Font));
                            if (this.parent.TextAlign == ToolBarTextAlign.Right)
                            {
                                if (size4.Width == 0)
                                {
                                    width = imageSize.Width + (size.Width * 4);
                                }
                                else
                                {
                                    width = (imageSize.Width + size4.Width) + (size.Width * 6);
                                }
                            }
                            else if (imageSize.Width > size4.Width)
                            {
                                width = imageSize.Width + (size.Width * 4);
                            }
                            else
                            {
                                width = size4.Width + (size.Width * 4);
                            }
                            if ((style == ToolBarButtonStyle.DropDownButton) && this.parent.DropDownArrows)
                            {
                                width += 15;
                            }
                        }
                        else
                        {
                            width = this.parent.ButtonSize.Width;
                        }
                        goto Label_014D;
                    }
                }
                width = size.Width * 2;
            Label_014D:
                return (short) width;
            }
        }

        internal class ToolBarButtonImageIndexer : System.Windows.Forms.ImageList.Indexer
        {
            private ToolBarButton owner;

            public ToolBarButtonImageIndexer(ToolBarButton button)
            {
                this.owner = button;
            }

            public override System.Windows.Forms.ImageList ImageList
            {
                get
                {
                    if ((this.owner != null) && (this.owner.parent != null))
                    {
                        return this.owner.parent.ImageList;
                    }
                    return null;
                }
                set
                {
                }
            }
        }
    }
}

