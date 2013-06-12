namespace System.Web.UI
{
    using System;

    public interface IStateManager
    {
        void LoadViewState(object state);
        object SaveViewState();
        void TrackViewState();

        bool IsTrackingViewState { get; }
    }
}

