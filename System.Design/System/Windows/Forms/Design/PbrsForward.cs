namespace System.Windows.Forms.Design
{
    using System;
    using System.Collections;
    using System.ComponentModel.Design;
    using System.Design;
    using System.Runtime.InteropServices;
    using System.Windows.Forms;

    internal class PbrsForward : IWindowTarget
    {
        private ArrayList bufferedChars;
        private bool ignoreMessages;
        private Message lastKeyDown;
        private IMenuCommandService menuCommandSvc;
        private IWindowTarget oldTarget;
        private bool postCharMessage;
        private IServiceProvider sp;
        private Control target;
        private const int WM_PRIVATE_POSTCHAR = 0x1998;

        public PbrsForward(Control target, IServiceProvider sp)
        {
            this.target = target;
            this.oldTarget = target.WindowTarget;
            this.sp = sp;
            target.WindowTarget = this;
        }

        public void Dispose()
        {
            this.target.WindowTarget = this.oldTarget;
        }

        void IWindowTarget.OnHandleChange(IntPtr newHandle)
        {
        }

        void IWindowTarget.OnMessage(ref Message m)
        {
            this.ignoreMessages = false;
            if ((((m.Msg >= 0x100) && (m.Msg <= 0x108)) || ((m.Msg >= 0x10d) && (m.Msg <= 0x10f))) && (this.InSituSupportService != null))
            {
                this.ignoreMessages = this.InSituSupportService.IgnoreMessages;
            }
            int msg = m.Msg;
            if (msg <= 0x102)
            {
                switch (msg)
                {
                    case 0x100:
                        this.lastKeyDown = m;
                        break;

                    case 0x101:
                        goto Label_024C;

                    case 0x102:
                        goto Label_025D;

                    case 8:
                        if (this.postCharMessage)
                        {
                            System.Design.UnsafeNativeMethods.PostMessage(this.target.Handle, 0x1998, IntPtr.Zero, IntPtr.Zero);
                            this.postCharMessage = false;
                        }
                        break;
                }
                goto Label_0339;
            }
            switch (msg)
            {
                case 0x10d:
                case 0x10f:
                    goto Label_025D;

                case 270:
                    break;

                default:
                    if (msg != 0x1998)
                    {
                        goto Label_0339;
                    }
                    if (this.bufferedChars != null)
                    {
                        IntPtr zero = IntPtr.Zero;
                        if (!this.ignoreMessages)
                        {
                            zero = System.Design.NativeMethods.GetFocus();
                        }
                        else if (this.InSituSupportService != null)
                        {
                            zero = this.InSituSupportService.GetEditWindow();
                        }
                        else
                        {
                            zero = System.Design.NativeMethods.GetFocus();
                        }
                        if (zero != m.HWnd)
                        {
                            foreach (BufferedKey key in this.bufferedChars)
                            {
                                if (key.KeyChar.Msg == 0x102)
                                {
                                    if (key.KeyDown.Msg != 0)
                                    {
                                        System.Design.NativeMethods.SendMessage(zero, 0x100, key.KeyDown.WParam, key.KeyDown.LParam);
                                    }
                                    System.Design.NativeMethods.SendMessage(zero, 0x102, key.KeyChar.WParam, key.KeyChar.LParam);
                                    if (key.KeyUp.Msg != 0)
                                    {
                                        System.Design.NativeMethods.SendMessage(zero, 0x101, key.KeyUp.WParam, key.KeyUp.LParam);
                                    }
                                }
                                else
                                {
                                    System.Design.NativeMethods.SendMessage(zero, key.KeyChar.Msg, key.KeyChar.WParam, key.KeyChar.LParam);
                                }
                            }
                        }
                        this.bufferedChars.Clear();
                    }
                    return;
            }
        Label_024C:
            this.lastKeyDown.Msg = 0;
            goto Label_0339;
        Label_025D:
            if ((Control.ModifierKeys & (Keys.Alt | Keys.Control)) == Keys.None)
            {
                if (this.bufferedChars == null)
                {
                    this.bufferedChars = new ArrayList();
                }
                this.bufferedChars.Add(new BufferedKey(this.lastKeyDown, m, this.lastKeyDown));
                if (!this.ignoreMessages && (this.MenuCommandService != null))
                {
                    this.postCharMessage = true;
                    this.MenuCommandService.GlobalInvoke(StandardCommands.PropertiesWindow);
                }
                else if ((this.ignoreMessages && (m.Msg != 0x10f)) && (this.InSituSupportService != null))
                {
                    this.postCharMessage = true;
                    this.InSituSupportService.HandleKeyChar();
                }
                if (this.postCharMessage)
                {
                    return;
                }
            }
        Label_0339:
            if (this.oldTarget != null)
            {
                this.oldTarget.OnMessage(ref m);
            }
        }

        private ISupportInSituService InSituSupportService
        {
            get
            {
                return (ISupportInSituService) this.sp.GetService(typeof(ISupportInSituService));
            }
        }

        private IMenuCommandService MenuCommandService
        {
            get
            {
                if ((this.menuCommandSvc == null) && (this.sp != null))
                {
                    this.menuCommandSvc = (IMenuCommandService) this.sp.GetService(typeof(IMenuCommandService));
                }
                return this.menuCommandSvc;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct BufferedKey
        {
            public readonly Message KeyDown;
            public readonly Message KeyUp;
            public readonly Message KeyChar;
            public BufferedKey(Message keyDown, Message keyChar, Message keyUp)
            {
                this.KeyChar = keyChar;
                this.KeyDown = keyDown;
                this.KeyUp = keyUp;
            }
        }
    }
}

