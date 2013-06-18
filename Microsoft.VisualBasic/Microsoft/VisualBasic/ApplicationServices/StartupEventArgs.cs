namespace Microsoft.VisualBasic.ApplicationServices
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Runtime;
    using System.Runtime.InteropServices;

    [EditorBrowsable(EditorBrowsableState.Advanced), ComVisible(false)]
    public class StartupEventArgs : CancelEventArgs
    {
        private ReadOnlyCollection<string> m_CommandLine;

        public StartupEventArgs(ReadOnlyCollection<string> args)
        {
            if (args == null)
            {
                args = new ReadOnlyCollection<string>(null);
            }
            this.m_CommandLine = args;
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

