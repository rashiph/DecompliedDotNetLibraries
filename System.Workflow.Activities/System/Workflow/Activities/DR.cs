namespace System.Workflow.Activities
{
    using System;
    using System.Drawing;
    using System.Resources;

    internal static class DR
    {
        internal const string AddEventDriven = "AddEventDriven";
        internal const string AddingChild = "AddingChild";
        internal const string AddNewEvent = "AddNewEvent";
        internal const string AddNewEventDesc = "AddNewEventDesc";
        internal const string AddState = "AddState";
        internal const string AddStateFinalization = "AddStateFinalization";
        internal const string AddStateInitialization = "AddStateInitialization";
        internal const string BringToFront = "BringToFront";
        internal const string Compensation = "Compensation";
        internal const string CompensationView = "CompensationView";
        internal const string CompletedState = "CompletedState";
        internal const string Delete = "Delete";
        internal const string DeleteEvent = "DeleteEvent";
        internal const string DeleteEventDesc = "DeleteEventDesc";
        internal const string DropActivityHere = "DropActivityHere";
        internal const string DropEventsHere = "DropEventsHere";
        internal const string Error_InvalidImageResource = "Error_InvalidImageResource";
        internal const string Event = "Event";
        internal const string EventBasedWorkFlow = "EventBasedWorkFlow";
        internal const string EventsDesc = "EventsDesc";
        internal const string EventsView = "EventsView";
        internal const string Exception = "Exception";
        internal const string ExceptionsView = "ExceptionsView";
        internal const string ImageFileFilter = "ImageFileFilter";
        internal const string InitialState = "InitialState";
        internal const string InvokeWebServiceDisplayName = "InvokeWebServiceDisplayName";
        internal const string MoveLeftDesc = "MoveLeftDesc";
        internal const string MoveRightDesc = "MoveRightDesc";
        internal const string NavigateEvent = "NavigateEvent";
        internal const string NavigateToEvent = "NavigateToEvent";
        internal const string NavigateToEventDesc = "NavigateToEventDesc";
        internal const string NewEvent = "NewEvent";
        internal const string NextEvent = "NextEvent";
        internal const string PreviousEvent = "PreviousEvent";
        private static ResourceManager resourceManager = new ResourceManager("System.Workflow.Activities.ActivityDesignerResources", Assembly.GetExecutingAssembly());
        internal const string ResourceSet = "System.Workflow.Activities.ActivityDesignerResources";
        internal const string ScopeDesc = "ScopeDesc";
        internal const string SendToBack = "SendToBack";
        internal const string SequenceArrow = "SequenceArrow";
        internal const string SequentialWorkflowHelpText = "SequentialWorkflowHelpText";
        internal const string SetAsCompletedState = "SetAsCompletedState";
        internal const string SetAsInitialState = "SetAsInitialState";
        internal const string StartSequentialWorkflow = "StartSequentialWorkflow";
        internal const string StateHelpText = "StateHelpText";
        internal const string StateMachineView = "StateMachineView";
        internal const string StateMachineWorkflowHelpText = "StateMachineWorkflowHelpText";
        internal const string ThemePropertyReadOnly = "ThemePropertyReadOnly";
        internal static Color TransparentColor = Color.FromArgb(0xff, 0, 0xff);
        internal const string View = "View";
        internal const string ViewNextEvent = "ViewNextEvent";
        internal const string ViewNextEventDesc = "ViewNextEventDesc";
        internal const string ViewPreviousEvent = "ViewPreviousEvent";
        internal const string ViewPreviousEventDesc = "ViewPreviousEventDesc";
        internal const string WebServiceFaultDisplayName = "WebServiceFaultDisplayName";
        internal const string WebServiceReceiveDisplayName = "WebServiceReceiveDisplayName";
        internal const string WebServiceResponseDisplayName = "WebServiceResponseDisplayName";
        internal const string WorkflowCancellation = "WorkflowCancellation";
        internal const string WorkflowCompensation = "WorkflowCompensation";
        internal const string WorkflowEvents = "WorkflowEvents";
        internal const string WorkflowExceptions = "WorkflowExceptions";
        internal const string WorkflowView = "WorkflowView";

        internal static Image GetImage(string resID)
        {
            Image image = resourceManager.GetObject(resID) as Image;
            Bitmap bitmap = image as Bitmap;
            if (bitmap != null)
            {
                bitmap.MakeTransparent(TransparentColor);
            }
            return image;
        }

        internal static string GetString(string resID)
        {
            return resourceManager.GetString(resID);
        }
    }
}

