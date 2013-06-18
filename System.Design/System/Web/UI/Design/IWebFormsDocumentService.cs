namespace System.Web.UI.Design
{
    using System;

    [Obsolete("The recommended alternative is System.Web.UI.Design.WebFormsRootDesigner. The WebFormsRootDesigner contains additional functionality and allows for more extensibility. To get the WebFormsRootDesigner use the RootDesigner property from your ControlDesigner. http://go.microsoft.com/fwlink/?linkid=14202")]
    public interface IWebFormsDocumentService
    {
        event EventHandler LoadComplete;

        object CreateDiscardableUndoUnit();
        void DiscardUndoUnit(object discardableUndoUnit);
        void EnableUndo(bool enable);
        void UpdateSelection();

        string DocumentUrl { get; }

        bool IsLoading { get; }
    }
}

