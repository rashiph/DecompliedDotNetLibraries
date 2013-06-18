namespace System.Windows.Forms
{
    using System;

    internal interface ISupportOleDropSource
    {
        void OnGiveFeedback(GiveFeedbackEventArgs gfbevent);
        void OnQueryContinueDrag(QueryContinueDragEventArgs qcdevent);
    }
}

