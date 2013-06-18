namespace Microsoft.VisualBasic.ApplicationServices
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Threading;

    [ComVisible(false), EditorBrowsable(EditorBrowsableState.Advanced)]
    public class UnhandledExceptionEventArgs : ThreadExceptionEventArgs
    {
        private bool m_ExitApplication;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public UnhandledExceptionEventArgs(bool exitApplication, Exception exception) : base(exception)
        {
            this.m_ExitApplication = exitApplication;
        }

        public bool ExitApplication
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_ExitApplication;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_ExitApplication = value;
            }
        }
    }
}

