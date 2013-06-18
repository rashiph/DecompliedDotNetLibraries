namespace System.Windows.Forms.Design
{
    using System;
    using System.ComponentModel.Design;

    internal interface IMenuStatusHandler
    {
        bool OverrideInvoke(MenuCommand cmd);
        bool OverrideStatus(MenuCommand cmd);
    }
}

