namespace System.Web
{
    using System;
    using System.Collections.Generic;

    internal sealed class PipelineModuleStepContainer
    {
        private List<HttpApplication.IExecutionStep>[] _modulePostSteps;
        private List<HttpApplication.IExecutionStep>[] _moduleSteps;

        internal PipelineModuleStepContainer()
        {
        }

        internal void AddEvent(RequestNotification notification, bool isPostEvent, HttpApplication.IExecutionStep step)
        {
            int index = EventToIndex(notification);
            List<HttpApplication.IExecutionStep>[] listArray = null;
            if (isPostEvent)
            {
                if (this._modulePostSteps == null)
                {
                    this._modulePostSteps = new List<HttpApplication.IExecutionStep>[0x20];
                }
                listArray = this._modulePostSteps;
            }
            else
            {
                if (this._moduleSteps == null)
                {
                    this._moduleSteps = new List<HttpApplication.IExecutionStep>[0x20];
                }
                listArray = this._moduleSteps;
            }
            List<HttpApplication.IExecutionStep> list = listArray[index];
            if (list == null)
            {
                list = new List<HttpApplication.IExecutionStep>();
                listArray[index] = list;
            }
            list.Add(step);
        }

        private static int EventToIndex(RequestNotification notification)
        {
            int num = -1;
            RequestNotification notification2 = notification;
            if (notification2 <= RequestNotification.PreExecuteRequestHandler)
            {
                if (notification2 <= RequestNotification.ResolveRequestCache)
                {
                    switch (notification2)
                    {
                        case RequestNotification.BeginRequest:
                            return 0;

                        case RequestNotification.AuthenticateRequest:
                            return 1;

                        case (RequestNotification.AuthenticateRequest | RequestNotification.BeginRequest):
                            return num;

                        case RequestNotification.AuthorizeRequest:
                            return 2;

                        case RequestNotification.ResolveRequestCache:
                            return 3;
                    }
                    return num;
                }
                switch (notification2)
                {
                    case RequestNotification.MapRequestHandler:
                        return 4;

                    case RequestNotification.AcquireRequestState:
                        return 5;

                    case RequestNotification.PreExecuteRequestHandler:
                        return 6;
                }
                return num;
            }
            if (notification2 <= RequestNotification.UpdateRequestCache)
            {
                switch (notification2)
                {
                    case RequestNotification.ExecuteRequestHandler:
                        return 7;

                    case RequestNotification.ReleaseRequestState:
                        return 8;

                    case RequestNotification.UpdateRequestCache:
                        return 9;
                }
                return num;
            }
            switch (notification2)
            {
                case RequestNotification.LogRequest:
                    return 10;

                case RequestNotification.EndRequest:
                    return 11;

                case RequestNotification.SendResponse:
                    return 12;
            }
            return num;
        }

        internal int GetEventCount(RequestNotification notification, bool isPostEvent)
        {
            List<HttpApplication.IExecutionStep> stepArray = this.GetStepArray(notification, isPostEvent);
            if (stepArray == null)
            {
                return 0;
            }
            return stepArray.Count;
        }

        internal HttpApplication.IExecutionStep GetNextEvent(RequestNotification notification, bool isPostEvent, int eventIndex)
        {
            return this.GetStepArray(notification, isPostEvent)[eventIndex];
        }

        private List<HttpApplication.IExecutionStep> GetStepArray(RequestNotification notification, bool isPostEvent)
        {
            List<HttpApplication.IExecutionStep>[] listArray = this._moduleSteps;
            if (isPostEvent)
            {
                listArray = this._modulePostSteps;
            }
            int index = EventToIndex(notification);
            return listArray[index];
        }

        internal void RemoveEvent(RequestNotification notification, bool isPostEvent, Delegate handler)
        {
            List<HttpApplication.IExecutionStep>[] listArray = this._moduleSteps;
            if (isPostEvent)
            {
                listArray = this._modulePostSteps;
            }
            if (listArray != null)
            {
                int index = EventToIndex(notification);
                List<HttpApplication.IExecutionStep> list = listArray[index];
                if (list != null)
                {
                    int num2 = -1;
                    for (int i = 0; i < list.Count; i++)
                    {
                        HttpApplication.SyncEventExecutionStep step = list[i] as HttpApplication.SyncEventExecutionStep;
                        if ((step != null) && (step.Handler == ((EventHandler) handler)))
                        {
                            num2 = i;
                            break;
                        }
                    }
                    if (num2 != -1)
                    {
                        list.RemoveAt(num2);
                    }
                }
            }
        }
    }
}

