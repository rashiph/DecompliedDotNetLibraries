namespace System.Web.UI
{
    using System;

    internal enum ControlState
    {
        Constructed,
        FrameworkInitialized,
        ChildrenInitialized,
        Initialized,
        ViewStateLoaded,
        Loaded,
        PreRendered
    }
}

