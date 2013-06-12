namespace System.Web.UI
{
    using System;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public class ControlSkin
    {
        private ControlSkinDelegate _controlSkinDelegate;
        private Type _controlType;

        public ControlSkin(Type controlType, ControlSkinDelegate themeDelegate)
        {
            this._controlType = controlType;
            this._controlSkinDelegate = themeDelegate;
        }

        public void ApplySkin(Control control)
        {
            this._controlSkinDelegate(control);
        }

        public Type ControlType
        {
            get
            {
                return this._controlType;
            }
        }
    }
}

