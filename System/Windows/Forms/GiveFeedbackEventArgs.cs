namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class GiveFeedbackEventArgs : EventArgs
    {
        private readonly DragDropEffects effect;
        private bool useDefaultCursors;

        public GiveFeedbackEventArgs(DragDropEffects effect, bool useDefaultCursors)
        {
            this.effect = effect;
            this.useDefaultCursors = useDefaultCursors;
        }

        public DragDropEffects Effect
        {
            get
            {
                return this.effect;
            }
        }

        public bool UseDefaultCursors
        {
            get
            {
                return this.useDefaultCursors;
            }
            set
            {
                this.useDefaultCursors = value;
            }
        }
    }
}

