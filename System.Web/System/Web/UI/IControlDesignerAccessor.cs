namespace System.Web.UI
{
    using System;
    using System.Collections;

    public interface IControlDesignerAccessor
    {
        IDictionary GetDesignModeState();
        void SetDesignModeState(IDictionary data);
        void SetOwnerControl(Control owner);

        IDictionary UserData { get; }
    }
}

