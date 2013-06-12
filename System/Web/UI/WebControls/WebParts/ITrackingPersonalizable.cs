namespace System.Web.UI.WebControls.WebParts
{
    using System;

    public interface ITrackingPersonalizable
    {
        void BeginLoad();
        void BeginSave();
        void EndLoad();
        void EndSave();

        bool TracksChanges { get; }
    }
}

