namespace System.Windows.Forms
{
    using System;
    using System.Windows.Forms.Layout;

    public abstract class LayoutSettings
    {
        private IArrangedElement _owner;

        protected LayoutSettings()
        {
        }

        internal LayoutSettings(IArrangedElement owner)
        {
            this._owner = owner;
        }

        public virtual System.Windows.Forms.Layout.LayoutEngine LayoutEngine
        {
            get
            {
                return null;
            }
        }

        internal IArrangedElement Owner
        {
            get
            {
                return this._owner;
            }
        }
    }
}

