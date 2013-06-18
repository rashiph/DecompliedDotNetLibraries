namespace System.Web.UI.WebControls
{
    using System;

    [Flags]
    public enum DataControlRowState
    {
        Alternate = 1,
        Edit = 4,
        Insert = 8,
        Normal = 0,
        Selected = 2
    }
}

