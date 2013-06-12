namespace System.Media
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;

    [HostProtection(SecurityAction.LinkDemand, UI=true)]
    public class SystemSound
    {
        private int soundType;

        internal SystemSound(int soundType)
        {
            this.soundType = soundType;
        }

        public void Play()
        {
            System.ComponentModel.IntSecurity.UnmanagedCode.Assert();
            try
            {
                SafeNativeMethods.MessageBeep(this.soundType);
            }
            finally
            {
                CodeAccessPermission.RevertAssert();
            }
        }

        private class SafeNativeMethods
        {
            private SafeNativeMethods()
            {
            }

            [DllImport("user32.dll", CharSet=CharSet.Auto, ExactSpelling=true)]
            internal static extern bool MessageBeep(int type);
        }
    }
}

