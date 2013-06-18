namespace Microsoft.VisualBasic.Devices
{
    using Microsoft.VisualBasic.CompilerServices;
    using System;
    using System.Runtime;
    using System.Security;
    using System.Security.Permissions;
    using System.Windows.Forms;

    [HostProtection(SecurityAction.LinkDemand, Resources=HostProtectionResource.ExternalProcessMgmt)]
    public class Keyboard
    {
        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public void SendKeys(string keys)
        {
            this.SendKeys(keys, false);
        }

        public void SendKeys(string keys, bool wait)
        {
            if (wait)
            {
                System.Windows.Forms.SendKeys.SendWait(keys);
            }
            else
            {
                System.Windows.Forms.SendKeys.Send(keys);
            }
        }

        public bool AltKeyDown
        {
            get
            {
                return ((Control.ModifierKeys & Keys.Alt) > Keys.None);
            }
        }

        public bool CapsLock
        {
            [SecuritySafeCritical]
            get
            {
                return ((Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.GetKeyState(20) & 1) > 0);
            }
        }

        public bool CtrlKeyDown
        {
            get
            {
                return ((Control.ModifierKeys & Keys.Control) > Keys.None);
            }
        }

        public bool NumLock
        {
            [SecuritySafeCritical]
            get
            {
                return ((Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.GetKeyState(0x90) & 1) > 0);
            }
        }

        public bool ScrollLock
        {
            [SecuritySafeCritical]
            get
            {
                return ((Microsoft.VisualBasic.CompilerServices.UnsafeNativeMethods.GetKeyState(0x91) & 1) > 0);
            }
        }

        public bool ShiftKeyDown
        {
            get
            {
                return ((Control.ModifierKeys & Keys.Shift) > Keys.None);
            }
        }
    }
}

