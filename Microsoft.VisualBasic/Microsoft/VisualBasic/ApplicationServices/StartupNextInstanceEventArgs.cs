namespace Microsoft.VisualBasic.ApplicationServices
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class StartupNextInstanceEventArgs : EventArgs
    {
        private bool m_BringToForeground;
        private ReadOnlyCollection<string> m_CommandLine;

        public StartupNextInstanceEventArgs(ReadOnlyCollection<string> args, bool bringToForegroundFlag)
        {
            if (args == null)
            {
                args = new ReadOnlyCollection<string>(null);
            }
            this.m_CommandLine = args;
            this.m_BringToForeground = bringToForegroundFlag;
        }

        public bool BringToForeground
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_BringToForeground;
            }
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            set
            {
                this.m_BringToForeground = value;
            }
        }

        public ReadOnlyCollection<string> CommandLine
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.m_CommandLine;
            }
        }
    }
}

