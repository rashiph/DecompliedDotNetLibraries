namespace System.Web.SessionState
{
    using System;
    using System.Web;
    using System.Web.Util;

    internal class SessionOnEndTarget
    {
        internal int _sessionEndEventHandlerCount;

        internal SessionOnEndTarget()
        {
        }

        internal void RaiseOnEnd(HttpSessionState sessionState)
        {
            if (this._sessionEndEventHandlerCount > 0)
            {
                HttpApplicationFactory.EndSession(sessionState, this, EventArgs.Empty);
            }
        }

        internal void RaiseSessionOnEnd(string id, SessionStateStoreData item)
        {
            HttpSessionStateContainer container = new HttpSessionStateContainer(id, item.Items, item.StaticObjects, item.Timeout, false, SessionStateModule.s_configCookieless, SessionStateModule.s_configMode, true);
            HttpSessionState sessionState = new HttpSessionState(container);
            if (HttpRuntime.ShutdownInProgress)
            {
                this.RaiseOnEnd(sessionState);
            }
            else
            {
                SessionOnEndTargetWorkItem item2 = new SessionOnEndTargetWorkItem(this, sessionState);
                WorkItem.PostInternal(new WorkItemCallback(item2.RaiseOnEndCallback));
            }
        }

        internal int SessionEndEventHandlerCount
        {
            get
            {
                return this._sessionEndEventHandlerCount;
            }
            set
            {
                this._sessionEndEventHandlerCount = value;
            }
        }
    }
}

