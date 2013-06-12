namespace System.Windows.Forms
{
    using System;
    using System.Runtime.InteropServices;

    [ComVisible(true)]
    public class DragEventArgs : EventArgs
    {
        private readonly DragDropEffects allowedEffect;
        private readonly IDataObject data;
        private DragDropEffects effect;
        private readonly int keyState;
        private readonly int x;
        private readonly int y;

        public DragEventArgs(IDataObject data, int keyState, int x, int y, DragDropEffects allowedEffect, DragDropEffects effect)
        {
            this.data = data;
            this.keyState = keyState;
            this.x = x;
            this.y = y;
            this.allowedEffect = allowedEffect;
            this.effect = effect;
        }

        public DragDropEffects AllowedEffect
        {
            get
            {
                return this.allowedEffect;
            }
        }

        public IDataObject Data
        {
            get
            {
                return this.data;
            }
        }

        public DragDropEffects Effect
        {
            get
            {
                return this.effect;
            }
            set
            {
                this.effect = value;
            }
        }

        public int KeyState
        {
            get
            {
                return this.keyState;
            }
        }

        public int X
        {
            get
            {
                return this.x;
            }
        }

        public int Y
        {
            get
            {
                return this.y;
            }
        }
    }
}

