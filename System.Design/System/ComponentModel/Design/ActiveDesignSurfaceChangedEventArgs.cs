namespace System.ComponentModel.Design
{
    using System;

    public class ActiveDesignSurfaceChangedEventArgs : EventArgs
    {
        private DesignSurface _newSurface;
        private DesignSurface _oldSurface;

        public ActiveDesignSurfaceChangedEventArgs(DesignSurface oldSurface, DesignSurface newSurface)
        {
            this._oldSurface = oldSurface;
            this._newSurface = newSurface;
        }

        public DesignSurface NewSurface
        {
            get
            {
                return this._newSurface;
            }
        }

        public DesignSurface OldSurface
        {
            get
            {
                return this._oldSurface;
            }
        }
    }
}

