namespace System.Configuration.Install
{
    using System;
    using System.Collections;
    using System.Runtime;

    public class InstallEventArgs : EventArgs
    {
        private IDictionary savedState;

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstallEventArgs()
        {
        }

        [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
        public InstallEventArgs(IDictionary savedState)
        {
            this.savedState = savedState;
        }

        public IDictionary SavedState
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.savedState;
            }
        }
    }
}

