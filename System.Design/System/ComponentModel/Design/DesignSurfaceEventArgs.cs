namespace System.ComponentModel.Design
{
    using System;

    public class DesignSurfaceEventArgs : EventArgs
    {
        private DesignSurface _surface;

        public DesignSurfaceEventArgs(DesignSurface surface)
        {
            if (surface == null)
            {
                throw new ArgumentNullException("surface");
            }
            this._surface = surface;
        }

        public DesignSurface Surface
        {
            get
            {
                return this._surface;
            }
        }
    }
}

